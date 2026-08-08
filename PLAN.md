# Save Peps — Development Plan

A one-tap 3D puzzle game for Android. RevenueCat Shipaton 2026, primary category **Best Game**, secondary **RevenueCat Design Award**.

---

## 0. The situation, stated plainly

**Today: 7 Aug 2026. Devpost deadline: 30 Sep 2026, 23:45 PDT. That is 7.5 weeks.**

Three Shipaton rules shape everything below:

1. The app must be **brand new**, first released between 1 Aug and 30 Sep 2026. Save Pip cannot be updated into this; Save Peps must be a new listing. (Fine — that is what we are doing.)
2. The app must be **published on Google Play**. Not "buildable". Live.
3. It must ship **at least one RevenueCat-managed purchase**, and judges need either a free trial or a promo code.

Rule 2 is the schedule. **Confirmed: the Play account is verified but does not yet have production access**, so Google requires **12 testers opted into a closed test continuously for 14 days**, followed by a review of the production-access application. That clock cannot be compressed and cannot be started late. Counting back from a target of live-on-Play by 24 Sep, the closed test must be running by **16 Aug**.

**Therefore the first milestone is not a fun rescue. It is a signed AAB with the RevenueCat SDK linked, sitting in a closed testing track with 12 testers, by 16 Aug.** Everything else is built while that clock runs.

### Current toolchain state

| Thing | Status |
|---|---|
| `/Users/michi/save-peps` | Empty except `INSTRUCTIONS.md`. Not a git repo. |
| Unity / Unity Hub | **Not installed.** Day-0 task. |
| Android SDK | Present at `~/Library/Android/sdk` (Android Studio installed) — but use Unity's bundled SDK/NDK/JDK instead, it avoids a class of build failures. |
| System JDK | Broken (x86 openjdk on an arm Mac). Irrelevant if Unity's bundled OpenJDK is used. |
| Save Pip | `/Users/michi/save-pip`, TypeScript, 11.3k lines, 106 rescues. Source material, not a dependency. |

---

## 1. Decisions to make before coding

### Settled (7 Aug)

| # | Decision | Answer | Consequence |
|---|---|---|---|
| **D1** | Google Play account status | **Verified, no production access yet** | The 12-testers × 14-days rule applies. Closed testing must be live by **16 Aug**. This is the schedule's spine. |
| **D2** | Rescue count for launch | **36 rescues / 12 rounds** (10 free rounds = 30, 2 paid rounds = 6) | ~9 rescues/week during the content sprint. Floor 33, stretch 45. See §12. |
| **D4** | 3D art sourcing | **CC0 kits (Kenney low-poly) re-materialled onto one palette atlas, + custom-modelled Peps** | ~3–4 days on the characters; props and environments are adopted, not made. Art direction locked in P0. |
| **D6** | App identity | **`fan.sound.savepeps`** — reverse-domain namespace for `sound.fan`. Store name "Save Peps". | FINAL as of 7 Aug. Immutable once the first bundle reaches Play. |
| **D7** | Choreography runtime | **Hand-rolled, no tween library** (was: PrimeTween) | The additive-delta model is bespoke, so a tween lib would only have supplied easing and scheduling — ~60 lines. Dropping it removes a dependency from the Android build path (R4) and let us port Save Pip's exact cubic-bezier curves, so gag timing transfers rather than being re-approximated. |

### Still open

| # | Decision | Recommendation | Why it matters |
|---|---|---|---|
| **D5** | Subscription price | One entitlement `peps_unlimited`, one subscription with **two base plans: monthly and annual**, annual highlighted and carrying a **7-day free trial**. Suggest **$3.99/mo, $19.99/yr**. | Two durations of one tier is not "multiple tiers" — it is standard and converts better. The free trial also satisfies Devpost's "free trial or promo code for judges" requirement for free. Needed by week 2, when the Play product is created. |
| **D9** | Repo visibility | Private during the build; public at submission is a small credibility bonus. | Affects where Privacy/Terms are hosted (GitHub Pages is the free answer either way). |

### My call unless overruled

| # | Decision | Choice | Reasoning |
|---|---|---|---|
| **D3** | Where the free/paid gate sits | Keep at **round 11**, but make `FreeRoundCount` a config value so it can move to 8 or 9 in release week if content slips | The brief fixes it at 10. That means 30 polished rescues is a hard floor *before the paywall is even reachable*. Keeping it configurable is the cheap insurance. |
| **D8** | Play "target audience" declaration | **13+.** Do not declare a child-directed audience | A cute art style plus a "children" declaration pulls the app into the Designed for Families programme, which adds policy work we do not have weeks for. |

---

## 2. Recommended project structure

Single repo, Unity project nested so that docs and store assets live alongside it without polluting `Assets/`.

