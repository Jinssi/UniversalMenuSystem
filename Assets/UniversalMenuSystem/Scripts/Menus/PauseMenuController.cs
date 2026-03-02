// ============================================================================
// Universal Menu System - Pause Menu Controller
// Compatible with Unity 6000.0+
// ============================================================================
// In-game pause menu. Triggered by Escape (via GameManager).
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using UniversalMenuSystem.Core;

namespace UniversalMenuSystem.Menus
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        private void OnEnable()
        {
            resumeButton?.onClick.AddListener(OnResume);
            saveGameButton?.onClick.AddListener(OnSaveGame);
            loadGameButton?.onClick.AddListener(OnLoadGame);
            settingsButton?.onClick.AddListener(OnSettings);
            mainMenuButton?.onClick.AddListener(OnMainMenu);
            quitButton?.onClick.AddListener(OnQuit);
        }

        private void OnDisable()
        {
            resumeButton?.onClick.RemoveListener(OnResume);
            saveGameButton?.onClick.RemoveListener(OnSaveGame);
            loadGameButton?.onClick.RemoveListener(OnLoadGame);
            settingsButton?.onClick.RemoveListener(OnSettings);
            mainMenuButton?.onClick.RemoveListener(OnMainMenu);
            quitButton?.onClick.RemoveListener(OnQuit);
        }

        private void OnResume()
        {
            MenuManager.Instance?.CloseAllMenus();
            if (GameManager.Instance != null)
                GameManager.Instance.TogglePause();
            else
                Time.timeScale = 1f;
        }

        private void OnSaveGame() => MenuManager.Instance?.NavigateTo(MenuState.SaveMenu);
        private void OnLoadGame() => MenuManager.Instance?.NavigateTo(MenuState.LoadMenu);
        private void OnSettings() => MenuManager.Instance?.NavigateTo(MenuState.Settings);

        private void OnMainMenu()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ReturnToMainMenu();
            else
            {
                Time.timeScale = 1f;
                MenuManager.Instance?.CloseAllMenus();
            }
        }

        private void OnQuit() => MenuManager.Instance?.NavigateTo(MenuState.ConfirmQuit);
    }
}
