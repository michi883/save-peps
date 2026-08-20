using System;
using System.Collections;
using SavePeps.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.Rescue
{
    /// <summary>The four states a rescue slot can show in the in-scene HUD.</summary>
    public enum DotState
    {
        Upcoming = 0,
        Current = 1,
        Check = 2,
        Star = 3,
    }

    /// <summary>
    /// The deliberately small layer above the diorama: where you are, what the
    /// Peps need, and a quip that briefly lands after a physical failure.
    /// It never congratulates over the characters; reunion owns success.
    ///
    /// Two rules keep it from drifting back into a notification bar. Nothing
    /// here holds still — the objective arrives with a bounce and then shrinks
    /// out of the way once it has been read, and the quip is sized to its own
    /// sentence so a four-word joke is a tag rather than a banner. And the
    /// only permanent text is the round number; which rescue is in play is
    /// already said by the mastery marks, so printing "RESCUE 2/3" beside them
    /// was the same fact twice in the language of a progress dialog.
    /// </summary>
    public sealed class RescueHud : MonoBehaviour
    {
        [Tooltip("Everything the HUD draws. Toggled off wholesale while the round card is up.")]
        [SerializeField] private GameObject _root;

        [Header("Top")]
        [SerializeField] private RectTransform _statusRect;
        [SerializeField] private Text _roundLabel;
        [SerializeField] private MasteryMarkGraphic[] _marks;

        [Header("Menu")]
        [SerializeField] private Button _menuButton;
        [SerializeField] private CanvasGroup _menuGroup;

        [Header("Scene")]
        [Tooltip("2-4 words: what the Peps need, never how.")]
        [SerializeField] private Text _goal;
        [SerializeField] private RectTransform _goalRect;
        [SerializeField] private CanvasGroup _goalGroup;

        [Tooltip("Seconds the objective stays at full size before shrinking out of the way.")]
        [SerializeField, Range(0.6f, 6f)] private float _goalHold = 2.1f;

        [Header("Failure beat")]
        [SerializeField] private GameObject _tray;
        [SerializeField] private CanvasGroup _trayGroup;
        [SerializeField] private RectTransform _trayRect;
        [SerializeField] private Text _quip;

        private const float GoalRestScale = 0.88f;
        // Quiet, not disabled. Below about 0.65 on a Pixel 4 the objective
        // stops reading as "still here if you need it" and starts reading as
        // a greyed-out control.
        private const float GoalRestAlpha = 0.74f;

        private Vector2 _trayFallback;
        private Vector2 _trayRest;
        private Coroutine _quipRoutine;
        private Coroutine _goalRoutine;
        private int _quipCount;

        public bool QuipVisible => _tray != null && _tray.activeSelf;

        /// <summary>Raised when the player asks for the pause sheet.</summary>
        public event Action OnMenuRequested;

        private void Awake()
        {
            if (_menuButton != null) _menuButton.onClick.AddListener(() => OnMenuRequested?.Invoke());
            if (_trayRect != null)
            {
                _trayFallback = _trayRect.anchoredPosition;
                _trayRest = _trayFallback;
            }
            ClearQuip();
        }

        public void SetRound(int round, int rescueIndex, int rescuesPerRound)
        {
            if (_roundLabel != null) _roundLabel.text = $"ROUND {round}";

            // Preview tooling does not have progression to call SetDots, so a
            // pristine row still needs one obvious "you are here" mark.
            var untouched = true;
            foreach (var mark in _marks ?? System.Array.Empty<MasteryMarkGraphic>())
            {
                if (mark != null && mark.State != MasteryMarkState.Empty) untouched = false;
            }

            if (!untouched) return;
            var states = new DotState[_marks?.Length ?? 0];
            for (var i = 0; i < states.Length; i++)
            {
                states[i] = i == rescueIndex ? DotState.Current : DotState.Upcoming;
            }
            SetDots(states);
        }

        /// <summary>Paints mastery and punches only a newly earned mark.</summary>
        public void SetDots(DotState[] states)
        {
            if (_marks == null || states == null) return;

            var count = Mathf.Min(_marks.Length, states.Length);
            for (var i = 0; i < count; i++)
            {
                var mark = _marks[i];
                if (mark == null) continue;

                var next = states[i] switch
                {
                    DotState.Star => MasteryMarkState.Star,
                    DotState.Check => MasteryMarkState.Check,
                    DotState.Current => MasteryMarkState.Current,
                    _ => MasteryMarkState.Empty,
                };
                var earnedNow = next switch
                {
                    MasteryMarkState.Star => mark.State != MasteryMarkState.Star,
                    MasteryMarkState.Check => mark.State == MasteryMarkState.Current,
                    _ => false,
                };
                mark.SetState(next, animate: earnedNow);

                // The plaque flinches with the mark it holds, so an earned
                // star reads as one event in the corner of the eye rather
                // than as a small icon quietly changing shape.
                if (earnedNow && isActiveAndEnabled && _statusRect != null)
                {
                    StartCoroutine(UIPop.Punch(_statusRect, 1.06f));
                }
            }
        }

        /// <summary>
        /// Announces the objective, then gets out of the way. A 2-4 word line
        /// is read once; leaving it at full contrast for the whole rescue is
        /// what made the top of the screen look like an app header.
        /// </summary>
        public void Show(string goal)
        {
            ClearQuip();
            if (_goal != null)
            {
                _goal.text = goal ?? string.Empty;
                if (_goalRect != null)
                {
                    var width = Mathf.Clamp(_goal.preferredWidth + 120f, 440f, 900f);
                    _goalRect.sizeDelta = new Vector2(width, _goalRect.sizeDelta.y);
                }
            }

            if (_goalRoutine != null) StopCoroutine(_goalRoutine);
            _goalRoutine = null;
            if (!isActiveAndEnabled || _goalRect == null) return;
            _goalRoutine = StartCoroutine(AnnounceGoal());
        }

        /// <summary>
        /// Whether the pause control accepts a tap. It is switched off while
        /// an outcome plays: a gag is under four seconds, and letting the
        /// player suspend the game halfway through one would mean freezing a
        /// running choreography rather than simply not starting a new one.
        /// </summary>
        public void SetMenuAvailable(bool available)
        {
            if (_menuButton != null && _menuButton.interactable != available)
            {
                _menuButton.interactable = available;
            }
            if (_menuGroup != null) _menuGroup.alpha = available ? 1f : 0.35f;
        }

        /// <summary>
        /// Lets the joke breathe for the retry beat without asking the player
        /// to dismiss a generic error modal.
        /// </summary>
        public void ShowQuip(string quip, Transform actionTarget)
        {
            if (_quipRoutine != null) StopCoroutine(_quipRoutine);
            _quipRoutine = null;

            PositionQuip(actionTarget);
            if (_quip != null)
            {
                _quip.text = (quip ?? string.Empty).Replace('\n', ' ').Replace('\r', ' ').Trim();

                // A tag cut to its own sentence reads as part of the gag; a
                // fixed-width plaque reads as the app talking to the player.
                var width = Mathf.Clamp(_quip.preferredWidth + 104f, 420f, 1000f);
                if (_trayRect != null) _trayRect.sizeDelta = new Vector2(width, _trayRect.sizeDelta.y);
                _quip.rectTransform.sizeDelta = new Vector2(width - 44f, _quip.rectTransform.sizeDelta.y);
            }

            if (_tray == null) return;
            _tray.SetActive(true);
            if (_trayRect != null) _trayRect.anchoredPosition = _trayRest;

            // Alternating the tilt stops two quips in a row from landing in
            // exactly the same pose, which is what makes a repeat feel scripted.
            var tilt = (_quipCount++ % 2 == 0 ? 1f : -1f) * 4.5f;
            UIPop.Prepare(_trayRect, _trayGroup, 0.68f, tilt);
            _quipRoutine = StartCoroutine(UIPop.In(_trayRect, _trayGroup, 0.20f, 0.68f, tilt));
        }

        /// <summary>
        /// Keeps the reaction visually attached to the thing that just moved,
        /// while clamping it clear of the top HUD and the phone's bottom edge.
        /// Canvas scaling is height-matched, so these values remain Pixel 4
        /// reference pixels on every supported portrait aspect.
        /// </summary>
        private void PositionQuip(Transform actionTarget)
        {
            _trayRest = _trayFallback;
            if (_trayRect == null || Camera.main == null || actionTarget == null) return;
            if (_trayRect.parent is not RectTransform parent) return;

            var camera = Camera.main;
            var minY = float.PositiveInfinity;
            var maxY = float.NegativeInfinity;
            foreach (var renderer in actionTarget.GetComponentsInChildren<Renderer>(includeInactive: true))
            {
                var bounds = renderer.bounds;
                for (var x = -1; x <= 1; x += 2)
                for (var y = -1; y <= 1; y += 2)
                for (var z = -1; z <= 1; z += 2)
                {
                    var corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
                    var screen = camera.WorldToScreenPoint(corner);
                    if (screen.z <= 0f) continue;
                    var canvasY = screen.y * parent.rect.height / Screen.height;
                    minY = Mathf.Min(minY, canvasY);
                    maxY = Mathf.Max(maxY, canvasY);
                }
            }

            if (float.IsInfinity(minY))
            {
                var screen = camera.WorldToScreenPoint(actionTarget.position);
                if (screen.z <= 0f) return;
                minY = maxY = screen.y * parent.rect.height / Screen.height;
            }

            const float gap = 36f;
            var halfHeight = _trayRect.rect.height * 0.5f;
            var actionMidpoint = (minY + maxY) * 0.5f;
            var desiredY = actionMidpoint < parent.rect.height * 0.5f
                ? minY - halfHeight - gap
                : maxY + halfHeight + gap;
            _trayRest = new Vector2(0f, Mathf.Clamp(desiredY, 390f, 1840f));
        }

        public void HideQuip()
        {
            if (!QuipVisible) return;
            if (_quipRoutine != null) StopCoroutine(_quipRoutine);
            _quipRoutine = StartCoroutine(HideQuipRoutine());
        }

        public void ClearQuip()
        {
            if (_quipRoutine != null) StopCoroutine(_quipRoutine);
            _quipRoutine = null;
            if (_trayGroup != null) _trayGroup.alpha = 0f;
            if (_trayRect != null)
            {
                _trayRect.anchoredPosition = _trayRest;
                _trayRect.localScale = Vector3.one;
                _trayRect.localRotation = Quaternion.identity;
            }
            if (_tray != null) _tray.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            if (_root != null && _root.activeSelf != visible) _root.SetActive(visible);
        }

        private IEnumerator AnnounceGoal()
        {
            yield return UIPop.In(_goalRect, _goalGroup, 0.30f, 0.60f, -5f);
            yield return new WaitForSecondsRealtime(_goalHold);
            yield return UIPop.Settle(_goalRect, _goalGroup, GoalRestScale, GoalRestAlpha, 0.40f);
            _goalRoutine = null;
        }

        /// <summary>
        /// The quip leaves by shrinking upward, as if the thought popped —
        /// the same shape as its arrival, played backwards and faster, so the
        /// retry starts the instant the eye has finished with it.
        /// </summary>
        private IEnumerator HideQuipRoutine()
        {
            var startScale = _trayRect != null ? _trayRect.localScale.x : 1f;
            var startPosition = _trayRect != null ? _trayRect.anchoredPosition : Vector2.zero;

            var elapsed = 0f;
            const float duration = 0.15f;
            while (elapsed < duration && _tray != null)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.In, Mathf.Clamp01(elapsed / duration));
                if (_trayGroup != null) _trayGroup.alpha = 1f - t;
                if (_trayRect != null)
                {
                    _trayRect.localScale = Vector3.one * Mathf.Lerp(startScale, 0.74f, t);
                    _trayRect.anchoredPosition = startPosition + Vector2.up * (26f * t);
                }
                yield return null;
            }

            _quipRoutine = null;
            ClearQuip();
        }
    }
}
