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
        public void ProgressStillGatesASubscriber()
        {
            // Paying does not skip the game. Round 5 is free, but unreached.
            Assert.IsFalse(Access.CanPlay(_catalog, 5, highestUnlocked: 3, subscribed: true));
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

            // Not reached yet: locked, but showing a paywall here would be a bug.
            Assert.IsFalse(Access.IsPaywalled(_catalog, 11, highestUnlocked: 5, subscribed: false));

            // Free round, and past the end of the catalogue: never a paywall.
            Assert.IsFalse(Access.IsPaywalled(_catalog, 4, highestUnlocked: 11, subscribed: false));
            Assert.IsFalse(Access.IsPaywalled(_catalog, 13, highestUnlocked: 99, subscribed: false));
        }

        [Test]
        public void ANullCatalogDeniesRatherThanThrows()
        {
            Assert.IsFalse(Access.CanPlay(null, 1, 1, subscribed: true));
            Assert.IsFalse(Access.IsPaywalled(null, 1, 1, subscribed: false));
        }
    }
}
