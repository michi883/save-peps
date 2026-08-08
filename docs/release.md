# Release runbook

## Signing

The upload keystore lives **outside the repo**, at `~/.savepeps/`:

| File | What |
|---|---|
| `~/.savepeps/upload-keystore.jks` | The upload key. Alias `upload`, RSA 2048, valid 10 000 days. |
| `~/.savepeps/credentials.env` | The passwords, `chmod 600`. |

Put both in a password manager now. The keystore is not in git and never should be — `.gitignore` blocks `*.jks` and `*.keystore`.

Because the app will be enrolled in **Play App Signing**, Google holds the actual app signing key and this is only the *upload* key. If it is lost, Google can reset it — so this is a multi-day disruption rather than the end of the app. Still worth not losing.

Build a signed bundle:

```bash
set -a; . ~/.savepeps/credentials.env; set +a
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit -nographics \
  -projectPath unity/SavePeps \
  -executeMethod SavePeps.EditorTools.BuildScript.BuildAndroid \
  -logFile /tmp/aab.log
```

Output lands in `unity/SavePeps/Build/Android/SavePeps-<timestamp>.aab`. With no env vars set the build still succeeds but is **debug-signed**, and Play rejects it — the log says so explicitly.

For device testing use `BuildAndroidApk` instead; `adb install` cannot take a bundle.

## First upload — the P1 critical path

The closed test must be **live by 16 Aug** for the 14-day clock to finish in time. Do these in order.

1. **Create the app** in Play Console. Package id `fan.sound.savepeps` — **immutable after this step**. App type: Game. Free, with in-app purchases.
2. **Enrol in Play App Signing** (the default). Google generates the app signing key; our keystore stays the upload key.
3. **Upload the AAB** to Internal testing first — it is instant and shakes out any manifest or signing rejections without burning time.
4. **Promote to Closed testing** and create a tester list of **12+ people**. Over-recruit to ~15: the requirement is 12 opted in *continuously* for 14 days, and anyone who opts out restarts their own clock.
5. Send testers the opt-in link and confirm each one actually accepts. An invited tester is not an opted-in tester.
6. **Complete the required declarations** — these block the release, not the upload:
   - Content rating questionnaire (IARC)
   - Data safety form. It must agree with `docs/privacy.md`: no data collected by us; purchase data processed by RevenueCat and Google.
   - Ads declaration: none.
   - Target audience: **13+**. Do not declare a child-directed audience — it pulls the app into the Designed for Families programme and adds policy work we have no room for.
   - Privacy policy URL: the GitHub Pages address for `docs/privacy.md`.
7. **Apply for production access** the moment the 14-day window closes (~30 Aug if the test starts 16 Aug).

## Before the production upload

- Confirm `targetSdkVersion` resolves to whatever Play currently requires for new apps. It is set to `Auto`, which tracks the editor — verify the number rather than trusting it, because a wrong target API is a hard rejection.
- Run the pre-launch report and fix what it flags.
- Check the bundle size against the 150 MB warning threshold Unity is configured with.

## Known-good baseline

| | |
|---|---|
| Unity | 6000.3.21f1 LTS |
| Slice APK | ~18 MB, installs and runs on a Pixel 4 (API 33, arm64-v8a) |
| Min / target SDK | 24 / Auto |
| ABI | ARM64 only, IL2CPP |
