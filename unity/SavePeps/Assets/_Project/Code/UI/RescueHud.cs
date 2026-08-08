using System;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.Rescue
{
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

            for (var i = 0; i < (_dots?.Length ?? 0); i++)
            {
                // Hollow for upcoming, filled for the one being played.
                _dots[i].color = i == rescueIndex
                    ? new Color(1f, 0.71f, 0.24f)      // warm accent
                    : new Color(1f, 1f, 1f, 0.35f);
            }
        }

        public void Show(string goal)
        {
            if (_goal != null) _goal.text = goal;
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

        public void ShowResult(string text)
        {
            if (_resultStamp == null) return;
            _resultStamp.text = text;
            _resultStamp.gameObject.SetActive(true);
        }
    }
}
