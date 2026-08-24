using System.Collections.Generic;
using NUnit.Framework;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEngine;

namespace SavePeps.Tests
{
    public sealed class RoundSelectorTests
    {
        private Catalog _catalog;
        private readonly List<Object> _owned = new();

        [SetUp]
        public void SetUp()
        {
            _catalog = Own(ScriptableObject.CreateInstance<Catalog>());
            _catalog.FreeRoundCount = 3;
            _catalog.Rounds = new RoundDefinition[5];
            for (var number = 1; number <= _catalog.RoundCount; number++)
            {
                var round = Own(ScriptableObject.CreateInstance<RoundDefinition>());
                round.Number = number;
                round.Rescues = new RescueDefinition[RoundDefinition.RescuesPerRound];
                for (var rescueIndex = 0; rescueIndex < round.Rescues.Length; rescueIndex++)
                {
                    var rescue = Own(ScriptableObject.CreateInstance<RescueDefinition>());
                    rescue.Id = $"r{number}_{rescueIndex}";
                    round.Rescues[rescueIndex] = rescue;
                }
                _catalog.Rounds[number - 1] = round;
            }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var owned in _owned) Object.DestroyImmediate(owned);
            _owned.Clear();
        }

        [Test]
        public void FreeSelectionNeverEscapesUnlockedFreeRounds()
        {
            var save = SaveData.Fresh();
            save.UnlockThrough(2);

            for (var i = 0; i <= 100; i++)
            {
                var chosen = RoundSelector.Choose(_catalog, save, hasFullGame: false, i / 100f);
                Assert.That(chosen, Is.InRange(1, 2));
            }
        }

        [Test]
        public void ImmediateRepeatIsAvoidedWhenAnotherRoundExists()
        {
            var save = SaveData.Fresh();
            save.UnlockThrough(2);
            save.LastPlayedRound = 1;

            for (var i = 0; i <= 10; i++)
            {
                Assert.AreEqual(2, RoundSelector.Choose(_catalog, save, false, i / 10f));
            }
        }

        [Test]
        public void OnlyAvailableRoundMayRepeat()
        {
            var save = SaveData.Fresh();
            save.LastPlayedRound = 1;
            Assert.AreEqual(1, RoundSelector.Choose(_catalog, save, false, 0.5f));
        }

        [Test]
        public void FullGameSelectionIncludesUnreachedAndPremiumRounds()
        {
            var save = SaveData.Fresh();
            var available = RoundSelector.Available(_catalog, save, hasFullGame: true);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, available);
        }

        [Test]
        public void NewestUnplayedRoundHasMoreSelectionWeightThanAPerfectRound()
        {
            var save = SaveData.Fresh();
            save.UnlockThrough(3);
            foreach (var rescue in _catalog.Round(1).Rescues)
            {
                save.RecordSolved(rescue.Id, firstTap: true);
            }

            var selections = new Dictionary<int, int>();
            for (var i = 0; i < 900; i++)
            {
                var chosen = RoundSelector.Choose(_catalog, save, false, (i + 0.5f) / 900f);
                selections[chosen] = selections.GetValueOrDefault(chosen) + 1;
            }

            Assert.Greater(selections[3], selections[1]);
            Assert.IsTrue(RoundProgress.Read(_catalog.Round(1), save).IsPerfect);
            Assert.IsTrue(RoundProgress.Read(_catalog.Round(3), save).IsUnplayed);
        }

        private T Own<T>(T value) where T : Object
        {
            _owned.Add(value);
            return value;
        }
    }
}
