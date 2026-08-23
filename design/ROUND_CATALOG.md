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
| 04 | **Windrock canyon** | The far rim is higher, and the air is going somewhere. | Local thermal, diagonal cableway, fractured monolith over a deep chasm | Airflow · Counterweight · Momentum | Cold blue sky, hard sun 1.34 on warm rock | `amb_canyon` | r12 |
| 05 | **Tidewater docks** | Everything floats or sinks, and the water is going somewhere. | Local punt, two-gate lock, low-tide harbor and barrage | Buoyancy · Momentum · Crossing | Bright cyan, sun 1.16, near-zero fog | `amb_tide` | r15 |
| 06 | **Storm rooftop** | The wind has a direction and it is taking things. | Narrow roof on a tower shaft that leaves frame | Airflow · Signal · Momentum | Near-black, sun 0.58, heavy fog | `amb_storm` | r17 |
| 07 | **Crystal cave** | You cannot see and you cannot reach. | The only enclosed world: walls, back wall, stalactites | Reflection · Resonance · Momentum | Ink sky, cold key + lantern-warm fill 0.66 | `amb_cave` | r20 |
| 08 | **Snowpeak slopes** | Everything rolls downhill. | Local drift, gated banked run, fractured summit / avalanche fan | Airflow · Momentum · Crossing | White-blue, sun 0.82, thin fog | `amb_peak` | r22 |
| 09 | **Deep ocean trench** | Down is slow, up is free, sound goes nowhere. | Trench walls and rising bubble columns | Buoyancy · Luring · Crossing | Deep teal, sun 0.62, fog 0.165 (the densest) | `amb_abyss` | r25 |
| 10 | **Orbital station** | Nothing falls and nothing stops. | **No ground** — three modules in a starfield | Momentum · Magnetism · Airflow | Black, hard white key 1.62, fill 0.10 | `amb_orbit` | r28 |
| 11 | **Foundry floor** | The machine is already running and will not wait. | Riveted deck, molten trough, overhead gantry | Activation · Temperature · Momentum | Smoke violet, **lit from below** (molten ground bounce) | `amb_forge` | r32 |
| 12 | **Neon skyline** | The city moves for you if you catch it. | Three rooftops at three heights, transit beam, lit skyline | Signal · Momentum · Crossing | Night violet, cold key + sodium fill 0.86 | `amb_neon` | r35 |

Every world has its own camera. Framing is authored on `DioramaAtmosphere`
(`CameraPitch` / `CameraDistance` / `CameraHeight` / `CameraFov`) and pushed
through `GameFeel.SetFraming` by `AtmosphereDirector`; there is no global
camera constant left. The spread runs from Orbit's detached
30° / 7.2 m to the canyon's 46.5° local look-down, with its climax backing out
to 37.5° / 7.35 m to hold the geological silhouette. The cave
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
| r11 | Stabilise the cableway. | plumb | Counterweight | Medium | Canyon_Cablecar | feather / rope / **weight** |
| r12 | Reshape the canyon. | topple | Momentum | Surprising | Canyon_Spire | **grapple** / scissors / fan |
| r13 | Lift the punt. | bail | Buoyancy | Easy | Tide_Punt | **bucket** / stone / balloon |
| r14 | Navigate the lock. | paddle | Momentum | Medium | Tide_Channel | weight / **oar** / balloon |
| r15 | Release the tide. | drift | Crossing | Surprising | Tide_Current | plank / net / **buoy** |
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

## 4. Per-round detail & escalation arcs

Every round is built as a three-act miniature adventure following a strict progression:
1. **Rescue 1 — INTRODUCE:** Simplest expression of the world rule, smaller/local obstacle, restrained choreography, quickly establishing what is special about this world.
2. **Rescue 2 — EXPAND:** Larger or more spatially interesting problem, more moving/reacting elements, stronger cause → effect chain, deeper use of the world's physical rule.
3. **Rescue 3 — CLIMAX:** Biggest visual consequence in the round, environment itself participates, strongest choreography/camera/game-feel moment, memorable payoff that could only happen in this world.

---

### Round 01 — Garden

