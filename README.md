# Save Peps

A one-tap 3D puzzle game for Android.

> Two Peps. One small predicament. Tap the right thing to bring them back together.

Built for [RevenueCat Shipaton 2026](https://revenuecat-shipaton-2026.devpost.com/) — primary category **Best Game**, secondary **RevenueCat Design Award**. Submission deadline **30 September 2026**.

## How it plays

A fixed-camera toy diorama shows two Peps separated by something small and silly, with three tappable objects in the scene. Exactly one reunites them. The other two fail funny — a short visual gag, a dry one-liner, and an immediate retry. There is no fail state and no punishment; wrong answers are part of the entertainment.

A **round is 3 rescues**. Solve one on the first tap and it earns a ★, after a retry a ✓. Rounds 1–10 are free; **Peps Unlimited** unlocks the rest.

The shell stays deliberately small: **Play** chooses a useful available round without immediately repeating the last one, while **Choose round** offers direct control. Finishing a round returns to **Keep playing** rather than forcing a strictly linear next button.

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

The RevenueCat SDK does not run in the Unity Editor. Editor play uses `FakeEntitlementService`, which can simulate every subscription state; real purchase paths must be tested on a device.

## Authoring a rescue

A rescue is data, never C#: a `RescueDefinition` asset naming a diorama, two Peps, three objects and one correct answer, with each outcome as a flat list of timed steps. Everything below lives under **Tools > Save Peps**.

| Tool | What it is for |
|---|---|
| **Rescue inspector** | Select any rescue asset. Validation shows inline, and the preview buttons enter play mode and run one outcome — the cost of checking a gag has to be one click. |
| **Rescue Gauntlet** | Plays every outcome in the catalogue back to back, unattended. Wrong outcomes first, correct one last, so each rescue ends on the reunion. This is how a polish pass takes an hour instead of a day. |
| **Validate Content** | The catalogue-wide rules: unique verbs, round composition, and the protean-object rule. Also runs on save, and as an EditMode test. |
| **Show Anchor Gizmos** | Draws `Anchor_*`, `Slot_*` and movers in the scene view while dressing a diorama. |
| **Save >** | Delete the save, unlock all rounds, or reveal the file — the loop progression work needs over and over. |
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

## Relationship to Save Pip

Save Peps is a new game, not a port. It inherits the authoring discipline of [Save Pip](https://github.com/michi883/save-pip) — declarative rescue specs, a shared choreography vocabulary, machine-checked content rules, and the house rule that a predicament must read in three seconds with no text. The art, the code, the cast, and the monetization are all new.

## License

<!-- TODO: decide before making the repo public (D9 in PLAN.md). -->
