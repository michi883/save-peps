# Release runbook

## Signing

The upload keystore lives **outside the repo**, at `~/.savepeps/`:

| File | What |
|---|---|
| `~/.savepeps/upload-keystore.jks` | The upload key. Alias `upload`, RSA 2048, valid 10 000 days. |
| `~/.savepeps/credentials.env` | The passwords, `chmod 600`. |

Put both in a password manager now. The keystore is not in git and never should be — `.gitignore` blocks `*.jks` and `*.keystore`.

Because the app will be enrolled in **Play App Signing**, Google holds the actual app signing key and this is only the *upload* key. If it is lost, Google can reset it — so this is a multi-day disruption rather than the end of the app. Still worth not losing.

Build a signed bundle. If the tracked RevenueCat settings have not yet been
updated, `REVENUECAT_GOOGLE_PLAY_API_KEY` may be supplied in the environment;
the build validates its `goog_` prefix and imports it without logging the key:

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

Before a real internal-track purchase can work:

1. Confirm the Play Console app exists for `fan.sound.savepeps`. Invite `save-peps-revenuecat@coding-490122.iam.gserviceaccount.com` under **Users and permissions**, scope it to Save Peps, and grant **View app information**, **View financial data**, **Manage orders and subscriptions**, **Manage store presence**, and **Release apps to testing tracks**. Do not grant production-release permission.
2. Once API access works, create and activate the Play one-time product `lifetime` through the modern `monetization.onetimeproducts` API. Use **Unlock Full Game** / **Get Rounds 11–12**, set the US base price to **$0.99**, and review the converted regional prices.
3. Upload `/Users/michi/.savepeps/revenuecat-play-service-account.json` in RevenueCat under **Project Settings > Save Peps (Google Play) > Service account credentials**. The public V2 API does not expose credential upload. Never copy this JSON into the repository.
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

## First upload — the P1 critical path

The closed test must be **live by 16 Aug** for the 14-day clock to finish in time. Do these in order.

1. **Create the app** in Play Console. Package id `fan.sound.savepeps` — **immutable after this step**. App type: Game. Free, with in-app purchases.
2. **Enrol in Play App Signing** (the default). Google generates the app signing key; our keystore stays the upload key.
3. **Upload the AAB** to Internal testing first — it is instant and shakes out any manifest or signing rejections without burning time.
4. **Promote to Closed testing** and use the Google Group `save-peps@googlegroups.com` for **12+ people**. Over-recruit to ~15: the requirement is 12 opted in *continuously* for 14 days, and anyone who opts out restarts their own clock.
5. Send testers the opt-in link and confirm each one actually accepts. An invited tester is not an opted-in tester.
6. **Complete the required declarations** — these block the release, not the upload:
   - Content rating questionnaire (IARC)
   - Data safety form. RevenueCat's SDK behaviour counts even though Save Peps never receives payment details: declare **Financial info > Purchase history** as collected, required, encrypted in transit, and used for **App functionality** and **Analytics**. It is not processed ephemerally. Mark it as shared only if a configured RevenueCat integration sends it to another third party.
   - Ads declaration: none.
   - Target audience: **13+**. Do not declare a child-directed audience — it pulls the app into the Designed for Families programme and adds policy work we have no room for.
   - Privacy policy URL: `https://michi883.github.io/save-peps/privacy/`. Configure GitHub Pages to publish the repository's `/docs` folder before submitting the closed release, and confirm the URL is public without a login. The generated Home screen links to the same policy and to `https://michi883.github.io/save-peps/terms/`.
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
