# Save Peps — Round Catalogue

> **Status:** Source of truth for shipped content.
> **Scope:** 12 rounds / 36 rescues / 12 worlds / 36 stages.
> **Generated from:** `Assets/_Project/Content/` (the seeded `RescueDefinition` assets),
> `Assets/_Project/Code/Editor/Round*Rescues.cs` (authoring), and
> `Assets/_Project/Code/Editor/WorldKits.cs` (world kits and atmospheres).
> **Art policy:** `design/palette.md`. **Build/verify policy:** `AGENTS.md`.

This document describes the game as implemented. If it disagrees with the assets,
the assets win and this file is wrong — regenerate the tables from
`Content/Rescues/*.asset` before editing prose.

---

## 1. What a "world" is

A round is not a colour scheme. Every round owns a **world**, and a world is
nine authored things:

| Axis | Where it lives |
|---|---|
| World rule | The round's doc comment in `Round*Rescues.cs`, and §3 below |
| Dominant spatial geometry | `Worlds.Begin` → the world's base silhouette in `WorldKits.cs` |
| Interaction vocabulary | The `ReasoningKind` of its three rescues |
| Prop vocabulary | The nine `RescueObject.Prop` references, built in `PropLibrary.cs` |
| Movement / choreography | The `OutcomeStep` lists in `Round*Rescues.cs` |
| Atmosphere & lighting | `DioramaAtmosphere` on each stage prefab, written by `Worlds.Atmosphere` |
| Environmental animation | `AmbientMotion` components placed by `Worlds.Dress` |
| Sound character | The world's `Ambience` bed plus the round's `Sfx` cues |
| Signature payoff | The correct outcome of the round's only-here rescue |

Three rules are enforced by `ContentValidator` and cannot be reintroduced by
hand:

* **`ValidateWorlds`** — all three rescues in a round must share one
  `DioramaAtmosphere.WorldId`, and no two rounds may share a world.
* **`ValidateStagesAreUnique`** — no two rescues anywhere may use the same
  environment prefab. Thirty-six rescues means thirty-six stages.
* **`ValidateSolutionsAreUnique`** — no two rescues may share the same
  *(correct prop, reasoning kind)* pair. "Cut the thing with the scissors"
  can be asked once.

---

## 2. The twelve worlds at a glance

| # | World | World rule | Geometry | Reasoning mix | Sky / key light | Ambience | Only-here |
|:--:|---|---|---|---|---|---|:--:|
| 01 | **Garden** | Simple things do simple jobs. | Grass plinth, no horizon line; brook, path, trellis | Crossing · Activation · Cutting | Pale blue, warm sun 1.15 | `amb_garden` | r01 |
| 02 | **Clockwork courtyard** | Nothing moves until a linkage moves it. | Chequer floor under an overhead brass gantry | Counterweight · Activation · Reflection | Cream, flat sun 1.05, almost no fog | `amb_clock` | r05 |
| 03 | **Weather terrace** | Change the state of the air, not the Peps. | Three stacked terraces at 0.15 / 0.42 / 0.70 | Temperature · Growth · Shelter | **Three skies** — frost, full sun, downpour | `amb_weather` | r09 |
| 04 | **Windrock canyon** | The far rim is higher, and the air is going somewhere. | Two mesas with a visible drop between them | Airflow · Counterweight · Momentum | Cold blue sky, hard sun 1.34 on warm rock | `amb_canyon` | r12 |
| 05 | **Tidewater docks** | Everything floats or sinks, and the water is going somewhere. | Open water 2.30 × 4.10, beach at the *front* | Buoyancy · Momentum · Crossing | Bright cyan, sun 1.16, near-zero fog | `amb_tide` | r15 |
| 06 | **Storm rooftop** | The wind has a direction and it is taking things. | Narrow roof on a tower shaft that leaves frame | Airflow · Signal · Momentum | Near-black, sun 0.58, heavy fog | `amb_storm` | r17 |
| 07 | **Crystal cave** | You cannot see and you cannot reach. | The only enclosed world: walls, back wall, stalactites | Reflection · Resonance · Momentum | Ink sky, cold key + lantern-warm fill 0.66 | `amb_cave` | r20 |
| 08 | **Snowpeak slopes** | Everything rolls downhill. | A 7-step diagonal wedge with a cornice | Airflow · Momentum · Crossing | White-blue, sun 1.06, thin fog | `amb_peak` | r22 |
| 09 | **Deep ocean trench** | Down is slow, up is free, sound goes nowhere. | Trench walls and rising bubble columns | Buoyancy · Luring · Crossing | Deep teal, sun 0.62, fog 0.165 (the densest) | `amb_abyss` | r25 |
| 10 | **Orbital station** | Nothing falls and nothing stops. | **No ground** — three modules in a starfield | Momentum · Magnetism · Airflow | Black, hard white key 1.62, fill 0.10 | `amb_orbit` | r28 |
| 11 | **Foundry floor** | The machine is already running and will not wait. | Riveted deck, molten trough, overhead gantry | Activation · Temperature · Momentum | Smoke violet, **lit from below** (molten ground bounce) | `amb_forge` | r32 |
| 12 | **Neon skyline** | The city moves for you if you catch it. | Three rooftops at three heights, transit beam, lit skyline | Signal · Momentum · Crossing | Night violet, cold key + sodium fill 0.86 | `amb_neon` | r35 |

