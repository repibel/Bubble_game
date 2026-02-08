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
        public Vector2Int GridPosition { get; private set; }
        private bool isInteracting = false;

        public void Initialize(int number, Vector2Int gridPos)
        {
            Number = number;
            GridPosition = gridPos;
            gameObject.name = $"Bubble_{number}";
            
            if (numberText != null)
                numberText.text = number.ToString();
            
            spriteRenderer.color = normalColor;
            isInteracting = false;

            StartCoroutine(SpawnAnimation());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // [수정된 부분] IsPlaying 대신 CurrentState를 확인합니다.
            if (isInteracting || GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing) return;

            int currentTarget = GameManager.Instance.CurrentTargetNumber;

            if (Number == currentTarget)
            {
                OnCorrectTouch();
            }
            else
            {
                OnWrongTouch();
            }
        }

        private void OnCorrectTouch()
        {
            isInteracting = true; 
            
            if (Managers.SoundManager.Instance != null)
                Managers.SoundManager.Instance.PlayPopSound();

            GameManager.Instance.ValidateInput(Number);
            Managers.BubbleManager.Instance.OnBubbleCorrect(this);
        }

        private void OnWrongTouch()
        {
            if (Managers.SoundManager.Instance != null)
                Managers.SoundManager.Instance.PlayWrongSound();

            GameManager.Instance.ValidateInput(Number);
            StartCoroutine(FlashColor(wrongColor));
        }

        private IEnumerator SpawnAnimation()
        {
            float duration = 0.3f;
            float time = 0;
            Vector3 originalScale = new Vector3(1.2f, 1.2f, 1f);

            transform.localScale = Vector3.zero;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float scale = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.1f; 
                if (scale > 1f) scale = 1f; 

                transform.localScale = originalScale * t;
                yield return null;
            }
            transform.localScale = originalScale;
        }

        private IEnumerator FlashColor(Color color)
        {
            spriteRenderer.color = color;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = normalColor;
        }
    }
}