* **World rule.** Simple things do simple jobs. A flat thing spans, a loud thing wakes, a sharp thing cuts.
* **Geometry.** A grass plinth with no ground plane beyond it, so the world ends at the toy's edge. Brook across the middle, diagonal path, vertical trellis — three different compositions inside one world.
* **Interaction vocabulary.** Crossing, Activation, Cutting. The three foundational verbs the rest of the game builds upon.
* **Props.** plank, bell, balloon, pillow, bone, scissors, watering can, fan.
* **Choreography.** Snapped, hop-eased toy timing.
* **Atmosphere.** Pale blue sky, warm sun at 1.15, light fill, 40° / 6.3 m.
* **Environmental animation.** `Sway` on bushes, flowers and grass tufts.
* **Sound.** `amb_garden` under `slide`, `bell`, `snip`, `splash`, `poof`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r01: Brook (`bridge` / Crossing):** Local, intimate obstacle. A simple plank spans a narrow babbling stream, demonstrating 1-to-1 direct interaction and establishing the crisp toy physics of the garden.
  * **Rescue 2 (Expansion) — r02: Gate (`wake` / Activation):** Diagonal spatial layout with an indirect cause-and-effect chain. The bell rings, waking the sleeping garden helper (shedding its sleep mask and Zzzs), who hops to the lever and pulls it, cranking the garden gate open.
  * **Rescue 3 (Climax) — r03: Trellis (`prune` / Cutting):** Monumental vertical overgrowth. Scissors snip the central root knot, triggering an environmental chain reaction: the towering vine wall collapses, garden flowers bloom open in celebration, clearing the full archway for an energetic, joyful reunion.
* **Player Escalation Experience:** Small discovery (bridging a trickle of water) → Growing situation (waking an intermediary helper to operate machinery) → Memorable climax (demolishing an overgrown garden wall to unite the Peps in full sunlight).

---

### Round 02 — Clockwork courtyard

* **World rule.** Nothing moves until a linkage moves it. You never act on a Pep; you act on a machine that acts on the world.
* **Geometry.** Chequered stone floor under an overhead brass frame, with pendulums, hanging chains and cog wheels crossing the top of frame.
* **Interaction vocabulary.** Counterweight, Activation, Reflection. Three ways of inserting a component *into* a mechanical system.
* **Props.** gear, wrench, magnet, mirror, stone, rope, pillow, balloon, umbrella. Brass and steel parts bin.
* **Choreography.** Strict two-beat mechanical response: the part seats, then the mechanism drives the world.
* **Atmosphere.** Cream sky, flat sun 1.05, fog 0.02 — crystal-clear mechanical visibility.
* **Environmental animation.** `Spin` on idler cogs and `Sway` on pendulums.
* **Sound.** `amb_clock` under `ratchet`, `clank`, `clunk`, `chime`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r04: Pulley (`hoist` / Counterweight):** Single vertical lift problem. Dropping mass into the counterweight tray directly turns the single overhead pulley wheel and lifts the platform out of the pit.
  * **Rescue 2 (Expansion) — r05: Gearwall (`mesh` / Activation):** Multi-gear mechanical train across the rear wall. Inserting the missing middle cog meshes the drive gear to the output cog, spinning the flyball governor up, which disengages the ratchet pawl and hoists the heavy portcullis.
  * **Rescue 3 (Climax) — r06: Optics (`reflect` / Reflection):** Complex optical-mechanical synthesis. Positioning the hand mirror bounces the focused beam across the courtyard into the optical receiver; sensor glows, overhead pendulums surge, and the 6-blade iris aperture gate spirals open with radial force and camera impact.
* **Player Escalation Experience:** Direct 1:1 mechanical lift → Multi-gear interlocking machine reaction → Grand optical-clockwork aperture unlocking the courtyard.

---

### Round 03 — Weather terrace

