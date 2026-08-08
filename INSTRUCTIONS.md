We are planning a new Android game for RevenueCat Shipaton 2026 (https://revenuecat-shipaton-2026.devpost.com/).

Project working title: Save Peps

Do not start implementing yet. First inspect the existing codebase and produce a high-level implementation plan, proposed architecture, major milestones, risks, and any decisions that should be made before coding.

## Objective

Build a polished, simple, mobile-first 3D puzzle game for Android.

The game is an evolution of the existing Save Pip concept (https://github.com/michi883/save-pip), but it should feel like a new native mobile game rather than a direct port of the Reddit version.

The core concept is:

"Save Peps is a one-tap 3D puzzle game where a lovely couple has been separated by some small predicament. The player taps the right object to reunite them. Wrong choices trigger funny consequences instead of punishing the player."

The experience should be immediately understandable without a tutorial.

## Core gameplay

Each rescue presents:

- Two Peps who are separated from each other
- A small visual predicament preventing them from reuniting
- A fixed-camera 3D or 2.5D diorama-like scene
- Three clearly tappable objects in the scene
- Exactly one object that solves the situation
- Two wrong objects that produce short, funny visual outcomes

The player makes one tap.

If correct:
- The selected object changes the scene
- The Peps are reunited
- A short celebratory animation plays
- The player proceeds

If incorrect:
- A short humorous consequence plays
- The Peps remain separated
- The player can immediately retry
- There should be no harsh failure state

The design principle from Save Pip should remain:

Wrong answers are part of the entertainment, not punishment.

## Round structure

A round consists of exactly 3 rescues.

The game should make this structure visually clear without unnecessary UI complexity.

For example:

Round 4
Rescue 1 of 3
Rescue 2 of 3
Rescue 3 of 3

Completing all three finishes the round and advances progression.

We can retain the idea of recognizing first-tap solutions, such as a star or "perfect" result, but progression should not depend on getting the answer right on the first attempt.

## Content philosophy

Do not simply reskin the same puzzle repeatedly.

Different rescues should require different kinds of reasoning, such as:

- crossing a gap
- moving an obstacle
- waking or distracting something
- manipulating simple physics
- using weather or environmental conditions
- choosing the safe path
- recognizing a trap or visual clue
- changing something about one of the Peps

The existing Save Pip catalog can be used as inspiration and source material, but the new game should select and redesign the strongest concepts for a 3D mobile presentation.

Do not attempt to reproduce all 106 existing rescues for the first release.

Prioritize a smaller number of highly polished rescues over a very large catalog.

## Visual direction

The game should feel:

- cute
- warm
- playful
- minimal
- polished
- easy to read on a phone

Prefer simple stylized 3D over technically ambitious 3D.

The intended presentation is closer to small toy-like dioramas than a freely navigable 3D world.

Prefer:

- fixed camera
- compact scenes
- simple geometry
- expressive character animation
- strong visual clues
- satisfying object motion
- subtle sound
- haptics where useful

Avoid:

- player-controlled camera
- walking controls
- inventory systems
- dialogue trees
- complicated HUDs
- long onboarding
- unnecessary menus

A rescue should be understandable within a few seconds.

## Peps

"Peps" refers to the lovely couple at the center of the game.

They should have enough personality and animation that the player wants to reunite them, but the game should communicate mostly visually rather than through dialogue.

Their reactions should contribute to the humor.

Success should feel affectionate and satisfying.

Failure should feel funny and harmless.

## Monetization

Keep monetization extremely simple.

Rounds 1 through 10 are completely free.

Since each round contains 3 rescues, a player gets 30 rescues before reaching the premium gate.

After completing the free content, attempting to access Round 11 or later should present the subscription paywall.

Subscription entitlement:

peps_unlimited

An active subscriber can access all existing and future rounds.

There should be no complicated currency, consumables, energy system, or multiple subscription tiers for the initial release.

RevenueCat should manage subscription entitlement state.

The subscription proposition should be something close to:

"Peps Unlimited: Unlock every round, including new rounds as they are added."

Include a Restore Purchases path.

## Technical direction

Current preferred stack:

- Unity 6
- C#
- URP
- Android as the initial target
- RevenueCat Unity SDK
- Google Play subscription
- No backend unless there is a concrete need for one

Do not introduce infrastructure simply because it might be useful later.

Keep the first release as self-contained as practical.

## Puzzle architecture

One important planning goal is to avoid writing completely custom game logic for every rescue.

The existing Save Pip project uses declarative puzzle specifications for much of its content. Preserve that philosophy where practical.

Ideally, a rescue can describe things such as:

- scene/environment
- Peps positions
- objective
- three interactive objects
- correct object
- success sequence
- wrong outcome A
- wrong outcome B

The implementation does not have to copy Save Pip's architecture, but planning should explore how to create a reusable system that makes new rescues inexpensive to author.

Individual scenes may still need custom animation or behavior when that meaningfully improves the experience.

## MVP priorities

Prioritize, in this order:

1. The one-tap rescue interaction feels great
2. Peps are charming and immediately understandable
3. Correct and incorrect choices have satisfying animations
4. Several genuinely different puzzle types work
5. The 3-rescue round loop feels polished
6. Progression between rounds works cleanly
7. The first 10 rounds are freely playable
8. RevenueCat subscription correctly unlocks later rounds
9. Android build is stable and ready for Google Play
10. Overall visual and audio polish

Avoid adding secondary systems until this core loop is strong.

## Hackathon positioning

The primary Shipaton category to optimize for is Best Game.

A strong secondary fit is the RevenueCat Design Award.

Therefore, optimize for:

- immediately understandable gameplay
- charm
- art direction
- animation quality
- mobile polish
- an appropriate monetization model
- a strong two-minute demo experience

Do not optimize for technical complexity for its own sake.

## Planning request

Before coding, inspect the repository and return a concrete development plan covering:

1. Recommended project structure
2. What can conceptually be reused from Save Pip
3. What should be rebuilt specifically for Unity and 3D
4. Proposed reusable rescue/puzzle architecture
5. Scene and animation architecture
6. Round and progression model
7. Save-state approach
8. RevenueCat subscription integration
9. Paywall flow
10. Suggested initial number of rescues and rounds
11. Asset and 3D-content strategy
12. Android and Google Play release path
13. Testing strategy
14. Major technical risks
15. A phased implementation sequence from prototype to store-ready release

Keep scope aggressively controlled.

The goal is not to build the largest game possible. The goal is to ship a small, delightful, highly polished one-tap puzzle game that feels complete.