using System.Collections.Generic;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// The primitive vocabulary every generated toy is assembled from.
    ///
    /// These used to be private helpers inside <see cref="PrototypeArt"/>.
    /// They moved out when the catalogue went from fourteen shared dioramas to
    /// thirty-six per-world stages: one file holding the palette, the faces,
    /// the Peps, thirty-six props and thirty-six environments is not a file
    /// anyone reviews. The helpers are the only thing all of those share, so
    /// they are the seam to cut on.
    /// </summary>
    internal static class Toy
    {
        /// <summary>Set once per generation run so props can plant their blob shadow.</summary>
        internal static Material ShadowMaterial;

        private static readonly List<AnimTarget> HiddenAtRest = new();

        /// <summary>
        /// Marks a mover as invisible until choreography reveals it.
        ///
        /// Deferred rather than applied on the spot, and that is the whole
        /// point: <see cref="AnimTarget.SetVisibleAtRest"/> caches the
        /// renderers it finds *at the moment it is called*, so calling it on a
        /// freshly created mover — before its visuals are parented — records
        /// the flag but disables nothing. The runtime survives that (it caches
        /// again in Awake), but the saved prefab keeps every beam, glow and
        /// lightning bolt switched on, which makes the authored scene and any
        /// still frame of it a lie.
        /// </summary>
        internal static Transform Reveal(Transform choreo)
        {
            var target = choreo.GetComponent<AnimTarget>();
            if (target != null) HiddenAtRest.Add(target);
            return choreo;
        }

        /// <summary>Applies every deferred hide. Called once a stage is fully built.</summary>
        internal static void ApplyReveals()
        {
            foreach (var target in HiddenAtRest)
            {
                if (target != null) target.SetVisibleAtRest(false);
            }

            HiddenAtRest.Clear();
        }

        internal static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        internal static Transform Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        internal static Transform Anchor(Transform parent, string name, Vector3 localPos)
        {
            var t = Child(parent, name);
            t.localPosition = localPos;
            return t;
        }

        /// <summary>
        /// Scenery an outcome can animate. Mirrors a prop's shape — a named
        /// container whose Choreo child holds the AnimTarget and rests at
        /// identity — so the same additive-delta and reset rules apply to a
        /// mine cart as to the plank. Returns the transform to parent visuals to.
        /// </summary>
        internal static Transform Mover(Transform parent, string name)
        {
            var container = Child(parent, name);
            var choreo = Child(container, "Choreo");
            choreo.gameObject.AddComponent<AnimTarget>();
            return choreo;
        }

        /// <summary>
        /// A looping idle *below* a mover's Choreo node.
        ///
        /// Ambient motion may never share a transform with an
        /// <see cref="AnimTarget"/>: choreography's exact reset depends on rest
        /// being local identity, and a permanent sway would make that false.
        /// Putting the loop one level down is the same trick
        /// <see cref="ChoicePresentation"/> uses for a prop's idle bob, and it
        /// composes rather than fights.
        /// </summary>
        internal static Transform Idle(Transform choreo, AmbientMode mode, float amplitude, float speed,
            Vector3 axis, float phase = 0f, string controlId = null)
        {
            var idle = Child(choreo, "Idle");
            idle.gameObject.AddComponent<AmbientMotion>()
                .Configure(mode, amplitude, speed, axis, staggerChildren: false, phase, controlId);
            return idle;
        }

        /// <summary>
        /// Static scenery that moves on its own: grass, rain, bubbles, stars.
        /// Returns the holder to parent visuals to. With
        /// <paramref name="stagger"/> the holder drives its direct children by
        /// phase, so twelve conveyor slats cost one component.
        /// </summary>
        internal static Transform Living(Transform parent, string name, AmbientMode mode, float amplitude,
            float speed, Vector3 axis, bool stagger = false, float phase = 0f, string controlId = null)
        {
            var holder = Child(parent, name);
            holder.gameObject.AddComponent<AmbientMotion>()
                .Configure(mode, amplitude, speed, axis, stagger, phase, controlId);
            return holder;
        }

        internal static GameObject Primitive(PrimitiveType type, string name, Transform parent, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            // Visual meshes never carry colliders: tap targets are explicit
            // and oversized, and stray colliders would intercept raycasts.
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        /// <summary>Cube at a position and scale, in one line. The workhorse.</summary>
        internal static GameObject Box(Transform parent, string name, Material mat, Vector3 pos, Vector3 scale,
            Vector3 euler = default)
        {
            var go = Primitive(PrimitiveType.Cube, name, parent, mat);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (euler != Vector3.zero) go.transform.localRotation = Quaternion.Euler(euler);
            return go;
        }

        /// <summary>
        /// A box whose long axis joins two authored points. Cables, fallen
        /// trees and diagonal routes otherwise require hand-tuned Euler
        /// angles that stop lining up as soon as either endpoint moves.
        /// </summary>
        internal static GameObject Beam(Transform parent, string name, Material mat, Vector3 from, Vector3 to,
            float thickness)
        {
            var delta = to - from;
            var go = Primitive(PrimitiveType.Cube, name, parent, mat);
            go.transform.localPosition = (from + to) * 0.5f;
            go.transform.localScale = new Vector3(thickness, delta.magnitude, thickness);
            if (delta.sqrMagnitude > 0.000001f)
            {
                go.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            }
            return go;
        }

        internal static GameObject Ball(Transform parent, string name, Material mat, Vector3 pos, Vector3 scale,
            Vector3 euler = default)
        {
            var go = Primitive(PrimitiveType.Sphere, name, parent, mat);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (euler != Vector3.zero) go.transform.localRotation = Quaternion.Euler(euler);
            return go;
        }

        internal static GameObject Rod(Transform parent, string name, Material mat, Vector3 pos, Vector3 scale,
            Vector3 euler = default)
        {
            var go = Primitive(PrimitiveType.Cylinder, name, parent, mat);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            if (euler != Vector3.zero) go.transform.localRotation = Quaternion.Euler(euler);
            return go;
        }

        /// <summary>
        /// A chunky front-facing oval assembled from a handful of blocks.
        /// At this scale the gaps read as facets, while the empty centre is
        /// what distinguishes a fan cage, scissor grip or handle from a disc.
        /// </summary>
        internal static void BlockRing(Transform parent, string name, Material material, Vector3 centre,
            Vector2 radii, int segments, float thickness, float depth)
        {
            var length = Mathf.PI * (radii.x + radii.y) / segments * 1.12f;
            for (var i = 0; i < segments; i++)
            {
                var angle = i * Mathf.PI * 2f / segments;
                var degrees = i * 360f / segments;
                var block = Primitive(PrimitiveType.Cube, $"{name}_{i}", parent, material);
                block.transform.localPosition = centre + new Vector3(
                    Mathf.Cos(angle) * radii.x,
                    Mathf.Sin(angle) * radii.y,
                    0f);
                block.transform.localRotation = Quaternion.Euler(0f, 0f, -degrees);
                block.transform.localScale = new Vector3(thickness, length, depth);
            }
        }

        /// <summary>A cog: a disc with square teeth around it, facing the camera.</summary>
        internal static void Cog(Transform parent, string name, Material body, Material teeth, Vector3 centre,
            float radius, int toothCount, float depth = 0.05f)
        {
            var disc = Primitive(PrimitiveType.Cylinder, $"{name}_Disc", parent, body);
            disc.transform.localPosition = centre;
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            disc.transform.localScale = new Vector3(radius * 1.6f, depth * 0.5f, radius * 1.6f);

            for (var i = 0; i < toothCount; i++)
            {
                var angle = i * Mathf.PI * 2f / toothCount;
                var tooth = Primitive(PrimitiveType.Cube, $"{name}_Tooth", parent, teeth);
                tooth.transform.localPosition = centre + new Vector3(
                    Mathf.Cos(angle) * radius * 0.88f, Mathf.Sin(angle) * radius * 0.88f, 0f);
                tooth.transform.localRotation = Quaternion.Euler(0f, 0f, -i * 360f / toothCount);
                tooth.transform.localScale = new Vector3(radius * 0.30f, radius * 0.42f, depth);
            }
        }

        internal static void AddZ(Transform parent, Material material, Vector3 centre, float size)
        {
            var top = Primitive(PrimitiveType.Cube, "Z_Top", parent, material);
            top.transform.localPosition = centre + Vector3.up * size * 0.40f;
            top.transform.localScale = new Vector3(size, size * 0.16f, size * 0.15f);

            var slash = Primitive(PrimitiveType.Cube, "Z_Slash", parent, material);
            slash.transform.localPosition = centre;
            slash.transform.localRotation = Quaternion.Euler(0f, 0f, 38f);
            slash.transform.localScale = new Vector3(size * 0.16f, size * 1.05f, size * 0.15f);

            var bottom = Primitive(PrimitiveType.Cube, "Z_Bottom", parent, material);
            bottom.transform.localPosition = centre + Vector3.down * size * 0.40f;
            bottom.transform.localScale = new Vector3(size, size * 0.16f, size * 0.15f);
        }

        /// <summary>Softens a cube's read by shaving its corners with scale.</summary>
        internal static void Round(GameObject cube)
        {
            var s = cube.transform.localScale;
            cube.transform.localScale = new Vector3(s.x * 0.96f, s.y, s.z * 0.96f);
        }

        /// <summary>
        /// The shell every tappable prop shares: an oversized invisible
        /// collider, an <see cref="AnimTarget"/> child that choreography
        /// drives, and the visual mesh below that. The collider is deliberately
        /// much larger than the art — Save Pip's tap circles ran ~25% wider and
        /// that generosity is most of why it felt good under a thumb.
        /// </summary>
        internal static GameObject NewProp(string id, Vector3 tapSize, float tapCentreY)
        {
            var root = new GameObject(id);
            var box = root.AddComponent<BoxCollider>();
            box.size = tapSize;
            // The centre is explicit per prop rather than derived from the
            // height: props are modelled around different origins, and
            // assuming a base-origin left the plank's collider hovering above
            // its mesh, where taps slid underneath it.
            box.center = new Vector3(0f, tapCentreY, 0f);

            var tappable = root.AddComponent<Tappable>();
            tappable.ObjectId = id;

            var choreo = Child(root.transform, "Choreo");
            choreo.gameObject.AddComponent<AnimTarget>();
            var visual = Child(choreo, "Visual");
            root.AddComponent<ChoicePresentation>().Configure(visual);

            var shadow = Primitive(PrimitiveType.Sphere, "BlobShadow", root.transform, ShadowMaterial);
            shadow.transform.localPosition = new Vector3(0f, 0.008f, 0.025f);
            shadow.transform.localScale = new Vector3(
                Mathf.Max(0.16f, tapSize.x * 0.58f),
                0.012f,
                Mathf.Max(0.13f, tapSize.z * 0.48f));
            shadow.AddComponent<BlobShadow>().Configure(choreo, shadow.GetComponent<Renderer>());
            return root;
        }

        internal static Transform FindChild(Transform root, string name)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                if (child.name == name) return child;
            }

            return null;
        }

        internal static GameObject SavePrefab(GameObject instance, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return prefab;
        }
    }
}
