using NUnit.Framework;
using SavePeps.Progression;
using UnityEditor;

namespace SavePeps.Tests
{
    /// <summary>
    /// Tester profiles are destructive on purpose, so their exact shape must
    /// be more trustworthy than a hand-authored save file.
    /// </summary>
    public sealed class TesterProfileTests
    {
        private const string CatalogPath = "Assets/_Project/Content/Catalog.asset";
        private Catalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _catalog = AssetDatabase.LoadAssetAtPath<Catalog>(CatalogPath);
            Assert.IsNotNull(_catalog, "The authored catalogue is required for Tester Mode presets.");
        }

        [Test]
        public void FreshMatchesAFirstLaunch()
        {
            var save = TesterProfiles.Create(_catalog, TesterProfilePreset.Fresh);

            Assert.AreEqual(1, save.HighestUnlockedRound);
            Assert.AreEqual(0, save.LastPlayedRound);
            Assert.AreEqual(0, save.TotalRescuesSolved);
            Assert.IsFalse(save.SoundMuted);
            Assert.IsFalse(save.HapticsOff);
            Assert.Greater(save.FirstRunUtc, 0);

            foreach (var round in _catalog.Rounds)
            foreach (var rescue in round.Rescues)
            {
                Assert.AreEqual(Mark.None, save.MarkFor(rescue.Id), rescue.Id);
            }
        }

        [Test]
        public void PartialIsARealisticInterruptedSecondRound()
        {
            var save = TesterProfiles.Create(_catalog, TesterProfilePreset.Partial);

            Assert.AreEqual(2, save.HighestUnlockedRound);
            Assert.AreEqual(2, save.LastPlayedRound);
            Assert.AreEqual(5, save.TotalRescuesSolved);
            Assert.IsTrue(RoundProgress.Read(_catalog.Round(1), save).IsComplete);
            Assert.AreEqual(2, RoundProgress.Read(_catalog.Round(2), save).Solved);
            Assert.IsFalse(RoundProgress.Read(_catalog.Round(2), save).IsComplete);
            Assert.IsTrue(RoundProgress.Read(_catalog.Round(3), save).IsUnplayed);
        }

        [Test]
        public void AllCompletedUsesChecksWithoutClaimingPerfection()
        {
            var save = TesterProfiles.Create(_catalog, TesterProfilePreset.AllCompleted);

            Assert.AreEqual(_catalog.RoundCount, save.HighestUnlockedRound);
            Assert.AreEqual(36, save.TotalRescuesSolved);
            foreach (var round in _catalog.Rounds)
            {
                var progress = RoundProgress.Read(round, save);
                Assert.IsTrue(progress.IsComplete, $"Round {round.Number} should be complete.");
                Assert.IsFalse(progress.IsPerfect, $"Round {round.Number} should use checks, not stars.");
            }
        }

        [Test]
        public void AllPerfectUsesStarsEverywhere()
        {
            var save = TesterProfiles.Create(_catalog, TesterProfilePreset.AllPerfect);

            Assert.AreEqual(36, save.TotalRescuesSolved);
            foreach (var round in _catalog.Rounds)
            {
                Assert.IsTrue(RoundProgress.Read(round, save).IsPerfect,
                    $"Round {round.Number} should be perfect.");
            }
        }

        [Test]
        public void UnlockAllChangesOnlyTheProgressionCeiling()
        {
            var save = SaveData.Fresh();
            var rescue = _catalog.Round(1).RescueAt(0);
            save.RecordSolved(rescue.Id, firstTap: false);
            save.LastPlayedRound = 1;
            save.SoundMuted = true;
            var firstRun = save.FirstRunUtc;

            TesterProfiles.UnlockAll(_catalog, save);

            Assert.AreEqual(_catalog.RoundCount, save.HighestUnlockedRound);
            Assert.AreEqual(1, save.TotalRescuesSolved);
            Assert.AreEqual(Mark.Check, save.MarkFor(rescue.Id));
            Assert.AreEqual(1, save.LastPlayedRound);
            Assert.IsTrue(save.SoundMuted);
            Assert.AreEqual(firstRun, save.FirstRunUtc);
            Assert.AreEqual(RoundAccess.FullGameLocked,
                Access.State(_catalog, 11, save.HighestUnlockedRound, hasFullGame: false),
                "Unlocking progression must not manufacture entitlement.");
        }
    }
}
