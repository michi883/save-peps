using System.Collections.Generic;
using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// One thing choreography can move.
    ///
    /// Authored placement lives on this component's *parent*, and every
    /// animation is applied to this transform, which rests at local identity.
    /// Nothing ever writes to the placement transform.
    ///
    /// That is what makes reset exact rather than approximate. `ResetToRest`
    /// is not "remember where it was and put it back" — there is no
    /// bookkeeping to get wrong. Rest is identity, always, so an object that
    /// has been through five failed outcomes is bit-for-bit where it started.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimTarget : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Tooltip("Off for effects such as a reflected beam that choreography reveals, then retry hides again.")]
        [SerializeField] private bool _visibleAtRest = true;

        private Renderer[] _renderers;
        private MaterialPropertyBlock _block;
        private readonly List<float> _baseAlphas = new();

        /// <summary>Accumulated contributions from every live move.</summary>
        private Vector3 _position;
        private Vector3 _euler;
        private float _scale = 1f;
        private float _alpha = 1f;
        private bool? _visibilityOverride;

        private void Awake() => CacheRenderers();

        private void CacheRenderers()
        {
            if (_renderers != null) return;
            _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
            _block = new MaterialPropertyBlock();
            _baseAlphas.Clear();
            foreach (var r in _renderers)
            {
                var mat = r.sharedMaterial;
                _baseAlphas.Add(mat != null && mat.HasProperty(BaseColorId) ? mat.GetColor(BaseColorId).a : 1f);
            }
        }

        /// <summary>Clears every accumulator and snaps back to the rest pose.</summary>
        public void ResetToRest()
        {
            _position = Vector3.zero;
            _euler = Vector3.zero;
            _scale = 1f;
            _visibilityOverride = null;
            _alpha = _visibleAtRest ? 1f : 0f;
            Apply();
        }

        /// <summary>
        /// Sets whether this target exists visually before an outcome.
        /// Generated dioramas use this for beams, glows and other effects
        /// revealed by a normal <see cref="StepKind.Show"/> step. It belongs
        /// to the target rather than to rescue code so retry stays exact.
        /// </summary>
        public void SetVisibleAtRest(bool visible)
        {
            _visibleAtRest = visible;
            // Editor generators add the AnimTarget before parenting its
            // renderers. A previous cache may therefore be legitimately
            // empty when this authoring call arrives.
            _renderers = null;
            ResetToRest();
        }

        /// <summary>
        /// Changes visibility without changing the authored rest state.
        /// Outcome state swaps use this instead of cross-fading two opaque
        /// toys through each other, which can flash or z-fight on mobile.
        /// </summary>
        public void SetVisible(bool visible)
        {
            // This is an outcome-state override, not a one-frame renderer
            // write. Choreography may also be moving the incoming twin; its
            // next Accumulate must preserve the atomic swap.
            _visibilityOverride = visible;
            _alpha = visible ? 1f : 0f;
            ApplyAlpha();
        }

        /// <summary>
        /// Sums the deltas of all live moves and writes them to the transform.
        /// Positions and rotations add; scales multiply; alpha is
        /// last-writer-wins, matching how Save Pip ran opacity as a replacing
        /// animation while transforms composited additively.
        /// </summary>
        public void Accumulate(IReadOnlyList<MoveInstance> moves, float now)
        {
            _position = Vector3.zero;
            _euler = Vector3.zero;
            _scale = 1f;
            _alpha = (_visibilityOverride ?? _visibleAtRest) ? 1f : 0f;

            for (var i = 0; i < moves.Count; i++)
            {
                var frame = moves[i].Evaluate(now);
                _position += frame.Position;
                _euler += frame.Rotation;
                _scale *= frame.Scale;
                if (frame.Alpha >= 0f) _alpha = frame.Alpha;
            }

            Apply();
        }

        private void Apply()
        {
            var t = transform;
            t.localPosition = _position;
            t.localRotation = Quaternion.Euler(_euler);
            t.localScale = Vector3.one * _scale;
            ApplyAlpha();
        }

        private void ApplyAlpha()
        {
            CacheRenderers();
            for (var i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (r == null) continue;

                // Below a hair of visible, disable outright: our opaque
                // palette material cannot actually fade, and a "hidden"
                // object that is still drawn is the kind of thing that only
                // shows up on a phone.
                var visible = _alpha > 0.01f;
                if (r.enabled != visible) r.enabled = visible;
                if (!visible) continue;

                r.GetPropertyBlock(_block);
                var c = r.sharedMaterial != null && r.sharedMaterial.HasProperty(BaseColorId)
                    ? r.sharedMaterial.GetColor(BaseColorId)
                    : Color.white;
                c.a = _baseAlphas[i] * _alpha;
                _block.SetColor(BaseColorId, c);
                r.SetPropertyBlock(_block);
            }
        }
    }
}
