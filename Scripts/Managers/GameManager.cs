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
        
        [Header("Combo Difficulty")]
        [SerializeField] private float startComboTime = 1.5f; // 초기 여유 시간
        [SerializeField] private float minComboTime = 0.3f;   // 최소 마지노선
        [SerializeField] private int difficultyMaxCombo = 60; // 이 콤보에 도달하면 0.3초가 됨

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

            // 1. 전체 게임 시간
            CurrentTime -= Time.deltaTime;
            if (CurrentTime <= 0)
            {
                EndGame();
                return;
            }

            // 2. 콤보 게이지 (생존 시간)
            // 콤보가 0일 때는 게이지가 줄지 않음 (첫 스타트는 여유 있게)
            if (ComboCount > 0)
            {
                CurrentComboTime -= Time.deltaTime;
                float ratio = CurrentComboTime / MaxCurrentComboTime;
                OnComboTimeUpdated?.Invoke(ratio);
                
                if (CurrentComboTime <= 0)
                {
                    // 시간 초과 -> 콤보 초기화
                    ResetCombo();
                }
            }
            else
            {
                // 콤보가 없으면 게이지 꽉 채워둠
                OnComboTimeUpdated?.Invoke(1f);
            }
        }

        public void StartGame()
        {
            CurrentTime = gameDuration;
            CurrentTargetNumber = 1;
            ComboCount = 0;
            MaxCombo = 0;
            
            // 초기 콤보 시간 설정
            MaxCurrentComboTime = startComboTime;
            CurrentComboTime = MaxCurrentComboTime;
            
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
            
            // [난이도 조절 핵심] 콤보가 오를수록 시간이 0.3초까지 줄어듦
            float progress = Mathf.Clamp01((float)ComboCount / difficultyMaxCombo);
            MaxCurrentComboTime = Mathf.Lerp(startComboTime, minComboTime, progress);
            
            // 콤보 성공 시 시간 리필
            CurrentComboTime = MaxCurrentComboTime;

            OnComboUpdated?.Invoke(ComboCount);
        }

        public void ResetCombo()
        {
            if (ComboCount > 0)
            {
                ComboCount = 0;
                OnComboUpdated?.Invoke(ComboCount);
                
                // 콤보 끊기면 시간도 초기화
                MaxCurrentComboTime = startComboTime;
                CurrentComboTime = startComboTime; 
                OnComboTimeUpdated?.Invoke(1f);
            }
        }
    }
}