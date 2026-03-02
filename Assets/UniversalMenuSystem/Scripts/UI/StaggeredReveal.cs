// ============================================================================
// Universal Menu System - Staggered Menu Reveal
// Compatible with Unity 6000.0+
// ============================================================================
// Animates child elements (menu items) in sequence with a stagger delay.
// Classic horror menu feel where options appear one by one.
// ============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniversalMenuSystem.UI
{
    public class StaggeredReveal : MonoBehaviour
    {
        [Header("Stagger Settings")]
        [SerializeField] private float initialDelay = 0.2f;
        [SerializeField] private float staggerDelay = 0.1f;             // delay between each child
        [SerializeField] private float itemFadeDuration = 0.35f;

        [Header("Item Animation")]
        [SerializeField] private Vector2 slideFromOffset = new Vector2(-40f, 0f); // slide from left
        [SerializeField] private AnimationCurve itemCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Options")]
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool hideOnDisable = true;

        private readonly List<RectTransform> _children = new List<RectTransform>();
        private readonly List<CanvasGroup> _childGroups = new List<CanvasGroup>();
        private readonly List<Vector2> _childOriginalPositions = new List<Vector2>();
        private Coroutine _revealCoroutine;

        private void Awake()
        {
            CacheChildren();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            if (hideOnDisable)
                HideAllImmediate();
        }

        /// <summary>
        /// Cache all direct children that we'll animate.
        /// </summary>
        private void CacheChildren()
        {
            _children.Clear();
            _childGroups.Clear();
            _childOriginalPositions.Clear();

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                RectTransform rt = child.GetComponent<RectTransform>();
                if (rt == null) continue;

                // Ensure each child has a CanvasGroup
                CanvasGroup cg = child.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = child.gameObject.AddComponent<CanvasGroup>();

                _children.Add(rt);
                _childGroups.Add(cg);
                _childOriginalPositions.Add(rt.anchoredPosition);
            }
        }

        /// <summary>
        /// Play the staggered reveal animation.
        /// </summary>
        public void Play()
        {
            if (_children.Count == 0) CacheChildren();
            Stop();
            _revealCoroutine = StartCoroutine(RevealRoutine());
        }

        public void Stop()
        {
            if (_revealCoroutine != null)
            {
                StopCoroutine(_revealCoroutine);
                _revealCoroutine = null;
            }
        }

        public void HideAllImmediate()
        {
            for (int i = 0; i < _childGroups.Count; i++)
            {
                _childGroups[i].alpha = 0f;
                _childGroups[i].interactable = false;
            }
        }

        public void ShowAllImmediate()
        {
            for (int i = 0; i < _childGroups.Count; i++)
            {
                _childGroups[i].alpha = 1f;
                _childGroups[i].interactable = true;
                _children[i].anchoredPosition = _childOriginalPositions[i];
            }
        }

        private IEnumerator RevealRoutine()
        {
            // Start all hidden
            HideAllImmediate();

            yield return new WaitForSecondsRealtime(initialDelay);

            for (int i = 0; i < _children.Count; i++)
            {
                StartCoroutine(RevealItem(i));
                yield return new WaitForSecondsRealtime(staggerDelay);
            }
        }

        private IEnumerator RevealItem(int index)
        {
            RectTransform rt = _children[index];
            CanvasGroup cg = _childGroups[index];
            Vector2 originalPos = _childOriginalPositions[index];
            Vector2 startPos = originalPos + slideFromOffset;

            float elapsed = 0f;
            while (elapsed < itemFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = itemCurve.Evaluate(Mathf.Clamp01(elapsed / itemFadeDuration));

                cg.alpha = t;
                rt.anchoredPosition = Vector2.Lerp(startPos, originalPos, t);

                yield return null;
            }

            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
            rt.anchoredPosition = originalPos;
        }
    }
}
