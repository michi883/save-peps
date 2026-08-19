using NUnit.Framework;
using SavePeps.EditorTools;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;

namespace SavePeps.Tests
{
    /// <summary>
    /// The gauntlet's schedule, asserted rather than watched.
    ///
    /// The run itself needs play mode and a human, but what it decides to play
    /// and in what order is ordinary logic — and getting that wrong (skipping
    /// an outcome, never reaching the answer) would quietly hollow out the
    /// review pass it exists to support.
    /// </summary>
    public sealed class GauntletTests
    {
        private static Catalog LoadCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(RescuePlayback.CatalogPath);
            Assert.IsNotNull(catalog, "No catalogue to schedule.");
            return catalog;
        }

        [Test]
        public void EveryOutcomeOfEveryRescueIsScheduledExactlyOnce()
        {
            var catalog = LoadCatalog();
            var order = RescuePlayback.GauntletOrder();

            var expected = 0;
            for (var number = 1; number <= catalog.RoundCount; number++)
            {
                foreach (var rescue in catalog.Round(number)?.Rescues ?? System.Array.Empty<RescueDefinition>())
                {
                    if (rescue != null) expected += rescue.Objects.Length;
                }
            }

            Assert.AreEqual(expected, order.Count,
                "The gauntlet must play every object of every rescue.");

            // No duplicates: one beat per (rescue, object).
            var seen = new System.Collections.Generic.HashSet<string>();
            foreach (var beat in order)
            {
                Assert.IsTrue(seen.Add($"{beat.Rescue.Id}/{beat.Outcome}"),
                    $"{beat.Rescue.Id} outcome {beat.Outcome} is scheduled twice.");
            }
        }

        [Test]
        public void EachRescueEndsOnItsCorrectOutcome()
        {
            var order = RescuePlayback.GauntletOrder();
            Assert.Greater(order.Count, 0);

            // Walk the run; the last beat of each rescue's block must be the
            // answer, so the reviewer always finishes on the reunion.
            for (var i = 0; i < order.Count; i++)
            {
                var isLastOfRescue = i == order.Count - 1 || order[i + 1].Rescue != order[i].Rescue;
                if (isLastOfRescue)
                {
                    Assert.IsTrue(order[i].IsCorrect,
                        $"{order[i].Rescue.Id} does not end on its correct outcome.");
                }
                else
                {
                    Assert.IsFalse(order[i].IsCorrect,
                        $"{order[i].Rescue.Id} plays its answer before its wrong outcomes.");
                }
            }
        }

        [Test]
        public void TheRunIsContiguousPerRescue()
        {
            // A rescue's outcomes must not be interleaved with another's —
            // staging a diorama is the expensive part and the review reads
            // better rescue by rescue.
            var order = RescuePlayback.GauntletOrder();
            var seen = new System.Collections.Generic.HashSet<RescueDefinition>();

            RescueDefinition current = null;
            foreach (var beat in order)
            {
                if (beat.Rescue == current) continue;
                Assert.IsTrue(seen.Add(beat.Rescue),
                    $"{beat.Rescue.Id} is returned to after moving on.");
                current = beat.Rescue;
            }
        }

        [Test]
        public void TheEstimateIsSaneAndNonZero()
        {
            var (beats, seconds) = RescuePlayback.EstimateGauntlet();

            Assert.AreEqual(RescuePlayback.GauntletOrder().Count, beats);
            Assert.Greater(seconds, 0d);
            // Every beat is a settle plus an outcome plus a hold: nothing can
            // be under a second, and a 3.6s cap plus holds puts it under ten.
            Assert.Greater(seconds / beats, 1d);
            Assert.Less(seconds / beats, 10d);
        }
    }
}
