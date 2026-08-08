using UnityEditor;
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

        // D6 in PLAN.md is still open. This is a working default — it must be
        // confirmed before the first Play upload, after which it is immutable.
        private const string ApplicationId = "com.michi883.savepeps";

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
            PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.High);

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

            // Play distributes app bundles, not APKs.
            EditorUserBuildSettings.buildAppBundle = true;

            AssetDatabase.SaveAssets();
            Debug.Log($"[SavePeps] Project settings applied: {ProductName} ({ApplicationId}), " +
                      $"portrait, linear, IL2CPP/ARM64, AAB, minSdk 24.");
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