* **World rule.** You never touch a Pep or an obstacle. You change the state of the air over them, and the world changes back.
* **Geometry.** Three deliberately different compositions on a three-elevation hillside: a tight frozen top shelf, an empty diagonal climb from a dry basin to a remote ledge, and a full-frame storm channel splitting all three terraces.
* **Interaction vocabulary.** Temperature, Growth, Shelter. Every answer is a *field* applied to a region.
* **Props.** hair dryer, watering can, umbrella, leaf, fan, scissors, bell, pillow, balloon.
* **Choreography.** Causal state changes at three scales: a shell melts, a living causeway grows, then wind drives drainage machinery that reshapes the whole hillside.
* **Atmosphere.** **Three distinct skies:** `frost` (pale cold), `sun` (golden warmth), `rain` (grey mist).
* **Environmental animation.** `Drift` snowfall, flood current, rain and full-width gusts; `Sway` storm trees; `Spin` drainage wheel and weather vane; controlled shutdowns during the climax.
* **Sound.** `amb_weather` under `splash`, `crunch`, `rumble`, `ratchet`, `wind`, `chime` and `reunion`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r07: Frost (`thaw` / Temperature): LOCAL EVENT.** Applying heat shrinks only Pep B's ice shell into a puddle. One nearby Pep makes one short hop; the terrain, silhouette and wider weather remain unchanged.
  * **Rescue 2 (Expansion) — r08: Bloom (`sprout` / Growth): LANDSCAPE EVENT.** Watering the dry root heaves the basin, grows a three-run vine spine across every elevation, raises three leaf steps and a crown on the opposite ledge, then sends Pep B over the new landscape in four distinct hops.
  * **Rescue 3 (Climax) — r09: Downpour (`shelter` / Shelter): WORLD EVENT.** The opened umbrella catches the gale beside an oversized drainage wheel. The wheel turns, the sluice gate rises, flood motion dies, debris and the awning wash off-screen, rain/gust/tree motion settles, and the storm cloud leaves the frame. Both muddy banks are replaced by raised green terrain, the full-height torrent narrows to a stream, four causeway stones emerge, and sunbeams, meadow growth and a rainbow recompose the scene before Pep B makes the longest four-part crossing of the round.
* **Player Escalation Experience:** Melting one local shell → Growing a route across the landscape → Mechanically draining and rebuilding an entire storm world into a new traversable state.

---

### Round 04 — Windrock canyon

* **World rule.** The gap is vertical as well as horizontal, the far rim is higher than the near one, and the air is going somewhere.
* **Geometry.** Two fluted red-rock mesas separated by a deep chasm, reframed per rescue: a tight thermal pocket, a broad diagonal cableway, then an asymmetrical three-part rock skyline.
* **Interaction vocabulary.** Airflow, Counterweight, Momentum. Planks are not offered; gravity and air rule the void.
* **Props.** umbrella, weight, grapple, feather, rope, fan, stone, scissors.
* **Choreography.** High-airtime trajectories and heavy kinetic impacts.
* **Atmosphere.** Cold blue sky against warm sandstone, hard 1.34 sun, cool shadows.
* **Environmental animation.** `Drift` canyon dust and directional wind ribbons, `Spin` soaring hawk, controlled `Sway` on the suspended car.
* **Sound.** `amb_canyon` under `wind`, `rumble`, `creak`, `ratchet`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r10: Updraft (`glide` / Airflow): LOCAL EVENT.** One compact thermal pocket pulses as the umbrella lifts Pep A in one clean arc. Neither rim nor the wider canyon changes.
  * **Rescue 2 (Expansion) — r11: Cablecar (`plumb` / Counterweight): LANDSCAPE / SYSTEM EVENT.** Ballast drops into the cradle, both towers flex under load, the lower rig descends, broad crosswind ribbons settle, two sagging cable runs pull taut and the unsafe swinging car becomes stable. Pep A boards and rides the full diagonal span from low near rim to high far rim.
  * **Rescue 3 (Climax) — r12: Spire (`topple` / Momentum): WORLD EVENT.** A grapple pulls the leaning monolith through a full-screen fall. Faults light across both rims, both hoodoo crowns break, boulders drop into the chasm and the three-piece vertical skyline disappears in one geological impact. It is replaced by fractured near/far shelves and a wide, rust-coloured diagonal mass of rock facets; Pep A makes the round's longest four-part climb over the transformed canyon.
