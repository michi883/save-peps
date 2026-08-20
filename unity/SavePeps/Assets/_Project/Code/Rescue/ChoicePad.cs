using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// A quiet toy token under each choice. The common shape establishes the
    /// interaction hierarchy without a label, outline shader, or tutorial.
    /// </summary>
    public sealed class ChoicePad : MonoBehaviour
    {
        [SerializeField] private string _anchorId;
        [SerializeField] private Transform _halo;
        [SerializeField] private Transform _surface;

        private Vector3 _haloRest;
        private Vector3 _surfaceRest;
        private float _phase;
        private bool _selected;
        private bool _locked;

        public string AnchorId => _anchorId;

        public void Configure(string anchorId, Transform halo, Transform surface)
        {
            _anchorId = anchorId;
            _halo = halo;
            _surface = surface;
        }

        private void Awake()
        {
            if (_halo != null) _haloRest = _halo.localScale;
            if (_surface != null) _surfaceRest = _surface.localScale;
            _phase = (Mathf.Abs(gameObject.name.GetHashCode()) % 500) / 500f * Mathf.PI * 2f;
        }

        public void SetSelection(bool selected, bool locked)
        {
            _selected = selected;
            _locked = locked;
        }

        public void ResetPresentation()
        {
            _selected = false;
            _locked = false;
            ApplyScale(1f, 1f);
        }

        private void Update()
        {
            _phase += Time.deltaTime * 2f;
            if (!_locked)
            {
                ApplyScale(1f + Mathf.Sin(_phase) * 0.035f, 1f);
            }
            else if (_selected)
            {
                ApplyScale(1.16f + Mathf.Sin(_phase * 1.5f) * 0.035f, 1.03f);
            }
            else
            {
                ApplyScale(0.86f, 0.94f);
            }
        }

        private void ApplyScale(float halo, float surface)
        {
            if (_halo != null) _halo.localScale = _haloRest * halo;
            if (_surface != null) _surface.localScale = _surfaceRest * surface;
        }
    }
}
