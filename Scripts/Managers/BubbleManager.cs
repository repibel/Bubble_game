using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Gameplay;

namespace Managers
{
    public class BubbleManager : MonoBehaviour
    {
        public static BubbleManager Instance { get; private set; }

        [Header("Prefab & Container")]
        [SerializeField] private Bubble bubblePrefab;
        [SerializeField] private Transform bubbleContainer;

        [Header("Grid System (SafeArea)")]
        [SerializeField] private Vector2Int gridSize = new Vector2Int(6, 7); // 가로 5 -> 6 확장
        [SerializeField] private Vector2 cellSize = new Vector2(1.5f, 1.5f);
        [SerializeField] private Vector2 gridOrigin = new Vector2(-4.5f, -3.5f); // 중심을 맞추기 위해 x를 -4.5로 조정
        [SerializeField] private float minDistanceBetweenSequential = 2.5f;
        
        [Header("Gameplay Settings")]
        [SerializeField] private int startBubbleCount = 5; // 처음에 5개로 시작
        [SerializeField] private int decreaseThreshold = 15; // 15콤보마다 1개씩 줄임
        
        // 상태 관리
        private List<Bubble> activeBubbles = new List<Bubble>();
        private Queue<Bubble> bubblePool = new Queue<Bubble>();
        private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>(); // 겹침 방지용

        private string logFilePath;

        private void Awake() 
        {
            Instance = this;
            InitializeLogger();
        }

        private void InitializeLogger()
        {
            // 에디터/빌드 환경에 따라 경로 조정. 여기서는 요청대로 Scripts 폴더 내 저장.
            // 주의: 빌드 시 Scripts 폴더가 없을 수 있으므로 Application.dataPath 사용.
            logFilePath = Path.Combine(Application.dataPath, "Scripts", "log.txt");
            try {
                // 파일 초기화 (덮어쓰기)
                File.WriteAllText(logFilePath, $"--- Game Start: {System.DateTime.Now} ---\n");
            } catch (System.Exception e) {
                Debug.LogError($"Failed to init log file: {e.Message}");
            }
        }

        private void LogToFile(string message, bool isWarning = false)
        {
            if (isWarning) Debug.LogWarning(message);
            else Debug.Log(message);
            
            if (string.IsNullOrEmpty(logFilePath)) return;

            try {
                string prefix = isWarning ? "[WARNING] " : "";
                File.AppendAllText(logFilePath, $"[{System.DateTime.Now:HH:mm:ss}] {prefix}{message}\n");
            } catch { }
        }

        private void Start()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnGameStart += OnGameStart;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnGameStart -= OnGameStart;
        }

        private void OnGameStart()
        {
            ClearAllBubbles();
            SpawnInitialBubbles();
        }

        public void OnBubbleCorrect(Bubble bubble)
        {
            // 1. 맞춘 버블 제거
            activeBubbles.Remove(bubble);
            occupiedCells.Remove(bubble.GridPosition);
            ReturnToPool(bubble);

            // 2. 상황에 맞춰 버블 보충
            UpdateBubbleCount();
        }

        private void SpawnInitialBubbles()
        {
            for (int i = 0; i < startBubbleCount; i++)
            {
                SpawnBubble(i + 1);
            }
        }

        private void UpdateBubbleCount()
        {
            StartCoroutine(UpdateBubbleCountRoutine());
        }

        // [오류 해결] 누락되었던 코루틴 로직
        private System.Collections.IEnumerator UpdateBubbleCountRoutine()
        {
            if (GameManager.Instance == null) yield break;
            
            int maxCapacity = gridSize.x * gridSize.y;
            int safetyLoop = 0;

            while (true)
            {
                if (GameManager.Instance == null) yield break;

                int currentTarget = GameManager.Instance.CurrentTargetNumber;
                
                // [구현 완료] 함수 호출 연결
                int desiredCount = CalculateDesiredBubbleCount(GameManager.Instance.ComboCount);
                
                // [구현 완료] 함수 호출 연결
                bool needMore = activeBubbles.Count < desiredCount || !IsNumberActive(currentTarget);
                bool canSpawn = activeBubbles.Count < maxCapacity;

                LogToFile($"[UpdateBubbleCount] Active: {activeBubbles.Count}, Desired: {desiredCount}, Target: {currentTarget}, NeedMore: {needMore}, CanSpawn: {canSpawn}");

                if (!needMore || !canSpawn) break;

                if (safetyLoop > 50) 
                {
                    LogToFile("BubbleManager: Safety loop hit", true);
                    break;
                }

                // [구현 완료] 함수 호출 연결
                if (SpawnNextBubble()) 
                {
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    LogToFile("[UpdateBubbleCount] Failed to spawn bubble (Grid full?)", true);
                    break;
                }
                
                safetyLoop++;
            }
        }

