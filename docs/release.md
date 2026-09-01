# Release runbook

## Signing

Keep the upload keystore and its credentials **outside the repository**. The build reads these environment variables:

| Variable | Value |
|---|---|
| `SAVEPEPS_KEYSTORE` | Absolute path to the upload keystore |
| `SAVEPEPS_KEYSTORE_PASS` | Keystore password |
| `SAVEPEPS_KEYALIAS` | Upload-key alias |
| `SAVEPEPS_KEYALIAS_PASS` | Alias password |

Keep the keystore and credentials in a password manager. The keystore is not in git and never should be — `.gitignore` blocks common Android signing and service-account formats.

Because the app will be enrolled in **Play App Signing**, Google holds the actual app signing key and this is only the *upload* key. If it is lost, Google can reset it — so this is a multi-day disruption rather than the end of the app. Still worth not losing.

Build a signed bundle. `REVENUECAT_GOOGLE_PLAY_API_KEY` may be supplied in the
environment to refresh the tracked public SDK key; the build validates its
`goog_` prefix and imports it without logging the value:

```bash
set -a
. /absolute/path/to/save-peps-signing.env
set +a

UNITY=/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity
PROJ="$(git rev-parse --show-toplevel)/unity/SavePeps"
"$UNITY" \
  -batchmode -quit -nographics \
  -projectPath "$PROJ" \
  -executeMethod SavePeps.EditorTools.BuildScript.BuildAndroid \
  -logFile /tmp/aab.log
```

Output lands in `unity/SavePeps/Build/Android/SavePeps-<timestamp>.aab`. With no env vars set the build still succeeds but is **debug-signed**, and Play rejects it — the log says so explicitly.

For device testing use `BuildAndroidApk` instead; `adb install` cannot take a bundle.

## Full-game purchase configuration

The app has one store contract and rejects release builds that do not have a Google Play public SDK key:

| Item | Required value |
|---|---|
| Package name | `fan.sound.savepeps` |
| Google Play one-time product | `lifetime` |
| RevenueCat entitlement | `save_peps_pro` |
| RevenueCat current offering | `default` |
| RevenueCat package | `$rc_lifetime` (Lifetime) |
| Access | Rounds 1–10 free; Rounds 11–12 require the entitlement |

As of 24 Aug 2026, the RevenueCat project has been read back through its V2
API and contains the Play app `fan.sound.savepeps`, an active non-consumable
Play product `lifetime`, entitlement `save_peps_pro`, and a current `default`
Offering whose only package is `$rc_lifetime`. The Play and Test Store lifetime
products are attached to both the package and entitlement. The real `goog_`
public SDK key is configured in Unity.

The repository cannot prove Play Console permissions, product activation, uploaded credentials, track enrollment, or tester state. Verify these external prerequisites before a real internal-track purchase test:

1. Confirm the Play Console app exists for `fan.sound.savepeps`. Invite the project's RevenueCat service account under **Users and permissions**, scope it to Save Peps, and grant **View app information**, **View financial data**, **Manage orders and subscriptions**, **Manage store presence**, and **Release apps to testing tracks**. Do not grant production-release permission.
2. Once API access works, create and activate the Play one-time product `lifetime` through the modern `monetization.onetimeproducts` API. Use **Unlock Full Game** / **Get Rounds 11–12**, set the US base price to **$0.99**, and review the converted regional prices.
3. Upload the service-account JSON in RevenueCat under **Project Settings > Save Peps (Google Play) > Service account credentials**. Never copy this JSON into the repository.
4. Upload the signed AAB to Internal testing, add the purchase tester both to the internal track and Play Console's license testers, open the opt-in URL with that account, and install from Google Play. A sideloaded APK does not exercise the real Play purchase path.
5. Verify purchase, immediate R11/R12 access, relaunch, uninstall/reinstall, and **Restore Purchase**. Check the RevenueCat customer timeline and confirm `save_peps_pro` is active.

The current Test Store `lifetime` product is `$99.99`; that is why the
Development APK displays `$99.99`. RevenueCat does not permit changing an
existing Test Store product's price. For a `$0.99` simulated checkout, create a
new non-consumable Test Store product (for example `lifetime_099`) at `$0.99`,
replace the old Test Store product on the entitlement and `$rc_lifetime`
package, then archive the old Test Store product. This does not affect the real
Google Play product ID, which remains `lifetime`. Before Play testing, the AAB
must use the Google key and Google product; never submit a `test_…` key.

## Google Play test and release path

Devpost closes **30 Sep 2026**. The planned 16 Aug closed-test start is in the past, and the repository cannot show whether that external step happened. Check Play Console first. This release plan treats the account as subject to [Google's production-access testing requirement for qualifying personal accounts](https://support.google.com/googleplay/android-developer/answer/14151465). If the qualifying closed test is not already active, starting it is the critical path because the required tester clock cannot be recovered from code changes. Then proceed in this order:

1. **Create the app** in Play Console. Package id `fan.sound.savepeps` — **immutable after this step**. App type: Game. Free, with in-app purchases.
2. **Enrol in Play App Signing** (the default). Google generates the app signing key; our keystore stays the upload key.
3. **Upload the AAB** to Internal testing first — it is instant and shakes out any manifest or signing rejections without burning time.
4. **Promote to Closed testing** and add the project's tester group or email list with **12+ people**. Over-recruit to ~15: the requirement is 12 opted in *continuously* for 14 days, and anyone who opts out restarts their own clock.
5. Send testers the opt-in link and confirm each one actually accepts. An invited tester is not an opted-in tester.
6. **Complete the required declarations** — these block the release, not the upload:
   - Content rating questionnaire (IARC)
   - Data safety form. RevenueCat's SDK behaviour counts even though Save Peps never receives payment details: declare **Financial info > Purchase history** as collected, required, encrypted in transit, and used for **App functionality** and **Analytics**. It is not processed ephemerally. Mark it as shared only if a configured RevenueCat integration sends it to another third party.
   - Ads declaration: none.
   - Target audience: **13+**. Do not declare a child-directed audience — it pulls the app into the Designed for Families programme and adds policy work we have no room for.
   - Privacy policy URL: `https://michi883.github.io/save-peps/privacy/`. Configure GitHub Pages to publish the repository's `/docs` folder before submitting the closed release, and confirm the URL is public without a login. The generated Home screen links to the same policy and to `https://michi883.github.io/save-peps/terms/`.
7. **Apply for production access** as soon as the qualifying 14-day window closes.

## Before the production upload

- Confirm `targetSdkVersion` resolves to **API 36 or newer**, the [Google Play requirement for new mobile apps and updates since 31 August 2026](https://support.google.com/googleplay/android-developer/answer/11926878). It is set to `Auto`, which tracks the editor — inspect the built manifest rather than trusting the label, because a wrong target API is a hard rejection.
- Run the pre-launch report and fix what it flags.
- Check the bundle size against the 150 MB warning threshold Unity is configured with.

## Known-good baseline

| | |
|---|---|
| Unity | 6000.3.21f1 LTS |
| Slice APK | ~18 MB, installs and runs on a Pixel 4 (API 33, arm64-v8a) |
| Min / target SDK | 25 / Auto |
| ABI | ARM64 only, IL2CPP |
