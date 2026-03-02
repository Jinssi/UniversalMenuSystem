// ============================================================================
// Universal Menu System - Main Menu Controller
// Compatible with Unity 6000.0+
// ============================================================================
// Drives the main menu screen. Wire up buttons in the Inspector.
// Layout: Resident Evil style - right-aligned text buttons.
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniversalMenuSystem.Core;

namespace UniversalMenuSystem.Menus
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons (TextMeshPro with Button component)")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private string gameTitle = "YOUR GAME TITLE";

        [Header("Scene References")]
        [Tooltip("Scene name to load on New Game.")]
        [SerializeField] private string newGameScene = "Gameplay";

        [Header("Continue Logic")]
        [SerializeField] private bool hasSaveData = false;

        private void Awake()
        {
            if (titleText != null)
                titleText.text = gameTitle;
        }

        private void OnEnable()
        {
            newGameButton?.onClick.AddListener(OnNewGame);
            continueButton?.onClick.AddListener(OnContinue);
            loadGameButton?.onClick.AddListener(OnLoadGame);
            settingsButton?.onClick.AddListener(OnSettings);
            quitButton?.onClick.AddListener(OnQuit);
            UpdateContinueButton();
        }

        private void OnDisable()
        {
            newGameButton?.onClick.RemoveListener(OnNewGame);
            continueButton?.onClick.RemoveListener(OnContinue);
            loadGameButton?.onClick.RemoveListener(OnLoadGame);
            settingsButton?.onClick.RemoveListener(OnSettings);
            quitButton?.onClick.RemoveListener(OnQuit);
        }

        public void SetHasSaveData(bool has)
        {
            hasSaveData = has;
            UpdateContinueButton();
        }

        private void UpdateContinueButton()
        {
            if (continueButton == null) return;
            continueButton.interactable = hasSaveData;
            var text = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                Color c = text.color;
                c.a = hasSaveData ? 1f : 0.3f;
                text.color = c;
            }
        }

        private void OnNewGame()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StartNewGame(newGameScene);
            else
                MenuManager.Instance?.LoadScene(newGameScene);
        }

        private void OnContinue()
        {
            if (!hasSaveData) return;
            Debug.Log("[MenuSystem] Continue pressed \u2014 inject your save system's load-latest logic here.");
        }

        private void OnLoadGame() => MenuManager.Instance?.NavigateTo(MenuState.LoadMenu);
        private void OnSettings() => MenuManager.Instance?.NavigateTo(MenuState.Settings);
        private void OnQuit() => MenuManager.Instance?.NavigateTo(MenuState.ConfirmQuit);
    }
}
