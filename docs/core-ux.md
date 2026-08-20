# Core UX freeze

Frozen on **20 August 2026** after the P2 polish pass, extended the same day by the shell-and-feedback pass, and validated on a Pixel 4 both times. The content sprint may add rescue, round, environment, prop, and choreography data; it should not invent a second interaction or feedback pattern.

## Loop contract

`Home → Play / Choose round → rescue → failure and automatic retry or reunion → next rescue → round complete → Keep playing`

- **Home:** Play is dominant and chooses a useful available authored round. Choose round is secondary and never changes progression. One small earned line reports total saves and first-try saves, and opens Progress; it is hidden entirely until there is something to report.
- **Choice:** one tap locks input and lets the authored physical consequence play without an overlay interrupting it.
- **Wrong answer:** the Peps react, then a large one-line authored quip pops in beside the failed prop for a 1.1-second reading beat. It leaves over 0.15 seconds and the same rescue resets automatically. There is no fail state, retry button, life, timer, or penalty beyond earning ✓ instead of ★ on a newly solved rescue.
- **Success:** the authored Meet step hands off to the shared run, hug, heart, confetti, reunion sound, and success haptic. No generic success sentence covers the scene. The earned ★ or ✓ animates in the HUD, and the plaque holding it flinches with it, with a short punctuation sound.
- **Rescue transition:** after the authored outcome, the reunion rests for 1.35 seconds, then the current diorama swaps out and the next drops in.
- **Round completion:** the three actual ★/✓ shapes are the dominant reward. Perfect rounds read ★★★. Keep playing is primary; Choose round is secondary.
- **Keep playing:** the result card fades out and `RoundSelector` chooses another useful available authored round, avoiding the one just finished when possible.

## Shell contract

The player must always be able to leave a rescue without leaving the app, and progress must always be one tap from wherever they are.

- **Pause control:** one small circular pause button in the top-right of the HUD. It is live exactly when a rescue is waiting for a tap (`RescueRunner.AwaitingChoice`) and dims while an authored outcome plays — a gag is under four seconds, and suspending one would mean freezing a running choreography rather than declining to start a new one. Nothing in this game uses `Time.timeScale`.
- **Pause sheet:** a bottom sheet over the still-visible diorama, never a full screen. Resume, Progress, Choose round, Home, and two settings toggles — sound and haptics. Settings are inline rather than a sixth destination; those two switches are the whole of what there is to configure.
- **Input while a shell surface is open:** taps on the diorama are suspended for the whole visit, including a detour into Progress, and handed back exactly once on resume.
- **Progress:** derived entirely from the ★/✓ marks already in the save — rounds completed, perfect rounds, total first-try saves, and one row per round showing the same three shapes. The shelf sizes itself to the catalogue. It is read-only; choosing a round has its own screen. No currency, XP, lives, leaderboard, timer, or streak.
- **Android Back:** resolved outermost surface first — Progress closes to wherever it was opened from, the pause sheet resumes, the picker goes back, the round-complete card goes Home, a running rescue opens the pause sheet, and Home quits. Back never skips a rescue or changes progression.

## Presentation contract

- The scene remains the game. HUD chrome is limited to one status plaque holding an uppercase round label and the three mastery marks, the 2–4 word objective, and the pause control. The marks already say which rescue is in play, so the HUD never prints "rescue 2 of 3" beside them.
- **Nothing in the UI holds still.** The objective arrives with an overshoot and a slight tilt, then shrinks to a quieter resting size once it has been read; the quip is cut to the width of its own sentence and pops in and out; panels enter with `UIPop` and buttons squash under a thumb with `ToyButton`. All of it runs on unscaled time and on the same `Easing` curves as the 3D choreography. Do not add a panel that only fades.
- Cream toy-label plaques keep type readable across all diorama colours on a 1080×2280 Pixel 4.
- ★ and ✓ are code-drawn UI geometry, not font glyphs. This avoids Android fallback-font boxes and keeps the same language in the HUD, picker, progress shelf, and completion card. For the same reason, no player-facing string contains a ★ character.
- Failure copy stays affectionate and authored in `RescueDefinition`; shared runtime code never supplies a scold or generic error message. Quips are one line, at most 28 characters, in 56 px bold type on the Pixel 4 reference canvas.
- Success intensity comes from choreography, characters, sound, haptics, camera response, and mastery marks—not larger text.

## Content-sprint guardrails

- New rescues remain data-only. Do not add rescue-specific UI or MonoBehaviours.
- Reuse the shared retry, reunion, mastery, pacing, HUD, shell, and completion paths.
- Do not add timers, lives, currencies, points, streaks, or additional progression.
- New UI belongs to `UIPop`, `ToyButton`, and the existing plaque vocabulary. A new panel with its own bespoke fade is a regression, not a feature.
- Change this contract only for a demonstrated device regression, accessibility issue, or release blocker. Content preference alone is not a reason to reopen the core loop.

## Freeze verification

- Unity compile: clean.
- EditMode: 42/42, including the one-line/28-character quip contract.
- PlayMode: 7/7 — automatic retry, ✓ after retry, ★ on first tap, a full three-rescue round, Keep playing, pause suspending and restoring tap input, Progress opening from the pause sheet and returning to it, Home from pause leaving gameplay without touching progression, and settings toggles reaching both the save and the audio/haptics layer.
- Pixel 4: home with the beating heart and the earned-line chip, Play, HUD status plaque and pause control, objective announce-and-settle, a wrong answer's quip beside the failed prop, automatic reset, reunion, the mixed ✓★✓ completion card, Keep playing into a different round, the pause sheet by button and by Android Back, both settings toggles, Progress from the pause sheet and from home, and Back out of every one of them.