        // ---------------------------------------------------------
        // 👇 [누락되었던 핵심 함수 3개 구현 추가] 👇
        // ---------------------------------------------------------

        // 1. 콤보에 따른 목표 버블 개수 계산 (서바이벌 로직)
        private int CalculateDesiredBubbleCount(int combo)
        {
            int reduction = combo / decreaseThreshold; // 15콤보마다 1개 감소
            return Mathf.Max(1, startBubbleCount - reduction); // 최소 1개는 유지
        }

        // 2. 특정 숫자가 현재 화면에 있는지 확인
        private bool IsNumberActive(int number)
        {
            foreach (var b in activeBubbles)
            {
                if (b.Number == number) return true;
            }
            return false;
        }

        // 3. 다음 버블을 결정하고 생성 (타겟 우선, 없으면 Max+1)
        private bool SpawnNextBubble()
        {
            if (GameManager.Instance == null) return false;

            int target = GameManager.Instance.CurrentTargetNumber;
            int numberToSpawn;

            // 만약 타겟 숫자(예: 1)가 화면에 없으면 1순위로 생성 (안전장치)
            if (!IsNumberActive(target))
            {
                LogToFile($"[SpawnNextBubble] Target {target} missing. Spawning it.");
                numberToSpawn = target;
            }
            else
            {
                // 타겟이 있으면 순서대로 (가장 큰 수 + 1) 생성
                numberToSpawn = GetNextSpawnNumber();
                LogToFile($"[SpawnNextBubble] Target present. Spawning next sequence: {numberToSpawn}");
            }

            return SpawnBubble(numberToSpawn);
        }
        // ---------------------------------------------------------

        private int GetNextSpawnNumber()
        {
            if (activeBubbles.Count == 0)
            {
                // 버블이 하나도 없으면 현재 타겟 생성
                return (GameManager.Instance != null) ? GameManager.Instance.CurrentTargetNumber : 1;
            }

            int maxNum = 0;
            foreach (var b in activeBubbles)
            {
                if (b.Number > maxNum) maxNum = b.Number;
            }
            return maxNum + 1;
        }

        private bool SpawnBubble(int number)
        {
            Vector2Int spawnPos = GetRandomEmptyPosition();
            if (spawnPos.x == -1) 
            {
                LogToFile($"[SpawnBubble] No empty position found for Number: {number}", true);
                return false; 
            }

            Vector3 worldPos = GridToWorld(spawnPos);
            
            LogToFile($"[SpawnBubble] Spawning Number {number} at Grid {spawnPos}");

            Bubble bubble = GetFromPool();
            bubble.transform.position = worldPos;
            bubble.Initialize(number, spawnPos);
            
            activeBubbles.Add(bubble);
            occupiedCells.Add(spawnPos);

            return true;
        }

        private Vector2Int GetRandomEmptyPosition()
        {
            List<Vector2Int> emptyCells = new List<Vector2Int>();

            for (int x = 0; x < gridSize.x; x++)
            {
                // [수정] y=0 행은 보이지 않으므로 y=1부터 탐색
                for (int y = 1; y < gridSize.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (!occupiedCells.Contains(pos))
                    {
                        emptyCells.Add(pos);
                    }
                }
            }

            if (emptyCells.Count > 0)
            {
                return emptyCells[Random.Range(0, emptyCells.Count)];
            }

            return new Vector2Int(-1, -1);
        }

        private Vector3 GridToWorld(Vector2Int gridPos)
        {
            return new Vector3(
                gridOrigin.x + (gridPos.x * cellSize.x) + (cellSize.x * 0.5f),
                gridOrigin.y + (gridPos.y * cellSize.y) + (cellSize.y * 0.5f),
                0
            );
        }

        private Bubble GetFromPool()
        {
            Bubble b = (bubblePool.Count > 0) ? bubblePool.Dequeue() : Instantiate(bubblePrefab, bubbleContainer);
            b.gameObject.SetActive(true);
            return b;
        }

        private void ReturnToPool(Bubble bubble)
        {
            bubble.gameObject.SetActive(false);
            bubblePool.Enqueue(bubble);
        }

        private void ClearAllBubbles()
        {
            foreach (var b in activeBubbles) ReturnToPool(b);
            activeBubbles.Clear();
            occupiedCells.Clear();
        }
    }
}