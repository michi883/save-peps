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
    /// The deliberately small layer above the diorama: location, objective,
    /// mastery marks, and a quip that briefly lands after a physical failure.
    /// It never congratulates over the characters; reunion owns success.
    /// </summary>
    public sealed class RescueHud : MonoBehaviour
    {
        [Tooltip("Everything the HUD draws. Toggled off wholesale while the round card is up.")]
        [SerializeField] private GameObject _root;

        [Header("Top")]
        [SerializeField] private Text _roundLabel;
        [SerializeField] private MasteryMarkGraphic[] _marks;

        [Header("Scene")]
        [Tooltip("2-4 words: what the Peps need, never how.")]
        [SerializeField] private Text _goal;

        [Header("Failure beat")]
        [SerializeField] private GameObject _tray;
        [SerializeField] private CanvasGroup _trayGroup;
        [SerializeField] private RectTransform _trayRect;
        [SerializeField] private Text _quip;

        private Vector2 _trayFallback;
        private Vector2 _trayRest;

        public bool QuipVisible => _tray != null && _tray.activeSelf;

        private void Awake()
        {
            if (_trayRect != null)
            {
                _trayFallback = _trayRect.anchoredPosition;
                _trayRest = _trayFallback;
            }
            ClearQuip();
        }

        public void SetRound(int round, int rescueIndex, int rescuesPerRound)
        {
            if (_roundLabel != null)
            {
                _roundLabel.text = $"ROUND {round}   •   RESCUE {rescueIndex + 1}/{rescuesPerRound}";
            }

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
            }
        }

        public void Show(string goal)
        {
            if (_goal != null) _goal.text = goal;
            ClearQuip();
        }

        /// <summary>
        /// Lets the joke breathe for the retry beat without asking the player
        /// to dismiss a generic error modal.
        /// </summary>
        public void ShowQuip(string quip, Transform actionTarget)
        {
            StopAllCoroutines();
            PositionQuip(actionTarget);
            if (_quip != null)
            {
                _quip.text = (quip ?? string.Empty).Replace('\n', ' ').Replace('\r', ' ').Trim();
            }
            if (_tray == null) return;

            _tray.SetActive(true);
            if (_trayGroup != null) _trayGroup.alpha = 0f;
            if (_trayRect != null) _trayRect.anchoredPosition = _trayRest + Vector2.down * 28f;
            StartCoroutine(RevealQuip());
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
            StopAllCoroutines();
            StartCoroutine(HideQuipRoutine());
        }

        public void ClearQuip()
        {
            StopAllCoroutines();
            if (_trayGroup != null) _trayGroup.alpha = 0f;
            if (_trayRect != null) _trayRect.anchoredPosition = _trayRest;
            if (_tray != null) _tray.SetActive(false);
        }

        public void SetVisible(bool visible)
        {
            if (_root != null && _root.activeSelf != visible) _root.SetActive(visible);
        }

        private IEnumerator RevealQuip()
        {
            var elapsed = 0f;
            const float duration = 0.16f;
            while (elapsed < duration && _tray != null)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.Out, Mathf.Clamp01(elapsed / duration));
                if (_trayGroup != null) _trayGroup.alpha = t;
                if (_trayRect != null) _trayRect.anchoredPosition = Vector2.LerpUnclamped(
                    _trayRest + Vector2.down * 28f, _trayRest, t);
                yield return null;
            }

            if (_trayGroup != null) _trayGroup.alpha = 1f;
            if (_trayRect != null) _trayRect.anchoredPosition = _trayRest;
        }

        private IEnumerator HideQuipRoutine()
        {
            var elapsed = 0f;
            const float duration = 0.16f;
            while (elapsed < duration && _tray != null)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.In, Mathf.Clamp01(elapsed / duration));
                if (_trayGroup != null) _trayGroup.alpha = 1f - t;
                if (_trayRect != null) _trayRect.anchoredPosition = Vector2.LerpUnclamped(
                    _trayRest, _trayRest + Vector2.down * 18f, t);
                yield return null;
            }

            if (_tray != null) _tray.SetActive(false);
        }
    }
}
