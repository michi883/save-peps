using System.Collections;
using NUnit.Framework;
using SavePeps.Core;
using SavePeps.Progression;
using SavePeps.Rescue;
using SavePeps.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SavePeps.Tests
{
    /// <summary>
    /// The shell around gameplay: pause, progress, settings, and Back.
    ///
    /// The interesting property is not that each panel opens — it is that tap
    /// input is suspended for the whole visit and handed back exactly once. A
    /// detour through Progress and back to the pause sheet is the path where
    /// that is easiest to get wrong, and the symptom on a device would be a
    /// rescue that silently stops accepting taps.
    /// </summary>
    public sealed class ShellTests
    {
        private const string SceneName = "Game";

        [SetUp]
        public void SetUp() => SaveStore.Delete();

        [TearDown]
        public void TearDown() => SaveStore.Delete();

        [UnityTest]
        public IEnumerator PauseSuspendsTheRescueAndBackResumesIt()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var router = Object.FindFirstObjectByType<TapRouter>();
            var pause = Object.FindFirstObjectByType<PauseOverlay>();

            Assert.IsNotNull(pause, "The Game scene has no PauseOverlay.");

            flow.PlayRecommendedRound();
            yield return WaitUntil(() => runner.AwaitingChoice, 10f);
            Assert.IsTrue(runner.AwaitingChoice, "The rescue never accepted input.");

            flow.OpenPause();
            Assert.IsTrue(pause.Visible, "The pause sheet did not open.");
            Assert.IsFalse(router.InputEnabled, "Pausing must suspend taps on the diorama.");
            Assert.IsNotNull(runner.Current, "Pausing must not tear the rescue down.");

            flow.HandleBack();
            yield return WaitUntil(() => !pause.Visible, 3f);
            Assert.IsFalse(pause.Visible, "Back did not close the pause sheet.");
            Assert.IsTrue(router.InputEnabled, "Resuming must hand tap input back.");
            Assert.IsTrue(runner.AwaitingChoice, "The same rescue should still be waiting.");
        }

        [UnityTest]
        public IEnumerator HomeExposesReleaseLegalLinks()
        {
            yield return LoadGameScene();

            var menu = Object.FindFirstObjectByType<GameMenu>();
            Assert.IsNotNull(menu, "The Game scene has no GameMenu.");

            var privacy = Find(menu, "Privacy");
            var terms = Find(menu, "Terms");
            Assert.AreEqual("Privacy", privacy.GetComponentInChildren<Text>().text);
            Assert.AreEqual("Terms", terms.GetComponentInChildren<Text>().text);
        }

        [UnityTest]
        public IEnumerator ProgressOpensFromPauseAndBackReturnsToIt()
        {
            var save = SaveData.Fresh();
            save.UnlockThrough(2);
            save.RecordSolved("r01", firstTap: true);
            save.RecordSolved("r02", firstTap: false);
            Assert.IsTrue(SaveStore.Save(save));

            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var router = Object.FindFirstObjectByType<TapRouter>();
            var pause = Object.FindFirstObjectByType<PauseOverlay>();
            var progress = Object.FindFirstObjectByType<ProgressPanel>();

            Assert.IsNotNull(progress, "The Game scene has no ProgressPanel.");

            flow.PlayRecommendedRound();
            yield return WaitUntil(() => runner.AwaitingChoice, 10f);

            flow.OpenPause();
            yield return WaitUntil(() => pause.Visible, 2f);

            Find(pause, "Progress").onClick.Invoke();
            yield return WaitUntil(() => progress.Visible, 3f);
            Assert.IsTrue(progress.Visible, "Progress did not open from the pause sheet.");
            Assert.IsFalse(router.InputEnabled, "Input must stay suspended through the detour.");

            var rows = progress.Rows;
            Assert.AreEqual(flow.Catalog.Rounds.Length, rows.Count, "Progress should list every authored round.");
            Assert.AreEqual(1, rows[0].RoundNumber);

            flow.HandleBack();
            yield return WaitUntil(() => pause.Visible && !progress.Visible, 3f);
            Assert.IsTrue(pause.Visible, "Back from Progress should return to the pause sheet.");

            flow.HandleBack();
            yield return WaitUntil(() => !pause.Visible, 3f);
            Assert.IsTrue(router.InputEnabled, "Input must come back exactly once, after the resume.");
        }

        [UnityTest]
        public IEnumerator HomeFromPauseLeavesGameplayWithoutLosingProgress()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var menu = Object.FindFirstObjectByType<GameMenu>();
            var pause = Object.FindFirstObjectByType<PauseOverlay>();

            flow.PlayRecommendedRound();
            yield return WaitUntil(() => runner.AwaitingChoice, 10f);
            Assert.AreEqual(1, flow.Save.LastPlayedRound);

            flow.OpenPause();
            yield return WaitUntil(() => pause.Visible, 2f);

            Find(pause, "Home").onClick.Invoke();
            yield return WaitUntil(() => menu.HomeVisible, 3f);

            Assert.IsTrue(menu.HomeVisible, "Home from the pause sheet should return to the title.");
            Assert.IsNull(runner.Current, "Leaving gameplay should clear the staged rescue.");
            Assert.AreEqual(1, flow.Save.HighestUnlockedRound,
                "Leaving a round must not change progression.");
        }

        [UnityTest]
        public IEnumerator SettingsTogglesPersistAndReachTheFeedbackLayer()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var pause = Object.FindFirstObjectByType<PauseOverlay>();
            var feedback = Object.FindFirstObjectByType<Feedback>();

            flow.PlayRecommendedRound();
            yield return WaitUntil(() => runner.AwaitingChoice, 10f);

            flow.OpenPause();
            yield return WaitUntil(() => pause.Visible, 2f);

            Assert.IsTrue(feedback.SoundEnabled);
            Assert.IsTrue(feedback.HapticsAllowed);

            Find(pause, "Sound").onClick.Invoke();
            Find(pause, "Haptics").onClick.Invoke();
            yield return null;

            Assert.IsTrue(flow.Save.SoundMuted, "The sound toggle should write to the save.");
            Assert.IsTrue(flow.Save.HapticsOff, "The vibration toggle should write to the save.");
            Assert.IsFalse(feedback.SoundEnabled, "A muted save must reach the audio layer.");
            Assert.IsFalse(feedback.HapticsAllowed, "A vibration-off save must reach the haptics layer.");

            var vibrationLabel = Find(pause, "Haptics").GetComponentInChildren<Text>();
            Assert.AreEqual("VIBRATION OFF", vibrationLabel.text,
                "The setting should name the physical effect instead of calling it buzz.");

            var reloaded = SaveStore.Load();
            Assert.IsTrue(reloaded.SoundMuted, "Settings should survive a restart.");
            Assert.IsTrue(reloaded.HapticsOff, "Settings should survive a restart.");
        }

        // -------------------------------------------------------------------

        private static Button Find(Component parent, string name)
        {
            foreach (var button in parent.GetComponentsInChildren<Button>(includeInactive: true))
            {
                if (button.gameObject.name == name) return button;
            }

            Assert.Fail($"No button named '{name}' under {parent.name}.");
            return null;
        }

        private static IEnumerator LoadGameScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            yield return null;
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
