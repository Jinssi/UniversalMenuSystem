// ============================================================================
// Universal Menu System - Settings Menu Controller
// Compatible with Unity 6000.0+
// ============================================================================
// Full settings menu with tabs: Display, Audio, Gameplay, Accessibility.
// Supports apply/revert/defaults workflow.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniversalMenuSystem.Core;
using UniversalMenuSystem.Settings;

namespace UniversalMenuSystem.Menus
{
    public class SettingsMenuController : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private Button displayTabButton;
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button gameplayTabButton;
        [SerializeField] private Button accessibilityTabButton;

        [SerializeField] private GameObject displayPanel;
        [SerializeField] private GameObject audioPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject accessibilityPanel;

        [Header("Display")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Slider brightnessSlider;
        [SerializeField] private TextMeshProUGUI brightnessValueText;

        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider voiceVolumeSlider;
        [SerializeField] private Slider ambientVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterValueText;
        [SerializeField] private TextMeshProUGUI musicValueText;
        [SerializeField] private TextMeshProUGUI sfxValueText;
        [SerializeField] private TextMeshProUGUI voiceValueText;
        [SerializeField] private TextMeshProUGUI ambientValueText;
        [SerializeField] private Toggle muteAllToggle;

        [Header("Gameplay")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TextMeshProUGUI sensitivityValueText;
        [SerializeField] private Toggle invertYToggle;
        [SerializeField] private Slider fovSlider;
        [SerializeField] private TextMeshProUGUI fovValueText;
        [SerializeField] private Toggle subtitlesToggle;
        [SerializeField] private TMP_Dropdown subtitleSizeDropdown;
        [SerializeField] private Toggle headBobToggle;
        [SerializeField] private Slider gammaSlider;
        [SerializeField] private TextMeshProUGUI gammaValueText;

        [Header("Accessibility")]
        [SerializeField] private Toggle reducedMotionToggle;
        [SerializeField] private Toggle highContrastToggle;
        [SerializeField] private Toggle screenShakeToggle;

        [Header("Action Buttons")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button revertButton;
        [SerializeField] private Button defaultsButton;
        [SerializeField] private Button backButton;

        private SettingsData _workingCopy;
        private Resolution[] _resolutions;
        private int _activeTabIndex;
        private readonly GameObject[] _panels = new GameObject[4];
        private readonly Button[] _tabButtons = new Button[4];

        private void Awake()
        {
            _panels[0] = displayPanel;
            _panels[1] = audioPanel;
            _panels[2] = gameplayPanel;
            _panels[3] = accessibilityPanel;
            _tabButtons[0] = displayTabButton;
            _tabButtons[1] = audioTabButton;
            _tabButtons[2] = gameplayTabButton;
            _tabButtons[3] = accessibilityTabButton;
        }

        private void OnEnable()
        {
            if (GameSettings.Instance != null)
                _workingCopy = GameSettings.Instance.Current.Clone();
            else
                _workingCopy = SettingsData.CreateDefaults();

            PopulateResolutions();
            PopulateQuality();
            PopulateSubtitleSizes();
            RefreshUI();
            SwitchTab(0);

            displayTabButton?.onClick.AddListener(() => SwitchTab(0));
            audioTabButton?.onClick.AddListener(() => SwitchTab(1));
            gameplayTabButton?.onClick.AddListener(() => SwitchTab(2));
            accessibilityTabButton?.onClick.AddListener(() => SwitchTab(3));

            applyButton?.onClick.AddListener(OnApply);
            revertButton?.onClick.AddListener(OnRevert);
            defaultsButton?.onClick.AddListener(OnDefaults);
            backButton?.onClick.AddListener(OnBack);

            WireSliders();
            WireToggles();
            WireDropdowns();
        }

        private void OnDisable()
        {
            displayTabButton?.onClick.RemoveAllListeners();
            audioTabButton?.onClick.RemoveAllListeners();
            gameplayTabButton?.onClick.RemoveAllListeners();
            accessibilityTabButton?.onClick.RemoveAllListeners();
            applyButton?.onClick.RemoveAllListeners();
            revertButton?.onClick.RemoveAllListeners();
            defaultsButton?.onClick.RemoveAllListeners();
            backButton?.onClick.RemoveAllListeners();
            UnwireSliders();
            UnwireToggles();
            UnwireDropdowns();
        }

        private void SwitchTab(int index)
        {
            _activeTabIndex = index;
            for (int i = 0; i < _panels.Length; i++)
            {
                if (_panels[i] != null)
                    _panels[i].SetActive(i == index);
                if (_tabButtons[i] != null)
                {
                    var txt = _tabButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null)
                    {
                        Color c = txt.color;
                        c.a = (i == index) ? 1f : 0.4f;
                        txt.color = c;
                    }
                }
            }
        }

        private void PopulateResolutions()
        {
            if (resolutionDropdown == null) return;
            _resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            int currentIndex = 0;
            for (int i = 0; i < _resolutions.Length; i++)
            {
                Resolution r = _resolutions[i];
                string label = $"{r.width} x {r.height} @ {Mathf.RoundToInt((float)r.refreshRateRatio.value)}Hz";
                options.Add(label);
                if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height)
                    currentIndex = i;
            }
            resolutionDropdown.AddOptions(options);
            if (_workingCopy.resolutionIndex >= 0 && _workingCopy.resolutionIndex < options.Count)
                resolutionDropdown.value = _workingCopy.resolutionIndex;
            else
                resolutionDropdown.value = currentIndex;
        }

        private void PopulateQuality()
        {
            if (qualityDropdown == null) return;
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
            int qi = _workingCopy.qualityLevel >= 0 ? _workingCopy.qualityLevel : QualitySettings.GetQualityLevel();
            qualityDropdown.value = qi;
        }

        private void PopulateSubtitleSizes()
        {
            if (subtitleSizeDropdown == null) return;
            subtitleSizeDropdown.ClearOptions();
            subtitleSizeDropdown.AddOptions(new List<string> { "Small", "Medium", "Large" });
            subtitleSizeDropdown.value = _workingCopy.subtitleSize;
        }

        private void RefreshUI()
        {
            if (fullscreenToggle != null) fullscreenToggle.isOn = _workingCopy.fullscreen;
            if (vsyncToggle != null) vsyncToggle.isOn = _workingCopy.vsyncCount > 0;
            if (brightnessSlider != null) { brightnessSlider.value = _workingCopy.brightness; UpdateLabel(brightnessValueText, _workingCopy.brightness, "0%", 100f); }
            if (masterVolumeSlider != null) { masterVolumeSlider.value = _workingCopy.masterVolume; UpdateLabel(masterValueText, _workingCopy.masterVolume, "0%", 100f); }
            if (musicVolumeSlider != null) { musicVolumeSlider.value = _workingCopy.musicVolume; UpdateLabel(musicValueText, _workingCopy.musicVolume, "0%", 100f); }
            if (sfxVolumeSlider != null) { sfxVolumeSlider.value = _workingCopy.sfxVolume; UpdateLabel(sfxValueText, _workingCopy.sfxVolume, "0%", 100f); }
            if (voiceVolumeSlider != null) { voiceVolumeSlider.value = _workingCopy.voiceVolume; UpdateLabel(voiceValueText, _workingCopy.voiceVolume, "0%", 100f); }
            if (ambientVolumeSlider != null) { ambientVolumeSlider.value = _workingCopy.ambientVolume; UpdateLabel(ambientValueText, _workingCopy.ambientVolume, "0%", 100f); }
            if (muteAllToggle != null) muteAllToggle.isOn = _workingCopy.muteAll;
            if (sensitivitySlider != null) { sensitivitySlider.value = _workingCopy.mouseSensitivity; UpdateLabel(sensitivityValueText, _workingCopy.mouseSensitivity, "0.0"); }
            if (invertYToggle != null) invertYToggle.isOn = _workingCopy.invertYAxis;
            if (fovSlider != null) { fovSlider.value = _workingCopy.fieldOfView; UpdateLabel(fovValueText, _workingCopy.fieldOfView, "0"); }
            if (subtitlesToggle != null) subtitlesToggle.isOn = _workingCopy.showSubtitles;
            if (headBobToggle != null) headBobToggle.isOn = _workingCopy.headBob;
            if (gammaSlider != null) { gammaSlider.value = _workingCopy.gamma; UpdateLabel(gammaValueText, _workingCopy.gamma, "0.0"); }
            if (reducedMotionToggle != null) reducedMotionToggle.isOn = _workingCopy.reducedMotion;
            if (highContrastToggle != null) highContrastToggle.isOn = _workingCopy.highContrastUI;
            if (screenShakeToggle != null) screenShakeToggle.isOn = _workingCopy.screenShake;
        }

        private void WireSliders()
        {
            brightnessSlider?.onValueChanged.AddListener(v => { _workingCopy.brightness = v; UpdateLabel(brightnessValueText, v, "0%", 100f); });
            masterVolumeSlider?.onValueChanged.AddListener(v => { _workingCopy.masterVolume = v; UpdateLabel(masterValueText, v, "0%", 100f); });
            musicVolumeSlider?.onValueChanged.AddListener(v => { _workingCopy.musicVolume = v; UpdateLabel(musicValueText, v, "0%", 100f); });
            sfxVolumeSlider?.onValueChanged.AddListener(v => { _workingCopy.sfxVolume = v; UpdateLabel(sfxValueText, v, "0%", 100f); });
            voiceVolumeSlider?.onValueChanged.AddListener(v => { _workingCopy.voiceVolume = v; UpdateLabel(voiceValueText, v, "0%", 100f); });
            ambientVolumeSlider?.onValueChanged.AddListener(v => { _workingCopy.ambientVolume = v; UpdateLabel(ambientValueText, v, "0%", 100f); });
            sensitivitySlider?.onValueChanged.AddListener(v => { _workingCopy.mouseSensitivity = v; UpdateLabel(sensitivityValueText, v, "0.0"); });
            fovSlider?.onValueChanged.AddListener(v => { _workingCopy.fieldOfView = v; UpdateLabel(fovValueText, v, "0"); });
            gammaSlider?.onValueChanged.AddListener(v => { _workingCopy.gamma = v; UpdateLabel(gammaValueText, v, "0.0"); });
        }

        private void UnwireSliders()
        {
            brightnessSlider?.onValueChanged.RemoveAllListeners();
            masterVolumeSlider?.onValueChanged.RemoveAllListeners();
            musicVolumeSlider?.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider?.onValueChanged.RemoveAllListeners();
            voiceVolumeSlider?.onValueChanged.RemoveAllListeners();
            ambientVolumeSlider?.onValueChanged.RemoveAllListeners();
            sensitivitySlider?.onValueChanged.RemoveAllListeners();
            fovSlider?.onValueChanged.RemoveAllListeners();
            gammaSlider?.onValueChanged.RemoveAllListeners();
        }

        private void WireToggles()
        {
            fullscreenToggle?.onValueChanged.AddListener(v => _workingCopy.fullscreen = v);
            vsyncToggle?.onValueChanged.AddListener(v => _workingCopy.vsyncCount = v ? 1 : 0);
            muteAllToggle?.onValueChanged.AddListener(v => _workingCopy.muteAll = v);
            invertYToggle?.onValueChanged.AddListener(v => _workingCopy.invertYAxis = v);
            subtitlesToggle?.onValueChanged.AddListener(v => _workingCopy.showSubtitles = v);
            headBobToggle?.onValueChanged.AddListener(v => _workingCopy.headBob = v);
            reducedMotionToggle?.onValueChanged.AddListener(v => _workingCopy.reducedMotion = v);
            highContrastToggle?.onValueChanged.AddListener(v => _workingCopy.highContrastUI = v);
            screenShakeToggle?.onValueChanged.AddListener(v => _workingCopy.screenShake = v);
        }

        private void UnwireToggles()
        {
            fullscreenToggle?.onValueChanged.RemoveAllListeners();
            vsyncToggle?.onValueChanged.RemoveAllListeners();
            muteAllToggle?.onValueChanged.RemoveAllListeners();
            invertYToggle?.onValueChanged.RemoveAllListeners();
            subtitlesToggle?.onValueChanged.RemoveAllListeners();
            headBobToggle?.onValueChanged.RemoveAllListeners();
            reducedMotionToggle?.onValueChanged.RemoveAllListeners();
            highContrastToggle?.onValueChanged.RemoveAllListeners();
            screenShakeToggle?.onValueChanged.RemoveAllListeners();
        }

        private void WireDropdowns()
        {
            resolutionDropdown?.onValueChanged.AddListener(v => _workingCopy.resolutionIndex = v);
            qualityDropdown?.onValueChanged.AddListener(v => _workingCopy.qualityLevel = v);
            subtitleSizeDropdown?.onValueChanged.AddListener(v => _workingCopy.subtitleSize = v);
        }

        private void UnwireDropdowns()
        {
            resolutionDropdown?.onValueChanged.RemoveAllListeners();
            qualityDropdown?.onValueChanged.RemoveAllListeners();
            subtitleSizeDropdown?.onValueChanged.RemoveAllListeners();
        }

        private void OnApply()
        {
            if (GameSettings.Instance == null) return;
            var json = JsonUtility.ToJson(_workingCopy);
            JsonUtility.FromJsonOverwrite(json, GameSettings.Instance.Current);
            GameSettings.Instance.Apply();
            GameSettings.Instance.Save();
        }

        private void OnRevert()
        {
            if (GameSettings.Instance != null)
                _workingCopy = GameSettings.Instance.Current.Clone();
            else
                _workingCopy = SettingsData.CreateDefaults();
            RefreshUI();
        }

        private void OnDefaults()
        {
            _workingCopy = SettingsData.CreateDefaults();
            RefreshUI();
        }

        private void OnBack() => MenuManager.Instance?.GoBack();

        private void UpdateLabel(TextMeshProUGUI label, float value, string format, float multiplier = 1f)
        {
            if (label == null) return;
            label.text = (value * multiplier).ToString(format);
        }
    }
}
