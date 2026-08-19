using System.Collections;
using NUnit.Framework;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SavePeps.Tests
{
    /// <summary>
    /// Plays a whole round in the real Game scene.
    ///
    /// This is the P2 definition of done expressed as a test — three rescues
    /// start to finish, then the round-complete card. Everything else is
    /// tested as data or as a pure function; this is the only place the
    /// sequencing, the reset between rescues, and the save actually run
    /// together, which is where the interesting bugs live.
    /// </summary>
    public sealed class RoundLoopTests
    {
        private const string SceneName = "Game";

        private float _timeScale;

        [SetUp]
        public void SetUp()
        {
            // Start every run from a clean install.
            SaveStore.Delete();
            _timeScale = Time.timeScale;
            // Outcomes plus dwell run to about twenty seconds of real time.
            // The choreography is all delta-time based, so compressing it does
            // not change what is being asserted.
            Time.timeScale = 5f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = _timeScale;
            SaveStore.Delete();
        }

        [UnityTest]
        public IEnumerator ARoundOfThreeRescuesPlaysStartToFinish()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var router = Object.FindFirstObjectByType<TapRouter>();
            var card = Object.FindFirstObjectByType<RoundCompleteCard>();

            Assert.IsNotNull(flow, "The Game scene has no GameFlow.");
            Assert.IsNotNull(runner, "The Game scene has no RescueRunner.");
            Assert.IsNotNull(router, "The Game scene has no TapRouter.");
            Assert.IsNotNull(card, "The Game scene has no RoundCompleteCard.");

            Assert.AreEqual(1, flow.CurrentRound, "A fresh save should start on round 1.");

            var played = 0;
            for (var i = 0; i < RoundDefinition.RescuesPerRound; i++)
            {
                var rescue = runner.Current;
                Assert.IsNotNull(rescue, $"No rescue staged for slot {i + 1}.");
                Assert.IsNotNull(rescue.Correct, $"{rescue.Id} has no correct object.");

                var id = rescue.Id;
                var correctId = rescue.Correct.Id;

                yield return WaitUntil(() => router.InputEnabled, 10f);
                Assert.IsTrue(router.InputEnabled, $"{id} never accepted input.");

                router.SimulateTap(correctId);

                // The outcome has to finish playing before the solve lands.
                yield return WaitUntil(() => flow.Save.MarkFor(id) != Mark.None, 15f);
                played++;

                Assert.AreEqual(Mark.Star, flow.Save.MarkFor(id),
                    $"{id} was solved on the first tap and should have earned a star.");

                // Wait for the flow to stage the next rescue, or finish the round.
                yield return WaitUntil(() => runner.Current == null || runner.Current.Id != id, 15f);
            }

            Assert.AreEqual(RoundDefinition.RescuesPerRound, played);
            Assert.AreEqual(3, flow.Save.TotalRescuesSolved);
            Assert.AreEqual(2, flow.Save.HighestUnlockedRound,
                "Finishing round 1 should unlock round 2.");

            // The card is the visible end of the round.
            yield return WaitUntil(() => CardVisible(card), 8f);
            Assert.IsTrue(CardVisible(card), "The round-complete card never appeared.");
        }

        [UnityTest]
        public IEnumerator AWrongTapCostsTheStarButNotTheProgress()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var router = Object.FindFirstObjectByType<TapRouter>();

            var rescue = runner.Current;
            Assert.IsNotNull(rescue);
            var id = rescue.Id;

            // Tap a wrong object, then retry and solve it.
            var wrong = System.Array.Find(rescue.Objects, o => o != null && !rescue.IsCorrect(o));
            Assert.IsNotNull(wrong, "Every rescue must offer a wrong object.");

            var correctId = rescue.Correct.Id;

            router.SimulateTap(wrong.Id);
            yield return WaitRealSeconds(wrong.Duration / Time.timeScale + 0.5f);

            Assert.AreEqual(Mark.None, flow.Save.MarkFor(id), "A wrong tap must not mark the rescue solved.");
            Assert.IsFalse(router.InputEnabled,
                "Input must stay locked after a wrong outcome until the player asks to retry.");

            // Try Again. The scene resets and the same rescue is playable.
            runner.Retry();
            yield return WaitUntil(() => router.InputEnabled, 5f);
            Assert.IsTrue(router.InputEnabled, "Retry should hand input back.");

            router.SimulateTap(correctId);
            yield return WaitUntil(() => flow.Save.MarkFor(id) != Mark.None, 12f);

            Assert.AreEqual(Mark.Check, flow.Save.MarkFor(id),
                "Solved after a wrong tap should be a check, not a star.");
            Assert.AreEqual(1, flow.Save.TotalRescuesSolved);
        }

        // -------------------------------------------------------------------

        private static IEnumerator LoadGameScene()
        {
            // By name, through the build settings, rather than through
            // EditorSceneManager: this test is worth running on a device too,
            // and an editor-only load would rule that out.
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            // Two frames for Awake/Start to run and GameFlow to stage round 1.
            yield return null;
            yield return null;
        }

        private static bool CardVisible(RoundCompleteCard card)
        {
            // The component sits on an always-active holder and toggles its
            // panel, so "visible" means the child is on.
            foreach (Transform child in card.transform)
            {
                if (child.gameObject.activeSelf) return true;
            }

            return false;
        }

        private static IEnumerator WaitRealSeconds(float seconds)
        {
            var until = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < until) yield return null;
        }

        private static IEnumerator WaitUntil(System.Func<bool> condition, float timeoutSeconds)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
        }
    }
}
