# Core UX freeze

Frozen on **20 August 2026** after the P2 polish pass and Pixel 4 validation. The content sprint may add rescue, round, environment, prop, and choreography data; it should not invent a second interaction or feedback pattern.

## Loop contract

`Home → Play / Choose round → rescue → failure and automatic retry or reunion → next rescue → round complete → Keep playing`

- **Home:** Play is dominant and chooses a useful available authored round. Choose round is secondary and never changes progression.
- **Choice:** one tap locks input and lets the authored physical consequence play without an overlay interrupting it.
- **Wrong answer:** the Peps react, then a large one-line authored quip appears beside the failed prop for a 1.1-second reading beat. It leaves over 0.16 seconds and the same rescue resets automatically. There is no fail state, retry button, life, timer, or penalty beyond earning ✓ instead of ★ on a newly solved rescue.
- **Success:** the authored Meet step hands off to the shared run, hug, heart, confetti, reunion sound, and success haptic. No generic success sentence covers the scene. The earned ★ or ✓ animates in the HUD with a short punctuation sound.
- **Rescue transition:** after the authored outcome, the reunion rests for 1.35 seconds, then the current diorama swaps out and the next drops in.
- **Round completion:** the three actual ★/✓ shapes are the dominant reward. Perfect rounds read ★★★. Keep playing is primary; Choose round is secondary.
- **Keep playing:** the result card fades out and `RoundSelector` chooses another useful available authored round, avoiding the one just finished when possible.

## Presentation contract

- The scene remains the game. HUD chrome is limited to an uppercase round/rescue label, three mastery marks, and the 2–4 word objective.
- Cream toy-label plaques keep type readable across all diorama colours on a 1080×2280 Pixel 4.
- ★ and ✓ are code-drawn UI geometry, not font glyphs. This avoids Android fallback-font boxes and keeps the same language in the HUD, picker, and completion card.
- Failure copy stays affectionate and authored in `RescueDefinition`; shared runtime code never supplies a scold or generic error message. Quips are one line, at most 28 characters, in 56 px bold type on the Pixel 4 reference canvas.
- Success intensity comes from choreography, characters, sound, haptics, camera response, and mastery marks—not larger text.

## Content-sprint guardrails

- New rescues remain data-only. Do not add rescue-specific UI or MonoBehaviours.
- Reuse the shared retry, reunion, mastery, pacing, HUD, and completion paths.
- Do not add timers, lives, currencies, points, streaks, or additional progression.
- Change this contract only for a demonstrated device regression, accessibility issue, or release blocker. Content preference alone is not a reason to reopen the core loop.

## Freeze verification

- Unity compile: clean.
- EditMode: 42/42, including the one-line/28-character quip contract.
- PlayMode: 3/3, including automatic retry, ✓ after retry, ★ on first tap, a full three-rescue round, and Keep playing.
- Pixel 4: home, picker, HUD, low- and high-action wrong quips, automatic reset, reunion, perfect ★★★ card, mixed ★✓★ card, and Keep playing into a different round were exercised with synthetic taps.