Every world has its own camera. Framing is authored on `DioramaAtmosphere`
(`CameraPitch` / `CameraDistance` / `CameraHeight` / `CameraFov`) and pushed
through `GameFeel.SetFraming` by `AtmosphereDirector`; there is no global
camera constant left. The spread runs from Orbit's detached
30° / 7.2 m to the canyon's 45° look-down into its own chasm, and the cave
takes the widest lens in the game at 34° because it is the only stage with a
ceiling to fit in.

---

## 3. The thirty-six rescues

Bold is the correct object. `Stage` is the environment prefab, one per rescue.

| # | Goal | Verb | Reasoning | Diff. | Stage | Slot 1 / 2 / 3 |
|:--:|---|---|---|:--:|---|---|
| r01 | Cross the brook. | bridge | Crossing | Easy | Garden_Brook | **plank** / bell / balloon |
| r02 | Wake the helper. | wake | Activation | Easy | Garden_Gate | pillow / bone / **bell** |
| r03 | Clear the vines. | prune | Cutting | Medium | Garden_Trellis | watering_can / **scissors** / fan |
| r04 | Raise the platform. | hoist | Counterweight | Easy | Clock_Pulley | pillow / **stone** / balloon |
| r05 | Turn the gears. | mesh | Activation | Medium | Clock_Gearwall | **gear** / rope / wrench |
| r06 | Bounce the beam. | reflect | Reflection | Medium | Clock_Optics | magnet / umbrella / **mirror** |
| r07 | Melt the ice. | thaw | Temperature | Easy | Weather_Frost | pillow / bell / **hair_dryer** |
| r08 | Reach the ledge. | sprout | Growth | Medium | Weather_Bloom | scissors / **watering_can** / balloon |
| r09 | Stay out of rain. | shelter | Shelter | Medium | Weather_Downpour | **umbrella** / fan / leaf |
| r10 | Cross the chasm. | glide | Airflow | Medium | Canyon_Updraft | fan / **umbrella** / stone |
| r11 | Stop the swinging. | plumb | Counterweight | Medium | Canyon_Cablecar | feather / rope / **weight** |
| r12 | Bring down the spire. | topple | Momentum | Surprising | Canyon_Spire | **grapple** / scissors / fan |
| r13 | Lift the punt. | bail | Buoyancy | Easy | Tide_Punt | **bucket** / stone / balloon |
| r14 | Row across the bay. | paddle | Momentum | Medium | Tide_Channel | weight / **oar** / balloon |
| r15 | Ride the current. | drift | Crossing | Surprising | Tide_Current | plank / net / **buoy** |
| r16 | Tame the tarp. | pin | Airflow | Medium | Storm_Tarp | umbrella / rope / **sandbag** |
| r17 | Stop the lightning. | ground | Signal | Surprising | Storm_Mast | **lightning_rod** / lantern / plank |
| r18 | Slide down safely. | chute | Momentum | Medium | Storm_Gutter | pillow / **plank** / stone |
| r19 | Light the cave. | kindle | Reflection | Medium | Cave_Dark | mirror / **lantern** / pickaxe |
| r20 | Ring the crystal. | ring | Resonance | Surprising | Cave_Vein | pillow / bell / **chime_crystal** |
| r21 | Free the mine cart. | hew | Momentum | Medium | Cave_Cart | **pickaxe** / rope / stone |
| r22 | Firm up the snow. | crust | Airflow | Surprising | Peak_Powder | hair_dryer / watering_can / **fan** |
| r23 | Get down the slope. | sled | Momentum | Medium | Peak_Chute | **sled** / stone / umbrella |
| r24 | Follow the rope. | traverse | Crossing | Medium | Peak_Traverse | pickaxe / **rope** / pillow |
| r25 | Rise from the floor. | rise | Buoyancy | Easy | Abyss_Floor | stone / bell / **bubble_shell** |
| r26 | Move the angler. | beckon | Luring | Medium | Abyss_Wreck | **glow_jelly** / net / scissors |
| r27 | Beat the current. | moor | Crossing | Surprising | Abyss_Current | leaf / **weight** / balloon |
| r28 | Stop the drifting. | push | Momentum | Medium | Orbit_Drift | rope / **thruster** / umbrella |
| r29 | Pull them back. | attract | Magnetism | Surprising | Orbit_Tumble | **magnet** / bell / balloon |
| r30 | Shut the airlock. | seal | Airflow | Medium | Orbit_Airlock | mirror / stone / **pillow** |
| r31 | Open the gate. | feed | Activation | Medium | Forge_Conveyor | **crate** / pillow / watering_can |
| r32 | Cool the spill. | quench | Temperature | Surprising | Forge_Spill | plank / **watering_can** / fan |
| r33 | Stop the piston. | jam | Momentum | Medium | Forge_Piston | rope / magnet / **wrench** |
| r34 | Light the sign. | power | Signal | Medium | Neon_Sign | **neon_tube** / scissors / hair_dryer |
| r35 | Catch the tram. | board | Momentum | Surprising | Neon_Transit | balloon / pillow / **zip_grip** |
| r36 | Fly the skyline. | soar | Crossing | Surprising | Neon_Skyline | plank / **balloon** / rope |