```
save-peps/
├── INSTRUCTIONS.md
├── PLAN.md
├── README.md
├── docs/                          # GitHub Pages → privacy + terms URLs Play requires
│   ├── privacy.md
│   └── terms.md
├── store/                         # listing copy, 1024² icon, screenshots, feature graphic
├── design/                        # rescue design sheets, palette, Peps turnaround
└── unity/SavePeps/
    ├── Assets/
    │   ├── _Project/
    │   │   ├── Art/
    │   │   │   ├── Characters/    # Peps meshes, rig, face atlas, clips
    │   │   │   ├── Props/         # the ~25 tappable objects
    │   │   │   ├── Environments/  # the 8 diorama prefabs
    │   │   │   └── Materials/     # ONE palette atlas material for almost everything
    │   │   ├── Audio/
    │   │   ├── Code/
    │   │   │   ├── Core/          # bootstrap, service locator, save, audio, haptics
    │   │   │   ├── Rescue/        # RescueDefinition, OutcomeStep, ChoreographyPlayer, RescueRunner, TapRouter
    │   │   │   ├── Progression/   # RoundDefinition, ProgressionService, gating
    │   │   │   ├── Monetization/  # IEntitlementService + RevenueCat/Fake impls, PaywallController
    │   │   │   ├── UI/
    │   │   │   └── Editor/        # validators, RescueAuthorWindow, preview tools
    │   │   ├── Content/
    │   │   │   ├── Rescues/       # 36 × RescueDefinition.asset
    │   │   │   ├── Rounds/        # 12 × RoundDefinition.asset
    │   │   │   └── Catalog.asset  # the ordered playlist
    │   │   └── Scenes/            # Boot, Game, RescuePreview
    │   └── Plugins/               # RevenueCat, EDM4U (vendor code, kept separate)
    ├── Packages/manifest.json
    └── ProjectSettings/
```

Conventions worth fixing on day one, because retrofitting them is expensive:

- **Assembly definitions per `Code/` folder.** Keeps compile times in single-digit seconds through the content sprint. `Editor` asmdef is editor-only so validators never ship.
- **Git + Git LFS** for `*.fbx *.png *.wav *.mp3`. `.gitignore` for `Library/ Temp/ Logs/ Build/ obj/`. Force Text serialization, Visible Meta Files.
- **One diorama = one prefab**, never a Unity scene. Prefab instantiation is faster than additive scene loading and far easier to preview in the editor.
- **Portrait only, locked.** Single aspect-safe framing rule (§6).

---

## 3. What to reuse from Save Pip (conceptually)

Save Pip's real asset is not its code, it is a **proven authoring discipline**. Port that; retype the code.

**Reuse directly as design law:**

1. **The house rules.** One solver per scene; the answer is physics, never trivia or memory; the predicament reads in ~3 seconds with zero text; wrong answers fail *funny*, never grim; the only text in the scene is a 2–4 word objective line naming the goal and never the solution.
2. **The declarative pattern spec.** `PatternSpec` (backdrop + scenery + character + three items with move-based outcomes) → `RescueDefinition` ScriptableObject. Same shape, same payoff: new content is data, not code.
3. **The choreography vocabulary.** `choreo.ts` factories — `fly, arc, hop, drop, shake, hopInPlace, pop, startle, show, hide, puff, spin, flyOff, sink, settle` — are a battle-tested grammar for comic physics. Port the names 1:1 into C# so the design language survives.
4. **Additive/delta transform semantics.** `choreo.ts` runs moves with `composite: 'add'`, so "Pip hops onto the leaf, *then* drifts with the leaf" is two independent moves. This is a subtle, important idea. Rebuild it (§5).
5. **The face stack.** `pipStack()` renders every expression and a class picks one. In Unity this becomes a face-atlas UV swap — cheap, expressive, no blend shapes, no facial rig.
6. **`catalog.test.ts` as a machine-checked style guide.** It enforces: 3 objects, exactly 1 correct, unique verbs, 2–4 word unique objective lines, a quip on every wrong object, every choreography selector resolving to a real element, all timings inside the outcome window (2.0–3.6 s). This is *the* mechanism that kept 106 rescues coherent. Port it (§15).
7. **The round rules** (`rounds.ts`): no two rescues from the same category, no repeated correct object, mixed difficulty. In Save Peps rounds are authored rather than rolled, so these become **editor-time validators** rather than a runtime sampler.
8. **Star / check marks.** First-tap save = ★, save after retries = ✓, three stars = a perfect round. Keep exactly this.
9. **Zero-text telegraphing.** Destination flags glow, sleepers show zzz, wind lines sweep, edge-perched props teeter, streams visibly pour. Translate each into a 3D equivalent.
10. **The quip voice.** Dry, short, affectionate. *"Lovely flower. Still not a ladder." / "Please do not fan the boulder."* Two sentences maximum, never scolding. This voice is a genuine asset — reuse it verbatim in tone.

**Reuse as source material, redesigned:**

