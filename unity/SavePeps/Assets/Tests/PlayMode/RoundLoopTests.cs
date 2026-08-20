using System.Collections;
using NUnit.Framework;
using SavePeps.Core;
using SavePeps.Progression;
using SavePeps.Rescue;
using SavePeps.UI;
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
            var menu = Object.FindFirstObjectByType<GameMenu>();
            var gameFeel = Object.FindFirstObjectByType<GameFeel>();

            Assert.IsNotNull(flow, "The Game scene has no GameFlow.");
            Assert.IsNotNull(runner, "The Game scene has no RescueRunner.");
            Assert.IsNotNull(router, "The Game scene has no TapRouter.");
            Assert.IsNotNull(card, "The Game scene has no RoundCompleteCard.");
            Assert.IsNotNull(menu, "The Game scene has no GameMenu.");
            Assert.IsNotNull(gameFeel, "The Game scene has no shared GameFeel.");
            foreach (var mesh in gameFeel.GetComponentsInChildren<MeshFilter>(includeInactive: true))
            {
                Assert.IsNotNull(mesh.sharedMesh, $"Celebration shape '{mesh.name}' has no built-in mesh.");
            }
            Assert.IsEmpty(gameFeel.GetComponentsInChildren<Collider>(includeInactive: true),
                "Celebration FX should not create colliders that release stripping then rejects on Android.");

            Assert.AreEqual(0, flow.CurrentRound, "A fresh launch should wait on the home screen.");
            Assert.IsTrue(menu.HomeVisible, "A fresh launch should show the home screen.");
            Assert.IsNull(runner.Current, "No rescue should run under the home screen.");

            flow.PlayRecommendedRound();
            Assert.AreEqual(1, flow.CurrentRound, "Only round 1 is available on a fresh save.");

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
                yield return WaitUntil(() => runner.Current == null || runner.Current.Id != id || CardVisible(card), 15f);
            }

            Assert.AreEqual(RoundDefinition.RescuesPerRound, played);
            Assert.AreEqual(3, flow.Save.TotalRescuesSolved);
            Assert.AreEqual(2, flow.Save.HighestUnlockedRound,
                "Finishing round 1 should unlock round 2.");

            // The card is the visible end of the round.
            yield return WaitUntil(() => CardVisible(card), 8f);
            Assert.IsTrue(CardVisible(card), "The round-complete card never appeared.");
            var resultMarks = card.GetComponentsInChildren<MasteryMarkGraphic>(includeInactive: false);
            Assert.AreEqual(RoundDefinition.RescuesPerRound, resultMarks.Length,
                "The completion card should make all three mastery marks the reward.");
            foreach (var mark in resultMarks)
            {
                Assert.AreEqual(MasteryMarkState.Star, mark.State,
                    "A first-tap round should resolve into three visible stars.");
            }

            flow.KeepPlaying();
            Assert.AreEqual(2, flow.CurrentRound,
                "Keep playing should avoid the just-finished round when round 2 is available.");
            Assert.AreEqual(2, flow.Save.LastPlayedRound);
        }

        [UnityTest]
        public IEnumerator AWrongTapCostsTheStarButNotTheProgress()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var router = Object.FindFirstObjectByType<TapRouter>();
            var hud = Object.FindFirstObjectByType<RescueHud>();

            Assert.IsNotNull(hud, "The Game scene has no RescueHud.");
            flow.PlayRecommendedRound();

            var rescue = runner.Current;
            Assert.IsNotNull(rescue);
            var id = rescue.Id;
            yield return WaitUntil(() => router.InputEnabled, 5f);

            // Tap a wrong object. The quip gets one brief beat, then the scene
            // resets itself without a generic Try Again interruption.
            var wrong = System.Array.Find(rescue.Objects, o => o != null && !rescue.IsCorrect(o));
            Assert.IsNotNull(wrong, "Every rescue must offer a wrong object.");

            var correctId = rescue.Correct.Id;

            router.SimulateTap(wrong.Id);
            yield return WaitUntil(() => hud.QuipVisible, 8f);

            Assert.AreEqual(Mark.None, flow.Save.MarkFor(id), "A wrong tap must not mark the rescue solved.");
            Assert.IsFalse(router.InputEnabled,
                "Input must stay locked while the consequence and quip land.");

            yield return WaitUntil(() => router.InputEnabled, 5f);
            Assert.IsFalse(hud.QuipVisible, "The quip should leave with the automatic reset.");

            router.SimulateTap(correctId);
            yield return WaitUntil(() => flow.Save.MarkFor(id) != Mark.None, 12f);

            Assert.AreEqual(Mark.Check, flow.Save.MarkFor(id),
                "Solved after a wrong tap should be a check, not a star.");
            Assert.AreEqual(1, flow.Save.TotalRescuesSolved);
        }

        [UnityTest]
        public IEnumerator ChooseRoundUsesThePickerWithoutChangingProgression()
        {
            var save = SaveData.Fresh();
            save.UnlockThrough(3);
            Assert.IsTrue(SaveStore.Save(save));

            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var menu = Object.FindFirstObjectByType<GameMenu>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var router = Object.FindFirstObjectByType<TapRouter>();

            flow.ShowRoundPickerFromHome();
            yield return null;
            Assert.IsTrue(menu.PickerVisible);
            Assert.AreEqual(3, menu.Items.Count);

            RoundPickerItem roundTwo = null;
            foreach (var item in menu.Items)
            {
                if (item.RoundNumber == 2) roundTwo = item;
            }

            Assert.IsNotNull(roundTwo);
            Assert.AreEqual(RoundAccess.Playable, roundTwo.AccessState);
            roundTwo.Select();
            yield return WaitUntil(() => flow.CurrentRound == 2, 2f);

            Assert.AreEqual(2, flow.CurrentRound);
            Assert.IsFalse(router.InputEnabled,
                "The picker pointer-up must stay locked during the entrance instead of falling through.");
            Assert.AreEqual(2, flow.Save.LastPlayedRound);
            Assert.AreEqual(3, flow.Save.HighestUnlockedRound,
                "Choosing a round must not alter the sequential unlock.");
            Assert.IsNotNull(runner.Current);
            Assert.AreEqual("r04", runner.Current.Id);
        }

        // -------------------------------------------------------------------

        private static IEnumerator LoadGameScene()
        {
            // By name, through the build settings, rather than through
            // EditorSceneManager: this test is worth running on a device too,
            // and an editor-only load would rule that out.
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            // Two frames for Awake/Start to run and GameFlow to present home.
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