---

## 4. Per-round detail

### Round 01 — Garden

* **World rule.** Simple things do simple jobs. A flat thing spans, a loud
  thing wakes, a sharp thing cuts.
* **Geometry.** A grass plinth with no ground plane beyond it, so the world
  ends at the toy's edge. Brook across the middle, diagonal path, vertical
  trellis — three different compositions inside one world.
* **Interaction vocabulary.** Crossing, Activation, Cutting. The three verbs
  the rest of the game will subvert.
* **Props.** plank, bell, balloon, pillow, bone, scissors, watering can, fan.
  Nothing here is world-specific: that is the point of a tutorial.
* **Choreography.** Short, snapped, hop-eased. Nothing takes longer than 2.6 s.
* **Atmosphere.** Pale blue sky, warm sun at 1.15, light fill, 40° / 6.3 m.
* **Environmental animation.** `Sway` on bushes, flowers and grass tufts;
  nothing else moves. The baseline the other eleven worlds depart from.
* **Sound.** `amb_garden` (a breathing breeze over a low hum) under `slide`,
  `bell`, `snip`, `splash`, `poof`.
* **Signature payoff.** Pep A double-hopping the plank and landing in a hug.
* **Only-here rescue — r01, the plank bridge.** Every later world takes the
  plank away: the canyon is too wide, the sea takes it, the foundry burns it,
  the city is a block across. The running joke only works because it worked
  here first.

### Round 02 — Clockwork courtyard

* **World rule.** Nothing moves until a linkage moves it. You never act on a
  Pep; you act on a machine that acts on the world.
* **Geometry.** A chequered stone floor under an overhead brass frame, with
  pendulums, hanging chains and cog wheels crossing the top of frame.
* **Interaction vocabulary.** Counterweight, Activation, Reflection — three
  ways of putting something *into* a mechanism.
* **Props.** gear, wrench, magnet, mirror, stone, rope, pillow, balloon,
  umbrella. Brass and steel; the first world with a parts bin.
* **Choreography.** Two-beat: the object seats, then the machine answers
  second-hand. Nothing arrives directly.
* **Atmosphere.** Cream sky, flat sun 1.05, fog 0.02 — the clearest air in the
  game, because gearing has to read.
* **Environmental animation.** `Spin` on the idler cogs and `Sway` on the
  pendulums, staggered so no two are in phase.
* **Sound.** `amb_clock` (a tick every half second) under `ratchet`,
  `clank`, `clunk`, `chime`.
