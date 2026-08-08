using System;
using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// Marks a prop as tappable and carries its id back to the runner.
    ///
    /// The collider is deliberately separate from and larger than the visible
    /// mesh — Save Pip's tap circles ran about 25% wider than the art, and
    /// that generosity is most of why it felt good on a phone. A player
    /// aiming at a small plank should not have to be accurate.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class Tappable : MonoBehaviour
    {
        [Tooltip("Matches RescueObject.Id.")]
        public string ObjectId;

        /// <summary>The transform choreography drives for this prop.</summary>
        public AnimTarget Target { get; private set; }

        private void Awake() => Target = GetComponentInChildren<AnimTarget>();
    }

    /// <summary>
    /// A single touch-up raycast. No gestures, no drag, no camera control —
    /// the whole game is one tap, and the input layer should be that boring.
    /// </summary>
    public sealed class TapRouter : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        /// <summary>Raised with the tapped object's id.</summary>
        public event Action<string> OnTap;

        /// <summary>
        /// While an outcome plays, taps are ignored outright. This is the
        /// input race the brief calls out: without it, a fast double-tap
        /// starts a second outcome on top of the first and the scene ends up
        /// in a state no reset was designed for.
        /// </summary>
        public bool InputEnabled { get; set; } = true;

        private bool _pressed;

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
        }

        private void Update()
        {
            if (!TryReadTapUp(out var screenPos)) return;
            if (!InputEnabled || _camera == null) return;

            var ray = _camera.ScreenPointToRay(screenPos);
            if (!Physics.Raycast(ray, out var hit, 100f)) return;

            var tappable = hit.collider.GetComponentInParent<Tappable>();
            if (tappable != null && !string.IsNullOrEmpty(tappable.ObjectId))
            {
                OnTap?.Invoke(tappable.ObjectId);
            }
        }

        /// <summary>
        /// Fires on release rather than press, so a player can slide off an
        /// object they did not mean to choose. Touch is authoritative on
        /// device; the mouse path exists only so the scene is playable in the
        /// editor.
        /// </summary>
        private bool TryReadTapUp(out Vector2 screenPos)
        {
            screenPos = default;

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase != TouchPhase.Ended || touch.tapCount < 1) return false;
                screenPos = touch.position;
                return true;
            }

            if (Input.GetMouseButtonDown(0)) _pressed = true;
            if (!_pressed || !Input.GetMouseButtonUp(0)) return false;
            _pressed = false;
            screenPos = Input.mousePosition;
            return true;
        }
    }
}
