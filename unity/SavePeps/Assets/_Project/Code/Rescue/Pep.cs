using UnityEngine;

namespace SavePeps.Rescue
{
    public enum PepFace { Neutral = 0, Worried = 1, Hopeful = 2, Panic = 3, Happy = 4, Love = 5 }

    /// <summary>
    /// One articulated toy Pep. Character animation deliberately lives in a
    /// handful of child-transform poses rather than a rig or an Animator
    /// graph: the same inexpensive body language then works in every rescue,
    /// including future content that knows nothing about character animation.
    /// </summary>
    public sealed class Pep : MonoBehaviour
    {
        private const string BaseMapId = "_BaseMap";

        private enum Motion
        {
            Still,
            Idle,
            Run,
            Hug,
            Celebrate,
            CoverEyes,
            Shrug,
        }

        private struct RestPose
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;

            public static RestPose Capture(Transform target) => target == null
                ? default
                : new RestPose
                {
                    Position = target.localPosition,
                    Rotation = target.localRotation,
                    Scale = target.localScale,
                };

            public readonly void Apply(Transform target)
            {
                if (target == null) return;
                target.localPosition = Position;
                target.localRotation = Rotation;
                target.localScale = Scale;
            }
        }

        [Header("Wiring")]
        [SerializeField] private Renderer _faceRenderer;
        [SerializeField] private Transform _body;
        [SerializeField] private Transform _leftArm;
        [SerializeField] private Transform _rightArm;
        [SerializeField] private Transform _leftFoot;
        [SerializeField] private Transform _rightFoot;
        [SerializeField] private Transform _accessory;

        [Header("Personality")]
        [Tooltip("Fallback partner side when both Peps overlap in screen X. A and B use opposite values.")]
        [SerializeField] private float _naturalReachSide = 1f;
        [SerializeField] private float _leanDegrees = 7f;
        [SerializeField] private float _bobHeight = 0.025f;
        [SerializeField] private float _bobSpeed = 1.7f;

        [Header("Face atlas")]
        [SerializeField] private int _faceCount = 6;

        private Material _faceMaterial;
        private Transform _partner;
        private Vector3 _rootRest;
        private RestPose _bodyRest;
        private RestPose _leftArmRest;
        private RestPose _rightArmRest;
        private RestPose _leftFootRest;
        private RestPose _rightFootRest;
        private RestPose _accessoryRest;
        private Motion _motion = Motion.Idle;
        private float _phase;
        private float _motionClock;

        public AnimTarget Target { get; private set; }

        private void Awake()
        {
            Target = GetComponentInChildren<AnimTarget>();
            _rootRest = transform.localPosition;
            _bodyRest = RestPose.Capture(_body);
            _leftArmRest = RestPose.Capture(_leftArm);
            _rightArmRest = RestPose.Capture(_rightArm);
            _leftFootRest = RestPose.Capture(_leftFoot);
            _rightFootRest = RestPose.Capture(_rightFoot);
            _accessoryRest = RestPose.Capture(_accessory);

            // Stable per-character phase: the couple breathe together without
            // looking like duplicated wind-up toys.
            _phase = _naturalReachSide > 0f ? 0.35f : 1.15f;
            SetFace(PepFace.Hopeful);
        }

        public void SetPartner(Transform partner) => _partner = partner;

        public void SetIdle(bool idle)
        {
            _motion = idle ? Motion.Idle : Motion.Still;
            _motionClock = 0f;
            ApplyRestPose();
        }

        public void BeginRun()
        {
            _motion = Motion.Run;
            _motionClock = 0f;
            SetFace(PepFace.Happy);
        }

        public void BeginHug()
        {
            _motion = Motion.Hug;
            _motionClock = 0f;
            SetFace(PepFace.Love);
        }

        public void BeginCelebrate()
        {
            _motion = Motion.Celebrate;
            _motionClock = 0f;
            SetFace(PepFace.Love);
        }

