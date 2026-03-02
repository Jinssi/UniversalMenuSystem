// ============================================================================
// Universal Menu System - Settings Data (Serializable)
// Compatible with Unity 6000.0+
// ============================================================================
// Pure data class for game settings. Serialized to JSON for persistence.
// ============================================================================

using System;
using UnityEngine;

namespace UniversalMenuSystem.Settings
{
    [Serializable]
    public class SettingsData
    {
        // Display
        public int resolutionIndex = -1;
        public int qualityLevel = -1;
        public bool fullscreen = true;
        public int vsyncCount = 1;
        public int targetFrameRate = -1;
        public float brightness = 1f;

        // Audio
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        public float voiceVolume = 1f;
        public float ambientVolume = 0.7f;
        public bool muteAll = false;

        // Gameplay
        public float mouseSensitivity = 1f;
        public bool invertYAxis = false;
        public float fieldOfView = 70f;
        public bool showSubtitles = true;
        public int subtitleSize = 1;
        public bool headBob = true;
        public float gamma = 1f;

        // Accessibility
        public bool reducedMotion = false;
        public bool highContrastUI = false;
        public bool screenShake = true;

        public SettingsData Clone()
        {
            return JsonUtility.FromJson<SettingsData>(JsonUtility.ToJson(this));
        }

        public static SettingsData CreateDefaults()
        {
            return new SettingsData();
        }
    }
}
