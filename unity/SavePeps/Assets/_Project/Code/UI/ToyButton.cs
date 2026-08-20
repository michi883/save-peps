using System.Collections;
using SavePeps.Rescue;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SavePeps.UI
{
    /// <summary>
    /// Makes a uGUI button feel like something physical to press.
    ///
    /// uGUI's own transition is a colour tint, which on a toy-coloured button
    /// is nearly invisible on a phone and communicates nothing about touch.
    /// This squashes on press and springs back on release, and can breathe
    /// while idle so the primary action on a quiet screen still looks alive.
    /// It drives only its own <c>localScale</c>, so a parent panel's entrance
    /// animation and this can run at the same time without fighting.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class ToyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Tooltip("Scale while held. Below about 0.9 the label starts to read as a glitch.")]
        [SerializeField, Range(0.85f, 1f)] private float _pressedScale = 0.94f;

        [Tooltip("Amplitude of the idle breath. Zero for secondary buttons.")]
        [SerializeField, Range(0f, 0.06f)] private float _breathe;

        [SerializeField] private float _breatheSpeed = 1.9f;

        private RectTransform _rect;
        private Button _button;
        private Coroutine _spring;
        private bool _held;
        private float _clock;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _button = GetComponent<Button>();
            ClearTint();
        }

        private void OnEnable() => ClearTint();

        private void OnDisable()
        {
            _held = false;
            _spring = null;
            if (_rect != null) _rect.localScale = Vector3.one;
        }

        /// <summary>
        /// Turns off uGUI's colour transition, and — the part that is not
        /// obvious — undoes the tint it has already applied.
        ///
        /// A panel is activated with its CanvasGroup still non-interactable
        /// while the entrance animation runs. Selectable reaches OnEnable
        /// before this component's Awake, sees a disabled state, and multiplies
        /// the graphic by (0.78, 0.78, 0.78, 0.5) in the canvas renderer.
        /// Switching the transition off afterwards prevents further tinting but
        /// never reverts that one, so every button in the shell rendered washed
        /// out and half transparent — gold read as mud. The tint lives on the
        /// CanvasRenderer rather than the Graphic's colour, which is why it is
        /// invisible in the inspector and had to be found by sampling a
        /// screenshot.
        /// </summary>
        private void ClearTint()
        {
            if (_button == null) return;
            _button.transition = Selectable.Transition.None;
            if (_button.targetGraphic != null) _button.targetGraphic.canvasRenderer.SetColor(Color.white);
        }

        private void Update()
        {
            if (_breathe <= 0f || _held || _spring != null) return;
            if (_button != null && !_button.interactable)
            {
                _rect.localScale = Vector3.one;
                return;
            }

            _clock += Time.unscaledDeltaTime * _breatheSpeed;
            _rect.localScale = Vector3.one * (1f + Mathf.Sin(_clock) * _breathe);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            _held = true;
            if (_spring != null) StopCoroutine(_spring);
            _spring = null;
            _rect.localScale = Vector3.one * _pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData) => Release();

        public void OnPointerExit(PointerEventData eventData) => Release();

        private void Release()
        {
            if (!_held) return;
            _held = false;
            _clock = 0f;
            if (!isActiveAndEnabled)
            {
                _rect.localScale = Vector3.one;
                return;
            }

            if (_spring != null) StopCoroutine(_spring);
            _spring = StartCoroutine(SpringBack());
        }

        private IEnumerator SpringBack()
        {
            var from = _rect.localScale.x;
            var elapsed = 0f;
            const float duration = 0.20f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.Back, Mathf.Clamp01(elapsed / duration));
                _rect.localScale = Vector3.one * Mathf.LerpUnclamped(from, 1f, t);
                yield return null;
            }

            _rect.localScale = Vector3.one;
            _spring = null;
        }
    }
}
