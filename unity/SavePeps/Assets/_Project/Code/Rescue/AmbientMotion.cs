using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>What a piece of scenery does while nobody is touching it.</summary>
    public enum AmbientMode
    {
        /// <summary>Rocks about an axis. Kelp, banners, hanging cables, tarps.</summary>
        Sway = 0,
        /// <summary>Rises and falls in place. Boats on water, drifting debris.</summary>
        Bob = 1,
        /// <summary>Travels along an axis and wraps. Conveyor slats, rain, bubbles, stars.</summary>
        Drift = 2,
        /// <summary>Turns continuously. Gears, fans, station rings.</summary>
        Spin = 3,
        /// <summary>Breathes in scale. Glows, crystal veins, crucible light.</summary>
        Pulse = 4,
        /// <summary>Blinks on a pseudo-random schedule. Neon, sparks, lightning.</summary>
        Flicker = 5,
        /// <summary>Hammers on a beat: a fast strike, then a slow return. Pistons, presses.</summary>
        Beat = 6,
    }

    /// <summary>
    /// Continuous environmental motion — the difference between a world and a
    /// diagram of a world.
    ///
    /// This is deliberately separate from <see cref="AnimTarget"/> and never
    /// shares a transform with one. Choreography's whole reset guarantee rests
    /// on "rest pose is identity", and a looping idle that wrote to the same
    /// transform would make that false. Ambient motion instead runs on plain
    /// scenery, or on a child *below* a mover's <c>Choreo</c> node, exactly
    /// where <see cref="ChoicePresentation"/> puts a prop's idle bob.
    ///
    /// One component can drive its own transform or stagger its direct
    /// children by phase, so seven raindrops or twelve conveyor slats cost one
    /// component and one Update rather than twelve.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AmbientMotion : MonoBehaviour
    {
        [SerializeField] private AmbientMode _mode = AmbientMode.Sway;

        [Tooltip("Degrees for Sway/Spin, metres for Bob/Drift/Beat, scale delta for Pulse, " +
                 "fraction of each cycle spent off for Flicker.")]
        [SerializeField] private float _amplitude = 6f;

        [Tooltip("Cycles per second.")]
        [SerializeField] private float _speed = 0.6f;

        [Tooltip("Axis of rotation or direction of travel, in local space.")]
        [SerializeField] private Vector3 _axis = Vector3.forward;

        [Tooltip("Animate each direct child with its own phase instead of this transform.")]
        [SerializeField] private bool _staggerChildren;

        [Tooltip("Seconds added to this instance's clock, so two of the same thing do not move in lockstep.")]
        [SerializeField] private float _phase;

        [Tooltip("Optional choreography id. Defaults to this GameObject's name.")]
        [SerializeField] private string _controlId;

        private Transform[] _movers;
        private Vector3[] _restPositions;
        private Quaternion[] _restRotations;
        private Vector3[] _restScales;
        private Renderer[][] _renderers;
        private float _clock;
        private float _activity = 1f;
        private float _activityFrom = 1f;
        private float _activityTo = 1f;
        private float _activityClock;
        private float _activityDuration;

        public string ControlId => string.IsNullOrWhiteSpace(_controlId) ? name : _controlId;

        /// <summary>Authoring entry point for the generated dioramas.</summary>
        public AmbientMotion Configure(AmbientMode mode, float amplitude, float speed, Vector3 axis,
            bool staggerChildren = false, float phase = 0f, string controlId = null)
        {
            _mode = mode;
            _amplitude = amplitude;
            _speed = speed;
            _axis = axis == Vector3.zero ? Vector3.forward : axis;
            _staggerChildren = staggerChildren;
            _phase = phase;
            _controlId = controlId;
            return this;
        }

        /// <summary>
        /// Blends a loop between fully active and still. Oscillating modes
        /// settle back to their authored rest pose; travelling modes slow to
        /// a stop where they are. Retry restores activity and phase exactly.
        /// </summary>
        public void SetActivity(float activity, float duration)
        {
            Capture();
            _activityFrom = _activity;
            _activityTo = Mathf.Clamp01(activity);
            _activityClock = 0f;
            _activityDuration = Mathf.Max(0f, duration);

            if (_activityDuration <= 0.001f)
            {
                _activity = _activityTo;
                ApplyCurrent();
            }
        }

        /// <summary>Restores the authored loop after retry or rescue restart.</summary>
        public void ResetControl()
        {
            Capture();
            _clock = _phase;
            _activity = 1f;
            _activityFrom = 1f;
            _activityTo = 1f;
            _activityClock = 0f;
            _activityDuration = 0f;
            ApplyCurrent();
        }

        private void Awake() => Capture();

        private void Capture()
        {
            if (_movers != null) return;

            if (_staggerChildren)
            {
                _movers = new Transform[transform.childCount];
                for (var i = 0; i < _movers.Length; i++) _movers[i] = transform.GetChild(i);
            }
            else
            {
                _movers = new[] { transform };
            }

            _restPositions = new Vector3[_movers.Length];
            _restRotations = new Quaternion[_movers.Length];
            _restScales = new Vector3[_movers.Length];
            _renderers = new Renderer[_movers.Length][];

            for (var i = 0; i < _movers.Length; i++)
            {
                _restPositions[i] = _movers[i].localPosition;
                _restRotations[i] = _movers[i].localRotation;
                _restScales[i] = _movers[i].localScale;
                _renderers[i] = _mode == AmbientMode.Flicker
                    ? _movers[i].GetComponentsInChildren<Renderer>(includeInactive: true)
                    : System.Array.Empty<Renderer>();
            }

            // A stagger component with no children would otherwise animate
            // nothing forever; say so once rather than looking broken on device.
            if (_movers.Length == 0)
            {
                Debug.LogWarning($"[SavePeps] AmbientMotion on '{name}' has nothing to move.", this);
            }
        }

        private void OnEnable()
        {
            Capture();
            ResetControl();
        }

        private void Update()
        {
            if (_activityClock < _activityDuration)
            {
                _activityClock += Time.deltaTime;
                var t = Easing.Evaluate(EaseKind.InOut,
                    Mathf.Clamp01(_activityClock / Mathf.Max(0.001f, _activityDuration)));
                _activity = Mathf.Lerp(_activityFrom, _activityTo, t);
            }

            // Travelling loops decelerate rather than snapping to rest.
            _clock += Time.deltaTime * _activity;
            ApplyCurrent();
        }

        private void ApplyCurrent()
        {
            if (_movers == null) return;

            for (var i = 0; i < _movers.Length; i++)
            {
                var mover = _movers[i];
                if (mover == null) continue;

                // Even spacing along the cycle is what turns one component
                // into a conveyor, a rain curtain or a bubble column.
                var offset = _movers.Length > 1 ? i / (float)_movers.Length : 0f;
                Apply(i, mover, _clock * _speed + offset);
            }
        }

        private void Apply(int index, Transform mover, float cycles)
        {
            switch (_mode)
            {
                case AmbientMode.Sway:
                    mover.localRotation = _restRotations[index] *
                                          Quaternion.AngleAxis(
                                              Mathf.Sin(cycles * Mathf.PI * 2f) * _amplitude * _activity, _axis);
                    break;

                case AmbientMode.Bob:
                    mover.localPosition = _restPositions[index] +
                                          _axis.normalized *
                                          (Mathf.Sin(cycles * Mathf.PI * 2f) * _amplitude * _activity);
                    break;

                case AmbientMode.Drift:
                {
                    // Saw wave: travel one full amplitude, then restart. The
                    // pop back is invisible when the moving things are
                    // identical, which is the only place this is used.
                    var t = cycles - Mathf.Floor(cycles);
                    mover.localPosition = _restPositions[index] + _axis.normalized * (t * _amplitude);
                    break;
                }

                case AmbientMode.Spin:
                    mover.localRotation = _restRotations[index] *
                                          Quaternion.AngleAxis(cycles * _amplitude, _axis);
                    break;

                case AmbientMode.Pulse:
                {
                    var s = 1f + Mathf.Sin(cycles * Mathf.PI * 2f) * _amplitude * _activity;
                    mover.localScale = _restScales[index] * s;
                    break;
                }

                case AmbientMode.Flicker:
                {
                    // A cheap deterministic blink. Amplitude is deliberately
                    // the off fraction: 0.965 gives lightning a rare flash,
                    // while a lower value leaves a sign on for longer.
                    var t = cycles - Mathf.Floor(cycles);
                    var lit = t > Mathf.Clamp01(_amplitude);
                    var rows = _renderers[index];
                    for (var r = 0; r < rows.Length; r++)
                    {
                        if (rows[r] != null && rows[r].enabled != lit) rows[r].enabled = lit;
                    }
                    break;
                }

                case AmbientMode.Beat:
                {
                    // Down fast, up slow: a press, not a pendulum.
                    var t = cycles - Mathf.Floor(cycles);
                    var stroke = t < 0.22f ? t / 0.22f : 1f - (t - 0.22f) / 0.78f;
                    stroke = Easing.Evaluate(t < 0.22f ? EaseKind.In : EaseKind.Out, Mathf.Clamp01(stroke));
                    mover.localPosition = _restPositions[index] +
                                          _axis.normalized * (stroke * _amplitude * _activity);
                    break;
                }
            }
        }
    }
}
