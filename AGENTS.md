# Working on Save Peps

Operating notes for coding agents. **This is the "how", not the "what"** — [`PLAN.md`](PLAN.md) is the source of truth for what is being built and why, and [`README.md`](README.md) covers the authoring tools. Read this first anyway: most of it is a day someone already lost.

---

## 1. Unity is single-instance. Check before you launch it.

**Only one Unity process can hold this project at a time.** A second one does not fail fast — it hangs, or sits in what looks like a slow licensing handshake, or blocks until the first finishes. With several agents on one machine this is the single most likely reason a command appears to freeze.

Before any Unity command:

```bash
pgrep -lf "Unity.app/Contents/MacOS/Unity"          # someone else mid-run?
ls unity/SavePeps/Temp/UnityLockfile 2>/dev/null    # lock held (Editor open counts)
```

If either shows something, **wait or do non-Unity work** — read code, edit files, inspect assets. Do not start a second run and do not delete the lockfile. A stale lockfile with no matching process is safe to remove; a lockfile with a live process is not.

The Unity **Editor being open counts as that one instance.** Batch runs will not proceed alongside it.

### Sandbox

Unity needs local IPC sockets and its licensing client. Under a restrictive sandbox the launch is refused. Run Unity commands with the sandbox disabled (`dangerouslyDisableSandbox: true` on the Bash call) rather than trying to work around the socket.

### Expect it to be slow

A Unity batch invocation is tens of seconds *before* it does anything, and the first run after a domain reload or a fresh checkout is the slowest. Rough wall-clock on this project:

| Command | Typical |
|---|---|
| Compile-only | ~40–70 s |
| `-runTests EditMode` | ~60–90 s |
| `-runTests PlayMode` | ~90–120 s |
| APK build | ~90–150 s |

Set a generous Bash `timeout` (600000 ms is fine). Slow is normal; **hung usually means the lock.**

---

## 2. Running Unity headlessly

Four commands cover essentially all work. `UNITY` and `PROJ` below:

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity
PROJ=/Users/michi/save-peps/unity/SavePeps
```

```bash
# 1. Compile only — the fastest "did I break it" check
$UNITY -batchmode -quit -nographics -projectPath $PROJ -logFile /tmp/x/compile.log

# 2. Run an editor menu method (seeding, scene build, validation, APK)
$UNITY -batchmode -quit -nographics -projectPath $PROJ \
       -executeMethod SavePeps.EditorTools.ContentValidator.ValidateFromMenu \
       -logFile /tmp/x/run.log

# 3. Tests — note: NO -quit, the test runner owns the exit
$UNITY -batchmode -runTests -testPlatform EditMode -projectPath $PROJ \
       -testResults /tmp/x/edit.xml -logFile /tmp/x/edit.log

# 4. Android APK for device testing (BuildAndroid for the AAB)
$UNITY -batchmode -quit -nographics -projectPath $PROJ \
       -executeMethod SavePeps.EditorTools.BuildScript.BuildAndroidApk \
       -logFile /tmp/x/apk.log
```

Three rules that will otherwise cost you a run each:

- **`-projectPath` must be absolute.** A `cd` earlier in the same compound command silently breaks a relative path, and the error is the unhelpful `Couldn't set project path to:`.
- **The exit code lies.** Unity prints `Aborting batchmode due to failure: Scripts have compiler errors` and still exits `0`. Never trust `$?`; always grep the log:
  ```bash
  grep -E 'error CS' /tmp/x/compile.log | sort -u | head
  ```
- **Always use `-logFile <path>`, never `-logFile -`.** Unity's log to stdout is enormous and will bury the tool output you actually need. Write it to a file and grep it.

Reading test results — the XML is the only reliable signal:

