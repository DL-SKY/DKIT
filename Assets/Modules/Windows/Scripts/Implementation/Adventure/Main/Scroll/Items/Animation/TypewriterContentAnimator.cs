using System;
using TMPro;
using UnityEngine;

namespace Modules.Windows.Scripts.Implementation.Adventure.Main.Scroll.Items.Animation
{
    /// <summary>
    /// Reveals <see cref="TextMeshProUGUI"/> text one character at a time via
    /// <see cref="TMP_Text.maxVisibleCharacters"/>.
    /// Uses <see cref="TMP_TextInfo.characterCount"/> after a mesh update so rich-text tags
    /// are excluded from the reveal budget while printable and control characters (e.g. '\n')
    /// are counted as TMP characters.
    /// Text must be assigned on the TMP component before <see cref="Play"/>.
    /// </summary>
    public sealed class TypewriterContentAnimator : MonoBehaviour, IContentAnimator
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private float _charsPerSecond = 40f;

        private bool _isPlaying;
        private bool _isCompleted;
        private int _totalCharacters;
        private float _visibleCharacters;

        public bool IsPlaying => _isPlaying;

        public event Action Completed;

        public void Play()
        {
            if (_isCompleted || _isPlaying)
                return;

            if (_text == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"[{nameof(TypewriterContentAnimator)}] TextMeshProUGUI is not assigned on '{name}'. Completing immediately.",
                    this);
                Complete();
                return;
            }

            _text.ForceMeshUpdate();
            _totalCharacters = _text.textInfo.characterCount;
            _visibleCharacters = 0f;
            _text.maxVisibleCharacters = 0;

            if (_totalCharacters <= 0 || _charsPerSecond <= 0f)
            {
                Complete();
                return;
            }

            _isPlaying = true;
        }

        public void Skip()
        {
            if (_isCompleted)
                return;

            Complete();
        }

        private void Update()
        {
            if (!_isPlaying)
                return;

            _visibleCharacters += _charsPerSecond * Time.deltaTime;
            int visible = Mathf.Min(_totalCharacters, Mathf.FloorToInt(_visibleCharacters));
            _text.maxVisibleCharacters = visible;

            if (visible >= _totalCharacters)
                Complete();
        }

        private void Complete()
        {
            _isPlaying = false;
            _isCompleted = true;

            if (_text != null)
            {
                // Ensure layout/count are current even if Play was skipped before a mesh update.
                _text.ForceMeshUpdate();
                _text.maxVisibleCharacters = int.MaxValue;
            }

            Completed?.Invoke();
        }
    }
}
