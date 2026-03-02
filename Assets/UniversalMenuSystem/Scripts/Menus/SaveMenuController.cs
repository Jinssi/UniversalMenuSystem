// ============================================================================
// Universal Menu System - Save Menu Controller
// Compatible with Unity 6000.0+
// ============================================================================
// Shows all save slots. Player selects a slot to save into.
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniversalMenuSystem.Core;

namespace UniversalMenuSystem.SaveLoad
{
    public class SaveMenuController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject saveSlotPrefab;
        [SerializeField] private Button backButton;

        [Header("Overwrite Confirmation")]
        [SerializeField] private GameObject confirmOverwritePanel;
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        [Header("Text")]
        [SerializeField] private string headerLabel = "SAVE GAME";
        [SerializeField] private string overwriteMessage = "Overwrite existing save?";

        public System.Func<string> GetSavePayload { get; set; }
        public System.Func<string> GetLocationName { get; set; }
        public System.Func<float> GetPlayTime { get; set; }

        private readonly List<GameObject> _spawnedSlots = new List<GameObject>();
        private SaveSlotData _pendingOverwriteSlot;

        private void OnEnable()
        {
            if (headerText != null) headerText.text = headerLabel;
            if (confirmOverwritePanel != null) confirmOverwritePanel.SetActive(false);
            backButton?.onClick.AddListener(OnBack);
            confirmYesButton?.onClick.AddListener(OnConfirmOverwrite);
            confirmNoButton?.onClick.AddListener(OnCancelOverwrite);
            RefreshSlots();
        }

        private void OnDisable()
        {
            backButton?.onClick.RemoveListener(OnBack);
            confirmYesButton?.onClick.RemoveListener(OnConfirmOverwrite);
            confirmNoButton?.onClick.RemoveListener(OnCancelOverwrite);
            ClearSlots();
        }

        private void RefreshSlots()
        {
            ClearSlots();
            if (SaveLoadManager.Instance == null || saveSlotPrefab == null || slotContainer == null)
            {
                Debug.LogWarning("[SaveMenu] Missing SaveLoadManager, prefab, or container.");
                return;
            }
            List<SaveSlotData> slots = SaveLoadManager.Instance.GetAllSlots();
            foreach (SaveSlotData slot in slots)
            {
                if (slot.slotId == SaveLoadManager.Instance.AutoSaveSlotId)
                    continue;
                GameObject go = Instantiate(saveSlotPrefab, slotContainer);
                SaveSlotUI ui = go.GetComponent<SaveSlotUI>();
                if (ui != null)
                    ui.Setup(slot, OnSlotSelected, OnSlotDelete);
                _spawnedSlots.Add(go);
            }
        }

        private void ClearSlots()
        {
            foreach (var go in _spawnedSlots)
                if (go != null) Destroy(go);
            _spawnedSlots.Clear();
        }

        private void OnSlotSelected(SaveSlotData slot)
        {
            if (slot.hasData)
            {
                _pendingOverwriteSlot = slot;
                if (confirmOverwritePanel != null)
                {
                    confirmOverwritePanel.SetActive(true);
                    if (confirmText != null) confirmText.text = overwriteMessage;
                }
            }
            else
                SaveToSlot(slot.slotId);
        }

        private void OnConfirmOverwrite()
        {
            if (_pendingOverwriteSlot != null)
            {
                SaveToSlot(_pendingOverwriteSlot.slotId);
                _pendingOverwriteSlot = null;
            }
            if (confirmOverwritePanel != null) confirmOverwritePanel.SetActive(false);
        }

        private void OnCancelOverwrite()
        {
            _pendingOverwriteSlot = null;
            if (confirmOverwritePanel != null) confirmOverwritePanel.SetActive(false);
        }

        private void SaveToSlot(string slotId)
        {
            string payload = GetSavePayload?.Invoke() ?? "{}";
            string location = GetLocationName?.Invoke() ?? "";
            float playTime = GetPlayTime?.Invoke() ?? 0f;
            SaveLoadManager.Instance.Save(slotId, payload, location, playTime);
            RefreshSlots();
        }

        private void OnSlotDelete(SaveSlotData slot)
        {
            SaveLoadManager.Instance?.DeleteSlot(slot.slotId);
            RefreshSlots();
        }

        private void OnBack() => MenuManager.Instance?.GoBack();
    }
}
