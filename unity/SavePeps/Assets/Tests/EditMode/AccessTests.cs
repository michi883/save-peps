using NUnit.Framework;
using SavePeps.Progression;
using UnityEngine;

namespace SavePeps.Tests
{
    /// <summary>
    /// The paywall boundary, exhaustively. PLAN §15 asks for the
    /// FreeRoundCount boundary and every entitlement state; these are cheap to
    /// assert here and genuinely awkward to reach on a device, which is the
    /// whole argument for <see cref="Access"/> being a pure function.
    /// </summary>
    public sealed class AccessTests
    {
        private Catalog _catalog;

        [SetUp]
        public void SetUp()
        {
            // Twelve rounds, ten of them free — the shipping shape from D2/D3.
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
        public void FreeRoundsPlayWithoutASubscription()
        {
            for (var round = 1; round <= 10; round++)
            {
                Assert.IsTrue(
                    Access.CanPlay(_catalog, round, highestUnlocked: 12, subscribed: false),
                    $"Round {round} is inside the free block and must be playable.");
            }
        }

        [Test]
        public void TheFirstPaidRoundIsBlockedWithoutASubscription()
        {
            Assert.IsFalse(Access.CanPlay(_catalog, 11, highestUnlocked: 12, subscribed: false));
        }

        [Test]
        public void PaidRoundsPlayWithASubscription()
        {
            Assert.IsTrue(Access.CanPlay(_catalog, 11, highestUnlocked: 12, subscribed: true));
            Assert.IsTrue(Access.CanPlay(_catalog, 12, highestUnlocked: 12, subscribed: true));
        }

        [Test]
        public void SubscriberCanChooseAnyExistingRoundImmediately()
        {
            Assert.IsTrue(Access.CanPlay(_catalog, 5, highestUnlocked: 1, subscribed: true));
            Assert.IsTrue(Access.CanPlay(_catalog, 12, highestUnlocked: 1, subscribed: true));
        }

        [Test]
        public void RoundsBeyondTheCatalogAreNeverPlayable()
        {
            Assert.IsFalse(Access.CanPlay(_catalog, 13, highestUnlocked: 99, subscribed: true));
            Assert.IsFalse(Access.CanPlay(_catalog, 0, highestUnlocked: 99, subscribed: true));
        }

        [Test]
        public void ALapsedSubscriptionRelocksPaidRounds()
        {
            Assert.IsTrue(Access.CanPlay(_catalog, 11, 12, subscribed: true));
            Assert.IsFalse(Access.CanPlay(_catalog, 11, 12, subscribed: false),
                "A lapsed subscriber must lose the paid rounds and keep the free ones.");
            Assert.IsTrue(Access.CanPlay(_catalog, 10, 12, subscribed: false));
        }

        [Test]
        public void MovingTheGateMovesTheBoundary()
        {
            // D3: FreeRoundCount is the release-week lever if content slips.
            _catalog.FreeRoundCount = 8;

            Assert.IsTrue(Access.CanPlay(_catalog, 8, 12, subscribed: false));
            Assert.IsFalse(Access.CanPlay(_catalog, 9, 12, subscribed: false));
        }

        [Test]
        public void PaywallShowsOnlyWhenTheSubscriptionIsWhatIsMissing()
        {
            // Reached, paid, not subscribed: this is the sales moment.
            Assert.IsTrue(Access.IsPaywalled(_catalog, 11, highestUnlocked: 11, subscribed: false));

            // Subscription bypasses sequential progression, so a premium
            // round in the picker is genuinely a subscription opportunity
            // even when the free path has not reached it yet.
            Assert.IsTrue(Access.IsPaywalled(_catalog, 11, highestUnlocked: 5, subscribed: false));

            // Free round, and past the end of the catalogue: never a paywall.
            Assert.IsFalse(Access.IsPaywalled(_catalog, 4, highestUnlocked: 11, subscribed: false));
            Assert.IsFalse(Access.IsPaywalled(_catalog, 13, highestUnlocked: 99, subscribed: false));
        }

        [Test]
        public void AccessStateDistinguishesProgressFromSubscriptionLocks()
        {
            Assert.AreEqual(RoundAccess.Playable, Access.State(_catalog, 2, 3, subscribed: false));
            Assert.AreEqual(RoundAccess.ProgressLocked, Access.State(_catalog, 4, 3, subscribed: false));
            Assert.AreEqual(RoundAccess.SubscriptionLocked, Access.State(_catalog, 11, 3, subscribed: false));
            Assert.AreEqual(RoundAccess.Playable, Access.State(_catalog, 11, 1, subscribed: true));
            Assert.AreEqual(RoundAccess.Missing, Access.State(_catalog, 13, 99, subscribed: true));
        }

        /// <summary>
        /// The out-of-content card offers "Play again", which routes to round
        /// 1. If round 1 could ever be unplayable that button would dead-end
        /// exactly where the player already had nowhere to go.
        /// </summary>
        [Test]
        public void RoundOneIsAlwaysPlayable()
        {
            // A fresh save, a finished save, subscribed or not.
            foreach (var unlocked in new[] { 1, 5, 12 })
            foreach (var subscribed in new[] { true, false })
            {
                Assert.IsTrue(Access.CanPlay(_catalog, 1, unlocked, subscribed),
                    $"Round 1 must stay playable (unlocked {unlocked}, subscribed {subscribed}).");
            }

            // Even if the gate were moved to its most aggressive setting.
            _catalog.FreeRoundCount = 1;
            Assert.IsTrue(Access.CanPlay(_catalog, 1, 12, subscribed: false));
        }

        [Test]
        public void ANullCatalogDeniesRatherThanThrows()
        {
            Assert.IsFalse(Access.CanPlay(null, 1, 1, subscribed: true));
            Assert.IsFalse(Access.IsPaywalled(null, 1, 1, subscribed: false));
        }
    }
}
