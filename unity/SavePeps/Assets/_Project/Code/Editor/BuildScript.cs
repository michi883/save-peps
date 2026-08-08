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
            var report = Run(appBundle: true);
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
            var report = Run(appBundle: false);
            if (report.summary.result != BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Tools/Save Peps/Build Android AAB")]
        public static void BuildAndroidMenu() => Run(appBundle: true);

        [MenuItem("Tools/Save Peps/Build Android APK (device)")]
        public static void BuildAndroidApkMenu() => Run(appBundle: false);

        private static BuildReport Run(bool appBundle)
        {
            ProjectBootstrap.Apply();
            ApplySigningFromEnvironment();

            EditorUserBuildSettings.buildAppBundle = appBundle;

            Directory.CreateDirectory(OutputDir);
            var name = $"SavePeps-{DateTime.Now:yyyyMMdd-HHmm}.{(appBundle ? "aab" : "apk")}";
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
                options = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                var mb = summary.totalSize / (1024f * 1024f);
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
