using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SavePeps.Monetization;
using SavePeps.Progression;
using SavePeps.Rescue;
using SavePeps.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SavePeps.Tests
{
    /// <summary>The guarded runtime seams exercised in the real generated scene.</summary>
    public sealed class TesterModeTests
    {
        private const string SceneName = "Game";
        private float _timeScale;

        [SetUp]
        public void SetUp()
        {
            SaveStore.Delete();
            _timeScale = Time.timeScale;
            Time.timeScale = 5f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = _timeScale;
            SaveStore.Delete();
        }

        [UnityTest]
        public IEnumerator UserModeIsAlwaysTheBootStateAndSecretSequenceTogglesIt()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var tester = Object.FindFirstObjectByType<TesterMode>();
            var menu = Object.FindFirstObjectByType<GameMenu>();

            Assert.IsFalse(tester.Active);
            Assert.IsFalse(tester.Visible);
            Assert.IsNull(GameObject.Find("TesterIndicator"));
            Assert.AreEqual(RoundAccess.SubscriptionLocked, flow.AccessFor(12));

            SubmitSecretSequence();
            Assert.IsTrue(tester.Active);
            Assert.IsTrue(tester.Visible, "Activating Tester Mode opens Tester Tools immediately.");
            Assert.AreEqual(0, flow.Save.TotalRescuesSolved);

            Click("TesterClose");
            yield return null;
            Assert.IsFalse(tester.Visible, "Tapping CLOSE hides Tester Tools.");
            Assert.IsNotNull(GameObject.Find("TesterIndicator"));
            Assert.IsTrue(menu.HomeVisible, "Home screen should remain visible and accessible in Tester Mode.");

            Click("TesterIndicator");
            yield return null;
            Assert.IsTrue(tester.Visible, "Tapping the TESTER indicator opens Tester Tools.");

            // The sheet covers home, so the tap areas are only under a finger
            // again once it is closed.
            yield return CloseTesterTools(tester);

            SubmitSecretSequence();
            yield return null;
            Assert.IsFalse(tester.Active);
            Assert.IsNull(GameObject.Find("TesterIndicator"));
            Assert.AreEqual("PLAY", ButtonNamed("Play").GetComponentInChildren<UnityEngine.UI.Text>().text);
            Assert.AreEqual(RoundAccess.SubscriptionLocked, flow.AccessFor(12));
        }

        [UnityTest]
        public IEnumerator TappingHeartSevenTimesTogglesTesterModeAndExitButtonDeactivatesIt()
        {
            yield return LoadGameScene();

            var tester = Object.FindFirstObjectByType<TesterMode>();
            Assert.IsFalse(tester.Active);

            // Seven taps on the heart, and Tester Tools is open.
            TapHeartSevenTimes();
            Assert.IsTrue(tester.Active, "7 taps on the secret heart should activate Tester Mode.");
            Assert.IsTrue(tester.Visible, "Tester Tools should be visible upon activation.");

            // Seven more, from home, and the game is back in Normal Mode.
            yield return CloseTesterTools(tester);
            TapHeartSevenTimes();
            Assert.IsFalse(tester.Active, "7 taps on the secret heart should deactivate Tester Mode.");
            Assert.IsFalse(tester.Visible);

            // The switch keeps working, so this is a toggle rather than a
            // one-shot door. Leaving Tester Mode rebuilds home, which fades in
            // before it takes taps again.
            yield return WaitUntil(() => TapReaches("TesterSecretHeart"), 2f);
            TapHeartSevenTimes();
            Assert.IsTrue(tester.Active);
            Assert.IsTrue(tester.Visible);

            // Click NORMAL MODE button to deactivate
            Click("TesterExitMode");
            yield return null;
            Assert.IsFalse(tester.Active, "Clicking NORMAL MODE button should switch back to Normal Mode.");
            Assert.IsFalse(tester.Visible);
        }

        [UnityTest]
        public IEnumerator RoundTwelvePlayInTesterModeRecordsProgressAndUnlocks()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var router = Object.FindFirstObjectByType<TapRouter>();
            var tester = Object.FindFirstObjectByType<TesterMode>();

            Assert.AreEqual(RoundAccess.SubscriptionLocked, flow.AccessFor(12));
            SubmitSecretSequence();
            Assert.IsTrue(tester.Active);

            // Play round 12 via Tester Tools / TesterPlay (direct access regardless of ACCESS)
            Assert.IsTrue(flow.TesterPlay(12, 0));
            yield return WaitUntil(() => runner.AwaitingChoice, 8f);

            Assert.AreEqual(12, flow.CurrentRound);
            Assert.AreEqual(0, flow.CurrentRescueIndex);
            Assert.IsFalse(flow.TesterPreviewActive, "Normal Play in Tester Mode is not a preview.");

            // Solve rescue 1
            var r1 = flow.Catalog.Round(12).RescueAt(0);
            router.SimulateTap(r1.Correct.Id);
            yield return WaitUntil(() => flow.CurrentRescueIndex == 1, 8f);
            yield return WaitUntil(() => runner.AwaitingChoice, 8f);

            Assert.AreEqual(Mark.Star, flow.Save.MarkFor(r1.Id), "First tap should earn a star.");
            Assert.AreEqual(1, flow.Save.TotalRescuesSolved);

            // Solve rescue 2
            var r2 = flow.Catalog.Round(12).RescueAt(1);
            router.SimulateTap(r2.Correct.Id);
            yield return WaitUntil(() => flow.CurrentRescueIndex == 2, 8f);
            yield return WaitUntil(() => runner.AwaitingChoice, 8f);

            // Solve rescue 3
            var r3 = flow.Catalog.Round(12).RescueAt(2);
            router.SimulateTap(r3.Correct.Id);
            yield return WaitUntil(() => flow.Save.TotalRescuesSolved == 3, 8f);

            Assert.IsTrue(RoundProgress.Read(flow.Catalog.Round(12), flow.Save).IsComplete,
                "Completing 3 rescues in Tester Mode must mark Round 12 as complete in save.");
        }

        [UnityTest]
        public IEnumerator ChooseRoundRespectsSimulatedEntitlementUnderFreeAndUnlimited()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var fake = Object.FindFirstObjectByType<FakeEntitlementService>();
            var menu = Object.FindFirstObjectByType<GameMenu>();
            var tester = Object.FindFirstObjectByType<TesterMode>();

            SubmitSecretSequence();
            Assert.IsTrue(tester.Active);

            // 1. FREE simulated entitlement
            fake.SetSubscribed(false);
            flow.ShowRoundPickerFromHome();
            yield return null;

            Assert.AreEqual(12, menu.Items.Count);
            for (var i = 0; i < 10; i++)
            {
                Assert.AreEqual(RoundAccess.Playable, menu.Items[i].AccessState,
                    $"Round {i + 1} should be Playable under FREE in Tester Mode.");
                Assert.IsTrue(menu.Items[i].Interactable);
            }
            for (var i = 10; i < 12; i++)
            {
                Assert.AreEqual(RoundAccess.SubscriptionLocked, menu.Items[i].AccessState,
                    $"Round {i + 1} should be SubscriptionLocked under FREE in Tester Mode.");
                Assert.IsFalse(menu.Items[i].Interactable,
                    $"Round {i + 1} should not be interactable under FREE in Tester Mode.");
            }

            // Tapping locked round does not dismiss or get stuck
            menu.Items[11].Select();
            yield return null;
            Assert.IsTrue(menu.PickerVisible);
            Assert.AreEqual(0, flow.CurrentRound);

            // 2. PEPS UNLIMITED simulated entitlement
            fake.SetSubscribed(true);
            yield return null;

            Assert.AreEqual(12, menu.Items.Count);
            for (var i = 0; i < 12; i++)
            {
                Assert.AreEqual(RoundAccess.Playable, menu.Items[i].AccessState,
                    $"Round {i + 1} should be Playable under PEPS UNLIMITED in Tester Mode.");
                Assert.IsTrue(menu.Items[i].Interactable);
            }
        }

        [UnityTest]
        public IEnumerator FreshProfileCanInspectRoundNineRescueTwoWithoutChangingAccess()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var tester = Object.FindFirstObjectByType<TesterMode>();

            Assert.IsNotNull(tester, "The generated scene has no Tester Mode component.");
            Assert.IsTrue(TesterMode.Available, "Tester Mode should be available in Editor play mode.");
            Assert.AreEqual(RoundAccess.ProgressLocked, flow.AccessFor(9));

            var expected = flow.Catalog.Round(9).RescueAt(1);
            Assert.IsTrue(flow.TesterJumpTo(9, 1));
            yield return WaitUntil(() => runner.AwaitingChoice, 8f);

            Assert.AreSame(expected, runner.Current);
            Assert.AreEqual(9, flow.CurrentRound);
            Assert.AreEqual(1, flow.CurrentRescueIndex);
            Assert.IsTrue(flow.TesterPreviewActive);
            Assert.AreEqual(1, flow.Save.HighestUnlockedRound);
            Assert.AreEqual(0, flow.Save.LastPlayedRound);
            Assert.AreEqual(RoundAccess.ProgressLocked, flow.AccessFor(9),
                "The QA bypass must not weaken the real access function.");
        }

        [UnityTest]
        public IEnumerator WrongAndCorrectPreviewsNeverWriteOrAdvance()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var router = Object.FindFirstObjectByType<TapRouter>();
            var rescue = flow.Catalog.Round(9).RescueAt(1);
            var wrong = System.Array.Find(rescue.Objects, o => o != null && !rescue.IsCorrect(o));

            Assert.IsTrue(flow.TesterJumpTo(9, 1));
            yield return WaitUntil(() => runner.AwaitingChoice, 8f);
            router.SimulateTap(wrong.Id);
            yield return WaitUntil(() => runner.AwaitingChoice, 8f);

            Assert.AreEqual(Mark.None, flow.Save.MarkFor(rescue.Id));
            Assert.AreEqual(0, flow.Save.TotalRescuesSolved);

            Assert.IsTrue(flow.TesterJumpTo(9, 1));
            yield return WaitUntil(() => runner.AwaitingChoice, 8f);
            var solved = false;
            runner.OnSolved += _ => solved = true;
            router.SimulateTap(rescue.Correct.Id);
            yield return WaitUntil(() => solved, 8f);
            yield return null;

            Assert.AreSame(rescue, runner.Current);
            Assert.AreEqual(9, flow.CurrentRound);
            Assert.AreEqual(1, flow.CurrentRescueIndex,
                "A previewed reunion must not advance to the next rescue.");
            Assert.AreEqual(Mark.None, flow.Save.MarkFor(rescue.Id));
            Assert.AreEqual(0, flow.Save.TotalRescuesSolved);
        }

        [UnityTest]
        public IEnumerator ResetIsRepeatableAndPreservesFakeEntitlementSeparation()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var fake = Object.FindFirstObjectByType<FakeEntitlementService>();
            var menu = Object.FindFirstObjectByType<GameMenu>();

            fake.SetSubscribed(true);
            flow.TesterApplyProfile(TesterProfilePreset.AllPerfect);
            Assert.AreEqual(36, flow.Save.TotalRescuesSolved);

            flow.TesterApplyProfile(TesterProfilePreset.Fresh);
            flow.TesterApplyProfile(TesterProfilePreset.Fresh);

            Assert.IsTrue(menu.HomeVisible);
            Assert.AreEqual(1, flow.Save.HighestUnlockedRound);
            Assert.AreEqual(0, flow.Save.LastPlayedRound);
            Assert.AreEqual(0, flow.Save.TotalRescuesSolved);
            Assert.IsTrue(fake.IsSubscribed,
                "Local-profile reset must not rewrite the entitlement service.");

            var reloaded = SaveStore.Load();
            Assert.AreEqual(1, reloaded.HighestUnlockedRound);
            Assert.AreEqual(0, reloaded.LastPlayedRound);
            Assert.AreEqual(0, reloaded.TotalRescuesSolved);
        }

        [UnityTest]
        public IEnumerator ExitReturnsToHomeAndNormalPaidGate()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var menu = Object.FindFirstObjectByType<GameMenu>();

            Assert.AreEqual(RoundAccess.SubscriptionLocked, flow.AccessFor(12));
            Assert.IsTrue(flow.TesterJumpTo(12, 2));
            yield return WaitUntil(() => runner.AwaitingChoice, 8f);
            Assert.IsNotNull(runner.Current);

            flow.EndTesterSession();
            yield return null;

            Assert.IsTrue(menu.HomeVisible);
            Assert.IsNull(runner.Current);
            Assert.IsFalse(flow.TesterPreviewActive);
            Assert.AreEqual(RoundAccess.SubscriptionLocked, flow.AccessFor(12));
        }

        [UnityTest]
        public IEnumerator GoToRoundTwelveRescueTwoPlayRescueLaunchesDirectly()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var tester = Object.FindFirstObjectByType<TesterMode>();

            SubmitSecretSequence();
            Assert.IsTrue(tester.Active);
            Assert.IsTrue(tester.Visible);

            Click("TesterRound_12");
            Click("TesterRescue_2");
            Click("TesterPlayRescue");
            yield return WaitUntil(() => runner.AwaitingChoice, 8f);

            Assert.IsFalse(tester.Visible);
            Assert.AreEqual(12, flow.CurrentRound);
            Assert.AreEqual(1, flow.CurrentRescueIndex);
            Assert.AreSame(flow.Catalog.Round(12).RescueAt(1), runner.Current);
            Assert.IsFalse(flow.TesterPreviewActive);
        }

        [UnityTest]
        public IEnumerator AccessToggleSwitchesFreeAndUnlimitedImmediately()
        {
            yield return LoadGameScene();

            var fake = Object.FindFirstObjectByType<FakeEntitlementService>();
            var tester = Object.FindFirstObjectByType<TesterMode>();

            SubmitSecretSequence();
            Assert.IsTrue(tester.Active);
            Assert.IsTrue(tester.Visible);

            Click("TesterUnlimited");
            Assert.IsTrue(fake.IsSubscribed);

            Click("TesterFree");
            Assert.IsFalse(fake.IsSubscribed);

            Click("TesterUnlimited");
            Assert.IsTrue(fake.IsSubscribed);
        }

        [UnityTest]
        public IEnumerator ClearAllProgressRequiresConfirmationAndResetsProfile()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var tester = Object.FindFirstObjectByType<TesterMode>();

            flow.Save.UnlockThrough(5);
            flow.Save.LastPlayedRound = 4;
            flow.Save.RecordSolved("r1", firstTap: true);
            Assert.AreEqual(1, flow.Save.TotalRescuesSolved);

            SubmitSecretSequence();
            Assert.IsTrue(tester.Active);
            Assert.IsTrue(tester.Visible);

            // First click enters confirmation state without clearing yet
            Click("TesterClearProgress");
            Assert.AreEqual(1, flow.Save.TotalRescuesSolved);
            Assert.AreEqual(5, flow.Save.HighestUnlockedRound);

            // Second click confirms and executes fresh reset
            Click("TesterClearProgress");
            yield return null;

            Assert.AreEqual(0, flow.Save.TotalRescuesSolved);
            Assert.AreEqual(1, flow.Save.HighestUnlockedRound);
            Assert.AreEqual(0, flow.Save.LastPlayedRound);
        }

        [UnityTest]
        public IEnumerator UserModeChooseRoundFollowsRealEntitlement()
        {
            yield return LoadGameScene();

            var flow = Object.FindFirstObjectByType<GameFlow>();
            var menu = Object.FindFirstObjectByType<GameMenu>();
            var tester = Object.FindFirstObjectByType<TesterMode>();
            var fake = Object.FindFirstObjectByType<FakeEntitlementService>();

            // Ensure Tester Mode is NOT active (normal User Mode)
            Assert.IsFalse(tester.Active);

            // 1. Free user with a fresh save: Round 1 playable, 2-10 progress locked, 11-12 subscription locked
            fake.SetSubscribed(false);
            flow.ShowRoundPickerFromHome();
            yield return null;

            Assert.AreEqual(12, menu.Items.Count);
            Assert.AreEqual(RoundAccess.Playable, menu.Items[0].AccessState);
            Assert.IsTrue(menu.Items[0].Interactable);

            for (var i = 1; i < 10; i++)
            {
                Assert.AreEqual(RoundAccess.ProgressLocked, menu.Items[i].AccessState,
                    $"Round {i + 1} should be ProgressLocked for fresh free user.");
                Assert.IsFalse(menu.Items[i].Interactable);
            }

            for (var i = 10; i < 12; i++)
            {
                Assert.AreEqual(RoundAccess.SubscriptionLocked, menu.Items[i].AccessState,
                    $"Round {i + 1} should be SubscriptionLocked for free user.");
                Assert.IsFalse(menu.Items[i].Interactable);
            }

            // Normal PlayRound(12) is blocked by production gating and sets pending round
            flow.PlayRound(12);
            yield return null;
            Assert.AreEqual(0, flow.CurrentRound);

            // 2. Real Peps Unlimited purchase: auto-launches pending Round 12
            fake.SetSubscribed(true);
            yield return null;
            Assert.AreEqual(12, flow.CurrentRound);

            // 3. Opening picker as a subscriber: all 12 rounds playable and interactable immediately
            flow.ShowRoundPickerFromHome();
            yield return null;

            Assert.AreEqual(12, menu.Items.Count);
            for (var i = 0; i < 12; i++)
            {
                Assert.AreEqual(RoundAccess.Playable, menu.Items[i].AccessState,
                    $"Round {i + 1} should be Playable for subscriber.");
                Assert.IsTrue(menu.Items[i].Interactable);
            }

            // Subscriber can play any round (e.g. Round 6) immediately
            menu.Items[5].Select();
            yield return null;
            Assert.AreEqual(6, flow.CurrentRound);
        }

        private static IEnumerator LoadGameScene()
        {
            yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);
            yield return null;
            yield return null;
            // Home fades in, and its CanvasGroup only starts blocking raycasts
            // once that finishes, so a tap sent any earlier lands on nothing.
            yield return WaitUntil(() => TapReaches("TesterSecretHeart"), 3f);
        }

        private static void TapHeartSevenTimes()
        {
            for (var i = 0; i < 7; i++) Tap("TesterSecretHeart");
        }

        /// <summary>
        /// Closes the sheet and waits for the home tap areas to be reachable
        /// again. CLOSE runs a coroutine that holds the panel busy for a frame,
        /// and a tap taken during it is dropped on purpose.
        /// </summary>
        private static IEnumerator CloseTesterTools(TesterMode tester)
        {
            Click("TesterClose");
            yield return null;
            yield return null;
            Assert.IsFalse(tester.Visible, "CLOSE should hide Tester Tools.");
            yield return WaitUntil(() => TapReaches("TesterSecretHeart"), 2f);
        }

        private static void SubmitSecretSequence()
        {
            Tap("TesterSecretHeart");
            Tap("TesterSecretGreenPep");
            Tap("TesterSecretPinkPep");
            Tap("TesterSecretHeart");
            Tap("TesterSecretGreenPep");
            Tap("TesterSecretPinkPep");
            Tap("TesterSecretHeart");
        }

        /// <summary>
        /// A tap the way a finger makes one: an EventSystem raycast at that
        /// point on screen, then the click delivered to whatever the raycast
        /// actually found.
        ///
        /// Invoking <c>onClick</c> directly passes even when nothing on screen
        /// can reach the button, which is how the three secret tap areas came
        /// to be untappable — transparent mesh culling dropped their meshes and
        /// GraphicRaycaster skips culled graphics — while this suite stayed
        /// green. Anything a player is expected to touch is tapped, not invoked.
        /// </summary>
        private static void Tap(string objectName)
        {
            var target = ButtonNamed(objectName);
            var reached = TopGraphicAt(target);
            Assert.IsNotNull(reached, $"A tap at the centre of '{objectName}' reaches nothing at all.");

            var handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(reached);
            Assert.AreSame(target.gameObject, handler,
                $"A tap at the centre of '{objectName}' is answered by " +
                $"'{(handler == null ? "nothing" : handler.name)}' instead.");

            var pointer = new PointerEventData(EventSystem.current) { position = ScreenPointOf(target) };
            ExecuteEvents.Execute(handler, pointer, ExecuteEvents.pointerClickHandler);
        }

        /// <summary>True when a tap at the centre of the named button reaches it.</summary>
        private static bool TapReaches(string objectName)
        {
            var target = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.name == objectName);
            if (target == null) return false;
            var reached = TopGraphicAt(target);
            return reached != null &&
                   ExecuteEvents.GetEventHandler<IPointerClickHandler>(reached) == target.gameObject;
        }

        private static GameObject TopGraphicAt(Component target)
        {
            if (EventSystem.current == null) return null;
            var pointer = new PointerEventData(EventSystem.current) { position = ScreenPointOf(target) };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            return hits.Count == 0 ? null : hits[0].gameObject;
        }

        // The shell canvas is Screen Space - Overlay, so its world space and
        // screen space are the same space and no camera is involved.
        private static Vector2 ScreenPointOf(Component target) =>
            RectTransformUtility.WorldToScreenPoint(null, target.transform.position);

        private static void Click(string objectName) => ButtonNamed(objectName).onClick.Invoke();

        private static UnityEngine.UI.Button ButtonNamed(string objectName)
        {
            var button = Object.FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate.name == objectName);
            Assert.IsNotNull(button, $"No active button named '{objectName}' was found.");
            return button;
        }

        private static IEnumerator WaitUntil(System.Func<bool> condition, float timeoutSeconds)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
        }
    }
}
