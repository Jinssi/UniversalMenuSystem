// ============================================================================
// Universal Menu System - Load Menu Controller
// Compatible with Unity 6000.0+
// ============================================================================
// Shows all save slots. Player selects one to load.
// ============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniversalMenuSystem.Core;

namespace UniversalMenuSystem.SaveLoad
{
    public class LoadMenuController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject saveSlotPrefab;
        [SerializeField] private Button backButton;

        [Header("Confirm Load")]
        [SerializeField] private GameObject confirmLoadPanel;
        [SerializeField] private TextMeshProUGUI confirmText;
        [SerializeField] private Button confirmYesButton;
        [SerializeField] private Button confirmNoButton;

        [Header("Text")]
        [SerializeField] private string headerLabel = "LOAD GAME";
        [SerializeField] private string confirmMessage = "Load this save? Unsaved progress will be lost.";

        public Action<string, SaveSlotData> OnPayloadLoaded { get; set; }

        private readonly List<GameObject> _spawnedSlots = new List<GameObject>();
        private SaveSlotData _pendingLoadSlot;

        private void OnEnable()
        {
            if (headerText != null) headerText.text = headerLabel;
            if (confirmLoadPanel != null) confirmLoadPanel.SetActive(false);
            backButton?.onClick.AddListener(OnBack);
            confirmYesButton?.onClick.AddListener(OnConfirmLoad);
            confirmNoButton?.onClick.AddListener(OnCancelLoad);
            RefreshSlots();
        }

        private void OnDisable()
        {
            backButton?.onClick.RemoveListener(OnBack);
            confirmYesButton?.onClick.RemoveListener(OnConfirmLoad);
            confirmNoButton?.onClick.RemoveListener(OnCancelLoad);
            ClearSlots();
        }

        private void RefreshSlots()
        {
            ClearSlots();
            if (SaveLoadManager.Instance == null || saveSlotPrefab == null || slotContainer == null)
            {
                Debug.LogWarning("[LoadMenu] Missing SaveLoadManager, prefab, or container.");
                return;
            }
            List<SaveSlotData> slots = SaveLoadManager.Instance.GetAllSlots();
            foreach (SaveSlotData slot in slots)
            {
                GameObject go = Instantiate(saveSlotPrefab, slotContainer);
                SaveSlotUI ui = go.GetComponent<SaveSlotUI>();
                if (ui != null)
                {
                    ui.Setup(slot, OnSlotSelected, OnSlotDelete);
                    if (!slot.hasData)
                        ui.SetInteractable(false);
                }
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
            if (!slot.hasData) return;
            _pendingLoadSlot = slot;
            if (confirmLoadPanel != null)
            {
                confirmLoadPanel.SetActive(true);
                if (confirmText != null) confirmText.text = confirmMessage;
            }
            else
                LoadFromSlot(slot);
        }

        private void OnConfirmLoad()
        {
            if (_pendingLoadSlot != null)
            {
                LoadFromSlot(_pendingLoadSlot);
                _pendingLoadSlot = null;
            }
            if (confirmLoadPanel != null) confirmLoadPanel.SetActive(false);
        }

        private void OnCancelLoad()
        {
            _pendingLoadSlot = null;
            if (confirmLoadPanel != null) confirmLoadPanel.SetActive(false);
        }

        private void LoadFromSlot(SaveSlotData slot)
        {
            if (SaveLoadManager.Instance.Load(slot.slotId, out string payload))
            {
                MenuManager.Instance?.CloseAllMenus();
                OnPayloadLoaded?.Invoke(payload, slot);
            }
        }

        private void OnSlotDelete(SaveSlotData slot)
        {
            SaveLoadManager.Instance?.DeleteSlot(slot.slotId);
            RefreshSlots();
        }

        private void OnBack() => MenuManager.Instance?.GoBack();
    }
}