* **Player Escalation Experience:** Riding one pocket of rising air → Loading and traversing a canyon-wide suspended system → Collapsing the canyon skyline into a new landform and route.

---

### Round 05 — Tidewater docks

* **World rule.** Everything floats or sinks, and the water is going somewhere whether you like it or not.
* **Geometry.** Three different water compositions: a tight two-jetty punt, a long two-gate lock, and an exposed low-tide bay sealed by a full-width barrage.
* **Interaction vocabulary.** Buoyancy, Momentum, Crossing.
* **Props.** bucket, oar, buoy, net, plank, weight, stone, balloon.
* **Choreography.** Layered floating physics: local displacement, a gate/level/navigation chain, then an atomic low-tide/high-tide world state with independently refloating harbor structures.
* **Atmosphere.** Bright cyan sky, crisp 1.16 sun, near-zero fog over sparkling water.
* **Environmental animation.** `Bob` on surface craft and ocean swell, controlled full-harbor `Drift` after the barrage opens, `Spin` seagull.
* **Sound.** `amb_tide` under `splash`, `creak`, `boing`, `ratchet` and `rumble`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r13: Punt (`bail` / Buoyancy): LOCAL EVENT.** Bailing one waterlogged hull removes only its bilge and raises only the punt. One short boarding hop follows; the sea and docks remain unchanged.
  * **Rescue 2 (Expansion) — r14: Channel (`paddle` / Momentum): LANDSCAPE / SYSTEM EVENT.** The oar drives a dockside capstan. The lower gates open, mooring releases, the whole lock chamber swaps from low to high water, its gauge and raft rise, then the upper gates open and Pep A navigates the raft through two long legs of the waterway.
  * **Rescue 3 (Climax) — r15: Current (`drift` / Crossing): WORLD EVENT.** The buoy floats into the barrage latch instead of carrying a Pep. The wheel turns, chains lift and the gate folds open; a full-width surge crosses the bay. Exposed mudflats and their narrow channel are replaced by high water, two stranded boats refloat in different directions, the broken dock becomes a large floating pontoon and a current begins across the entire harbor. The tide then carries that piece of the environment through the round's longest three-leg water traversal.
* **Player Escalation Experience:** Raising one swamped hull → Operating and navigating a lock system → Releasing a tide that replaces the entire harbor state and turns stranded infrastructure into the route.

---

### Round 06 — Storm rooftop

* **World rule.** The wind has a direction and it is taking things. Anything loose is already leaving, and there is nothing underneath.
* **Geometry.** Three rooftop scales: a local tarp gap, a mast/grid landscape, and a storm-locked roof that can become a full spillway.
* **Interaction vocabulary.** Airflow, Signal, Momentum.
* **Props.** sandbag, lightning rod, plank, lantern, rope, umbrella, pillow, stone.
* **Choreography.** Wind-driven lateral motion grows from one pinned surface through a causal electrical grid to a roof-wide hydraulic route change.
* **Atmosphere.** Dark storm sky, low sun 0.58, dense driving fog, 31° camera emphasizing vertical plunge.
* **Environmental animation.** `Drift` driving rain curtain, `Sway` communications mast, deterministic `Flicker` lightning flashes.
* **Sound.** `amb_storm` under `wind`, `zap`, `clank`, `glide_hiss`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r16: Tarp (`pin` / Airflow): LOCAL EVENT.** The sandbag atomically pins one whipping tarp into a short walkable patch; the surrounding storm remains unchanged.
  * **Rescue 2 (Expansion) — r17: Mast (`ground` / Signal): LANDSCAPE / SYSTEM EVENT.** The rod intercepts a strike, sends charge through a staged relay/grid chain, grounds the roof and opens a broad bridge route before the Pep crosses it.
  * **Rescue 3 (Climax) — r18: Gutter (`chute` / Momentum): WORLD EVENT.** The plank releases the storm cistern. The locked roof, rain and gutter network give way to a full-width spillway state, multiple rooftop structures react, and the Peps take the round's longest multi-leg storm traversal.
