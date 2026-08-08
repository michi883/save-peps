# Save Peps

A one-tap 3D puzzle game for Android.

> Two Peps. One small predicament. Tap the right thing to bring them back together.

Built for [RevenueCat Shipaton 2026](https://revenuecat-shipaton-2026.devpost.com/) — primary category **Best Game**, secondary **RevenueCat Design Award**. Submission deadline **30 September 2026**.

## How it plays

A fixed-camera toy diorama shows two Peps separated by something small and silly, with three tappable objects in the scene. Exactly one reunites them. The other two fail funny — a short visual gag, a dry one-liner, and an immediate retry. There is no fail state and no punishment; wrong answers are part of the entertainment.

A **round is 3 rescues**. Solve one on the first tap and it earns a ★, after a retry a ✓. Rounds 1–10 are free; **Peps Unlimited** unlocks the rest.

## Repo layout

```
PLAN.md            the development plan — architecture, schedule, risks, decisions
design/            art direction lock and the palette atlas
docs/              privacy + terms (published to GitHub Pages; Play requires the URLs)
store/             listing copy, icon, screenshots
unity/SavePeps/    the Unity project
```

Start with [`PLAN.md`](PLAN.md). It is the source of truth for what is being built and why.

## Toolchain

- **Unity 6.3 LTS (6000.3.21f1)** + Android Build Support. Chosen over 6000.0 LTS because it supports Android target API 35/36, which Google Play requires for new apps.
- **URP**, mobile renderer, portrait only, IL2CPP + ARM64, AAB output.
- **RevenueCat** `purchases-unity` via OpenUPM, plus EDM4U.
- **PrimeTween** for the choreography runtime.

Use Unity's bundled OpenJDK/SDK/NDK rather than a system install.

## Development

```bash
git lfs install                      # once per clone — meshes, textures and audio are LFS
open unity/SavePeps                  # via Unity Hub
```

On a fresh clone, run **Tools > Save Peps > Apply Project Settings** once. Most build settings live in `ProjectSettings/` and travel with the repo, but a few (active build target, AAB-vs-APK) are machine-local and would otherwise silently default back to APK.

The RevenueCat SDK does not run in the Unity Editor. Editor play uses `FakeEntitlementService`, which can simulate every subscription state; real purchase paths must be tested on a device.

## Relationship to Save Pip

Save Peps is a new game, not a port. It inherits the authoring discipline of [Save Pip](https://github.com/michi883/save-pip) — declarative rescue specs, a shared choreography vocabulary, machine-checked content rules, and the house rule that a predicament must read in three seconds with no text. The art, the code, the cast, and the monetization are all new.

## License

<!-- TODO: decide before making the repo public (D9 in PLAN.md). -->
