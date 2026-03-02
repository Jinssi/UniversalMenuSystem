// ============================================================================
// Universal Menu System - Resolution Manager
// Compatible with Unity 6000.0+
// ============================================================================
// Manages dynamic resolution scaling for canvases. Ensures the UI stays
// crisp and centered regardless of display resolution or aspect ratio.
// ============================================================================

using UnityEngine;
using UnityEngine.UI;

namespace UniversalMenuSystem.Core
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public class ResolutionManager : MonoBehaviour
    {
        [Header("Reference Resolution")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

        [Header("Scaling")]
        [SerializeField, Range(0f, 1f)] private float matchWidthOrHeight = 0.5f;

        [Header("Safe Area")]
        [SerializeField] private bool applySafeArea = true;
        [SerializeField] private RectTransform safeAreaTarget;

        [Header("Camera Reference (for Screen Space - Camera)")]
        [SerializeField] private bool autoAssignCamera = true;

        private Canvas _canvas;
        private CanvasScaler _scaler;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _scaler = GetComponent<CanvasScaler>();
            ConfigureScaler();
            ConfigureCamera();
        }

        private void Start() => ApplySafeArea();

        private void Update()
        {
            Vector2Int currentSize = new Vector2Int(Screen.width, Screen.height);
            if (currentSize != _lastScreenSize || Screen.safeArea != _lastSafeArea)
            {
                ApplySafeArea();
                _lastScreenSize = currentSize;
                _lastSafeArea = Screen.safeArea;
            }
        }

        private void ConfigureScaler()
        {
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = referenceResolution;
            _scaler.matchWidthOrHeight = matchWidthOrHeight;
            _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        }

        private void ConfigureCamera()
        {
            if (!autoAssignCamera) return;
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera && _canvas.worldCamera == null)
                _canvas.worldCamera = Camera.main;
        }

        private void ApplySafeArea()
        {
            if (!applySafeArea || safeAreaTarget == null) return;
            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            safeAreaTarget.anchorMin = anchorMin;
            safeAreaTarget.anchorMax = anchorMax;
        }

        public void SetRenderMode(RenderMode mode, Camera cam = null)
        {
            _canvas.renderMode = mode;
            if (mode == RenderMode.ScreenSpaceCamera)
                _canvas.worldCamera = cam ?? Camera.main;
        }

        public void SetReferenceResolution(Vector2 resolution)
        {
            referenceResolution = resolution;
            _scaler.referenceResolution = resolution;
        }

        public void SetMatchWidthOrHeight(float match)
        {
            matchWidthOrHeight = Mathf.Clamp01(match);
            _scaler.matchWidthOrHeight = matchWidthOrHeight;
        }
    }
}
