using System;
using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// A single touch-up raycast. No gestures, no drag, no camera control —
    /// the whole game is one tap, and the input layer should be that boring.
    /// </summary>
    public sealed class TapRouter : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        [Tooltip("Near-miss forgiveness, as a fraction of the shorter screen edge.")]
        [SerializeField, Range(0f, 0.3f)] private float _forgivenessPixels = 0.14f;

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

        /// <summary>
        /// Fires the tap path without touching the screen.
        ///
        /// Not test-only scaffolding: the editor preview and the Rescue
        /// Gauntlet (PLAN §5.3) both need to play an outcome on demand, and
        /// routing them through the same entry point as a real tap is what
        /// keeps the preview honest about what the player will get.
        /// </summary>
        public void SimulateTap(string objectId)
        {
            if (!InputEnabled || string.IsNullOrEmpty(objectId)) return;
            OnTap?.Invoke(objectId);
        }

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
        }

        private void Update()
        {
            if (!TryReadTapUp(out var screenPos)) return;
            if (!InputEnabled || _camera == null) return;

            var tappable = Pick(screenPos);
            if (tappable == null || string.IsNullOrEmpty(tappable.ObjectId)) return;

            Debug.Log($"[SavePeps] Tapped '{tappable.ObjectId}'.");
            OnTap?.Invoke(tappable.ObjectId);
        }

        /// <summary>
        /// Raycast first; if that misses, take the nearest tappable within
        /// <see cref="_forgivenessPixels"/> on screen. When generous hitboxes
        /// overlap, the visual whose centre is closest to the finger wins.
        ///
        /// The fallback is not a workaround, it is the feature: a thumb
        /// covers a large, imprecise area, and a player who clearly meant the
        /// plank should get the plank. There are only ever three candidates,
        /// so "nearest one they plausibly meant" is unambiguous.
        /// </summary>
        private Tappable Pick(Vector2 screenPos)
        {
            var ray = _camera.ScreenPointToRay(screenPos);
            var hitAChoice = false;
            foreach (var hit in Physics.RaycastAll(ray, 100f))
            {
                if (hit.collider.GetComponentInParent<Tappable>() != null)
                {
                    hitAChoice = true;
                    break;
                }
            }

            // Scaled off the shorter screen edge so forgiveness is consistent
            // across densities rather than being generous on small screens.
            // A direct choice hit removes the threshold, but still resolves
            // against all three visible centres. This is what makes an
            // overlapping front collider incapable of stealing the choice
            // the player's finger is visibly centred on.
            var threshold = hitAChoice
                ? float.PositiveInfinity
                : Mathf.Min(Screen.width, Screen.height) * _forgivenessPixels;
            var best = (Tappable)null;
            var bestDistance = threshold;

            foreach (var candidate in FindObjectsByType<Tappable>(FindObjectsSortMode.None))
            {
                var collider = candidate.GetComponent<Collider>();
                if (collider == null) continue;

                var centre = collider.bounds.center;
                var renderers = candidate.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    var visibleBounds = renderers[0].bounds;
                    for (var i = 1; i < renderers.Length; i++) visibleBounds.Encapsulate(renderers[i].bounds);
                    centre = visibleBounds.center;
                }

                var point = _camera.WorldToScreenPoint(centre);
                if (point.z <= 0f) continue;

                var distance = Vector2.Distance(screenPos, point);
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                best = candidate;
            }

            return best;
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