```bash
python3 - <<'PY'
import xml.etree.ElementTree as ET
r = ET.parse("/tmp/x/edit.xml").getroot()
print("total=%s passed=%s failed=%s" % (r.get('total'), r.get('passed'), r.get('failed')))
for tc in r.iter('test-case'):
    if tc.get('result') != 'Passed':
        print("FAIL:", tc.get('fullname'))
        m = tc.find('.//message')
        if m is not None and m.text: print(" ", m.text.strip()[:800])
PY
```

Menu methods worth knowing (all under **Tools > Save Peps** in the Editor):

| Method | Does |
|---|---|
| `ContentSeeder.SeedFromMenu` | Creates missing content. Never overwrites. |
| `ContentSeeder.ReseedFromMenu` | **Destructive.** Rewrites all content from code. Skips its dialog in batch mode. |
| `BrookScene.BuildGameScene` | Regenerates `Game.unity`. Never touches content. |
| `PrototypeArt.Generate` | Regenerates all prefabs and materials. |
| `ContentValidator.ValidateFromMenu` | Validates the catalogue. |
| `BuildScript.BuildAndroidApk` / `BuildAndroid` | APK for `adb` / AAB for Play. |

---

## 3. What "done" means

In order, cheapest first. Do not skip to the end.

1. **Compiles** — command 1.
2. **EditMode tests pass** — includes the content validator over the real catalogue, so a typo'd step target fails here rather than silently doing nothing on stage.
3. **PlayMode tests pass** — plays a full round in the real scene. This is what catches sequencing and reset bugs.
4. **It runs on the device** — for anything visual, see §6. Several classes of bug are *invisible* in the editor (§7).

Report what you actually ran. "Tests pass" without the numbers is not a result.

---

## 4. Architecture invariants

Break these and things fail silently rather than loudly.

- **Content is data, never C#.** A rescue is a `RescueDefinition` asset: an environment, two Peps, three objects, one correct index, and each outcome as a flat list of timed `OutcomeStep`s. Adding a rescue must not mean writing a new MonoBehaviour.
- **Scenes are generated, not hand-edited.** `Game.unity` is built by `BrookScene.BuildGameScene`. Any manual edit is destroyed the next time someone runs it. Change the builder.
- **Seeding creates, never overwrites.** Existing assets are left alone; `Danger > Re-seed` is the only overwriting path. `Catalog.FreeRoundCount` in particular is the release-week paywall lever (PLAN D3) and must survive every tool. There are tests for this — if you make seeding overwrite, they fail on purpose.
- **Rest pose is identity.** Authored placement lives on a prop's outer transform; `AnimTarget` sits on a child that rests at local identity and receives every animation. That is why `ResetToRest()` is exact rather than approximate. Never animate the placement transform.
- **Choreography deltas are slot-relative.** Moving a prop to a different `Slot_n` invalidates every `Delta` in its outcome. Re-aim them, or reuse a gag written purely in self-relative terms (see `PropGags`).
- **One definition of the paywall.** `Access.CanPlay` is a pure function and the only gate. `GameFlow` delegates to it. Do not restate the rule anywhere.
- **Entitlement is never persisted.** RevenueCat's `CustomerInfo` is the only source of truth. Writing "is subscribed" into `save.json` is both wrong and trivially defeated.
- **Assembly definitions exist.** `SavePeps.Runtime`, `SavePeps.Editor` (Editor-only, never ships), and the two test assemblies. EditMode tests cannot reference `Assembly-CSharp`, which is why the asmdefs are not optional.

---

## 5. Authoring content

A round is three rescues; the catalogue is an ordered list of rounds. Environments are reused — PLAN §6 budgets eight dioramas for thirty-six rescues.

Rules the validator enforces (`ContentValidator`), each of which exists because breaking it produced a rescue that looked fine and failed silently:

- Exactly three objects, exactly one correct, unique ids.
- Every wrong object has a quip. Failure has to land as a joke, never a scold.
- Every step target resolves to a real anchor, mover, or `$self`/`$pepA`/`$pepB`.
- Every step finishes inside its outcome's duration; duration in 2.0–3.6 s.
- Only the correct object may contain a `Meet` step.
- Within a round: no shared correct object, and **the answer must not always sit in the same slot** — a round whose answers never move is winnable by tapping one screen position repeatedly.
- Protean-object rule (warning): no prop should be the answer every time it appears, or never.

