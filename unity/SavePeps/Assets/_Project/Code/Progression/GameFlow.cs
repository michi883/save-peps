using System;
using System.Collections;
using SavePeps.Core;
using SavePeps.Monetization;
using SavePeps.Rescue;
using SavePeps.UI;
using UnityEngine;

namespace SavePeps.Progression
{
    /// <summary>
    /// Sequences the game: rescue, rescue, rescue, round complete, next round.
    ///
    /// This is the only component that knows about progression, and it holds
    /// the entire gating rule (<see cref="CanPlay"/>). Keeping that in one
    /// readable method matters more than it looks — a paywall that is checked
    /// in three places is a paywall with three chances to be wrong, and the
    /// failure mode is either giving the game away or blocking a paying
    /// customer.
    /// </summary>
    public sealed class GameFlow : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private Catalog _catalog;

        [Header("Scene")]
        [SerializeField] private RescueRunner _runner;
        [SerializeField] private RescueHud _hud;
        [SerializeField] private RoundCompleteCard _card;
        [SerializeField] private GameMenu _menu;
        [SerializeField] private PauseOverlay _pause;
        [SerializeField] private ProgressPanel _progress;
        [SerializeField] private FullGameUnlockPanel _unlock;
        [SerializeField] private TesterMode _testerMode;
        [SerializeField] private Feedback _feedback;

        [Tooltip("Editor/test entitlement and store stand-in.")]
        [SerializeField] private MonoBehaviour _entitlementSource;

        [Tooltip("RevenueCat entitlement and store source used by Android players.")]
        [SerializeField] private MonoBehaviour _deviceEntitlementSource;

        [Header("Pacing")]
        [Tooltip("Seconds after the authored outcome before the next rescue drops in. The reunion itself is already inside that outcome.")]
        [SerializeField, Range(0.5f, 5f)] private float _winDwell = 1.35f;

        private IEntitlementService _entitlements;
        private IEntitlementService _testerEntitlements;
        private IEntitlementService _deviceEntitlements;
        private IFullGameStore _deviceStore;
        private SaveData _save;
        private int _roundNumber;
        private int _rescueIndex;
        private int _pendingRound;
        private Action _pickerBack;
        private bool _pickerShowsHomeDiorama;
        private bool _progressFromPause;
        private bool _testerPreviewActive;
        private bool _testerPlayThrough;

        /// <summary>Raised with the round the player just tried to reach.</summary>
        public event Action<int> OnPaywallRequested;

        /// <summary>Raised when the player finishes the last authored round.</summary>
        public event Action OnCatalogComplete;

        public SaveData Save => _save;
        public int CurrentRound => _roundNumber;
        public int CurrentRescueIndex => _rescueIndex;
        public Catalog Catalog => _catalog;
        public bool HasFullGame => FullGameUnlocked;
        public bool TesterPreviewActive => _testerPreviewActive;
        public string TesterBilling => Debug.isDebugBuild || Application.isEditor ? "Test Store" : "Google Play";
        public bool TesterStoreOwned => _deviceEntitlements is { HasFullGame: true };
        public bool TesterStoreProductReady => _deviceStore is { ProductReady: true };
        public string TesterStorePrice => _deviceStore?.LocalizedPrice;

        // -------------------------------------------------------------------
        // Boot
        // -------------------------------------------------------------------

        private void Awake()
        {
            var source = _entitlementSource;
            _testerEntitlements = _entitlementSource as IEntitlementService;
            _deviceEntitlements = _deviceEntitlementSource as IEntitlementService;
            _deviceStore = _deviceEntitlementSource as IFullGameStore;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_deviceEntitlementSource != null) source = _deviceEntitlementSource;
#endif
            _entitlements = source as IEntitlementService;
            if (source != null && _entitlements == null)
            {
                Debug.LogError(
                    $"[SavePeps] '{source.GetType().Name}' is wired as the entitlement source but does " +
                    "not implement IEntitlementService. Paid rounds will stay locked.");
            }

