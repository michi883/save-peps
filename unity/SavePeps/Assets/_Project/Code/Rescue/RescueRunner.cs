using System;
using System.Collections;
using System.Collections.Generic;
using SavePeps.Core;
using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// Runs one rescue: builds the diorama, wires the taps, plays outcomes,
    /// and owns the win / fail / retry loop.
    ///
    /// The loop is deliberately tiny, because the whole game is one tap. The
    /// only genuinely delicate part is retry, which must put the scene back
    /// exactly — see <see cref="AnimTarget"/> for why that is cheap here.
    ///
    /// The runner knows nothing about rounds, progression or the catalogue.
    /// It is handed one rescue at a time by <c>GameFlow</c> and reports the
    /// result; keeping it ignorant of what comes next is what lets the same
    /// component serve the game, the editor preview, and the Gauntlet.
    /// </summary>
    public sealed class RescueRunner : MonoBehaviour
    {
        [Tooltip("Optional. Played at boot when nothing else drives this runner — the P1 slice, the editor preview.")]
        [SerializeField] private RescueDefinition _rescue;

        [Tooltip("Off when a GameFlow owns the sequencing.")]
        [SerializeField] private bool _autoPlayOnStart = true;

        [SerializeField] private TapRouter _tapRouter;
        [SerializeField] private ChoreographyPlayer _player;
        [SerializeField] private RescueHud _hud;
        [SerializeField] private Feedback _feedback;
        [SerializeField] private GameFeel _gameFeel;

        private readonly Dictionary<string, AnimTarget> _targets = new();
        private readonly Dictionary<string, ChoicePresentation> _choices = new();
        private readonly Dictionary<string, ChoicePad> _choicePads = new();
        private GameObject _diorama;
        private Pep _pepA, _pepB;
        private Transform _meetAnchor;
        private RescueObject _tapped;
        private int _attempts;
        private bool _solved;

        /// <summary>Raised when the Peps are reunited. True if solved first tap.</summary>
        public event Action<bool> OnSolved;

        /// <summary>The rescue currently staged, or null between rescues.</summary>
        public RescueDefinition Current => _rescue;

        private void Awake()
        {
            if (_player != null) _player.OnEvent += HandleEvent;
            if (_tapRouter != null) _tapRouter.OnTap += HandleTap;
        }

        private void Start()
        {
            if (!_autoPlayOnStart) return;

            if (_rescue == null)
            {
                Debug.LogError("[SavePeps] RescueRunner is set to auto-play but has no rescue assigned.");
                return;
            }

            Load(_rescue);
        }

        private void OnDestroy()
        {
            if (_player != null) _player.OnEvent -= HandleEvent;
            if (_tapRouter != null) _tapRouter.OnTap -= HandleTap;
        }

        // -------------------------------------------------------------------
        // Staging
        // -------------------------------------------------------------------

        /// <summary>
        /// Stages a rescue, replacing whatever was there. Safe to call
        /// repeatedly — this is the seam the round loop drives.
        /// </summary>
        public void Load(RescueDefinition rescue) => Load(rescue, lockInputDuringEntrance: false);

        /// <summary>
        /// Flow-driven loads lock the entrance so the UI pointer-up that chose
        /// a round cannot fall through and choose a rescue object in the same
        /// frame. Editor outcome preview keeps the immediate overload above.
        /// </summary>
        public void Load(RescueDefinition rescue, bool lockInputDuringEntrance)
        {
            if (rescue == null)
            {
                Debug.LogError("[SavePeps] RescueRunner.Load was given no rescue.");
                return;
            }

            StopAllCoroutines();
            if (_player != null) _player.Stop();
            if (_tapRouter != null) _tapRouter.InputEnabled = false;

            if (_diorama != null)
            {
                // Current changes immediately for progression/tests, while
                // the visual stage gets a short toy-box swap of its own.
                _rescue = rescue;
                StartCoroutine(SwapDiorama(rescue));
                return;
            }

            ClearStage(stopCoroutines: false);
            _rescue = rescue;
            Build();
            // On the very first scene, keeping input live also keeps editor
            // Preview Outcome immediate. A human cannot beat this 0.42s drop
            // with a deliberate tap; automated authoring tools can.
            StartCoroutine(EnterDiorama(_diorama, lockInput: lockInputDuringEntrance));
        }

        /// <summary>
        /// Clears the staged rescue. Every per-rescue field is reset here
        /// rather than in Build, so that a half-built rescue (a missing
        /// diorama, a bad anchor) cannot leave a previous one's Peps or
        /// targets behind to be animated by the next tap.
        /// </summary>
        public void Teardown() => ClearStage(stopCoroutines: true);

        private void ClearStage(bool stopCoroutines)
        {
            if (stopCoroutines) StopAllCoroutines();
            if (_player != null) _player.Stop();

            if (_diorama != null) Destroy(_diorama);
            _diorama = null;

            _targets.Clear();
            _choices.Clear();
            _choicePads.Clear();
            _rescue = null;
            _pepA = null;
            _pepB = null;
            _meetAnchor = null;
            _tapped = null;
            _attempts = 0;
            _solved = false;

            if (_tapRouter != null) _tapRouter.InputEnabled = false;
            _gameFeel?.ResetPresentation();
        }

        private IEnumerator SwapDiorama(RescueDefinition next)
        {
            var outgoing = _diorama;
            if (outgoing != null)
            {
                var startPosition = outgoing.transform.localPosition;
                var startRotation = outgoing.transform.localRotation;
                var startScale = outgoing.transform.localScale;
                var elapsed = 0f;
                const float outDuration = 0.26f;
                while (elapsed < outDuration && outgoing != null)
                {
                    elapsed += Time.deltaTime;
                    var t = Easing.Evaluate(EaseKind.In, Mathf.Clamp01(elapsed / outDuration));
                    outgoing.transform.localPosition = startPosition + new Vector3(-0.72f * t, -0.20f * t, 0f);
                    outgoing.transform.localRotation = startRotation * Quaternion.Euler(0f, 0f, 8f * t);
                    outgoing.transform.localScale = startScale * Mathf.Lerp(1f, 0.94f, t);
                    yield return null;
                }
            }

            ClearStage(stopCoroutines: false);
            _rescue = next;
            Build();
            yield return EnterDiorama(_diorama, lockInput: true);
        }

        private IEnumerator EnterDiorama(GameObject staged, bool lockInput)
        {
            if (staged == null) yield break;

            if (lockInput && _tapRouter != null) _tapRouter.InputEnabled = false;
            staged.transform.localPosition = new Vector3(0.58f, 0.56f, 0f);
            staged.transform.localRotation = Quaternion.Euler(0f, 0f, -7f);
            staged.transform.localScale = Vector3.one * 0.93f;

            var elapsed = 0f;
            const float duration = 0.42f;
            while (elapsed < duration && staged != null)
            {
                elapsed += Time.deltaTime;
                var t = Easing.Evaluate(EaseKind.Back, Mathf.Clamp01(elapsed / duration));
                staged.transform.localPosition = Vector3.LerpUnclamped(new Vector3(0.58f, 0.56f, 0f), Vector3.zero, t);
                staged.transform.localRotation = Quaternion.SlerpUnclamped(
                    Quaternion.Euler(0f, 0f, -7f), Quaternion.identity, t);
                staged.transform.localScale = Vector3.one * Mathf.LerpUnclamped(0.93f, 1f, t);
                yield return null;
            }

            if (staged != null)
            {
                staged.transform.localPosition = Vector3.zero;
                staged.transform.localRotation = Quaternion.identity;
                staged.transform.localScale = Vector3.one;
            }

            // A preview/test can choose during the entrance via SimulateTap.
            // Never let the animation's completion reopen input over an
            // outcome that has already locked it.
            if (_tapRouter != null && _diorama == staged && _tapped == null && !_solved)
            {
                _tapRouter.InputEnabled = true;
            }
        }

        private void Build()
        {
            _diorama = Instantiate(_rescue.Environment, transform);
            _diorama.name = "Diorama";

            var anchors = new Dictionary<string, Transform>();
            foreach (var t in _diorama.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                anchors[t.name] = t;
            }

            _meetAnchor = Look(anchors, _rescue.MeetAnchor);
            _pepA = SpawnPep(anchors, _rescue.PepAAnchor, _rescue.PepAPrefab, "PepA");
            _pepB = SpawnPep(anchors, _rescue.PepBAnchor, _rescue.PepBPrefab, "PepB");
            if (_pepA != null && _pepB != null)
            {
                _pepA.SetPartner(_pepB.transform);
                _pepB.SetPartner(_pepA.transform);
            }

            foreach (var pad in _diorama.GetComponentsInChildren<ChoicePad>(includeInactive: true))
            {
                if (pad != null && !string.IsNullOrEmpty(pad.AnchorId)) _choicePads[pad.AnchorId] = pad;
            }

            foreach (var obj in _rescue.Objects)
            {
                if (obj?.Prop == null) continue;
                var anchor = Look(anchors, obj.AnchorId);
                if (anchor == null) continue;

                var prop = Instantiate(obj.Prop, anchor);
                prop.name = obj.Id;
                prop.transform.localPosition = Vector3.zero;
                prop.transform.localRotation = Quaternion.identity;

                var tappable = prop.GetComponentInChildren<Tappable>();
                if (tappable != null) tappable.ObjectId = obj.Id;

                var target = prop.GetComponentInChildren<AnimTarget>();
                if (target != null) _targets[obj.Id] = target;

                var presentation = prop.GetComponent<ChoicePresentation>();
                if (presentation != null) _choices[obj.Id] = presentation;
            }

            // Named fx and scenery the choreography can move.
            foreach (var target in _diorama.GetComponentsInChildren<AnimTarget>(includeInactive: true))
            {
                _targets.TryAdd(target.transform.parent != null ? target.transform.parent.name : target.name, target);
            }

            // GameFlow hides the HUD while the home/picker shell is up. The
            // runner is also driven directly by editor Preview Outcome, so
            // staging—not the flow—is the reliable place to reveal it again.
            _hud?.SetVisible(true);
            _hud?.Show(_rescue.Goal);
            ResetScene();
        }

        private Pep SpawnPep(IReadOnlyDictionary<string, Transform> anchors, string anchorId, GameObject prefab, string label)
        {
            var anchor = Look(anchors, anchorId);
            if (anchor == null || prefab == null) return null;

            var pep = Instantiate(prefab, anchor).GetComponent<Pep>();
            if (pep == null) return null;
            pep.name = label;
            pep.transform.localPosition = Vector3.zero;
            return pep;
        }

        private static Transform Look(IReadOnlyDictionary<string, Transform> anchors, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (anchors.TryGetValue(id, out var t)) return t;
            Debug.LogWarning($"[SavePeps] Anchor '{id}' is not in this diorama.");
            return null;
        }

        // -------------------------------------------------------------------
        // The one tap
        // -------------------------------------------------------------------

        private void HandleTap(string objectId)
        {
            if (_rescue == null || _solved) return;

            var obj = Array.Find(_rescue.Objects, o => o != null && o.Id == objectId);
            if (obj == null) return;

            _tapped = obj;
            _attempts++;
            _tapRouter.InputEnabled = false;

            foreach (var (id, presentation) in _choices)
            {
                presentation?.SetSelection(id == obj.Id, locked: true);
            }

            foreach (var (anchorId, pad) in _choicePads)
            {
                pad?.SetSelection(anchorId == obj.AnchorId, locked: true);
            }

            _feedback?.Tap();
            if (_targets.TryGetValue(obj.Id, out var selectedTarget) && selectedTarget != null)
            {
                _gameFeel?.Tap(selectedTarget.transform.position);
            }
            _hud?.ClearQuip();

            _player.Play(obj.Steps, Resolve);
            StartCoroutine(FinishAfter(obj));
        }

        private IEnumerator FinishAfter(RescueObject obj)
        {
            yield return new WaitForSeconds(obj.Duration);

            if (_rescue.IsCorrect(obj)) Win();
            else Fail(obj);
        }

        private void Win()
        {
            _solved = true;
            var firstTap = _attempts == 1;
            // Reunion is the message. The earned HUD mark adds one small
            // punctuation sound after the physical celebration instead of
            // covering the Peps with congratulatory copy.
            _feedback?.Play(firstTap ? "star" : "check");
            if (firstTap) _feedback?.Haptic("light");
            OnSolved?.Invoke(firstTap);
        }

        private void Fail(RescueObject obj)
        {
            _feedback?.Wrong();
            _pepA?.ReactToWrong(coverEyes: _attempts % 2 == 1);
            _pepB?.ReactToWrong(coverEyes: _attempts % 2 == 0);
            Transform actionTarget = null;
            if (_targets.TryGetValue(obj.Id, out var selectedTarget) && selectedTarget != null)
            {
                actionTarget = selectedTarget.transform;
                _gameFeel?.Wrong(selectedTarget.transform.position);
            }
            _hud?.ShowQuip(obj.Quip, actionTarget);
            Debug.Log($"[SavePeps] Wrong '{obj.Id}'. Quip shown; retry will reset automatically.");
            StartCoroutine(RetryAfterFailureBeat());
        }

        private IEnumerator RetryAfterFailureBeat()
        {
            // Unscaled time keeps the joke readable in editor speed-up tests
            // and through any future slow-motion choreography.
            yield return new WaitForSecondsRealtime(1.10f);
            _hud?.HideQuip();
            yield return new WaitForSecondsRealtime(0.16f);
            Debug.Log("[SavePeps] Retry ready.");
            ResetScene();
        }

        /// <summary>
        /// Puts the scene back exactly as it started. The brief's test —
        /// wrong, retry, wrong, retry, correct — has to behave identically
        /// every time, so this resets state rather than reloading the scene.
        /// </summary>
        public void Retry()
        {
            ResetScene();
            _hud?.ClearQuip();
        }

        private void ResetScene()
        {
            StopAllCoroutines();
            _player.Stop();

            foreach (var target in _targets.Values)
            {
                if (target != null) target.ResetToRest();
            }

            _pepA?.ResetToRest();
            _pepB?.ResetToRest();
            foreach (var choice in _choices.Values) choice?.ResetPresentation();
            foreach (var pad in _choicePads.Values) pad?.ResetPresentation();
            _gameFeel?.ResetPresentation();

            _tapped = null;
            _tapRouter.InputEnabled = true;
        }

        // -------------------------------------------------------------------
        // Choreography plumbing
        // -------------------------------------------------------------------

        private AnimTarget Resolve(string reference)
        {
            if (string.IsNullOrEmpty(reference)) return null;

            return reference switch
            {
                SceneRef.Self => _tapped != null && _targets.TryGetValue(_tapped.Id, out var self) ? self : null,
                SceneRef.PepA => _pepA?.Target,
                SceneRef.PepB => _pepB?.Target,
                _ => _targets.GetValueOrDefault(reference),
            };
        }

        private void HandleEvent(OutcomeStep step)
        {
            switch (step.Kind)
            {
                case StepKind.Face:
                    if (Enum.TryParse<PepFace>(step.Param, ignoreCase: true, out var face))
                    {
                        var who = step.Target switch
                        {
                            SceneRef.PepA => _pepA,
                            SceneRef.PepB => _pepB,
                            _ => null,
                        };
                        if (who != null) who.SetFace(face);
                        else { _pepA?.SetFace(face); _pepB?.SetFace(face); }
                    }
                    break;

                case StepKind.Sfx:
                    _feedback?.Play(step.Param);
                    if (step.Param is "thud" or "bonk" or "click" or "snip")
                    {
                        _gameFeel?.Impact(step.Param == "thud" ? 1f : 0.55f);
                    }
                    break;

                case StepKind.Haptic:
                    _feedback?.Haptic(step.Param);
                    break;

                case StepKind.Meet:
                    StartCoroutine(PlayReunion(step.Duration));
                    break;
            }
        }

        /// <summary>
        /// The emotional payoff, and the most-watched animation in the game.
        /// Both Peps drop their idle, run to the meet anchor, and land on
        /// happy faces.
        /// </summary>
        private IEnumerator PlayReunion(float duration)
        {
            if (_pepA == null || _pepB == null || _meetAnchor == null) yield break;

            _pepA.BeginRun();
            _pepB.BeginRun();

            // Take the Peps off the choreography player and drive their
            // animated transforms directly, starting from wherever the
            // outcome left them — a Pep that just hopped across a plank must
            // not snap back to its anchor to begin the reunion.
            var aTarget = _pepA.Target.transform;
            var bTarget = _pepB.Target.transform;
            _player.Release(_pepA.Target);
            _player.Release(_pepB.Target);

            var aStart = aTarget.position;
            var bStart = bTarget.position;
            var meet = _meetAnchor.position;

            // Stop a body-width short of each other rather than intersecting.
            // 0.22 was too close on device — they read as one blob at the
            // moment the reunion is supposed to land.
            var offset = (aStart - bStart).normalized * 0.34f;
            var aEnd = meet + offset;
            var bEnd = meet - offset;

            var runDuration = Mathf.Max(0.35f, duration);
            var elapsed = 0f;
            while (elapsed < runDuration)
            {
                elapsed += Time.deltaTime;
                var linear = Mathf.Clamp01(elapsed / runDuration);
                var t = Easing.Evaluate(EaseKind.InOut, linear);
                // Three quick footfalls; the articulated run pose supplies
                // the counter-swing while this gives the whole toy weight.
                var skip = Mathf.Abs(Mathf.Sin(linear * Mathf.PI * 3f)) * 0.055f;
                aTarget.position = Vector3.Lerp(aStart, aEnd, t) + Vector3.up * skip;
                bTarget.position = Vector3.Lerp(bStart, bEnd, t) + Vector3.up * skip;
                yield return null;
            }

            aTarget.position = aEnd;
            bTarget.position = bEnd;
            _pepA.BeginHug();
            _pepB.BeginHug();
            _gameFeel?.Reunion(meet);
            _feedback?.Haptic("success");

            // Squeeze together, then share one small spin. This beat is
            // deliberately longer than the run: solving the prop is the
            // setup, affection is the repeated reward.
            var hugVector = offset;
            elapsed = 0f;
            const float hugDuration = 0.42f;
            while (elapsed < hugDuration)
            {
                elapsed += Time.deltaTime;
                var t = Easing.Evaluate(EaseKind.Back, Mathf.Clamp01(elapsed / hugDuration));
                var close = Vector3.Lerp(hugVector, hugVector.normalized * 0.24f, t);
                var lift = Mathf.Sin(Mathf.Clamp01(elapsed / hugDuration) * Mathf.PI) * 0.045f;
                aTarget.position = meet + close + Vector3.up * lift;
                bTarget.position = meet - close + Vector3.up * lift;
                yield return null;
            }

            _pepA.BeginCelebrate();
            _pepB.BeginCelebrate();
            var closeVector = hugVector.normalized * 0.24f;
            elapsed = 0f;
            const float spinDuration = 0.56f;
            while (elapsed < spinDuration)
            {
                elapsed += Time.deltaTime;
                var t = Easing.Evaluate(EaseKind.InOut, Mathf.Clamp01(elapsed / spinDuration));
                var spun = Quaternion.AngleAxis(t * 58f, Vector3.up) * closeVector;
                var lift = Mathf.Sin(t * Mathf.PI) * 0.035f;
                aTarget.position = meet + spun + Vector3.up * lift;
                bTarget.position = meet - spun + Vector3.up * lift;
                yield return null;
            }
        }
    }
}
