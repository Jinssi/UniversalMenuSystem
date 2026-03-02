// ============================================================================
// Universal Menu System - Confirm Quit Panel
// Compatible with Unity 6000.0+
// ============================================================================
// Simple Yes/No confirmation overlay for quitting the game.
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniversalMenuSystem.Core;

namespace UniversalMenuSystem.Menus
{
    public class ConfirmQuitController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        [Header("Text")]
        [SerializeField] private string confirmMessage = "Are you sure you want to quit?";

        private void OnEnable()
        {
            if (promptText != null)
                promptText.text = confirmMessage;
            yesButton?.onClick.AddListener(OnYes);
            noButton?.onClick.AddListener(OnNo);
        }

        private void OnDisable()
        {
            yesButton?.onClick.RemoveListener(OnYes);
            noButton?.onClick.RemoveListener(OnNo);
        }

        private void OnYes() => MenuManager.Instance?.QuitGame();
        private void OnNo() => MenuManager.Instance?.GoBack();
    }
}
