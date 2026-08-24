# Save Peps

A one-tap 3D puzzle game for Android.

> Two Peps. One small predicament. Tap the right thing to bring them back together.

Built for [RevenueCat Shipaton 2026](https://revenuecat-shipaton-2026.devpost.com/) — primary category **Best Game**, secondary **RevenueCat Design Award**. Submission deadline **30 September 2026**.

## How it plays

A fixed-camera toy diorama shows two Peps separated by something small and silly, with three tappable objects in the scene. Exactly one reunites them. The other two fail funny — a short visual gag, a dry one-liner, and an immediate retry. There is no fail state and no punishment; wrong answers are part of the entertainment.

A **round is 3 rescues**. Solve one on the first tap and it earns a ★, after a retry a ✓. Rounds 1–10 are free; one lifetime purchase unlocks Rounds 11–12.

Every round owns a **world**, and a world is more than a colour scheme: its own ground silhouette, camera, sky, key light, fog, ambient motion and sound bed, plus a physical rule its three rescues all obey — the clockwork courtyard never moves until a linkage moves it, the orbital station has no ground at all, the crystal cave is the only enclosed space in the game. Twelve rounds are twelve worlds and thirty-six rescues are thirty-six stages; [`design/ROUND_CATALOG.md`](design/ROUND_CATALOG.md) is the source of truth for all of it.

The shell stays deliberately small: **Play** chooses a useful available round without immediately repeating the last one, while **Choose round** offers direct control. Finishing a round returns to **Keep playing** rather than forcing a strictly linear next button. A pause control in the corner of the HUD opens a bottom sheet with Resume, Progress, Choose round, Home, and sound/buzz toggles, and Android Back walks the same path — the player is never stuck inside a rescue.

## Repo layout

```
PLAN.md            the development plan — architecture, schedule, risks, decisions
design/            art direction lock and the palette atlas
docs/              privacy + terms (published to GitHub Pages; Play requires the URLs)
store/             listing copy, icon, screenshots
unity/SavePeps/    the Unity project
```

Start with [`PLAN.md`](PLAN.md). It is the source of truth for what is being built and why.
The polished interaction and feedback loop is frozen in [`docs/core-ux.md`](docs/core-ux.md); the content sprint should reuse that contract rather than reopen it per rescue.

## Toolchain

- **Unity 6.3 LTS (6000.3.21f1)** + Android Build Support. Chosen over 6000.0 LTS because it supports Android target API 35/36, which Google Play requires for new apps.
- **URP**, mobile renderer, portrait only, IL2CPP + ARM64, AAB output.
- **RevenueCat** `purchases-unity` via OpenUPM, plus EDM4U.
- **No tween library.** The choreography runtime is hand-rolled (decision D7): the additive-delta model is bespoke, so a tween library would only have supplied easing and scheduling, and dropping it keeps a dependency off the Android build path.

Use Unity's bundled OpenJDK/SDK/NDK rather than a system install.

## Development

```bash
git lfs install                      # once per clone — meshes, textures and audio are LFS
open unity/SavePeps                  # via Unity Hub
```

On a fresh clone, run **Tools > Save Peps > Apply Project Settings** once. Most build settings live in `ProjectSettings/` and travel with the repo, but a few (active build target, AAB-vs-APK) are machine-local and would otherwise silently default back to APK.

The RevenueCat SDK does not run in the Unity Editor. Editor play uses `FakeEntitlementService`, which simulates free/full-game ownership and store outcomes; real Google Play purchase paths must be tested from a Play-installed build on a device.

### Tester Mode

The editor and Unity **Development Builds** always boot in ordinary User Mode with no debug control visible. On the title tableau, tap:

`heart → green Pep → pink Pep → heart → green Pep → pink Pep → heart`

That switches on Tester Mode, opens its compact target sheet, and shows a small `TESTER` indicator until the same title sequence switches back to User Mode. Choose a round and optional rescue, select **Return to title**, then use the title's Play button; Play keeps that exact target and bypasses progression and premium gates without writing `save.json`. The normal round picker also bypasses its locks while Tester Mode is active.

The secondary controls restart the current rescue, preview any of its three outcomes, reset the profile, apply Fresh/Partial/All completed/All perfect states, unlock progression without adding marks, and switch the existing fake entitlement between Free and Full Game. The **Purchase** section reports the active billing store, real RevenueCat ownership, product availability, and localized price, then opens the exact production Full Game Unlock screen without granting access or changing progress. Simulated **Access** and real store ownership deliberately remain separate. Profile changes happen only from those explicit buttons; navigation, Play, and outcome preview remain profile-safe.

Build the device QA artifact with **Tools > Save Peps > Build Android Development APK (Tester Mode)** or:

```bash
Unity -batchmode -quit -nographics -projectPath /absolute/path/to/unity/SavePeps \
  -executeMethod SavePeps.EditorTools.BuildScript.BuildAndroidDevelopmentApk \
  -logFile /tmp/save-peps-dev-apk.log
```

The secret hit areas, indicator, and controls are disabled before the first frame of a non-development APK/AAB. Tester Play and outcome previews never alter `save.json`, and fake entitlement remains separate from local profile data.

## Run on the Pixel 4

From the repository root, use Unity's bundled `adb`; no separate Android SDK install is required:

```bash
ADB=/Applications/Unity/Hub/Editor/6000.3.21f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb

# The Pixel should appear as `device`, not `unauthorized`.
$ADB devices -l

# Optional after making a new APK. `-r` preserves the current save.
APK="$(ls -t unity/SavePeps/Build/Android/*.apk | head -1)"
$ADB install -r "$APK"

# Stop any old process and launch the installed game.
$ADB shell am force-stop fan.sound.savepeps
$ADB shell am start -n fan.sound.savepeps/com.unity3d.player.UnityPlayerActivity
```

If the device says `unauthorized`, unlock it and accept the USB debugging prompt, then run `$ADB devices -l` again. To confirm the launch or diagnose a blank screen:

```bash
$ADB shell dumpsys activity activities | grep -E 'mResumedActivity|topResumedActivity'
$ADB logcat -d | grep -oE '\[SavePeps\].*'
```

Do not run `pm clear fan.sound.savepeps` unless a fresh-install state is intentional; it deletes the on-device save. The reference device is 1080×2280, and repeatable tap coordinates and screenshot commands live in [`AGENTS.md`](AGENTS.md#6-device-testing).

## Authoring a rescue

A rescue is data, never C#: a `RescueDefinition` asset naming a diorama, two Peps, three objects and one correct answer, with each outcome as a flat list of timed steps. Everything below lives under **Tools > Save Peps**.

| Tool | What it is for |
|---|---|
| **Rescue inspector** | Select any rescue asset. Validation shows inline, and the preview buttons enter play mode and run one outcome — the cost of checking a gag has to be one click. |
| **Rescue Gauntlet** | Plays every outcome in the catalogue back to back, unattended. Wrong outcomes first, correct one last, so each rescue ends on the reunion. This is how a polish pass takes an hour instead of a day. |
| **Validate Content** | The catalogue-wide rules: unique verbs, round composition, one world per round, one stage per rescue, one *(prop, reasoning)* answer per catalogue, and the protean-object rule. Also runs on save, and as an EditMode test. |
| **Render Stage Contact Sheet** | Renders all 36 opening frames in their own world's light and framing. The screenshot test — hide the HUD, look at one frame, know which round it is — made cheap enough to run every pass. |
| **Show Anchor Gizmos** | Draws `Anchor_*`, `Slot_*` and movers in the scene view while dressing a diorama. |
| **Save >** | Delete the save, unlock all rounds, or reveal the file — the loop progression work needs over and over. |
| **Tester Mode** | In Editor play or a Development APK, stage any catalogue rescue/outcome and apply deterministic profile/access states without weakening the player path. |
| **Build Game Scene** | Regenerates the Game scene only. Reads the catalogue off disk; never writes content. |
| **Seed Round One Content** | Creates any missing round-one assets. Existing ones are left alone. |

Two safety properties worth relying on:

- **Seeding creates, it never overwrites.** The generator is how round one came into being, but the assets on disk are the source of truth the moment anyone edits one. Re-seeding exists under **Danger >**, and it names what it is about to discard before doing it.
- **Previewing never touches progress.** The flow is disabled during playback, so a solved preview records no mark.

Run the tests with:

```bash
Unity -batchmode -runTests -testPlatform EditMode -projectPath unity/SavePeps
Unity -batchmode -runTests -testPlatform PlayMode -projectPath unity/SavePeps
```

The EditMode suite validates the real authored catalogue, so a rescue with a step aimed at a target that does not exist fails the build rather than silently doing nothing on stage.

### Worlds and atmosphere

Sky, ambient, fog, sun, fill and camera framing are authored per world on a `DioramaAtmosphere` component sitting on every diorama prefab, and applied by `AtmosphereDirector` — the single writer of scene-wide lighting — which cross-fades them over 0.38 s when a rescue is built and pushes framing through `GameFeel.SetFraming`. Nothing about lighting lives in `Game.unity` any more.

Continuous environmental motion is `AmbientMotion` (`Sway`, `Bob`, `Drift`, `Spin`, `Pulse`, `Flicker`, `Beat`). It never shares a transform with an `AnimTarget`, because choreography composes additively onto an idle and can never cancel one: anything that has to *stop* moving is a Hide/Show swap between a moving twin and a still twin. `Flicker` is deterministic, so the same world always screenshots the same.

The art pipeline builds everything from Unity primitives: `ToyShapes` (helpers) → `PropLibrary` (36 props) → `WorldKits` (12 world kits: base silhouette, dressing, atmosphere) → `DioramaLibrary` (36 stages) → `PrototypeArt.Generate`. Regenerate with:

```bash
Unity -batchmode -quit -nographics -projectPath /absolute/path/to/unity/SavePeps   -executeMethod SavePeps.EditorTools.PrototypeArt.Generate -logFile /tmp/art.log
```

The contact sheet is the one batchmode tool that must **not** be given `-nographics`: that forces a Null GfxDevice and every PNG comes back uniform grey with nothing in the log.

## Relationship to Save Pip

Save Peps is a new game, not a port. It inherits the authoring discipline of [Save Pip](https://github.com/michi883/save-pip) — declarative rescue specs, a shared choreography vocabulary, machine-checked content rules, and the house rule that a predicament must read in three seconds with no text. The art, the code, the cast, and the monetization are all new.

## License

<!-- TODO: decide before making the repo public (D9 in PLAN.md). -->