11. **The 100-verb catalog** (README §"The 100-pattern catalog") is a ready-made content bible with difficulty ratings and object lineups already balanced. Select the ~36 that survive translation to 3D and to a *couple* rather than a single character (§12).
12. **The backdrop-reuse pattern.** `bgMeadow / bgSeaside / bgSnow / bgCanyon / bgCave / bgPond / bgNight / bgDesert / bgLava / bgStorm` is how 100 scenes got made without 100 scene builds. Direct precedent for 8 environment prefabs hosting 36 rescues.
13. **"The protean nine".** The plank saves in 7 patterns and fails in 6; the fan is the biggest red herring in the game. Objects must never mean one fixed thing — that is what makes the player *read* rather than memorise. Enforce it with a validator.

**Explicitly do not port:** seeds and shareable rounds, Reddit posting, the round timer and best-time stats, random round generation. All are Reddit-native or add HUD noise. Save Peps has authored progression and a paywall; determinism comes from the playlist, not a seed.

---

## 4. What must be rebuilt for Unity and 3D

| Area | Save Pip | Save Peps |
|---|---|---|
| Art | Inline hand-written SVG | Meshes + one palette atlas material |
| Cast | One character (Pip) | **A couple, separated.** New character design, and a new win beat: two actors converging and reuniting. |
| Win condition | Pip reaches safety | **The Peps reach each other.** Choreography needs a `Meet` step that hands both actors to a shared reunion animation. |
| Input | SVG `<circle class="hot">` tap targets | Physics raycast onto colliders, generous radii, plus an idle shimmer/rim highlight so tappables read instantly |
| Camera | Fixed 360×500 viewBox | Fixed per-rescue camera authored into each diorama prefab, with aspect-safe framing (§6) |
| Choreography | CSS keyframes (6 bespoke) + WAAPI `Move[]` (100 catalog) | **One system only** — the data timeline. No second hand-authored path. An escape hatch to Unity Timeline exists for 3–4 set-pieces, deliberately rationed. |
| Audio | WebAudio synth (`fx.ts`) | Designed short samples; a small mixer; music bed optional and quiet |
| Haptics | None | Light impact on tap, medium on wrong, a small double-pulse on reunion (Android `VibrationEffect` via `AndroidJavaObject`; `Handheld.Vibrate` is too blunt) |
| Persistence | `localStorage` | JSON file in `Application.persistentDataPath` (§9) |
| Monetization | None | RevenueCat + Play Billing (§10–11) |
| Progression | Endless random rounds | Authored, ordered, gated (§8) |

Also new and worth naming as work: **retry must fully reset the scene.** In the web version a wrong answer re-rendered the DOM from scratch. In Unity, transforms, particles, animator states, and enabled flags all persist. Every animated target needs an explicit `ResetToRest()`. This is the single most likely source of embarrassing bugs and is budgeted for in §15.

---

## 5. Proposed rescue architecture

The goal, restated: **adding a rescue means filling in a ScriptableObject and dragging in prefabs. It must not mean writing C#.**

### 5.1 Data model

```csharp
[CreateAssetMenu(menuName = "Peps/Rescue")]
public sealed class RescueDefinition : ScriptableObject
{
    public string   Id;             // "r07"
    public Category Category;       // Crossing, Water, Falling, Weather, Animals,
                                    // Machines, Light, Paths, Tricks, ThePeps
    public string   Verb;           // "bridge" — unique across the catalog
    public string   Goal;           // "Bring them together." — 2–4 words, unique
    public Difficulty Difficulty;   // Easy | Medium | Surprising
    [TextArea] public string SceneDescription;   // accessibility + design review

    public EnvironmentRef Environment;           // diorama prefab
    public Placement      PepA, PepB;            // anchor id + idle pose
    public Dressing[]     Dressing;              // extra props: prefab + anchor id
    public RescueObject[] Objects;               // exactly 3
    public int            CorrectIndex;          // exactly 1
}

[Serializable] public sealed class RescueObject
{
    public string     Id;            // "plank"
    public GameObject Prop;
    public string     AnchorId;      // "Slot_1" — a marker inside the diorama prefab
    public string     Label;         // screen-reader label
    [TextArea] public string Quip;   // wrong objects only
    public float      Duration;      // 2.0–3.6 s, validated
    public OutcomeStep[] Steps;
}

[Serializable] public struct OutcomeStep
{
    public float    At;          // seconds after the tap
    public float    Duration;
    public StepKind Kind;        // Move Arc Hop Drop Spin Shake Pop Startle
                                 // Show Hide Puff Sink FlyOff Settle
                                 // Face Sfx Haptic Meet CameraNudge
    public string   Target;      // "$self", "$pepA", "$pepB", or an anchor/fx name
    public Vector3  Delta;       // position delta in diorama-local space
    public Vector3  EulerDelta;
    public float    Scale;       // 1 = unchanged
    public float    Alpha;       // -1 = untouched
    public Ease     Ease;        // Out In InOut Linear Back Hop
    public string   Param;       // sfx id / face name / meet anchor
}
```

