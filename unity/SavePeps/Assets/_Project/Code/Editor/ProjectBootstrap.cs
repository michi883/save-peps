using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// The project's build configuration, as code rather than as clicks.
    ///
    /// Everything here is settable by hand in the inspector, which is exactly
    /// why it is written down: these are the settings a Google Play upload
    /// rejects us for getting wrong, and re-deriving them from memory in
    /// release week is how that happens. Re-runnable at any time via
    /// Tools > Save Peps > Apply Project Settings.
    /// </summary>
    public static class ProjectBootstrap
    {
        private const string CompanyName = "michi883";
        private const string ProductName = "Save Peps";
        private const string AppIconPath = "Assets/_Project/Art/UI/AppIcon.png";
        private const string AppIconBackgroundPath = "Assets/_Project/Art/UI/AppIconBackground.png";
        private const string AppIconForegroundPath = "Assets/_Project/Art/UI/AppIconForeground.png";

        // FINAL. Reverse-domain namespace for sound.fan, which we control.
        // Immutable once the first bundle reaches Play — do not change.
        private const string ApplicationId = "fan.sound.savepeps";

        [MenuItem("Tools/Save Peps/Apply Project Settings")]
        public static void Apply()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;

            // Portrait only. The camera is fixed and the diorama is composed
            // to a 4:3 safe box; landscape has nothing to offer and would
            // silently break every scene's framing.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            // URP expects linear; gamma would wash out the palette ramps.
            PlayerSettings.colorSpace = ColorSpace.Linear;

            var android = NamedBuildTarget.Android;
            PlayerSettings.SetApplicationIdentifier(android, ApplicationId);
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIl2CppCompilerConfiguration(android, Il2CppCompilerConfiguration.Release);
            // Medium, not High. High strips MonoBehaviours that are only ever
            // reached through serialized prefab references, which on device
            // shows up as "the referenced script on this Behaviour is
            // missing" and a scene full of inert objects. Assets/link.xml
            // preserves our assembly on top of this.
            PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Medium);

            // 64-bit only. Play has not accepted 32-bit-only uploads for years,
            // and shipping both just inflates the download.
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            // Auto tracks the highest API the installed editor supports, which
            // is what keeps us on the right side of Play's annual target-API
            // bump. Verify the resolved value against Play's current
            // requirement before the production upload.
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // RevenueCat requires standard or singleTop; the default activity
            // is standard, but purchases get cancelled mid-flow if this ever
            // drifts to singleTask.
            PlayerSettings.Android.androidIsGame = true;
            ApplyAndroidIcons();

            // Play distributes app bundles, not APKs.
            EditorUserBuildSettings.buildAppBundle = true;

            AssetDatabase.SaveAssets();
            Debug.Log($"[SavePeps] Project settings applied: {ProductName} ({ApplicationId}), " +
                      $"portrait, linear, IL2CPP/ARM64, AAB, minSdk 24.");
        }

        private static void ApplyAndroidIcons()
        {
            var icon = LoadIcon(AppIconPath);
            var background = LoadIcon(AppIconBackgroundPath);
            var foreground = LoadIcon(AppIconForegroundPath);

            ApplySingleLayerIcons(AndroidPlatformIconKind.Legacy, icon);
            ApplySingleLayerIcons(AndroidPlatformIconKind.Round, icon);

            var adaptive = PlayerSettings.GetPlatformIcons(
                NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive);
            foreach (var slot in adaptive)
            {
                if (slot.maxLayerCount != 2)
                {
                    throw new BuildFailedException(
                        $"Android adaptive icon slot {slot.width}x{slot.height} expects " +
                        $"{slot.maxLayerCount} layers instead of background + foreground.");
                }
                slot.SetTextures(background, foreground);
            }
            PlayerSettings.SetPlatformIcons(
                NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive, adaptive);
        }

        private static void ApplySingleLayerIcons(PlatformIconKind kind, Texture2D texture)
        {
            var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
            foreach (var slot in icons)
            {
                slot.SetTextures(texture);
            }
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, icons);
        }

        private static Texture2D LoadIcon(string path)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture != null) return texture;
            throw new BuildFailedException($"Required Android icon is missing at '{path}'.");
        }

        /// <summary>Batchmode entry point: apply settings and switch to Android.</summary>
        public static void ApplyAndSwitchToAndroid()
        {
            Apply();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.Log("[SavePeps] Switching active build target to Android…");
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
        }
    }
}