* **Signature payoff.** The gear wall meshing and driving the portcullis up.
* **Only-here rescue — r05, the missing cog.** The one puzzle whose answer is
  a spare part. It can only exist somewhere made of gears.

### Round 03 — Weather terrace

* **World rule.** You never touch a Pep or an obstacle. You change the state of
  the air over them, and the world changes back.
* **Geometry.** One hillside as three terraces, tops at exactly 0.15, 0.42 and
  0.70 — the round is read vertically.
* **Interaction vocabulary.** Temperature, Growth, Shelter. Every answer is a
  *field* applied to a place.
* **Props.** hair dryer, watering can, umbrella, leaf, fan, scissors, bell,
  pillow, balloon.
* **Choreography.** Slow states rather than events: ice shrinking, a stem
  growing, a cloud being pushed.
* **Atmosphere.** The only round whose three stages carry **different skies** —
  `frost` (pale, sun 0.98), `sun` (gold, sun 1.42), `rain` (grey, sun 0.72).
  `Worlds.ApplyWeather` writes the variant; `ValidateWorlds` still passes
  because the `WorldId` stays `weather`.
* **Environmental animation.** `Drift` snowfall and rainfall, `Sway` on the
  flowers and grass, and a `Pulse` sun shaft on the middle terrace — the
  animation itself changes with the weather.
* **Sound.** `amb_weather` under `poof`, `splash`, `pop`.
* **Signature payoff.** The rain cloud being shoved off the terrace and the
  sun arriving behind it.
* **Only-here rescue — r09, the moving rain cloud.** The cloud is a character
  with a position; both wrong answers move the *weather* rather than the Peps.

### Round 04 — Windrock canyon

* **World rule.** The gap is vertical as well as horizontal, the far rim is
  higher than the near one, and the air is going somewhere.
* **Geometry.** Two mesas with fluted, eroded walls facing each other and a
  chasm floor visible between them. The camera looks down at 45° — steeper
  than any other outdoor world — because at a shallower angle the near rim
  hides the floor and the two mesas read as one step up.
* **Interaction vocabulary.** Airflow, Counterweight, Momentum. **Nothing here
  is solved by laying something flat across the hole, and the plank is not
  offered.**
* **Props.** umbrella, weight, grapple, feather, rope, fan, stone, scissors.
* **Choreography.** Long arcs with real air time; the round's Peps are the
  first to be *carried* rather than to walk.
* **Atmosphere.** A cold blue sky and blue fog against warm rock, with a hard
  near-white sun at 1.34 from a low angle and a cool fill in the shadows. The
  complementary split is the whole read: with the gold sky it shipped with
  first, the mesas sat in the same hue and value family as the air and the
  round looked like furniture.
* **Environmental animation.** `Drift` dust plumes rising out of the chasm, a
  `Spin` bird circling with `Sway` wings, and a slow `Sway` on the cable.
* **Sound.** `amb_canyon` (a hollow wind) under `wind`, `rumble`, `creak`,
  `ratchet`.
* **Signature payoff.** The spire falling across the chasm and becoming the
  bridge.
* **Only-here rescue — r12, toppling the spire.** It needs a chasm with
  something standing in it, which exists in exactly one world.

### Round 05 — Tidewater docks

* **World rule.** Everything floats or sinks, and the water is going somewhere
  whether you like it or not.
* **Geometry.** The first world where the ground is not solid: open water
  2.30 × 4.10 running off the edge of frame, with the beach at the *front* of
  the stage rather than the back.
* **Interaction vocabulary.** Buoyancy, Momentum, Crossing.
* **Props.** bucket, oar, buoy, net, plank, weight, stone, balloon.
* **Choreography.** Everything with a hull carries a `Bob` at rest, so the
  authored motion stacks on top of a world already in motion.
* **Atmosphere.** Bright cyan sky, sun 1.16, fog 0.022 — near-zero, because
  water needs edges.
* **Environmental animation.** `Bob` on the sea and on every hull, `Drift` on
  the surface flow, and a `Spin` gull — staggered, so the bay never pulses in
  unison.
* **Sound.** `amb_tide` (a slow swell) under `splash`, `creak`, `boing`.
* **Signature payoff.** The buoy swinging across on the current with a Pep
  aboard.