Three things carried over from Save Pip that are easy to lose and expensive to re-derive:

- **Everything is a delta from rest, applied additively.** A target keeps its rest TRS; concurrent and sequential steps accumulate into an offset. That is what makes "Pep hops onto the leaf, then rides the leaf downstream" two independent, composable steps instead of one hand-computed path.
- **Placement and choreography are separated.** Rest placement lives on an outer transform; choreography animates an inner child. Otherwise the animation clobbers the placement — exactly the bug the `scenekit.ts` header warns about.
- **Timings are absolute milliseconds from the tap.** Not a state machine, not a graph. A flat, sorted, readable list.

### 5.2 Runtime

- **`RescueRunner`** — instantiates the diorama prefab, resolves anchors, places the Peps, the 3 objects and dressing, wires tap targets, shows the goal line, and owns the tap → outcome → win/retry loop.
- **`TapRouter`** — one raycast on touch-up, walks up to the nearest `TappableObject`, ignores input while an outcome plays. Generous collider radii (a separate invisible collider, larger than the visual mesh — the direct analogue of Save Pip's `hot()` circle being bigger than `ring()`).
- **`ChoreographyPlayer`** — takes `OutcomeStep[]`, schedules each on PrimeTween at `At`, accumulates deltas per target, and reports completion at `Duration`. Provides `ResetToRest()`.
- **`SceneRefResolver`** — maps `"$self" / "$pepA" / "$pepB" / "Fx_Splash"` to live transforms in the instantiated diorama. Its lookup table is what the editor validator checks against, so a typo'd target is caught at author time, not at demo time.

### 5.3 Authoring tooling (build this in week 2, not week 5)

This is the multiplier on everything in §12. Budget two days.

- **`RescueDefinition` custom inspector** with a **Play This Rescue** button (loads `RescuePreview` and runs it) and three **Preview Outcome** buttons — the direct heir to Save Pip's `?p=c17&v=2` dev shortcut, which is the trick that made 106 rescues authorable at all.
- **Rescue Gauntlet** editor window: play every rescue's three outcomes back to back, unattended. This is how a polish pass on 36 rescues takes an hour instead of a day.
- **Validator** run on save and in CI (§15).
- **Anchor gizmos** so slots and Pep positions are visible in the scene view.

### 5.4 Deliberate simplification

Save Pip's bespoke templates had **variants** (same scene, different lineup, different correct answer) to resist memorisation across random rounds. Save Peps has an authored playlist where each rescue is met once in a fixed order, so variants add authoring cost for no benefit. **Drop variants in v1.** The `RescueObject[]` is the lineup, full stop.

---

## 6. Scene and animation architecture

### The diorama

Each rescue is a **small toy diorama on a floating platform** against a soft gradient sky. This is not decoration — it does real work:

- Environments become composable and reusable (8 dioramas host 36 rescues).
- The platform silhouette crops cleanly across every phone aspect ratio.
- It reads instantly as "toy", which is the requested visual direction.
- It gives a free, charming transition: the solved diorama tilts and slides away, the next drops in with a small bounce. One transition, used everywhere, costs a day and carries the whole game's sense of polish.

### Structure of an environment prefab

```
Diorama_Meadow (prefab)
├── CameraFraming        # position, rotation, FOV — the rescue's fixed camera
├── Ground / Geometry    # static, one shared material
├── Anchors/
│   ├── Anchor_PepA, Anchor_PepB
│   ├── Slot_1, Slot_2, Slot_3
│   └── Anchor_Meet      # where the reunion plays
├── Movers/              # scenery an outcome can move (the log, the gate, the water plane)
└── Fx/                  # pre-placed, disabled: splash, puff, sparkle, zzz
```

### Rendering

- **URP, mobile renderer.** Forward, no realtime shadows (blob shadows under the Peps and props), one directional light plus a gradient ambient. Bake nothing — the scenes are tiny and lighting must stay dynamic across prop swaps.
- **One palette atlas material** for essentially all geometry. Everything is untextured flat colour sampled from a small gradient atlas. This gives visual consistency for free, batches into a handful of draw calls, and makes CC0 packs from different sources look like one game after a material swap.
- **Post-processing:** a single global Volume — colour grading LUT, gentle vignette, a touch of bloom for the glow accents. Nothing else.
- **Camera:** perspective, low FOV (~28°), slightly elevated and tilted — the tilt-shift toy look. Fixed per rescue, no player control. A subtle punch/shake on impacts. Aspect safety: frame to a 4:3 safe box, let taller phones show more sky.
- **Budget:** 60 fps on a mid-range 2023 Android, < 60 draw calls, < 50k tris per diorama, ASTC textures, IL2CPP + ARM64, engine code stripping. Verified on a real low-end device in week 5, not week 7.

### Animation

- **Peps:** a light bone rig (~12 bones) or, if rigging becomes a bottleneck, an articulated toy — head/body/arms as separate rigid meshes parented and animated by transform, with squash-and-stretch via non-uniform scale. Both work with the aesthetic; the second is dramatically cheaper. **Faces are an atlas swap on a face quad**, straight from Save Pip's face stack: `neutral, worried, hopeful, panic, happy, love`.
- **Shared clips (~10):** idle, idle-reach-toward-partner, worried, panic, cheer, walk, hop, hug, and two shared reunion beats. Authored once, used by every rescue.
- **Per-rescue motion is data** (`OutcomeStep[]`), not clips. Clips supply *character*, the timeline supplies *staging*.
- **Escape hatch:** a `StepKind.PlayTimeline` for 3–4 signature set-pieces (round 1's first rescue, the round-10 finale). Rationed on purpose — every set-piece is a day that does not go to the other 32 rescues.

---

## 7. The Peps

Two characters, and the design job is to make a player *want* them back together within one second of seeing them.

- **Distinct silhouettes, not just colours** — one taller and rounder, one smaller and squarer; warm coral and soft mint against the palette. They must be tellable apart at thumbnail size and in the store screenshot.
- **The idle sells the premise.** Separated, they lean and reach toward each other, glance across the gap, sigh. No dialogue, no speech bubbles, no hearts-over-head clutter — body language and faces.
- **They react to each other, not to the player.** On a wrong tap: one covers their eyes, the other shrugs at them. That reciprocity is what makes the humour land and is the thing a single-character game cannot do.
- **The reunion is the payoff and gets real budget.** Run, hug, spin, one heart pop, a soft chime, a short haptic double-pulse. Roughly 1.5 s. This animation plays 36+ times per playthrough — it is the most-watched asset in the game and deserves to be the best one.
- **Failure is harmless and physical.** Nobody gets hurt; things go comically sideways and the pair end up further apart, dustier, or wearing something silly.

---

## 8. Round and progression model

```
Catalog (ordered)
└── Round 1..12
    └── 3 × RescueDefinition
```

- **Authored, ordered playlist** — not procedural. A paywall at "Round 11" requires a stable notion of round 11, the difficulty ramp needs hand-tuning, and a two-minute demo video needs the first 90 seconds to be identical every time. Save Pip's sampler was right for endless Reddit play and is wrong here.
- Save Pip's **round rules become editor validators**: within a round, no two rescues share a category, no two share a correct object, and difficulty should mix. Warnings, not errors — the last rounds may legitimately be all-hard.
- **Linear unlock.** Completing round *N* unlocks *N+1*. Rescues within a round are played in order; a wrong tap never blocks progress, it just costs the star.
- **Access rule**, the only gating logic in the game:

```csharp
bool CanPlay(int round) =>
    round <= Progress.HighestUnlocked &&
    (round <= Config.FreeRoundCount || Entitlements.HasPeps Unlimited);
```

- **UI, kept deliberately thin.** In-scene: `Round 4 · Rescue 2 of 3`, three dots, a goal line, a mute button. That is the whole HUD. Round-complete: a card with the three dots resolving into ★/✓, `Round 4 complete`, and **Continue**. No round map, no level select, no menus — a **Replay round** link on the complete card covers the replay case. Settings (sound, haptics, restore purchases, privacy, terms) live behind one small gear on the title screen.
- **Onboarding is one rescue, not a tutorial.** Round 1 rescue 1 is the easiest, most legible scene in the game with a single pulsing hint ring after 3 seconds of inactivity. Nothing else.

---

## 9. Save-state approach

- **JSON file in `Application.persistentDataPath/save.json`.** Atomic write (temp file + move), debounced to once per rescue completion and on `OnApplicationPause`. Not PlayerPrefs — a versioned file is easier to migrate, inspect and test.
- **Shape:**

```csharp
public sealed class SaveData
{
    public int   SchemaVersion = 1;
    public int   HighestUnlockedRound = 1;
    public Dictionary<string, Mark> RescueMarks;   // rescueId → Star | Check
    public int   TotalRescuesSolved;
    public bool  SoundMuted, HapticsOff;
    public long  FirstRunUtc;
}
```

- **Entitlement is never stored here.** RevenueCat's `CustomerInfo` is the only source of truth, and its SDK-side cache already handles offline launches. Persisting "is subscribed" in a user-writable file is both a correctness bug and a trivially defeated one.
- **Android Auto Backup** carries the file across reinstall and device change for free. That is our entire "cloud save" story, and per the brief it introduces no infrastructure.
- **Forward-compatible:** unknown fields ignored, missing fields defaulted, `SchemaVersion` gate for migrations. Corrupt file → fresh save, never a crash. Covered by unit tests.

---

## 10. RevenueCat subscription integration

**Install:** `purchases-unity` via OpenUPM (recommended over `.unitypackage`), plus **EDM4U (External Dependency Manager for Unity)** — mandatory, and the usual source of Android build pain. Set the Android activity `launchMode` to `standard` or `singleTop` in the manifest or purchases get cancelled mid-flow.

**The constraint that shapes the design: the RevenueCat SDK does not run in the Unity Editor.** Every purchase path must be tested on a device. Therefore:

```csharp
public interface IEntitlementService
{
    bool  IsSubscribed { get; }
    event Action Changed;
    Task<Offerings> GetOfferings();
    Task<PurchaseResult> Purchase(Package package);
    Task<PurchaseResult> Restore();
}
```

- `RevenueCatEntitlementService` — real, device only.
- `FakeEntitlementService` — editor and automated tests. A dev overlay toggles subscribed on/off, fakes cancel/pending/error, and fakes an expiry. **This is what lets 90% of paywall and gating work happen in the editor at editor speed**, and it is not optional given the SDK constraint.

**Configuration:** Google Play API key from RevenueCat; entitlement id `peps_unlimited`; one Offering (`default`) with two packages (`$rc_monthly`, `$rc_annual`). Read `CustomerInfo.Entitlements.Active` and subscribe to updates so a purchase, restore or lapse propagates without a restart.

**Play Console side:** one subscription product with two base plans (monthly, annual), the annual carrying a 7-day free-trial offer. Link Play to RevenueCat with a service account JSON; add Real-time Developer Notifications if it is quick, skip it if it is not — it is not required for entitlement to work.

**Failure modes that must all be handled explicitly** (each is a real, reachable state, and a judge hitting one is a bad demo): user cancels; payment pending/deferred; already owned; no network; Play Store unavailable or signed-out; subscription lapsed mid-session; restore finds nothing.

---

## 11. Paywall flow

**Trigger:** finishing round 10, or tapping a locked round. Never mid-rescue, never interrupting a reunion.

**Beat sequence:**

1. Round 10 completes with its normal celebration.
2. A short in-character beat — the Peps look up at a "to be continued" horizon. ~2 s, skippable by tapping.
3. The paywall slides up as a **diorama-styled sheet, not a system dialog**. This screen is a Design Award exhibit: same palette, same lighting, the Peps present in it.
   - Headline: **Peps Unlimited**
   - One line: *Unlock every round — including new rounds as they are added.*
   - Two options, prices and trial text pulled from RevenueCat Offerings so they are always correctly localised: **Annual — 7 days free, then $X/year** (highlighted, "best value") and **Monthly — $Y/month**.
   - Primary button: **Start free trial** / **Subscribe**.
   - Below the fold, small and honest: **Restore purchases**, Terms, Privacy, and the renewal/cancellation disclosure Play requires.
4. **Purchase succeeds** → sheet dismisses into a celebration, rounds 11+ visibly unlock, straight into round 11. No confirmation dialog, no receipt screen.
5. **Dismissed** → back to the round-complete card. Rounds 11+ show a small lock; tapping re-opens the paywall. No nagging, no timers, no interstitials.

**Restore** also lives in Settings, always reachable without hitting the paywall — required by policy and by decency.

---

## 12. Content plan: how much, and of what

**Target: 36 rescues in 12 rounds.** 10 free rounds (30 rescues) + 2 paid rounds (6 rescues). Floor 33; stretch 45 if the pipeline outruns the schedule.

The brief fixes the gate at round 11, which makes **30 polished rescues a hard floor** — there is no version of this that ships with 12 lovely rescues. The only way that is safe in 7.5 weeks is aggressive reuse, so the content plan is built around it:

- **8 environments** hosting **4–5 rescues each**: Meadow/Brook · Seaside Dock · Snow · Canyon · Pond · Cave · Night Garden · Rainy Rooftop.
- **~25 tappable props**, each appearing in 4–6 rescues — and, per Save Pip's "protean nine", each **saving in some and failing in others**. The plank bridges the brook and comes up six inches short at the canyon. The fan is the great red herring.
- **Reasoning-type spread**, hitting all eight kinds the brief asks for, roughly: crossing a gap ×6 · moving an obstacle ×5 · waking/distracting ×4 · simple physics ×5 · weather/environment ×4 · safe path ×4 · trap or visual clue ×4 · changing a Pep ×4.
- **Difficulty ramp:** rounds 1–3 easy-weighted, 4–8 mixed, 9–12 surprising-weighted. Round 1 rescue 1 is the demo-video opener and gets set-piece treatment.
- **Selection from Save Pip:** take the 100-verb catalog, drop anything that needs a full 3D character rig we do not have, anything that reads only in 2D cross-section (the deep-hole pit view), and anything that cannot be re-framed around a *couple*. Redesign the survivors so the Peps' separation is the problem, not one character's predicament.

**Throughput required:** 36 rescues over roughly 4 weeks of content work ≈ **9–10 per week**, ≈ 1.5 hours each once the authoring tooling and prop library exist. That is achievable only if §5.3 ships in week 2. If by end of week 5 the run rate is below 7/week, cut to 33 rounds and, if needed, drop `FreeRoundCount` to 9 — **cut count, never polish.**

---

## 13. Asset and 3D-content strategy

- **Foundation: CC0 low-poly kits** (Kenney's Nature/Platformer/Food/Holiday packs are the right style, genuinely free, and internally consistent). Re-material everything onto our single palette atlas on import so mixed sources unify.
- **Custom: the Peps only.** Modelled, rigged, and animated by us. Roughly 3–4 days including the reunion animation, and worth every hour.
- **A palette lock, decided once and never renegotiated:** see `design/palette.md` — 36 swatches in 9 ramps, inherited from Save Pip's proven warm palette, shipped as a 4×9 `palette_atlas.png`. Everything samples this one texture through one material.
- **Readability rules for tappables:** a soft rim/outline highlight, a slow idle bob or shimmer, and a colour that separates from the ground. If a tappable is unclear on a 6" screen at arm's length, it is broken regardless of how nice it looks in the editor.
- **Audio:** ~15 short samples (tap, slide, boing, plop, whoosh, splash, pop, chime, cheer, lock) plus an optional quiet music bed. CC0 sources (Kenney audio, freesound CC0) and light processing. Save Pip's `fx.ts` names the exact vocabulary that worked — reuse those names.
- **Store assets** (icon, feature graphic, 8 screenshots at 1179×2556, 1024² icon) get their own half-day in week 6, produced by staging the game in the editor with a screenshot camera. Not an afterthought — for the Design Award these *are* the first impression.

---

## 14. Android and Google Play release path

**This runs in parallel from week 1. It is not a phase at the end.**

1. **Now:** confirm/create the developer account and complete identity verification (this alone can take days).
2. **Week 1:** create the app listing, reserve the name, lock the package id. Upload a first signed AAB — real RevenueCat SDK linked, even if the game behind it is one placeholder rescue. Enrol in Play App Signing and back up the upload key.
3. **Week 1–2:** internal testing immediately, then **closed testing with 12+ testers**. If a personal account needs production access, the 14-day continuous-opt-in clock starts here and gates everything. Recruit testers before the build exists, not after.
4. **Week 2:** subscription product + two base plans + trial offer created in Play Console; service account linked to RevenueCat; license testers added so purchases are free and renewals are fast.
5. **Week 3–4:** store listing content, content rating questionnaire (IARC), Data Safety form, ads declaration (none), target audience **13+** (D8), Privacy and Terms live on GitHub Pages — adapt Save Pip's existing `PRIVACY.md` / `TERMS.md`.
6. **~Week 4:** apply for production access the moment the 14-day window closes.
7. **Build settings:** AAB, IL2CPP, ARM64 only, target the latest API level Play requires for new apps at submission time (verify in Console — this changes annually and is a hard rejection if wrong), min API 24, ASTC textures, engine code stripping, portrait-only.
8. **Week 6:** release candidate → production, staged then 100%. Run the pre-launch report and fix anything it flags.
9. **Target live on Play by ~24 Sep**, leaving a week of buffer for review turnaround before the 30 Sep deadline.
10. **Week 7:** two-minute demo video, Devpost submission with promo codes and/or the trial for judges.

---

## 15. Testing strategy

**EditMode unit tests** (fast, run on every commit):
- **Content validator** — the direct heir to `catalog.test.ts`, and the highest-value tests in the project: exactly 3 objects with unique ids and exactly 1 correct; unique verb; 2–4 word unique goal line; a quip on every wrong object; duration in 2.0–3.6 s; every step's `At` and `At + Duration` inside the outcome window; **every step target resolving to a real anchor/fx in the environment prefab**; the protean-object rule (no prop is always correct or always wrong).
- **Round validator** — no repeated category or correct object within a round; difficulty mix as a warning.
- **Progression and gating** — unlock order, `FreeRoundCount` boundary, behaviour with a `FakeEntitlementService` in every state (subscribed, not, lapsed mid-session).
- **Save** — round-trip, corrupt file, missing fields, schema migration.

**PlayMode tests:**
- Choreography player lands every target on its expected final transform.
- `ResetToRest()` after a wrong outcome restores *every* animated target, particle, and enabled flag. Run this for all 36 rescues, automated — this is the bug class most likely to be seen by a judge.
- Tap routing selects the correct object across the collider set.

**Manual and device:**
- **Rescue Gauntlet** review pass (§5.3) after each content batch — every rescue's three outcomes, watched end to end.
- Device matrix: one low-end Android (4-year-old midrange) and one modern. Check 60 fps, cold start under ~3 s, memory, thermals, and readability at arm's length.
- **Purchase testing on device only** (SDK constraint): buy monthly, buy annual with trial, cancel mid-flow, restore on a fresh install, launch offline with a cached entitlement, and let a test subscription expire to confirm rounds re-lock cleanly.
- A full 12-round playthrough from a clean install, twice, in the final week.

---

## 16. Major technical risks

| # | Risk | Impact | Mitigation |
|---|---|---|---|
| R1 | **Play production access gated behind 12 testers × 14 days** | Fatal — the app never goes live, submission is invalid | Start day 1. Testers recruited before the build exists. Track the opt-in count daily. |
| R2 | **30-rescue content floor** (the brief's free-round structure) | Ship a thin or unpolished game | Environment/prop reuse, authoring tooling in week 2, weekly quota, hard freeze, `FreeRoundCount` as the release-week lever |
| R3 | **RevenueCat SDK does not run in the Editor** | Slow, error-prone monetization work | `IEntitlementService` + fake impl from day one; device-test loop scheduled, not improvised |
| R4 | **Unity Android build pain** — EDM4U, Gradle, manifest merge, signing | Days lost at the worst possible moment | Get a signed AAB with the real SDK to Play in week 1, while it is cheap to lose a day |
| R5 | **No 3D artist** | Inconsistent or unfinished look, which directly costs the Design Award | CC0 kits + one palette atlas + custom Peps only; art direction decided once in week 1 |
| R6 | **Readability at phone size** | The core interaction fails | Test on a real device in week 1; rim highlights; generous colliders; the arm's-length rule |
| R7 | **Retry state leaks** | Visible, embarrassing bugs | Explicit `ResetToRest()`, automated across all rescues |
| R8 | **Perf/size on low-end Android** | Bad judge experience, bad reviews | One material, no realtime shadows, draw-call budget, low-end device in the loop from week 5 |
| R9 | **Play policy friction** — Data Safety, subscription disclosure, target audience | Rejection or delay in the final week | Complete all forms by week 4; declare 13+; disclosures on the paywall from the start |
| R10 | **Solo time budget over 7.5 weeks** | Everything above, compounded | Front-load the irreversible (Play, tooling), keep the cut list explicit and honest |

---

## 17. Phased implementation sequence

| Phase | Dates | Outcome | Definition of done |
|---|---|---|---|
| **P0 — Setup** | 7–9 Aug | Unity 6 + Android module installed; empty project; git + LFS; Play account and app created; RevenueCat project created; art direction and palette locked | An empty Unity project builds and installs on a physical phone |
| **P1 — Vertical slice + the Play clock** | 10–16 Aug | One complete rescue: tap → outcome → reunion → retry. `RescueDefinition` v1, `ChoreographyPlayer` v1, Peps prototype, diorama look. **Signed AAB with RevenueCat linked uploaded to closed testing with 12 testers.** | The one-tap loop feels good on a phone, and the 14-day clock is running |
| **P2 — Systems + tooling** | 17–23 Aug | Rescue authoring tooling, validators, Rescue Gauntlet. 3 environments, 6–9 rescues. Round loop, progression, save. Audio, haptics, camera punch. Play subscription product created and linked to RevenueCat | A round of 3 rescues plays start to finish; a new rescue takes < 2 hours to author |
| **P3 — Content sprint** | 24 Aug – 6 Sep | The bulk: to ~24–30 rescues across 8 environments. Art and audio passes. Production access applied for as soon as the window closes | 8 rounds playable end to end; run rate ≥ 7 rescues/week |
| **P4 — Monetization** | 7–13 Sep | Paywall UI, entitlement gating, restore, all failure states. Store listing, screenshots, icon, ratings, Data Safety. On-device purchase testing complete | Purchase, restore, lapse and offline all verified on a device |
| **P5 — Freeze + polish** | 14–20 Sep | **Content freeze at 36.** Perf pass on a low-end device, bug bash, full playthroughs, accessibility, the whole feel pass | No P0/P1 bugs; 60 fps on the low-end device; two clean playthroughs |
| **P6 — Release** | 21–27 Sep | Release candidate → production rollout. Live on Play by ~24 Sep. Two-minute demo video. Devpost draft | App is publicly installable from Google Play |
| **P7 — Submit + buffer** | 28–30 Sep | Devpost submission with promo codes/trial, screenshots, description | Submitted, with a day of slack left |

**Hard checkpoints** — miss one and cut scope the same day, not the next week:

- **16 Aug** — closed testing live with 12 testers. *If this slips, everything else is moot.*
- **23 Aug** — authoring a new rescue takes under two hours.
- **6 Sep** — 24+ rescues playable.
- **20 Sep** — content frozen, release candidate built.
- **24 Sep** — live on Google Play.

---

## 18. Explicit non-goals for v1

Named here so they can be refused quickly later: no player-controlled camera or movement; no inventory, dialogue, or currency; no energy, hearts, or timers; no level select map; no leaderboards, accounts, or backend; no seeds or shareable rounds; no analytics beyond what RevenueCat provides; no iOS; no localisation beyond English (store-listing localisation only if there is spare time); no consumables, ads, or second subscription tier; no procedural round generation.

The goal is a small, delightful, highly polished one-tap puzzle game that feels complete — and is live on Google Play with a working subscription before 30 September.
