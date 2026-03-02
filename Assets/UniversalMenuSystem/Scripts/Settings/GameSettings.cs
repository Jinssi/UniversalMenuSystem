// ============================================================================
// Universal Menu System - Game Settings Manager
// Compatible with Unity 6000.0+
// ============================================================================
// Persists settings to PlayerPrefs (JSON). Applies settings to Unity systems.
// ============================================================================

using System;
using UnityEngine;

namespace UniversalMenuSystem.Settings
{
    public class GameSettings : MonoBehaviour
    {
        public static GameSettings Instance { get; private set; }

        private const string PREFS_KEY = "UMS_GameSettings";

        [SerializeField] private SettingsData currentSettings;

        public SettingsData Current => currentSettings;

        public event Action<SettingsData> OnSettingsApplied;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
            Apply();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Load()
        {
            string json = PlayerPrefs.GetString(PREFS_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    currentSettings = JsonUtility.FromJson<SettingsData>(json);
                }
                catch
                {
                    Debug.LogWarning("[GameSettings] Corrupted settings data. Resetting to defaults.");
                    currentSettings = SettingsData.CreateDefaults();
                }
            }
            else
            {
                currentSettings = SettingsData.CreateDefaults();
            }
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(currentSettings, true);
            PlayerPrefs.SetString(PREFS_KEY, json);
            PlayerPrefs.Save();
        }

        public void ResetToDefaults()
        {
            currentSettings = SettingsData.CreateDefaults();
            Apply();
            Save();
        }

        public void Apply()
        {
            ApplyDisplay();
            ApplyAudio();
            ApplyGameplay();
            OnSettingsApplied?.Invoke(currentSettings);
        }

        private void ApplyDisplay()
        {
            if (currentSettings.resolutionIndex >= 0 &&
                currentSettings.resolutionIndex < Screen.resolutions.Length)
            {
                Resolution res = Screen.resolutions[currentSettings.resolutionIndex];
                Screen.SetResolution(res.width, res.height,
                    currentSettings.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            }
            if (currentSettings.qualityLevel >= 0 &&
                currentSettings.qualityLevel < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(currentSettings.qualityLevel, true);
            }
            QualitySettings.vSyncCount = currentSettings.vsyncCount;
            Application.targetFrameRate = currentSettings.targetFrameRate;
        }

        private void ApplyAudio()
        {
            float master = currentSettings.muteAll ? 0f : currentSettings.masterVolume;
            AudioListener.volume = master;
        }

        private void ApplyGameplay()
        {
            // FOV, sensitivity, etc. are game-specific.
            // They're stored in currentSettings and can be queried at any time.
        }

        public void SetMasterVolume(float vol) { currentSettings.masterVolume = Mathf.Clamp01(vol); ApplyAudio(); }
        public void SetMusicVolume(float vol) { currentSettings.musicVolume = Mathf.Clamp01(vol); }
        public void SetSFXVolume(float vol) { currentSettings.sfxVolume = Mathf.Clamp01(vol); }
        public void SetFullscreen(bool fs) { currentSettings.fullscreen = fs; ApplyDisplay(); }
        public void SetResolution(int index) { currentSettings.resolutionIndex = index; ApplyDisplay(); }
        public void SetQuality(int level) { currentSettings.qualityLevel = level; ApplyDisplay(); }
        public void SetMouseSensitivity(float sens) { currentSettings.mouseSensitivity = Mathf.Clamp(sens, 0.1f, 5f); }
    }
}
