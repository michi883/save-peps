using System;
using SavePeps.Rescue;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.Progression
{
    /// <summary>
    /// The one interstitial in the game: three dots resolving into their
    /// marks, "Round 4 complete", Keep playing, and a quiet round-picker link.
    ///
    /// PLAN §8 rules out a level select and a round map, which leaves this
    /// card carrying the whole of progression feedback. It stays deliberately
    /// small — the reason to keep playing is the next diorama dropping in, not
    /// a summary screen, so this should feel like a beat rather than a
    /// destination.
    /// </summary>
    public sealed class RoundCompleteCard : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _title;
        [SerializeField] private Text _subtitle;
        [SerializeField] private Image[] _dots;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Text _continueLabel;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Text _replayLabel;

        private Action _onContinue;
        private Action _onReplay;

        private void Awake()
        {
            if (_continueButton != null) _continueButton.onClick.AddListener(() => _onContinue?.Invoke());
            if (_replayButton != null) _replayButton.onClick.AddListener(() => _onReplay?.Invoke());
            Hide();
        }

        public void Show(int roundNumber, Mark[] marks, Action onKeepPlaying, Action onChooseRound)
        {
            _onContinue = onKeepPlaying;
            _onReplay = onChooseRound;

            if (_title != null) _title.text = $"Round {roundNumber} complete";

            var perfect = 0;
            foreach (var mark in marks ?? Array.Empty<Mark>())
            {
                if (mark == Mark.Star) perfect++;
            }

            if (_subtitle != null)
            {
                // Never a score and never a rebuke. Three of three is worth
                // celebrating; anything less is just stated, because the brief
                // is explicit that missing a first tap costs nothing.
                _subtitle.text = perfect == RoundDefinition.RescuesPerRound
                    ? "All three, first try."
                    : $"{perfect} of {RoundDefinition.RescuesPerRound} first try.";
            }

            foreach (var dot in _dots ?? System.Array.Empty<Image>())
            {
                if (dot != null) dot.gameObject.SetActive(true);
            }

            PaintDots(marks);

            SetActions("Keep playing", "Choose round");
            SetVisible(true);
        }

        /// <summary>
        /// The player has finished everything authored. This is a real state
        /// during development and will be a real state for a subscriber who
        /// catches up with the content, so it gets an honest screen rather
        /// than a silent dead end.
        /// </summary>
        public void ShowOutOfContent(Action onKeepPlaying, Action onChooseRound)
        {
            _onContinue = onKeepPlaying;
            _onReplay = onChooseRound;

            if (_title != null) _title.text = "That is everything, for now.";
            if (_subtitle != null) _subtitle.text = "New rounds are on the way.";

            // No dots here: this card is not about a round, and three empty
            // ones just read as three things the player failed to earn.
            foreach (var dot in _dots ?? System.Array.Empty<Image>())
            {
                if (dot != null) dot.gameObject.SetActive(false);
            }

            // Both ways out stay live. This screen used to hide its buttons,
            // which left the player with nothing to tap and no way back into
            // the game short of killing the app — a dead end reachable by
            // anybody who finishes the last authored round.
            SetActions("Keep playing", "Choose round");
            SetVisible(true);
        }

        /// <summary>Labels and re-enables both buttons.</summary>
        private void SetActions(string continueText, string replayText)
        {
            if (_continueLabel != null) _continueLabel.text = continueText;
            if (_replayLabel != null) _replayLabel.text = replayText;
            if (_continueButton != null) _continueButton.gameObject.SetActive(true);
            if (_replayButton != null) _replayButton.gameObject.SetActive(true);
        }

        public void Hide() => SetVisible(false);

        private void SetVisible(bool visible)
        {
            var target = _root != null ? _root : gameObject;
            if (target.activeSelf != visible) target.SetActive(visible);
        }

        /// <summary>
        /// Marks are drawn as coloured dots rather than star and tick glyphs:
        /// the built-in font has no dependable coverage for those codepoints,
        /// and a missing-glyph box on the celebration screen is a worse bug
        /// than a plainer symbol. Size carries the difference so the row still
        /// reads at a glance.
        /// </summary>
        private void PaintDots(Mark[] marks)
        {
            if (_dots == null) return;

            for (var i = 0; i < _dots.Length; i++)
            {
                var dot = _dots[i];
                if (dot == null) continue;

                var mark = marks != null && i < marks.Length ? marks[i] : Mark.None;
                dot.color = mark switch
                {
                    Mark.Star  => new Color(1f, 0.71f, 0.24f),
                    Mark.Check => new Color(1f, 0.71f, 0.24f, 0.55f),
                    _          => new Color(1f, 1f, 1f, 0.35f),
                };

                var scale = mark == Mark.Star ? 1.35f : 1f;
                dot.rectTransform.localScale = Vector3.one * scale;
            }
        }
    }
}
