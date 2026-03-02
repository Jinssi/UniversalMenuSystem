// ============================================================================
// Universal Menu System - Typewriter Effect
// Compatible with Unity 6000.0+
// ============================================================================
// Types out text character-by-character for a horror atmosphere.
// Great for title text, subtitles, or dramatic reveals.
// ============================================================================

using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace UniversalMenuSystem.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TypewriterEffect : MonoBehaviour
    {
        [Header("Typewriter Settings")]
        [SerializeField] private float charactersPerSecond = 25f;
        [SerializeField] private float punctuationDelay = 0.15f;    // extra delay on . , ! ?
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool cursorBlink = true;
        [SerializeField] private string cursorChar = "_";
        [SerializeField] private float cursorBlinkRate = 0.5f;

        [Header("Horror Variation")]
        [SerializeField] private bool randomizeSpeed = true;        // slight speed jitter
        [SerializeField] private float speedJitter = 0.3f;          // ± percentage of base speed
        [SerializeField] private bool glitchOnReveal = false;       // brief character scramble
        [SerializeField] private string glitchChars = "█▓▒░╳╱╲";

        private TextMeshProUGUI _text;
        private string _fullText;
        private Coroutine _typeCoroutine;
        private Coroutine _cursorCoroutine;

        public event Action OnTypewriterComplete;
        public bool IsTyping { get; private set; }

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (playOnEnable && !string.IsNullOrEmpty(_text.text))
            {
                Play(_text.text);
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        /// <summary>
        /// Start typing the given text from scratch.
        /// </summary>
        public void Play(string text)
        {
            Stop();
            _fullText = text;
            _typeCoroutine = StartCoroutine(TypeRoutine());
        }

        /// <summary>
        /// Instantly reveal all text.
        /// </summary>
        public void Skip()
        {
            Stop();
            _text.text = _fullText;
            IsTyping = false;
            OnTypewriterComplete?.Invoke();
        }

        /// <summary>
        /// Stop typing without completing.
        /// </summary>
        public void Stop()
        {
            if (_typeCoroutine != null)
            {
                StopCoroutine(_typeCoroutine);
                _typeCoroutine = null;
            }
            if (_cursorCoroutine != null)
            {
                StopCoroutine(_cursorCoroutine);
                _cursorCoroutine = null;
            }
            IsTyping = false;
        }

        private IEnumerator TypeRoutine()
        {
            IsTyping = true;
            _text.text = "";

            for (int i = 0; i < _fullText.Length; i++)
            {
                char c = _fullText[i];

                if (glitchOnReveal && char.IsLetter(c))
                {
                    // Show a random glitch char briefly
                    char glitch = glitchChars[UnityEngine.Random.Range(0, glitchChars.Length)];
                    _text.text = _fullText.Substring(0, i) + glitch;
                    yield return new WaitForSecondsRealtime(0.02f);
                }

                _text.text = _fullText.Substring(0, i + 1);

                // Calculate delay
                float baseDelay = 1f / charactersPerSecond;

                if (randomizeSpeed)
                    baseDelay *= 1f + UnityEngine.Random.Range(-speedJitter, speedJitter);

                if (IsPunctuation(c))
                    baseDelay += punctuationDelay;

                yield return new WaitForSecondsRealtime(baseDelay);
            }

            IsTyping = false;

            // Start blinking cursor at end
            if (cursorBlink)
                _cursorCoroutine = StartCoroutine(CursorBlinkRoutine());

            OnTypewriterComplete?.Invoke();
        }

        private IEnumerator CursorBlinkRoutine()
        {
            while (true)
            {
                _text.text = _fullText + cursorChar;
                yield return new WaitForSecondsRealtime(cursorBlinkRate);
                _text.text = _fullText;
                yield return new WaitForSecondsRealtime(cursorBlinkRate);
            }
        }

        private bool IsPunctuation(char c)
        {
            return c == '.' || c == ',' || c == '!' || c == '?' || c == ';' || c == ':';
        }
    }
}