* **Player Escalation Experience:** Pinning one patch of canvas → Grounding and opening a rooftop grid → Releasing the storm water so the entire roof becomes the route.

---

### Round 07 — Crystal cave

* **World rule.** You cannot see and you cannot reach. Make light, make the right sound, or move the rock — and the cave answers back.
* **Geometry.** A compact dark pocket, a sealed resonant chamber, then a rail-bound cavern whose rock shell can open into a geode cathedral.
* **Interaction vocabulary.** Reflection, Resonance, Momentum.
* **Props.** lantern, chime crystal, pickaxe, mirror, bell, rope, pillow, stone.
* **Choreography.** Propagating energy grows from one atomic light reveal through a three-part roof resonance chain to a cart-triggered whole-cavern opening.
* **Atmosphere.** Ink sky, cold rim lighting + lantern-warm 0.66 fill, wide 34° lens capturing cavernous depth.
* **Environmental animation.** `Drift` dripping stalactites, `Pulse` bioluminescent crystal seam.
* **Sound.** `amb_cave` under `crystal`, `drip`, `chip`, `rumble`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r19: Dark (`kindle` / Reflection): LOCAL EVENT.** Hanging the lantern replaces one compact black floor pocket with a warm pool and stepping stones for a short crossing.
  * **Rescue 2 (Expansion) — r20: Vein (`ring` / Resonance): LANDSCAPE / SYSTEM EVENT.** The tuned note travels through three roof sections, shakes open the broad rock shutter and unfolds a stage-spanning crystal stair route.
  * **Rescue 3 (Climax) — r21: Cart (`hew` / Momentum): WORLD EVENT.** The released cart strikes a resonant gate; both cavern seals split away, the rail becomes a crystal causeway and the whole room opens into a luminous geode cathedral before the longest traversal.
* **Player Escalation Experience:** Lighting one dark pocket → Ringing open a crystal stair system → Using the ore cart to reveal an entirely different cavern state.

---

### Round 08 — Snowpeak slopes

* **World rule.** Everything rolls downhill. Your job is to aim it, ride it, or hold on.
* **Geometry.** A seven-tier diagonal slope staged as one powder pocket, a blocked gate course, and a fractured summit that can settle into an avalanche fan.
* **Interaction vocabulary.** Airflow, Momentum, Crossing.
* **Props.** fan, sled, rope, pickaxe, hair dryer, watering can, umbrella, stone.
* **Choreography.** Downhill motion grows from one short crossing through a four-leg banked run to a cornice release, screen-wide avalanche and two-Pep runout.
* **Atmosphere.** White-blue alpine sky, restrained 0.82 sun and cool fill, with controlled clear/warm outcome states that preserve snow detail.
* **Environmental animation.** Controlled `Drift` spindrift and `Sway` marker flags react to the course opening and avalanche release.
* **Sound.** `amb_peak` under `crunch`, `glide_hiss`, `crack`, `rumble`, `wind`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r22: Powder (`crust` / Airflow): LOCAL EVENT.** The fan clears and atomically packs one compact drift pocket into a wind slab for a single short crossing.
  * **Rescue 2 (Expansion) — r23: Chute (`sled` / Momentum): LANDSCAPE / SYSTEM EVENT.** The sled releases the start mechanism and three gates in sequence. Seven blocked sections become a continuous low-bank S-course, then Pep and sled carve four visible turns down the whole stage.
  * **Rescue 3 (Climax) — r24: Traverse (`traverse` / Crossing): WORLD EVENT.** Tensioning the safety line carries one Pep to the release arm, fractures the cornice and sends an avalanche across the frame. The broken shelves and crevasses are replaced by a broad settled fan; both Peps then ride the new mountain state to the runout.
* **Player Escalation Experience:** Packing one snow pocket → Configuring and riding the mountain's route system → Releasing the summit so the mountain rebuilds itself as the final route.

---

### Round 09 — Deep ocean trench

