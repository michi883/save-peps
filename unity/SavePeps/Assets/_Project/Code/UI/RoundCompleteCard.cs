using System;
using System.Collections;
using SavePeps.Core;
using SavePeps.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.Progression
{
    /// <summary>
    /// The round's one result beat. Earned ★/✓ marks arrive first; copy and
    /// actions support them rather than turning mastery into a score screen.
    /// </summary>
    public sealed class RoundCompleteCard : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Text _title;
        [SerializeField] private Text _subtitle;
        [SerializeField] private MasteryMarkGraphic[] _marks;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Text _continueLabel;
        [SerializeField] private Button _replayButton;
        [SerializeField] private Text _replayLabel;
        [SerializeField] private Feedback _feedback;

        private Action _onContinue;
        private Action _onReplay;

        public bool Visible => _root != null && _root.activeSelf;

        private void Awake()
        {
            if (_continueButton != null) _continueButton.onClick.AddListener(HandleContinue);
            if (_replayButton != null) _replayButton.onClick.AddListener(HandleReplay);
            Hide();
        }

        public void Show(int roundNumber, Mark[] marks, Action onKeepPlaying, Action onChooseRound)
        {
            _onContinue = onKeepPlaying;
            _onReplay = onChooseRound;

            if (_title != null) _title.text = $"ROUND {roundNumber} COMPLETE";

            var perfect = 0;
            foreach (var mark in marks ?? Array.Empty<Mark>())
            {
                if (mark == Mark.Star) perfect++;
            }

            if (_subtitle != null)
            {
                _subtitle.text = perfect == RoundDefinition.RescuesPerRound
                    ? "PERFECT ROUND"
                    : "THREE RESCUES SAVED";
                _subtitle.color = perfect == RoundDefinition.RescuesPerRound
                    ? new Color(0.92f, 0.55f, 0.12f)
                    : new Color(0.24f, 0.20f, 0.33f, 0.66f);
            }

            foreach (var mark in _marks ?? Array.Empty<MasteryMarkGraphic>())
            {
                if (mark != null) mark.gameObject.SetActive(true);
            }
            PaintMarks(marks);

            SetActions("Keep playing", "Choose round");
            Reveal();
        }

        public void ShowOutOfContent(Action onKeepPlaying, Action onChooseRound)
        {
            _onContinue = onKeepPlaying;
            _onReplay = onChooseRound;

            if (_title != null) _title.text = "ALL CAUGHT UP";
            if (_subtitle != null)
            {
                _subtitle.text = "NEW RESCUES ARE ON THE WAY";
                _subtitle.color = new Color(0.24f, 0.20f, 0.33f, 0.66f);
            }

            foreach (var mark in _marks ?? Array.Empty<MasteryMarkGraphic>())
            {
                if (mark != null) mark.gameObject.SetActive(false);
            }

            SetActions("Keep playing", "Choose round");
            Reveal();
        }

        public void Hide()
        {
            StopAllCoroutines();
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.interactable = false;
                _group.blocksRaycasts = false;
            }
            if (_panel != null)
            {
                _panel.localScale = Vector3.one;
                _panel.localRotation = Quaternion.identity;
            }
            SetVisible(false);
        }

        private void SetActions(string continueText, string replayText)
        {
            if (_continueLabel != null) _continueLabel.text = continueText;
            if (_replayLabel != null) _replayLabel.text = replayText;
            if (_continueButton != null)
            {
                _continueButton.gameObject.SetActive(true);
                _continueButton.interactable = false;
            }
            if (_replayButton != null)
            {
                _replayButton.gameObject.SetActive(true);
                _replayButton.interactable = false;
            }
        }

        private void PaintMarks(Mark[] marks)
        {
            for (var i = 0; i < (_marks?.Length ?? 0); i++)
            {
                var view = _marks[i];
                if (view == null) continue;

                var mark = marks != null && i < marks.Length ? marks[i] : Mark.None;
                view.SetState(mark switch
                {
                    Mark.Star => MasteryMarkState.Star,
                    Mark.Check => MasteryMarkState.Check,
                    _ => MasteryMarkState.Empty,
                });
            }
        }

        private void Reveal()
        {
            StopAllCoroutines();
            SetVisible(true);
            StartCoroutine(RevealRoutine());
        }

        private IEnumerator RevealRoutine()
        {
            // The card is a toy landing on the table, not a dialog opening:
            // it arrives slightly turned and overshoots before it settles.
            yield return UIPop.In(_panel, _group, 0.26f, 0.86f, -3f);

            foreach (var mark in _marks ?? Array.Empty<MasteryMarkGraphic>())
            {
                if (mark == null || !mark.gameObject.activeSelf) continue;
                mark.Punch(mark.State == MasteryMarkState.Star ? 1.30f : 1.18f);
                yield return new WaitForSecondsRealtime(0.09f);
            }

            if (_group != null)
            {
                _group.alpha = 1f;
                _group.interactable = true;
                _group.blocksRaycasts = true;
            }
            if (_continueButton != null) _continueButton.interactable = true;
            if (_replayButton != null) _replayButton.interactable = true;
        }

        private void HandleContinue()
        {
            if (_continueButton != null && !_continueButton.interactable) return;
            _feedback?.Tap();
            StartCoroutine(DismissRoutine(_onContinue));
        }

        private void HandleReplay()
        {
            if (_replayButton != null && !_replayButton.interactable) return;
            _feedback?.Tap();
            StartCoroutine(DismissRoutine(_onReplay));
        }

        private IEnumerator DismissRoutine(Action action)
        {
            if (_continueButton != null) _continueButton.interactable = false;
            if (_replayButton != null) _replayButton.interactable = false;
            if (_group != null) _group.interactable = false;

            yield return UIPop.Out(_panel, _group, 0.14f, 0.94f);

            SetVisible(false);
            action?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            var target = _root != null ? _root : gameObject;
            if (target.activeSelf != visible) target.SetActive(visible);
        }
    }
}
