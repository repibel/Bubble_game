# Markdown 내용 작성
md_content = """# Project Status: Bubble Touch 60

## 1. Project Overview
- **Game Title:** Bubble Touch 60 (Temporary)
- **Genre:** Hyper Casual / Speed Touch Action
- **Engine:** Unity 6 (6000.0.3f1)
- **Resolution:** Portrait (9:16 / 1080x1920)
- **Core Loop:** Touch bubbles in ascending order (1 -> 2 -> 3...) within 60 seconds.
- **Key Features:**
  - Infinite spawning (Max number + 1 logic).
  - Dynamic difficulty (Grid-based spawning, Distance constraints).
  - Combo system with rising pitch audio.
  - Start Screen & Volume Control.

## 2. Current Architecture & Scripts

### A. Managers

#### `GameManager.cs` (Singleton)
- Manages Game State (`Menu`, `Playing`, `GameOver`).
- Handles Global Timer (60s) and Combo Timer.
- Events: `OnGameStart`, `OnGameEnd`, `OnTargetNumberChanged`, `OnComboUpdated`, `OnComboTimeUpdated`.

```csharp
using System;
using UnityEngine;

namespace Managers
{
    public enum GameState { Menu, Playing, GameOver }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private float gameDuration = 60f;
        [SerializeField] private float initialComboTime = 2.0f;
        [SerializeField] private float minComboTime = 0.5f;
        [SerializeField] private int comboForMinTime = 40;

        public float TotalGameDuration => gameDuration;

        [Header("Game State")]
        public GameState CurrentState { get; private set; } = GameState.Menu;
        public float CurrentTime { get; private set; }
        public float CurrentComboTime { get; private set; }
        public float MaxCurrentComboTime { get; private set; }
        public int CurrentTargetNumber { get; private set; }
        public int ComboCount { get; private set; }
        public float MaxCombo { get; private set; }
        
        public event Action OnGameStart;
        public event Action OnGameEnd;
        public event Action<int> OnComboUpdated;
        public event Action<float> OnComboTimeUpdated;
        public event Action<int> OnTargetNumberChanged;
        public event Action OnWrongTouch;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start() => CurrentState = GameState.Menu;

        private void Update()
        {
            if (CurrentState != GameState.Playing) return;

            CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 0)
            {
                EndGame();
                return;
            }

            if (ComboCount > 0)
            {
                CurrentComboTime -= Time.deltaTime;
                float ratio = CurrentComboTime / MaxCurrentComboTime;
                OnComboTimeUpdated?.Invoke(ratio);
                
                if (CurrentComboTime <= 0) ResetCombo();
            }
            else
            {
                OnComboTimeUpdated?.Invoke(0f);
            }
        }

        public void StartGame()
        {
            CurrentTime = gameDuration;
            CurrentTargetNumber = 1;
            ComboCount = 0;
            MaxCombo = 0;
            MaxCurrentComboTime = initialComboTime;
            CurrentState = GameState.Playing;
            
            OnGameStart?.Invoke();
            OnTargetNumberChanged?.Invoke(CurrentTargetNumber);
            OnComboUpdated?.Invoke(0);
        }

        public void EndGame()
        {
            CurrentState = GameState.GameOver;
            CurrentTime = 0;
            OnGameEnd?.Invoke();
        }

        public void ValidateInput(int number)
        {
            if (CurrentState != GameState.Playing) return;

            if (number == CurrentTargetNumber)
            {
                CurrentTargetNumber++;
                IncreaseCombo();
                OnTargetNumberChanged?.Invoke(CurrentTargetNumber);
            }
            else
            {
                ResetCombo();
                OnWrongTouch?.Invoke();
            }
        }

        private void IncreaseCombo()
        {
            ComboCount++;
            if (ComboCount > MaxCombo) MaxCombo = ComboCount;
            
            float t = Mathf.Clamp01((float)ComboCount / comboForMinTime);
            MaxCurrentComboTime = Mathf.Lerp(initialComboTime, minComboTime, t);
            CurrentComboTime = MaxCurrentComboTime;

            OnComboUpdated?.Invoke(ComboCount);
        }

        public void ResetCombo()
        {
            if (ComboCount > 0)
            {
                ComboCount = 0;
                OnComboUpdated?.Invoke(ComboCount);
                CurrentComboTime = 0;
                OnComboTimeUpdated?.Invoke(0f);
            }
        }
    }
}

BubbleManager.cs (Singleton)
Handles Object Pooling for Bubbles.

Logic: Grid-based Spawning (prevents overlap), Distance Check (spreads bubbles).

Critical Logic: SpawnNextBubble finds the max number on screen + 1 to prevent number skipping.

using System.Collections.Generic;
using UnityEngine;
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
        [SerializeField] private Vector2Int gridSize = new Vector2Int(5, 7);
        [SerializeField] private Vector2 cellSize = new Vector2(1.5f, 1.5f);
        [SerializeField] private Vector2 gridOrigin = new Vector2(-3.75f, -6.5f);
        [SerializeField] private float minDistanceBetweenSequential = 2.5f;

        [Header("Difficulty")]
        [SerializeField] private int initialBubbleCount = 5;
        [SerializeField] private int minBubbleCount = 2;
        [SerializeField] private int comboForMaxDifficulty = 50;

        private List<Bubble> activeBubbles = new List<Bubble>();
        private Queue<Bubble> bubblePool = new Queue<Bubble>();
        private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();
        
        private Vector2Int lastSpawnedGridPos = new Vector2Int(-1, -1);

        private void Awake() => Instance = this;

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
            lastSpawnedGridPos = new Vector2Int(-1, -1);
            UpdateBubbleCount();
        }

        public void OnBubbleCorrect(Bubble bubble)
        {
            activeBubbles.Remove(bubble);
            Vector2Int gridPos = WorldToGrid(bubble.transform.position);
            occupiedCells.Remove(gridPos);
            ReturnToPool(bubble);
            UpdateBubbleCount();
        }

        private void UpdateBubbleCount()
        {
            if (GameManager.Instance == null) return;
            int currentCombo = GameManager.Instance.ComboCount;
            int desiredCount = CalculateDesiredBubbleCount(currentCombo);

            while (activeBubbles.Count < desiredCount)
            {
                SpawnNextBubble();
            }
        }

        private int CalculateDesiredBubbleCount(int combo)
        {
            float t = Mathf.Clamp01((float)combo / comboForMaxDifficulty);
            int count = Mathf.RoundToInt(Mathf.Lerp(initialBubbleCount, minBubbleCount, t));
            return Mathf.Max(minBubbleCount, count);
        }

        private void SpawnNextBubble()
        {
            int numberToSpawn;

            if (activeBubbles.Count > 0)
            {
                int maxNumberOnScreen = 0;
                foreach (var b in activeBubbles)
                {
                    if (b.Number > maxNumberOnScreen) maxNumberOnScreen = b.Number;
                }
                numberToSpawn = maxNumberOnScreen + 1;
            }
            else
            {
                numberToSpawn = (GameManager.Instance != null) ? GameManager.Instance.CurrentTargetNumber : 1;
            }

            Vector2Int spawnPos = GetRandomGridPosition();
            if (spawnPos.x == -1) return;

            Vector3 worldPos = GridToWorld(spawnPos);
            
            Bubble bubble = GetFromPool();
            bubble.transform.position = worldPos;
            bubble.Initialize(numberToSpawn);
            
            activeBubbles.Add(bubble);
            occupiedCells.Add(spawnPos);
            lastSpawnedGridPos = spawnPos;
        }

        private Vector2Int GetRandomGridPosition()
        {
            List<Vector2Int> candidates = new List<Vector2Int>();
            List<Vector2Int> fallbackCandidates = new List<Vector2Int>();

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (!occupiedCells.Contains(pos))
                    {
                        fallbackCandidates.Add(pos);
                        if (lastSpawnedGridPos.x != -1)
                        {
                            float dist = Vector2.Distance(GridToWorld(pos), GridToWorld(lastSpawnedGridPos));
                            if (dist >= minDistanceBetweenSequential) candidates.Add(pos);
                        }
                        else candidates.Add(pos);
                    }
                }
            }

            if (candidates.Count > 0) return candidates[Random.Range(0, candidates.Count)];
            if (fallbackCandidates.Count > 0) return fallbackCandidates[Random.Range(0, fallbackCandidates.Count)];
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

        private Vector2Int WorldToGrid(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize.x);
            int y = Mathf.FloorToInt((worldPos.y - gridOrigin.y) / cellSize.y);
            return new Vector2Int(x, y);
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


UIManager.cs
Manages HUD (Time, Score, Combo), Start Screen, Game Over Panel, and Volume Slider.

Key Feature: Updates the Combo Progress Bar (Fill Amount).

AudioManager.cs
Manages SFX (Pop, Error) and BGM.

Key Feature: Increases SFX pitch based on Combo count.

B. Gameplay
Bubble.cs
Uses IPointerDownHandler for responsive touch.

Visuals: Scale animation on spawn, Flash color on wrong touch.

Structure: Parent (Sprite) -> Child (TextMeshPro 3D).

using UnityEngine;
using TMPro;
using Managers;
using UnityEngine.EventSystems;
using System.Collections;

namespace Gameplay
{
    [RequireComponent(typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public class Bubble : MonoBehaviour, IPointerDownHandler
    {
        [Header("Components")]
        [SerializeField] private TextMeshPro numberText; 
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [Header("Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color correctColor = Color.green;
        [SerializeField] private Color wrongColor = Color.red;

        public int Number { get; private set; }
        private bool isInteracting = false;

        public void Initialize(int number)
        {
            Number = number;
            gameObject.name = $"Bubble_{number}";
            if (numberText != null) numberText.text = number.ToString();
            spriteRenderer.color = normalColor;
            isInteracting = false;
            StartCoroutine(SpawnAnimation());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isInteracting || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            int currentTarget = GameManager.Instance.CurrentTargetNumber;
            if (Number == currentTarget) OnCorrectTouch();
            else OnWrongTouch();
        }

        private void OnCorrectTouch()
        {
            isInteracting = true; 
            GameManager.Instance.ValidateInput(Number);
            BubbleManager.Instance.OnBubbleCorrect(this);
        }

        private void OnWrongTouch()
        {
            GameManager.Instance.ValidateInput(Number);
            StartCoroutine(FlashColor(wrongColor));
        }

        private IEnumerator SpawnAnimation() { /* Scale Up Logic */ yield return null; }
        private IEnumerator FlashColor(Color color) { /* Color Flash Logic */ yield return null; }
    }
}

3. Unity Scene Setup (Crucial)
Hierarchy
Main Camera: Contains Physics 2D Raycaster (Essential for Bubble touch).

GameManager: Holds GameManager, BubbleManager, UIManager, AudioManager.

Canvas: Scale With Screen Size.

Start_Screen: Panel (Black BG) -> Start Button, Volume Slider (Top-Right).

HUD: Time Text, Score Text, Combo Text (Parent of Combo Bar).

Game_Over: Panel -> Final Score, Retry Button.

Bubble Container: Empty object to hold spawned bubbles.

Prefabs
Bubble:

SpriteRenderer (Circle image).

CircleCollider2D (IsTrigger = true).

Script: Bubble.cs.

Child: Text (TMP) [3D Object], Order In Layer = 10, Pos Z = -1.

4. Known Issues & Recent Fixes
Fixed: Number skipping issue (Implemented Max+1 spawn logic).

Fixed: CS0136 Local variable naming conflict in BubbleManager.

Fixed: IsPlaying property mismatch in Bubble.cs (Updated to CurrentState).

Fixed: UI Overlapping (Volume slider moved, Combo bar attached to text). """