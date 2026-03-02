// ============================================================================
// Universal Menu System - Menu Manager (Central Hub)
// Compatible with Unity 6000.0+
// ============================================================================
// The MenuManager is the central orchestrator. It owns all menu panels,
// handles transitions with animated fades, and maintains a navigation stack
// so "Back" always works correctly.
// ============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniversalMenuSystem.Core
{
    public class MenuManager : MonoBehaviour
    {
        // ── Singleton ──────────────────────────────────────────────────────
        public static MenuManager Instance { get; private set; }

        // ── Inspector References ───────────────────────────────────────────
        [Header("Menu Panels (assign in Inspector)")]
        [SerializeField] private CanvasGroup mainMenuPanel;
        [SerializeField] private CanvasGroup pauseMenuPanel;
        [SerializeField] private CanvasGroup settingsPanel;
        [SerializeField] private CanvasGroup saveMenuPanel;
        [SerializeField] private CanvasGroup loadMenuPanel;
        [SerializeField] private CanvasGroup confirmQuitPanel;
        [SerializeField] private CanvasGroup creditsPanel;

        [Header("Transition Settings")]
        [SerializeField] private float fadeDuration = 0.4f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Background")]
        [SerializeField] private CanvasGroup backgroundOverlay;

        // ── State ──────────────────────────────────────────────────────────
        private MenuState _currentState = MenuState.None;
        private readonly Stack<MenuState> _navigationStack = new Stack<MenuState>();
        private Coroutine _transitionCoroutine;
        private bool _isTransitioning;

        public MenuState CurrentState => _currentState;
        public bool IsMenuOpen => _currentState != MenuState.None;
        public bool IsTransitioning => _isTransitioning;

        // ── Events ─────────────────────────────────────────────────────────
        public event Action<MenuState> OnMenuOpened;
        public event Action<MenuState> OnMenuClosed;
        public event Action<MenuState, MenuState> OnMenuTransition;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            HideAllImmediate();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void ShowMainMenu()
        {
            TransitionTo(MenuState.MainMenu, clearStack: true);
        }

        public void TogglePause()
        {
            if (_isTransitioning) return;
            if (_currentState == MenuState.PauseMenu)
                CloseAllMenus();
            else if (_currentState == MenuState.None)
            {
                TransitionTo(MenuState.PauseMenu, clearStack: true);
                Time.timeScale = 0f;
            }
        }

        public void NavigateTo(MenuState state)
        {
            if (_isTransitioning) return;
            TransitionTo(state, clearStack: false);
        }

        public void GoBack()
        {
            if (_isTransitioning) return;
            if (_navigationStack.Count > 0)
            {
                MenuState previous = _navigationStack.Pop();
                TransitionTo(previous, clearStack: false, pushToStack: false);
            }
            else
                CloseAllMenus();
        }

        public void CloseAllMenus()
        {
            if (_isTransitioning) return;
            TransitionTo(MenuState.None, clearStack: true);
            Time.timeScale = 1f;
        }

        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        public void LoadScene(int buildIndex)
        {
            Time.timeScale = 1f;
            StartCoroutine(LoadSceneByIndexRoutine(buildIndex));
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void TransitionTo(MenuState newState, bool clearStack, bool pushToStack = true)
        {
            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(TransitionRoutine(newState, clearStack, pushToStack));
        }

        private IEnumerator TransitionRoutine(MenuState newState, bool clearStack, bool pushToStack)
        {
            _isTransitioning = true;
            MenuState oldState = _currentState;
            OnMenuTransition?.Invoke(oldState, newState);

            if (pushToStack && oldState != MenuState.None)
                _navigationStack.Push(oldState);
            if (clearStack)
                _navigationStack.Clear();

            CanvasGroup currentPanel = GetPanel(oldState);
            if (currentPanel != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(currentPanel, 1f, 0f, fadeDuration));
                SetPanelActive(currentPanel, false);
                OnMenuClosed?.Invoke(oldState);
            }

            _currentState = newState;

            if (backgroundOverlay != null)
            {
                bool shouldShowBg = newState != MenuState.None;
                if (shouldShowBg && backgroundOverlay.alpha < 0.01f)
                {
                    backgroundOverlay.gameObject.SetActive(true);
                    yield return StartCoroutine(FadeCanvasGroup(backgroundOverlay, 0f, 1f, fadeDuration * 0.5f));
                }
                else if (!shouldShowBg && backgroundOverlay.alpha > 0.01f)
                {
                    yield return StartCoroutine(FadeCanvasGroup(backgroundOverlay, 1f, 0f, fadeDuration * 0.5f));
                    backgroundOverlay.gameObject.SetActive(false);
                }
            }

            CanvasGroup newPanel = GetPanel(newState);
            if (newPanel != null)
            {
                SetPanelActive(newPanel, true);
                yield return StartCoroutine(FadeCanvasGroup(newPanel, 0f, 1f, fadeDuration));
                OnMenuOpened?.Invoke(newState);
            }

            _isTransitioning = false;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null) yield break;
            float elapsed = 0f;
            group.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                group.alpha = Mathf.Lerp(from, to, fadeCurve.Evaluate(t));
                yield return null;
            }
            group.alpha = to;
        }

        private void SetPanelActive(CanvasGroup panel, bool active)
        {
            if (panel == null) return;
            panel.gameObject.SetActive(active);
            panel.interactable = active;
            panel.blocksRaycasts = active;
        }

        private void HideAllImmediate()
        {
            CanvasGroup[] allPanels = { mainMenuPanel, pauseMenuPanel, settingsPanel,
                                         saveMenuPanel, loadMenuPanel, confirmQuitPanel,
                                         creditsPanel };
            foreach (var panel in allPanels)
            {
                if (panel != null)
                {
                    panel.alpha = 0f;
                    SetPanelActive(panel, false);
                }
            }
        }

        private CanvasGroup GetPanel(MenuState state)
        {
            return state switch
            {
                MenuState.MainMenu => mainMenuPanel,
                MenuState.PauseMenu => pauseMenuPanel,
                MenuState.Settings => settingsPanel,
                MenuState.SaveMenu => saveMenuPanel,
                MenuState.LoadMenu => loadMenuPanel,
                MenuState.ConfirmQuit => confirmQuitPanel,
                MenuState.Credits => creditsPanel,
                _ => null
            };
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            if (_currentState != MenuState.None)
            {
                CanvasGroup panel = GetPanel(_currentState);
                if (panel != null)
                    yield return StartCoroutine(FadeCanvasGroup(panel, 1f, 0f, fadeDuration));
            }
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
                yield return null;
        }

        private IEnumerator LoadSceneByIndexRoutine(int index)
        {
            if (_currentState != MenuState.None)
            {
                CanvasGroup panel = GetPanel(_currentState);
                if (panel != null)
                    yield return StartCoroutine(FadeCanvasGroup(panel, 1f, 0f, fadeDuration));
            }
            AsyncOperation op = SceneManager.LoadSceneAsync(index);
            while (!op.isDone)
                yield return null;
        }
    }
}
