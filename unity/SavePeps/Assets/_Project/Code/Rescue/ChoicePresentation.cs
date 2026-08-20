using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// Shared presentation for every tappable prop. It animates a child below
    /// the choreography transform, so a readable idle bob and an immediate
    /// press response compose with authored outcome movement instead of
    /// fighting it.
    /// </summary>
    public sealed class ChoicePresentation : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField, Range(0f, 0.05f)] private float _bobHeight = 0.018f;
        [SerializeField, Range(0.5f, 3f)] private float _bobSpeed = 1.25f;

        private Vector3 _restPosition;
        private Quaternion _restRotation;
        private Vector3 _restScale;
        private float _phase;
        private float _selectionClock;
        private bool _selected;
        private bool _locked;

        public void Configure(Transform visual) => _visual = visual;

        private void Awake()
        {
            if (_visual == null) _visual = transform;
            _restPosition = _visual.localPosition;
            _restRotation = _visual.localRotation;
            _restScale = _visual.localScale;
            _phase = (Mathf.Abs(gameObject.name.GetHashCode()) % 1000) / 1000f * Mathf.PI * 2f;
        }

        public void SetSelection(bool selected, bool locked)
        {
            _selected = selected;
            _locked = locked;
            _selectionClock = 0f;
        }

        public void ResetPresentation()
        {
            _selected = false;
            _locked = false;
            _selectionClock = 0f;
            ApplyRest();
        }

        private void Update()
        {
            if (_visual == null) return;

            _phase += Time.deltaTime * _bobSpeed;
            _selectionClock += Time.deltaTime;
            ApplyRest();

            if (!_locked)
            {
                var wave = Mathf.Sin(_phase);
                _visual.localPosition += Vector3.up * (wave * _bobHeight + _bobHeight);
                _visual.localRotation *= Quaternion.Euler(0f, wave * 2.2f, wave * 1.2f);
                return;
            }

            if (!_selected)
            {
                _visual.localScale = Vector3.Scale(_restScale, Vector3.one * 0.94f);
                return;
            }

            // Squash on contact, overshoot, settle. The whole response is
            // shorter than a typical touch-up, so the tap always feels heard
            // before the first authored physical beat begins.
            var t = Mathf.Clamp01(_selectionClock / 0.24f);
            Vector3 squash;
            if (t < 0.35f)
            {
                var q = Easing.Evaluate(EaseKind.Out, t / 0.35f);
                squash = Vector3.Lerp(Vector3.one, new Vector3(1.10f, 0.82f, 1.10f), q);
            }
            else
            {
                var q = Easing.Evaluate(EaseKind.Back, (t - 0.35f) / 0.65f);
                squash = Vector3.Lerp(new Vector3(1.10f, 0.82f, 1.10f), Vector3.one, q);
            }

            _visual.localScale = Vector3.Scale(_restScale, squash);
        }

        private void ApplyRest()
        {
            _visual.localPosition = _restPosition;
            _visual.localRotation = _restRotation;
            _visual.localScale = _restScale;
        }
    }
}
