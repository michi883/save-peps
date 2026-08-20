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
        [SerializeField] private Feedback _feedback;

        [Tooltip("Anything implementing IEntitlementService: the fake in the editor, RevenueCat on device.")]
        [SerializeField] private MonoBehaviour _entitlementSource;

        [Header("Pacing")]
        [Tooltip("Seconds after the authored outcome before the next rescue drops in. The reunion itself is already inside that outcome.")]
        [SerializeField, Range(0.5f, 5f)] private float _winDwell = 1.35f;

        private IEntitlementService _entitlements;
        private SaveData _save;
        private int _roundNumber;
        private int _rescueIndex;
        private int _pendingRound;
        private Action _pickerBack;
        private bool _pickerShowsHomeDiorama;
        private bool _progressFromPause;

        /// <summary>Raised with the round the player just tried to reach.</summary>
        public event Action<int> OnPaywallRequested;

        /// <summary>Raised when the player finishes the last authored round.</summary>
        public event Action OnCatalogComplete;

        public SaveData Save => _save;
        public int CurrentRound => _roundNumber;
        public Catalog Catalog => _catalog;

        // -------------------------------------------------------------------
        // Boot
        // -------------------------------------------------------------------

        private void Awake()
        {
            _entitlements = _entitlementSource as IEntitlementService;
            if (_entitlementSource != null && _entitlements == null)
            {
                Debug.LogError(
                    $"[SavePeps] '{_entitlementSource.GetType().Name}' is wired as the entitlement source but does " +
                    "not implement IEntitlementService. Paid rounds will stay locked.");
            }

            _save = SaveStore.Load();
            _hud?.SetVisible(false);
            _card?.Hide();
            _menu?.Hide();
            _pause?.Hide();
            _progress?.Hide();
            ApplySettings();
        }

        private void OnEnable()
        {
            if (_runner != null) _runner.OnSolved += HandleSolved;
            if (_hud != null) _hud.OnMenuRequested += OpenPause;
            if (_entitlements != null) _entitlements.Changed += HandleEntitlementChanged;
        }

        private void OnDisable()
        {
            if (_runner != null) _runner.OnSolved -= HandleSolved;
            if (_hud != null) _hud.OnMenuRequested -= OpenPause;
            if (_entitlements != null) _entitlements.Changed -= HandleEntitlementChanged;
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
            Access.CanPlay(_catalog, round, _save.HighestUnlockedRound, IsSubscribed);

        public RoundAccess AccessFor(int round) =>
            Access.State(_catalog, round, _save.HighestUnlockedRound, IsSubscribed);

        private bool IsSubscribed => _entitlements is { IsSubscribed: true };

        // -------------------------------------------------------------------
        // Rounds
        // -------------------------------------------------------------------

        public void ShowHome()
        {
            _hud?.SetVisible(false);
            _card?.Hide();
            _pause?.Hide();
            _progress?.Hide();
            _runner?.Teardown();
            _menu?.ShowHome(PlayRecommendedRound, ShowRoundPickerFromHome, ShowProgressFromHome, HomeStatLine());
        }

        /// <summary>The dominant Play action: useful randomness over available rounds.</summary>
        public void PlayRecommendedRound()
        {
            var number = RoundSelector.Choose(_catalog, _save, IsSubscribed, UnityEngine.Random.value);
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
            _hud?.SetVisible(false);
            _menu?.ShowPicker(_catalog, _save, IsSubscribed, showHomeDiorama, SelectRound, back);
            Debug.Log($"[SavePeps] Round picker opened with {_catalog.RoundCount} rounds.");
        }

        private void SelectRound(int number)
        {
            Debug.Log($"[SavePeps] Round {number} selected from picker.");
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

            if (!CanPlay(number))
            {
                // Locked rather than unpurchased is a bug, not a sales moment:
                // only surface the paywall when the subscription is genuinely
                // what stands between the player and the round.
                if (Access.IsPaywalled(_catalog, number, _save.HighestUnlockedRound, IsSubscribed))
                {
                    _pendingRound = number;
                    OnPaywallRequested?.Invoke(number);
                }
                else
                {
                    Debug.LogWarning($"[SavePeps] Round {number} is not unlocked yet.");
                }
                return;
            }

            _pendingRound = 0;
            _roundNumber = number;
            _rescueIndex = 0;
            _save.LastPlayedRound = number;
            Persist();
            _menu?.Hide();
            _card?.Hide();
            Debug.Log($"[SavePeps] Round {number} started.");
            StartRescue();
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
            _runner.Load(rescue, lockInputDuringEntrance: true);
        }

        private void HandleSolved(bool firstTap)
        {
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
            // A subscriber may jump straight to any authored round. Finishing
            // that round must not silently skip the free player's sequential
            // progression if the entitlement later lapses.
            if (_roundNumber <= _save.HighestUnlockedRound)
            {
                _save.UnlockThrough(_roundNumber + 1);
            }
            Persist();

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
            (_progress != null && _progress.Visible);

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
            _pause?.Show(_save, ResumeFromShell, ShowProgressFromPause, ShowRoundPickerFromPause,
                ShowHome, ApplyAndPersistSettings);

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
            _progress?.Show(_catalog, _save, IsSubscribed, CloseProgress);
        }

        private void ShowProgressFromPause()
        {
            _progressFromPause = true;
            _progress?.Show(_catalog, _save, IsSubscribed, CloseProgress);
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
            if (_pendingRound > 0 && IsSubscribed)
            {
                var purchasedRound = _pendingRound;
                _pendingRound = 0;
                PlayRound(purchasedRound);
                return;
            }

            if (_menu != null && _menu.PickerVisible)
            {
                _menu.ShowPicker(_catalog, _save, IsSubscribed, _pickerShowsHomeDiorama,
                    SelectRound, _pickerBack ?? ShowHome);
            }
        }

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
