using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// Keeps a cheap translucent blob on the ground beneath an animated toy.
    /// No realtime shadows are needed, and hops still read as height because
    /// the blob stays grounded and contracts as its subject rises.
    /// </summary>
    public sealed class BlobShadow : MonoBehaviour
    {
        [SerializeField] private Transform _subject;
        [SerializeField] private Renderer _renderer;
        [SerializeField, Range(0f, 2f)] private float _fadeHeight = 0.9f;

        private Vector3 _restPosition;
        private Vector3 _restScale;
        private Color _restColor;
        private MaterialPropertyBlock _properties;

        public void Configure(Transform subject, Renderer shadowRenderer)
        {
            _subject = subject;
            _renderer = shadowRenderer;
        }

        private void Awake()
        {
            _restPosition = transform.localPosition;
            _restScale = transform.localScale;
            if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null && _renderer.sharedMaterial != null)
            {
                _restColor = _renderer.sharedMaterial.GetColor("_BaseColor");
                _properties = new MaterialPropertyBlock();
            }
        }

        private void LateUpdate()
        {
            if (_subject == null || transform.parent == null) return;

            var local = transform.parent.InverseTransformPoint(_subject.position);
            var height = Mathf.Max(0f, local.y - _restPosition.y);
            transform.localPosition = new Vector3(local.x, _restPosition.y, local.z);

            var height01 = _fadeHeight <= 0f ? 0f : Mathf.Clamp01(height / _fadeHeight);
            transform.localScale = Vector3.Scale(_restScale,
                new Vector3(Mathf.Lerp(1f, 0.62f, height01), 1f, Mathf.Lerp(1f, 0.62f, height01)));

            if (_renderer == null || _properties == null) return;
            _renderer.GetPropertyBlock(_properties);
            var color = _restColor;
            color.a *= Mathf.Lerp(1f, 0.18f, height01);
            _properties.SetColor("_BaseColor", color);
            _renderer.SetPropertyBlock(_properties);
        }
    }
}
