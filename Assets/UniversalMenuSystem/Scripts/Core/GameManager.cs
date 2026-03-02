// ============================================================================
// Universal Menu System - Game Manager
// Compatible with Unity 6000.0+
// ============================================================================
// Lightweight game state tracker. Knows whether we're in the main menu scene
// or in gameplay. Listens for Escape key to toggle pause.
// ============================================================================

using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniversalMenuSystem.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Configuration")]
        [Tooltip("Name of the main menu scene. Leave empty if main menu is in-scene.")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Tooltip("Key to toggle pause menu during gameplay.")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        [Header("State (Read Only)")]
        [SerializeField] private bool isInMainMenu = true;
        [SerializeField] private bool isPaused = false;

        public bool IsInMainMenu => isInMainMenu;
        public bool IsPaused => isPaused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (!isInMainMenu && Input.GetKeyDown(pauseKey))
                TogglePause();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            isInMainMenu = scene.name == mainMenuSceneName;
            if (isInMainMenu)
            {
                isPaused = false;
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (MenuManager.Instance != null)
                    MenuManager.Instance.ShowMainMenu();
            }
        }

        public void TogglePause()
        {
            if (MenuManager.Instance == null) return;
            MenuManager.Instance.TogglePause();
            isPaused = MenuManager.Instance.IsMenuOpen;
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
        }

        public void StartNewGame(string gameplaySceneName)
        {
            isInMainMenu = false;
            isPaused = false;
            Time.timeScale = 1f;
            MenuManager.Instance?.LoadScene(gameplaySceneName);
        }

        public void ReturnToMainMenu()
        {
            isPaused = false;
            Time.timeScale = 1f;
            MenuManager.Instance?.CloseAllMenus();
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
