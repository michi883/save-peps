using NUnit.Framework;
using SavePeps.EditorTools;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.Tests
{
    /// <summary>
    /// Runs the content validator over the real authored catalogue.
    ///
    /// This is the test that has to stay green through the content sprint: it
    /// is the only thing standing between a typo'd step target and a rescue
    /// that quietly does nothing during a demo.
    /// </summary>
    public sealed class ContentTests
    {
        private const string CatalogPath = "Assets/_Project/Content/Catalog.asset";

        private static Catalog LoadCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(CatalogPath);
            Assert.IsNotNull(catalog, $"No catalogue at {CatalogPath}.");
            return catalog;
        }

        [Test]
        public void TheCatalogueIsValid()
        {
            var report = ContentValidator.Validate(LoadCatalog());
            Assert.IsTrue(report.Ok, "\n" + report);
        }

        [Test]
        public void EveryRoundIsFullyAuthored()
        {
            var catalog = LoadCatalog();
            Assert.Greater(catalog.RoundCount, 0, "The catalogue is empty.");

            for (var number = 1; number <= catalog.RoundCount; number++)
            {
                var round = catalog.Round(number);
                Assert.IsNotNull(round, $"Round {number} is missing.");
                Assert.AreEqual(number, round.Number, $"Round at position {number} is numbered {round.Number}.");

                for (var i = 0; i < RoundDefinition.RescuesPerRound; i++)
                {
                    var rescue = round.RescueAt(i);
                    Assert.IsNotNull(rescue, $"Round {number} has no rescue at slot {i + 1}.");
                    Assert.IsNotNull(rescue.Correct, $"{rescue.Id} has no correct object.");
                }
            }
        }

        /// <summary>
        /// Guards the rule that came out of the first device playthrough: the
        /// whole round was winnable by tapping one screen position three
        /// times, because every answer shared an anchor.
        /// </summary>
        [Test]
        public void ARoundWhoseAnswersNeverMoveIsRejected()
        {
            var catalog = ScriptableObject.CreateInstance<Catalog>();
            var round = ScriptableObject.CreateInstance<RoundDefinition>();
            var rescues = new RescueDefinition[RoundDefinition.RescuesPerRound];

            try
            {
                for (var i = 0; i < rescues.Length; i++)
                {
                    var rescue = ScriptableObject.CreateInstance<RescueDefinition>();
                    rescue.Id = $"t0{i}";
                    rescue.Verb = $"verb{i}";
                    rescue.Goal = "Bring them together.";
                    rescue.Objects = new[]
                    {
                        // The answer never leaves Slot_1.
                        new RescueObject { Id = $"right{i}", AnchorId = "Slot_1", Duration = 2.5f },
                        new RescueObject { Id = $"wrongA{i}", AnchorId = "Slot_2", Duration = 2.5f, Quip = "No." },
                        new RescueObject { Id = $"wrongB{i}", AnchorId = "Slot_3", Duration = 2.5f, Quip = "Also no." },
                    };
                    rescue.CorrectIndex = 0;
                    rescues[i] = rescue;
                }

                round.Number = 1;
                round.Rescues = rescues;
                catalog.Rounds = new[] { round };
                catalog.FreeRoundCount = 1;

                var report = ContentValidator.Validate(catalog);

                Assert.IsTrue(
                    report.Errors.Exists(e => e.Contains("every answer is at")),
                    "A round with a fixed answer position should be an error. Got:\n" + report);
            }
            finally
            {
                foreach (var rescue in rescues) if (rescue != null) Object.DestroyImmediate(rescue);
                Object.DestroyImmediate(round);
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void ThePaywallIsReachable()
        {
            // FreeRoundCount above the authored round count would make the
            // gate untestable — which is exactly how it ships broken.
            var catalog = LoadCatalog();
            Assert.LessOrEqual(catalog.FreeRoundCount, catalog.RoundCount,
                $"{catalog.FreeRoundCount} free rounds but only {catalog.RoundCount} authored: " +
                "nothing behind the paywall to reach.");
        }
    }
}