* **World rule.** Down is slow and up is free. Nothing falls, everything drifts, and sound travels as deep resonance.
* **Geometry.** Vertical underwater abyss trench bounded by basalt ledges, rising bubble columns, sunken galleon wreck, and hydrothermal vent canyon.
* **Interaction vocabulary.** Buoyancy, Luring, Crossing.
* **Props.** bubble shell, glow jelly, weight, net, leaf, bell, scissors, stone, balloon.
* **Choreography.** Weightless underwater easing: long gentle ascents, fluid drift, and suppressed impact damping.
* **Atmosphere.** Deep teal ocean depths, top-down 0.62 sun shaft, thick 0.165 benthic fog, blooming to luminous cyan/aquamarine 0.95 sun shaft with radiant benthic glow on climax.
* **Environmental animation.** `Drift` rising bubble streams and trench currents, `Sway` deep-sea kelp, `Bob` angler predator and benthic flora, `Pulse` vent glow.
* **Sound.** `amb_abyss` under `bubble`, `sonar`, `clunk`, `whoosh`, `thud`, `clank`, `hiss`, `reunion`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r25: Floor (`rise` / Buoyancy): LOCAL EVENT.** Trapped in soft benthic sediment, a released air bubble column creates a localized vertical elevator lift up to the high shelf for a single gentle crossing.
  * **Rescue 2 (Expansion) — r26: Wreck (`beckon` / Luring): LANDSCAPE / ECOSYSTEM EVENT.** A sunken galleon wreck is guarded by a predatory deep-sea anglerfish. Releasing the bioluminescent glow jelly draws the creature in a sweeping arc away into the deep abyss, which awakens the dormant coral reef, illuminates the interior hull passage, and enables a dual traversal through the ship's illuminated ribcage.
  * **Rescue 3 (Climax) — r27: Current (`moor` / Crossing): WORLD EVENT.** A roaring hydrothermal vent chimney blasts violent updraft plumes and torrential cross-currents across a massive abyss chasm. Plunging an iron mooring ballast directly into the vent spire triggers a massive hydraulic back-pressure reaction and seismic tremor. The abyss undergoes a radical physical reorganization: the erupting chimney collapses and submerged volcanic basalt monoliths heave upward out of the abyss pit, locking together into an interlocking monumental stepped causeway spanning the chasm.
* **Player Escalation Experience:** Floating up inside an elevator bubble → Luring a deep-sea angler away to awaken an illuminated sunken wreck → Triggering a seismic hydrothermal reaction that raises a massive stepped basalt causeway across the chasm.

---

### Round 10 — Orbital station

* **World rule.** Nothing falls and nothing stops. Every push lasts forever, and you have to push off something.
* **Geometry.** True zero-gravity void: three disconnected station hull modules floating in an ungrounded starfield.
* **Interaction vocabulary.** Momentum, Magnetism, Airflow.
* **Props.** thruster, magnet, pillow, mirror, rope, bell, stone, balloon, umbrella.
* **Choreography.** Linear zero-g vectors: no gravity drop, no velocity damping, constant-speed drift.
* **Atmosphere.** Pitch-black cosmos, searing 1.62 white key light, pitch shadows (0.10 fill), detached 30° / 7.2 m camera.
* **Environmental animation.** `Spin` rotating station ring and orbital debris, linear `Drift` micrometeorite streams, `Flicker` station beacon.
* **Sound.** `amb_orbit` under `thrust`, `servo`, `hiss`, `snap_on`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r28: Drift (`push` / Momentum):** Pure Newtonian RCS translation. Firing a directional gas thruster imparts frictionless velocity, sending Pep A gliding along a straight line across the module gap.
  * **Rescue 2 (Expansion) — r29: Tumble (`attract` / Magnetism):** Multi-axis zero-g magnetic recovery. Activating the station crane's electromagnet generates a magnetic flux vector that arrests Pep B's tumbling trajectory through spinning space debris and reels them safely to the docking platform.
  * **Rescue 3 (Climax) — r30: Airlock (`seal` / Airflow):** High-stakes orbital depressurization containment. Releasing the reinforced seal pillow into the breached habitat module allows decompression suction to seat it firmly into the hatch collar; the vacuum hiss seals shut, station telemetry resets to green, and the rotating station hub aligns its umbilical for an orbital spacewalk embrace.
