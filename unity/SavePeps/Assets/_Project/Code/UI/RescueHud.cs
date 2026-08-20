using System;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.Rescue
{
    /// <summary>
    /// What one of the three round dots is showing.
    ///
    /// Star and Check are earned marks and outrank position: a dot the player
    /// has already starred keeps its star while they replay the round, rather
    /// than reverting to "you are here". The dot row is the only progress
    /// display in the game, so it has to be readable at a glance and never
    /// appear to take something away.
    /// </summary>
    public enum DotState
    {
        Upcoming = 0,
        Current = 1,
        Check = 2,
        Star = 3,
    }

    /// <summary>
    /// The whole HUD: a round label, three dots, an objective line, and a
    /// Try Again button that only exists after a wrong answer.
    ///
    /// P1's real question for this component is whether any of it competes
    /// with the scene. The scene is the game; if the eye goes to the chrome
    /// first, the chrome is wrong. Everything here is sized and placed to
    /// lose that competition on purpose.
    /// </summary>
    public sealed class RescueHud : MonoBehaviour
    {
        [Tooltip("Everything the HUD draws. Toggled off wholesale while the round card is up.")]
        [SerializeField] private GameObject _root;

        [Header("Top")]
        [SerializeField] private Text _roundLabel;
        [SerializeField] private Image[] _dots;

        [Header("Scene")]
        [Tooltip("2-4 words: what the Peps need, never how.")]
        [SerializeField] private Text _goal;

        [Header("Result")]
        [SerializeField] private GameObject _tray;
        [SerializeField] private Text _quip;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Text _resultStamp;

        private Action _onRetry;

        private void Awake()
        {
            if (_retryButton != null) _retryButton.onClick.AddListener(() => _onRetry?.Invoke());
            ClearQuip();
            if (_resultStamp != null) _resultStamp.gameObject.SetActive(false);
        }

        public void SetRound(int round, int rescueIndex, int rescuesPerRound)
        {
            if (_roundLabel != null)
            {
                _roundLabel.text = $"Round {round}  ·  Rescue {rescueIndex + 1} of {rescuesPerRound}";
            }

            // A sensible default for anything driving the HUD without
            // progression - the editor preview and the P1 slice both do.
            // GameFlow follows this with SetDots to layer earned marks on top.
            var states = new DotState[_dots?.Length ?? 0];
            for (var i = 0; i < states.Length; i++)
            {
                states[i] = i == rescueIndex ? DotState.Current : DotState.Upcoming;
            }

            SetDots(states);
        }

        /// <summary>Paints the three dots. Shorter arrays leave the rest untouched.</summary>
        public void SetDots(DotState[] states)
        {
            if (_dots == null || states == null) return;

            var count = Mathf.Min(_dots.Length, states.Length);
            for (var i = 0; i < count; i++)
            {
                if (_dots[i] == null) continue;
                _dots[i].color = states[i] switch
                {
                    DotState.Star    => new Color(1f, 0.71f, 0.24f),          // warm accent, solid
                    DotState.Check   => new Color(1f, 0.71f, 0.24f, 0.55f),   // earned, quieter
                    DotState.Current => new Color(1f, 1f, 1f, 0.9f),          // you are here
                    _                => new Color(1f, 1f, 1f, 0.35f),         // hollow
                };
            }
        }

        public void Show(string goal)
        {
            if (_goal != null) _goal.text = goal;

            // A staged rescue starts clean. Without this the previous
            // rescue's "Perfect!" hangs over the new scene until the first
            // tap — which on device read as the game congratulating you for
            // a puzzle you had not looked at yet.
            ClearQuip();
        }

        public void ShowQuip(string quip, Action onRetry)
        {
            _onRetry = onRetry;
            if (_quip != null) _quip.text = quip;
            if (_tray != null) _tray.SetActive(true);
        }

        public void ClearQuip()
        {
            if (_tray != null) _tray.SetActive(false);
            if (_resultStamp != null) _resultStamp.gameObject.SetActive(false);
        }

        /// <summary>
        /// Hides or shows the whole HUD. The round card covers the screen, and
        /// a half-visible "Rescue 3 of 3" behind it reads as a rendering bug
        /// rather than as depth.
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (_root != null && _root.activeSelf != visible) _root.SetActive(visible);
        }

        public void ShowResult(string text)
        {
            if (_resultStamp == null) return;
            _resultStamp.text = text;
            _resultStamp.gameObject.SetActive(true);
        }
    }
}
