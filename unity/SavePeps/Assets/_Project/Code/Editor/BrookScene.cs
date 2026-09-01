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
            var revenueCatSettings = EnsureRevenueCatSettings();

            // Fixed camera, low FOV, tilted down: the tilt-shift toy read from
            // design/palette.md. Framing is tuned for portrait 9:19.5 and
            // must be accepted on the reference device (AGENTS.md §6).
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

            var homeDiorama = BuildHomeDiorama(out var homePepA, out var homePepB, out var homeHeart);
            var hud = BuildHud(out var hudComponent);
            BuildRoundCard(hud.transform, out var cardComponent);
            BuildGameMenu(hud.transform, homeDiorama, homeHeart, homePepA, homePepB, out var menuComponent);
            // Order is draw order: the pause sheet covers the shell, and
            // Progress covers the pause sheet it was opened from.
            BuildPauseOverlay(hud.transform, out var pauseComponent);
            BuildProgressPanel(hud.transform, out var progressComponent);
            BuildFullGameUnlockPanel(hud.transform, out var unlockComponent);

            var game = new GameObject("Game");
            var player = game.AddComponent<ChoreographyPlayer>();
            var router = game.AddComponent<TapRouter>();
            var feedback = game.AddComponent<Feedback>();
            var gameFeel = game.AddComponent<GameFeel>();
            // Added before the runner so its Awake captures the scene's own
            // lighting as the resting mood the shell returns to.
            var atmosphere = game.AddComponent<AtmosphereDirector>();
            var runner = game.AddComponent<RescueRunner>();

            Wire(router, "_camera", cam);
            Wire(gameFeel, "_camera", cam);
            Wire(atmosphere, "_camera", cam);
            Wire(atmosphere, "_sun", light);
            Wire(atmosphere, "_fill", fill);
            Wire(atmosphere, "_gameFeel", gameFeel);
            Wire(atmosphere, "_feedback", feedback);
            Wire(runner, "_atmosphere", atmosphere);
            Wire(runner, "_tapRouter", router);
            Wire(runner, "_player", player);
            Wire(runner, "_hud", hudComponent);
            Wire(runner, "_feedback", feedback);
            Wire(runner, "_gameFeel", gameFeel);
            Wire(menuComponent, "_feedback", feedback);
            Wire(cardComponent, "_feedback", feedback);
            Wire(pauseComponent, "_feedback", feedback);
            Wire(progressComponent, "_feedback", feedback);
            Wire(unlockComponent, "_feedback", feedback);
            // GameFlow owns sequencing now; the runner is handed one rescue
            // at a time rather than playing a fixed asset at boot.
            WireBool(runner, "_autoPlayOnStart", false);

            // The fake stays wired for Editor/PlayMode tests. Android players
            // select the RevenueCat source from the same GameFlow gate.
            var entitlements = game.AddComponent<FakeEntitlementService>();
            var purchases = game.AddComponent<Purchases>();
            purchases.useRuntimeSetup = true;
            purchases.productIdentifiers = new[] { StoreProducts.Lifetime };
            var revenueCat = game.AddComponent<RevenueCatEntitlementService>();
            purchases.listener = revenueCat;
            Wire(revenueCat, "_settings", revenueCatSettings);

            var flow = game.AddComponent<GameFlow>();
            Wire(flow, "_catalog", catalog);
            Wire(flow, "_runner", runner);
            Wire(flow, "_hud", hudComponent);
            Wire(flow, "_card", cardComponent);
            Wire(flow, "_menu", menuComponent);
            Wire(flow, "_pause", pauseComponent);
            Wire(flow, "_progress", progressComponent);
            Wire(flow, "_unlock", unlockComponent);
            Wire(flow, "_feedback", feedback);
            Wire(flow, "_entitlementSource", entitlements);
            Wire(flow, "_deviceEntitlementSource", revenueCat);

            // Last sibling wins draw order. Tester Mode is intentionally the
            // outermost development-only surface so it can be reached from
            // home, gameplay, or any shell state without hand-editing scene
            // state between catalogue checks.
            BuildTesterMode(hud.transform, flow, menuComponent, runner, router, entitlements,
                out var testerMode);
            Wire(flow, "_testerMode", testerMode);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log($"[SavePeps] Scene saved to {ScenePath} and set as the only build scene.");
            _ = hud;
        }

        private static RevenueCatSettings EnsureRevenueCatSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<RevenueCatSettings>(ContentPaths.RevenueCatSettingsPath);
            if (settings != null) return settings;

            settings = ScriptableObject.CreateInstance<RevenueCatSettings>();
            AssetDatabase.CreateAsset(settings, ContentPaths.RevenueCatSettingsPath);
            Debug.Log($"[SavePeps] Created RevenueCat settings at {ContentPaths.RevenueCatSettingsPath}.");
            return settings;
        }

        /// <summary>
        /// The persistent layer, kept to two toy plaques and one small control.
        ///
        /// The previous version stacked a status bar, a mark row and an
        /// objective bar down the top of the screen, which is the silhouette of
        /// an app rather than of a game. This consolidates status and mastery
        /// into one plaque — the marks already say which rescue is in play —
        /// puts the pause control opposite it, and leaves the objective as a
        /// single pill that announces itself and then shrinks away.
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

            // --- status plaque: where you are, and what you have earned -----
            var statusPlate = new GameObject("StatusPlate", typeof(Image));
            statusPlate.transform.SetParent(hudRoot.transform, false);
            var statusRt = statusPlate.GetComponent<RectTransform>();
            statusRt.anchorMin = statusRt.anchorMax = new Vector2(0.5f, 1f);
            statusRt.sizeDelta = new Vector2(500f, 96f);
            statusRt.anchoredPosition = new Vector2(-70f, -104f);
            StylePanel(statusPlate.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f, 0.88f));
            statusPlate.GetComponent<Image>().raycastTarget = false;
            AddShadow(statusPlate.GetComponent<Image>(), new Color(0.24f, 0.20f, 0.33f, 0.16f),
                new Vector2(0f, -5f));

            var roundLabel = Text(statusPlate.transform, "RoundLabel", font, 34, ink,
                new Vector2(0.5f, 0.5f), new Vector2(-120f, 0f), new Vector2(220f, 60f));
            roundLabel.fontStyle = FontStyle.Bold;
            roundLabel.alignment = TextAnchor.MiddleLeft;
            roundLabel.color = new Color(ink.r, ink.g, ink.b, 0.80f);

            var marks = new MasteryMarkGraphic[RoundDefinition.RescuesPerRound];
            for (var i = 0; i < marks.Length; i++)
            {
                var markGo = new GameObject($"Mark_{i}", typeof(CanvasRenderer), typeof(MasteryMarkGraphic));
                markGo.transform.SetParent(statusPlate.transform, false);
                var rt = markGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(48f, 48f);
                rt.anchoredPosition = new Vector2(42f + i * 68f, 0f);
                marks[i] = markGo.GetComponent<MasteryMarkGraphic>();
            }

            // --- the way out ------------------------------------------------
            var menuHolder = new GameObject("MenuButton", typeof(RectTransform), typeof(CanvasGroup));
            menuHolder.transform.SetParent(hudRoot.transform, false);
            var menuHolderRt = menuHolder.GetComponent<RectTransform>();
            menuHolderRt.anchorMin = menuHolderRt.anchorMax = new Vector2(0.5f, 1f);
            menuHolderRt.sizeDelta = new Vector2(136f, 136f);
            menuHolderRt.anchoredPosition = new Vector2(416f, -104f);

            var menuGo = new GameObject("Button", typeof(Image), typeof(Button), typeof(ToyButton));
            menuGo.transform.SetParent(menuHolder.transform, false);
            var menuRt = menuGo.GetComponent<RectTransform>();
            Stretch(menuRt);
            var menuImage = menuGo.GetComponent<Image>();
            menuImage.sprite = CircleSprite();
            menuImage.type = Image.Type.Simple;
            menuImage.color = new Color(1f, 0.97f, 0.88f, 0.88f);
            AddShadow(menuImage, new Color(0.24f, 0.20f, 0.33f, 0.16f), new Vector2(0f, -5f));

            // This control opens the whole in-game menu, not a literal pause:
            // three rounded horizontal bars describe that destination without
            // looking like the Roman numeral II on a small phone screen.
            for (var i = 0; i < 3; i++)
            {
                var bar = new GameObject($"Bar_{i}", typeof(Image));
                bar.transform.SetParent(menuGo.transform, false);
                var barRt = bar.GetComponent<RectTransform>();
                barRt.anchorMin = barRt.anchorMax = new Vector2(0.5f, 0.5f);
                barRt.sizeDelta = new Vector2(54f, 12f);
                barRt.anchoredPosition = new Vector2(0f, 20f - i * 20f);
                StylePanel(bar.GetComponent<Image>(), new Color(ink.r, ink.g, ink.b, 0.72f));
                bar.GetComponent<Image>().raycastTarget = false;
            }

            // --- objective --------------------------------------------------
            var goalPlate = new GameObject("GoalPlate", typeof(Image), typeof(CanvasGroup));
            goalPlate.transform.SetParent(hudRoot.transform, false);
            var goalRt = goalPlate.GetComponent<RectTransform>();
            goalRt.anchorMin = goalRt.anchorMax = new Vector2(0.5f, 1f);
            goalRt.sizeDelta = new Vector2(720f, 96f);
            goalRt.anchoredPosition = new Vector2(0f, -218f);
            StylePanel(goalPlate.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f, 0.92f));
            goalPlate.GetComponent<Image>().raycastTarget = false;
            AddShadow(goalPlate.GetComponent<Image>(), new Color(0.24f, 0.20f, 0.33f, 0.15f),
                new Vector2(0f, -5f));

            var goal = Text(goalPlate.transform, "Goal", font, 48, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 76f));
            goal.fontStyle = FontStyle.Bold;
            goal.horizontalOverflow = HorizontalWrapMode.Overflow;
            // The plate is resized to the line at runtime, so a stretched
            // anchor keeps the text inside it without a layout group.
            var goalTextRt = goal.rectTransform;
            goalTextRt.anchorMin = new Vector2(0f, 0.5f);
            goalTextRt.anchorMax = new Vector2(1f, 0.5f);
            goalTextRt.offsetMin = new Vector2(24f, -38f);
            goalTextRt.offsetMax = new Vector2(-24f, 38f);

            // Failure copy is one large glanceable line that follows the
            // selected prop, cut to the width of its own sentence.
            var tray = new GameObject("QuipRibbon", typeof(Image), typeof(CanvasGroup));
            tray.transform.SetParent(hudRoot.transform, false);
            var trayRt = tray.GetComponent<RectTransform>();
            trayRt.anchorMin = trayRt.anchorMax = new Vector2(0.5f, 0f);
            trayRt.sizeDelta = new Vector2(900f, 136f);
            trayRt.anchoredPosition = new Vector2(0f, 390f);
            StylePanel(tray.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f, 0.98f));
            tray.GetComponent<Image>().raycastTarget = false;
            AddShadow(tray.GetComponent<Image>(), new Color(0.24f, 0.20f, 0.33f, 0.22f),
                new Vector2(0f, -8f));

            var quip = Text(tray.transform, "Quip", font, 56, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(856f, 112f));
            quip.fontStyle = FontStyle.Bold;
            quip.horizontalOverflow = HorizontalWrapMode.Overflow;
            quip.verticalOverflow = VerticalWrapMode.Truncate;

            hud = canvasGo.AddComponent<RescueHud>();
            Wire(hud, "_root", hudRoot);
            Wire(hud, "_statusRect", statusRt);
            Wire(hud, "_roundLabel", roundLabel);
            Wire(hud, "_menuButton", menuGo.GetComponent<Button>());
            Wire(hud, "_menuGroup", menuHolder.GetComponent<CanvasGroup>());
            Wire(hud, "_goal", goal);
            Wire(hud, "_goalRect", goalRt);
            Wire(hud, "_goalGroup", goalPlate.GetComponent<CanvasGroup>());
            Wire(hud, "_tray", tray);
            Wire(hud, "_trayGroup", tray.GetComponent<CanvasGroup>());
            Wire(hud, "_trayRect", trayRt);
            Wire(hud, "_quip", quip);
            WireArray(hud, "_marks", marks);

            tray.SetActive(false);
            return canvasGo;
        }

        /// <summary>
        /// A tiny reusable title tableau made from the same Peps and palette
        /// as gameplay. It gives the home screen a character-led focal point
        /// without creating a second scene or one-off title artwork.
        /// </summary>
        private static GameObject BuildHomeDiorama(out Pep pepA, out Pep pepB, out Transform heartOut)
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

            // Floats above and between them, and now beats there — see the
            // rest-pose note in GameMenu for why it spent one build sitting on
            // the tabletop instead. Slightly larger than the original tableau
            // called for, because it is the one thing on this screen that says
            // what the game is about at arm's length.
            var heart = new GameObject("Heart").transform;
            heart.SetParent(root.transform, false);
            heart.localPosition = new Vector3(0f, 0.98f, -0.02f);
            var left = WorldPrimitive("Left", heart, PrimitiveType.Sphere,
                new Vector3(-0.063f, 0.052f, 0f), new Vector3(0.150f, 0.150f, 0.092f), coral);
            var right = WorldPrimitive("Right", heart, PrimitiveType.Sphere,
                new Vector3(0.063f, 0.052f, 0f), new Vector3(0.150f, 0.150f, 0.092f), coral);
            var point = WorldPrimitive("Point", heart, PrimitiveType.Cube,
                new Vector3(0f, -0.052f, 0f), new Vector3(0.184f, 0.184f, 0.086f), coral);
            point.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            heartOut = heart;
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

        /// <summary>
        /// Home and the round picker. Home keeps its two choices and spends
        /// its whole budget on presentation: a title that lands, a Play button
        /// that breathes and squashes under a thumb, and one small earned line
        /// that doubles as the way into Progress.
        /// </summary>
        private static void BuildGameMenu(Transform canvas, GameObject homeDiorama, Transform homeHeart,
            Pep homePepA, Pep homePepB, out GameMenu menu)
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

            // Title and strapline move as one object so the entrance is a
            // single gesture rather than two elements fading independently.
            var titleGroup = new GameObject("TitleGroup", typeof(RectTransform), typeof(CanvasGroup));
            titleGroup.transform.SetParent(home.transform, false);
            var titleRt = titleGroup.GetComponent<RectTransform>();
            titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.sizeDelta = new Vector2(960f, 250f);
            titleRt.anchoredPosition = new Vector2(0f, 735f);

            var title = Text(titleGroup.transform, "Title", font, 92, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 45f), new Vector2(940f, 130f));
            title.text = "SAVE PEPS";
            title.fontStyle = FontStyle.Bold;
            var subtitle = Text(titleGroup.transform, "Subtitle", font, 38, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), new Vector2(940f, 70f));
            subtitle.text = "Two Peps. One little predicament.";
            subtitle.color = new Color(ink.r, ink.g, ink.b, 0.68f);

            var play = ShellButton(home.transform, "Play", font, "PLAY",
                new Vector2(620f, 148f), new Vector2(0f, -590f), Hex("FFB53E"), 54, ink, breathe: 0.016f);
            var playLabel = play.GetComponentInChildren<Text>();
            var choose = ShellButton(home.transform, "ChooseRound", font, "Choose round",
                new Vector2(520f, 110f), new Vector2(0f, -748f),
                new Color(0.97f, 0.95f, 0.91f, 0.92f), 40, ink);

            // The one number home reports. GameFlow hides it entirely until
            // there is something to report, so a first launch stays two taps
            // and a couple.
            var stat = ShellButton(home.transform, "Progress", font, "0 SAVED",
                new Vector2(560f, 78f), new Vector2(0f, -880f),
                new Color(1f, 1f, 1f, 0.30f), 28, new Color(ink.r, ink.g, ink.b, 0.74f),
                shadow: false);
            var statLabel = stat.GetComponentInChildren<Text>();

            // Legal links are deliberately quiet utility actions. They must
            // be reachable in-app for Play review without competing with the
            // two gameplay choices or the earned Progress line.
            var privacy = ShellButton(home.transform, "Privacy", font, "Privacy",
                new Vector2(210f, 58f), new Vector2(-118f, -985f),
                new Color(1f, 1f, 1f, 0.18f), 25, new Color(ink.r, ink.g, ink.b, 0.68f),
                shadow: false);
            var terms = ShellButton(home.transform, "Terms", font, "Terms",
                new Vector2(180f, 58f), new Vector2(118f, -985f),
                new Color(1f, 1f, 1f, 0.18f), 25, new Color(ink.r, ink.g, ink.b, 0.68f),
                shadow: false);

            // These transparent hit areas sit over the title and tableau
            // characters. TesterMode shows them at boot; the seven-tap command
            // is the only way in, so there is nothing to find by accident.
            var secretHeart = SecretTapArea(home.transform, "TesterSecretHeart",
                new Vector2(1080f, 1550f), new Vector2(0f, 325f));
            var secretGreen = SecretTapArea(home.transform, "TesterSecretGreenPep",
                new Vector2(330f, 390f), new Vector2(250f, 150f));
            var secretPink = SecretTapArea(home.transform, "TesterSecretPinkPep",
                new Vector2(330f, 390f), new Vector2(-250f, 150f));

            var picker = new GameObject("RoundPicker", typeof(Image), typeof(CanvasGroup));
            picker.transform.SetParent(holder.transform, false);
            Stretch(picker.GetComponent<RectTransform>());
            picker.GetComponent<Image>().color = new Color(0.97f, 0.95f, 0.91f, 0.94f);

            var pickerTitle = Text(picker.transform, "Title", font, 66, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 880f), new Vector2(940f, 100f));
            pickerTitle.text = "Choose round";
            pickerTitle.fontStyle = FontStyle.Bold;
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

            var back = ShellButton(picker.transform, "Back", font, "Back",
                new Vector2(360f, 104f), new Vector2(0f, -900f),
                new Color(1f, 1f, 1f, 0.40f), 36, new Color(ink.r, ink.g, ink.b, 0.80f), shadow: false);

            menu = holder.AddComponent<GameMenu>();
            Wire(menu, "_homeRoot", home);
            Wire(menu, "_homeTitle", titleRt);
            Wire(menu, "_homeTitleGroup", titleGroup.GetComponent<CanvasGroup>());
            Wire(menu, "_playButton", play);
            Wire(menu, "_playLabel", playLabel);
            Wire(menu, "_chooseButton", choose);
            Wire(menu, "_statButton", stat);
            Wire(menu, "_statLabel", statLabel);
            Wire(menu, "_privacyButton", privacy);
            Wire(menu, "_termsButton", terms);
            Wire(menu, "_secretHeartButton", secretHeart);
            Wire(menu, "_secretGreenPepButton", secretGreen);
            Wire(menu, "_secretPinkPepButton", secretPink);
            Wire(menu, "_homeDiorama", homeDiorama);
            Wire(menu, "_homeHeart", homeHeart);
            Wire(menu, "_homePepA", homePepA);
            Wire(menu, "_homePepB", homePepB);
            Wire(menu, "_pickerRoot", picker);
            Wire(menu, "_pickerContent", content.transform);
            Wire(menu, "_itemTemplate", template);
            Wire(menu, "_backButton", back);

            home.SetActive(false);
            picker.SetActive(false);
        }

        private static Button SecretTapArea(Transform parent, string name, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;

            var button = go.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            return button;
        }

        private static RoundPickerItem BuildRoundPickerItem(Transform parent, Font font)
        {
            var ink = Hex("3D3354");
            var itemGo = new GameObject("RoundTemplate", typeof(Image), typeof(Button), typeof(ToyButton));
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

            NeutraliseTint(itemGo.GetComponent<Button>());
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
        /// The pause sheet: the player's way out of a rescue.
        ///
        /// A bottom sheet rather than a full screen, so the diorama stays
        /// visible above it and stepping out never looks like stepping away.
        /// Sound and haptics are two toggles at its foot rather than a sixth
        /// destination — that is the entire settings surface this game has.
        /// </summary>
        private static void BuildPauseOverlay(Transform canvas, out PauseOverlay pause)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var ink = Hex("3D3354");

            // As with the round card, the component lives on an always-active
            // holder and toggles a child: a MonoBehaviour that switches its own
            // GameObject off in Awake never gets an Awake at all.
            var holder = new GameObject("Pause", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            Stretch(holder.GetComponent<RectTransform>());

            var root = new GameObject("Overlay", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(holder.transform, false);
            Stretch(root.GetComponent<RectTransform>());

            var scrimGo = new GameObject("Scrim", typeof(Image), typeof(Button));
            scrimGo.transform.SetParent(root.transform, false);
            Stretch(scrimGo.GetComponent<RectTransform>());
            scrimGo.GetComponent<Image>().color = new Color(ink.r, ink.g, ink.b, 0.44f);
            scrimGo.GetComponent<Button>().transition = Selectable.Transition.None;

            var sheetGo = new GameObject("Sheet", typeof(Image));
            sheetGo.transform.SetParent(root.transform, false);
            var sheet = sheetGo.GetComponent<RectTransform>();
            sheet.anchorMin = sheet.anchorMax = new Vector2(0.5f, 0f);
            sheet.pivot = new Vector2(0.5f, 0f);
            sheet.sizeDelta = new Vector2(1010f, 880f);
            sheet.anchoredPosition = Vector2.zero;
            StylePanel(sheetGo.GetComponent<Image>(), Hex("FFF6E4"));
            AddShadow(sheetGo.GetComponent<Image>(), new Color(ink.r, ink.g, ink.b, 0.26f),
                new Vector2(0f, 8f));

            var handle = new GameObject("Handle", typeof(Image));
            handle.transform.SetParent(sheet, false);
            var handleRt = handle.GetComponent<RectTransform>();
            handleRt.anchorMin = handleRt.anchorMax = new Vector2(0.5f, 0.5f);
            handleRt.sizeDelta = new Vector2(150f, 12f);
            handleRt.anchoredPosition = new Vector2(0f, 388f);
            StylePanel(handle.GetComponent<Image>(), new Color(ink.r, ink.g, ink.b, 0.18f));
            handle.GetComponent<Image>().raycastTarget = false;

            // Secondary rows sit on a cream sheet, so they need a plate a
            // shade deeper than the sheet or they dissolve into it.
            var quiet = Hex("F0E3C8");
            var resume = ShellButton(sheet, "Resume", font, "Resume",
                new Vector2(800f, 136f), new Vector2(0f, 290f), Hex("FFB53E"), 48, ink);
            var progress = ShellButton(sheet, "Progress", font, "Progress",
                new Vector2(800f, 110f), new Vector2(0f, 150f), quiet, 38, ink);
            var choose = ShellButton(sheet, "ChooseRound", font, "Choose round",
                new Vector2(800f, 110f), new Vector2(0f, 25f), quiet, 38, ink);
            var home = ShellButton(sheet, "Home", font, "Home",
                new Vector2(800f, 110f), new Vector2(0f, -100f), quiet, 38, ink);
            var testerTools = ShellButton(sheet, "PauseTesterTools", font, "TESTER TOOLS",
                new Vector2(800f, 96f), new Vector2(0f, -215f), new Color(ink.r, ink.g, ink.b, 0.86f), 28, Color.white, shadow: false);

            var sound = ShellButton(sheet, "Sound", font, "SOUND ON",
                new Vector2(388f, 106f), new Vector2(-206f, -330f), Hex("5CCCAE"), 28, ink);
            var haptics = ShellButton(sheet, "Haptics", font, "VIBRATION ON",
                new Vector2(388f, 106f), new Vector2(206f, -330f), Hex("5CCCAE"), 28, ink);

            pause = holder.AddComponent<PauseOverlay>();
            Wire(pause, "_root", root);
            Wire(pause, "_group", root.GetComponent<CanvasGroup>());
            Wire(pause, "_sheet", sheet);
            Wire(pause, "_scrim", scrimGo.GetComponent<Button>());
            Wire(pause, "_resumeButton", resume);
            Wire(pause, "_progressButton", progress);
            Wire(pause, "_chooseButton", choose);
            Wire(pause, "_homeButton", home);
            Wire(pause, "_testerToolsButton", testerTools);
            Wire(pause, "_soundToggle", sound);
            Wire(pause, "_soundPanel", sound.GetComponent<Image>());
            Wire(pause, "_soundLabel", sound.GetComponentInChildren<Text>());
            Wire(pause, "_hapticsToggle", haptics);
            Wire(pause, "_hapticsPanel", haptics.GetComponent<Image>());
            Wire(pause, "_hapticsLabel", haptics.GetComponentInChildren<Text>());

            testerTools.gameObject.SetActive(false);
            root.SetActive(false);
        }

        /// <summary>
        /// The progress shelf. Three tiles summarise, one row per round shows
        /// the actual marks. Every number is derived from the same save the
        /// HUD reads, so this can never disagree with what the player earned.
        /// </summary>
        private static void BuildProgressPanel(Transform canvas, out ProgressPanel progress)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var ink = Hex("3D3354");

            var holder = new GameObject("Progress", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            Stretch(holder.GetComponent<RectTransform>());

            var root = new GameObject("Overlay", typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(holder.transform, false);
            Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = new Color(ink.r, ink.g, ink.b, 0.34f);

            var panelGo = new GameObject("Shelf", typeof(Image));
            panelGo.transform.SetParent(root.transform, false);
            var panel = panelGo.GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            // Height is set at runtime from the number of rounds; three rounds
            // inside a full-height shelf was mostly empty cream.
            panel.sizeDelta = new Vector2(1000f, 1000f);
            panel.anchoredPosition = Vector2.zero;
            StylePanel(panelGo.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f, 0.99f));
            AddShadow(panelGo.GetComponent<Image>(), new Color(ink.r, ink.g, ink.b, 0.22f),
                new Vector2(0f, -10f));

            var title = Text(panel, "Title", font, 62, ink,
                new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(900f, 96f));
            title.text = "Progress";
            title.fontStyle = FontStyle.Bold;

            var ribbon = new GameObject("Ribbon", typeof(Image));
            ribbon.transform.SetParent(panel, false);
            var ribbonRt = ribbon.GetComponent<RectTransform>();
            ribbonRt.anchorMin = ribbonRt.anchorMax = new Vector2(0.5f, 1f);
            ribbonRt.sizeDelta = new Vector2(240f, 14f);
            ribbonRt.anchoredPosition = new Vector2(0f, -168f);
            StylePanel(ribbon.GetComponent<Image>(), Hex("FFB53E"));
            ribbon.GetComponent<Image>().raycastTarget = false;

            var tiles = new RectTransform[3];
            var values = new Text[3];
            var captions = new[] { "ROUNDS", "PERFECT", "FIRST TRY" };
            var tints = new[] { Hex("DFF4E9"), Hex("FFF0C2"), Hex("E9E0F2") };
            for (var i = 0; i < tiles.Length; i++)
            {
                var tileGo = new GameObject($"Tile_{i}", typeof(Image));
                tileGo.transform.SetParent(panel, false);
                var tile = tileGo.GetComponent<RectTransform>();
                tile.anchorMin = tile.anchorMax = new Vector2(0.5f, 1f);
                tile.sizeDelta = new Vector2(292f, 214f);
                tile.anchoredPosition = new Vector2((i - 1) * 306f, -318f);
                StylePanel(tileGo.GetComponent<Image>(), tints[i]);
                tileGo.GetComponent<Image>().raycastTarget = false;

                values[i] = Text(tile, "Value", font, 74, ink,
                    new Vector2(0.5f, 0.5f), new Vector2(0f, 26f), new Vector2(268f, 96f));
                values[i].fontStyle = FontStyle.Bold;
                values[i].text = "0";

                var caption = Text(tile, "Caption", font, 24, ink,
                    new Vector2(0.5f, 0.5f), new Vector2(0f, -62f), new Vector2(268f, 40f));
                caption.text = captions[i];
                caption.color = new Color(ink.r, ink.g, ink.b, 0.60f);
                tiles[i] = tile;
            }

            var scrollGo = new GameObject("RoundScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(panel, false);
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.anchorMin = scrollRt.anchorMax = new Vector2(0.5f, 1f);
            scrollRt.pivot = new Vector2(0.5f, 1f);
            scrollRt.sizeDelta = new Vector2(920f, 460f);
            scrollRt.anchoredPosition = new Vector2(0f, -ProgressPanel.HeaderHeight);
            scrollGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.001f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
            viewport.transform.SetParent(scrollGo.transform, false);
            Stretch(viewport.GetComponent<RectTransform>());

            var content = new GameObject("Rounds", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.sizeDelta = new Vector2(0f, 1060f);
            contentRt.anchoredPosition = Vector2.zero;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.inertia = true;
            scroll.decelerationRate = 0.12f;
            scroll.scrollSensitivity = 28f;

            var rowTemplate = BuildProgressRow(content.transform, font);

            var back = ShellButton(panel, "Back", font, "Back",
                new Vector2(420f, 116f), Vector2.zero, Hex("FFB53E"), 40, ink);
            var backRt = (RectTransform)back.transform;
            backRt.anchorMin = backRt.anchorMax = new Vector2(0.5f, 0f);
            backRt.anchoredPosition = new Vector2(0f, 96f);

            progress = holder.AddComponent<ProgressPanel>();
            Wire(progress, "_root", root);
            Wire(progress, "_group", root.GetComponent<CanvasGroup>());
            Wire(progress, "_panel", panel);
            Wire(progress, "_roundsValue", values[0]);
            Wire(progress, "_perfectValue", values[1]);
            Wire(progress, "_starsValue", values[2]);
            Wire(progress, "_content", contentRt);
            Wire(progress, "_rowTemplate", rowTemplate);
            Wire(progress, "_scroll", scroll);
            Wire(progress, "_backButton", back);
            WireArray(progress, "_tiles", tiles);

            root.SetActive(false);
        }

        /// <summary>
        /// The whole purchase UX in one toy card. The price label starts in a
        /// loading state and is replaced only by RevenueCat's localized store
        /// price; no currency amount is authored into the scene.
        /// </summary>
        private static void BuildFullGameUnlockPanel(Transform canvas, out FullGameUnlockPanel unlock)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var ink = Hex("3D3354");

            var holder = new GameObject("FullGameUnlock", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            Stretch(holder.GetComponent<RectTransform>());

            var root = new GameObject("Overlay", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(holder.transform, false);
            Stretch(root.GetComponent<RectTransform>());

            var scrimGo = new GameObject("Scrim", typeof(Image), typeof(Button));
            scrimGo.transform.SetParent(root.transform, false);
            Stretch(scrimGo.GetComponent<RectTransform>());
            scrimGo.GetComponent<Image>().color = new Color(ink.r, ink.g, ink.b, 0.62f);
            NeutraliseTint(scrimGo.GetComponent<Button>());

            var cardGo = new GameObject("UnlockCard", typeof(Image));
            cardGo.transform.SetParent(root.transform, false);
            var card = cardGo.GetComponent<RectTransform>();
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(920f, 910f);
            card.anchoredPosition = new Vector2(0f, 20f);
            StylePanel(cardGo.GetComponent<Image>(), Hex("FFF6E4"));
            AddShadow(cardGo.GetComponent<Image>(), new Color(ink.r, ink.g, ink.b, 0.28f),
                new Vector2(0f, -12f));

            var close = ShellButton(card, "UnlockClose", font, "X",
                new Vector2(86f, 86f), new Vector2(378f, 365f),
                new Color(ink.r, ink.g, ink.b, 0.88f), 30, Color.white, shadow: false);

            var heart = Text(card, "Heart", font, 92, Hex("F06C7A"),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 314f), new Vector2(180f, 120f));
            heart.text = "♥";

            var title = Text(card, "Title", font, 64, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 205f), new Vector2(790f, 92f));
            title.text = "Unlock Full Game";
            title.fontStyle = FontStyle.Bold;

            var subtitle = Text(card, "Subtitle", font, 42, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 112f), new Vector2(790f, 70f));
            subtitle.text = "Get Rounds 11–12";
            subtitle.fontStyle = FontStyle.Bold;

            var detail = Text(card, "Detail", font, 31, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), new Vector2(760f, 76f));
            detail.text = "Six more rescues · yours forever";
            detail.color = new Color(ink.r, ink.g, ink.b, 0.70f);

            var purchase = ShellButton(card, "UnlockPurchase", font, "Loading price…",
                new Vector2(790f, 148f), new Vector2(0f, -105f), Hex("FFB53E"), 36, ink,
                breathe: 0.018f);
            var restore = ShellButton(card, "UnlockRestore", font, "Restore Purchase",
                new Vector2(600f, 106f), new Vector2(0f, -245f), Hex("F0E3C8"), 31, ink);

            var status = Text(card, "Status", font, 27, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -350f), new Vector2(760f, 90f));
            status.text = string.Empty;
            status.color = new Color(ink.r, ink.g, ink.b, 0.76f);

            unlock = holder.AddComponent<FullGameUnlockPanel>();
            Wire(unlock, "_root", root);
            Wire(unlock, "_group", root.GetComponent<CanvasGroup>());
            Wire(unlock, "_card", card);
            Wire(unlock, "_scrim", scrimGo.GetComponent<Button>());
            Wire(unlock, "_closeButton", close);
            Wire(unlock, "_purchaseButton", purchase);
            Wire(unlock, "_purchaseLabel", purchase.GetComponentInChildren<Text>());
            Wire(unlock, "_restoreButton", restore);
            Wire(unlock, "_restoreLabel", restore.GetComponentInChildren<Text>());
            Wire(unlock, "_statusLabel", status);

            root.SetActive(false);
        }

        private static ProgressRow BuildProgressRow(Transform parent, Font font)
        {
            var ink = Hex("3D3354");
            var rowGo = new GameObject("ProgressRowTemplate", typeof(Image));
            rowGo.transform.SetParent(parent, false);
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.sizeDelta = new Vector2(920f, 118f);
            StylePanel(rowGo.GetComponent<Image>(), Hex("FFFBEE"));
            rowGo.GetComponent<Image>().raycastTarget = false;

            var label = Text(rowGo.transform, "Round", font, 36, ink,
                new Vector2(0.5f, 0.5f), new Vector2(-286f, 20f), new Vector2(320f, 48f));
            label.text = "ROUND 1";
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleLeft;

            var status = Text(rowGo.transform, "Status", font, 24, ink,
                new Vector2(0.5f, 0.5f), new Vector2(-286f, -26f), new Vector2(320f, 40f));
            status.text = "UNPLAYED";
            status.alignment = TextAnchor.MiddleLeft;
            status.color = new Color(ink.r, ink.g, ink.b, 0.58f);

            var marks = new MasteryMarkGraphic[RoundDefinition.RescuesPerRound];
            for (var i = 0; i < marks.Length; i++)
            {
                var markGo = new GameObject($"Mark_{i}", typeof(CanvasRenderer), typeof(MasteryMarkGraphic));
                markGo.transform.SetParent(rowGo.transform, false);
                var rt = markGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(58f, 58f);
                rt.anchoredPosition = new Vector2(248f + i * 76f, 0f);
                marks[i] = markGo.GetComponent<MasteryMarkGraphic>();
            }

            var row = rowGo.AddComponent<ProgressRow>();
            Wire(row, "_panel", rowGo.GetComponent<Image>());
            Wire(row, "_label", label);
            Wire(row, "_status", status);
            WireArray(row, "_marks", marks);

            rowGo.SetActive(false);
            return row;
        }

        /// <summary>
        /// Development-build-only mode switch, catalogue target, and explicit
        /// profile tools. User Mode has no visible entry point; the title
        /// tableau's secret sequence activates the small indicator and sheet.
        /// </summary>
        private static void BuildTesterMode(Transform canvas, GameFlow flow, GameMenu menu,
            RescueRunner runner, TapRouter router, FakeEntitlementService entitlements,
            out TesterMode tester)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var ink = Hex("3D3354");
            var cream = Hex("FFFBEE");
            var gold = Hex("FFB53E");

            var holder = new GameObject("TesterMode", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            Stretch(holder.GetComponent<RectTransform>());

            var indicator = ShellButton(holder.transform, "TesterIndicator", font, "TESTER",
                new Vector2(210f, 64f), Vector2.zero, new Color(ink.r, ink.g, ink.b, 0.86f),
                24, Color.white, shadow: false);
            var indicatorRt = indicator.GetComponent<RectTransform>();
            indicatorRt.anchorMin = indicatorRt.anchorMax = new Vector2(0f, 1f);
            indicatorRt.anchoredPosition = new Vector2(130f, -104f);
            indicator.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;

            var root = new GameObject("TesterOverlay", typeof(Image), typeof(CanvasGroup));
            root.transform.SetParent(holder.transform, false);
            Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = new Color(ink.r, ink.g, ink.b, 0.72f);

            var cardGo = new GameObject("TesterCard", typeof(Image));
            cardGo.transform.SetParent(root.transform, false);
            var card = cardGo.GetComponent<RectTransform>();
            card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(1010f, 1780f);
            card.anchoredPosition = Vector2.zero;
            StylePanel(cardGo.GetComponent<Image>(), new Color(1f, 0.97f, 0.88f, 0.99f));

            var title = Text(card, "Title", font, 40, ink, new Vector2(0.5f, 0.5f),
                new Vector2(-230f, 570f), new Vector2(420f, 65f));
            title.text = "TESTER TOOLS";
            title.fontStyle = FontStyle.Bold;

            var exitMode = ShellButton(card, "TesterExitMode", font, "NORMAL MODE",
                new Vector2(230f, 66f), new Vector2(170f, 570f), cream,
                24, ink, shadow: false);

            var close = ShellButton(card, "TesterClose", font, "CLOSE",
                new Vector2(150f, 66f), new Vector2(385f, 570f), new Color(ink.r, ink.g, ink.b, 0.86f),
                26, Color.white, shadow: false);

            // 1. GO TO
            SectionLabel(card, font, ink, "GO TO", 495f);
            var goToSummary = Text(card, "TesterGoToSummary", font, 28, ink, new Vector2(0.5f, 0.5f),
                new Vector2(0f, 455f), new Vector2(900f, 40f));
            goToSummary.text = "ROUND 1 · RESCUE 1";
            goToSummary.fontStyle = FontStyle.Bold;
            goToSummary.alignment = TextAnchor.MiddleCenter;
            goToSummary.color = ink;

            var roundButtons = new Button[12];
            for (var i = 0; i < roundButtons.Length; i++)
            {
                var column = i % 6;
                var row = i / 6;
                var x = -365f + column * 146f;
                var y = 375f - row * 80f;
                roundButtons[i] = ShellButton(card, $"TesterRound_{i + 1}", font, (i + 1).ToString(),
                    new Vector2(138f, 70f), new Vector2(x, y), cream, 32, ink, shadow: false);
            }

            var rescueButtons = new Button[RoundDefinition.RescuesPerRound];
            for (var i = 0; i < rescueButtons.Length; i++)
            {
                rescueButtons[i] = ShellButton(card, $"TesterRescue_{i + 1}", font, $"RESCUE {i + 1}",
                    new Vector2(278f, 76f), new Vector2((i - 1) * 295f, 205f), cream, 28, ink,
                    shadow: false);
            }

            var playRescue = ShellButton(card, "TesterPlayRescue", font,
                "PLAY RESCUE", new Vector2(870f, 96f),
                new Vector2(0f, 100f), gold, 34, ink, shadow: false);

            // 2. ACCESS
            SectionLabel(card, font, ink, "ACCESS", 10f);
            var accessSub = Text(card, "AccessSubtext", font, 26, ink, new Vector2(0.5f, 0.5f),
                new Vector2(0f, -30f), new Vector2(900f, 38f));
            accessSub.text = "Simulate player entitlement";
            accessSub.alignment = TextAnchor.MiddleCenter;
            accessSub.color = new Color(ink.r, ink.g, ink.b, 0.90f);

            var free = ShellButton(card, "TesterFree", font, "FREE", new Vector2(425f, 88f),
                new Vector2(-225f, -105f), cream, 30, ink, shadow: false);
            var fullGame = ShellButton(card, "TesterFullGame", font, "FULL GAME",
                new Vector2(425f, 88f), new Vector2(225f, -105f), cream, 30, ink, shadow: false);

            // 3. PURCHASE — real store state, deliberately separate from ACCESS.
            SectionLabel(card, font, ink, "PURCHASE", -195f);
            var openUnlock = ShellButton(card, "TesterOpenUnlock", font, "OPEN UNLOCK SCREEN",
                new Vector2(870f, 90f), new Vector2(0f, -270f), gold, 30, ink, shadow: false);
            var purchaseDiagnostics = Text(card, "TesterPurchaseDiagnostics", font, 25, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -395f), new Vector2(820f, 140f));
            purchaseDiagnostics.text =
                "Billing: Test Store\nEntitlement: FREE\nProduct: MISSING\nPrice: —";
            purchaseDiagnostics.lineSpacing = 1.05f;
            purchaseDiagnostics.alignment = TextAnchor.MiddleLeft;
            purchaseDiagnostics.color = new Color(ink.r, ink.g, ink.b, 0.88f);

            // 4. PROFILE
            SectionLabel(card, font, ink, "PROFILE", -515f);
            var profileSub = Text(card, "ProfileSubtext", font, 25, ink, new Vector2(0.5f, 0.5f),
                new Vector2(0f, -555f), new Vector2(900f, 44f));
            profileSub.text = "Erases all marks, completed rounds, and history back to a fresh install.";
            profileSub.alignment = TextAnchor.MiddleCenter;
            profileSub.color = new Color(ink.r, ink.g, ink.b, 0.90f);

            var clearProgress = ShellButton(card, "TesterClearProgress", font, "CLEAR ALL PROGRESS",
                new Vector2(870f, 90f), new Vector2(0f, -635f), new Color(0.96f, 0.91f, 0.91f, 1f),
                28, new Color(0.60f, 0.15f, 0.15f, 1f), shadow: false);

            var cancelClear = ShellButton(card, "TesterCancelClear", font, "CANCEL",
                new Vector2(320f, 62f), new Vector2(0f, -715f), cream, 24, ink, shadow: false);
            cancelClear.gameObject.SetActive(false);

            var note = Text(card, "SafetyNote", font, 25, ink, new Vector2(0.5f, 0.5f),
                new Vector2(0f, -805f), new Vector2(900f, 90f));
            note.text = "• Same game, but you can go anywhere.\n• Playing in Tester Mode records progress and unlocks.";
            note.lineSpacing = 1.2f;
            note.alignment = TextAnchor.MiddleCenter;
            note.color = new Color(ink.r, ink.g, ink.b, 0.85f);

            tester = holder.AddComponent<TesterMode>();
            Wire(tester, "_indicatorRoot", indicator.gameObject);
            Wire(tester, "_indicatorButton", indicator);
            Wire(tester, "_indicatorLabel", indicator.GetComponentInChildren<Text>());
            Wire(tester, "_root", root);
            Wire(tester, "_group", root.GetComponent<CanvasGroup>());
            Wire(tester, "_closeButton", close);
            Wire(tester, "_exitModeButton", exitMode);
            WireArray(tester, "_roundButtons", roundButtons);
            WireArray(tester, "_rescueButtons", rescueButtons);
            Wire(tester, "_playRescueButton", playRescue);
            Wire(tester, "_goToSelectionSummary", goToSummary);
            Wire(tester, "_freeButton", free);
            Wire(tester, "_fullGameButton", fullGame);
            Wire(tester, "_freeLabel", free.GetComponentInChildren<Text>());
            Wire(tester, "_fullGameLabel", fullGame.GetComponentInChildren<Text>());
            Wire(tester, "_openUnlockButton", openUnlock);
            Wire(tester, "_purchaseDiagnostics", purchaseDiagnostics);
            Wire(tester, "_clearProgressButton", clearProgress);
            Wire(tester, "_clearProgressLabel", clearProgress.GetComponentInChildren<Text>());
            Wire(tester, "_cancelClearButton", cancelClear);
            Wire(tester, "_flow", flow);
            Wire(tester, "_menu", menu);
            Wire(tester, "_runner", runner);
            Wire(tester, "_fakeEntitlements", entitlements);

            root.SetActive(false);
            indicator.gameObject.SetActive(false);
        }

        private static void SectionLabel(Transform parent, Font font, Color ink, string value, float y)
        {
            var label = Text(parent, value.Replace(' ', '_'), font, 26, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(900f, 44f));
            label.text = value;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleLeft;
            label.color = new Color(ink.r, ink.g, ink.b, 0.85f);
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

            var continueButton = ShellButton(panel, "Continue", font, "Keep playing",
                new Vector2(650f, 134f), new Vector2(0f, -182f), Hex("FFB53E"), 46, ink);
            var continueLabel = continueButton.GetComponentInChildren<Text>();

            // Direct round choice is a link, not a button: it is the rarer
            // intent and should not compete with Keep playing for the thumb.
            var replayButton = ShellButton(panel, "Replay", font, "Choose round",
                new Vector2(480f, 98f), new Vector2(0f, -326f), new Color(1f, 1f, 1f, 0.34f),
                36, new Color(ink.r, ink.g, ink.b, 0.72f), shadow: false);
            var replayLabel = replayButton.GetComponentInChildren<Text>();

            card = holder.AddComponent<RoundCompleteCard>();
            Wire(card, "_root", root);
            Wire(card, "_group", root.GetComponent<CanvasGroup>());
            Wire(card, "_panel", panel);
            Wire(card, "_title", title);
            Wire(card, "_subtitle", subtitle);
            Wire(card, "_continueButton", continueButton);
            Wire(card, "_continueLabel", continueLabel);
            Wire(card, "_replayButton", replayButton);
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

        /// <summary>
        /// Unity's built-in knob, used purely as a circle. An Image with no
        /// sprite draws a square, and the round controls in this shell have to
        /// be round without shipping a texture to do it.
        /// </summary>
        private static Sprite CircleSprite() =>
            UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        /// <summary>
        /// A labelled toy button: rounded plate, drop shadow, tactile press.
        /// Every button in the shell comes from here so none of them can drift
        /// into looking like a form control.
        /// </summary>
        private static Button ShellButton(Transform parent, string name, Font font, string label,
            Vector2 size, Vector2 position, Color plate, int fontSize, Color textColor,
            float breathe = 0f, bool shadow = true)
        {
            var go = new GameObject(name, typeof(Image), typeof(Button), typeof(ToyButton));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            StylePanel(go.GetComponent<Image>(), plate);
            NeutraliseTint(go.GetComponent<Button>());
            if (shadow)
            {
                AddShadow(go.GetComponent<Image>(), new Color(0.24f, 0.20f, 0.33f, 0.17f),
                    new Vector2(0f, -6f));
            }

            var text = Text(go.transform, "Label", font, fontSize, textColor,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(size.x - 40f, size.y - 24f));
            text.text = label;
            text.fontStyle = FontStyle.Bold;
            if (breathe > 0f) WireFloat(go.GetComponent<ToyButton>(), "_breathe", breathe);
            return go.GetComponent<Button>();
        }

        /// <summary>
        /// Bakes an identity colour block into the button.
        ///
        /// <see cref="ToyButton"/> switches uGUI's tint off at runtime, but
        /// whether that happens before or after Selectable's first state
        /// transition depends on component Awake order, and a panel that is
        /// activated while its CanvasGroup is still non-interactable can have
        /// the disabled tint written into its CanvasRenderer first. Making
        /// every state white removes the ordering question entirely: uGUI may
        /// tint all it likes, and the result is the plate colour either way.
        /// </summary>
        private static void NeutraliseTint(Button button)
        {
            if (button == null) return;
            button.transition = Selectable.Transition.None;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = Color.white;
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.white;
            colors.colorMultiplier = 1f;
            button.colors = colors;
        }

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

        private static void WireArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError($"[SavePeps] {target.GetType().Name} has no serialized field '{field}'.");
                return;
            }

            prop.arraySize = values.Length;
            for (var i = 0; i < values.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireFloat(Object target, string field, float value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError($"[SavePeps] {target.GetType().Name} has no serialized field '{field}'.");
                return;
            }

            prop.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
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
