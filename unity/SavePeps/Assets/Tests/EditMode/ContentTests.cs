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

        [TestCase("This quip breaks onto\ntwo lines.", "one line")]
        [TestCase("12345678901234567890123456789", "Pixel 4 one-line limit")]
        public void WrongAnswerQuipsStayGlanceable(string quip, string expectedError)
        {
            var rescue = ScriptableObject.CreateInstance<RescueDefinition>();
            try
            {
                rescue.Id = "quip-test";
                rescue.Objects = new[]
                {
                    new RescueObject { Id = "right" },
                    new RescueObject { Id = "wrong", Quip = quip },
                    new RescueObject { Id = "other-wrong", Quip = "Nope." },
                };
                rescue.CorrectIndex = 0;

                var report = ContentValidator.Validate(rescue);

                Assert.IsTrue(report.Errors.Exists(e => e.Contains(expectedError)),
                    $"Expected a quip readability error containing '{expectedError}'. Got:\n{report}");
            }
            finally
            {
                Object.DestroyImmediate(rescue);
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
        public void ARoundThatRenamesTheSameReasoningIsRejected()
        {
            var catalog = ScriptableObject.CreateInstance<Catalog>();
            var round = ScriptableObject.CreateInstance<RoundDefinition>();
            var rescues = new RescueDefinition[RoundDefinition.RescuesPerRound];

            try
            {
                for (var i = 0; i < rescues.Length; i++)
                {
                    var rescue = ScriptableObject.CreateInstance<RescueDefinition>();
                    rescue.Id = $"same{i}";
                    rescue.Verb = $"differentVerb{i}";
                    rescue.Goal = $"Solve idea {i}.";
                    rescue.Reasoning = ReasoningKind.Crossing;
                    rescue.Objects = new[]
                    {
                        new RescueObject { Id = $"right{i}", AnchorId = $"Slot_{i + 1}", Duration = 2.5f },
                        new RescueObject { Id = $"wrongA{i}", Duration = 2.5f, Quip = "No." },
                        new RescueObject { Id = $"wrongB{i}", Duration = 2.5f, Quip = "Also no." },
                    };
                    rescue.CorrectIndex = 0;
                    rescues[i] = rescue;
                }

                round.Number = 1;
                round.Rescues = rescues;
                catalog.Rounds = new[] { round };

                var report = ContentValidator.Validate(catalog);

                Assert.IsTrue(
                    report.Errors.Exists(e => e.Contains("different verbs cannot disguise the same puzzle")),
                    "Unique verbs must not let a structurally repeated round pass. Got:\n" + report);
            }
            finally
            {
                foreach (var rescue in rescues) if (rescue != null) Object.DestroyImmediate(rescue);
                Object.DestroyImmediate(round);
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void TheFreeBoundaryStaysAtRoundTenWhileContentIsAuthored()
        {
            var catalog = LoadCatalog();
            Assert.AreEqual(Catalog.DefaultFreeRoundCount, catalog.FreeRoundCount,
                "The current catalogue is intentionally smaller than the planned free block; " +
                "future rounds 4–10 must not silently become premium as they are added.");
        }
    }
}
