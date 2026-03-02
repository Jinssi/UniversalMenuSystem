// ============================================================================
// Universal Menu System - Credits Controller
// Compatible with Unity 6000.0+
// ============================================================================
// Scrolling credits with a horror/atmospheric twist.
// ============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniversalMenuSystem.Core;

namespace UniversalMenuSystem.Menus
{
    public class CreditsController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentRect;
        [SerializeField] private TextMeshProUGUI creditsText;
        [SerializeField] private Button backButton;
        [SerializeField] private Button skipButton;

        [Header("Scroll Settings")]
        [SerializeField] private float autoScrollSpeed = 30f;
        [SerializeField] private float initialDelay = 1.5f;
        [SerializeField] private bool autoScroll = true;

        [Header("Credits Content")]
        [TextArea(20, 100)]
        [SerializeField] private string creditsContent = @"
YOUR GAME TITLE
\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500

A game by
YOUR NAME / STUDIO

Programming
Developer Name

Art & Design
Artist Name

Sound Design
Sound Designer Name

Music
Composer Name

Special Thanks
Person 1
Person 2

\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500

Built with Unity

Universal Menu System
by [Your Name]

\u00a9 2026 All Rights Reserved
";

        private Coroutine _scrollCoroutine;
        private bool _isScrolling;

        private void OnEnable()
        {
            if (creditsText != null)
                creditsText.text = creditsContent;
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 1f;

            backButton?.onClick.AddListener(OnBack);
            skipButton?.onClick.AddListener(OnSkip);

            if (autoScroll)
                _scrollCoroutine = StartCoroutine(AutoScrollRoutine());
        }

        private void OnDisable()
        {
            backButton?.onClick.RemoveListener(OnBack);
            skipButton?.onClick.RemoveListener(OnSkip);
            if (_scrollCoroutine != null)
            {
                StopCoroutine(_scrollCoroutine);
                _scrollCoroutine = null;
            }
            _isScrolling = false;
        }

        private IEnumerator AutoScrollRoutine()
        {
            yield return new WaitForSecondsRealtime(initialDelay);
            _isScrolling = true;

            while (_isScrolling && scrollRect != null)
            {
                float contentHeight = contentRect != null ? contentRect.rect.height : 1f;
                float viewportHeight = scrollRect.viewport != null
                    ? scrollRect.viewport.rect.height
                    : scrollRect.GetComponent<RectTransform>().rect.height;

                float scrollableHeight = contentHeight - viewportHeight;
                if (scrollableHeight <= 0f)
                {
                    _isScrolling = false;
                    yield break;
                }

                float normalizedStep = (autoScrollSpeed * Time.unscaledDeltaTime) / scrollableHeight;
                scrollRect.verticalNormalizedPosition -= normalizedStep;

                if (scrollRect.verticalNormalizedPosition <= 0f)
                {
                    scrollRect.verticalNormalizedPosition = 0f;
                    _isScrolling = false;
                    yield break;
                }

                yield return null;
            }
        }

        private void OnSkip()
        {
            if (_scrollCoroutine != null)
            {
                StopCoroutine(_scrollCoroutine);
                _scrollCoroutine = null;
            }
            _isScrolling = false;
            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f;
        }

        private void OnBack() => MenuManager.Instance?.GoBack();
    }
}
