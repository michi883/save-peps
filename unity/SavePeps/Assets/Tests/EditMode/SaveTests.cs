using System.IO;
using NUnit.Framework;
using SavePeps.Progression;
using UnityEngine;

namespace SavePeps.Tests
{
    /// <summary>
    /// The save file is the one piece of player-visible state that survives
    /// the process, so every way it can arrive malformed is a way to lose
    /// somebody's progress or crash them on launch. Cover round-trip, corrupt
    /// input, missing fields, and schema migration here.
    /// </summary>
    public sealed class SaveTests
    {
        [TearDown]
        public void Cleanup() => SaveStore.Delete();

        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        [Test]
        public void RoundTrips()
        {
            var data = SaveData.Fresh();
            data.UnlockThrough(4);
            data.RecordSolved("r01", firstTap: true);
            data.RecordSolved("r02", firstTap: false);
            data.SoundMuted = true;
            data.LastPlayedRound = 3;

            Assert.IsTrue(SaveStore.Save(data));

            var loaded = SaveStore.Load();
            Assert.AreEqual(4, loaded.HighestUnlockedRound);
            Assert.AreEqual(Mark.Star, loaded.MarkFor("r01"));
            Assert.AreEqual(Mark.Check, loaded.MarkFor("r02"));
            Assert.AreEqual(Mark.None, loaded.MarkFor("r03"));
            Assert.AreEqual(2, loaded.TotalRescuesSolved);
            Assert.IsTrue(loaded.SoundMuted);
            Assert.AreEqual(3, loaded.LastPlayedRound);
        }

        [Test]
        public void MissingFileGivesFreshSave()
        {
            SaveStore.Delete();
            var loaded = SaveStore.Load();

            Assert.AreEqual(1, loaded.HighestUnlockedRound);
            Assert.AreEqual(0, loaded.TotalRescuesSolved);
        }

        [Test]
        public void CorruptFileGivesFreshSaveRatherThanThrowing()
        {
            File.WriteAllText(SavePath, "{ this is not json at all ");

            var loaded = SaveStore.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(1, loaded.HighestUnlockedRound);
        }

        [Test]
        public void EmptyFileGivesFreshSave()
        {
            File.WriteAllText(SavePath, string.Empty);
            Assert.AreEqual(1, SaveStore.Load().HighestUnlockedRound);
        }

        [Test]
        public void MissingFieldsAreDefaulted()
        {
            // A save written by an older build: valid JSON, almost nothing in it.
            File.WriteAllText(SavePath, "{\"SchemaVersion\":1,\"HighestUnlockedRound\":3}");

            var loaded = SaveStore.Load();

            Assert.AreEqual(3, loaded.HighestUnlockedRound);
            Assert.AreEqual(0, loaded.TotalRescuesSolved);
            Assert.IsFalse(loaded.SoundMuted);
            Assert.AreEqual(0, loaded.LastPlayedRound,
                "Older saves have no last-played field and must remain compatible.");
            Assert.AreEqual(Mark.None, loaded.MarkFor("r01"));
        }

        [Test]
        public void NewerSchemaIsKeptRatherThanWiped()
        {
            // Rolling a build back must not cost a full-game owner their progress.
            File.WriteAllText(SavePath, "{\"SchemaVersion\":99,\"HighestUnlockedRound\":7}");

            var loaded = SaveStore.Load();

            Assert.AreEqual(7, loaded.HighestUnlockedRound);
        }

        [Test]
        public void NonsenseValuesAreClamped()
        {
            File.WriteAllText(SavePath,
                "{\"SchemaVersion\":1,\"HighestUnlockedRound\":-5,\"LastPlayedRound\":-4," +
                "\"TotalRescuesSolved\":-9}");

            var loaded = SaveStore.Load();

            Assert.AreEqual(1, loaded.HighestUnlockedRound);
            Assert.AreEqual(0, loaded.TotalRescuesSolved);
            Assert.AreEqual(0, loaded.LastPlayedRound);
        }

        [Test]
        public void StarIsNotDowngradedByReplaying()
        {
            var data = SaveData.Fresh();
            data.RecordSolved("r01", firstTap: true);
            data.RecordSolved("r01", firstTap: false);

            Assert.AreEqual(Mark.Star, data.MarkFor("r01"));
            Assert.AreEqual(1, data.TotalRescuesSolved, "A replay must not count as a second solve.");
        }

        [Test]
        public void CheckIsUpgradedToStar()
        {
            var data = SaveData.Fresh();
            data.RecordSolved("r01", firstTap: false);
            data.RecordSolved("r01", firstTap: true);

            Assert.AreEqual(Mark.Star, data.MarkFor("r01"));
            Assert.AreEqual(1, data.TotalRescuesSolved);
        }

        [Test]
        public void UnlockNeverGoesBackwards()
        {
            var data = SaveData.Fresh();
            data.UnlockThrough(5);
            data.UnlockThrough(2);

            Assert.AreEqual(5, data.HighestUnlockedRound);
        }
    }
}
