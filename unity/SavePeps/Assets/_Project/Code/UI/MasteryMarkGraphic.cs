using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.UI
{
    /// <summary>
    /// Draws the game's tiny mastery vocabulary without depending on font
    /// glyph coverage. ★ and ✓ are progression, not decoration, so a missing
    /// character box on one Android font fallback would make the result card
    /// lie about what the player earned.
    /// </summary>
    public enum MasteryMarkState
    {
        Empty = 0,
        Current = 1,
        Check = 2,
        Star = 3,
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MasteryMarkGraphic : MaskableGraphic
    {
        private static readonly Color32 Ink = new(61, 51, 84, 255);
        private static readonly Color32 Cream = new(255, 248, 224, 255);
        private static readonly Color32 Gold = new(255, 181, 62, 255);
        private static readonly Color32 Mint = new(91, 215, 193, 255);

        [SerializeField] private MasteryMarkState _state;

        public MasteryMarkState State => _state;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        public void SetState(MasteryMarkState state, bool animate = false)
        {
            var changed = _state != state;
            _state = state;
            SetVerticesDirty();

            if (animate && changed && state is MasteryMarkState.Star or MasteryMarkState.Check)
            {
                Punch(state == MasteryMarkState.Star ? 1.42f : 1.24f);
            }
            else if (!animate)
            {
                rectTransform.localScale = Vector3.one;
            }
        }

        public void Punch(float peak = 1.24f)
        {
            if (!isActiveAndEnabled) return;
            StopAllCoroutines();
            StartCoroutine(PunchRoutine(Mathf.Max(1f, peak)));
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = GetPixelAdjustedRect();
            var radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            var centre = rect.center;

            switch (_state)
            {
                case MasteryMarkState.Star:
                    AddDisc(vh, centre + Vector2.down * radius * 0.04f, radius * 0.48f,
                        new Color32(61, 51, 84, 28));
                    AddStar(vh, centre, radius * 0.88f, radius * 0.40f, Gold);
                    break;

                case MasteryMarkState.Check:
                    AddDisc(vh, centre, radius * 0.88f, new Color32(Mint.r, Mint.g, Mint.b, 76));
                    AddRing(vh, centre, radius * 0.86f, radius * 0.70f,
                        new Color32(Mint.r, Mint.g, Mint.b, 210));
                    AddSegment(vh,
                        centre + new Vector2(-radius * 0.43f, -radius * 0.02f),
                        centre + new Vector2(-radius * 0.10f, -radius * 0.34f),
                        radius * 0.11f, Ink);
                    AddSegment(vh,
                        centre + new Vector2(-radius * 0.10f, -radius * 0.34f),
                        centre + new Vector2(radius * 0.50f, radius * 0.35f),
                        radius * 0.11f, Ink);
                    break;

                case MasteryMarkState.Current:
                    AddDisc(vh, centre, radius * 0.82f, Cream);
                    AddRing(vh, centre, radius * 0.84f, radius * 0.70f,
                        new Color32(Ink.r, Ink.g, Ink.b, 150));
                    AddDisc(vh, centre, radius * 0.20f, Gold);
                    break;

                default:
                    AddRing(vh, centre, radius * 0.74f, radius * 0.60f,
                        new Color32(Ink.r, Ink.g, Ink.b, 70));
                    break;
            }
        }

        private IEnumerator PunchRoutine(float peak)
        {
            rectTransform.localScale = Vector3.one * 0.72f;
            var elapsed = 0f;
            const float upDuration = 0.18f;
            while (elapsed < upDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / upDuration);
                rectTransform.localScale = Vector3.one * Mathf.LerpUnclamped(0.72f, peak,
                    SavePeps.Rescue.Easing.Evaluate(SavePeps.Rescue.EaseKind.Back, t));
                yield return null;
            }

            elapsed = 0f;
            const float settleDuration = 0.16f;
            while (elapsed < settleDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = SavePeps.Rescue.Easing.Evaluate(SavePeps.Rescue.EaseKind.Out,
                    Mathf.Clamp01(elapsed / settleDuration));
                rectTransform.localScale = Vector3.one * Mathf.Lerp(peak, 1f, t);
                yield return null;
            }

            rectTransform.localScale = Vector3.one;
        }

        private static void AddStar(VertexHelper vh, Vector2 centre, float outer, float inner, Color32 color)
        {
            var points = new Vector2[10];
            for (var i = 0; i < points.Length; i++)
            {
                var angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                var radius = i % 2 == 0 ? outer : inner;
                points[i] = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            for (var i = 0; i < points.Length; i++)
            {
                AddTriangle(vh, centre, points[i], points[(i + 1) % points.Length], color);
            }
        }

        private static void AddDisc(VertexHelper vh, Vector2 centre, float radius, Color32 color)
        {
            const int segments = 24;
            for (var i = 0; i < segments; i++)
            {
                var a = i * Mathf.PI * 2f / segments;
                var b = (i + 1) * Mathf.PI * 2f / segments;
                AddTriangle(vh, centre,
                    centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    centre + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius,
                    color);
            }
        }

        private static void AddRing(VertexHelper vh, Vector2 centre, float outer, float inner, Color32 color)
        {
            const int segments = 24;
            for (var i = 0; i < segments; i++)
            {
                var a = i * Mathf.PI * 2f / segments;
                var b = (i + 1) * Mathf.PI * 2f / segments;
                var directionA = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                var directionB = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
                AddQuad(vh, centre + directionA * inner, centre + directionA * outer,
                    centre + directionB * outer, centre + directionB * inner, color);
            }
        }

        private static void AddSegment(VertexHelper vh, Vector2 from, Vector2 to, float halfWidth, Color32 color)
        {
            var direction = (to - from).normalized;
            var normal = new Vector2(-direction.y, direction.x) * halfWidth;
            AddQuad(vh, from - normal, from + normal, to + normal, to - normal, color);
        }

        private static void AddTriangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color32 color)
        {
            var start = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
        }

        private static void AddQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color32 color)
        {
            var start = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddVert(d, color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }
    }
}
