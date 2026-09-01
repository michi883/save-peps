using NUnit.Framework;
using SavePeps.Progression;
using UnityEngine;

namespace SavePeps.Tests
{
    /// <summary>
    /// The paywall boundary, exhaustively. The FreeRoundCount boundary and
    /// every entitlement state are cheap to assert here and genuinely awkward
    /// to reach on a device, which is the whole argument for
    /// <see cref="Access"/> being a pure function.
    /// </summary>
    public sealed class AccessTests
    {
        private Catalog _catalog;

        [SetUp]
        public void SetUp()
        {
            // Twelve rounds, ten of them free — the shipping catalogue.
            _catalog = ScriptableObject.CreateInstance<Catalog>();
            _catalog.FreeRoundCount = 10;
            _catalog.Rounds = new RoundDefinition[12];
            for (var i = 0; i < _catalog.Rounds.Length; i++)
            {
                var round = ScriptableObject.CreateInstance<RoundDefinition>();
                round.Number = i + 1;
                _catalog.Rounds[i] = round;
            }
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var round in _catalog.Rounds) Object.DestroyImmediate(round);
            Object.DestroyImmediate(_catalog);
        }

        [Test]
        public void FreeRoundsPlayWithoutTheFullGameUnlock()
        {
            for (var round = 1; round <= 10; round++)
            {
                Assert.IsTrue(
                    Access.CanPlay(_catalog, round, highestUnlocked: 12, hasFullGame: false),
                    $"Round {round} is inside the free block and must be playable.");
            }
        }

        [Test]
        public void TheFirstPaidRoundIsBlockedWithoutTheFullGameUnlock()
        {
            Assert.IsFalse(Access.CanPlay(_catalog, 11, highestUnlocked: 12, hasFullGame: false));
        }

        [Test]
        public void PaidRoundsPlayWithTheFullGameUnlock()
        {
            Assert.IsTrue(Access.CanPlay(_catalog, 11, highestUnlocked: 12, hasFullGame: true));
            Assert.IsTrue(Access.CanPlay(_catalog, 12, highestUnlocked: 12, hasFullGame: true));
        }

        [Test]
        public void FullGameOwnerCanChooseAnyExistingRoundImmediately()
        {
            Assert.IsTrue(Access.CanPlay(_catalog, 5, highestUnlocked: 1, hasFullGame: true));
            Assert.IsTrue(Access.CanPlay(_catalog, 12, highestUnlocked: 1, hasFullGame: true));
        }

        [Test]
        public void RoundsBeyondTheCatalogAreNeverPlayable()
        {
            Assert.IsFalse(Access.CanPlay(_catalog, 13, highestUnlocked: 99, hasFullGame: true));
            Assert.IsFalse(Access.CanPlay(_catalog, 0, highestUnlocked: 99, hasFullGame: true));
        }

        [Test]
        public void LosingTheEntitlementRelocksPaidRounds()
        {
            Assert.IsTrue(Access.CanPlay(_catalog, 11, 12, hasFullGame: true));
            Assert.IsFalse(Access.CanPlay(_catalog, 11, 12, hasFullGame: false),
                "A missing entitlement must relock paid rounds and keep the free ones.");
            Assert.IsTrue(Access.CanPlay(_catalog, 10, 12, hasFullGame: false));
        }

        [Test]
        public void MovingTheGateMovesTheBoundary()
        {
            // D3: FreeRoundCount is the release-week lever if content slips.
            _catalog.FreeRoundCount = 8;

            Assert.IsTrue(Access.CanPlay(_catalog, 8, 12, hasFullGame: false));
            Assert.IsFalse(Access.CanPlay(_catalog, 9, 12, hasFullGame: false));
        }

        [Test]
        public void PaywallShowsOnlyWhenTheFullGameEntitlementIsMissing()
        {
            // Reached, paid, not owned: this is the sales moment.
            Assert.IsTrue(Access.IsPaywalled(_catalog, 11, highestUnlocked: 11, hasFullGame: false));

            // Full-game ownership bypasses sequential progression, so a
            // premium picker tile is genuinely a purchase opportunity
            // even when the free path has not reached it yet.
            Assert.IsTrue(Access.IsPaywalled(_catalog, 11, highestUnlocked: 5, hasFullGame: false));

            // Free round, and past the end of the catalogue: never a paywall.
            Assert.IsFalse(Access.IsPaywalled(_catalog, 4, highestUnlocked: 11, hasFullGame: false));
            Assert.IsFalse(Access.IsPaywalled(_catalog, 13, highestUnlocked: 99, hasFullGame: false));
        }

        [Test]
        public void AccessStateDistinguishesProgressFromFullGameLocks()
        {
            Assert.AreEqual(RoundAccess.Playable, Access.State(_catalog, 2, 3, hasFullGame: false));
            Assert.AreEqual(RoundAccess.ProgressLocked, Access.State(_catalog, 4, 3, hasFullGame: false));
            Assert.AreEqual(RoundAccess.FullGameLocked, Access.State(_catalog, 11, 3, hasFullGame: false));
            Assert.AreEqual(RoundAccess.Playable, Access.State(_catalog, 11, 1, hasFullGame: true));
            Assert.AreEqual(RoundAccess.Missing, Access.State(_catalog, 13, 99, hasFullGame: true));
        }

        /// <summary>
        /// The out-of-content card offers "Play again", which routes to round
        /// 1. If round 1 could ever be unplayable that button would dead-end
        /// exactly where the player already had nowhere to go.
        /// </summary>
        [Test]
        public void RoundOneIsAlwaysPlayable()
        {
            // A fresh save, a finished save, full-game owner or not.
            foreach (var unlocked in new[] { 1, 5, 12 })
            foreach (var hasFullGame in new[] { true, false })
            {
                Assert.IsTrue(Access.CanPlay(_catalog, 1, unlocked, hasFullGame),
                    $"Round 1 must stay playable (unlocked {unlocked}, full game {hasFullGame}).");
            }

            // Even if the gate were moved to its most aggressive setting.
            _catalog.FreeRoundCount = 1;
            Assert.IsTrue(Access.CanPlay(_catalog, 1, 12, hasFullGame: false));
        }

        [Test]
        public void ANullCatalogDeniesRatherThanThrows()
        {
            Assert.IsFalse(Access.CanPlay(null, 1, 1, hasFullGame: true));
            Assert.IsFalse(Access.IsPaywalled(null, 1, 1, hasFullGame: false));
        }
    }
}
