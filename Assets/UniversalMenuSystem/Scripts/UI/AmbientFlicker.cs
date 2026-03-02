// ============================================================================
// Universal Menu System - Ambient Flicker Effect
// Compatible with Unity 6000.0+
// ============================================================================
// Subtle ambient animation for UI elements — random opacity flicker,
// slow drift, or pulse. Adds life to static horror menus.
// ============================================================================

using UnityEngine;

namespace UniversalMenuSystem.UI
{
    public class AmbientFlicker : MonoBehaviour
    {
        public enum FlickerMode
        {
            AlphaPulse,      // smooth sine-wave alpha
            RandomFlicker,   // random alpha snaps (like a dying lightbulb)
            PositionDrift,   // slow random position offset
            ScalePulse       // subtle scale breathing
        }

        [Header("Mode")]
        [SerializeField] private FlickerMode mode = FlickerMode.AlphaPulse;

        [Header("Alpha Pulse")]
        [SerializeField] private float pulseSpeed = 1.5f;
        [SerializeField] private float pulseMin = 0.7f;
        [SerializeField] private float pulseMax = 1f;

        [Header("Random Flicker")]
        [SerializeField] private float flickerInterval = 0.1f;
        [SerializeField] private float flickerMinAlpha = 0.4f;
        [SerializeField] private float flickerMaxAlpha = 1f;
        [SerializeField] private float flickerReturnSpeed = 15f;

        [Header("Position Drift")]
        [SerializeField] private float driftAmount = 2f;
        [SerializeField] private float driftSpeed = 0.5f;

        [Header("Scale Pulse")]
        [SerializeField] private float scaleSpeed = 2f;
        [SerializeField] private float scaleMin = 0.98f;
        [SerializeField] private float scaleMax = 1.02f;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector2 _originalPos;
        private Vector3 _originalScale;
        private float _flickerTimer;
        private float _flickerTarget;
        private float _noiseOffsetX;
        private float _noiseOffsetY;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();

            if (_rectTransform != null)
            {
                _originalPos = _rectTransform.anchoredPosition;
                _originalScale = _rectTransform.localScale;
            }

            _noiseOffsetX = Random.Range(0f, 100f);
            _noiseOffsetY = Random.Range(0f, 100f);
            _flickerTarget = flickerMaxAlpha;
        }

        private void Update()
        {
            float t = Time.unscaledTime;

            switch (mode)
            {
                case FlickerMode.AlphaPulse:
                    if (_canvasGroup != null)
                    {
                        float pulse = Mathf.Lerp(pulseMin, pulseMax,
                            (Mathf.Sin(t * pulseSpeed) + 1f) * 0.5f);
                        _canvasGroup.alpha = pulse;
                    }
                    break;

                case FlickerMode.RandomFlicker:
                    if (_canvasGroup != null)
                    {
                        _flickerTimer -= Time.unscaledDeltaTime;
                        if (_flickerTimer <= 0f)
                        {
                            _flickerTarget = Random.Range(flickerMinAlpha, flickerMaxAlpha);
                            _flickerTimer = flickerInterval + Random.Range(0f, flickerInterval);
                        }
                        _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, _flickerTarget,
                            Time.unscaledDeltaTime * flickerReturnSpeed);
                    }
                    break;

                case FlickerMode.PositionDrift:
                    if (_rectTransform != null)
                    {
                        float dx = (Mathf.PerlinNoise(t * driftSpeed + _noiseOffsetX, 0f) - 0.5f) * 2f * driftAmount;
                        float dy = (Mathf.PerlinNoise(0f, t * driftSpeed + _noiseOffsetY) - 0.5f) * 2f * driftAmount;
                        _rectTransform.anchoredPosition = _originalPos + new Vector2(dx, dy);
                    }
                    break;

                case FlickerMode.ScalePulse:
                    if (_rectTransform != null)
                    {
                        float s = Mathf.Lerp(scaleMin, scaleMax,
                            (Mathf.Sin(t * scaleSpeed) + 1f) * 0.5f);
                        _rectTransform.localScale = _originalScale * s;
                    }
                    break;
            }
        }

        private void OnDisable()
        {
            // Reset to original state
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            if (_rectTransform != null)
            {
                _rectTransform.anchoredPosition = _originalPos;
                _rectTransform.localScale = _originalScale;
            }
        }
    }
}
