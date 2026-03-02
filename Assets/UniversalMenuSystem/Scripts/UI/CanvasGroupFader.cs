// ============================================================================
// Universal Menu System - Canvas Group Fader
// Compatible with Unity 6000.0+
// ============================================================================
// Utility component for fading individual CanvasGroups with horror-style
// effects. Can be used standalone or by MenuManager.
// ============================================================================

using System;
using System.Collections;
using UnityEngine;

namespace UniversalMenuSystem.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupFader : MonoBehaviour
    {
        [Header("Fade Settings")]
        [SerializeField] private float defaultFadeDuration = 0.5f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Horror Entrance (optional)")]
        [SerializeField] private bool slideOnFadeIn = false;
        [SerializeField] private Vector2 slideFromOffset = new Vector2(0f, -30f);
        [SerializeField] private bool scaleOnFadeIn = false;
        [SerializeField] private float scaleFrom = 0.95f;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector2 _originalAnchoredPos;
        private Vector3 _originalScale;
        private Coroutine _activeCoroutine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _originalAnchoredPos = _rectTransform.anchoredPosition;
            _originalScale = _rectTransform.localScale;
        }

        /// <summary>
        /// Fade in with optional slide/scale.
        /// </summary>
        public void FadeIn(float? duration = null, Action onComplete = null)
        {
            Stop();
            _activeCoroutine = StartCoroutine(FadeInRoutine(duration ?? defaultFadeDuration, onComplete));
        }

        /// <summary>
        /// Fade out.
        /// </summary>
        public void FadeOut(float? duration = null, Action onComplete = null)
        {
            Stop();
            _activeCoroutine = StartCoroutine(FadeOutRoutine(duration ?? defaultFadeDuration, onComplete));
        }

        /// <summary>
        /// Immediately show.
        /// </summary>
        public void ShowImmediate()
        {
            Stop();
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _rectTransform.anchoredPosition = _originalAnchoredPos;
            _rectTransform.localScale = _originalScale;
        }

        /// <summary>
        /// Immediately hide.
        /// </summary>
        public void HideImmediate()
        {
            Stop();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        private void Stop()
        {
            if (_activeCoroutine != null)
            {
                StopCoroutine(_activeCoroutine);
                _activeCoroutine = null;
            }
        }

        private IEnumerator FadeInRoutine(float duration, Action onComplete)
        {
            gameObject.SetActive(true);
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            Vector2 startPos = slideOnFadeIn ? _originalAnchoredPos + slideFromOffset : _originalAnchoredPos;
            Vector3 startScale = scaleOnFadeIn ? _originalScale * scaleFrom : _originalScale;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = fadeCurve.Evaluate(Mathf.Clamp01(elapsed / duration));

                _canvasGroup.alpha = t;

                if (slideOnFadeIn)
                    _rectTransform.anchoredPosition = Vector2.Lerp(startPos, _originalAnchoredPos, t);

                if (scaleOnFadeIn)
                    _rectTransform.localScale = Vector3.Lerp(startScale, _originalScale, t);

                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _rectTransform.anchoredPosition = _originalAnchoredPos;
            _rectTransform.localScale = _originalScale;

            onComplete?.Invoke();
        }

        private IEnumerator FadeOutRoutine(float duration, Action onComplete)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = fadeCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
                _canvasGroup.alpha = 1f - t;
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);

            onComplete?.Invoke();
        }
    }
}
