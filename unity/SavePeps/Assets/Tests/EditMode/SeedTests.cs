using NUnit.Framework;
using SavePeps.EditorTools;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;

namespace SavePeps.Tests
{
    /// <summary>
    /// The seeder must not eat authored work.
    ///
    /// This is a regression guard for a specific hazard: round one was
    /// originally generated from code, and once rescues are tuned in the
    /// inspector, a generator that reasserts itself discards that tuning with
    /// no warning and no diff to notice. The catalogue case is sharper still —
    /// FreeRoundCount is the release-week lever, and silently resetting it
    /// ships the paywall in the wrong place.
    /// </summary>
    public sealed class SeedTests
    {
        private const string WakePath = ContentPaths.RescueDir + "/r02_wake.asset";

        [Test]
        public void SeedingKeepsInspectorEditsToARescue()
        {
            var rescue = AssetDatabase.LoadAssetAtPath<RescueDefinition>(WakePath);
            Assert.IsNotNull(rescue, $"No rescue at {WakePath}.");

            var originalGoal = rescue.Goal;
            var originalDuration = rescue.Objects[0].Duration;

            try
            {
                rescue.Goal = "Seed hazard sentinel.";
                rescue.Objects[0].Duration = 3.05f;
                EditorUtility.SetDirty(rescue);
                AssetDatabase.SaveAssets();

                ContentSeeder.Seed(overwrite: false);

                var after = AssetDatabase.LoadAssetAtPath<RescueDefinition>(WakePath);
                Assert.AreEqual("Seed hazard sentinel.", after.Goal,
                    "Seeding overwrote an authored goal.");
                Assert.AreEqual(3.05f, after.Objects[0].Duration, 0.0001f,
                    "Seeding overwrote authored step timing.");
            }
            finally
            {
                rescue.Goal = originalGoal;
                rescue.Objects[0].Duration = originalDuration;
                EditorUtility.SetDirty(rescue);
                AssetDatabase.SaveAssets();
            }
        }

        [Test]
        public void SeedingKeepsTheReleaseWeekPaywallLever()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(ContentPaths.CatalogPath);
            Assert.IsNotNull(catalog);

            var original = catalog.FreeRoundCount;

            try
            {
                // Standing in for "release week set this to 10".
                catalog.FreeRoundCount = 7;
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();

                ContentSeeder.Seed(overwrite: false);

                Assert.AreEqual(7, AssetDatabase.LoadAssetAtPath<Catalog>(ContentPaths.CatalogPath).FreeRoundCount,
                    "Seeding reset FreeRoundCount — the paywall would move without anyone noticing.");
            }
            finally
            {
                catalog.FreeRoundCount = original;
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
            }
        }

        [Test]
        public void SeedingAnAlreadySeededProjectWritesNothing()
        {
            var log = new ContentSeeder.SeedLog();
            ContentSeeder.Seed(overwrite: false, log);

            Assert.IsEmpty(log.Written,
                "Seeding is meant to be idempotent once the content exists; it wrote " +
                string.Join(", ", log.Written) + ".");
            Assert.IsNotEmpty(log.Kept, "Seeding should report what it left alone.");
        }
    }
}