* **Only-here rescue — r15, the current-swung buoy.** The only rescue solved
  by a force that was already moving before the player arrived.

### Round 06 — Storm rooftop

* **World rule.** The wind has a direction and it is taking things. Anything
  loose is already leaving, and there is nothing underneath.
* **Geometry.** A narrow roof on a tower shaft that drops out of frame. The
  most exposed composition in the game.
* **Interaction vocabulary.** Airflow, Signal, Momentum.
* **Props.** sandbag, lightning rod, plank, lantern, rope, umbrella, pillow,
  stone.
* **Choreography.** Fast and lateral. Things that fail here leave the frame
  sideways.
* **Atmosphere.** Near-black sky, sun 0.58, fog 0.095, camera dropped to 31°
  so the tower reads as height.
* **Environmental animation.** A full-frame `Drift` rain curtain, a `Sway`
  aerial, and a deterministic `Flicker` sky flash on a long duty cycle —
  deterministic so the same world always screenshots the same.
* **Sound.** `amb_storm` (gale and rain) under `wind`, `zap`, `clank`,
  `glide_hiss`.
* **Signature payoff.** The strike arriving on the rod instead of the walkway,
  and the walkway going quiet.
* **Only-here rescue — r17, the lightning rod.** The only rescue in the game
  where the answer does not remove the hazard — it redirects it.

### Round 07 — Crystal cave

* **World rule.** You cannot see and you cannot reach. Make light, make the
  right sound, or move the rock — and the cave answers back.
* **Geometry.** The **only enclosed world**: side walls, a tall back wall that
  runs above the top of frame, wall bosses, a lit crystal seam and stalactites
  hung from the back wall and the side lips. There is no flat ceiling slab —
  at 39° the camera would see its lid rather than its underside.
* **Interaction vocabulary.** Reflection, Resonance, Momentum.
* **Props.** lantern, chime crystal, pickaxe, mirror, bell, rope, pillow,
  stone.
* **Choreography.** Answers travel. Light spreads, a ring flies down the seam,
  a cart rolls the length of the tunnel — nothing resolves where it started.
* **Atmosphere.** Ink sky, cold sun 1.02 from high behind, lantern-warm fill
  0.66 from low in front, fog 0.045. Widest lens in the game at 34° / 6.75 m.
* **Environmental animation.** `Drift` water drops falling from the roof and
  a slow `Pulse` on the crystal vein. The quietest world in the game — the
  cave is meant to feel like it is waiting.
* **Sound.** `amb_cave` (a drip every 1.3 seconds over a low room tone)
  under `crystal`, `drip`, `chip`, `rumble`.
* **Signature payoff.** The vein ringing down its whole length and the rock
  curtain shivering apart.
* **Only-here rescue — r20, ringing the crystal vein.** The game's only puzzle
  about *pitch*. The bell — right in round one — is the near miss.

### Round 08 — Snowpeak slopes

* **World rule.** Everything rolls downhill. Your job is to aim it, ride it,
  or hold on.
* **Geometry.** One steep wedge of seven steps running corner to corner — the
  only diagonal ground in the game — under an overhanging cornice.
* **Interaction vocabulary.** Airflow, Momentum, Crossing.
* **Props.** fan, sled, rope, pickaxe, hair dryer, watering can, umbrella,
  stone.
* **Choreography.** The fastest in the game: r23 puts a Pep from the cornice
  to the valley in nine tenths of a second.
* **Atmosphere.** White-blue sky, sun 1.06 with a warm equator so the snow is
  not blown out, fog 0.034.
* **Environmental animation.** `Drift` spindrift blowing across the slope and
  a `Sway` on the marker flags.
* **Sound.** `amb_peak` (thin high wind) under `crunch`, `glide_hiss`,
  `chip`, `wind`.
* **Signature payoff.** The powder packing into a slab that visibly holds.
* **Only-here rescue — r22, wind-packing powder.** Heat makes it worse and
  water makes it worse; only moving air helps, which inverts everything round
  three taught.

### Round 09 — Deep ocean trench

* **World rule.** Down is slow and up is free. Nothing falls, everything
  drifts, and sound goes nowhere.
* **Geometry.** Trench walls on both sides with rising bubble columns and a
  shelf high on one wall.
* **Interaction vocabulary.** Buoyancy, Luring, Crossing.
* **Props.** bubble shell, glow jelly, weight, net, leaf, bell, scissors,
  stone, balloon.
