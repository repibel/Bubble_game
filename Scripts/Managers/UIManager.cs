using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        [Header("Start Screen")]
        [SerializeField] private GameObject startPanel;
        [SerializeField] private Button startButton;
        [SerializeField] private Slider volumeSlider;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI scoreText; // 변수명 targetText -> scoreText로 이해하기 쉽게 변경 권장
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private Image timeProgressBar; 
        [SerializeField] private Image comboProgressBar;

        [Header("Game Over Panel")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI maxComboText;
        [SerializeField] private Button retryButton;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += OnGameStart;
                GameManager.Instance.OnGameEnd += OnGameEnd;
                GameManager.Instance.OnTargetNumberChanged += OnTargetUpdate;
                GameManager.Instance.OnComboUpdated += OnComboUpdate;
                GameManager.Instance.OnComboTimeUpdated += OnComboTimeUpdate;
            }

            if (startButton != null) startButton.onClick.AddListener(OnStartClicked);
            if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
                volumeSlider.value = AudioListener.volume; 
            }

            if (startPanel != null) startPanel.SetActive(true);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            
            // [수정] 시작 시 UI 0점으로 초기화
            if (scoreText != null) scoreText.text = "0";
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= OnGameStart;
                GameManager.Instance.OnGameEnd -= OnGameEnd;
                GameManager.Instance.OnTargetNumberChanged -= OnTargetUpdate;
                GameManager.Instance.OnComboUpdated -= OnComboUpdate;
                GameManager.Instance.OnComboTimeUpdated -= OnComboTimeUpdate;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                float currentTime = GameManager.Instance.CurrentTime;
                float totalTime = GameManager.Instance.TotalGameDuration;

                if (timeText != null) timeText.text = $"{currentTime:F1}";
                if (timeProgressBar != null && totalTime > 0) timeProgressBar.fillAmount = currentTime / totalTime;
            }
        }

        private void OnStartClicked()
        {
            if (startPanel != null) startPanel.SetActive(false);
            GameManager.Instance.StartGame();
        }

        private void OnRetryClicked()
        {
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            GameManager.Instance.StartGame();
        }

        private void OnVolumeChanged(float value)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetBgmVolume(value);
                SoundManager.Instance.SetSfxVolume(value);
            }
        }

        private void OnGameStart()
        {
            if (comboText != null) comboText.text = "";
            if (comboProgressBar != null) comboProgressBar.fillAmount = 0f;
            
            // [수정] 게임 시작 시 점수는 0
            if (scoreText != null) scoreText.text = "0";
        }

        private void OnGameEnd()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                // 점수 계산: (도달한 타겟 - 1)
                int score = Mathf.Max(0, GameManager.Instance.CurrentTargetNumber - 1);
                
                if (finalScoreText != null) finalScoreText.text = $"Score: {score}";
                if (maxComboText != null) maxComboText.text = $"Max Combo: {GameManager.Instance.MaxCombo}";
            }
        }

        // [핵심 로직] 타겟이 1번이면 0점, 2번이면 1점 표시
        private void OnTargetUpdate(int target)
        {
            int displayScore = Mathf.Max(0, target - 1);
            if (scoreText != null) scoreText.text = $"{displayScore}";
        }

        private void OnComboUpdate(int combo)
        {
            if (comboText != null)
                comboText.text = (combo > 1) ? $"{combo} Combo!" : "";
        }

        private void OnComboTimeUpdate(float ratio)
        {
            if (comboProgressBar != null)
            {
                comboProgressBar.fillAmount = ratio;
                comboProgressBar.color = (ratio < 0.3f) ? Color.red : Color.yellow;
            }
        }
    }
}