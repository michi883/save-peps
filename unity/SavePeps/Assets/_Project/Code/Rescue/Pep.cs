using UnityEngine;

namespace SavePeps.Rescue
{
    public enum PepFace { Neutral = 0, Worried = 1, Hopeful = 2, Panic = 3, Happy = 4, Love = 5 }

    /// <summary>
    /// A Pep.
    ///
    /// Faces are a UV swap on a flat quad, not blend shapes and not a facial
    /// rig — inherited straight from Save Pip, where swapping one drawing for
    /// another carried the entire emotional range of 106 rescues. It costs
    /// nothing and it is the single highest-return piece of character tech
    /// available to us.
    ///
    /// The idle lean is the other half: at rest a Pep tips toward its partner.
    /// The player should understand that these two want to be together before
    /// reading a single word.
    /// </summary>
    public sealed class Pep : MonoBehaviour
    {
        private const string BaseMapId = "_BaseMap";

        [Header("Wiring")]
        [SerializeField] private Renderer _faceRenderer;
        [SerializeField] private Transform _body;

        [Header("Idle")]
        [Tooltip("Degrees of lean toward the partner.")]
        [SerializeField] private float _leanDegrees = 8f;
        [SerializeField] private float _bobHeight = 0.03f;
        [SerializeField] private float _bobSpeed = 1.6f;

        [Header("Face atlas")]
        [Tooltip("Faces laid out in a single row, left to right, matching PepFace order.")]
        [SerializeField] private int _faceCount = 6;

        private Material _faceMaterial;
        private Transform _partner;
        private Vector3 _bodyRest;
        private Quaternion _bodyRestRotation;
        private Vector3 _rootRest;
        private float _phase;
        private bool _idle = true;

        public AnimTarget Target { get; private set; }

        private void Awake()
        {
            Target = GetComponentInChildren<AnimTarget>();
            if (_body != null)
            {
                _bodyRest = _body.localPosition;
                _bodyRestRotation = _body.localRotation;
            }

            // The reunion drives this root directly in world space, so its
            // rest has to be remembered — unlike the choreography transforms,
            // which rest at identity by construction.
            _rootRest = transform.localPosition;
            _phase = Random.value * Mathf.PI * 2f;
            SetFace(PepFace.Neutral);
        }

        /// <summary>Who this Pep leans toward and, eventually, runs to.</summary>
        public void SetPartner(Transform partner) => _partner = partner;

        public void SetIdle(bool idle)
        {
            _idle = idle;
            if (!idle || _body == null) return;
            _body.localPosition = _bodyRest;
        }

        public void SetFace(PepFace face)
        {
            if (_faceRenderer == null || _faceCount <= 0) return;

            // Written to an instanced material rather than a property block:
            // URP does not reliably honour _BaseMap_ST from a
            // MaterialPropertyBlock, which showed up on device as all six
            // expressions squeezed into one quad.
            _faceMaterial ??= _faceRenderer.material;
            var step = 1f / _faceCount;
            _faceMaterial.SetTextureScale(BaseMapId, new Vector2(step, 1f));
            _faceMaterial.SetTextureOffset(BaseMapId, new Vector2((int)face * step, 0f));
        }

        private void OnDestroy()
        {
            if (_faceMaterial != null) Destroy(_faceMaterial);
        }

        /// <summary>Resets to the pose a rescue starts from.</summary>
        public void ResetToRest()
        {
            transform.localPosition = _rootRest;
            if (_body != null)
            {
                _body.localPosition = _bodyRest;
                _body.localRotation = _bodyRestRotation;
            }

            SetIdle(true);
            SetFace(PepFace.Worried);
            Target?.ResetToRest();
        }

        private void Update()
        {
            if (!_idle || _body == null) return;

            _phase += Time.deltaTime * _bobSpeed;
            _body.localPosition = _bodyRest + Vector3.up * (Mathf.Sin(_phase) * _bobHeight);

            if (_partner == null) return;

            // Lean toward the partner: the separation should read as longing,
            // not as two props that happen to be apart.
            var toPartner = _partner.position - transform.position;
            toPartner.y = 0f;
            if (toPartner.sqrMagnitude < 0.0001f) return;

            var lean = Quaternion.AngleAxis(_leanDegrees, Vector3.Cross(Vector3.up, toPartner.normalized));
            _body.localRotation = Quaternion.Slerp(_body.localRotation, lean, Time.deltaTime * 4f);
        }
    }
}