* **Choreography.** The one round where the *timing* changes: every movement
  is longer, eased in and out rather than snapped, and arcs are shallow —
  nothing is thrown here, it is released.
* **Atmosphere.** Deep teal, sun 0.62 from directly above, fog 0.165 — the
  densest in the game.
* **Environmental animation.** `Drift` bubble columns and trench flow, `Sway`
  kelp, `Spin` fish and a `Pulse` on the wall seam — the busiest ambient set
  in the game, because still water would read as empty.
* **Sound.** `amb_abyss` (a low pressure hum) under `bubble`, `sonar`,
  `clunk`. The bell lands as a dull knock: the round teaches its physics in a
  wrong answer.
* **Signature payoff.** Four seconds of a Pep going gently upward.
* **Only-here rescue — r25, the slow buoyant rise.** The answer is a shell of
  trapped air, and the whole beat is the rise itself.

### Round 10 — Orbital station

* **World rule.** Nothing falls and nothing stops. Every push lasts forever,
  and you have to push off something.
* **Geometry.** The **only world with no ground**: three hull modules hanging
  in a starfield with real gaps between them, plus a slowly spinning ring.
* **Interaction vocabulary.** Momentum, Magnetism, Airflow.
* **Props.** thruster, magnet, pillow, mirror, rope, bell, stone, balloon,
  umbrella.
* **Choreography.** The inverse of every other round: arcs become straight
  lines, easing becomes linear, and anything launched simply keeps going.
* **Atmosphere.** Black sky, one hard white key at 1.62, fill 0.10 so the
  shadow side is genuinely black, camera detached at 30° / 7.2 m.
* **Environmental animation.** `Spin` on the station ring, the starfield and
  the debris, a linear `Drift` stream, and a `Flicker` beacon. Everything
  rotates; nothing bobs.
* **Sound.** `amb_orbit` (a quiet air handler — deliberately unnormalised so
  it stays quiet) under `thrust`, `servo`, `hiss`, `snap_on`.
* **Signature payoff.** One puff of gas, and a Pep travelling in a dead
  straight line for two full seconds.
* **Only-here rescue — r28, the gas thruster.** The only rescue solved by
  adding velocity to a Pep rather than changing the world between them, and it
  only works where nothing slows them down.

### Round 11 — Foundry floor

* **World rule.** The machine is already running and it will not wait for you.
  Feed it, cool it, or jam it.
* **Geometry.** A riveted deck with a molten trough running *through* it and a
  gantry overhead.
* **Interaction vocabulary.** Activation, Temperature, Momentum.
* **Props.** crate, watering can, wrench, magnet, rope, plank, pillow, fan.
* **Choreography.** The round starts in motion. Conveyor slats scroll, the
  press hammers on a beat and steam vents on its own clock before the player
  has touched anything.
* **Atmosphere.** Smoke-violet sky and — uniquely — **light from underneath**:
  the ambient ground colour is molten gold, so every silhouette carries a hot
  rim. Key light is dimmed to 0.72 to let the trough do the work.
* **Environmental animation.** `Drift` conveyor slats (one component driving
  twelve staggered children), `Drift` steam, a `Bob` crane trolley, a `Beat`
  press and a `Pulse` on the melt.
* **Sound.** `amb_forge` (a press hammering on a one-second beat over a
  55 Hz floor) under `sizzle`, `hiss`, `clank`, `snap_on`.
* **Signature payoff.** A river of molten metal going black and solid under
  water, and the Peps walking over it.
* **Only-here rescue — r32, quenching the spill.** Water is a gardening tool
  in round three and a hazard in round eight; here it is the only thing in the
  building that can make a floor.

### Round 12 — Neon skyline

* **World rule.** The city moves for you if you catch it at the right moment.
  Everything is fast, lit, and a long way up.
* **Geometry.** Three rooftops at three heights, a transit beam crossing the
  frame, and a lit skyline behind — the most vertical composition in the game.
* **Interaction vocabulary.** Signal, Momentum, Crossing.
* **Props.** neon tube, zip grip, balloon, scissors, hair dryer, pillow,
  plank, rope.
* **Choreography.** Fast, and the only round that asks the player to act on
  something already crossing the frame.
