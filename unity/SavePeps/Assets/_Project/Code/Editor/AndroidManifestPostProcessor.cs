using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Injects the permissions Unity will not add for us.
    ///
    /// VIBRATE is the only one we need, and Unity only adds it automatically
    /// for <c>Handheld.Vibrate</c> — which we deliberately do not use, because
    /// it is a single blunt buzz with no intensity control. Editing the
    /// generated manifest keeps the permission list explicit and reviewable
    /// instead of hidden behind an API call we do not want.
    /// </summary>
    public sealed class AndroidManifestPostProcessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 1;

        private const string AndroidNs = "http://schemas.android.com/apk/res/android";

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            if (!File.Exists(manifestPath))
            {
                Debug.LogWarning($"[SavePeps] No manifest to patch at {manifestPath}.");
                return;
            }

            var doc = new XmlDocument();
            doc.Load(manifestPath);

            var manifest = doc.DocumentElement;
            if (manifest == null) return;

            foreach (var permission in new[] { "android.permission.VIBRATE" })
            {
                if (HasPermission(manifest, permission)) continue;

                var node = doc.CreateElement("uses-permission");
                node.SetAttribute("name", AndroidNs, permission);
                manifest.AppendChild(node);
                Debug.Log($"[SavePeps] Added {permission} to the Android manifest.");
            }

            doc.Save(manifestPath);
        }

        private static bool HasPermission(XmlNode manifest, string permission)
        {
            foreach (XmlNode child in manifest.ChildNodes)
            {
                if (child.Name != "uses-permission") continue;
                var name = child.Attributes?["android:name"]?.Value
                           ?? child.Attributes?.GetNamedItem("name", AndroidNs)?.Value;
                if (name == permission) return true;
            }

            return false;
        }
    }
}