            _save = SaveStore.Load();
            _hud?.SetVisible(false);
            _card?.Hide();
            _menu?.Hide();
            _pause?.Hide();
            _progress?.Hide();
            _unlock?.Hide();
            ApplySettings();
        }

        private void OnEnable()
        {
            if (_runner != null) _runner.OnSolved += HandleSolved;
            if (_hud != null) _hud.OnMenuRequested += OpenPause;
            if (_entitlements != null) _entitlements.Changed += HandleEntitlementChanged;
            if (_testerEntitlements != null && _testerEntitlements != _entitlements)
            {
                _testerEntitlements.Changed += HandleEntitlementChanged;
            }
        }

        private void OnDisable()
        {
            if (_runner != null) _runner.OnSolved -= HandleSolved;
            if (_hud != null) _hud.OnMenuRequested -= OpenPause;
            if (_entitlements != null) _entitlements.Changed -= HandleEntitlementChanged;
            if (_testerEntitlements != null && _testerEntitlements != _entitlements)
            {
                _testerEntitlements.Changed -= HandleEntitlementChanged;
            }
        }

        private void Start()
        {
            _entitlements?.Initialise();

            if (_catalog == null || _catalog.RoundCount == 0)
            {
                Debug.LogError("[SavePeps] GameFlow has no catalog. Nothing to play.");
                return;
            }

            // Opening the app is intentionally a choice-free pause. Play is
            // one tap away and chooses well; direct control is beside it.
            // Keeping this in the same scene avoids a load and leaves editor
            // preview free to disable GameFlow before Start stages anything.
            if (_menu != null) ShowHome();
            else PlayRecommendedRound();
        }

        /// <summary>
        /// Persisting on pause as well as on completion: Android can kill a
        /// backgrounded app without further warning, and losing a finished
        /// round to that is exactly the kind of small betrayal that gets a
        /// game uninstalled.
        /// </summary>
        private void OnApplicationPause(bool paused)
        {
            if (paused) Persist();
        }

        private void OnApplicationQuit() => Persist();

        private void Persist() => SaveStore.Save(_save);

        // -------------------------------------------------------------------
        // The gate — the only access rule in the game
        // -------------------------------------------------------------------

        public bool CanPlay(int round) =>
            Access.CanPlay(_catalog, round, _save.HighestUnlockedRound, FullGameUnlocked);

        public RoundAccess AccessFor(int round) =>
            Access.State(_catalog, round, _save.HighestUnlockedRound, FullGameUnlocked);

        private bool FullGameUnlocked =>
            TesterMode.Available && _testerMode is { Active: true }
                ? _testerEntitlements is { HasFullGame: true }
                : _entitlements is { HasFullGame: true };

        // -------------------------------------------------------------------
        // Rounds
        // -------------------------------------------------------------------

        public void ShowHome()
        {
            _pendingRound = 0;
            _testerPreviewActive = false;
            _testerPlayThrough = false;
            _hud?.SetVisible(false);
            _card?.Hide();
            _pause?.Hide();
            _progress?.Hide();
            _unlock?.Hide();
            _runner?.SuspendInput(false);
            _runner?.Teardown();
            _menu?.ShowHome(PlayRecommendedRound, ShowRoundPickerFromHome, ShowProgressFromHome,
                HomeStatLine(), _testerMode?.HomePlayLabel ?? "PLAY");
        }

        /// <summary>The dominant Play action: useful randomness over available rounds.</summary>
        public void PlayRecommendedRound()
        {
            if (_testerMode != null && _testerMode.TryGetPlayTarget(out var testerRound, out var testerRescue))
            {
                Debug.Log($"[SavePeps] Tester Play chose round {testerRound}, rescue {testerRescue + 1}.");
                TesterPlay(testerRound, testerRescue);
                return;
            }

            var number = RoundSelector.Choose(_catalog, _save, FullGameUnlocked, UnityEngine.Random.value);
            if (number <= 0)
            {
                Debug.LogError("[SavePeps] No round is currently available to Play.");
                return;
            }

            Debug.Log($"[SavePeps] Play chose round {number}.");
            PlayRound(number);
        }

        public void ShowRoundPickerFromHome() => ShowRoundPicker(showHomeDiorama: true, back: ShowHome);

        private void ShowRoundPickerFromResult() =>
            ShowRoundPicker(showHomeDiorama: false, back: ShowRoundCompleteCard);

        /// <summary>
        /// One picker, three ways in. The back action is remembered rather
        /// than inferred, so a picker opened from the pause sheet returns to
        /// the rescue the player is standing in instead of to the title.
        /// </summary>
        private void ShowRoundPicker(bool showHomeDiorama, Action back)
        {
            _pickerShowsHomeDiorama = showHomeDiorama;
            _pickerBack = back;
            _card?.Hide();
            _pause?.Hide();
            _progress?.Hide();
            _unlock?.Hide();
            _hud?.SetVisible(false);
            var testerBypass = _testerMode is { Active: true };
            _menu?.ShowPicker(_catalog, _save, FullGameUnlocked, showHomeDiorama, testerBypass,
                SelectRound, back);
            Debug.Log($"[SavePeps] Round picker opened with {_catalog.RoundCount} rounds.");
        }

        private void SelectRound(int number)
        {
            Debug.Log($"[SavePeps] Round {number} selected from picker.");
            if (_testerMode is { Active: true }) _testerMode.SelectTarget(number, 0);
            PlayRound(number);
        }

        public void PlayRound(int number)
        {
            if (!_catalog.Exists(number))
            {
                OnCatalogComplete?.Invoke();
                _hud?.SetVisible(false);
                _card?.ShowOutOfContent(KeepPlaying, ShowRoundPickerFromResult);
                return;
            }

            var isTester = _testerMode is { Active: true };
            var allowed = isTester
                ? (FullGameUnlocked || !_catalog.IsPaid(number))
                : CanPlay(number);

            if (!allowed)
            {
                // Locked by progression is not a sales moment. Only show the
                // unlock when lifetime ownership is what stands between the
                // player and the selected authored round.
                if (Access.IsPaywalled(_catalog, number, _save.HighestUnlockedRound, FullGameUnlocked))
                {
                    _pendingRound = number;
                    _unlock?.Show(_entitlements as IFullGameStore, CancelPendingUnlock);
                    OnPaywallRequested?.Invoke(number);
                }
                else
                {
                    Debug.LogWarning($"[SavePeps] Round {number} is not unlocked yet.");
                }
                return;
            }

            if (_testerMode is { Active: true })
            {
                _testerMode.SelectTarget(number, 0);
            }

            _pendingRound = 0;
            _testerPreviewActive = false;
            _testerPlayThrough = true;
            _roundNumber = number;
            _rescueIndex = 0;
            _save.LastPlayedRound = number;
            Persist();
            _menu?.Hide();
            _card?.Hide();
            _pause?.Hide();
            _progress?.Hide();
            _unlock?.Hide();
            _runner?.SuspendInput(false);
            _hud?.SetVisible(true);
            RefreshDots();
            Debug.Log($"[SavePeps] Round {number} started.");
            StartRescue();
        }

        // -------------------------------------------------------------------
        // Development-only inspection seams
        // -------------------------------------------------------------------

        /// <summary>
        /// Opens the ordinary purchase surface without selecting a round or
        /// touching the simulated ACCESS state. On Android the active store is
        /// RevenueCat; in Editor play the fake store keeps the UI inspectable.
        /// </summary>
        public bool TesterOpenUnlockScreen(Action onDismiss)
        {
            if (!TesterMode.Available || _testerMode is not { Active: true } || _unlock == null)
            {
                return false;
            }

            _pendingRound = 0;
            _unlock.Show(_entitlements as IFullGameStore, onDismiss);
            Debug.Log("[SavePeps] Tester opened the production unlock screen. No round is pending.");
            return true;
        }

        /// <summary>
        /// Stages any authored rescue for preview without saving progress or advancing.
        /// </summary>
        public bool TesterJumpTo(int roundNumber, int rescueIndex)
        {
            if (!TesterMode.Available)
            {
                Debug.LogWarning("[SavePeps] Tester jump ignored outside a Development Build.");
                return false;
            }

            var round = _catalog?.Round(roundNumber);
            var rescue = round?.RescueAt(rescueIndex);
            if (rescue == null)
            {
                Debug.LogWarning($"[SavePeps] Tester jump target {roundNumber}.{rescueIndex + 1} does not exist.");
                return false;
            }

            StopAllCoroutines();
            _pendingRound = 0;
            _testerPreviewActive = true;
            _testerPlayThrough = false;
            _roundNumber = roundNumber;
            _rescueIndex = rescueIndex;
            _menu?.Hide();
            _card?.Hide();
            _pause?.Hide();
            _progress?.Hide();
            _unlock?.Hide();
            _hud?.SetVisible(true);
            RefreshDots();
            _runner?.Load(rescue, lockInputDuringEntrance: true);
            Debug.Log($"[SavePeps] Tester staged preview for round {roundNumber}, rescue {rescueIndex + 1} " +
                      $"('{rescue.Id}'). Profile unchanged.");
            return true;
        }

        /// <summary>
        /// The Tester Mode Play path: starts at the selected rescue, advances
        /// through the rest of that round, and records normal progress on solve.
        /// </summary>
        public bool TesterPlay(int roundNumber, int rescueIndex)
        {
            if (_testerMode is not { Active: true })
            {
                Debug.LogWarning("[SavePeps] Tester Play ignored while User Mode is active.");
                return false;
            }

            if (!TesterMode.Available)
            {
                Debug.LogWarning("[SavePeps] Tester play ignored outside a Development Build.");
                return false;
            }

            var round = _catalog?.Round(roundNumber);
            var rescue = round?.RescueAt(rescueIndex);
            if (rescue == null)
            {
                Debug.LogWarning($"[SavePeps] Tester target {roundNumber}.{rescueIndex + 1} does not exist.");
                return false;
            }

            StopAllCoroutines();
            _pendingRound = 0;
            _testerPreviewActive = false;
            _testerPlayThrough = true;
            _roundNumber = roundNumber;
            _rescueIndex = rescueIndex;
            _save.LastPlayedRound = roundNumber;
            Persist();
            _menu?.Hide();
            _card?.Hide();
            _pause?.Hide();
            _progress?.Hide();
            _unlock?.Hide();
            _hud?.SetVisible(true);
            RefreshDots();
            _runner?.Load(rescue, lockInputDuringEntrance: true);
            Debug.Log($"[SavePeps] Tester playing round {roundNumber}, rescue {rescueIndex + 1} " +
                      $"('{rescue.Id}').");
            return true;
        }

        public void TesterRestartCurrent()
        {
            if (!TesterMode.Available || _runner?.Current == null) return;
            StopAllCoroutines();
            _testerPreviewActive = true;
            _runner.Restart();
        }

        public void TesterApplyProfile(TesterProfilePreset preset)
        {
            if (!TesterMode.Available) return;

            StopAllCoroutines();
            _pendingRound = 0;
            _roundNumber = 0;
            _rescueIndex = 0;
            _testerPreviewActive = false;
            _testerPlayThrough = false;
            SaveStore.Delete();
            _save = TesterProfiles.Create(_catalog, preset);
            SaveStore.Save(_save);
            ApplySettings();
            ShowHome();
            Debug.Log($"[SavePeps] Tester profile applied: {preset}. Entitlement unchanged.");
        }

        public void TesterUnlockAllRounds()
        {
            if (!TesterMode.Available) return;
            TesterProfiles.UnlockAll(_catalog, _save);
            Persist();
            Debug.Log($"[SavePeps] Tester unlocked progression through round {_catalog?.RoundCount ?? 0}. " +
                      "Completion marks and entitlement unchanged.");
        }

        /// <summary>Returns navigation and access to the ordinary player paths.</summary>
        public void EndTesterSession()
        {
            if (!TesterMode.Available) return;
            StopAllCoroutines();
            _testerPreviewActive = false;
            _testerPlayThrough = false;
            _runner?.SuspendInput(false);
            ShowHome();
            Debug.Log("[SavePeps] Tester inspection ended. Normal gating restored.");
        }

        /// <summary>Returns to the title while preserving Tester Mode and its selected Play target.</summary>
        public void TesterReturnToTitle()
        {
            if (_testerMode is not { Active: true }) return;
            StopAllCoroutines();
            _testerPreviewActive = false;
            _testerPlayThrough = false;
            _runner?.SuspendInput(false);
            ShowHome();
            Debug.Log("[SavePeps] Tester returned to title. Selected Play target preserved.");
        }

        private void StartRescue()
        {
            var round = _catalog.Round(_roundNumber);
            var rescue = round?.RescueAt(_rescueIndex);
            if (rescue == null)
            {
                Debug.LogError($"[SavePeps] Round {_roundNumber} has no rescue at index {_rescueIndex}.");
                return;
            }

            _hud?.SetVisible(true);
            RefreshDots();
            _runner?.SuspendInput(false);
            _runner.Load(rescue, lockInputDuringEntrance: true);
        }

        private void HandleSolved(bool firstTap)
        {
            if (_testerPreviewActive)
            {
                var mode = _testerPlayThrough ? "play" : "preview";
                Debug.Log($"[SavePeps] Tester {mode} completed '{_runner?.Current?.Id}'. Profile unchanged.");
                if (_testerPlayThrough) StartCoroutine(AdvanceAfterDwell());
                return;
            }

            var rescue = _runner.Current;
            if (rescue != null) _save.RecordSolved(rescue.Id, firstTap);

            // Persist per rescue, not per round: the reunion is the moment the
            // player feels they earned something, and it should survive a kill
            // three seconds later.
            Persist();
            RefreshDots();

            StartCoroutine(AdvanceAfterDwell());
        }

        private IEnumerator AdvanceAfterDwell()
        {
            yield return new WaitForSeconds(_winDwell);

            _rescueIndex++;
            if (_rescueIndex < RoundDefinition.RescuesPerRound &&
                _catalog.Round(_roundNumber)?.RescueAt(_rescueIndex) != null)
            {
                StartRescue();
            }
            else
            {
                CompleteRound();
            }
        }

        private void CompleteRound()
        {
            if (!_testerPreviewActive)
            {
                // A full-game owner may jump straight to any authored round.
                // Finishing that round must not silently skip the free
                // player's sequential progression if entitlement later lapses.
                if (_roundNumber <= _save.HighestUnlockedRound)
                {
                    _save.UnlockThrough(_roundNumber + 1);
                }
                Persist();
            }
            else
            {
                Debug.Log($"[SavePeps] Tester completed round {_roundNumber}. Profile unchanged.");
            }

            // The diorama deliberately stays: the card washes over the scene
            // the player just solved, and that reunion is most of the reward.
            // Tearing it down here left the card floating on an empty
            // background, which read as a broken screen on device. The next
            // rescue's Load() clears it.
            _hud?.SetVisible(false);

            ShowRoundCompleteCard();
        }

        private void ShowRoundCompleteCard()
        {
            _menu?.Hide();
            _hud?.SetVisible(false);
            var round = _catalog.Round(_roundNumber);
            var marks = new Mark[RoundDefinition.RescuesPerRound];
            for (var i = 0; i < marks.Length; i++)
            {
                marks[i] = _save.MarkFor(round?.RescueAt(i)?.Id);
            }

            if (_card != null) _card.Show(_roundNumber, marks, KeepPlaying, ShowRoundPickerFromResult);
            else KeepPlaying();
        }

        /// <summary>Chooses another useful available round after the result beat.</summary>
        public void KeepPlaying() => PlayRecommendedRound();

        /// <summary>Compatibility alias for editor scripts from the linear flow.</summary>
        public void Continue() => KeepPlaying();

        /// <summary>Direct replay hook retained for editor tooling and QA.</summary>
        public void ReplayRound() => PlayRound(_roundNumber);

        /// <summary>
        /// Back to round 1, for a player who has finished everything authored.
        ///
        /// Deliberately not a progress wipe: earned stars stay earned. Round 1
        /// is always playable — it is unlocked from a fresh save and can never
        /// be behind the paywall — so this can never itself dead-end.
        /// </summary>
        public void PlayFromStart() => PlayRound(1);

        // -------------------------------------------------------------------
        // The shell: pause, progress, settings, and Android Back
        // -------------------------------------------------------------------

        /// <summary>True while any non-gameplay surface owns the screen.</summary>
        private bool ShellVisible =>
            (_menu != null && (_menu.HomeVisible || _menu.PickerVisible)) ||
            (_card != null && _card.Visible) ||
            (_pause != null && _pause.Visible) ||
            (_progress != null && _progress.Visible) ||
            (_unlock != null && _unlock.Visible) ||
            (_testerMode != null && _testerMode.Visible);

        private void Update()
        {
            // The pause control is live exactly when a rescue is waiting for a
            // tap. Driving it from here rather than from every state change
            // means no path can leave the button lit over a running gag.
            _hud?.SetMenuAvailable(_runner != null && _runner.AwaitingChoice && !ShellVisible);

            // Android's Back button arrives as Escape through the legacy input
            // module, which is what this project is configured for.
            if (Input.GetKeyDown(KeyCode.Escape)) HandleBack();
        }

        /// <summary>The HUD's pause control, and the Back button during a rescue.</summary>
        public void OpenPause()
        {
            if (_pause == null || ShellVisible) return;
            if (_runner == null || !_runner.AwaitingChoice) return;

            // Held for the whole visit, including a detour through Progress,
            // and handed back only by an explicit resume.
            _runner.SuspendInput(true);
            Debug.Log("[SavePeps] Pause opened.");
            ShowPauseSheet();
        }

        private void ShowPauseSheet() =>
            _pause?.Show(_save, _testerMode is { Active: true }, ResumeFromShell, ShowProgressFromPause, ShowRoundPickerFromPause,
                ShowHome, ApplyAndPersistSettings, OpenTesterToolsFromPause);

        private void OpenTesterToolsFromPause()
        {
            _testerMode?.Open();
        }

        private void ShowRoundPickerFromPause() =>
            ShowRoundPicker(showHomeDiorama: false, back: ResumeFromShell);

        /// <summary>Back into the rescue that was already on stage.</summary>
        private void ResumeFromShell()
        {
            _hud?.SetVisible(true);
            _runner?.SuspendInput(false);
            Debug.Log("[SavePeps] Resumed.");
        }

        public void ShowProgressFromHome()
        {
            _progressFromPause = false;
            _progress?.Show(_catalog, _save, FullGameUnlocked, CloseProgress);
        }

        private void ShowProgressFromPause()
        {
            _progressFromPause = true;
            _progress?.Show(_catalog, _save, FullGameUnlocked, CloseProgress);
        }

        private void CloseProgress()
        {
            if (_progressFromPause) ShowPauseSheet();
            else ShowHome();
        }

        /// <summary>
        /// One Back key, resolved outermost surface first. The rule the player
        /// feels is simply "Back undoes the last thing that opened", and the
        /// only place it leaves the game is the title screen — Android's own
        /// convention, and the one place where nothing is in progress.
        /// </summary>
        public void HandleBack()
        {
            if (_unlock != null && _unlock.Visible) { _unlock.RequestClose(); return; }
            if (_testerMode != null && _testerMode.Visible) { _testerMode.RequestClose(); return; }
            if (_progress != null && _progress.Visible) { _progress.RequestClose(); return; }
            if (_pause != null && _pause.Visible) { _pause.RequestClose(); return; }
            if (_menu != null && _menu.PickerVisible) { _menu.RequestBack(); return; }
            if (_card != null && _card.Visible) { ShowHome(); return; }
            if (_menu != null && _menu.HomeVisible)
            {
                Persist();
                Application.Quit();
                return;
            }

            OpenPause();
        }

        private void ApplySettings()
        {
            if (_feedback == null || _save == null) return;
            _feedback.SoundEnabled = !_save.SoundMuted;
            _feedback.HapticsAllowed = !_save.HapticsOff;
        }

        private void ApplyAndPersistSettings()
        {
            ApplySettings();
            Persist();
        }

        /// <summary>
        /// The one line of self-description on the title screen. It is hidden
        /// entirely until there is something to report, so a first launch
        /// still opens on two choices and a couple.
        /// </summary>
        private string HomeStatLine()
        {
            if (_save == null || _catalog == null || _save.TotalRescuesSolved == 0) return null;

            var stars = 0;
            for (var number = 1; number <= _catalog.RoundCount; number++)
            {
                stars += RoundProgress.Read(_catalog.Round(number), _save).Stars;
            }

            return stars > 0
                ? $"{_save.TotalRescuesSolved} SAVED   ·   {stars} FIRST TRY"
                : $"{_save.TotalRescuesSolved} SAVED";
        }

        /// <summary>
        /// A purchase or restore finished. If the player was stopped at the
        /// paywall, take them straight into the round they were reaching for —
        /// making someone tap through to the thing they just paid for is a
        /// small insult at the worst possible moment.
        /// </summary>
        private void HandleEntitlementChanged()
        {
            if (_pendingRound > 0 && FullGameUnlocked)
            {
                var purchasedRound = _pendingRound;
                _pendingRound = 0;
                PlayRound(purchasedRound);
                return;
            }

            if (_menu != null && _menu.PickerVisible)
            {
                _menu.ShowPicker(_catalog, _save, FullGameUnlocked, _pickerShowsHomeDiorama,
                    _testerMode is { Active: true }, SelectRound, _pickerBack ?? ShowHome);
            }
        }

        private void CancelPendingUnlock() => _pendingRound = 0;

        private void RefreshDots()
        {
            if (_hud == null) return;

            _hud.SetRound(_roundNumber, _rescueIndex, RoundDefinition.RescuesPerRound);

            var round = _catalog.Round(_roundNumber);
            var states = new DotState[RoundDefinition.RescuesPerRound];
            for (var i = 0; i < states.Length; i++)
            {
                var mark = _save.MarkFor(round?.RescueAt(i)?.Id);
                states[i] = mark switch
                {
                    Mark.Star => DotState.Star,
                    Mark.Check => DotState.Check,
                    _ => i == _rescueIndex ? DotState.Current : DotState.Upcoming,
                };
            }

            _hud.SetDots(states);
        }
    }
}