* **Player Escalation Experience:** Straight zero-g thruster glide → Magnetic crane recovery amidst spinning debris → Sealing a breached space station airlock in deep space.

---

### Round 11 — Foundry floor

* **World rule.** The machine is already running and it will not wait for you. Feed it, cool it, or jam it.
* **Geometry.** Heavy industrial deck with an active conveyor, an overhead crane gantry, and a molten metal spill.
* **Interaction vocabulary.** Activation, Temperature, Momentum.
* **Props.** crate, watering can, wrench, magnet, rope, plank, pillow, fan.
* **Choreography.** Relentless industrial machinery: continuously scrolling conveyor slats, heavy rhythmic press stomps, steam venting.
* **Atmosphere.** Smoke-violet haze lit from underneath by molten orange ground bounce; dimmed 0.72 key light.
* **Environmental animation.** `Drift` conveyor slats and steam exhaust plumes, `Bob` crane trolley, `Beat` pneumatic stamping ram, `Pulse` molten crucible glow.
* **Sound.** `amb_forge` under `sizzle`, `hiss`, `clank`, `snap_on`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r31: Conveyor (`feed` / Activation):** Automated intake trigger. Loading the heavy parts crate onto the conveyor intake triggers the line sensor, opening the transfer gate so Pep A can cross.
  * **Rescue 2 (Expansion) — r32: Spill (`quench` / Temperature):** Thermal state metamorphosis. Pouring water over the bubbling molten metal spill causes a violent steam burst and instantly crusts the liquid slag into solid, black basalt flagstones.
  * **Rescue 3 (Climax) — r33: Piston (`jam` / Momentum):** Massive industrial mechanical sabotage. Wedging the heavy steel wrench into the stamping press linkage jams the giant pneumatic piston at top dead center with showers of sparks and explosive steam exhaust, freezing the entire factory floor for a safe crossing.
* **Player Escalation Experience:** Feeding a conveyor sensor → Quenching molten lava into solid stone → Jamming a giant industrial stamping press with a wrench.

---

### Round 12 — Neon skyline

* **World rule.** The city moves for you if you catch it at the right moment. Everything is fast, lit, and a long way up.
* **Geometry.** Three tiered skyscraper rooftops overlooking a luminous midnight metropolis with an overhead transit beam.
* **Interaction vocabulary.** Signal, Momentum, Crossing.
* **Props.** neon tube, zip grip, balloon, scissors, hair dryer, pillow, plank, rope.
* **Choreography.** Fast, high-energy metropolitan movement: illuminated transit cars, flashing billboards, aerial flight.
* **Atmosphere.** Deep night violet, sharp 0.96 key light, rich sodium-orange 0.86 fill, glittering skyscraper window grids.
* **Environmental animation.** `Drift` skyline monorail tram, `Flicker` neon signage on staggered duty cycles, `Pulse` city skyline glow.
* **Sound.** `amb_neon` under `neon`, `transit`, `zap`, `chime`.
* **Escalation Arc:**
  * **Rescue 1 (Introduction) — r34: Sign (`power` / Signal):** Circuit completion. Snapping the neon tube into the rooftop sign fixture energizes the glowing neon circuit and extends the illuminated sign bridge across the building gap.
  * **Rescue 2 (Expansion) — r35: Transit (`board` / Momentum):** Fast dynamic transit interception. Latching the zip grip onto the moving sky tram's runner allows Pep A to hitch a ride across the open transit beam, disembarking onto the far helipad.
  * **Rescue 3 (Climax) — r36: Skyline (`soar` / Crossing):** The grand triumphant finale of Save Peps. Harnessing the iconic orange balloon — the game's running joke from r01 (*"Up is not across."*) — catches the powerful city updrafts across the entire metropolitan skyline; billboard lights sparkle, skyscraper searchlights sweep, and Pep A soars majestically over the midnight city to land on the summit spire for the ultimate reunion!
* **Player Escalation Experience:** Powering a neon bridge → Catching a high-speed sky tram → Soaring across the entire illuminated city skyline by balloon in the ultimate game finale.

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
