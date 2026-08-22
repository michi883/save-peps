using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Android builds, driven from the command line so CI and a laptop produce
    /// the same artifact:
    ///
    ///   Unity -batchmode -quit -projectPath . \
    ///         -executeMethod SavePeps.EditorTools.BuildScript.BuildAndroid
    ///
    /// Signing comes from the environment, never from the repo:
    ///   SAVEPEPS_KEYSTORE, SAVEPEPS_KEYSTORE_PASS,
    ///   SAVEPEPS_KEYALIAS, SAVEPEPS_KEYALIAS_PASS
    /// With none set, the build is debug-signed — fine for a toolchain smoke
    /// test, rejected by Play.
    /// </summary>
    public static class BuildScript
    {
        private const string OutputDir = "Build/Android";

        public static void BuildAndroid()
        {
            var report = Run(appBundle: true, development: false);
            // Batchmode swallows a non-zero exit unless we ask for one.
            if (report.summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// An APK for side-loading onto a test device. Play takes the bundle;
        /// `adb install` cannot, so on-device iteration needs this path.
        /// </summary>
        public static void BuildAndroidApk()
        {
            var report = Run(appBundle: false, development: false);
            if (report.summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Side-loadable APK with Unity's Development Build flag. This is the
        /// only Android artifact in which the runtime Tester Mode entry point
        /// is visible.
        /// </summary>
        public static void BuildAndroidDevelopmentApk()
        {
            var report = Run(appBundle: false, development: true);
            if (report.summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/Save Peps/Build Android AAB")]
        public static void BuildAndroidMenu() => Run(appBundle: true, development: false);

        [MenuItem("Tools/Save Peps/Build Android APK (device)")]
        public static void BuildAndroidApkMenu() => Run(appBundle: false, development: false);

        [MenuItem("Tools/Save Peps/Build Android Development APK (Tester Mode)")]
        public static void BuildAndroidDevelopmentApkMenu() => Run(appBundle: false, development: true);

        private static BuildReport Run(bool appBundle, bool development)
        {
            ProjectBootstrap.Apply();
            ApplySigningFromEnvironment();

            EditorUserBuildSettings.buildAppBundle = appBundle;

            Directory.CreateDirectory(OutputDir);
            var flavour = development ? "-dev" : string.Empty;
            var name = $"SavePeps{flavour}-{DateTime.Now:yyyyMMdd-HHmm}.{(appBundle ? "aab" : "apk")}";
            var path = Path.Combine(OutputDir, name);

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                // An empty scene list still builds, but silently produces an
                // app that opens to nothing — worth saying out loud.
                Debug.LogWarning("[SavePeps] No scenes are enabled in Build Settings.");
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = path,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = development ? BuildOptions.Development : BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                // The artifact on disk, not summary.totalSize — that counts
                // built content before packaging and reads several times
                // high for an Android build. docs/release.md sends a human to
                // compare this number against Play's size threshold, so it
                // has to be the number Play will actually see.
                var bytes = File.Exists(path) ? (ulong)new FileInfo(path).Length : summary.totalSize;
                var mb = bytes / (1024f * 1024f);
                Debug.Log($"[SavePeps] Build succeeded: {path} ({mb:F1} MB, {summary.totalTime:mm\\:ss}).");
            }
            else
            {
                Debug.LogError($"[SavePeps] Build {summary.result}: " +
                               $"{summary.totalErrors} error(s), {summary.totalWarnings} warning(s).");
            }

            return report;
        }

        /// <summary>
        /// Keystores and passwords come from the environment so nothing
        /// signing-related is ever committed. Absent credentials fall back to
        /// Unity's debug key.
        /// </summary>
        private static void ApplySigningFromEnvironment()
        {
            var keystore = Environment.GetEnvironmentVariable("SAVEPEPS_KEYSTORE");
            if (string.IsNullOrEmpty(keystore))
            {
                PlayerSettings.Android.useCustomKeystore = false;
                Debug.LogWarning("[SavePeps] No keystore configured — debug-signed build. " +
                                 "Google Play will reject this artifact.");
                return;
            }

            if (!File.Exists(keystore))
            {
                throw new BuildFailedException($"Keystore not found at '{keystore}'.");
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystore;
            PlayerSettings.Android.keystorePass = Environment.GetEnvironmentVariable("SAVEPEPS_KEYSTORE_PASS");
            PlayerSettings.Android.keyaliasName = Environment.GetEnvironmentVariable("SAVEPEPS_KEYALIAS");
            PlayerSettings.Android.keyaliasPass = Environment.GetEnvironmentVariable("SAVEPEPS_KEYALIAS_PASS");
            Debug.Log($"[SavePeps] Signing with keystore '{Path.GetFileName(keystore)}'.");
        }
    }
}
