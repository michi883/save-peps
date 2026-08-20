using System.Collections.Generic;
using System.IO;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Generates the low-poly toy art: palette materials, the Peps, readable
    /// choice props, and the reusable opening dioramas.
    ///
    /// Art as code is a deliberate production constraint here: these compact
    /// primitive toys are the final visual language, and one proportion or
    /// palette adjustment can improve every rescue without a DCC round-trip.
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
        private static readonly Color FoliageDark = Hex("557F50");
        private static readonly Color EarthBase = Hex("6B4A34");
        private static readonly Color EarthLight = Hex("9A6C49");
        private static readonly Color WoodLight = Hex("C9A87F");
        private static readonly Color WaterBase = Hex("6FC0E3");
        private static readonly Color WaterLight = Hex("BCEAF5");
        private static readonly Color AccentBase = Hex("FFB53E");
        private static readonly Color AccentLight = Hex("FFD66B");
        private static readonly Color PepACoral = Hex("FF7660");
        private static readonly Color PepALight = Hex("FFAA91");
        private static readonly Color PepBMint = Hex("2EC4B6");
        private static readonly Color PepBDark = Hex("168F88");
        private static readonly Color StoneBase = Hex("8E8BA7");
        private static readonly Color StoneLight = Hex("C3C0D5");
        private static readonly Color Cream = Hex("F7F3E8");
        private static readonly Color Night = Hex("514766");
        // Keep snow visibly blue-grey under the intentionally bright mobile
        // lighting. Near-white read as an untextured prototype plane on the
        // Pixel 4 and also swallowed cream props such as the pillow.
        private static readonly Color Snow = Hex("D9EDF0");
        private static readonly Color Ice = Hex("72D5E5");
        private static readonly Color Shadow = new(0.12f, 0.09f, 0.18f, 0.20f);

        private static Material _shadowMaterial;

        [MenuItem("Tools/Save Peps/Generate Prototype Art")]
        public static void Generate()
        {
            foreach (var dir in new[] { MatDir, PropDir, CharDir, EnvDir })
            {
                Directory.CreateDirectory(dir);
            }

            var mats = BuildMaterials();
            _shadowMaterial = mats["Shadow"];
            var faceMat = BuildFaceAtlas();

            var pepA = BuildPep("Pep_A", mats["PepA"], mats["PepALight"], mats["Cream"],
                mats["Accent"], mats["Ink"], faceMat, tall: true);
            var pepB = BuildPep("Pep_B", mats["PepB"], mats["PepBDark"], mats["Cream"],
                mats["Accent"], mats["Ink"], faceMat, tall: false);

            var plank = BuildPlank(mats["Wood"]);
            var balloon = BuildBalloon(mats["Accent"], mats["Ink"]);
            var fan = BuildFan(mats["Stone"], mats["Cream"]);
            var stone = BuildStone(mats["Stone"], mats["Ink"]);
            var leaf = BuildLeaf(mats["FoliageLight"], mats["Foliage"]);
            var umbrella = BuildUmbrella(mats["Accent"], mats["Ink"]);
            var rope = BuildRope(mats["Earth"]);
            var bell = BuildBell(mats["Accent"], mats["Ink"]);
            var pillow = BuildPillow(mats["Cream"], mats["PepB"]);
            var scissors = BuildScissors(mats["Cream"], mats["PepA"], mats["Ink"]);
            var wateringCan = BuildWateringCan(mats["Water"], mats["Cream"]);
            var bone = BuildBone(mats["Cream"], mats["Earth"]);
            var mirror = BuildMirror(mats["Water"], mats["Stone"], mats["Cream"]);
            var hairDryer = BuildHairDryer(mats["PepA"], mats["Cream"], mats["Ink"], mats["AccentLight"]);

            var diorama = BuildBrookDiorama(mats);
            var wake = BuildWakeDiorama(mats);
            var vines = BuildVinesDiorama(mats);
            var guard = BuildGuardDiorama(mats);
            var lift = BuildLiftDiorama(mats);
            var beam = BuildBeamDiorama(mats);
            var thaw = BuildThawDiorama(mats);
            var grow = BuildGrowDiorama(mats);
            var rain = BuildRainDiorama(mats);
            var canyon = BuildCanyonDiorama(mats);
            var ocean = BuildOceanDiorama(mats);
            var space = BuildSpaceDiorama(mats);
            var factory = BuildFactoryDiorama(mats);
            var neon = BuildNeonDiorama(mats);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[SavePeps] Prototype art generated: " +
                      $"{pepA.name}, {pepB.name}, {plank.name}, {balloon.name}, {fan.name}, " +
                      $"{stone.name}, {leaf.name}, {umbrella.name}, {rope.name}, {bell.name}, " +
                      $"{pillow.name}, {scissors.name}, {wateringCan.name}, {bone.name}, {mirror.name}, {hairDryer.name}, " +
                      $"{diorama.name}, {wake.name}, {vines.name}, {guard.name}, {lift.name}, {beam.name}, " +
                      $"{thaw.name}, {grow.name}, {rain.name}, {canyon.name}, {ocean.name}, {space.name}, {factory.name}, {neon.name}.");
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
                ["FoliageDark"] = FoliageDark,
                ["Earth"] = EarthBase,
                ["EarthLight"] = EarthLight,
                ["Wood"] = WoodLight,
                ["Water"] = WaterBase,
                ["WaterLight"] = WaterLight,
                ["Accent"] = AccentBase,
                ["AccentLight"] = AccentLight,
                ["PepA"] = PepACoral,
                ["PepALight"] = PepALight,
                ["PepB"] = PepBMint,
                ["PepBDark"] = PepBDark,
                ["Stone"] = StoneBase,
                ["StoneLight"] = StoneLight,
                ["Cream"] = Cream,
                ["Night"] = Night,
                ["Snow"] = Snow,
                ["Ice"] = Ice,
                ["Shadow"] = Shadow,
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

            var shadow = Primitive(PrimitiveType.Sphere, "BlobShadow", root.transform, _shadowMaterial);
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

        // -------------------------------------------------------------------
        // Props
        // -------------------------------------------------------------------

        private static GameObject BuildPlank(Material wood)
        {
            var root = NewProp("plank", new Vector3(0.48f, 0.30f, 0.96f), tapCentreY: 0.04f);
            var choreo = root.transform.Find("Choreo/Visual");
            var visual = Primitive(PrimitiveType.Cube, "Board", choreo, wood);
            // Long axis along Z so it bridges the brook without needing to be
            // rotated into place.
            visual.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            visual.transform.localScale = new Vector3(0.25f, 0.07f, 0.82f);

            // Raised grain and square end bands keep the board from reading
            // as an anonymous brown cuboid at phone scale.
            for (var side = -1; side <= 1; side += 2)
            {
                var grain = Primitive(PrimitiveType.Cube, side < 0 ? "Grain_L" : "Grain_R", choreo, wood);
                grain.transform.localPosition = new Vector3(side * 0.065f, 0.073f, 0f);
                grain.transform.localScale = new Vector3(0.018f, 0.012f, 0.62f);
            }

            foreach (var z in new[] { -0.32f, 0.32f })
            {
                var band = Primitive(PrimitiveType.Cube, z < 0 ? "EndBand_Near" : "EndBand_Far", choreo, wood);
                band.transform.localPosition = new Vector3(0f, 0.076f, z);
                band.transform.localScale = new Vector3(0.27f, 0.014f, 0.035f);
            }
            return SavePrefab(root, $"{PropDir}/plank.prefab");
        }

        private static GameObject BuildBalloon(Material skin, Material ink)
        {
            var root = NewProp("balloon", new Vector3(0.36f, 0.46f, 0.36f), tapCentreY: 0.17f);
            var choreo = root.transform.Find("Choreo/Visual");

            var bulb = Primitive(PrimitiveType.Sphere, "Bulb", choreo, skin);
            bulb.transform.localPosition = new Vector3(0f, 0.24f, 0f);
            bulb.transform.localScale = new Vector3(0.23f, 0.29f, 0.23f);

            var highlight = Primitive(PrimitiveType.Sphere, "Highlight", choreo, ink);
            highlight.transform.localPosition = new Vector3(-0.065f, 0.32f, -0.105f);
            highlight.transform.localScale = new Vector3(0.025f, 0.055f, 0.012f);

            var knot = Primitive(PrimitiveType.Cube, "Knot", choreo, skin);
            knot.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            knot.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);

            var string_ = Primitive(PrimitiveType.Cube, "String", choreo, ink);
            string_.transform.localPosition = new Vector3(0f, -0.015f, 0f);
            string_.transform.localScale = new Vector3(0.008f, 0.17f, 0.008f);

            return SavePrefab(root, $"{PropDir}/balloon.prefab");
        }

        private static GameObject BuildFan(Material shell, Material blade)
        {
            var root = NewProp("fan", new Vector3(0.52f, 0.52f, 0.38f), tapCentreY: 0.20f);
            var choreo = root.transform.Find("Choreo/Visual");
            var model = Child(choreo, "Model");
            model.localScale = Vector3.one * 0.84f;

            var stand = Primitive(PrimitiveType.Cube, "Stand", model, shell);
            stand.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            stand.transform.localScale = new Vector3(0.24f, 0.07f, 0.15f);

            var neck = Primitive(PrimitiveType.Cube, "Neck", model, shell);
            neck.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            neck.transform.localScale = new Vector3(0.055f, 0.18f, 0.055f);

            BlockRing(model, "Housing", shell, new Vector3(0f, 0.28f, 0f),
                new Vector2(0.22f, 0.22f), segments: 12, thickness: 0.035f, depth: 0.035f);

            var hub = Primitive(PrimitiveType.Cylinder, "Hub", model, shell);
            hub.transform.localPosition = new Vector3(0f, 0.28f, -0.025f);
            hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            hub.transform.localScale = new Vector3(0.065f, 0.035f, 0.065f);

            // Blades live under their own transform so a Spin step can turn
            // them without moving the housing.
            var blades = Child(model, "Blades");
            blades.localPosition = new Vector3(0f, 0.28f, -0.035f);
            for (var i = 0; i < 4; i++)
            {
                var b = Primitive(PrimitiveType.Cube, $"Blade_{i}", blades, blade);
                b.transform.localRotation = Quaternion.Euler(0f, 0f, i * 90f + 18f);
                b.transform.localPosition = Quaternion.Euler(0f, 0f, i * 90f) * new Vector3(0f, 0.085f, 0f);
                b.transform.localScale = new Vector3(0.065f, 0.16f, 0.018f);
            }

            return SavePrefab(root, $"{PropDir}/fan.prefab");
        }

        /// <summary>
        /// The stone that dams the brook. Squat and heavy-looking on purpose:
        /// it has to read as something that would stay put in moving water,
        /// or the solution is not legible before the tap.
        /// </summary>
        private static GameObject BuildStone(Material stone, Material ink)
        {
            var root = NewProp("stone", new Vector3(0.38f, 0.34f, 0.38f), tapCentreY: 0.09f);
            var choreo = root.transform.Find("Choreo/Visual");

            var mass = Primitive(PrimitiveType.Sphere, "Mass", choreo, stone);
            mass.transform.localPosition = new Vector3(0f, 0.085f, 0f);
            mass.transform.localScale = new Vector3(0.24f, 0.17f, 0.21f);

            var shoulder = Primitive(PrimitiveType.Sphere, "Shoulder", choreo, stone);
            shoulder.transform.localPosition = new Vector3(0.06f, 0.05f, -0.04f);
            shoulder.transform.localScale = new Vector3(0.13f, 0.10f, 0.12f);

            var crackA = Primitive(PrimitiveType.Cube, "Crack_A", choreo, ink);
            crackA.transform.localPosition = new Vector3(-0.025f, 0.13f, -0.102f);
            crackA.transform.localRotation = Quaternion.Euler(0f, 0f, -28f);
            crackA.transform.localScale = new Vector3(0.012f, 0.09f, 0.009f);

            var crackB = Primitive(PrimitiveType.Cube, "Crack_B", choreo, ink);
            crackB.transform.localPosition = new Vector3(0.012f, 0.095f, -0.108f);
            crackB.transform.localRotation = Quaternion.Euler(0f, 0f, 32f);
            crackB.transform.localScale = new Vector3(0.012f, 0.055f, 0.009f);

            return SavePrefab(root, $"{PropDir}/stone.prefab");
        }

        /// <summary>
        /// A big flat leaf — the ferry. Wide enough to obviously carry a Pep,
        /// which is the whole clue.
        /// </summary>
        private static GameObject BuildLeaf(Material blade, Material stem)
        {
            var root = NewProp("leaf", new Vector3(0.5f, 0.24f, 0.56f), tapCentreY: 0.03f);
            var choreo = root.transform.Find("Choreo/Visual");

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
            var choreo = root.transform.Find("Choreo/Visual");

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
        /// A coil of rope. Reads as "this is long" from a silhouette that is
        /// entirely compact, which is the whole trick — the player has to
        /// believe it spans the canyon before they tap it.
        /// </summary>
        private static GameObject BuildRope(Material fibre)
        {
            var root = NewProp("rope", new Vector3(0.36f, 0.26f, 0.36f), tapCentreY: 0.06f);
            var choreo = root.transform.Find("Choreo/Visual");

            var radii = new[] { 0.22f, 0.17f, 0.12f };
            for (var i = 0; i < radii.Length; i++)
            {
                var loop = Primitive(PrimitiveType.Cylinder, $"Coil_{i}", choreo, fibre);
                loop.transform.localPosition = new Vector3(0f, 0.03f + i * 0.032f, 0f);
                loop.transform.localScale = new Vector3(radii[i], 0.016f, radii[i]);
            }

            return SavePrefab(root, $"{PropDir}/rope.prefab");
        }

        /// <summary>A handled, flared bell with a visible clapper.</summary>
        private static GameObject BuildBell(Material brass, Material ink)
        {
            var root = NewProp("bell", new Vector3(0.46f, 0.54f, 0.38f), tapCentreY: 0.22f);
            var choreo = root.transform.Find("Choreo/Visual");

            var dome = Primitive(PrimitiveType.Sphere, "Dome", choreo, brass);
            dome.transform.localPosition = new Vector3(0f, 0.27f, 0f);
            dome.transform.localScale = new Vector3(0.17f, 0.16f, 0.16f);

            for (var i = 0; i < 3; i++)
            {
                var skirt = Primitive(PrimitiveType.Cylinder, $"Skirt_{i}", choreo, brass);
                skirt.transform.localPosition = new Vector3(0f, 0.21f - i * 0.045f, 0f);
                var radius = 0.16f + i * 0.035f;
                skirt.transform.localScale = new Vector3(radius, 0.025f, radius);
            }

            var rim = Primitive(PrimitiveType.Cylinder, "Rim", choreo, ink);
            rim.transform.localPosition = new Vector3(0f, 0.105f, 0f);
            rim.transform.localScale = new Vector3(0.235f, 0.018f, 0.205f);

            var clapper = Primitive(PrimitiveType.Sphere, "Clapper", choreo, ink);
            clapper.transform.localPosition = new Vector3(0f, 0.075f, -0.015f);
            clapper.transform.localScale = Vector3.one * 0.065f;

            BlockRing(choreo, "Handle", brass, new Vector3(0f, 0.40f, 0f),
                new Vector2(0.08f, 0.075f), segments: 8, thickness: 0.025f, depth: 0.045f);

            return SavePrefab(root, $"{PropDir}/bell.prefab");
        }

        private static GameObject BuildPillow(Material cloth, Material trim)
        {
            var root = NewProp("pillow", new Vector3(0.52f, 0.30f, 0.46f), tapCentreY: 0.07f);
            var choreo = root.transform.Find("Choreo/Visual");

            // A rounded sphere collapsed into an oval read as an egg on the
            // Pixel 4. A rectangular padded core, four soft lobes and visible
            // piping preserve the pillow silhouette from this steep camera.
            var cushion = Primitive(PrimitiveType.Cube, "Cushion", choreo, cloth);
            cushion.transform.localPosition = new Vector3(0f, 0.065f, 0f);
            cushion.transform.localScale = new Vector3(0.40f, 0.10f, 0.28f);

            foreach (var (x, z) in new[] { (-0.16f, -0.11f), (0.16f, -0.11f), (-0.16f, 0.11f), (0.16f, 0.11f) })
            {
                var lobe = Primitive(PrimitiveType.Sphere, "Puff", choreo, cloth);
                lobe.transform.localPosition = new Vector3(x, 0.085f, z);
                lobe.transform.localScale = new Vector3(0.18f, 0.10f, 0.15f);
            }

            foreach (var z in new[] { -0.135f, 0.135f })
            {
                var seam = Primitive(PrimitiveType.Cube, "Piping_X", choreo, trim);
                seam.transform.localPosition = new Vector3(0f, 0.122f, z);
                seam.transform.localScale = new Vector3(0.36f, 0.012f, 0.014f);
            }
            foreach (var x in new[] { -0.195f, 0.195f })
            {
                var seam = Primitive(PrimitiveType.Cube, "Piping_Z", choreo, trim);
                seam.transform.localPosition = new Vector3(x, 0.122f, 0f);
                seam.transform.localScale = new Vector3(0.014f, 0.012f, 0.25f);
            }

            var button = Primitive(PrimitiveType.Sphere, "Tuft", choreo, trim);
            button.transform.localPosition = new Vector3(0f, 0.125f, -0.01f);
            button.transform.localScale = new Vector3(0.035f, 0.018f, 0.035f);

            return SavePrefab(root, $"{PropDir}/pillow.prefab");
        }

        private static GameObject BuildScissors(Material metal, Material handles, Material ink)
        {
            var root = NewProp("scissors", new Vector3(0.50f, 0.58f, 0.34f), tapCentreY: 0.22f);
            var choreo = root.transform.Find("Choreo/Visual");

            BlockRing(choreo, "Handle_L", handles, new Vector3(-0.095f, 0.085f, 0f),
                new Vector2(0.075f, 0.09f), segments: 8, thickness: 0.028f, depth: 0.038f);
            BlockRing(choreo, "Handle_R", handles, new Vector3(0.095f, 0.085f, 0f),
                new Vector2(0.075f, 0.09f), segments: 8, thickness: 0.028f, depth: 0.038f);

            for (var side = -1; side <= 1; side += 2)
            {
                var blade = Primitive(PrimitiveType.Cube, side < 0 ? "Blade_L" : "Blade_R", choreo, metal);
                blade.transform.localPosition = new Vector3(side * 0.065f, 0.30f, 0f);
                blade.transform.localRotation = Quaternion.Euler(0f, 0f, side * 13f);
                blade.transform.localScale = new Vector3(0.055f, 0.31f, 0.035f);
            }

            var pivot = Primitive(PrimitiveType.Cylinder, "Pivot", choreo, ink);
            pivot.transform.localPosition = new Vector3(0f, 0.18f, -0.025f);
            pivot.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            pivot.transform.localScale = new Vector3(0.045f, 0.025f, 0.045f);

            return SavePrefab(root, $"{PropDir}/scissors.prefab");
        }

        private static GameObject BuildWateringCan(Material body, Material trim)
        {
            var root = NewProp("watering_can", new Vector3(0.58f, 0.50f, 0.40f), tapCentreY: 0.18f);
            var choreo = root.transform.Find("Choreo/Visual");

            var can = Primitive(PrimitiveType.Cylinder, "Can", choreo, body);
            can.transform.localPosition = new Vector3(-0.04f, 0.16f, 0f);
            can.transform.localScale = new Vector3(0.18f, 0.17f, 0.18f);

            BlockRing(choreo, "Handle", trim, new Vector3(-0.04f, 0.28f, 0.035f),
                new Vector2(0.20f, 0.17f), segments: 10, thickness: 0.028f, depth: 0.035f);

            var spout = Primitive(PrimitiveType.Cube, "Spout", choreo, body);
            spout.transform.localPosition = new Vector3(0.22f, 0.20f, 0f);
            spout.transform.localRotation = Quaternion.Euler(0f, 0f, -24f);
            spout.transform.localScale = new Vector3(0.30f, 0.07f, 0.08f);

            var rose = Primitive(PrimitiveType.Cylinder, "Rose", choreo, trim);
            rose.transform.localPosition = new Vector3(0.36f, 0.265f, 0f);
            rose.transform.localRotation = Quaternion.Euler(0f, 0f, -24f);
            rose.transform.localScale = new Vector3(0.09f, 0.025f, 0.09f);

            return SavePrefab(root, $"{PropDir}/watering_can.prefab");
        }

        private static GameObject BuildBone(Material bone, Material shadow)
        {
            var root = NewProp("bone", new Vector3(0.58f, 0.30f, 0.36f), tapCentreY: 0.09f);
            var choreo = root.transform.Find("Choreo/Visual");

            var shaft = Primitive(PrimitiveType.Cylinder, "Shaft", choreo, bone);
            shaft.transform.localPosition = new Vector3(0f, 0.10f, 0f);
            shaft.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            shaft.transform.localScale = new Vector3(0.065f, 0.19f, 0.065f);

            foreach (var x in new[] { -0.20f, 0.20f })
            foreach (var y in new[] { 0.055f, 0.145f })
            {
                var knob = Primitive(PrimitiveType.Sphere, "Knob", choreo, bone);
                knob.transform.localPosition = new Vector3(x, y, 0f);
                knob.transform.localScale = Vector3.one * 0.105f;
            }

            var underside = Primitive(PrimitiveType.Cube, "Underside", choreo, shadow);
            underside.transform.localPosition = new Vector3(0f, 0.055f, 0.035f);
            underside.transform.localScale = new Vector3(0.28f, 0.018f, 0.035f);

            return SavePrefab(root, $"{PropDir}/bone.prefab");
        }

        private static GameObject BuildMirror(Material glass, Material frame, Material highlight)
        {
            var root = NewProp("mirror", new Vector3(0.50f, 0.62f, 0.36f), tapCentreY: 0.25f);
            var choreo = root.transform.Find("Choreo/Visual");

            var handle = Primitive(PrimitiveType.Cube, "Handle", choreo, frame);
            handle.transform.localPosition = new Vector3(0f, 0.10f, 0.025f);
            handle.transform.localScale = new Vector3(0.075f, 0.22f, 0.075f);

            var back = Primitive(PrimitiveType.Sphere, "Frame", choreo, frame);
            back.transform.localPosition = new Vector3(0f, 0.34f, 0.025f);
            back.transform.localScale = new Vector3(0.25f, 0.30f, 0.065f);

            var face = Primitive(PrimitiveType.Sphere, "Glass", choreo, glass);
            face.transform.localPosition = new Vector3(0f, 0.34f, -0.018f);
            face.transform.localScale = new Vector3(0.205f, 0.25f, 0.028f);

            var gleam = Primitive(PrimitiveType.Cube, "Gleam", choreo, highlight);
            gleam.transform.localPosition = new Vector3(-0.065f, 0.41f, -0.04f);
            gleam.transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
            gleam.transform.localScale = new Vector3(0.022f, 0.13f, 0.012f);

            return SavePrefab(root, $"{PropDir}/mirror.prefab");
        }

        /// <summary>A broad barrel, narrowing nozzle, vent and angled grip.</summary>
        private static GameObject BuildHairDryer(Material body, Material trim, Material ink, Material heat)
        {
            var root = NewProp("hair_dryer", new Vector3(0.58f, 0.56f, 0.38f), tapCentreY: 0.22f);
            var visual = root.transform.Find("Choreo/Visual");

            var barrel = Primitive(PrimitiveType.Cylinder, "Barrel", visual, body);
            barrel.transform.localPosition = new Vector3(0.02f, 0.31f, 0f);
            barrel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            barrel.transform.localScale = new Vector3(0.14f, 0.19f, 0.14f);

            var nozzle = Primitive(PrimitiveType.Cylinder, "Nozzle", visual, trim);
            nozzle.transform.localPosition = new Vector3(-0.22f, 0.31f, 0f);
            nozzle.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            nozzle.transform.localScale = new Vector3(0.10f, 0.11f, 0.10f);
            var mouth = Primitive(PrimitiveType.Cylinder, "NozzleMouth", visual, ink);
            mouth.transform.localPosition = new Vector3(-0.335f, 0.31f, 0f);
            mouth.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            mouth.transform.localScale = new Vector3(0.078f, 0.018f, 0.078f);

            var vent = Primitive(PrimitiveType.Cylinder, "RearVent", visual, ink);
            vent.transform.localPosition = new Vector3(0.215f, 0.31f, 0f);
            vent.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            vent.transform.localScale = new Vector3(0.105f, 0.018f, 0.105f);
            foreach (var angle in new[] { 0f, 45f, 90f, 135f })
            {
                var slot = Primitive(PrimitiveType.Cube, "VentSlot", visual, trim);
                slot.transform.localPosition = new Vector3(0.236f, 0.31f, -0.01f);
                slot.transform.localRotation = Quaternion.Euler(angle, 90f, 0f);
                slot.transform.localScale = new Vector3(0.015f, 0.085f, 0.012f);
            }

            var grip = Primitive(PrimitiveType.Cube, "Grip", visual, body);
            grip.transform.localPosition = new Vector3(0.08f, 0.13f, 0f);
            grip.transform.localRotation = Quaternion.Euler(0f, 0f, -14f);
            grip.transform.localScale = new Vector3(0.10f, 0.27f, 0.11f);
            var button = Primitive(PrimitiveType.Cube, "HeatButton", visual, heat);
            button.transform.localPosition = new Vector3(0.005f, 0.17f, -0.065f);
            button.transform.localRotation = Quaternion.Euler(0f, 0f, -14f);
            button.transform.localScale = new Vector3(0.035f, 0.055f, 0.025f);

            // Three warm dashes make function visible even before the player
            // has learned the prop's silhouette.
            for (var i = 0; i < 3; i++)
            {
                var dash = Primitive(PrimitiveType.Cube, "WarmAir", visual, heat);
                dash.transform.localPosition = new Vector3(-0.41f - i * 0.065f, 0.31f + (i - 1) * 0.045f, 0f);
                dash.transform.localScale = new Vector3(0.055f, 0.016f, 0.018f);
            }

            return SavePrefab(root, $"{PropDir}/hair_dryer.prefab");
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
            var visual = Child(choreo, "Visual");
            root.AddComponent<ChoicePresentation>().Configure(visual);

            var shadow = Primitive(PrimitiveType.Sphere, "BlobShadow", root.transform, _shadowMaterial);
            shadow.transform.localPosition = new Vector3(0f, 0.008f, 0.025f);
            shadow.transform.localScale = new Vector3(
                Mathf.Max(0.16f, tapSize.x * 0.58f),
                0.012f,
                Mathf.Max(0.13f, tapSize.z * 0.48f));
            shadow.AddComponent<BlobShadow>().Configure(choreo, shadow.GetComponent<Renderer>());
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
            foreach (var (x, z, width) in new[] { (-0.38f, -0.10f, 0.30f), (0.28f, 0.08f, 0.36f), (-0.05f, 0.22f, 0.22f) })
            {
                var ripple = Primitive(PrimitiveType.Cube, "Ripple", water.transform.parent, mats["WaterLight"]);
                ripple.transform.localPosition = new Vector3(x, 0.078f, z);
                ripple.transform.localRotation = Quaternion.Euler(0f, x * 22f, 0f);
                ripple.transform.localScale = new Vector3(width, 0.012f, 0.026f);
            }

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

            FinishDiorama(root, mats, DioramaMood.Meadow);
            return SavePrefab(root, $"{EnvDir}/Diorama_Brook.prefab");
        }

        // -------------------------------------------------------------------
        // Wake — diagonal gate, sleeping helper, all choices foreground
        // -------------------------------------------------------------------

        private static GameObject BuildWakeDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Wake");

            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Earth"]);
            slab.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.16f, 3.4f);

            var lawn = Primitive(PrimitiveType.Cube, "Garden", root.transform, mats["FoliageLight"]);
            lawn.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            lawn.transform.localScale = new Vector3(1.35f, 0.15f, 3.15f);

            var path = Primitive(PrimitiveType.Cube, "Path", root.transform, mats["Cream"]);
            path.transform.localPosition = new Vector3(-0.04f, 0.16f, 0.10f);
            path.transform.localRotation = Quaternion.Euler(0f, -18f, 0f);
            path.transform.localScale = new Vector3(0.44f, 0.025f, 2.15f);

            // A fence crosses the route, but leaves a clearly gated opening.
            foreach (var x in new[] { -0.62f, -0.43f, 0.36f, 0.60f })
            {
                var post = Primitive(PrimitiveType.Cube, "FencePost", root.transform, mats["Wood"]);
                post.transform.localPosition = new Vector3(x, 0.34f, 0.18f);
                post.transform.localScale = new Vector3(0.055f, 0.45f, 0.055f);
            }

            foreach (var x in new[] { -0.52f, 0.48f })
            {
                var rail = Primitive(PrimitiveType.Cube, "FenceRail", root.transform, mats["Wood"]);
                rail.transform.localPosition = new Vector3(x, 0.34f, 0.18f);
                rail.transform.localScale = new Vector3(0.30f, 0.055f, 0.055f);
            }

            var gate = Mover(root.transform, "Gate");
            foreach (var x in new[] { -0.22f, -0.06f, 0.10f, 0.26f })
            {
                var bar = Primitive(PrimitiveType.Cube, "GateBar", gate, mats["PepA"]);
                bar.transform.localPosition = new Vector3(x, 0.36f, 0.17f);
                bar.transform.localScale = new Vector3(0.045f, 0.44f, 0.05f);
            }
            foreach (var y in new[] { 0.23f, 0.45f })
            {
                var cross = Primitive(PrimitiveType.Cube, "GateCrossbar", gate, mats["PepA"]);
                cross.transform.localPosition = new Vector3(0.02f, y, 0.17f);
                cross.transform.localScale = new Vector3(0.52f, 0.045f, 0.055f);
            }

            // A toy robot sleeps beside the lever. Open eyes are underneath
            // a movable sleep mask, so Hide reveals wakefulness with no
            // rescue-specific behaviour.
            var helper = Mover(root.transform, "Helper");
            var helperBody = Primitive(PrimitiveType.Cube, "Body", helper, mats["Stone"]);
            helperBody.transform.localPosition = new Vector3(0.47f, 0.28f, 0.30f);
            helperBody.transform.localScale = new Vector3(0.29f, 0.28f, 0.22f);
            var helperHead = Primitive(PrimitiveType.Cube, "Head", helper, mats["Cream"]);
            helperHead.transform.localPosition = new Vector3(0.47f, 0.48f, 0.28f);
            helperHead.transform.localScale = new Vector3(0.27f, 0.19f, 0.22f);
            foreach (var x in new[] { 0.41f, 0.53f })
            {
                var eye = Primitive(PrimitiveType.Sphere, "OpenEye", helper, mats["Ink"]);
                eye.transform.localPosition = new Vector3(x, 0.49f, 0.165f);
                eye.transform.localScale = Vector3.one * 0.045f;
            }
            var antenna = Primitive(PrimitiveType.Sphere, "Antenna", helper, mats["Accent"]);
            antenna.transform.localPosition = new Vector3(0.47f, 0.63f, 0.28f);
            antenna.transform.localScale = Vector3.one * 0.07f;

            var sleepMask = Mover(root.transform, "SleepMask");
            foreach (var x in new[] { 0.41f, 0.53f })
            {
                var patch = Primitive(PrimitiveType.Cube, "EyePatch", sleepMask, mats["Cream"]);
                patch.transform.localPosition = new Vector3(x, 0.49f, 0.145f);
                patch.transform.localScale = new Vector3(0.09f, 0.075f, 0.022f);
                var lid = Primitive(PrimitiveType.Cube, "ClosedEye", sleepMask, mats["Ink"]);
                lid.transform.localPosition = new Vector3(x, 0.49f, 0.128f);
                lid.transform.localRotation = Quaternion.Euler(0f, 0f, x < 0.47f ? -8f : 8f);
                lid.transform.localScale = new Vector3(0.07f, 0.018f, 0.018f);
            }

            var leverBase = Primitive(PrimitiveType.Cylinder, "LeverBase", root.transform, mats["Stone"]);
            leverBase.transform.localPosition = new Vector3(0.67f, 0.19f, 0.20f);
            leverBase.transform.localScale = new Vector3(0.11f, 0.04f, 0.11f);
            var lever = Primitive(PrimitiveType.Cube, "Lever", root.transform, mats["Accent"]);
            lever.transform.localPosition = new Vector3(0.62f, 0.32f, 0.20f);
            lever.transform.localRotation = Quaternion.Euler(0f, 0f, 24f);
            lever.transform.localScale = new Vector3(0.04f, 0.28f, 0.04f);

            var zzz = Mover(root.transform, "Zzz");
            AddZ(zzz, mats["Ink"], new Vector3(0.29f, 0.69f, 0.24f), 0.09f);
            AddZ(zzz, mats["Ink"], new Vector3(0.43f, 0.81f, 0.24f), 0.12f);
            AddZ(zzz, mats["Ink"], new Vector3(0.60f, 0.96f, 0.24f), 0.15f);

            Anchor(root.transform, "Anchor_PepA", new Vector3(0.35f, 0.15f, -0.48f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(-0.30f, 0.15f, 0.72f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(0.10f, 0.15f, -0.22f));
            Anchor(root.transform, "Slot_1", new Vector3(-0.40f, 0.15f, -1.18f));
            Anchor(root.transform, "Slot_2", new Vector3(0.40f, 0.15f, -1.18f));
            Anchor(root.transform, "Slot_3", new Vector3(0f, 0.15f, -0.92f));

            FinishDiorama(root, mats, DioramaMood.Garden);
            return SavePrefab(root, $"{EnvDir}/Diorama_Wake.prefab");
        }

        // -------------------------------------------------------------------
        // Free — one Pep visibly behind a vertical vine cage
        // -------------------------------------------------------------------

        private static GameObject BuildVinesDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Vines");

            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Earth"]);
            slab.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.16f, 3.4f);
            var garden = Primitive(PrimitiveType.Cube, "Garden", root.transform, mats["Foliage"]);
            garden.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            garden.transform.localScale = new Vector3(1.35f, 0.15f, 3.15f);

            var backWall = Primitive(PrimitiveType.Cube, "TrellisTop", root.transform, mats["Wood"]);
            backWall.transform.localPosition = new Vector3(0.23f, 0.62f, 0.72f);
            backWall.transform.localScale = new Vector3(0.80f, 0.07f, 0.07f);
            foreach (var x in new[] { -0.14f, 0.60f })
            {
                var post = Primitive(PrimitiveType.Cube, "TrellisPost", root.transform, mats["Wood"]);
                post.transform.localPosition = new Vector3(x, 0.39f, 0.72f);
                post.transform.localScale = new Vector3(0.07f, 0.54f, 0.07f);
            }

            var vines = Mover(root.transform, "Vines");
            foreach (var x in new[] { -0.08f, 0.08f, 0.24f, 0.40f, 0.56f })
            {
                var stalk = Primitive(PrimitiveType.Cylinder, "Vine", vines, mats["FoliageLight"]);
                stalk.transform.localPosition = new Vector3(x, 0.43f, 0.57f);
                stalk.transform.localScale = new Vector3(0.035f, 0.36f, 0.035f);
            }
            foreach (var angle in new[] { -32f, 32f })
            {
                var cross = Primitive(PrimitiveType.Cube, "CrossVine", vines, mats["FoliageLight"]);
                cross.transform.localPosition = new Vector3(0.24f, 0.43f, 0.54f);
                cross.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                cross.transform.localScale = new Vector3(0.055f, 0.78f, 0.045f);
            }
            foreach (var (x, y) in new[] { (0.02f, 0.30f), (0.28f, 0.56f), (0.50f, 0.38f) })
            {
                var leaf = Primitive(PrimitiveType.Sphere, "VineLeaf", vines, mats["FoliageLight"]);
                leaf.transform.localPosition = new Vector3(x, y, 0.49f);
                leaf.transform.localScale = new Vector3(0.11f, 0.06f, 0.025f);
            }

            Anchor(root.transform, "Anchor_PepA", new Vector3(-0.38f, 0.15f, -0.36f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0.26f, 0.15f, 0.62f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(-0.08f, 0.15f, -0.04f));
            Anchor(root.transform, "Slot_1", new Vector3(-0.32f, 0.15f, -1.20f));
            Anchor(root.transform, "Slot_2", new Vector3(0.40f, 0.15f, -1.20f));
            Anchor(root.transform, "Slot_3", new Vector3(0f, 0.15f, -0.86f));

            FinishDiorama(root, mats, DioramaMood.Garden);
            return SavePrefab(root, $"{EnvDir}/Diorama_Vines.prefab");
        }

        // -------------------------------------------------------------------
        // Distract — a dog guards a diagonal garden opening
        // -------------------------------------------------------------------

        private static GameObject BuildGuardDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Guard");

            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Earth"]);
            slab.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.16f, 3.4f);
            var lawn = Primitive(PrimitiveType.Cube, "Lawn", root.transform, mats["FoliageLight"]);
            lawn.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            lawn.transform.localScale = new Vector3(1.35f, 0.15f, 3.15f);

            var path = Primitive(PrimitiveType.Cube, "DiagonalPath", root.transform, mats["Cream"]);
            path.transform.localPosition = new Vector3(0f, 0.16f, 0f);
            path.transform.localRotation = Quaternion.Euler(0f, -17f, 0f);
            path.transform.localScale = new Vector3(0.46f, 0.025f, 2.45f);

            foreach (var (x, width) in new[] { (-0.48f, 0.48f), (0.48f, 0.34f) })
            {
                var hedge = Primitive(PrimitiveType.Cube, "Hedge", root.transform, mats["Foliage"]);
                hedge.transform.localPosition = new Vector3(x, 0.34f, 0.60f);
                hedge.transform.localScale = new Vector3(width, 0.38f, 0.25f);
            }

            var dog = Mover(root.transform, "Guard");
            var body = Primitive(PrimitiveType.Sphere, "DogBody", dog, mats["Earth"]);
            body.transform.localPosition = new Vector3(-0.08f, 0.30f, 0.16f);
            body.transform.localScale = new Vector3(0.34f, 0.22f, 0.27f);
            var head = Primitive(PrimitiveType.Sphere, "DogHead", dog, mats["Wood"]);
            head.transform.localPosition = new Vector3(-0.08f, 0.48f, -0.01f);
            head.transform.localScale = new Vector3(0.27f, 0.25f, 0.24f);
            var muzzle = Primitive(PrimitiveType.Sphere, "Muzzle", dog, mats["Cream"]);
            muzzle.transform.localPosition = new Vector3(-0.08f, 0.43f, -0.145f);
            muzzle.transform.localScale = new Vector3(0.18f, 0.12f, 0.10f);
            var nose = Primitive(PrimitiveType.Sphere, "Nose", dog, mats["Ink"]);
            nose.transform.localPosition = new Vector3(-0.08f, 0.46f, -0.205f);
            nose.transform.localScale = Vector3.one * 0.07f;
            foreach (var x in new[] { -0.16f, 0f })
            {
                var eye = Primitive(PrimitiveType.Sphere, "DogEye", dog, mats["Ink"]);
                eye.transform.localPosition = new Vector3(x, 0.54f, -0.13f);
                eye.transform.localScale = Vector3.one * 0.042f;
            }
            foreach (var x in new[] { -0.25f, 0.09f })
            {
                var ear = Primitive(PrimitiveType.Cube, "DogEar", dog, mats["Ink"]);
                ear.transform.localPosition = new Vector3(x, 0.53f, 0f);
                ear.transform.localRotation = Quaternion.Euler(0f, 0f, x < -0.08f ? -24f : 24f);
                ear.transform.localScale = new Vector3(0.09f, 0.22f, 0.08f);
            }
            foreach (var x in new[] { -0.20f, 0.04f })
            {
                var paw = Primitive(PrimitiveType.Cube, "DogPaw", dog, mats["Wood"]);
                paw.transform.localPosition = new Vector3(x, 0.18f, 0.03f);
                paw.transform.localScale = new Vector3(0.10f, 0.12f, 0.18f);
            }
            var tail = Primitive(PrimitiveType.Cube, "Tail", dog, mats["Earth"]);
            tail.transform.localPosition = new Vector3(0.18f, 0.37f, 0.27f);
            tail.transform.localRotation = Quaternion.Euler(28f, 0f, -36f);
            tail.transform.localScale = new Vector3(0.08f, 0.30f, 0.08f);
            var collar = Primitive(PrimitiveType.Cube, "Collar", dog, mats["PepA"]);
            collar.transform.localPosition = new Vector3(-0.08f, 0.39f, -0.08f);
            collar.transform.localScale = new Vector3(0.24f, 0.045f, 0.055f);
            var tag = Primitive(PrimitiveType.Sphere, "Tag", dog, mats["Accent"]);
            tag.transform.localPosition = new Vector3(-0.08f, 0.365f, -0.145f);
            tag.transform.localScale = Vector3.one * 0.052f;

            Anchor(root.transform, "Anchor_PepA", new Vector3(0.36f, 0.15f, -0.43f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(-0.33f, 0.15f, 0.78f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(0.14f, 0.15f, -0.12f));
            Anchor(root.transform, "FoodSpot", new Vector3(0.52f, 0.15f, 0.24f));
            Anchor(root.transform, "Slot_1", new Vector3(-0.40f, 0.15f, -1.20f));
            Anchor(root.transform, "Slot_2", new Vector3(0f, 0.15f, -0.94f));
            Anchor(root.transform, "Slot_3", new Vector3(0.40f, 0.15f, -1.20f));

            FinishDiorama(root, mats, DioramaMood.Garden);
            return SavePrefab(root, $"{EnvDir}/Diorama_Guard.prefab");
        }

        // -------------------------------------------------------------------
        // Balance — one Pep below the other on a counterweight lift
        // -------------------------------------------------------------------

        private static GameObject BuildLiftDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Lift");

            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Earth"]);
            slab.transform.localPosition = new Vector3(0f, -0.12f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.24f, 3.4f);
            var floor = Primitive(PrimitiveType.Cube, "Workshop", root.transform, mats["Stone"]);
            floor.transform.localPosition = new Vector3(0f, 0.035f, 0f);
            floor.transform.localScale = new Vector3(1.35f, 0.07f, 3.15f);

            var upperDeck = Primitive(PrimitiveType.Cube, "UpperDeck", root.transform, mats["FoliageLight"]);
            upperDeck.transform.localPosition = new Vector3(-0.35f, 0.21f, -0.02f);
            upperDeck.transform.localScale = new Vector3(0.66f, 0.36f, 1.55f);
            var pit = Primitive(PrimitiveType.Cube, "LiftPit", root.transform, mats["Ink"]);
            pit.transform.localPosition = new Vector3(0.30f, 0.08f, 0.43f);
            pit.transform.localScale = new Vector3(0.54f, 0.06f, 0.62f);

            var lift = Mover(root.transform, "LiftPlatform");
            var liftDeck = Primitive(PrimitiveType.Cube, "LiftDeck", lift, mats["Accent"]);
            liftDeck.transform.localPosition = new Vector3(0.30f, 0.14f, 0.43f);
            liftDeck.transform.localScale = new Vector3(0.48f, 0.10f, 0.48f);
            foreach (var x in new[] { 0.10f, 0.50f })
            {
                var rail = Primitive(PrimitiveType.Cube, "LiftRail", lift, mats["Cream"]);
                rail.transform.localPosition = new Vector3(x, 0.38f, 0.57f);
                rail.transform.localScale = new Vector3(0.035f, 0.46f, 0.035f);
            }

            var counterweight = Mover(root.transform, "Counterweight");
            var tray = Primitive(PrimitiveType.Cube, "WeightTray", counterweight, mats["Wood"]);
            tray.transform.localPosition = new Vector3(-0.52f, 0.69f, 0.48f);
            tray.transform.localScale = new Vector3(0.40f, 0.08f, 0.42f);
            foreach (var x in new[] { -0.70f, -0.34f })
            {
                var lip = Primitive(PrimitiveType.Cube, "TrayLip", counterweight, mats["Wood"]);
                lip.transform.localPosition = new Vector3(x, 0.77f, 0.48f);
                lip.transform.localScale = new Vector3(0.04f, 0.17f, 0.42f);
            }

            foreach (var x in new[] { -0.52f, 0.30f })
            {
                var rope = Primitive(PrimitiveType.Cube, "LiftRope", root.transform, mats["Cream"]);
                rope.transform.localPosition = new Vector3(x, 0.86f, 0.48f);
                rope.transform.localScale = new Vector3(0.026f, x < 0f ? 0.38f : 0.95f, 0.026f);
            }
            var gantry = Primitive(PrimitiveType.Cube, "Gantry", root.transform, mats["Wood"]);
            gantry.transform.localPosition = new Vector3(-0.11f, 1.02f, 0.48f);
            gantry.transform.localScale = new Vector3(1.05f, 0.07f, 0.08f);
            foreach (var x in new[] { -0.70f, 0.50f })
            {
                var tower = Primitive(PrimitiveType.Cube, "Tower", root.transform, mats["Wood"]);
                tower.transform.localPosition = new Vector3(x, 0.58f, 0.55f);
                tower.transform.localScale = new Vector3(0.07f, 0.95f, 0.08f);
            }

            var pulley = Mover(root.transform, "Pulley");
            BlockRing(pulley, "PulleyWheel", mats["Accent"], new Vector3(-0.11f, 1.02f, 0.42f),
                new Vector2(0.17f, 0.17f), segments: 10, thickness: 0.035f, depth: 0.05f);
            var axle = Primitive(PrimitiveType.Cylinder, "Axle", pulley, mats["Ink"]);
            axle.transform.localPosition = new Vector3(-0.11f, 1.02f, 0.39f);
            axle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            axle.transform.localScale = new Vector3(0.045f, 0.04f, 0.045f);

            Anchor(root.transform, "Anchor_PepA", new Vector3(-0.30f, 0.39f, -0.24f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0.30f, 0.20f, 0.43f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(-0.08f, 0.39f, -0.05f));
            Anchor(root.transform, "Slot_1", new Vector3(0.40f, 0.10f, -1.18f));
            Anchor(root.transform, "Slot_2", new Vector3(-0.40f, 0.10f, -1.18f));
            Anchor(root.transform, "Slot_3", new Vector3(0f, 0.10f, -0.90f));

            FinishDiorama(root, mats, DioramaMood.Workshop);
            return SavePrefab(root, $"{EnvDir}/Diorama_Lift.prefab");
        }

        // -------------------------------------------------------------------
        // Reflect — an angled beam, dark sensor and side gate
        // -------------------------------------------------------------------

        private static GameObject BuildBeamDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Beam");

            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Earth"]);
            slab.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.16f, 3.4f);
            var night = Primitive(PrimitiveType.Cube, "NightGarden", root.transform, mats["Night"]);
            night.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            night.transform.localScale = new Vector3(1.35f, 0.15f, 3.15f);

            var path = Primitive(PrimitiveType.Cube, "SensorPath", root.transform, mats["Stone"]);
            path.transform.localPosition = new Vector3(0.07f, 0.16f, 0.08f);
            path.transform.localRotation = Quaternion.Euler(0f, -20f, 0f);
            path.transform.localScale = new Vector3(0.42f, 0.025f, 2.2f);

            var lampBase = Primitive(PrimitiveType.Cylinder, "LampBase", root.transform, mats["Stone"]);
            lampBase.transform.localPosition = new Vector3(-0.53f, 0.20f, 0.30f);
            lampBase.transform.localScale = new Vector3(0.16f, 0.05f, 0.16f);
            var lampPost = Primitive(PrimitiveType.Cube, "LampPost", root.transform, mats["Stone"]);
            lampPost.transform.localPosition = new Vector3(-0.53f, 0.36f, 0.30f);
            lampPost.transform.localScale = new Vector3(0.06f, 0.32f, 0.06f);
            var lamp = Primitive(PrimitiveType.Sphere, "Lamp", root.transform, mats["AccentLight"]);
            lamp.transform.localPosition = new Vector3(-0.49f, 0.53f, 0.24f);
            lamp.transform.localScale = new Vector3(0.18f, 0.14f, 0.14f);

            var beamIn = Mover(root.transform, "BeamIn");
            var incoming = Primitive(PrimitiveType.Cube, "IncomingLight", beamIn, mats["AccentLight"]);
            incoming.transform.localPosition = new Vector3(-0.25f, 0.32f, 0.15f);
            incoming.transform.localRotation = Quaternion.Euler(0f, 0f, -13f);
            incoming.transform.localScale = new Vector3(0.54f, 0.045f, 0.045f);

            var pedestal = Primitive(PrimitiveType.Cylinder, "MirrorPedestal", root.transform, mats["Cream"]);
            pedestal.transform.localPosition = new Vector3(0.02f, 0.20f, 0.07f);
            pedestal.transform.localScale = new Vector3(0.15f, 0.06f, 0.15f);

            var sensorHousing = Primitive(PrimitiveType.Cylinder, "SensorHousing", root.transform, mats["Stone"]);
            sensorHousing.transform.localPosition = new Vector3(0.50f, 0.43f, 0.46f);
            sensorHousing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            sensorHousing.transform.localScale = new Vector3(0.18f, 0.05f, 0.18f);
            var sensorDark = Primitive(PrimitiveType.Sphere, "SensorDark", root.transform, mats["Ink"]);
            sensorDark.transform.localPosition = new Vector3(0.50f, 0.43f, 0.355f);
            sensorDark.transform.localScale = new Vector3(0.11f, 0.11f, 0.04f);

            var beamBounce = Mover(root.transform, "BeamBounce");
            var bounced = Primitive(PrimitiveType.Cube, "ReflectedLight", beamBounce, mats["AccentLight"]);
            bounced.transform.localPosition = new Vector3(0.26f, 0.35f, 0.25f);
            bounced.transform.localRotation = Quaternion.Euler(0f, 0f, 28f);
            bounced.transform.localScale = new Vector3(0.58f, 0.045f, 0.045f);
            beamBounce.GetComponent<AnimTarget>().SetVisibleAtRest(false);

            var sensorGlow = Mover(root.transform, "SensorGlow");
            var glow = Primitive(PrimitiveType.Sphere, "Glow", sensorGlow, mats["AccentLight"]);
            glow.transform.localPosition = new Vector3(0.50f, 0.43f, 0.33f);
            glow.transform.localScale = new Vector3(0.13f, 0.13f, 0.045f);
            sensorGlow.GetComponent<AnimTarget>().SetVisibleAtRest(false);

            var gate = Mover(root.transform, "LightGate");
            foreach (var x in new[] { 0.17f, 0.30f, 0.43f })
            {
                var bar = Primitive(PrimitiveType.Cube, "LightGateBar", gate, mats["PepA"]);
                bar.transform.localPosition = new Vector3(x, 0.42f, 0.58f);
                bar.transform.localScale = new Vector3(0.035f, 0.52f, 0.04f);
            }
            foreach (var y in new[] { 0.24f, 0.55f })
            {
                var cross = Primitive(PrimitiveType.Cube, "LightGateCross", gate, mats["PepA"]);
                cross.transform.localPosition = new Vector3(0.30f, y, 0.58f);
                cross.transform.localScale = new Vector3(0.38f, 0.035f, 0.04f);
            }

            Anchor(root.transform, "Anchor_PepA", new Vector3(-0.38f, 0.15f, -0.36f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0.36f, 0.15f, 0.76f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(-0.10f, 0.15f, -0.08f));
            Anchor(root.transform, "Slot_1", new Vector3(-0.40f, 0.15f, -1.18f));
            Anchor(root.transform, "Slot_2", new Vector3(0.40f, 0.15f, -1.18f));
            Anchor(root.transform, "Slot_3", new Vector3(0f, 0.15f, -0.86f));

            FinishDiorama(root, mats, DioramaMood.Night);
            return SavePrefab(root, $"{EnvDir}/Diorama_Beam.prefab");
        }

        // -------------------------------------------------------------------
        // Thaw — one Pep visibly enclosed by faceted ice
        // -------------------------------------------------------------------

        private static GameObject BuildThawDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Thaw");
            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Earth"]);
            slab.transform.localPosition = new Vector3(0f, -0.10f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.20f, 3.4f);
            var snow = Primitive(PrimitiveType.Cube, "SnowField", root.transform, mats["Snow"]);
            snow.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            snow.transform.localScale = new Vector3(1.35f, 0.15f, 3.15f);

            var frozenPatch = Primitive(PrimitiveType.Sphere, "FrozenPatch", root.transform, mats["WaterLight"]);
            frozenPatch.transform.localPosition = new Vector3(0.30f, 0.16f, 0.66f);
            frozenPatch.transform.localScale = new Vector3(0.48f, 0.025f, 0.42f);

            var shell = Mover(root.transform, "IceShell");
            foreach (var (x, y, angle, height) in new[]
                     {
                         (0.10f, 0.35f, -13f, 0.44f),
                         (0.48f, 0.34f, 15f, 0.42f),
                         (0.20f, 0.50f, -38f, 0.35f),
                         (0.41f, 0.51f, 40f, 0.34f),
                     })
            {
                var crystal = Primitive(PrimitiveType.Cube, "IceCrystal", shell, mats["Ice"]);
                crystal.transform.localPosition = new Vector3(x, y, 0.62f);
                crystal.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                crystal.transform.localScale = new Vector3(0.105f, height, 0.11f);
            }
            var iceCap = Primitive(PrimitiveType.Sphere, "IceCap", shell, mats["Ice"]);
            iceCap.transform.localPosition = new Vector3(0.30f, 0.49f, 0.68f);
            iceCap.transform.localScale = new Vector3(0.40f, 0.15f, 0.30f);

            var puddle = Mover(root.transform, "MeltPuddle");
            var puddleVisual = Primitive(PrimitiveType.Sphere, "Puddle", puddle, mats["Water"]);
            puddleVisual.transform.localPosition = new Vector3(0.30f, 0.17f, 0.66f);
            puddleVisual.transform.localScale = new Vector3(0.48f, 0.025f, 0.40f);
            puddle.GetComponent<AnimTarget>().SetVisibleAtRest(false);

            var snowball = Primitive(PrimitiveType.Sphere, "Snowball", root.transform, mats["Snow"]);
            snowball.transform.localPosition = new Vector3(-0.52f, 0.20f, 0.78f);
            snowball.transform.localScale = Vector3.one * 0.12f;

            Anchor(root.transform, "Anchor_PepA", new Vector3(-0.36f, 0.15f, -0.28f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0.30f, 0.15f, 0.67f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(-0.04f, 0.15f, -0.02f));
            Anchor(root.transform, "Slot_1", new Vector3(-0.40f, 0.15f, -1.18f));
            Anchor(root.transform, "Slot_2", new Vector3(0f, 0.15f, -0.91f));
            Anchor(root.transform, "Slot_3", new Vector3(0.40f, 0.15f, -1.18f));

            FinishDiorama(root, mats, DioramaMood.Snow);
            return SavePrefab(root, $"{EnvDir}/Diorama_Thaw.prefab");
        }

        // -------------------------------------------------------------------
        // Grow — a small flower lift below a high garden terrace
        // -------------------------------------------------------------------

        private static GameObject BuildGrowDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Grow");
            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Earth"]);
            slab.transform.localPosition = new Vector3(0f, -0.10f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.20f, 3.4f);
            var garden = Primitive(PrimitiveType.Cube, "Garden", root.transform, mats["FoliageLight"]);
            garden.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            garden.transform.localScale = new Vector3(1.35f, 0.15f, 3.15f);

            var terrace = Primitive(PrimitiveType.Cube, "HighTerrace", root.transform, mats["Foliage"]);
            terrace.transform.localPosition = new Vector3(-0.34f, 0.29f, 0.36f);
            terrace.transform.localScale = new Vector3(0.68f, 0.43f, 1.20f);
            var terraceEdge = Primitive(PrimitiveType.Cube, "TerraceEdge", root.transform, mats["EarthLight"]);
            terraceEdge.transform.localPosition = new Vector3(0.015f, 0.30f, 0.36f);
            terraceEdge.transform.localScale = new Vector3(0.055f, 0.43f, 1.20f);

            var pot = Primitive(PrimitiveType.Cylinder, "FlowerPot", root.transform, mats["PepA"]);
            pot.transform.localPosition = new Vector3(0.34f, 0.21f, 0.47f);
            pot.transform.localScale = new Vector3(0.18f, 0.08f, 0.18f);
            var potRim = Primitive(PrimitiveType.Cylinder, "PotRim", root.transform, mats["PepALight"]);
            potRim.transform.localPosition = new Vector3(0.34f, 0.28f, 0.47f);
            potRim.transform.localScale = new Vector3(0.205f, 0.025f, 0.205f);

            var plant = Mover(root.transform, "Plant");
            plant.parent.localPosition = new Vector3(0.34f, 0.15f, 0.47f);
            var stem = Primitive(PrimitiveType.Cylinder, "Stem", plant, mats["FoliageDark"]);
            stem.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            stem.transform.localScale = new Vector3(0.025f, 0.075f, 0.025f);
            foreach (var side in new[] { -1f, 1f })
            {
                var leaf = Primitive(PrimitiveType.Sphere, "Leaf", plant, mats["Foliage"]);
                leaf.transform.localPosition = new Vector3(side * 0.075f, 0.09f, 0f);
                leaf.transform.localRotation = Quaternion.Euler(0f, 0f, side * -25f);
                leaf.transform.localScale = new Vector3(0.12f, 0.045f, 0.055f);
            }
            var flowerDeck = Primitive(PrimitiveType.Sphere, "FlowerDeck", plant, mats["Accent"]);
            flowerDeck.transform.localPosition = new Vector3(0f, 0.17f, 0f);
            flowerDeck.transform.localScale = new Vector3(0.25f, 0.055f, 0.22f);
            for (var i = 0; i < 6; i++)
            {
                var angle = i * Mathf.PI * 2f / 6f;
                var petal = Primitive(PrimitiveType.Sphere, "Petal", plant, mats["Cream"]);
                petal.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.13f, 0.18f,
                    Mathf.Sin(angle) * 0.10f);
                petal.transform.localScale = new Vector3(0.12f, 0.035f, 0.085f);
            }

            Anchor(root.transform, "Anchor_PepA", new Vector3(-0.34f, 0.505f, 0.22f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0.34f, 0.35f, 0.47f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(-0.06f, 0.505f, 0.14f));
            Anchor(root.transform, "Slot_1", new Vector3(-0.40f, 0.15f, -1.18f));
            Anchor(root.transform, "Slot_2", new Vector3(0.40f, 0.15f, -1.18f));
            Anchor(root.transform, "Slot_3", new Vector3(0f, 0.15f, -0.88f));

            FinishDiorama(root, mats, DioramaMood.Garden);
            return SavePrefab(root, $"{EnvDir}/Diorama_Grow.prefab");
        }

        // -------------------------------------------------------------------
        // Shelter — rain falls beside the route, not between the Peps
        // -------------------------------------------------------------------

        private static GameObject BuildRainDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Rain");
            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Earth"]);
            slab.transform.localPosition = new Vector3(0f, -0.10f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.20f, 3.4f);
            var wet = Primitive(PrimitiveType.Cube, "WetGarden", root.transform, mats["FoliageDark"]);
            wet.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            wet.transform.localScale = new Vector3(1.35f, 0.15f, 3.15f);

            var path = Primitive(PrimitiveType.Cube, "WetPath", root.transform, mats["StoneLight"]);
            path.transform.localPosition = new Vector3(-0.02f, 0.16f, 0.12f);
            path.transform.localRotation = Quaternion.Euler(0f, -14f, 0f);
            path.transform.localScale = new Vector3(0.44f, 0.025f, 2.20f);

            // A tiny shelter establishes why one Pep is dry and the other is
            // huddling under the cloud off to the side.
            foreach (var x in new[] { -0.58f, -0.20f })
            {
                var post = Primitive(PrimitiveType.Cube, "AwningPost", root.transform, mats["Wood"]);
                post.transform.localPosition = new Vector3(x, 0.40f, -0.22f);
                post.transform.localScale = new Vector3(0.045f, 0.50f, 0.045f);
            }
            var awning = Primitive(PrimitiveType.Cube, "Awning", root.transform, mats["Accent"]);
            awning.transform.localPosition = new Vector3(-0.39f, 0.67f, -0.22f);
            awning.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
            awning.transform.localScale = new Vector3(0.48f, 0.065f, 0.46f);

            var cloud = Mover(root.transform, "Cloud");
            cloud.parent.localPosition = new Vector3(0.34f, 0.82f, 0.67f);
            foreach (var (x, y, s) in new[] { (-0.14f, 0f, 0.19f), (0f, 0.06f, 0.24f), (0.17f, 0f, 0.18f) })
            {
                var puff = Primitive(PrimitiveType.Sphere, "CloudPuff", cloud, mats["StoneLight"]);
                puff.transform.localPosition = new Vector3(x, y, 0f);
                puff.transform.localScale = new Vector3(s, s * 0.70f, s * 0.78f);
            }
            var cloudBase = Primitive(PrimitiveType.Cube, "CloudBase", cloud, mats["StoneLight"]);
            cloudBase.transform.localPosition = new Vector3(0.01f, -0.045f, 0f);
            cloudBase.transform.localScale = new Vector3(0.42f, 0.09f, 0.24f);

            var rain = Mover(root.transform, "Rain");
            rain.parent.localPosition = new Vector3(0.34f, 0.18f, 0.63f);
            for (var i = 0; i < 7; i++)
            {
                var drop = Primitive(PrimitiveType.Cube, "RainDrop", rain, mats["WaterLight"]);
                drop.transform.localPosition = new Vector3(-0.20f + i * 0.067f,
                    0.18f + (i % 3) * 0.12f, (i % 2) * 0.035f);
                drop.transform.localRotation = Quaternion.Euler(0f, 0f, -9f);
                drop.transform.localScale = new Vector3(0.018f, 0.14f, 0.018f);
            }

            foreach (var (x, z, s) in new[] { (0.33f, 0.61f, 0.30f), (0.49f, 0.40f, 0.16f) })
            {
                var puddle = Primitive(PrimitiveType.Sphere, "Puddle", root.transform, mats["Water"]);
                puddle.transform.localPosition = new Vector3(x, 0.17f, z);
                puddle.transform.localScale = new Vector3(s, 0.022f, s * 0.68f);
            }

            Anchor(root.transform, "Anchor_PepA", new Vector3(-0.39f, 0.15f, -0.26f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0.34f, 0.15f, 0.64f));
            // Meet in the open beside the awning. At the former centre point
            // Pep A's half of the hug landed beneath the roof and the repeated
            // emotional payoff was partly occluded on a portrait phone.
            Anchor(root.transform, "Anchor_Meet", new Vector3(0.22f, 0.15f, -0.10f));
            Anchor(root.transform, "Slot_1", new Vector3(-0.40f, 0.15f, -1.18f));
            Anchor(root.transform, "Slot_2", new Vector3(0.40f, 0.15f, -1.18f));
            Anchor(root.transform, "Slot_3", new Vector3(0f, 0.15f, -0.88f));

            FinishDiorama(root, mats, DioramaMood.Rain);
            return SavePrefab(root, $"{EnvDir}/Diorama_Rain.prefab");
        }

        // -------------------------------------------------------------------
        // The Canyon diorama
        // -------------------------------------------------------------------

        /// <summary>
        /// Two mesas with real sky between them.
        ///
        /// The gap is 0.8 deep against the brook's 0.62, and that number is
        /// load-bearing: the plank measures 0.78, so it visibly *almost*
        /// spans the canyon. PLAN §12 names this exact beat — the plank
        /// bridges the brook and comes up six inches short here — which is how
        /// a prop earns a personality instead of a fixed answer.
        ///
        /// Anchors and slots deliberately match the Brook's layout, so the
        /// fixed camera framing and the reunion offsets carry over untouched.
        /// Reuse at this level is what makes eight dioramas host thirty-six
        /// rescues.
        /// </summary>
        private static GameObject BuildCanyonDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Canyon");

            // One continuous base, with the canyon cut into its top — not two
            // free-standing mesas. Two mesas was the first attempt and it
            // failed on device: at the fixed 40-degree camera you see the far
            // mesa's front face with open sky underneath it, so it reads as a
            // slab floating in the air rather than as a far wall. A single
            // slab keeps the Brook's silhouette, which is also what lets both
            // dioramas share one camera.
            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Earth"]);
            slab.transform.localPosition = new Vector3(0f, -0.16f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.32f, 3.4f);

            var near = Primitive(PrimitiveType.Cube, "Plateau_Near", root.transform, mats["Foliage"]);
            near.transform.localPosition = new Vector3(0f, 0.075f, -1.05f);
            near.transform.localScale = new Vector3(1.35f, 0.15f, 1.24f);

            var far = Primitive(PrimitiveType.Cube, "Plateau_Far", root.transform, mats["FoliageLight"]);
            far.transform.localPosition = new Vector3(0f, 0.075f, 1.05f);
            far.transform.localScale = new Vector3(1.35f, 0.15f, 1.24f);

            // The floor of the cut, dark enough to read as depth rather than
            // as a painted stripe. The gap is 0.8 against the Brook's 0.62,
            // and that number is load-bearing: the plank measures 0.78, so it
            // visibly *almost* spans the canyon. PLAN §12 names this beat —
            // the plank bridges the brook and comes up six inches short here.
            var floor = Primitive(PrimitiveType.Cube, "Chasm", root.transform, mats["Ink"]);
            // Sits a hair proud of the platform top rather than flush with it.
            // Flush meant coplanar faces, which z-fought into horizontal
            // stripes across the whole chasm on device — invisible in the
            // editor's default view and obvious on a phone.
            floor.transform.localPosition = new Vector3(0f, -0.01f, 0f);
            floor.transform.localScale = new Vector3(1.35f, 0.05f, 0.8f);

            foreach (var (x, z, s) in new[] { (-0.54f, -1.35f, 0.11f), (0.52f, 1.4f, 0.09f) })
            {
                var rock = Primitive(PrimitiveType.Sphere, "Rock", root.transform, mats["Stone"]);
                rock.transform.localPosition = new Vector3(x, 0.15f, z);
                rock.transform.localScale = new Vector3(s, s * 0.7f, s);
            }

            Anchor(root.transform, "Anchor_PepA", new Vector3(0f, 0.15f, -0.62f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0f, 0.15f, 0.62f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(0f, 0.15f, 0.5f));

            Anchor(root.transform, "Slot_1", new Vector3(-0.42f, 0.15f, -1.25f));
            Anchor(root.transform, "Slot_2", new Vector3(0.45f, 0.15f, -1.35f));
            Anchor(root.transform, "Slot_3", new Vector3(-0.45f, 0.15f, 1.3f));

            FinishDiorama(root, mats, DioramaMood.Meadow);
            return SavePrefab(root, $"{EnvDir}/Diorama_Canyon.prefab");
        }

        // -------------------------------------------------------------------
        // Deep Ocean Trench Diorama (Round 9)
        // -------------------------------------------------------------------

        private static GameObject BuildOceanDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Ocean");

            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Night"]);
            slab.transform.localPosition = new Vector3(0f, -0.16f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.32f, 3.4f);

            var reefNear = Primitive(PrimitiveType.Cube, "Reef_Near", root.transform, mats["WaterLight"]);
            reefNear.transform.localPosition = new Vector3(0f, 0.075f, -1.05f);
            reefNear.transform.localScale = new Vector3(1.35f, 0.15f, 1.24f);

            var reefFar = Primitive(PrimitiveType.Cube, "Reef_Far", root.transform, mats["Water"]);
            reefFar.transform.localPosition = new Vector3(0f, 0.075f, 1.05f);
            reefFar.transform.localScale = new Vector3(1.35f, 0.15f, 1.24f);

            var abyss = Primitive(PrimitiveType.Cube, "AbyssTrench", root.transform, mats["Night"]);
            abyss.transform.localPosition = new Vector3(0f, -0.01f, 0f);
            abyss.transform.localScale = new Vector3(1.35f, 0.05f, 0.8f);

            // Bioluminescent sea creature / anemone
            var anemone = Mover(root.transform, "Anemone");
            var body = Primitive(PrimitiveType.Sphere, "Body", anemone, mats["PepA"]);
            body.transform.localPosition = new Vector3(0.48f, 0.22f, 0.15f);
            body.transform.localScale = new Vector3(0.24f, 0.28f, 0.24f);
            var lure = Primitive(PrimitiveType.Sphere, "Lure", anemone, mats["AccentLight"]);
            lure.transform.localPosition = new Vector3(0.48f, 0.44f, 0.24f);
            lure.transform.localScale = Vector3.one * 0.08f;

            // Kelp barrier gate
            var reefGate = Mover(root.transform, "ReefGate");
            foreach (var (x, h) in new[] { (-0.3f, 0.35f), (-0.1f, 0.42f), (0.1f, 0.38f), (0.3f, 0.32f) })
            {
                var kelp = Primitive(PrimitiveType.Cylinder, "KelpStalk", reefGate, mats["FoliageDark"]);
                kelp.transform.localPosition = new Vector3(x, 0.18f + h * 0.5f, 0f);
                kelp.transform.localScale = new Vector3(0.04f, h * 0.5f, 0.04f);
            }

            Anchor(root.transform, "Anchor_PepA", new Vector3(0f, 0.15f, -0.62f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0f, 0.15f, 0.62f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(0f, 0.15f, 0.5f));

            Anchor(root.transform, "Slot_1", new Vector3(-0.42f, 0.15f, -1.25f));
            Anchor(root.transform, "Slot_2", new Vector3(0.45f, 0.15f, -1.35f));
            Anchor(root.transform, "Slot_3", new Vector3(-0.45f, 0.15f, 1.3f));

            FinishDiorama(root, mats, DioramaMood.Ocean);
            return SavePrefab(root, $"{EnvDir}/Diorama_Ocean.prefab");
        }

        // -------------------------------------------------------------------
        // Space / Orbital Station Diorama (Round 10 - Grand Free Climax)
        // -------------------------------------------------------------------

        private static GameObject BuildSpaceDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Space");

            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Stone"]);
            slab.transform.localPosition = new Vector3(0f, -0.16f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.32f, 3.4f);

            var stationDeck = Primitive(PrimitiveType.Cube, "StationDeck", root.transform, mats["StoneLight"]);
            stationDeck.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            stationDeck.transform.localScale = new Vector3(1.35f, 0.15f, 3.15f);

            var vacuumChasm = Primitive(PrimitiveType.Cube, "SpaceVoid", root.transform, mats["Ink"]);
            vacuumChasm.transform.localPosition = new Vector3(0f, -0.01f, 0f);
            vacuumChasm.transform.localScale = new Vector3(1.35f, 0.05f, 0.72f);

            // Orbital airlock hatch / door
            var airlock = Mover(root.transform, "AirlockHatch");
            var door = Primitive(PrimitiveType.Cube, "Door", airlock, mats["Accent"]);
            door.transform.localPosition = new Vector3(0f, 0.32f, 0.02f);
            door.transform.localScale = new Vector3(0.48f, 0.34f, 0.08f);

            // Solar array wing
            var solarWing = Mover(root.transform, "SolarWing");
            var panel = Primitive(PrimitiveType.Cube, "Panel", solarWing, mats["Water"]);
            panel.transform.localPosition = new Vector3(0.55f, 0.38f, 0.85f);
            panel.transform.localScale = new Vector3(0.38f, 0.02f, 0.65f);

            Anchor(root.transform, "Anchor_PepA", new Vector3(0f, 0.15f, -0.62f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0f, 0.15f, 0.62f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(0f, 0.15f, 0.5f));

            Anchor(root.transform, "Slot_1", new Vector3(-0.42f, 0.15f, -1.25f));
            Anchor(root.transform, "Slot_2", new Vector3(0.45f, 0.15f, -1.35f));
            Anchor(root.transform, "Slot_3", new Vector3(-0.45f, 0.15f, 1.3f));

            FinishDiorama(root, mats, DioramaMood.Space);
            return SavePrefab(root, $"{EnvDir}/Diorama_Space.prefab");
        }

        // -------------------------------------------------------------------
        // Automated Foundry / Factory Diorama (Round 11 - Peps Unlimited Climax)
        // -------------------------------------------------------------------

        private static GameObject BuildFactoryDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Factory");

            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Stone"]);
            slab.transform.localPosition = new Vector3(0f, -0.16f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.32f, 3.4f);

            var factoryFloor = Primitive(PrimitiveType.Cube, "FactoryFloor", root.transform, mats["StoneLight"]);
            factoryFloor.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            factoryFloor.transform.localScale = new Vector3(1.35f, 0.15f, 3.15f);

            // Molten vat chasm
            var moltenVat = Primitive(PrimitiveType.Cube, "MoltenVat", root.transform, mats["PepA"]);
            moltenVat.transform.localPosition = new Vector3(0f, -0.01f, 0f);
            moltenVat.transform.localScale = new Vector3(1.35f, 0.05f, 0.8f);

            // Assembly conveyor belt
            var conveyor = Mover(root.transform, "ConveyorBelt");
            var belt = Primitive(PrimitiveType.Cube, "Belt", conveyor, mats["Ink"]);
            belt.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            belt.transform.localScale = new Vector3(0.45f, 0.04f, 0.85f);

            // Large factory cogs / gear assembly
            var gear = Mover(root.transform, "GearAssembly");
            var cog = Primitive(PrimitiveType.Cylinder, "Cog", gear, mats["Accent"]);
            cog.transform.localPosition = new Vector3(0.52f, 0.32f, 0.15f);
            cog.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            cog.transform.localScale = new Vector3(0.35f, 0.04f, 0.35f);

            Anchor(root.transform, "Anchor_PepA", new Vector3(0f, 0.15f, -0.62f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0f, 0.15f, 0.62f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(0f, 0.15f, 0.5f));

            Anchor(root.transform, "Slot_1", new Vector3(-0.42f, 0.15f, -1.25f));
            Anchor(root.transform, "Slot_2", new Vector3(0.45f, 0.15f, -1.35f));
            Anchor(root.transform, "Slot_3", new Vector3(-0.45f, 0.15f, 1.3f));

            FinishDiorama(root, mats, DioramaMood.Factory);
            return SavePrefab(root, $"{EnvDir}/Diorama_Factory.prefab");
        }

        // -------------------------------------------------------------------
        // Neon Metropolis Diorama (Round 12 - Grand Finale)
        // -------------------------------------------------------------------

        private static GameObject BuildNeonDiorama(IReadOnlyDictionary<string, Material> mats)
        {
            var root = new GameObject("Diorama_Neon");

            var slab = Primitive(PrimitiveType.Cube, "Platform", root.transform, mats["Night"]);
            slab.transform.localPosition = new Vector3(0f, -0.16f, 0f);
            slab.transform.localScale = new Vector3(1.5f, 0.32f, 3.4f);

            var helipad = Primitive(PrimitiveType.Cube, "Helipad", root.transform, mats["Ink"]);
            helipad.transform.localPosition = new Vector3(0f, 0.075f, 0f);
            helipad.transform.localScale = new Vector3(1.35f, 0.15f, 3.15f);

            var skylineGap = Primitive(PrimitiveType.Cube, "SkylineGap", root.transform, mats["Night"]);
            skylineGap.transform.localPosition = new Vector3(0f, -0.01f, 0f);
            skylineGap.transform.localScale = new Vector3(1.35f, 0.05f, 0.8f);

            // Glowing neon sign / cyber beacon
            var neonSign = Mover(root.transform, "NeonSign");
            var signBack = Primitive(PrimitiveType.Cube, "Backing", neonSign, mats["Ink"]);
            signBack.transform.localPosition = new Vector3(0.54f, 0.42f, 0.85f);
            signBack.transform.localScale = new Vector3(0.18f, 0.36f, 0.42f);
            var neonBorder = Primitive(PrimitiveType.Cube, "GlowBorder", neonSign, mats["PepB"]);
            neonBorder.transform.localPosition = new Vector3(0.53f, 0.42f, 0.85f);
            neonBorder.transform.localScale = new Vector3(0.19f, 0.38f, 0.44f);

            // Skyline suspension cable
            var cable = Mover(root.transform, "SkylineCable");
            var wire = Primitive(PrimitiveType.Cylinder, "Cable", cable, mats["PepA"]);
            wire.transform.localPosition = new Vector3(0f, 0.22f, 0f);
            wire.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wire.transform.localScale = new Vector3(0.025f, 0.65f, 0.025f);

            Anchor(root.transform, "Anchor_PepA", new Vector3(0f, 0.15f, -0.62f));
            Anchor(root.transform, "Anchor_PepB", new Vector3(0f, 0.15f, 0.62f));
            Anchor(root.transform, "Anchor_Meet", new Vector3(0f, 0.15f, 0.5f));

            Anchor(root.transform, "Slot_1", new Vector3(-0.42f, 0.15f, -1.25f));
            Anchor(root.transform, "Slot_2", new Vector3(0.45f, 0.15f, -1.35f));
            Anchor(root.transform, "Slot_3", new Vector3(-0.45f, 0.15f, 1.3f));

            FinishDiorama(root, mats, DioramaMood.Neon);
            return SavePrefab(root, $"{EnvDir}/Diorama_Neon.prefab");
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        private enum DioramaMood
        {
            Meadow,
            Garden,
            Workshop,
            Night,
            Snow,
            Rain,
            Ocean,
            Space,
            Factory,
            Neon,
        }

        /// <summary>
        /// The scalable toy-diorama finish: a layered floating base, the same
        /// interaction tokens under all three choices, and sparse dressing
        /// selected from a small mood vocabulary. New rescues get hierarchy,
        /// grounding and depth by calling this once rather than hand-polishing
        /// every environment independently.
        /// </summary>
        private static void FinishDiorama(GameObject root, IReadOnlyDictionary<string, Material> mats,
            DioramaMood mood)
        {
            var platform = FindChild(root.transform, "Platform");
            if (platform != null)
            {
                var lip = Primitive(PrimitiveType.Cube, "InsetLip", root.transform, mats["EarthLight"]);
                lip.transform.localPosition = new Vector3(0f, -0.035f, 0f);
                lip.transform.localScale = new Vector3(1.43f, 0.056f, 3.27f);

                var foot = Primitive(PrimitiveType.Cube, "BaseFoot", root.transform, mats["Ink"]);
                var bottom = platform.localPosition.y - platform.localScale.y * 0.5f;
                foot.transform.localPosition = new Vector3(0f, bottom - 0.025f, 0.03f);
                foot.transform.localScale = new Vector3(1.36f, 0.055f, 3.10f);

                foreach (var x in new[] { -0.70f, 0.70f })
                foreach (var z in new[] { -1.56f, 1.56f })
                {
                    var peg = Primitive(PrimitiveType.Cylinder, "CornerPeg", root.transform, mats["AccentLight"]);
                    peg.transform.localPosition = new Vector3(x, -0.003f, z);
                    peg.transform.localScale = new Vector3(0.032f, 0.018f, 0.032f);
                }
            }

            AddChoicePads(root.transform, mats);
            AddDressing(root.transform, mats, mood);
        }

        private static void AddChoicePads(Transform root, IReadOnlyDictionary<string, Material> mats)
        {
            foreach (var anchorId in new[] { "Slot_1", "Slot_2", "Slot_3" })
            {
                var anchor = FindChild(root, anchorId);
                if (anchor == null) continue;

                var pad = Child(root, $"ChoicePad_{anchorId}");
                pad.localPosition = anchor.localPosition + Vector3.up * 0.004f;

                var halo = Primitive(PrimitiveType.Cylinder, "Halo", pad, mats["AccentLight"]);
                halo.transform.localScale = new Vector3(0.30f, 0.008f, 0.25f);
                var surface = Primitive(PrimitiveType.Cylinder, "Surface", pad, mats["Cream"]);
                surface.transform.localPosition = Vector3.up * 0.012f;
                surface.transform.localScale = new Vector3(0.255f, 0.008f, 0.21f);

                pad.gameObject.AddComponent<ChoicePad>().Configure(anchorId, halo.transform, surface.transform);
            }
        }

        private static void AddDressing(Transform root, IReadOnlyDictionary<string, Material> mats,
            DioramaMood mood)
        {
            switch (mood)
            {
                case DioramaMood.Meadow:
                case DioramaMood.Garden:
                    AddBush(root, mats, new Vector3(-0.58f, 0.17f, 1.43f), 0.11f);
                    AddBush(root, mats, new Vector3(0.61f, 0.17f, 1.18f), 0.09f);
                    AddFlower(root, mats, new Vector3(-0.58f, 0.16f, 0.72f), mats["PepA"]);
                    AddFlower(root, mats, new Vector3(0.60f, 0.16f, 0.82f), mats["Accent"]);
                    if (mood == DioramaMood.Garden)
                    {
                        AddFlower(root, mats, new Vector3(-0.62f, 0.16f, -0.18f), mats["PepB"]);
                    }
                    break;

                case DioramaMood.Workshop:
                    foreach (var (x, z, s) in new[] { (-0.59f, 0.82f, 0.07f), (0.59f, 0.95f, 0.055f), (-0.60f, -0.10f, 0.045f) })
                    {
                        var bolt = Primitive(PrimitiveType.Cylinder, "FloorBolt", root, mats["StoneLight"]);
                        bolt.transform.localPosition = new Vector3(x, 0.17f, z);
                        bolt.transform.localScale = new Vector3(s, 0.018f, s);
                        var slot = Primitive(PrimitiveType.Cube, "BoltSlot", root, mats["Ink"]);
                        slot.transform.localPosition = new Vector3(x, 0.190f, z);
                        slot.transform.localRotation = Quaternion.Euler(0f, (x + z) * 40f, 0f);
                        slot.transform.localScale = new Vector3(s * 1.1f, 0.010f, 0.014f);
                    }
                    break;

                case DioramaMood.Night:
                    foreach (var (x, z, mat) in new[]
                             {
                                 (-0.58f, 0.86f, mats["WaterLight"]),
                                 (0.58f, 1.03f, mats["AccentLight"]),
                                 (-0.62f, -0.04f, mats["PepB"]),
                             })
                    {
                        var glow = Primitive(PrimitiveType.Sphere, "GlowStone", root, mat);
                        glow.transform.localPosition = new Vector3(x, 0.18f, z);
                        glow.transform.localScale = new Vector3(0.075f, 0.04f, 0.065f);
                    }
                    break;

                case DioramaMood.Snow:
                    AddPine(root, mats, new Vector3(-0.57f, 0.15f, 1.28f), 0.75f);
                    AddPine(root, mats, new Vector3(0.59f, 0.15f, 1.42f), 0.58f);
                    break;

                case DioramaMood.Rain:
                    AddBush(root, mats, new Vector3(-0.59f, 0.17f, 1.38f), 0.09f);
                    AddBush(root, mats, new Vector3(0.59f, 0.17f, 1.22f), 0.08f);
                    break;

                case DioramaMood.Ocean:
                    foreach (var (x, z, mat) in new[]
                             {
                                 (-0.58f, 0.95f, mats["PepB"]),
                                 (0.56f, 1.15f, mats["WaterLight"]),
                                 (-0.54f, -0.2f, mats["PepA"]),
                             })
                    {
                        var coral = Primitive(PrimitiveType.Sphere, "CoralNode", root, mat);
                        coral.transform.localPosition = new Vector3(x, 0.18f, z);
                        coral.transform.localScale = new Vector3(0.08f, 0.12f, 0.08f);
                    }
                    break;

                case DioramaMood.Space:
                    foreach (var (x, z) in new[] { (-0.56f, 1.1f), (0.58f, 0.95f), (-0.52f, -0.25f) })
                    {
                        var beacon = Primitive(PrimitiveType.Cylinder, "Beacon", root, mats["AccentLight"]);
                        beacon.transform.localPosition = new Vector3(x, 0.18f, z);
                        beacon.transform.localScale = new Vector3(0.04f, 0.08f, 0.04f);
                    }
                    break;

                case DioramaMood.Factory:
                    foreach (var (x, z) in new[] { (-0.58f, 1.05f), (0.56f, 1.12f) })
                    {
                        var pipe = Primitive(PrimitiveType.Cylinder, "SteamPipe", root, mats["StoneLight"]);
                        pipe.transform.localPosition = new Vector3(x, 0.22f, z);
                        pipe.transform.localScale = new Vector3(0.06f, 0.18f, 0.06f);
                    }
                    break;

                case DioramaMood.Neon:
                    foreach (var (x, z, mat) in new[]
                             {
                                 (-0.58f, 1.15f, mats["PepB"]),
                                 (0.58f, 1.15f, mats["PepA"]),
                                 (-0.58f, -0.3f, mats["AccentLight"]),
                                 (0.58f, -0.3f, mats["WaterLight"]),
                             })
                    {
                        var pylon = Primitive(PrimitiveType.Cube, "NeonPylon", root, mat);
                        pylon.transform.localPosition = new Vector3(x, 0.22f, z);
                        pylon.transform.localScale = new Vector3(0.04f, 0.25f, 0.04f);
                    }
                    break;
            }
        }

        private static void AddBush(Transform root, IReadOnlyDictionary<string, Material> mats,
            Vector3 position, float size)
        {
            for (var i = -1; i <= 1; i++)
            {
                var puff = Primitive(PrimitiveType.Sphere, "Bush", root,
                    i == 0 ? mats["FoliageDark"] : mats["Foliage"]);
                puff.transform.localPosition = position + new Vector3(i * size * 0.60f, Mathf.Abs(i) * -0.01f, 0f);
                puff.transform.localScale = new Vector3(size, size * 0.75f, size * 0.85f);
            }
        }

        private static void AddFlower(Transform root, IReadOnlyDictionary<string, Material> mats,
            Vector3 position, Material blossom)
        {
            var stem = Primitive(PrimitiveType.Cylinder, "FlowerStem", root, mats["FoliageDark"]);
            stem.transform.localPosition = position + Vector3.up * 0.045f;
            stem.transform.localScale = new Vector3(0.010f, 0.045f, 0.010f);
            var head = Primitive(PrimitiveType.Sphere, "Flower", root, blossom);
            head.transform.localPosition = position + Vector3.up * 0.095f;
            head.transform.localScale = new Vector3(0.045f, 0.032f, 0.045f);
        }

        private static void AddPine(Transform root, IReadOnlyDictionary<string, Material> mats,
            Vector3 position, float scale)
        {
            var trunk = Primitive(PrimitiveType.Cylinder, "PineTrunk", root, mats["Wood"]);
            trunk.transform.localPosition = position + Vector3.up * (0.12f * scale);
            trunk.transform.localScale = new Vector3(0.035f * scale, 0.12f * scale, 0.035f * scale);
            for (var i = 0; i < 3; i++)
            {
                var crown = Primitive(PrimitiveType.Sphere, "PineCrown", root,
                    i % 2 == 0 ? mats["FoliageDark"] : mats["Foliage"]);
                crown.transform.localPosition = position + Vector3.up * ((0.19f + i * 0.08f) * scale);
                var width = (0.24f - i * 0.045f) * scale;
                crown.transform.localScale = new Vector3(width, 0.12f * scale, width);
            }
        }

        private static Transform FindChild(Transform root, string name)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                if (child.name == name) return child;
            }

            return null;
        }

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

        /// <summary>
        /// A chunky front-facing oval assembled from a handful of blocks.
        /// At this scale the gaps read as facets, while the empty centre is
        /// what distinguishes a fan cage, scissor grip or handle from a disc.
        /// </summary>
        private static void BlockRing(Transform parent, string name, Material material, Vector3 centre,
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

        private static void AddZ(Transform parent, Material material, Vector3 centre, float size)
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
