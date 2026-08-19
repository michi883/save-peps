using SavePeps.Progression;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Small conveniences for working on progression.
    ///
    /// Reaching "a fresh install" and "already unlocked round N" by playing is
    /// exactly the loop the round work needs over and over, and doing it by
    /// hand is slow enough that it gets skipped — which is how a first-run bug
    /// survives to the store.
    /// </summary>
    public static class DevMenu
    {
        [MenuItem("Tools/Save Peps/Save/Delete Save (fresh install)")]
        private static void DeleteSave()
        {
            SaveStore.Delete();
            Debug.Log("[SavePeps] Save deleted. The next run starts at round 1.");
        }

        [MenuItem("Tools/Save Peps/Save/Unlock All Rounds")]
        private static void UnlockAll()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(RescuePlayback.CatalogPath);
            if (catalog == null)
            {
                Debug.LogError("[SavePeps] No catalogue to unlock against.");
                return;
            }

            var save = SaveStore.Load();
            save.UnlockThrough(catalog.RoundCount);
            SaveStore.Save(save);
            Debug.Log($"[SavePeps] Unlocked through round {catalog.RoundCount}. " +
                      "Paid rounds still need the entitlement.");
        }

        [MenuItem("Tools/Save Peps/Save/Reveal Save File")]
        private static void Reveal()
        {
            var path = System.IO.Path.Combine(Application.persistentDataPath, "save.json");
            if (System.IO.File.Exists(path)) EditorUtility.RevealInFinder(path);
            else Debug.Log($"[SavePeps] No save file yet at {path}.");
        }
    }
}
