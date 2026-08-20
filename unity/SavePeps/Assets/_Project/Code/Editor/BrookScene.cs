using SavePeps.Core;
using SavePeps.Monetization;
using SavePeps.Progression;
using SavePeps.Rescue;
using SavePeps.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Builds the Game scene: camera, lighting, HUD, round-complete card, and
    /// the components that run a rescue.
    ///
    ///   Tools > Save Peps > Build Game Scene
    ///
    /// This rebuilds the *scene* and nothing else. It reads the catalogue off
    /// disk and wires it in; it never writes content. Keeping that boundary
    /// sharp is what makes the scene safe to regenerate after a UI change now
    /// that rescues are authored in the inspector — see
    /// <see cref="RoundOneRescues"/> for the content side.
    /// </summary>
    public static class BrookScene
    {
        private const string ScenePath = ContentPaths.GameScenePath;

        [MenuItem("Tools/Save Peps/Build Game Scene")]
        public static void BuildGameScene()
        {
            if (AssetDatabase.LoadAssetAtPath<Catalog>(ContentPaths.CatalogPath) == null)
            {
                Debug.LogError(
                    $"[SavePeps] No catalogue at {ContentPaths.CatalogPath}. " +
                    "Run Tools > Save Peps > Seed Content first.");
                return;
            }

            BuildScene(ContentPaths.CatalogPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SavePeps] Game scene rebuilt. Content untouched.");
        }

        // -------------------------------------------------------------------
        // The scene
        // -------------------------------------------------------------------

        private static void BuildScene(string catalogPath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Loaded *after* the new scene, never before. Opening a scene
            // unloads unused assets, and an asset held only by a local
            // variable is exactly that — the reference survives as a destroyed
            // object that assigns as null, so the scene silently ends up with
            // no catalogue and the game boots to "nothing to play".
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(catalogPath);

            // Fixed camera, low FOV, tilted down: the tilt-shift toy read from
            // design/palette.md. Framing is tuned for portrait 9:19.5 and
            // wants a look on the actual device — see the note in PLAN.md.
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 30f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 40f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("B8E6F5");   // sky, from the palette
            // 40° reads as a toy on a table. The first device build used 55°
            // and looked like a floor plan — the tilt is what sells the
            // diorama, and past about 45° the objects lose their silhouettes.
            const float pitch = 40f, distance = 6.3f;
            camGo.transform.position = new Vector3(
                0f,
                0.1f + distance * Mathf.Sin(pitch * Mathf.Deg2Rad),
                -distance * Mathf.Cos(pitch * Mathf.Deg2Rad));
            camGo.transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
            camGo.AddComponent<AudioListener>();

            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Hex("FFF3CE");
            light.intensity = 1.15f;
            // No realtime shadows: blob shadows are cheaper and read better at
            // this scale (design/palette.md).
            light.shadows = LightShadows.None;
            lightGo.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            // A faint cool fill separates overlapping primitive silhouettes
            // without realtime shadows or post-processing. The main light
            // still owns the scene; this only prevents ink-side faces from
            // collapsing into one flat colour on the phone.
            var fillGo = new GameObject("Sky Fill");
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = Hex("BCEAF5");
            fill.intensity = 0.26f;
            fill.shadows = LightShadows.None;
            fillGo.transform.rotation = Quaternion.Euler(35f, 145f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Hex("B8E6F5");
            RenderSettings.ambientEquatorColor = Hex("F7F3E8");
            RenderSettings.ambientGroundColor = Hex("E8DCC8");

            var homeDiorama = BuildHomeDiorama(out var homePepA, out var homePepB);
            var hud = BuildHud(out var hudComponent);
            BuildRoundCard(hud.transform, out var cardComponent);
            BuildGameMenu(hud.transform, homeDiorama, homePepA, homePepB, out var menuComponent);

            var game = new GameObject("Game");
            var player = game.AddComponent<ChoreographyPlayer>();
            var router = game.AddComponent<TapRouter>();
            var feedback = game.AddComponent<Feedback>();
            var gameFeel = game.AddComponent<GameFeel>();
            var runner = game.AddComponent<RescueRunner>();

            Wire(router, "_camera", cam);
            Wire(gameFeel, "_camera", cam);
            Wire(runner, "_tapRouter", router);
            Wire(runner, "_player", player);
            Wire(runner, "_hud", hudComponent);
            Wire(runner, "_feedback", feedback);
            Wire(runner, "_gameFeel", gameFeel);
            Wire(menuComponent, "_feedback", feedback);
            Wire(cardComponent, "_feedback", feedback);
            // GameFlow owns sequencing now; the runner is handed one rescue
            // at a time rather than playing a fixed asset at boot.
            WireBool(runner, "_autoPlayOnStart", false);

            // The editor stand-in for RevenueCat. The real SDK does not run in
            // the editor at all, so this is what makes the gating path
            // testable without a device deploy - swapped for
            // RevenueCatEntitlementService in the Android build (P4).
            var entitlements = game.AddComponent<FakeEntitlementService>();

            var flow = game.AddComponent<GameFlow>();
            Wire(flow, "_catalog", catalog);
            Wire(flow, "_runner", runner);
            Wire(flow, "_hud", hudComponent);
            Wire(flow, "_card", cardComponent);
            Wire(flow, "_menu", menuComponent);
            Wire(flow, "_entitlementSource", entitlements);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log($"[SavePeps] Scene saved to {ScenePath} and set as the only build scene.");
            _ = hud;
        }

        /// <summary>
        /// Compact toy-label HUD. Cream plaques keep the objective readable
        /// over every diorama without growing into an overlay, while custom
        /// marks make ★/✓ dependable on Android fonts.
        /// </summary>
        private static GameObject BuildHud(out RescueHud hud)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Without an EventSystem a Canvas renders fine and no button ever
            // fires — which on device looked exactly like a broken Try Again
            // rather than like missing plumbing.
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2280f);
            scaler.matchWidthOrHeight = 1f;

            var ink = Hex("3D3354");

            // Everything the HUD owns hangs off one container so the round
            // card can hide it wholesale. On device the label, the goal and
            // the result stamp all bled through the card's wash at once.
            var hudRoot = new GameObject("HudRoot", typeof(RectTransform));
            hudRoot.transform.SetParent(canvasGo.transform, false);
            Stretch(hudRoot.GetComponent<RectTransform>());

            var statusPlate = new GameObject("StatusPlate", typeof(Image));
            statusPlate.transform.SetParent(hudRoot.transform, false);
            var statusRt = statusPlate.GetComponent<RectTransform>();
            statusRt.anchorMin = statusRt.anchorMax = new Vector2(0.5f, 1f);
            statusRt.sizeDelta = new Vector2(820f, 68f);
            statusRt.anchoredPosition = new Vector2(0f, -64f);
            StylePanel(statusPlate.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f, 0.84f));
            statusPlate.GetComponent<Image>().raycastTarget = false;
            AddShadow(statusPlate.GetComponent<Image>(), new Color(0.24f, 0.20f, 0.33f, 0.14f),
                new Vector2(0f, -4f));

            var roundLabel = Text(statusPlate.transform, "RoundLabel", font, 30, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(770f, 52f));
            roundLabel.fontStyle = FontStyle.Bold;
            roundLabel.color = new Color(ink.r, ink.g, ink.b, 0.78f);

            var marks = new MasteryMarkGraphic[RoundDefinition.RescuesPerRound];
            for (var i = 0; i < marks.Length; i++)
            {
                var markGo = new GameObject($"Mark_{i}", typeof(CanvasRenderer), typeof(MasteryMarkGraphic));
                markGo.transform.SetParent(hudRoot.transform, false);
                var rt = markGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(44f, 44f);
                rt.anchoredPosition = new Vector2((i - 1) * 58f, -128f);
                marks[i] = markGo.GetComponent<MasteryMarkGraphic>();
            }

            var goalPlate = new GameObject("GoalPlate", typeof(Image));
            goalPlate.transform.SetParent(hudRoot.transform, false);
            var goalRt = goalPlate.GetComponent<RectTransform>();
            goalRt.anchorMin = goalRt.anchorMax = new Vector2(0.5f, 1f);
            goalRt.sizeDelta = new Vector2(850f, 92f);
            goalRt.anchoredPosition = new Vector2(0f, -207f);
            StylePanel(goalPlate.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f, 0.90f));
            goalPlate.GetComponent<Image>().raycastTarget = false;
            AddShadow(goalPlate.GetComponent<Image>(), new Color(0.24f, 0.20f, 0.33f, 0.13f),
                new Vector2(0f, -5f));

            var goal = Text(goalPlate.transform, "Goal", font, 48, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 74f));
            goal.fontStyle = FontStyle.Bold;

            // Failure copy is one large glanceable line that follows the
            // selected prop, not a footer or a modal to dismiss.
            var tray = new GameObject("QuipRibbon", typeof(Image), typeof(CanvasGroup));
            tray.transform.SetParent(hudRoot.transform, false);
            var trayRt = tray.GetComponent<RectTransform>();
            trayRt.anchorMin = trayRt.anchorMax = new Vector2(0.5f, 0f);
            trayRt.sizeDelta = new Vector2(990f, 158f);
            trayRt.anchoredPosition = new Vector2(0f, 390f);
            StylePanel(tray.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f, 0.98f));
            tray.GetComponent<Image>().raycastTarget = false;
            AddShadow(tray.GetComponent<Image>(), new Color(0.24f, 0.20f, 0.33f, 0.20f),
                new Vector2(0f, -7f));

            var quip = Text(tray.transform, "Quip", font, 56, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(920f, 132f));
            quip.fontStyle = FontStyle.Bold;
            quip.horizontalOverflow = HorizontalWrapMode.Overflow;
            quip.verticalOverflow = VerticalWrapMode.Truncate;

            hud = canvasGo.AddComponent<RescueHud>();
            Wire(hud, "_root", hudRoot);
            Wire(hud, "_roundLabel", roundLabel);
            Wire(hud, "_goal", goal);
            Wire(hud, "_tray", tray);
            Wire(hud, "_trayGroup", tray.GetComponent<CanvasGroup>());
            Wire(hud, "_trayRect", trayRt);
            Wire(hud, "_quip", quip);

            var so = new SerializedObject(hud);
            var marksProp = so.FindProperty("_marks");
            marksProp.arraySize = marks.Length;
            for (var i = 0; i < marks.Length; i++)
            {
                marksProp.GetArrayElementAtIndex(i).objectReferenceValue = marks[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            tray.SetActive(false);
            return canvasGo;
        }

        /// <summary>
        /// A tiny reusable title tableau made from the same Peps and palette
        /// as gameplay. It gives the home screen a character-led focal point
        /// without creating a second scene or one-off title artwork.
        /// </summary>
        private static GameObject BuildHomeDiorama(out Pep pepA, out Pep pepB)
        {
            var root = new GameObject("HomeDiorama");
            var earth = AssetDatabase.LoadAssetAtPath<Material>(
                ContentPaths.Root + "/Art/Materials/M_Pal_Earth.mat");
            var earthLight = AssetDatabase.LoadAssetAtPath<Material>(
                ContentPaths.Root + "/Art/Materials/M_Pal_EarthLight.mat");
            var foliage = AssetDatabase.LoadAssetAtPath<Material>(
                ContentPaths.Root + "/Art/Materials/M_Pal_Foliage.mat");
            var foliageLight = AssetDatabase.LoadAssetAtPath<Material>(
                ContentPaths.Root + "/Art/Materials/M_Pal_FoliageLight.mat");
            var coral = AssetDatabase.LoadAssetAtPath<Material>(
                ContentPaths.Root + "/Art/Materials/M_Pal_PepA.mat");

            WorldPrimitive("Base", root.transform, PrimitiveType.Cylinder,
                new Vector3(0f, -0.18f, 0f), new Vector3(2.25f, 0.16f, 1.42f), earth);
            WorldPrimitive("Top", root.transform, PrimitiveType.Cylinder,
                new Vector3(0f, 0.02f, 0f), new Vector3(2.16f, 0.055f, 1.34f), earthLight);

            // Sparse dressing frames the couple but never looks tappable.
            WorldPrimitive("BushLeft", root.transform, PrimitiveType.Sphere,
                new Vector3(-1.35f, 0.22f, 0.45f), new Vector3(0.42f, 0.28f, 0.34f), foliage);
            WorldPrimitive("BushRight", root.transform, PrimitiveType.Sphere,
                new Vector3(1.30f, 0.20f, 0.48f), new Vector3(0.36f, 0.25f, 0.31f), foliageLight);

            var heart = new GameObject("Heart").transform;
            heart.SetParent(root.transform, false);
            heart.localPosition = new Vector3(0f, 0.95f, -0.02f);
            var left = WorldPrimitive("Left", heart, PrimitiveType.Sphere,
                new Vector3(-0.055f, 0.045f, 0f), new Vector3(0.13f, 0.13f, 0.08f), coral);
            var right = WorldPrimitive("Right", heart, PrimitiveType.Sphere,
                new Vector3(0.055f, 0.045f, 0f), new Vector3(0.13f, 0.13f, 0.08f), coral);
            var point = WorldPrimitive("Point", heart, PrimitiveType.Cube,
                new Vector3(0f, -0.045f, 0f), new Vector3(0.16f, 0.16f, 0.075f), coral);
            point.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            _ = left;
            _ = right;

            var pepAAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                ContentPaths.CharacterDir + "/Pep_A.prefab");
            var pepBAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                ContentPaths.CharacterDir + "/Pep_B.prefab");
            var pepAGo = pepAAsset != null
                ? PrefabUtility.InstantiatePrefab(pepAAsset, root.transform) as GameObject
                : null;
            var pepBGo = pepBAsset != null
                ? PrefabUtility.InstantiatePrefab(pepBAsset, root.transform) as GameObject
                : null;

            pepA = pepAGo != null ? pepAGo.GetComponent<Pep>() : null;
            pepB = pepBGo != null ? pepBGo.GetComponent<Pep>() : null;
            if (pepAGo != null)
            {
                pepAGo.name = "HomePepA";
                pepAGo.transform.localPosition = new Vector3(-0.43f, 0.14f, -0.05f);
                pepAGo.transform.localRotation = Quaternion.Euler(0f, -10f, 0f);
                pepAGo.transform.localScale = Vector3.one * 1.28f;
            }
            if (pepBGo != null)
            {
                pepBGo.name = "HomePepB";
                pepBGo.transform.localPosition = new Vector3(0.43f, 0.14f, -0.05f);
                pepBGo.transform.localRotation = Quaternion.Euler(0f, 10f, 0f);
                pepBGo.transform.localScale = Vector3.one * 1.28f;
            }

            root.SetActive(false);
            return root;
        }

        private static GameObject WorldPrimitive(string name, Transform parent, PrimitiveType type,
            Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.layer = 2; // Ignore Raycast: title dressing is never a choice.
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            if (go.TryGetComponent<Collider>(out var collider)) Object.DestroyImmediate(collider);
            if (material != null) go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }

        /// <summary>Builds the only two navigation surfaces in the game.</summary>
        private static void BuildGameMenu(Transform canvas, GameObject homeDiorama, Pep homePepA, Pep homePepB,
            out GameMenu menu)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var ink = Hex("3D3354");

            var holder = new GameObject("Menu", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            Stretch(holder.GetComponent<RectTransform>());

            var home = new GameObject("Home", typeof(Image), typeof(CanvasGroup));
            home.transform.SetParent(holder.transform, false);
            Stretch(home.GetComponent<RectTransform>());
            home.GetComponent<Image>().color = new Color(0.97f, 0.95f, 0.91f, 0.16f);

            var title = Text(home.transform, "Title", font, 92, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 780f), new Vector2(940f, 130f));
            title.text = "SAVE PEPS";
            var subtitle = Text(home.transform, "Subtitle", font, 38, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 685f), new Vector2(940f, 70f));
            subtitle.text = "Two Peps. One little predicament.";
            subtitle.color = new Color(ink.r, ink.g, ink.b, 0.68f);

            var playGo = new GameObject("Play", typeof(Image), typeof(Button));
            playGo.transform.SetParent(home.transform, false);
            var playRt = playGo.GetComponent<RectTransform>();
            playRt.anchorMin = playRt.anchorMax = new Vector2(0.5f, 0.5f);
            playRt.sizeDelta = new Vector2(620f, 144f);
            playRt.anchoredPosition = new Vector2(0f, -590f);
            StylePanel(playGo.GetComponent<Image>(), Hex("FFB53E"));
            var playLabel = Text(playGo.transform, "Label", font, 54, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(590f, 120f));
            playLabel.text = "PLAY";

            var chooseGo = new GameObject("ChooseRound", typeof(Image), typeof(Button));
            chooseGo.transform.SetParent(home.transform, false);
            var chooseRt = chooseGo.GetComponent<RectTransform>();
            chooseRt.anchorMin = chooseRt.anchorMax = new Vector2(0.5f, 0.5f);
            chooseRt.sizeDelta = new Vector2(520f, 108f);
            chooseRt.anchoredPosition = new Vector2(0f, -745f);
            StylePanel(chooseGo.GetComponent<Image>(), new Color(0.97f, 0.95f, 0.91f, 0.92f));
            var chooseLabel = Text(chooseGo.transform, "Label", font, 40, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(490f, 92f));
            chooseLabel.text = "Choose round";

            var picker = new GameObject("RoundPicker", typeof(Image), typeof(CanvasGroup));
            picker.transform.SetParent(holder.transform, false);
            Stretch(picker.GetComponent<RectTransform>());
            picker.GetComponent<Image>().color = new Color(0.97f, 0.95f, 0.91f, 0.94f);

            var pickerTitle = Text(picker.transform, "Title", font, 66, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 880f), new Vector2(940f, 100f));
            pickerTitle.text = "Choose round";
            var pickerSubtitle = Text(picker.transform, "Subtitle", font, 34, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 800f), new Vector2(940f, 60f));
            pickerSubtitle.text = "Any available round, whenever you like.";
            pickerSubtitle.color = new Color(ink.r, ink.g, ink.b, 0.62f);

            var scrollGo = new GameObject("RoundScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(picker.transform, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRt.sizeDelta = new Vector2(940f, 1210f);
            scrollRt.anchoredPosition = new Vector2(0f, -5f);
            scrollGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.001f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(scrollGo.transform, false);
            Stretch(viewport.GetComponent<RectTransform>());

            var content = new GameObject("Rounds", typeof(RectTransform), typeof(GridLayoutGroup));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0f, 1210f);
            contentRt.anchoredPosition = Vector2.zero;
            var grid = content.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(430f, 172f);
            grid.spacing = new Vector2(28f, 24f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
            grid.childAlignment = TextAnchor.UpperCenter;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.inertia = true;
            scroll.decelerationRate = 0.12f;
            scroll.scrollSensitivity = 28f;

            var template = BuildRoundPickerItem(content.transform, font);

            var backGo = new GameObject("Back", typeof(Image), typeof(Button));
            backGo.transform.SetParent(picker.transform, false);
            var backRt = backGo.GetComponent<RectTransform>();
            backRt.anchorMin = backRt.anchorMax = new Vector2(0.5f, 0.5f);
            backRt.sizeDelta = new Vector2(360f, 100f);
            backRt.anchoredPosition = new Vector2(0f, -900f);
            StylePanel(backGo.GetComponent<Image>(), new Color(1f, 1f, 1f, 0.38f));
            var backLabel = Text(backGo.transform, "Label", font, 36, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(330f, 84f));
            backLabel.text = "Back";

            menu = holder.AddComponent<GameMenu>();
            Wire(menu, "_homeRoot", home);
            Wire(menu, "_playButton", playGo.GetComponent<Button>());
            Wire(menu, "_chooseButton", chooseGo.GetComponent<Button>());
            Wire(menu, "_homeDiorama", homeDiorama);
            Wire(menu, "_homePepA", homePepA);
            Wire(menu, "_homePepB", homePepB);
            Wire(menu, "_pickerRoot", picker);
            Wire(menu, "_pickerContent", content.transform);
            Wire(menu, "_itemTemplate", template);
            Wire(menu, "_backButton", backGo.GetComponent<Button>());

            home.SetActive(false);
            picker.SetActive(false);
        }

        private static RoundPickerItem BuildRoundPickerItem(Transform parent, Font font)
        {
            var ink = Hex("3D3354");
            var itemGo = new GameObject("RoundTemplate", typeof(Image), typeof(Button));
            itemGo.transform.SetParent(parent, false);
            StylePanel(itemGo.GetComponent<Image>(), Hex("F7F3E8"));

            var roundLabel = Text(itemGo.transform, "Round", font, 38, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(390f, 54f));
            roundLabel.text = "ROUND 1";
            var statusLabel = Text(itemGo.transform, "Status", font, 25, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -9f), new Vector2(390f, 42f));
            statusLabel.text = "NEW";
            statusLabel.color = new Color(ink.r, ink.g, ink.b, 0.62f);

            var marks = new MasteryMarkGraphic[RoundDefinition.RescuesPerRound];
            for (var i = 0; i < marks.Length; i++)
            {
                var markGo = new GameObject($"Mark_{i}", typeof(CanvasRenderer), typeof(MasteryMarkGraphic));
                markGo.transform.SetParent(itemGo.transform, false);
                var rt = markGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(30f, 30f);
                rt.anchoredPosition = new Vector2((i - 1) * 42f, -57f);
                marks[i] = markGo.GetComponent<MasteryMarkGraphic>();
            }

            var item = itemGo.AddComponent<RoundPickerItem>();
            Wire(item, "_button", itemGo.GetComponent<Button>());
            Wire(item, "_panel", itemGo.GetComponent<Image>());
            Wire(item, "_roundLabel", roundLabel);
            Wire(item, "_statusLabel", statusLabel);

            var so = new SerializedObject(item);
            var marksProp = so.FindProperty("_marks");
            marksProp.arraySize = marks.Length;
            for (var i = 0; i < marks.Length; i++)
            {
                marksProp.GetArrayElementAtIndex(i).objectReferenceValue = marks[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            itemGo.SetActive(false);
            return item;
        }

        /// <summary>
        /// The round-complete card. It sits inside the same canvas as the HUD
        /// and covers it, because it is a beat rather than a screen — the
        /// player should feel the round land and then be back in a diorama,
        /// not navigate anywhere.
        /// </summary>
        private static void BuildRoundCard(Transform canvas, out RoundCompleteCard card)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var ink = Hex("3D3354");

            // The component lives on an always-active holder and toggles a
            // child panel. Putting it on the panel itself would mean Awake
            // never runs while the card is hidden, and the first Show would
            // switch the object on only for Awake's Hide to switch it back
            // off — a card that never appears, from code that looks correct.
            var holder = new GameObject("RoundComplete", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            Stretch(holder.GetComponent<RectTransform>());

            var root = new GameObject("Overlay", typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(holder.transform, false);
            Stretch(root.GetComponent<RectTransform>());
            // The last reunion stays visible around a physical-looking card.
            // A light ink veil grounds it without washing the solved toy away.
            root.GetComponent<Image>().color = new Color(ink.r, ink.g, ink.b, 0.16f);

            var shadow = new GameObject("CardShadow", typeof(Image));
            shadow.transform.SetParent(root.transform, false);
            var shadowRt = shadow.GetComponent<RectTransform>();
            shadowRt.anchorMin = shadowRt.anchorMax = new Vector2(0.5f, 0.5f);
            shadowRt.sizeDelta = new Vector2(900f, 900f);
            shadowRt.anchoredPosition = new Vector2(0f, -18f);
            StylePanel(shadow.GetComponent<Image>(), new Color(ink.r, ink.g, ink.b, 0.19f));
            shadow.GetComponent<Image>().raycastTarget = false;

            var panelGo = new GameObject("ToyCard", typeof(Image));
            panelGo.transform.SetParent(root.transform, false);
            var panel = panelGo.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(880f, 900f);
            panel.anchoredPosition = Vector2.zero;
            StylePanel(panelGo.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f, 0.98f));
            panelGo.GetComponent<Image>().raycastTarget = false;

            var ribbon = new GameObject("Ribbon", typeof(Image));
            ribbon.transform.SetParent(panel, false);
            var ribbonRt = ribbon.GetComponent<RectTransform>();
            ribbonRt.anchorMin = ribbonRt.anchorMax = new Vector2(0.5f, 0.5f);
            ribbonRt.sizeDelta = new Vector2(270f, 14f);
            ribbonRt.anchoredPosition = new Vector2(0f, 360f);
            StylePanel(ribbon.GetComponent<Image>(), Hex("FFB53E"));
            ribbon.GetComponent<Image>().raycastTarget = false;

            var title = Text(panel, "Title", font, 36, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 296f), new Vector2(790f, 60f));
            title.fontStyle = FontStyle.Bold;
            title.color = new Color(ink.r, ink.g, ink.b, 0.72f);

            var subtitle = Text(panel, "Subtitle", font, 42, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 224f), new Vector2(790f, 70f));
            subtitle.fontStyle = FontStyle.Bold;

            var marks = new MasteryMarkGraphic[RoundDefinition.RescuesPerRound];
            for (var i = 0; i < marks.Length; i++)
            {
                var markGo = new GameObject($"CardMark_{i}", typeof(CanvasRenderer), typeof(MasteryMarkGraphic));
                markGo.transform.SetParent(panel, false);
                var rt = markGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(144f, 144f);
                rt.anchoredPosition = new Vector2((i - 1) * 220f, 68f);
                marks[i] = markGo.GetComponent<MasteryMarkGraphic>();

                var number = Text(panel, $"Rescue_{i + 1}", font, 24, ink,
                    new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 220f, -24f), new Vector2(140f, 38f));
                number.text = $"RESCUE {i + 1}";
                number.color = new Color(ink.r, ink.g, ink.b, 0.52f);
            }

            var continueGo = new GameObject("Continue", typeof(Image), typeof(Button));
            continueGo.transform.SetParent(panel, false);
            var continueRt = continueGo.GetComponent<RectTransform>();
            continueRt.anchorMin = continueRt.anchorMax = new Vector2(0.5f, 0.5f);
            continueRt.sizeDelta = new Vector2(650f, 132f);
            continueRt.anchoredPosition = new Vector2(0f, -182f);
            StylePanel(continueGo.GetComponent<Image>(), Hex("FFB53E"));
            AddShadow(continueGo.GetComponent<Image>(), new Color(ink.r, ink.g, ink.b, 0.18f),
                new Vector2(0f, -6f));
            var continueLabel = Text(continueGo.transform, "Label", font, 46, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 112f));
            continueLabel.text = "Keep playing";
            continueLabel.fontStyle = FontStyle.Bold;

            // Direct round choice is a link, not a button: it is the rarer
            // intent and should not compete with Keep playing for the thumb.
            var replayGo = new GameObject("Replay", typeof(Image), typeof(Button));
            replayGo.transform.SetParent(panel, false);
            var replayRt = replayGo.GetComponent<RectTransform>();
            replayRt.anchorMin = replayRt.anchorMax = new Vector2(0.5f, 0.5f);
            replayRt.sizeDelta = new Vector2(480f, 96f);
            replayRt.anchoredPosition = new Vector2(0f, -326f);
            StylePanel(replayGo.GetComponent<Image>(), new Color(1f, 1f, 1f, 0.34f));
            var replayLabel = Text(replayGo.transform, "Label", font, 36, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(450f, 86f));
            replayLabel.text = "Choose round";
            replayLabel.color = new Color(ink.r, ink.g, ink.b, 0.72f);

            card = holder.AddComponent<RoundCompleteCard>();
            Wire(card, "_root", root);
            Wire(card, "_group", root.GetComponent<CanvasGroup>());
            Wire(card, "_panel", panel);
            Wire(card, "_title", title);
            Wire(card, "_subtitle", subtitle);
            Wire(card, "_continueButton", continueGo.GetComponent<Button>());
            Wire(card, "_continueLabel", continueLabel);
            Wire(card, "_replayButton", replayGo.GetComponent<Button>());
            Wire(card, "_replayLabel", replayLabel);

            var so = new SerializedObject(card);
            var marksProp = so.FindProperty("_marks");
            marksProp.arraySize = marks.Length;
            for (var i = 0; i < marks.Length; i++)
            {
                marksProp.GetArrayElementAtIndex(i).objectReferenceValue = marks[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
        }

        private static Text Text(Transform parent, string name, Font font, int size, Color color,
            Vector2 anchor, Vector2 position, Vector2 size2)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            // Text is visual content, never a hit target. Leaving this on made
            // transparent label rectangles steal taps from the 3D choices.
            t.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = size2;
            return t;
        }

        private static Sprite PanelSprite() =>
            UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        private static void StylePanel(Image image, Color color)
        {
            if (image == null) return;
            image.sprite = PanelSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
        }

        private static void AddShadow(Graphic graphic, Color color, Vector2 distance)
        {
            if (graphic == null) return;
            var shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void WireBool(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError($"[SavePeps] {target.GetType().Name} has no serialized field '{field}'.");
                return;
            }

            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Wire(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError($"[SavePeps] {target.GetType().Name} has no serialized field '{field}'.");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
