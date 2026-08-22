using System.Collections.Generic;
using System.IO;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Toy;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Generates the low-poly toy art: palette materials, the Peps, and then
    /// hands off to <see cref="PropLibrary"/> for the thirty-six tappables and
    /// <see cref="DioramaLibrary"/> for the thirty-six stages.
    ///
    /// Art as code is a deliberate production constraint here: these compact
    /// primitive toys are the final visual language, and one proportion or
    /// palette adjustment can improve every rescue without a DCC round-trip.
    /// It is also what made twelve genuinely different worlds affordable — a
    /// world is a function, not a folder of hand-placed prefabs.
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

        // From design/palette.md — the nine ramps, four steps each. The
        // prototype uses solid-colour materials rather than the atlas: Unity
        // primitives have no useful UVs to point at swatches. The one-material
        // atlas policy applies when authored meshes arrive.
        //
        // Rows 7 and 8 (coral, mint) appear here only because the Peps are
        // built from them. Nothing else in the game may wear them.
        private static readonly (string Name, string Hex)[] Swatches =
        {
            // Row 0 — ink and shadow. #3D3354 is the only dark in the game.
            ("Abyss", "221D33"), ("Ink", "3D3354"), ("Violet", "57406B"), ("Stone", "8E8BA7"),
            // Row 1 — foliage
            ("FoliageDark", "5E8F51"), ("Foliage", "7FB069"), ("FoliageLight", "95C77E"),
            ("FoliageBright", "A9D488"),
            // Row 2 — earth
            ("EarthDark", "57402D"), ("Earth", "6B4A34"), ("EarthLight", "9C6748"), ("Clay", "B27A58"),
            // Row 3 — wood and sand
            ("WoodDark", "8E6D50"), ("WoodMid", "B08F6C"), ("Wood", "C9A87F"), ("Sand", "E8DCC8"),
            // Row 4 — water
            ("WaterDeep", "5FB7D4"), ("Water", "6FC0E3"), ("WaterBright", "8FD6F9"), ("WaterLight", "CDEBF7"),
            // Row 5 — sky and cream
            ("Sky", "B8E6F5"), ("Cream", "F7F3E8"), ("Candle", "FFF3CE"),
            // Row 6 — warm accent, reserved for attention
            ("AccentDeep", "E8B62D"), ("Accent", "FFB53E"), ("AccentLight", "FFCF56"), ("AccentPale", "FFDE8A"),
            // Rows 7 and 8 — the Peps, and only the Peps
            ("PepA", "FF7660"), ("PepALight", "FFAA91"), ("PepB", "2EC4B6"), ("PepBDark", "168F88"),
            // Off-ramp utilities. Snow stays visibly blue-grey under the
            // intentionally bright mobile lighting: near-white read as an
            // untextured prototype plane on the Pixel 4 and also swallowed
            // cream props such as the pillow.
            ("StoneLight", "C3C0D5"), ("Night", "514766"), ("Snow", "D9EDF0"), ("Ice", "72D5E5"),
        };

        private static readonly Color Shadow = new(0.12f, 0.09f, 0.18f, 0.20f);

        // The two colours the face atlas draws with directly. Everything else
        // reaches the geometry as a material.
        private static readonly Color Ink = Hex("3D3354");
        private static readonly Color PepACoral = Hex("FF7660");

        /// <summary>
        /// Environments the twelve-world revamp replaced. They are deleted
        /// rather than left in place because an unreferenced diorama is the
        /// exact thing a later reader mistakes for live content — and because
        /// the validator's "no two rescues share a stage" rule means nothing
        /// can quietly point at one again.
        /// </summary>
        private static readonly string[] SupersededDioramas =
        {
            "Diorama_Brook", "Diorama_Wake", "Diorama_Vines", "Diorama_Guard", "Diorama_Lift",
            "Diorama_Beam", "Diorama_Thaw", "Diorama_Grow", "Diorama_Rain", "Diorama_Canyon",
            "Diorama_Ocean", "Diorama_Space", "Diorama_Factory", "Diorama_Neon",
        };

        [MenuItem("Tools/Save Peps/Generate Prototype Art")]
        public static void Generate()
        {
            foreach (var dir in new[] { MatDir, PropDir, CharDir, EnvDir })
            {
                Directory.CreateDirectory(dir);
            }

            var mats = BuildMaterials();
            Toy.ShadowMaterial = mats["Shadow"];
            var faceMat = BuildFaceAtlas();

            var pepA = BuildPep("Pep_A", mats["PepA"], mats["PepALight"], mats["Cream"],
                mats["Accent"], mats["Ink"], faceMat, tall: true);
            var pepB = BuildPep("Pep_B", mats["PepB"], mats["PepBDark"], mats["Cream"],
                mats["Accent"], mats["Ink"], faceMat, tall: false);

            PropLibrary.BuildAll(mats, PropDir);

            Worlds.M = mats;
            var stages = DioramaLibrary.BuildAll(EnvDir);

            RemoveSupersededDioramas();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SavePeps] Prototype art generated: {mats.Count} materials, " +
                      $"{pepA.name} and {pepB.name}, 36 props, {stages} stages across 12 worlds.");
        }

        private static void RemoveSupersededDioramas()
        {
            foreach (var name in SupersededDioramas)
            {
                var path = $"{EnvDir}/{name}.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"[SavePeps] Removed superseded diorama {name}.");
            }
        }

        // -------------------------------------------------------------------
        // Materials
        // -------------------------------------------------------------------

        private static Dictionary<string, Material> BuildMaterials()
        {
            var swatches = new Dictionary<string, Color>();
            foreach (var (name, hex) in Swatches) swatches[name] = Hex(hex);
            swatches["Shadow"] = Shadow;

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

                if (name is "Shadow" or "Ice")
                {
                    if (name == "Ice")
                    {
                        var translucent = color;
                        translucent.a = 0.58f;
                        mat.SetColor("_BaseColor", translucent);
                        mat.SetFloat("_Smoothness", 0.28f);
                    }
                    mat.SetFloat("_Surface", 1f);
                    mat.SetFloat("_Blend", 0f);
                    mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetFloat("_ZWrite", 0f);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }
                else
                {
                    mat.SetFloat("_Surface", 0f);
                    mat.SetFloat("_ZWrite", 1f);
                    mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = -1;
                }
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
        private static GameObject BuildPep(string name, Material body, Material bodyAccent, Material cream,
            Material gold, Material ink, Material faceMat, bool tall)
        {
            var root = new GameObject(name);
            var choreo = Child(root.transform, "Choreo");
            choreo.gameObject.AddComponent<AnimTarget>();
            var bodyT = Child(choreo, "Body");

            var torso = Primitive(tall ? PrimitiveType.Sphere : PrimitiveType.Cube, "Torso", bodyT, body);
            torso.transform.localPosition = new Vector3(0f, tall ? 0.22f : 0.18f, 0f);
            torso.transform.localScale = tall
                ? new Vector3(0.23f, 0.34f, 0.22f)     // pear-shaped and tall
                : new Vector3(0.27f, 0.23f, 0.23f);    // compact and boxy
            if (!tall) Round(torso);

            // A lighter belly panel gives the bodies a front and keeps their
            // silhouette readable even when the face is showing a tiny line.
            var belly = Primitive(PrimitiveType.Sphere, "Belly", bodyT, tall ? bodyAccent : cream);
            belly.transform.localPosition = new Vector3(0f, tall ? 0.12f : 0.10f, tall ? -0.116f : -0.142f);
            belly.transform.localScale = tall
                ? new Vector3(0.13f, 0.10f, 0.025f)
                : new Vector3(0.15f, 0.09f, 0.025f);

            var face = GameObject.CreatePrimitive(PrimitiveType.Quad);
            face.name = "Face";
            Object.DestroyImmediate(face.GetComponent<Collider>());
            face.transform.SetParent(bodyT, false);
            face.transform.localPosition = new Vector3(0f, tall ? 0.27f : 0.22f, tall ? -0.124f : -0.142f);
            face.transform.localScale = Vector3.one * (tall ? 0.195f : 0.18f);
            var faceRenderer = face.GetComponent<MeshRenderer>();
            faceRenderer.sharedMaterial = faceMat;

            Transform leftArm = null, rightArm = null, leftFoot = null, rightFoot = null;
            foreach (var side in new[] { -1f, 1f })
            {
                var armPivot = Child(bodyT, side < 0 ? "ArmPivot_L" : "ArmPivot_R");
                armPivot.localPosition = new Vector3(side * (tall ? 0.16f : 0.17f), tall ? 0.25f : 0.21f, 0f);
                var arm = Primitive(PrimitiveType.Sphere, side < 0 ? "Arm_L" : "Arm_R", armPivot, body);
                arm.transform.localPosition = new Vector3(side * 0.015f, -0.065f, 0f);
                arm.transform.localScale = new Vector3(0.060f, tall ? 0.13f : 0.115f, 0.060f);
                var hand = Primitive(PrimitiveType.Sphere, "Hand", armPivot, bodyAccent);
                hand.transform.localPosition = new Vector3(side * 0.018f, tall ? -0.135f : -0.120f, -0.006f);
                hand.transform.localScale = Vector3.one * 0.064f;

                var footPivot = Child(bodyT, side < 0 ? "FootPivot_L" : "FootPivot_R");
                footPivot.localPosition = new Vector3(side * 0.078f, 0.035f, -0.010f);
                var foot = Primitive(PrimitiveType.Sphere, side < 0 ? "Foot_L" : "Foot_R", footPivot, bodyAccent);
                foot.transform.localPosition = new Vector3(side * 0.012f, 0f, -0.028f);
                foot.transform.localScale = new Vector3(0.10f, 0.055f, 0.125f);

                if (side < 0)
                {
                    leftArm = armPivot;
                    leftFoot = footPivot;
                }
                else
                {
                    rightArm = armPivot;
                    rightFoot = footPivot;
                }
            }

            var accessory = Child(bodyT, tall ? "Curl" : "Bow");
            if (tall)
            {
                accessory.localPosition = new Vector3(0.025f, 0.37f, -0.015f);
                var stem = Primitive(PrimitiveType.Cube, "CurlStem", accessory, ink);
                stem.transform.localPosition = new Vector3(-0.012f, 0.025f, 0f);
                stem.transform.localRotation = Quaternion.Euler(0f, 0f, -24f);
                stem.transform.localScale = new Vector3(0.016f, 0.070f, 0.016f);
                var curl = Primitive(PrimitiveType.Sphere, "CurlTip", accessory, gold);
                curl.transform.localPosition = new Vector3(0.020f, 0.060f, 0f);
                curl.transform.localScale = new Vector3(0.048f, 0.040f, 0.040f);
            }
            else
            {
                accessory.localPosition = new Vector3(0.18f, 0.36f, -0.075f);
                foreach (var side in new[] { -1f, 1f })
                {
                    var wing = Primitive(PrimitiveType.Sphere, "BowWing", accessory, cream);
                    wing.transform.localPosition = new Vector3(side * 0.045f, 0f, 0f);
                    wing.transform.localScale = new Vector3(0.070f, 0.045f, 0.030f);
                }
                var knot = Primitive(PrimitiveType.Sphere, "BowKnot", accessory, gold);
                knot.transform.localScale = Vector3.one * 0.040f;
            }

            var shadow = Primitive(PrimitiveType.Sphere, "BlobShadow", root.transform, Toy.ShadowMaterial);
            shadow.transform.localPosition = new Vector3(0f, 0.008f, 0.035f);
            shadow.transform.localScale = tall
                ? new Vector3(0.23f, 0.012f, 0.17f)
                : new Vector3(0.25f, 0.012f, 0.18f);
            shadow.AddComponent<BlobShadow>().Configure(choreo, shadow.GetComponent<Renderer>());

            var pep = root.AddComponent<Pep>();
            var so = new SerializedObject(pep);
            so.FindProperty("_faceRenderer").objectReferenceValue = faceRenderer;
            so.FindProperty("_body").objectReferenceValue = bodyT;
            so.FindProperty("_leftArm").objectReferenceValue = leftArm;
            so.FindProperty("_rightArm").objectReferenceValue = rightArm;
            so.FindProperty("_leftFoot").objectReferenceValue = leftFoot;
            so.FindProperty("_rightFoot").objectReferenceValue = rightFoot;
            so.FindProperty("_accessory").objectReferenceValue = accessory;
            so.FindProperty("_naturalReachSide").floatValue = tall ? 1f : -1f;
            so.FindProperty("_faceCount").intValue = FaceCount;
            so.ApplyModifiedPropertiesWithoutUndo();

            return SavePrefab(root, $"{CharDir}/{name}.prefab");
        }

    }
}
