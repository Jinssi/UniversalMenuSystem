// ============================================================================
// Universal Menu System - Save Slot UI Entry
// Compatible with Unity 6000.0+
// ============================================================================
// UI representation of a single save slot. Text-based, horror-styled.
// ============================================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UniversalMenuSystem.SaveLoad
{
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI slotNameText;
        [SerializeField] private TextMeshProUGUI timestampText;
        [SerializeField] private TextMeshProUGUI locationText;
        [SerializeField] private TextMeshProUGUI playTimeText;
        [SerializeField] private TextMeshProUGUI emptyLabel;
        [SerializeField] private Button slotButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private GameObject dataContainer;
        [SerializeField] private GameObject emptyContainer;

        private SaveSlotData _data;
        private Action<SaveSlotData> _onSelectCallback;
        private Action<SaveSlotData> _onDeleteCallback;

        public void Setup(SaveSlotData data, Action<SaveSlotData> onSelect, Action<SaveSlotData> onDelete = null)
        {
            _data = data;
            _onSelectCallback = onSelect;
            _onDeleteCallback = onDelete;

            if (data.hasData)
            {
                if (dataContainer != null) dataContainer.SetActive(true);
                if (emptyContainer != null) emptyContainer.SetActive(false);
                if (slotNameText != null) slotNameText.text = data.displayName;
                if (timestampText != null) timestampText.text = data.GetFormattedTimestamp();
                if (locationText != null) locationText.text = data.locationName;
                if (playTimeText != null) playTimeText.text = data.GetFormattedPlayTime();

                if (deleteButton != null)
                {
                    deleteButton.gameObject.SetActive(true);
                    deleteButton.onClick.RemoveAllListeners();
                    deleteButton.onClick.AddListener(() => _onDeleteCallback?.Invoke(_data));
                }
            }
            else
            {
                if (dataContainer != null) dataContainer.SetActive(false);
                if (emptyContainer != null) emptyContainer.SetActive(true);
                if (emptyLabel != null) emptyLabel.text = $"\u2014 {data.displayName} \u2014   Empty";
                if (slotNameText != null) slotNameText.text = data.displayName;
                if (deleteButton != null)
                    deleteButton.gameObject.SetActive(false);
            }

            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => _onSelectCallback?.Invoke(_data));
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (slotButton != null)
                slotButton.interactable = interactable;
            float alpha = interactable ? 1f : 0.3f;
            if (slotNameText != null) { Color c = slotNameText.color; c.a = alpha; slotNameText.color = c; }
            if (emptyLabel != null) { Color c = emptyLabel.color; c.a = alpha; emptyLabel.color = c; }
        }
    }
}
