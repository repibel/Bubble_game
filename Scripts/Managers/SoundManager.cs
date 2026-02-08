using UnityEngine;

namespace Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip gameBgm;
        [SerializeField] private AudioClip bubblePopSfx;
        [SerializeField] private AudioClip wrongSfx;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart += PlayGameBGM;
                GameManager.Instance.OnGameEnd += StopBGM;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStart -= PlayGameBGM;
                GameManager.Instance.OnGameEnd -= StopBGM;
            }
        }

        public void PlayGameBGM()
        {
            if (bgmSource != null && gameBgm != null)
            {
                bgmSource.clip = gameBgm;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        public void StopBGM()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }
        }

        public void PlayPopSound()
        {
            PlaySFX(bubblePopSfx);
        }

        public void PlayWrongSound()
        {
            PlaySFX(wrongSfx);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
    }
}