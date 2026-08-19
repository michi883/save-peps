using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Validates a rescue as it is saved.
    ///
    /// A warning rather than a blocked save: authoring is iterative and a
    /// half-finished rescue is a normal thing to have on disk. The hard gate
    /// is the EditMode test over the whole catalogue, which is what stops a
    /// broken rescue reaching a build.
    /// </summary>
    public sealed class ContentSaveValidator : AssetModificationProcessor
    {
        private static string[] OnWillSaveAssets(string[] paths)
        {
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path) || !path.EndsWith(".asset", System.StringComparison.Ordinal)) continue;

                var rescue = AssetDatabase.LoadAssetAtPath<RescueDefinition>(path);
                if (rescue == null) continue;

                var report = ContentValidator.Validate(rescue);
                if (report.Errors.Count == 0) continue;

                Debug.LogWarning($"[SavePeps] '{rescue.Id}' saved with problems:\n{report}", rescue);
            }

            return paths;
        }
    }
}