* **Atmosphere.** Night violet, cold key 0.96 and a sodium-orange fill at 0.86
  — a deliberately two-colour world, because one colour is just night. Window
  grids on every tower carry the sparkle.
* **Environmental animation.** A `Drift` skyline tram, three `Flicker` side
  signs on different duty cycles, and a `Pulse` city glow.
* **Sound.** `amb_neon` (a three-note transformer chord under traffic noise)
  under `neon`, `transit`, `zap`, `chime`.
* **Signature payoff.** The last gap in the game crossed by balloon, over a
  whole lit city block.
* **Only-here rescue — r35, catching the tram.** The only rescue whose
  obstacle is *timing*, and the only one whose wrong answers fail for being
  too slow rather than too weak.

---

## 5. Cross-round duplicate risk

The old catalogue's failure mode was invisible per-asset and obvious only side
by side: fourteen dioramas shared one skeleton and one camera, rounds 1–8
reshuffled ten environments, and eight of thirty-six rescues were the same
*(diorama + prop + choreography)* asked twice. Three of the four guards are
now mechanical.

| Risk | Guard | Kind |
|---|---|:--:|
| Two rounds looking like the same place | `ValidateWorlds` — one `WorldId` per round, no world reused | error |
| Two rescues staged on the same geometry | `ValidateStagesAreUnique` — 36 rescues, 36 environment prefabs | error |
| The same idea asked twice | `ValidateSolutionsAreUnique` — *(correct prop, reasoning kind)* is unique | error |
| Two adjacent rescues reasoning alike | `ValidateAdjacentReasoning` — no repeat across a round boundary | error |
| One round's three answers being the same prop | `ValidateRound` — no shared correct object within a round | error |
| Answers clustering in one slot | `ValidateAnswerPositions` — the three answers must not all sit in one slot | error |
| A prop meaning different things in different worlds | `ValidateProtean` | warning |
| Two rounds *feeling* alike | The nine axes in §1 and the contact sheet | human |

**The contact sheet is the human half of that table.**
`Tools > Save Peps > Render Stage Contact Sheet` renders every rescue's opening
frame at 540 × 1140 in its own world's light and framing, which is the cheapest
version of the acceptance test: hide the HUD, look at one frame, know which
round it is. From the command line it must **not** be given `-nographics` — that
forces a Null GfxDevice and every PNG comes back uniform grey with no error in
the log. It does not replace the device pass in `AGENTS.md` §6.

### Deliberate echoes

Repetition that is authored on purpose, and would be a bug to "fix":

* **The balloon.** Wrong in `r01` — *"Up is not across."* — and the answer in
  `r36`, where in a city it is exactly both. It is also wrong in r04, r08, r13,
  r14, r27, r29 and r35, so the callback lands on a long-running joke.
* **The bell.** The answer in `r02`, then the near miss in `r20` (wrong note)
  and `r25` (water eats the sound) and `r29` (space took the sound). The same
  object, three different physics lessons.
* **Water.** A tool in r08, a hazard in r22, and the only floor-maker in r32.
* **The umbrella.** Shelter in r09, a glider in r10, and a liability in r16
  and r28.

---

## 6. Regenerating this document

```bash
UNITY=/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity
P=/absolute/path/to/unity/SavePeps

$UNITY -batchmode -quit -nographics -projectPath $P \
  -executeMethod SavePeps.EditorTools.PrototypeArt.Generate     -logFile art.log
$UNITY -batchmode -quit -nographics -projectPath $P \
  -executeMethod SavePeps.EditorTools.ContentSeeder.ReseedFromMenu -logFile seed.log
$UNITY -batchmode -quit -nographics -projectPath $P \
  -executeMethod SavePeps.EditorTools.ContentValidator.ValidateFromMenu -logFile check.log

# note: no -nographics here
SAVEPEPS_SHEET_DIR=./sheet $UNITY -batchmode -quit -projectPath $P \
  -executeMethod SavePeps.EditorTools.StageContactSheet.Render   -logFile sheet.log
```

The exit code lies — grep each log for `error CS` and `Aborting batchmode`.
`ReseedFromMenu` is the sanctioned destructive path and preserves asset GUIDs
through `ContentSeeder.LegacyRescueNames`; `Catalog.FreeRoundCount` (10) is the
release-week paywall lever and is guarded by tests. §3's table is generated
directly from `Content/Rescues/*.asset` — regenerate it rather than hand-editing.