**Environments cost far more than rescues.** Rescues are pure data and cheap. A new diorama took several device round-trips to get reading correctly — budget the environment, not the rescues, and expect to iterate on it against a real screen.

---

## 6. Device testing

A Pixel 4 is the reference device. Visual work is not verified until it has run there.

```bash
adb devices -l
adb install -r "$(ls -t unity/SavePeps/Build/Android/*.apk | head -1)"
adb shell pm clear fan.sound.savepeps          # fresh install state
adb logcat -c
adb shell am start -n fan.sound.savepeps/com.unity3d.player.UnityPlayerActivity
adb exec-out screencap -p > /tmp/x/shot.png    # then read the PNG
adb logcat -d | grep -oE '\[SavePeps\].*'      # taps, saves, errors
```

Drive the game with synthetic taps rather than asking a human — it makes runs reproducible:

```bash
adb shell input tap 198 1752    # Slot_1  (near-left)
adb shell input tap 909 1764    # Slot_2  (near-right)
adb shell input tap 272 521     # Slot_3  (far-left)
adb shell input tap 540 2162    # "Try again"
adb shell input tap 540 1180    # "Continue" on the round card
```

Screen is 1080×2280; those coordinates are for that resolution. Confirm every tap landed with `grep 'Tapped'` on logcat before trusting a screenshot — and note that **a human holding the phone will also be tapping**, which silently invalidates a scripted run.

Inspect the save directly:

```bash
adb shell run-as fan.sound.savepeps cat files/save.json
```

---

## 7. Gotchas that cost real time

Every one of these was found the hard way.

- **Coplanar faces z-fight**, and it is invisible in the editor and obvious on a phone. Offset by ~0.01 rather than sitting flush.
- **A `UI.Image` with no sprite draws a square.** "Three dots" shipped as three boxes.
- **A component that deactivates its own GameObject in `Awake` never runs `Awake`.** Put the component on an always-active holder and toggle a child.
- **`AssetDatabase.LoadAssetAtPath` before `EditorSceneManager.NewScene` gives you a dead reference.** Opening a scene unloads unused assets; the asset is destroyed and assigns as `null`. Load *after* the scene exists.
- **`EditorUtility.DisplayDialog` cannot be answered in batch.** Guard with `Application.isBatchMode`.
- **`IReadOnlySet<T>` is not available** at this project's language level. Use `HashSet<T>`.
- **`BuildReport.summary.totalSize` is not the artifact size** — it read 283 MB for an 18 MB APK. Stat the file.
- **Unsigned local builds flip `androidUseCustomKeystore` to 0** in `ProjectSettings.asset`. That is incidental churn from building without the signing env vars; do not commit it.
- **A wrong tap leaves input disabled** until `Retry()` is called. Waiting for `InputEnabled` after a wrong outcome will time out — that is correct behaviour, not a bug.

---

## 8. Conventions

- Match the surrounding style: `_camelCase` serialized fields, XML doc comments that explain **why** rather than what.
- Comments earn their place by recording a decision or a trap, not by narrating the code.
- Commit messages follow the existing history: a plain subject line, a body explaining the reasoning and what was verified, then the `Co-Authored-By` and `Claude-Session` trailers.
- `Build/`, `Library/`, `Temp/` and keystores are gitignored and must stay that way.
- The scratchpad, not `/tmp` and not the repo, is where logs and working files go.

---

## 9. The one deadline that matters

Devpost closes **30 Sep 2026**, and the app must be **live on Google Play** before then. Google requires 12 testers opted into a closed test continuously for 14 days before granting production access. That clock cannot be compressed and gates everything else — see PLAN §0 and `docs/release.md`. If you are choosing between tasks and one of them unblocks the Play track, choose that one.
