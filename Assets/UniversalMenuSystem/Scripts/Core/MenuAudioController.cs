// ============================================================================
// Universal Menu System - Menu Audio Controller
// Compatible with Unity 6000.0+
// ============================================================================
// Plays UI sounds for menu interactions: hover, click, back, error.
// Horror-themed — designed for subtle, atmospheric sound design.
// ============================================================================

using UnityEngine;

namespace UniversalMenuSystem.Core
{
    public class MenuAudioController : MonoBehaviour
    {
        public static MenuAudioController Instance { get; private set; }

        [Header("Audio Source")]
        [SerializeField] private AudioSource uiAudioSource;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip selectSound;
        [SerializeField] private AudioClip backSound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip openMenuSound;
        [SerializeField] private AudioClip closeMenuSound;
        [SerializeField] private AudioClip sliderTickSound;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] private float uiVolume = 0.5f;
        [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.3f;

        [Header("Ambient Menu Loop (optional)")]
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioClip menuAmbience;
        [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.2f;
        [SerializeField] private float ambienceFadeSpeed = 1f;

        private float _ambienceTargetVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (uiAudioSource == null)
            {
                uiAudioSource = gameObject.AddComponent<AudioSource>();
                uiAudioSource.playOnAwake = false;
                uiAudioSource.spatialBlend = 0f;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.OnMenuOpened += HandleMenuOpened;
                MenuManager.Instance.OnMenuClosed += HandleMenuClosed;
            }
        }

        private void OnDisable()
        {
            if (MenuManager.Instance != null)
            {
                MenuManager.Instance.OnMenuOpened -= HandleMenuOpened;
                MenuManager.Instance.OnMenuClosed -= HandleMenuClosed;
            }
        }

        private void Update()
        {
            if (ambientSource != null)
            {
                ambientSource.volume = Mathf.MoveTowards(
                    ambientSource.volume, _ambienceTargetVolume,
                    ambienceFadeSpeed * Time.unscaledDeltaTime);
            }
        }

        public void PlayHover() => PlayClip(hoverSound, hoverVolume);
        public void PlaySelect() => PlayClip(selectSound, uiVolume);
        public void PlayBack() => PlayClip(backSound, uiVolume);
        public void PlayError() => PlayClip(errorSound, uiVolume);
        public void PlaySliderTick() => PlayClip(sliderTickSound, hoverVolume * 0.5f);

        public void StartAmbience()
        {
            if (ambientSource == null || menuAmbience == null) return;
            if (!ambientSource.isPlaying)
            {
                ambientSource.clip = menuAmbience;
                ambientSource.loop = true;
                ambientSource.volume = 0f;
                ambientSource.Play();
            }
            _ambienceTargetVolume = ambienceVolume;
        }

        public void StopAmbience()
        {
            _ambienceTargetVolume = 0f;
        }

        public void SetUIVolume(float vol)
        {
            uiVolume = Mathf.Clamp01(vol);
        }

        private void PlayClip(AudioClip clip, float volume)
        {
            if (clip == null || uiAudioSource == null) return;
            uiAudioSource.PlayOneShot(clip, volume);
        }

        private void HandleMenuOpened(MenuState state)
        {
            PlayClip(openMenuSound, uiVolume);
            if (state == MenuState.MainMenu)
                StartAmbience();
        }

        private void HandleMenuClosed(MenuState state)
        {
            PlayClip(closeMenuSound, uiVolume * 0.7f);
            if (state == MenuState.MainMenu)
                StopAmbience();
        }
    }
}
