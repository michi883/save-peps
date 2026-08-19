using System.Collections.Generic;
using System.IO;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Generates the P1 prototype art: palette materials, a face atlas, two
    /// Peps, three props and the Brook diorama.
    ///
    /// Art as code, on purpose. P1 needs geometry good enough to judge whether
    /// the toy reads on a phone — not final art — and generating it means no
    /// Blender dependency, a reviewable diff, and the ability to re-tune a
    /// proportion by changing a number rather than round-tripping a DCC. All
    /// of this is disposable: P2 replaces it with real meshes.
    ///
    ///   Tools > Save Peps > Generate Prototype Art
    /// </summary>
    public static class PrototypeArt
    {
        private const string Root = "Assets/_Project";
        private const string MatDir = Root + "/Art/Materials";
        private const string PropDir = Root + "/Art/Props";
        private const string CharDir = Root + "/Art/Characters";
        private const string EnvDir = Root + "/Art/Environments";

        // From design/palette.md. The prototype uses solid-colour materials
        // rather than the atlas: Unity primitives have no useful UVs to point
        // at swatches. The one-material atlas policy applies from P2, when
        // authored meshes arrive.
        private static readonly Color Ink = Hex("3D3354");
        private static readonly Color FoliageBase = Hex("7FB069");
        private static readonly Color FoliageLight = Hex("95C77E");
        private static readonly Color EarthBase = Hex("6B4A34");
        private static readonly Color WoodLight = Hex("C9A87F");
        private static readonly Color WaterBase = Hex("6FC0E3");
        private static readonly Color AccentBase = Hex("FFB53E");
        private static readonly Color PepACoral = Hex("FF7660");
        private static readonly Color PepBMint = Hex("2EC4B6");
        private static readonly Color StoneBase = Hex("8E8BA7");
        private static readonly Color Cream = Hex("F7F3E8");

        [MenuItem("Tools/Save Peps/Generate Prototype Art")]
        public static void Generate()
        {
            foreach (var dir in new[] { MatDir, PropDir, CharDir, EnvDir })
            {
                Directory.CreateDirectory(dir);
            }

            var mats = BuildMaterials();
            var faceMat = BuildFaceAtlas();

            var pepA = BuildPep("Pep_A", mats["PepA"], faceMat, tall: true);
            var pepB = BuildPep("Pep_B", mats["PepB"], faceMat, tall: false);

            var plank = BuildPlank(mats["Wood"]);
            var balloon = BuildBalloon(mats["Accent"], mats["Ink"]);
            var fan = BuildFan(mats["Stone"], mats["Cream"]);
            var stone = BuildStone(mats["Stone"]);
            var leaf = BuildLeaf(mats["FoliageLight"], mats["Foliage"]);
            var umbrella = BuildUmbrella(mats["Accent"], mats["Ink"]);

            var diorama = BuildBrookDiorama(mats);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SavePeps] Prototype art generated: " +
                      $"{pepA.name}, {pepB.name}, {plank.name}, {balloon.name}, {fan.name}, " +
                      $"{stone.name}, {leaf.name}, {umbrella.name}, {diorama.name}.");
        }

        // -------------------------------------------------------------------
        // Materials
        // -------------------------------------------------------------------

        private static Dictionary<string, Material> BuildMaterials()
        {
            var swatches = new Dictionary<string, Color>
            {
                ["Ink"] = Ink,
                ["Foliage"] = FoliageBase,
                ["FoliageLight"] = FoliageLight,
                ["Earth"] = EarthBase,
                ["Wood"] = WoodLight,
                ["Water"] = WaterBase,
                ["Accent"] = AccentBase,
                ["PepA"] = PepACoral,
                ["PepB"] = PepBMint,
                ["Stone"] = StoneBase,
                ["Cream"] = Cream,
            };

            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var result = new Dictionary<string, Material>();

            foreach (var (name, color) in swatches)
            {
                var path = $"{MatDir}/M_Pal_{name}.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null)
                {
                    mat = new Material(lit);
                    AssetDatabase.CreateAsset(mat, path);
                }

                mat.shader = lit;
                mat.SetColor("_BaseColor", color);
                // Flat and matte: the toy look comes from silhouette and
                // colour, not from specular highlights.
                mat.SetFloat("_Smoothness", 0f);
                mat.SetFloat("_Metallic", 0f);
                EditorUtility.SetDirty(mat);
                result[name] = mat;
            }

            return result;
        }

        // -------------------------------------------------------------------
        // Face atlas — six expressions in a row, drawn in code.
        // Inherited wholesale from Save Pip: swapping one drawing for another
        // carried the emotional range of 106 rescues at no runtime cost.
        // -------------------------------------------------------------------

        private const int FaceCell = 128;
        private const int FaceCount = 6;

        private static Material BuildFaceAtlas()
        {
            var tex = new Texture2D(FaceCell * FaceCount, FaceCell, TextureFormat.RGBA32, mipChain: false);
            var clear = new Color(1f, 1f, 1f, 0f);
            var pixels = new Color[tex.width * tex.height];
            for (var i = 0; i < pixels.Length; i++) pixels[i] = clear;
            tex.SetPixels(pixels);

            for (var f = 0; f < FaceCount; f++) DrawFace(tex, f);

            tex.Apply();

            var pngPath = $"{CharDir}/T_PepFaces.png";
            File.WriteAllBytes(pngPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(pngPath, ImportAssetOptions.ForceUpdate);

            if (AssetDatabase.GetImporterOverride(pngPath) == null &&
                AssetImporter.GetAtPath(pngPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Default;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            var matPath = $"{CharDir}/M_PepFace.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(mat, matPath);
            }

            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath));
            mat.SetColor("_BaseColor", Color.white);

            // Alpha *clip*, not alpha blend. A blended face sorts against the
            // body it is drawn on and can vanish or halo depending on angle;
            // clipping keeps it in the opaque queue with crisp edges, which is
            // what a drawn-on face wants. Setting _Surface alone is not
            // enough — URP needs the keyword and the cutoff too.
            mat.SetFloat("_Surface", 0f);
            mat.SetFloat("_AlphaClip", 1f);
            mat.SetFloat("_Cutoff", 0.5f);
            mat.SetFloat("_ZWrite", 1f);
            mat.EnableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            EditorUtility.SetDirty(mat);

            return mat;
        }

        /// <summary>
        /// One expression per cell, in <see cref="PepFace"/> order:
        /// neutral, worried, hopeful, panic, happy, love.
        /// </summary>
        private static void DrawFace(Texture2D tex, int index)
        {
            var ox = index * FaceCell;
            const int eyeY = 78;
            const int lx = 44, rx = 84;

            switch ((PepFace)index)
            {
                case PepFace.Neutral:
                    Dot(tex, ox + lx, eyeY, 9, Ink);
                    Dot(tex, ox + rx, eyeY, 9, Ink);
                    Line(tex, ox + 54, 44, ox + 74, 44, 5, Ink);
                    break;

                case PepFace.Worried:
                    Dot(tex, ox + lx, eyeY, 9, Ink);
                    Dot(tex, ox + rx, eyeY, 9, Ink);
                    Line(tex, ox + 32, 98, ox + 52, 92, 4, Ink);   // brows angled in
                    Line(tex, ox + 96, 98, ox + 76, 92, 4, Ink);
                    Arc(tex, ox + 64, 34, 18, 200f, 340f, 5, Ink); // downturned mouth
                    break;

                case PepFace.Hopeful:
                    Dot(tex, ox + lx, eyeY, 12, Ink);
                    Dot(tex, ox + rx, eyeY, 12, Ink);
                    Dot(tex, ox + lx + 4, eyeY + 5, 4, Color.white);
                    Dot(tex, ox + rx + 4, eyeY + 5, 4, Color.white);
                    Arc(tex, ox + 64, 48, 16, 200f, 340f, 5, Ink);
                    break;

                case PepFace.Panic:
                    Dot(tex, ox + lx, eyeY, 14, Color.white);
                    Dot(tex, ox + rx, eyeY, 14, Color.white);
                    Dot(tex, ox + lx, eyeY, 7, Ink);
                    Dot(tex, ox + rx, eyeY, 7, Ink);
                    Dot(tex, ox + 64, 38, 11, Ink);                // open mouth
                    break;

                case PepFace.Happy:
                    Arc(tex, ox + lx, eyeY, 11, 20f, 160f, 5, Ink); // ^ ^
                    Arc(tex, ox + rx, eyeY, 11, 20f, 160f, 5, Ink);
                    Arc(tex, ox + 64, 46, 20, 200f, 340f, 6, Ink);
                    break;

                case PepFace.Love:
                    Arc(tex, ox + lx, eyeY, 11, 20f, 160f, 5, Ink);
                    Arc(tex, ox + rx, eyeY, 11, 20f, 160f, 5, Ink);
                    Arc(tex, ox + 64, 46, 22, 200f, 340f, 7, Ink);
                    Dot(tex, ox + 24, 60, 7, PepACoral);            // blush
                    Dot(tex, ox + 104, 60, 7, PepACoral);
                    break;
            }
        }

        private static void Dot(Texture2D tex, int cx, int cy, int r, Color c)
        {
            for (var y = -r; y <= r; y++)
            for (var x = -r; x <= r; x++)
            {
                if (x * x + y * y > r * r) continue;
                Plot(tex, cx + x, cy + y, c);
            }
        }

        private static void Line(Texture2D tex, int x0, int y0, int x1, int y1, int w, Color c)
        {
            var steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
            for (var i = 0; i <= steps; i++)
            {
                var t = steps == 0 ? 0f : i / (float)steps;
                Dot(tex, Mathf.RoundToInt(Mathf.Lerp(x0, x1, t)), Mathf.RoundToInt(Mathf.Lerp(y0, y1, t)), w / 2, c);
            }
        }

        private static void Arc(Texture2D tex, int cx, int cy, int r, float fromDeg, float toDeg, int w, Color c)
        {
            for (var a = fromDeg; a <= toDeg; a += 1.5f)
            {
                var rad = a * Mathf.Deg2Rad;
                Dot(tex, cx + Mathf.RoundToInt(Mathf.Cos(rad) * r), cy + Mathf.RoundToInt(Mathf.Sin(rad) * r), w / 2, c);
            }
        }

        private static void Plot(Texture2D tex, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= tex.width || y >= tex.height) return;
            tex.SetPixel(x, y, c);
        }

        // -------------------------------------------------------------------
        // Characters
        // -------------------------------------------------------------------

        /// <summary>
        /// An articulated toy, not a humanoid rig. Distinct silhouettes carry
        /// the pair: A is tall and egg-shaped, B is short and boxy, so they
        /// are tellable apart in a thumbnail and at arm's length.
        /// </summary>
        private static GameObject BuildPep(string name, Material body, Material faceMat, bool tall)
        {
            var root = new GameObject(name);
            var choreo = Child(root.transform, "Choreo");
            choreo.gameObject.AddComponent<AnimTarget>();
            var bodyT = Child(choreo, "Body");

            var torso = Primitive(tall ? PrimitiveType.Sphere : PrimitiveType.Cube, "Torso", bodyT, body);
            torso.transform.localPosition = new Vector3(0f, tall ? 0.16f : 0.13f, 0f);
            torso.transform.localScale = tall
                ? new Vector3(0.22f, 0.30f, 0.22f)     // egg
                : new Vector3(0.24f, 0.22f, 0.22f);    // boxy
            if (!tall) Round(torso);

            var face = GameObject.CreatePrimitive(PrimitiveType.Quad);
            face.name = "Face";
            Object.DestroyImmediate(face.GetComponent<Collider>());
            face.transform.SetParent(bodyT, false);
            face.transform.localPosition = new Vector3(0f, tall ? 0.19f : 0.15f, -0.115f);
            face.transform.localScale = Vector3.one * (tall ? 0.19f : 0.17f);
            var faceRenderer = face.GetComponent<MeshRenderer>();
            faceRenderer.sharedMaterial = faceMat;

            var footY = 0.03f;
            foreach (var side in new[] { -1f, 1f })
            {
                var foot = Primitive(PrimitiveType.Sphere, side < 0 ? "Foot_L" : "Foot_R", bodyT, body);
                foot.transform.localPosition = new Vector3(side * 0.07f, footY, 0f);
                foot.transform.localScale = new Vector3(0.09f, 0.06f, 0.11f);

                var arm = Primitive(PrimitiveType.Sphere, side < 0 ? "Arm_L" : "Arm_R", bodyT, body);
                arm.transform.localPosition = new Vector3(side * (tall ? 0.12f : 0.13f), tall ? 0.16f : 0.13f, 0f);
                arm.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
            }

            var pep = root.AddComponent<Pep>();
            var so = new SerializedObject(pep);
            so.FindProperty("_faceRenderer").objectReferenceValue = faceRenderer;
            so.FindProperty("_body").objectReferenceValue = bodyT;
            so.FindProperty("_faceCount").intValue = FaceCount;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SavePrefab(root, $"{CharDir}/{name}.prefab");
        }

        // -------------------------------------------------------------------
        // Props
        // -------------------------------------------------------------------

        private static GameObject BuildPlank(Material wood)
        {
            var root = NewProp("plank", new Vector3(0.36f, 0.26f, 0.92f), tapCentreY: 0.02f);
            var visual = Primitive(PrimitiveType.Cube, "Visual", root.transform.Find("Choreo"), wood);
            // Long axis along Z so it bridges the brook without needing to be
            // rotated into place.
            visual.transform.localScale = new Vector3(0.16f, 0.04f, 0.78f);
            return SavePrefab(root, $"{PropDir}/plank.prefab");
        }

        private static GameObject BuildBalloon(Material skin, Material ink)
        {
            var root = NewProp("balloon", new Vector3(0.36f, 0.46f, 0.36f), tapCentreY: 0.17f);
            var choreo = root.transform.Find("Choreo");

            var bulb = Primitive(PrimitiveType.Sphere, "Bulb", choreo, skin);
            bulb.transform.localPosition = new Vector3(0f, 0.20f, 0f);
            bulb.transform.localScale = new Vector3(0.20f, 0.24f, 0.20f);

            var knot = Primitive(PrimitiveType.Cube, "Knot", choreo, skin);
            knot.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            knot.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);

            var string_ = Primitive(PrimitiveType.Cube, "String", choreo, ink);
            string_.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            string_.transform.localScale = new Vector3(0.008f, 0.07f, 0.008f);

            return SavePrefab(root, $"{PropDir}/balloon.prefab");
        }

        private static GameObject BuildFan(Material shell, Material blade)
        {
            var root = NewProp("fan", new Vector3(0.36f, 0.34f, 0.34f), tapCentreY: 0.11f);
            var choreo = root.transform.Find("Choreo");

            var stand = Primitive(PrimitiveType.Cube, "Stand", choreo, shell);
            stand.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            stand.transform.localScale = new Vector3(0.14f, 0.08f, 0.10f);

            var hub = Primitive(PrimitiveType.Cylinder, "Hub", choreo, shell);
            hub.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            hub.transform.localScale = new Vector3(0.13f, 0.02f, 0.13f);

            // Blades live under their own transform so a Spin step can turn
            // them without moving the housing.
            var blades = Child(choreo, "Blades");
            blades.localPosition = new Vector3(0f, 0.15f, -0.03f);
            for (var i = 0; i < 3; i++)
            {
                var b = Primitive(PrimitiveType.Cube, $"Blade_{i}", blades, blade);
                b.transform.localRotation = Quaternion.Euler(0f, 0f, i * 120f);
                b.transform.localPosition = Quaternion.Euler(0f, 0f, i * 120f) * new Vector3(0f, 0.05f, 0f);
                b.transform.localScale = new Vector3(0.035f, 0.10f, 0.012f);
            }

            return SavePrefab(root, $"{PropDir}/fan.prefab");
        }

        /// <summary>
        /// The stone that dams the brook. Squat and heavy-looking on purpose:
        /// it has to read as something that would stay put in moving water,
        /// or the solution is not legible before the tap.
        /// </summary>
        private static GameObject BuildStone(Material stone)
        {
            var root = NewProp("stone", new Vector3(0.38f, 0.34f, 0.38f), tapCentreY: 0.09f);
            var choreo = root.transform.Find("Choreo");

            var mass = Primitive(PrimitiveType.Sphere, "Mass", choreo, stone);
            mass.transform.localPosition = new Vector3(0f, 0.085f, 0f);
            mass.transform.localScale = new Vector3(0.24f, 0.17f, 0.21f);

            var shoulder = Primitive(PrimitiveType.Sphere, "Shoulder", choreo, stone);
            shoulder.transform.localPosition = new Vector3(0.06f, 0.05f, -0.04f);
            shoulder.transform.localScale = new Vector3(0.13f, 0.10f, 0.12f);

            return SavePrefab(root, $"{PropDir}/stone.prefab");
        }

        /// <summary>
        /// A big flat leaf — the ferry. Wide enough to obviously carry a Pep,
        /// which is the whole clue.
        /// </summary>
        private static GameObject BuildLeaf(Material blade, Material stem)
        {
            var root = NewProp("leaf", new Vector3(0.5f, 0.24f, 0.56f), tapCentreY: 0.03f);
            var choreo = root.transform.Find("Choreo");

            var body = Primitive(PrimitiveType.Sphere, "Blade", choreo, blade);
            body.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            body.transform.localScale = new Vector3(0.30f, 0.035f, 0.42f);

            var rib = Primitive(PrimitiveType.Cube, "Stem", choreo, stem);
            rib.transform.localPosition = new Vector3(0f, 0.035f, -0.24f);
            rib.transform.localScale = new Vector3(0.018f, 0.012f, 0.14f);

            return SavePrefab(root, $"{PropDir}/leaf.prefab");
        }

        /// <summary>
        /// The umbrella: a wrong answer that is always *nearly* right, which
        /// is what makes it worth having in more than one lineup. It shelters,
        /// it catches wind, it does everything except close a gap.
        /// </summary>
        private static GameObject BuildUmbrella(Material canopy, Material ink)
        {
            var root = NewProp("umbrella", new Vector3(0.42f, 0.52f, 0.42f), tapCentreY: 0.20f);
            var choreo = root.transform.Find("Choreo");

            var dome = Primitive(PrimitiveType.Sphere, "Canopy", choreo, canopy);
            dome.transform.localPosition = new Vector3(0f, 0.30f, 0f);
            dome.transform.localScale = new Vector3(0.32f, 0.15f, 0.32f);

            var shaft = Primitive(PrimitiveType.Cube, "Shaft", choreo, ink);
            shaft.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            shaft.transform.localScale = new Vector3(0.016f, 0.30f, 0.016f);

            var hook = Primitive(PrimitiveType.Cube, "Hook", choreo, ink);
            hook.transform.localPosition = new Vector3(0.03f, 0.015f, 0f);
            hook.transform.localScale = new Vector3(0.06f, 0.016f, 0.016f);

            return SavePrefab(root, $"{PropDir}/umbrella.prefab");
        }

        /// <summary>
        /// A prop root: an oversized invisible tap collider on the outside,
        /// an <see cref="AnimTarget"/> child that choreography drives, and the
        /// visual mesh below that. The collider is deliberately much larger
        /// than the art — Save Pip's tap circles ran ~25% wider and that
        /// generosity is most of why it felt good under a thumb.
        /// </summary>
        private static GameObject NewProp(string id, Vector3 tapSize, float tapCentreY)
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
            return root;
        }

        // -------------------------------------------------------------------
        // The Brook diorama
        // -------------------------------------------------------------------

        private static GameObject BuildBrookDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Brook");

            // Portrait-shaped on purpose: narrow and deep. A square diorama
            // wastes most of a phone screen, because portrait's horizontal
            // field of view is tiny — the width is what limits how close the
            // camera can get, and the depth is what fills the frame.
            //
            // A visible platform edge keeps it reading as a toy on a table
            // rather than as a cropped world.
            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Earth"]);
            slab.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.16f, 3.4f);

            var near = Primitive(PrimitiveType.Cube, "Bank_Near", root.transform, mats["Foliage"]);
            near.transform.localPosition = new Vector3(0f, 0.075f, -1.0f);
            near.transform.localScale = new Vector3(1.35f, 0.15f, 1.4f);

            var far = Primitive(PrimitiveType.Cube, "Bank_Far", root.transform, mats["FoliageLight"]);
            far.transform.localPosition = new Vector3(0f, 0.075f, 1.0f);
            far.transform.localScale = new Vector3(1.35f, 0.15f, 1.4f);

            // The brook is a *mover*, not scenery: r02 dams it and drains it,
            // so choreography has to be able to reach it. Movers are named
            // containers whose Choreo child carries the AnimTarget, which is
            // how RescueRunner registers them - by the container's name.
            var movers = Child(root.transform, "Movers");
            var water = Primitive(PrimitiveType.Cube, "Visual", Mover(movers, "Water"), mats["Water"]);
            water.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            water.transform.localScale = new Vector3(1.35f, 0.07f, 0.62f);

            // A couple of rocks so the banks are not bare boxes.
            foreach (var (x, z, s) in new[] { (-0.52f, -0.42f, 0.10f), (0.5f, 0.45f, 0.08f) })
            {
                var rock = Primitive(PrimitiveType.Sphere, "Rock", root.transform, mats["Stone"]);
                rock.transform.localPosition = new Vector3(x, 0.15f, z);
                rock.transform.localScale = new Vector3(s, s * 0.7f, s);
            }

            Anchor(root.transform, "Anchor_PepA", new Vector3(0f, 0.15f, -0.62f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0f, 0.15f, 0.62f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(0f, 0.15f, 0.5f));

            // Three lineup positions, reused by every rescue staged here. They
            // sit off the play line on purpose: an object the player has not
            // chosen yet must never be mistaken for part of the predicament.
            Anchor(root.transform, "Slot_1", new Vector3(-0.42f, 0.15f, -1.25f));
            Anchor(root.transform, "Slot_2", new Vector3(0.45f, 0.15f, -1.35f));
            Anchor(root.transform, "Slot_3", new Vector3(-0.45f, 0.15f, 1.3f));

            return SavePrefab(root, $"{EnvDir}/Diorama_Brook.prefab");
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        private static Transform Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static Transform Anchor(Transform parent, string name, Vector3 localPos)
        {
            var t = Child(parent, name);
            t.localPosition = localPos;
            return t;
        }

        /// <summary>
        /// Scenery an outcome can animate. Mirrors a prop's shape - a named
        /// container whose Choreo child holds the AnimTarget and rests at
        /// identity - so the same additive-delta and reset rules apply to the
        /// brook as to the plank. Returns the transform to parent visuals to.
        /// </summary>
        private static Transform Mover(Transform parent, string name)
        {
            var container = Child(parent, name);
            var choreo = Child(container, "Choreo");
            choreo.gameObject.AddComponent<AnimTarget>();
            return choreo;
        }

        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Material mat)
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

        /// <summary>Softens a cube's read by shaving its corners with scale.</summary>
        private static void Round(GameObject cube)
        {
            var s = cube.transform.localScale;
            cube.transform.localScale = new Vector3(s.x * 0.96f, s.y, s.z * 0.96f);
        }

        private static GameObject SavePrefab(GameObject instance, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            return prefab;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
