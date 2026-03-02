// ============================================================================
// Universal Menu System - Text Hover Effect (Horror Style)
// Compatible with Unity 6000.0+
// ============================================================================
// Attach to any TextMeshProUGUI element that acts as a menu button.
// On hover: text shifts right, color desaturates/glows, optional flicker.
// Pure text-based — no boxes, no backgrounds. Resident Evil vibes.
// ============================================================================

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace UniversalMenuSystem.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextHoverEffect : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler
    {
        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.65f, 0.62f, 0.58f, 1f);   // muted parchment
        [SerializeField] private Color hoverColor = new Color(0.92f, 0.87f, 0.78f, 1f);    // warm glow
        [SerializeField] private Color pressedColor = new Color(1f, 0.95f, 0.85f, 1f);     // bright flash

        [Header("Animation")]
        [SerializeField] private float transitionSpeed = 8f;
        [SerializeField] private float hoverSlideOffset = 20f;       // pixels to slide right
        [SerializeField] private float hoverScaleMultiplier = 1.05f; // subtle scale up

        [Header("Horror Effects")]
        [SerializeField] private bool enableFlicker = true;
        [SerializeField] private float flickerChance = 0.03f;        // per-frame chance
        [SerializeField] private float flickerDuration = 0.05f;
        [SerializeField] private bool enableBreathing = true;        // subtle alpha pulse at rest
        [SerializeField] private float breathingSpeed = 1.2f;
        [SerializeField] private float breathingAmount = 0.08f;      // alpha oscillation range

        [Header("Selection Indicator")]
        [SerializeField] private string selectionPrefix = "►  ";     // prepended on hover
        [SerializeField] private bool useSelectionPrefix = true;

        // ── Private ────────────────────────────────────────────────────────
        private TextMeshProUGUI _text;
        private string _originalText;
        private Vector3 _originalPosition;
        private Vector3 _originalScale;
        private Color _targetColor;
        private bool _isHovered;
        private bool _isFlickering;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _originalText = _text.text;
            _originalPosition = transform.localPosition;
            _originalScale = transform.localScale;
            _targetColor = normalColor;
            _text.color = normalColor;
        }

        private void OnEnable()
        {
            // Reset state when re-enabled
            _isHovered = false;
            _targetColor = normalColor;
            _text.text = _originalText;
            transform.localPosition = _originalPosition;
            transform.localScale = _originalScale;
        }

        private void Update()
        {
            // Smooth color transition
            _text.color = Color.Lerp(_text.color, _targetColor, Time.unscaledDeltaTime * transitionSpeed);

            // Smooth position slide
            float targetX = _isHovered ? _originalPosition.x + hoverSlideOffset : _originalPosition.x;
            float currentX = Mathf.Lerp(transform.localPosition.x, targetX, Time.unscaledDeltaTime * transitionSpeed);
            transform.localPosition = new Vector3(currentX, transform.localPosition.y, transform.localPosition.z);

            // Smooth scale
            float targetScale = _isHovered ? hoverScaleMultiplier : 1f;
            float currentScale = Mathf.Lerp(transform.localScale.x, targetScale * _originalScale.x,
                                              Time.unscaledDeltaTime * transitionSpeed);
            transform.localScale = new Vector3(currentScale, currentScale, 1f);

            // Breathing effect when not hovered
            if (enableBreathing && !_isHovered)
            {
                float breath = 1f - breathingAmount * 0.5f + Mathf.Sin(Time.unscaledTime * breathingSpeed) * breathingAmount * 0.5f;
                Color c = _text.color;
                c.a = breath;
                _text.color = c;
            }

            // Random flicker
            if (enableFlicker && _isHovered && !_isFlickering)
            {
                if (Random.value < flickerChance)
                {
                    StartCoroutine(FlickerRoutine());
                }
            }
        }

        // ── Pointer Events ─────────────────────────────────────────────────

        public void OnPointerEnter(PointerEventData eventData) => SetHovered(true);
        public void OnPointerExit(PointerEventData eventData) => SetHovered(false);
        public void OnSelect(BaseEventData eventData) => SetHovered(true);
        public void OnDeselect(BaseEventData eventData) => SetHovered(false);

        private void SetHovered(bool hovered)
        {
            _isHovered = hovered;
            _targetColor = hovered ? hoverColor : normalColor;

            if (useSelectionPrefix)
            {
                _text.text = hovered ? selectionPrefix + _originalText : _originalText;
            }
        }

        /// <summary>
        /// Call from a Button's onClick or PointerDown to flash the pressed color.
        /// </summary>
        public void FlashPressed()
        {
            StartCoroutine(PressFlashRoutine());
        }

        // ── Coroutines ─────────────────────────────────────────────────────

        private IEnumerator FlickerRoutine()
        {
            _isFlickering = true;
            Color original = _text.color;

            // Momentary dip in alpha
            Color flickered = original;
            flickered.a *= 0.3f;
            _text.color = flickered;

            yield return new WaitForSecondsRealtime(flickerDuration);

            _text.color = original;
            _isFlickering = false;
        }

        private IEnumerator PressFlashRoutine()
        {
            _text.color = pressedColor;
            yield return new WaitForSecondsRealtime(0.1f);
            _targetColor = _isHovered ? hoverColor : normalColor;
        }

        /// <summary>
        /// Update the base text (use if you change text dynamically).
        /// </summary>
        public void SetText(string newText)
        {
            _originalText = newText;
            _text.text = _isHovered && useSelectionPrefix ? selectionPrefix + newText : newText;
        }
    }
}