        /// <summary>
        /// Complementary reactions make the pair respond to each other: one
        /// peeks through covered eyes while the other offers a helpless shrug.
        /// </summary>
        public void ReactToWrong(bool coverEyes)
        {
            _motion = coverEyes ? Motion.CoverEyes : Motion.Shrug;
            _motionClock = 0f;
            SetFace(coverEyes ? PepFace.Panic : PepFace.Worried);
        }

        public void SetFace(PepFace face)
        {
            if (_faceRenderer == null || _faceCount <= 0) return;

            // URP does not reliably honour _BaseMap_ST from a property block
            // on every Android renderer, so each Pep owns this tiny material
            // instance. It avoids the six faces being squeezed into one quad.
            _faceMaterial ??= _faceRenderer.material;
            var step = 1f / _faceCount;
            _faceMaterial.SetTextureScale(BaseMapId, new Vector2(step, 1f));
            _faceMaterial.SetTextureOffset(BaseMapId, new Vector2((int)face * step, 0f));
        }

        private void OnDestroy()
        {
            if (_faceMaterial != null) Destroy(_faceMaterial);
        }

        /// <summary>Restores every articulated part as well as choreography.</summary>
        public void ResetToRest()
        {
            transform.localPosition = _rootRest;
            _motion = Motion.Idle;
            _motionClock = 0f;
            ApplyRestPose();
            SetFace(PepFace.Hopeful);
            Target?.ResetToRest();
        }

        private void Update()
        {
            _phase += Time.deltaTime * _bobSpeed;
            _motionClock += Time.deltaTime;
            ApplyRestPose();

            switch (_motion)
            {
                case Motion.Idle:
                    ApplyIdle();
                    break;
                case Motion.Run:
                    ApplyRun();
                    break;
                case Motion.Hug:
                    ApplyHug();
                    break;
                case Motion.Celebrate:
                    ApplyCelebrate();
                    break;
                case Motion.CoverEyes:
                    ApplyCoverEyes();
                    break;
                case Motion.Shrug:
                    ApplyShrug();
                    break;
            }
        }

        private void ApplyIdle()
        {
            if (_body == null) return;

            var breath = Mathf.Sin(_phase);
            _body.localPosition += Vector3.up * (breath * _bobHeight);
            _body.localScale = Vector3.Scale(_body.localScale,
                new Vector3(1f - breath * 0.012f, 1f + breath * 0.018f, 1f - breath * 0.012f));

            var side = PartnerSide();
            _body.localRotation *= Quaternion.Euler(0f, side * 3f, -side * _leanDegrees);

            // A slow reach-and-relax beat reads as longing without becoming a
            // hint animation or competing with the three choices.
            var reachWave = Mathf.Max(0f, Mathf.Sin(_phase * 0.48f - 0.35f));
            var reach = reachWave * reachWave * 34f + 10f;
            RotateArm(side > 0f ? _rightArm : _leftArm, side, reach);
            RotateArm(side > 0f ? _leftArm : _rightArm, -side, 7f);

            if (_accessory != null)
            {
                _accessory.localRotation *= Quaternion.Euler(0f, 0f, breath * 4f);
            }
        }

        private void ApplyRun()
        {
            var beat = _motionClock * 13f;
            var stride = Mathf.Sin(beat);
            if (_body != null)
            {
                _body.localPosition += Vector3.up * (Mathf.Abs(stride) * 0.038f);
                _body.localRotation *= Quaternion.Euler(stride * 2f, 0f, -stride * 3f);
                var squash = Mathf.Abs(stride) * 0.035f;
                _body.localScale = Vector3.Scale(_body.localScale,
                    new Vector3(1f + squash, 1f - squash, 1f + squash));
            }

            RotateZ(_leftArm, stride * 30f);
            RotateZ(_rightArm, -stride * 30f);
            RotateZ(_leftFoot, -stride * 18f);
            RotateZ(_rightFoot, stride * 18f);
        }

        private void ApplyHug()
        {
            var t = Easing.Evaluate(EaseKind.Back, Mathf.Clamp01(_motionClock / 0.32f));
            var side = PartnerSide();
            if (_body != null)
            {
                _body.localRotation *= Quaternion.Euler(0f, side * 5f, -side * 11f * t);
                var squeeze = Mathf.Sin(Mathf.Clamp01(_motionClock / 0.55f) * Mathf.PI) * 0.06f;
                _body.localScale = Vector3.Scale(_body.localScale,
                    new Vector3(1f + squeeze, 1f - squeeze, 1f + squeeze));
            }

            RotateArm(_leftArm, -1f, 82f * t);
            RotateArm(_rightArm, 1f, 82f * t);
            RotateZ(_leftFoot, 7f * t);
            RotateZ(_rightFoot, -7f * t);
        }

        private void ApplyCelebrate()
        {
            var beat = _motionClock * 6.5f;
            var bounce = Mathf.Max(0f, Mathf.Sin(beat));
            if (_body != null)
            {
                _body.localPosition += Vector3.up * (0.025f + bounce * 0.055f);
                _body.localRotation *= Quaternion.Euler(0f, Mathf.Sin(beat * 0.5f) * 4f,
                    -PartnerSide() * (8f + Mathf.Sin(beat) * 3f));
            }

            RotateArm(_leftArm, -1f, 72f + Mathf.Sin(beat) * 12f);
            RotateArm(_rightArm, 1f, 72f - Mathf.Sin(beat) * 12f);
            if (_accessory != null) _accessory.localRotation *= Quaternion.Euler(0f, beat * 8f, 0f);
        }

        private void ApplyCoverEyes()
        {
            var t = Easing.Evaluate(EaseKind.Back, Mathf.Clamp01(_motionClock / 0.28f));
            RotateArm(_leftArm, -1f, 112f * t);
            RotateArm(_rightArm, 1f, 112f * t);
            if (_body != null)
            {
                _body.localPosition += Vector3.down * (0.025f * t);
                _body.localRotation *= Quaternion.Euler(7f * t, 0f, -PartnerSide() * 5f * t);
            }
        }

        private void ApplyShrug()
        {
            var t = Easing.Evaluate(EaseKind.Back, Mathf.Clamp01(_motionClock / 0.32f));
            RotateZ(_leftArm, -88f * t);
            RotateZ(_rightArm, 88f * t);
            if (_body != null)
            {
                var bob = Mathf.Sin(Mathf.Min(_motionClock, 0.7f) * Mathf.PI * 2f) * 0.025f;
                _body.localPosition += Vector3.up * bob;
                _body.localRotation *= Quaternion.Euler(0f, 0f, PartnerSide() * 7f * t);
            }
        }

        private void ApplyRestPose()
        {
            _bodyRest.Apply(_body);
            _leftArmRest.Apply(_leftArm);
            _rightArmRest.Apply(_rightArm);
            _leftFootRest.Apply(_leftFoot);
            _rightFootRest.Apply(_rightFoot);
            _accessoryRest.Apply(_accessory);
        }

        private float PartnerSide()
        {
            if (_partner == null) return Mathf.Sign(_naturalReachSide);

            var camera = Camera.main;
            if (camera != null)
            {
                var delta = camera.WorldToScreenPoint(_partner.position).x -
                            camera.WorldToScreenPoint(transform.position).x;
                if (Mathf.Abs(delta) > 4f) return Mathf.Sign(delta);
            }

            return Mathf.Sign(_naturalReachSide);
        }

        private static void RotateArm(Transform arm, float side, float degrees)
        {
            // Arms hang down at rest. Rotating toward the body's centre is the
            // negative direction for the right arm and positive for the left.
            RotateZ(arm, -side * degrees);
        }

        private static void RotateZ(Transform target, float degrees)
        {
            if (target != null) target.localRotation *= Quaternion.Euler(0f, 0f, degrees);
        }
    }
}
