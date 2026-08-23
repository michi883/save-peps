using UnityEngine;
using SavePeps.Rescue;

using static SavePeps.EditorTools.Toy;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Stages for the second half of the journey: cave, peak, abyss, orbit,
    /// forge and neon. Split from <see cref="DioramaLibrary"/> purely for
    /// length — the two files share one partial class and one set of rules.
    /// </summary>
    internal static partial class DioramaLibrary
    {
        // ===================================================================
        // World 7 — Crystal cave. You cannot see or reach. Make light, make sound.
        // ===================================================================

        /// <summary>Movers: Darkness, LampHook and LitPool (hidden).</summary>
        private static void CaveDark()
        {
            var root = Worlds.Begin(Worlds.Cave, "Dark");
            var t = root.transform;

            Rod(t, "HookPost", M("Stone"), new Vector3(-0.50f, 0.26f, -0.02f),
                new Vector3(0.05f, 0.22f, 0.05f));
            var hook = Mover(t, "LampHook");
            Box(hook, "Arm", M("StoneLight"), new Vector3(-0.41f, 0.47f, -0.02f),
                new Vector3(0.18f, 0.03f, 0.03f));
            Box(hook, "Barb", M("StoneLight"), new Vector3(-0.33f, 0.43f, -0.02f),
                new Vector3(0.025f, 0.08f, 0.025f));

            // Only this compact pocket changes. R7.1 teaches the cave's light
            // rule without spending the room-wide reveal reserved for later.
            var dark = Mover(t, "Darkness");
            Ball(dark, "Void", M("Abyss"), new Vector3(-0.01f, 0.055f, 0.08f),
                new Vector3(0.72f, 0.035f, 0.78f));
            Ball(dark, "Murk", M("Ink"), new Vector3(0.12f, 0.12f, 0.15f),
                new Vector3(0.34f, 0.16f, 0.30f));

            var pool = Hidden(Mover(t, "LitPool"));
            Ball(pool, "Warmth", M("Candle"), new Vector3(-0.01f, 0.050f, 0.08f),
                new Vector3(0.78f, 0.010f, 0.86f));
            Ball(pool, "Water", M("WaterDeep"), new Vector3(-0.01f, 0.060f, 0.08f),
                new Vector3(0.58f, 0.012f, 0.64f));
            foreach (var (x, z) in new[] { (-0.18f, -0.02f), (0.08f, 0.16f), (0.22f, 0.30f) })
            {
                Ball(pool, "Steppingstone", M("Violet"), new Vector3(x, 0.080f, z),
                    new Vector3(0.18f, 0.045f, 0.16f));
            }

            Worlds.Peps(t, new Vector3(-0.24f, 0.04f, -0.38f), new Vector3(0.24f, 0.04f, 0.48f),
                new Vector3(0.12f, 0.04f, 0.34f));
            Worlds.Slots(t, new Vector3(-0.46f, 0.04f, -1.14f), new Vector3(0f, 0.04f, -1.40f),
                new Vector3(0.48f, 0.04f, -1.14f));
            Worlds.Finish(root, Worlds.Cave, _dir);
        }

        /// <summary>
        /// Movers: CrystalVein, three resonance pulses, SealedPassage,
        /// OpenPassage and RouteLights (hidden), and Dust (hidden).
        /// </summary>
        private static void CaveVein()
        {
            var root = Worlds.Begin(Worlds.Cave, "Vein");
            var t = root.transform;

            // A tuned seam runs through the whole roof. Its three sections are
            // spatially legible so the successful note can travel rather than
            // appear everywhere at once.
            var vein = Mover(t, "CrystalVein");
            for (var i = 0; i < 6; i++)
            {
                var z = -0.72f + i * 0.43f;
                Box(vein, "Shard", M("WaterBright"),
                    new Vector3(-0.20f + (i % 2) * 0.40f, 1.18f + (i % 2) * 0.08f, z),
                    new Vector3(0.065f, 0.20f, 0.065f),
                    new Vector3(0f, 34f, i % 2 == 0 ? 12f : -12f));
            }

            for (var i = 0; i < 5; i++)
            {
                Box(vein, "Seam", M("Stone"),
                    new Vector3(-0.10f + (i % 2) * 0.20f, 1.28f, -0.64f + i * 0.52f),
                    new Vector3(0.09f, 0.04f, 0.56f),
                    new Vector3(0f, i % 2 == 0 ? 10f : -10f, 0f));
            }

            foreach (var (name, z, x) in new[]
                     {
                         ("VeinPulseNear", -0.62f, -0.20f),
                         ("VeinPulseMid", 0.16f, 0.18f),
                         ("VeinPulseFar", 0.92f, -0.14f),
                     })
            {
                var pulse = Hidden(Mover(t, name));
                Beam(pulse, "Tone", M("Candle"), new Vector3(x - 0.30f, 1.12f, z),
                    new Vector3(x + 0.30f, 1.36f, z + 0.08f), 0.030f);
                Beam(pulse, "Echo", M("WaterLight"), new Vector3(x - 0.24f, 1.34f, z + 0.04f),
                    new Vector3(x + 0.24f, 1.10f, z + 0.12f), 0.020f);
            }

            // Before: a broad, staggered rock shutter blocks both the tunnel
            // and the only continuous floor line through the cave.
            var sealedPassage = Mover(t, "SealedPassage");
            for (var i = 0; i < 7; i++)
            {
                Box(sealedPassage, "Shutter", i % 2 == 0 ? M("Violet") : M("Ink"),
                    new Vector3(-0.66f + i * 0.22f, 0.34f + (i % 2) * 0.10f, 0.58f),
                    new Vector3(0.23f, 0.68f + (i % 2) * 0.18f, 0.22f),
                    new Vector3(0f, 0f, i % 2 == 0 ? 4f : -4f));
            }

            // After: the resonant ceiling has unfolded a stepped crystal road
            // across most of the landscape. This is a route change, not a
            // light cue, and its diagonal is visible from the fixed camera.
            var open = Hidden(Mover(t, "OpenPassage"));
            for (var i = 0; i < 7; i++)
            {
                var z = -0.28f + i * 0.24f;
                var x = -0.34f + i * 0.11f;
                var y = 0.08f + i * 0.045f;
                Box(open, "CrystalStep", i % 2 == 0 ? M("Ice") : M("WaterBright"),
                    new Vector3(x, y, z), new Vector3(0.52f, 0.09f, 0.28f),
                    new Vector3(0f, -10f + i * 3f, 0f));
            }
            Beam(open, "RouteRail", M("Candle"), new Vector3(-0.58f, 0.15f, -0.42f),
                new Vector3(0.48f, 0.51f, 1.32f), 0.025f);

            var routeLights = Hidden(Mover(t, "RouteLights"));
            for (var i = 0; i < 6; i++)
                Ball(routeLights, "Beacon", i % 2 == 0 ? M("AccentLight") : M("WaterLight"),
                    new Vector3(-0.42f + i * 0.16f, 0.20f + i * 0.055f, -0.30f + i * 0.29f),
                    Vector3.one * 0.055f);

            var dust = Hidden(Mover(t, "Dust"));
            foreach (var (x, z, s) in new[]
                     {
                         (-0.46f, 0.54f, 0.10f), (-0.12f, 0.60f, 0.08f),
                         (0.22f, 0.56f, 0.11f), (0.48f, 0.62f, 0.07f),
                     })
            {
                Ball(dust, "Chip", M("Stone"), new Vector3(x, 0.26f, z), Vector3.one * s);
            }

            Box(t, "NearShelf", M("Violet"), new Vector3(-0.38f, 0.07f, -0.60f),
                new Vector3(0.76f, 0.14f, 0.54f));
            Box(t, "FarShelf", M("Violet"), new Vector3(0.34f, 0.20f, 1.18f),
                new Vector3(0.72f, 0.40f, 0.58f));

            Worlds.Peps(t, new Vector3(-0.42f, 0.12f, -0.68f), new Vector3(0.36f, 0.44f, 1.20f),
                new Vector3(0.28f, 0.44f, 1.08f));
            Worlds.Slots(t, new Vector3(-0.48f, 0.04f, -1.18f), new Vector3(0.48f, 0.04f, -1.18f),
                new Vector3(0f, 0.04f, -1.44f));
            Worlds.Finish(root, Worlds.Cave, _dir);
        }

        /// <summary>
        /// Movers: MineCart, Chock, Rail, TuningGate, SealLeft/Right,
        /// hidden GeodeLeft/Right, CrystalRoute, GeodeHeart and CrystalFall.
        /// </summary>
        private static void CaveCart()
        {
            var root = Worlds.Begin(Worlds.Cave, "Cart");
            var t = root.transform;

            // BEFORE: the cart rail is the only clear line through a cramped,
            // sealed cavern. Two rock masses erase most of the back silhouette.
            var rail = Mover(t, "Rail");
            foreach (var x in new[] { -0.16f, 0.16f })
            {
                Box(rail, "Rail", M("StoneLight"), new Vector3(x, 0.06f, 0.02f),
                    new Vector3(0.045f, 0.03f, 2.20f));
            }

            for (var i = 0; i < 7; i++)
            {
                Box(rail, "Sleeper", M("WoodDark"), new Vector3(0f, 0.045f, -0.94f + i * 0.32f),
                    new Vector3(0.42f, 0.025f, 0.08f));
            }

            var cart = Mover(t, "MineCart");
            cart.parent.localPosition = new Vector3(0f, 0f, -0.56f);
            Box(cart, "Tub", M("Stone"), new Vector3(0f, 0.23f, 0f), new Vector3(0.42f, 0.26f, 0.48f));
            Box(cart, "TubLip", M("StoneLight"), new Vector3(0f, 0.365f, 0f),
                new Vector3(0.46f, 0.03f, 0.52f));
            foreach (var (x, z) in new[]
                     {
                         (-0.18f, -0.17f), (0.18f, -0.17f), (-0.18f, 0.17f), (0.18f, 0.17f),
                     })
            {
                Rod(cart, "Wheel", M("Ink"), new Vector3(x, 0.09f, z), new Vector3(0.14f, 0.02f, 0.14f),
                    new Vector3(0f, 0f, 90f));
            }

            foreach (var (x, s) in new[] { (-0.12f, 0.9f), (0.04f, 1.1f), (0.17f, 0.8f) })
            {
                Ball(cart, "Ore", M("WaterBright"), new Vector3(x, 0.42f, 0f), Vector3.one * (0.10f * s));
            }

            var chock = Mover(t, "Chock");
            Box(chock, "Wedge", M("Wood"), new Vector3(-0.11f, 0.075f, -0.72f),
                new Vector3(0.13f, 0.10f, 0.18f), new Vector3(24f, 0f, 0f));

            var gate = Mover(t, "TuningGate");
            foreach (var x in new[] { -0.34f, 0.34f })
                Rod(gate, "GatePost", M("Accent"), new Vector3(x, 0.42f, 0.58f),
                    new Vector3(0.05f, 0.38f, 0.05f));
            Beam(gate, "GateBeam", M("AccentLight"), new Vector3(-0.36f, 0.76f, 0.58f),
                new Vector3(0.36f, 0.76f, 0.58f), 0.06f);
            Rod(gate, "Resonator", M("Candle"), new Vector3(0f, 0.60f, 0.58f),
                new Vector3(0.16f, 0.035f, 0.16f), new Vector3(90f, 0f, 0f));

            var sealLeft = Mover(t, "SealLeft");
            var sealRight = Mover(t, "SealRight");
            foreach (var (holder, sign) in new[] { (sealLeft, -1f), (sealRight, 1f) })
            {
                Box(holder, "SealSlab", M("Abyss"), new Vector3(sign * 0.40f, 0.78f, 1.12f),
                    new Vector3(0.82f, 1.38f, 0.46f), new Vector3(0f, 0f, sign * -7f));
                Ball(holder, "SealBoss", M("Violet"), new Vector3(sign * 0.50f, 1.20f, 0.93f),
                    new Vector3(0.56f, 0.48f, 0.34f));
                Ball(holder, "SealBoss", M("Ink"), new Vector3(sign * 0.28f, 0.44f, 0.91f),
                    new Vector3(0.48f, 0.44f, 0.30f));
            }

            // AFTER: the closed wall becomes a luminous geode cathedral and
            // the narrow mine rail becomes a wide crystalline causeway.
            var geodeLeft = Hidden(Mover(t, "GeodeLeft"));
            var geodeRight = Hidden(Mover(t, "GeodeRight"));
            foreach (var (holder, sign) in new[] { (geodeLeft, -1f), (geodeRight, 1f) })
            {
                for (var i = 0; i < 5; i++)
                {
                    var y = 0.34f + i * 0.30f;
                    var x = sign * (0.72f - i * 0.09f);
                    Box(holder, "GeodeRib", i % 2 == 0 ? M("WaterBright") : M("Ice"),
                        new Vector3(x, y, 1.04f + i * 0.06f),
                        new Vector3(0.16f, 0.54f + i * 0.08f, 0.18f),
                        new Vector3(0f, 22f * sign, sign * (18f - i * 5f)));
                    Box(holder, "GoldVein", M("Candle"),
                        new Vector3(x - sign * 0.08f, y + 0.05f, 0.94f + i * 0.07f),
                        new Vector3(0.035f, 0.38f, 0.035f),
                        new Vector3(0f, 0f, sign * (20f - i * 4f)));
                }
            }

            var route = Hidden(Mover(t, "CrystalRoute"));
            for (var i = 0; i < 8; i++)
            {
                var z = -0.92f + i * 0.29f;
                var x = i < 4 ? -0.22f + i * 0.13f : 0.17f - (i - 4) * 0.08f;
                Box(route, "Causeway", i % 2 == 0 ? M("WaterBright") : M("Ice"),
                    new Vector3(x, 0.08f + i * 0.025f, z), new Vector3(0.72f, 0.10f, 0.34f),
                    new Vector3(0f, i % 2 == 0 ? -10f : 10f, 0f));
            }
            Beam(route, "CausewayVein", M("Candle"), new Vector3(-0.42f, 0.16f, -1.05f),
                new Vector3(0.32f, 0.38f, 1.24f), 0.035f);

            var heart = Hidden(Mover(t, "GeodeHeart"));
            var pulse = Idle(heart, AmbientMode.Pulse, 0.10f, 0.28f, Vector3.up,
                controlId: "CaveGeode");
            BlockRing(pulse, "Heart", M("Candle"), new Vector3(0f, 1.18f, 1.34f),
                new Vector2(0.46f, 0.56f), 12, 0.08f, 0.10f);
            for (var i = 0; i < 8; i++)
            {
                var angle = i * Mathf.PI * 2f / 8f;
                var p = new Vector3(Mathf.Cos(angle) * 0.66f, 1.18f + Mathf.Sin(angle) * 0.76f, 1.40f);
                Box(pulse, "CrownShard", i % 2 == 0 ? M("WaterLight") : M("AccentLight"), p,
                    new Vector3(0.10f, 0.38f, 0.10f),
                    new Vector3(0f, 0f, -i * 45f + 90f));
            }

            var fall = Hidden(Mover(t, "CrystalFall"));
            foreach (var (x, y, z, r) in new[]
                     {
                         (-0.64f, 1.44f, 0.18f, 18f), (-0.28f, 1.58f, 0.62f, -14f),
                         (0.18f, 1.48f, 0.10f, 12f), (0.56f, 1.64f, 0.74f, -20f),
                     })
                Box(fall, "FallingShard", M("WaterLight"), new Vector3(x, y, z),
                    new Vector3(0.055f, 0.22f, 0.055f), new Vector3(0f, 0f, r));

            Box(t, "FarPod", M("Violet"), new Vector3(0.42f, 0.14f, 0.74f),
                new Vector3(0.46f, 0.28f, 0.42f));
            Box(t, "FarPodLip", M("Stone"), new Vector3(0.42f, 0.29f, 0.55f),
                new Vector3(0.46f, 0.035f, 0.05f));

            Worlds.Peps(t, new Vector3(-0.48f, 0.04f, -0.98f), new Vector3(0.42f, 0.34f, 0.74f),
                new Vector3(0.30f, 0.34f, 0.68f));
            Worlds.Slots(t, new Vector3(-0.46f, 0.04f, -1.26f), new Vector3(0.48f, 0.04f, -1.10f),
                new Vector3(0f, 0.04f, -1.46f));
            Worlds.Finish(root, Worlds.Cave, _dir);
        }

        // ===================================================================
        // World 8 — Snowpeak. Everything rolls downhill; aim it or stop it.
        // ===================================================================

        /// <summary>Movers: Powder, Crust (hidden), Sinkhole.</summary>
        private static void PeakPowder()
        {
            var root = Worlds.Begin(Worlds.Peak, "Powder");
            var t = root.transform;

            // One compact drift pocket teaches the snow rule. Later rescues
            // own the mountain; this one deliberately does not.
            var powder = Mover(t, "Powder");
            foreach (var (x, z, s) in new[]
                     {
                         (-0.20f, -0.04f, 0.86f), (0.16f, 0.02f, 1.00f),
                         (-0.08f, 0.26f, 0.78f), (0.22f, 0.30f, 0.68f),
                     })
                Ball(powder, "Drift", M("Cream"), new Vector3(x, 0.57f, z),
                    new Vector3(0.34f * s, 0.10f, 0.30f * s));

            var crust = Hidden(Mover(t, "Crust"));
            Box(crust, "Glaze", M("Ice"), new Vector3(0.02f, 0.565f, 0.12f),
                new Vector3(0.70f, 0.025f, 0.78f), new Vector3(18f, 0f, 0f));
            foreach (var x in new[] { -0.18f, 0.04f, 0.24f })
                Box(crust, "WindRipple", M("WaterLight"), new Vector3(x, 0.59f, 0.12f + x),
                    new Vector3(0.30f, 0.008f, 0.035f), new Vector3(18f, 10f, 0f));

            var hole = Mover(t, "Sinkhole");
            Ball(hole, "Pit", M("WaterDeep"), new Vector3(-0.02f, 0.555f, 0.10f),
                new Vector3(0.24f, 0.035f, 0.22f));

            Worlds.Peps(t, new Vector3(-0.28f, 0.41f, -0.42f), new Vector3(0.24f, 0.72f, 0.50f),
                new Vector3(0.12f, 0.66f, 0.38f));
            Worlds.Slots(t, new Vector3(-0.46f, 0.10f, -1.44f), new Vector3(0.46f, 0.10f, -1.44f),
                new Vector3(0f, 0.255f, -0.96f));
            Worlds.Finish(root, Worlds.Peak, _dir);
        }

        /// <summary>
        /// Movers: ClosedRun, BankedRun (hidden), StartGate, three CourseGates,
        /// TrailFlags (hidden), RunSpray (hidden), and Drift.
        /// </summary>
        private static void PeakChute()
        {
            var root = Worlds.Begin(Worlds.Peak, "Chute");
            var t = root.transform;

            // BEFORE: an alternating sequence of snow blocks interrupts the
            // course. It reads as a system waiting to be configured.
            var closedRun = Mover(t, "ClosedRun");
            for (var i = 0; i < 7; i++)
            {
                var z = -1.34f + i * 0.44f;
                var y = 0.13f + i * 0.145f;
                var x = i % 2 == 0 ? -0.22f : 0.22f;
                Box(closedRun, "RunBed", M("Ice"), new Vector3(x, y, z),
                    new Vector3(0.56f, 0.025f, 0.42f));
                Box(closedRun, "SnowBlock", M("Snow"), new Vector3(-x, y + 0.09f, z),
                    new Vector3(0.40f, 0.13f, 0.32f), new Vector3(0f, 0f, i % 2 == 0 ? 5f : -5f));
            }

            // AFTER: broad banked S-curves occupy most of the slope and force
            // a visibly different traversal pattern from the straight baseline.
            var banked = Hidden(Mover(t, "BankedRun"));
            var courseX = new[] { -0.30f, -0.12f, 0.22f, 0.36f, 0.16f, -0.24f, -0.34f, -0.08f };
            for (var i = 0; i < courseX.Length; i++)
            {
                var z = -1.44f + i * 0.41f;
                var y = 0.12f + i * 0.135f;
                var heading = i == 0 ? 8f : (courseX[i] - courseX[i - 1]) * -42f;
                Box(banked, "BankedBed", M("WaterBright"), new Vector3(courseX[i], y, z),
                    new Vector3(0.72f, 0.030f, 0.44f), new Vector3(0f, heading, 0f));

                // One low outside bank describes each turn. Two full-height
                // walls per segment made the open route look more obstructed
                // than the closed one on a phone.
                var side = i % 2 == 0 ? -1f : 1f;
                Box(banked, "SnowBank", M("Snow"),
                    new Vector3(courseX[i] + side * 0.36f, y + 0.055f, z),
                    new Vector3(0.085f, 0.10f, 0.36f),
                    new Vector3(0f, heading, side * -8f));
            }

            var startGate = Mover(t, "StartGate");
            foreach (var x in new[] { -0.30f, 0.30f })
                Rod(startGate, "Post", M("Accent"), new Vector3(x, 1.20f, 1.38f),
                    new Vector3(0.045f, 0.24f, 0.045f));
            Box(startGate, "Bar", M("AccentLight"), new Vector3(0f, 1.26f, 1.38f),
                new Vector3(0.64f, 0.055f, 0.055f));

            foreach (var (name, x, y, z, angle) in new[]
                     {
                         ("CourseGateHigh", 0.22f, 0.86f, 0.70f, -14f),
                         ("CourseGateMid", -0.22f, 0.58f, -0.14f, 14f),
                         ("CourseGateLow", 0.18f, 0.32f, -0.92f, -14f),
                     })
            {
                var gate = Mover(t, name);
                Box(gate, "Arm", M("Accent"), new Vector3(x, y, z),
                    new Vector3(0.68f, 0.055f, 0.055f), new Vector3(0f, 0f, angle));
                Rod(gate, "Pivot", M("Stone"), new Vector3(x - 0.31f, y, z),
                    new Vector3(0.07f, 0.04f, 0.07f), new Vector3(90f, 0f, 0f));
            }

            var trailFlags = Hidden(Mover(t, "TrailFlags"));
            foreach (var (x, y, z, sign) in new[]
                     {
                         (-0.60f, 0.22f, -1.15f, 1f), (0.60f, 0.48f, -0.30f, -1f),
                         (-0.58f, 0.78f, 0.56f, 1f), (0.56f, 1.02f, 1.20f, -1f),
                     })
            {
                Rod(trailFlags, "Pole", M("Stone"), new Vector3(x, y, z),
                    new Vector3(0.025f, 0.17f, 0.025f));
                Box(trailFlags, "Flag", M("Accent"), new Vector3(x + sign * 0.08f, y + 0.10f, z),
                    new Vector3(0.16f, 0.09f, 0.025f));
            }

            var drift = Mover(t, "Drift");
            foreach (var (x, z, s) in new[] { (-0.24f, -1.30f, 0.9f), (0.18f, -1.12f, 0.8f) })
                Ball(drift, "Mound", M("Snow"), new Vector3(x, 0.16f, z),
                    new Vector3(0.42f * s, 0.16f, 0.36f * s));

            var spray = Hidden(Mover(t, "RunSpray"));
            foreach (var (x, y, s) in new[]
                     {
                         (-0.28f, 0.24f, 0.18f), (0f, 0.34f, 0.22f), (0.26f, 0.25f, 0.16f),
                     })
                Ball(spray, "Spray", M("Cream"), new Vector3(x, y, -1.24f), Vector3.one * s);

            Worlds.Peps(t, new Vector3(-0.08f, 1.05f, 1.38f), new Vector3(-0.26f, 0.255f, -1.06f),
                new Vector3(-0.18f, 0.255f, -0.94f));
            Worlds.Slots(t, new Vector3(-0.46f, 0.10f, -1.48f), new Vector3(0.46f, 0.10f, -1.36f),
                new Vector3(0.30f, 0.255f, -0.98f));
            Worlds.Finish(root, Worlds.Peak, _dir);
        }

        /// <summary>
        /// Movers: IceBand, Bollard, RopeLine/TautLine, LockedPeakWorld,
        /// AvalancheWorld, CorniceSlab, TensionArm, FaultCracks,
        /// AvalancheFront, MountainDebris, RunoutSpray and SummitBeacons.
        /// </summary>
        private static void PeakTraverse()
        {
            var root = Worlds.Begin(Worlds.Peak, "Traverse");
            var t = root.transform;

            var band = Mover(t, "IceBand");
            Box(band, "Ice", M("Ice"), new Vector3(0f, 0.69f, 0.28f),
                new Vector3(1.30f, 0.025f, 0.34f), new Vector3(0f, -8f, 0f));

            var bollard = Mover(t, "Bollard");
            foreach (var (x, y, z) in new[] { (-0.62f, 0.90f, 0.66f), (0.62f, 1.04f, 1.04f) })
            {
                Rod(bollard, "Rock", M("Stone"), new Vector3(x, y, z),
                    new Vector3(0.17f, 0.22f, 0.17f));
                Rod(bollard, "Collar", M("AccentLight"), new Vector3(x, y + 0.17f, z),
                    new Vector3(0.13f, 0.025f, 0.13f));
            }

            var line = Mover(t, "RopeLine");
            Beam(line, "SlackLeft", M("Earth"), new Vector3(-0.62f, 1.04f, 0.66f),
                new Vector3(-0.05f, 0.84f, 0.84f), 0.018f);
            Beam(line, "SlackRight", M("Earth"), new Vector3(-0.05f, 0.84f, 0.84f),
                new Vector3(0.62f, 1.18f, 1.04f), 0.018f);

            var taut = Hidden(Mover(t, "TautLine"));
            Beam(taut, "Line", M("EarthDark"), new Vector3(-0.62f, 1.04f, 0.66f),
                new Vector3(0.62f, 1.18f, 1.04f), 0.024f);
            foreach (var p in new[] { new Vector3(-0.62f, 1.04f, 0.66f), new Vector3(0.62f, 1.18f, 1.04f) })
                Ball(taut, "Hitch", M("Cream"), p, Vector3.one * 0.055f);

            // BEFORE: a tall cornice and dark zig-zag crevasse break the peak
            // into isolated shelves. Their combined silhouette occupies most
            // of the stage, so removing them cannot read as a cosmetic change.
            var locked = Mover(t, "LockedPeakWorld");
            foreach (var (x, y, z, w, h, angle) in new[]
                     {
                         (-0.48f, 1.30f, 1.28f, 0.78f, 0.48f, -8f),
                         (0.30f, 1.38f, 1.30f, 0.92f, 0.62f, 7f),
                         (-0.34f, 0.82f, 0.34f, 0.82f, 0.28f, 12f),
                         (0.42f, 0.68f, -0.04f, 0.78f, 0.24f, -14f),
                     })
                Box(locked, "BrokenShelf", M("Snow"), new Vector3(x, y, z),
                    new Vector3(w, h, 0.42f), new Vector3(0f, 0f, angle));
            Beam(locked, "Crevasse", M("Abyss"), new Vector3(-0.78f, 0.76f, 0.52f),
                new Vector3(0.12f, 0.60f, 0.06f), 0.18f);
            Beam(locked, "Crevasse", M("Abyss"), new Vector3(0.12f, 0.60f, 0.06f),
                new Vector3(0.76f, 0.46f, -0.42f), 0.18f);

            // AFTER: the avalanche fills the crevasse and leaves one broad,
            // low fan from the summit to the runout.
            var avalancheWorld = Hidden(Mover(t, "AvalancheWorld"));
            for (var i = 0; i < 7; i++)
            {
                var z = 1.22f - i * 0.42f;
                var y = 1.06f - i * 0.14f;
                var x = -0.14f + Mathf.Sin(i * 0.90f) * 0.18f;
                Ball(avalancheWorld, "SettledFan", i % 2 == 0 ? M("Snow") : M("Ice"),
                    new Vector3(x, y, z), new Vector3(0.84f + i * 0.065f, 0.075f, 0.34f));
            }
            Beam(avalancheWorld, "BlueSpine", M("WaterBright"),
                new Vector3(-0.22f, 1.14f, 1.34f), new Vector3(0.20f, 0.24f, -1.28f), 0.045f);
            foreach (var (x, y, z) in new[]
                     {
                         (-0.58f, 0.78f, 0.54f), (0.58f, 0.34f, -0.86f),
                     })
                Ball(avalancheWorld, "SnowBank", M("Cream"), new Vector3(x, y, z),
                    new Vector3(0.38f, 0.10f, 0.28f));

            var slab = Mover(t, "CorniceSlab");
            Box(slab, "Lip", M("Snow"), new Vector3(0.08f, 1.42f, 1.32f),
                new Vector3(1.30f, 0.30f, 0.46f), new Vector3(0f, 0f, -5f));
            Box(slab, "BlueUnderside", M("Ice"), new Vector3(0.08f, 1.27f, 1.28f),
                new Vector3(1.24f, 0.08f, 0.42f), new Vector3(0f, 0f, -5f));

            var tension = Mover(t, "TensionArm");
            Box(tension, "Lever", M("Accent"), new Vector3(0.58f, 1.20f, 1.02f),
                new Vector3(0.08f, 0.48f, 0.08f), new Vector3(0f, 0f, -24f));

            var cracks = Hidden(Mover(t, "FaultCracks"));
            Beam(cracks, "Crack", M("Abyss"), new Vector3(-0.60f, 1.28f, 1.14f),
                new Vector3(-0.10f, 1.18f, 1.22f), 0.025f);
            Beam(cracks, "Crack", M("Abyss"), new Vector3(-0.10f, 1.18f, 1.22f),
                new Vector3(0.48f, 1.30f, 1.16f), 0.025f);

            var front = Hidden(Mover(t, "AvalancheFront"));
            for (var i = 0; i < 7; i++)
                Ball(front, "RollingSnow", i % 2 == 0 ? M("Cream") : M("Snow"),
                    new Vector3(-0.66f + i * 0.22f, 1.08f + (i % 2) * 0.10f, 1.04f),
                    new Vector3(0.34f, 0.18f, 0.28f));

            var debris = Mover(t, "MountainDebris");
            foreach (var (x, y, z, s) in new[]
                     {
                         (-0.48f, 1.08f, 0.88f, 0.11f), (0.12f, 0.90f, 0.52f, 0.09f),
                         (0.52f, 0.72f, 0.10f, 0.12f), (-0.18f, 0.54f, -0.34f, 0.08f),
                     })
                Ball(debris, "Rock", M("Stone"), new Vector3(x, y, z), Vector3.one * s);

            var spray = Hidden(Mover(t, "RunoutSpray"));
            foreach (var (x, y, z, s) in new[]
                     {
                         (-0.26f, 0.38f, -0.62f, 0.18f), (0.02f, 0.46f, -0.72f, 0.22f),
                         (0.30f, 0.36f, -0.62f, 0.16f),
                     })
                Ball(spray, "Spray", M("Cream"), new Vector3(x, y, z), Vector3.one * s);

            var beacons = Hidden(Mover(t, "SummitBeacons"));
            foreach (var (x, y, z) in new[] { (-0.58f, 1.14f, 1.22f), (0.58f, 1.20f, 1.22f) })
            {
                Rod(beacons, "Pole", M("Stone"), new Vector3(x, y, z),
                    new Vector3(0.025f, 0.18f, 0.025f));
                Box(beacons, "Flag", M("Accent"), new Vector3(x + 0.09f, y + 0.11f, z),
                    new Vector3(0.18f, 0.10f, 0.025f));
            }

            Worlds.Peps(t, new Vector3(-0.54f, 0.90f, 0.62f), new Vector3(0.52f, 1.04f, 1.02f),
                new Vector3(0.20f, 0.36f, -0.48f));
            Worlds.Slots(t, new Vector3(-0.46f, 0.10f, -1.42f), new Vector3(0.46f, 0.10f, -1.42f),
                new Vector3(0f, 0.255f, -0.94f));
            Worlds.Finish(root, Worlds.Peak, _dir);
        }

        // ===================================================================
        // World 9 — Deep ocean. Down is slow, up is free, nothing falls.
        // ===================================================================

        /// <summary>Movers: SiltCloud, Ledge, Lift (hidden).</summary>
        private static void AbyssFloor()
        {
            var root = Worlds.Begin(Worlds.Abyss, "Floor");
            var t = root.transform;

            var ledge = Mover(t, "Ledge");
            Box(ledge, "Shelf", M("Violet"), new Vector3(-0.50f, 0.66f, 0.62f), new Vector3(0.50f, 0.12f, 0.96f));
            Box(ledge, "ShelfLip", M("Stone"), new Vector3(-0.30f, 0.70f, 0.62f), new Vector3(0.09f, 0.05f, 0.96f));
            Box(ledge, "Support", M("Ink"), new Vector3(-0.52f, 0.38f, 0.62f), new Vector3(0.38f, 0.46f, 0.92f));

            var silt = Mover(t, "SiltCloud");
            foreach (var (x, z, s) in new[] { (0.24f, 0.28f, 1.1f), (0.02f, 0.02f, 0.8f), (0.40f, 0.56f, 0.9f) })
            {
                Ball(silt, "Cloud", M("WoodMid"), new Vector3(x, 0.08f, z), new Vector3(0.44f * s, 0.09f, 0.40f * s));
            }

            // The rise: a column of trapped air the shell releases, drawn so
            // the *direction* of the answer is legible before it happens.
            var lift = Hidden(Mover(t, "Lift"));
            for (var i = 0; i < 5; i++)
            {
                Ball(lift, "Bubble", M("WaterBright"), new Vector3(0.24f, 0.16f + i * 0.16f, 0.30f),
                    Vector3.one * (0.07f - i * 0.008f));
            }

            Worlds.Peps(t, new Vector3(-0.46f, 0.72f, 0.62f), new Vector3(0.24f, 0.05f, 0.30f),
                new Vector3(-0.30f, 0.72f, 0.52f));
            Worlds.Slots(t, new Vector3(-0.40f, 0.05f, -1.18f), new Vector3(0.42f, 0.05f, -1.18f),
                new Vector3(0f, 0.05f, -1.44f));
            Worlds.Finish(root, Worlds.Abyss, _dir);
        }

        /// <summary>Movers: Wreck, Angler, AnglerLure, LitWreckPassage (hidden).</summary>
        private static void AbyssWreck()
        {
            var root = Worlds.Begin(Worlds.Abyss, "Wreck");
            var t = root.transform;

            var wreck = Mover(t, "Wreck");
            Box(wreck, "Hull", M("WoodDark"), new Vector3(0.02f, 0.36f, 0.86f), new Vector3(1.06f, 0.66f, 0.74f),
                new Vector3(0f, 0f, -8f));
            Box(wreck, "Keel", M("Earth"), new Vector3(0.02f, 0.06f, 0.86f), new Vector3(1.10f, 0.10f, 0.82f));
            Box(wreck, "Stern", M("EarthDark"), new Vector3(0.42f, 0.50f, 1.12f), new Vector3(0.38f, 0.44f, 0.36f));

            // Ship's timber ribs framing the crossing route.
            for (var i = 0; i < 4; i++)
            {
                var x = -0.36f + i * 0.24f;
                Box(wreck, "Rib", M("WoodMid"), new Vector3(x, 0.42f, 0.52f), new Vector3(0.06f, 0.48f, 0.06f),
                    new Vector3(0f, 0f, i % 2 == 0 ? 6f : -6f));
            }

            Box(wreck, "Mouth", M("Abyss"), new Vector3(0.04f, 0.24f, 0.50f), new Vector3(0.42f, 0.44f, 0.06f));
            foreach (var (x, y) in new[] { (-0.34f, 0.52f), (0.36f, 0.44f) })
            {
                Ball(wreck, "Porthole", M("WaterBright"), new Vector3(x, y, 0.50f),
                    new Vector3(0.10f, 0.10f, 0.04f));
            }

            // A benthic predator with a glowing lamp guarding the wreck portal.
            var angler = Mover(t, "Angler");
            angler.parent.localPosition = new Vector3(0.06f, 0f, 0.34f);
            var hover = Idle(angler, AmbientMode.Bob, 0.032f, 0.34f, Vector3.up);
            Ball(hover, "Body", M("Violet"), new Vector3(0f, 0.26f, 0f), new Vector3(0.44f, 0.34f, 0.38f));
            Box(hover, "Jaw", M("Ink"), new Vector3(0f, 0.15f, -0.14f), new Vector3(0.36f, 0.07f, 0.14f));
            for (var i = 0; i < 5; i++)
            {
                Box(hover, "Tooth", M("Cream"), new Vector3(-0.12f + i * 0.06f, 0.19f, -0.17f),
                    new Vector3(0.024f, 0.06f, 0.024f));
            }

            Ball(hover, "Eye", M("Candle"), new Vector3(-0.12f, 0.33f, -0.13f), Vector3.one * 0.065f);
            Box(hover, "Tail", M("Violet"), new Vector3(0f, 0.28f, 0.24f), new Vector3(0.16f, 0.18f, 0.14f));
            Box(hover, "Fin", M("Violet"), new Vector3(0f, 0.42f, 0.08f), new Vector3(0.04f, 0.12f, 0.18f),
                new Vector3(-12f, 0f, 0f));

            var lure = Mover(t, "AnglerLure");
            lure.parent.localPosition = new Vector3(0.06f, 0f, 0.34f);
            var wave = Idle(lure, AmbientMode.Sway, 12f, 0.42f, Vector3.forward);
            Box(wave, "Stalk", M("Violet"), new Vector3(0f, 0.48f, -0.10f), new Vector3(0.02f, 0.24f, 0.02f),
                new Vector3(-24f, 0f, 0f));
            Ball(wave, "Bulb", M("AccentLight"), new Vector3(0f, 0.60f, -0.20f), Vector3.one * 0.08f);

            // ECOSYSTEM EVENT: The illuminated wreck passage that awakens when
            // the angler is lured away.
            var litPassage = Hidden(Mover(t, "LitWreckPassage"));
            Box(litPassage, "DeckWay", M("WoodMid"), new Vector3(0.02f, 0.10f, 0.50f), new Vector3(0.88f, 0.06f, 0.68f));
            for (var i = 0; i < 6; i++)
            {
                var x = -0.38f + i * 0.15f;
                var z = 0.38f + (i % 2) * 0.24f;
                Ball(litPassage, "BiolumPolyp", i % 2 == 0 ? M("WaterBright") : M("AccentLight"),
                    new Vector3(x, 0.14f, z), Vector3.one * (0.055f + (i % 3) * 0.015f));
            }
            Ball(litPassage, "CorridorLantern", M("Candle"), new Vector3(0.04f, 0.38f, 0.52f), Vector3.one * 0.07f);
            Beam(litPassage, "LightShaft", M("WaterLight"), new Vector3(-0.28f, 0.45f, 0.46f),
                new Vector3(0.28f, 0.12f, 0.54f), 0.035f);

            Worlds.Peps(t, new Vector3(-0.34f, 0.05f, -0.58f), new Vector3(0.20f, 0.05f, 1.34f),
                new Vector3(-0.02f, 0.05f, 0.02f));
            Worlds.Slots(t, new Vector3(-0.42f, 0.05f, -1.12f), new Vector3(0.44f, 0.05f, -1.26f),
                new Vector3(0f, 0.05f, -1.46f));
            Worlds.Finish(root, Worlds.Abyss, _dir);
        }

        /// <summary>
        /// Movers: LockedChasmWorld, TamedAbyssWorld (hidden), VentChimney,
        /// TurbulentPlume, CrossCurrentRace.
        /// </summary>
        private static void AbyssCurrent()
        {
            var root = Worlds.Begin(Worlds.Abyss, "Current");
            var t = root.transform;

            // Before: A deep, jagged abyss canyon split by an active towering
            // volcanic hydrothermal spire and violent rushing cross-current torrents.
            var locked = Mover(t, "LockedChasmWorld");
            Box(locked, "LeftShelf", M("Violet"), new Vector3(-0.56f, 0.28f, 0.10f), new Vector3(0.44f, 0.14f, 1.20f));
            Box(locked, "LeftWall", M("Ink"), new Vector3(-0.66f, 0.04f, 0.10f), new Vector3(0.36f, 0.38f, 1.20f));
            Box(locked, "RightShelf", M("Violet"), new Vector3(0.56f, 0.28f, 0.34f), new Vector3(0.44f, 0.14f, 1.20f));
            Box(locked, "RightWall", M("Ink"), new Vector3(0.66f, 0.04f, 0.34f), new Vector3(0.36f, 0.38f, 1.20f));
            Box(locked, "ChasmPit", M("Abyss"), new Vector3(0f, -0.32f, 0.22f), new Vector3(1.10f, 0.40f, 1.60f));

            // Deep sunken, jagged tilted basalt slabs lying submerged below in the abyss pit.
            Box(locked, "SubmergedSlabL", M("WoodDark"), new Vector3(-0.28f, -0.22f, 0.16f),
                new Vector3(0.24f, 0.16f, 0.36f), new Vector3(0f, 0f, 14f));
            Box(locked, "SubmergedSlabR", M("WoodDark"), new Vector3(0.28f, -0.24f, 0.28f),
                new Vector3(0.24f, 0.16f, 0.36f), new Vector3(0f, 0f, -14f));

            // Towering central hydrothermal chimney spire.
            var chimney = Mover(t, "VentChimney");
            Box(chimney, "Base", M("Ink"), new Vector3(0.02f, -0.06f, 0.22f), new Vector3(0.38f, 0.22f, 0.38f));
            Rod(chimney, "TowerSpire", M("Stone"), new Vector3(0.02f, 0.26f, 0.22f), new Vector3(0.24f, 0.48f, 0.24f));
            Rod(chimney, "Nozzle", M("Ink"), new Vector3(0.02f, 0.52f, 0.22f), new Vector3(0.28f, 0.06f, 0.28f));
            Box(chimney, "ButtressL", M("EarthDark"), new Vector3(-0.12f, 0.18f, 0.22f), new Vector3(0.08f, 0.30f, 0.18f),
                new Vector3(0f, 0f, 18f));
            Box(chimney, "ButtressR", M("EarthDark"), new Vector3(0.16f, 0.18f, 0.22f), new Vector3(0.08f, 0.30f, 0.18f),
                new Vector3(0f, 0f, -18f));

            var plume = Mover(t, "TurbulentPlume");
            var plumeFlow = Living(plume, "Updraft", AmbientMode.Drift, 1.6f, 0.75f, Vector3.up, stagger: true);
            for (var i = 0; i < 9; i++)
            {
                Ball(plumeFlow, "VortexBubble", i % 2 == 0 ? M("WaterBright") : M("Candle"),
                    new Vector3(-0.14f + (i % 3) * 0.14f, 0.54f + i * 0.11f, 0.15f + (i % 2) * 0.14f),
                    Vector3.one * (0.075f + (i % 3) * 0.02f));
            }
            Box(plume, "ThermalCore", M("WaterBright"), new Vector3(0.02f, 0.72f, 0.22f), new Vector3(0.26f, 0.38f, 0.26f));

            var race = Mover(t, "CrossCurrentRace");
            var flow = Living(race, "RaceFlow", AmbientMode.Drift, 2.2f, 0.95f, Vector3.right, stagger: true);
            for (var i = 0; i < 11; i++)
            {
                Box(flow, "Streak", M("WaterLight"),
                    new Vector3(-0.76f + (i % 4) * 0.20f, 0.26f + (i / 4) * 0.18f, -0.06f + (i % 3) * 0.30f),
                    new Vector3(0.36f, 0.022f, 0.022f));
            }

            // After: WORLD EVENT. Capping the volcanic vent triggers a massive hydraulic &
            // tectonic reorganization: the tall eruptive spire collapses and submerged basalt
            // monoliths heave up from the pit, locking into a monumental stepped basalt causeway!
            var trans = Hidden(Mover(t, "TransformedChasmWorld"));
            Box(trans, "LeftShelf", M("Violet"), new Vector3(-0.56f, 0.28f, 0.10f), new Vector3(0.44f, 0.14f, 1.20f));
            Box(trans, "RightShelf", M("Violet"), new Vector3(0.56f, 0.28f, 0.34f), new Vector3(0.44f, 0.14f, 1.20f));

            // Stepped Basalt Causeway — 5 massive interlocking monoliths rising from the canyon depth.
            // Monolith 1: Left step ascent
            Box(trans, "Pillar1", M("Ink"), new Vector3(-0.35f, 0.08f, 0.14f), new Vector3(0.24f, 0.38f, 0.44f));
            Box(trans, "Cap1", M("Violet"), new Vector3(-0.35f, 0.28f, 0.14f), new Vector3(0.26f, 0.04f, 0.46f));
            Box(trans, "Trim1", M("WaterBright"), new Vector3(-0.35f, 0.30f, 0.14f), new Vector3(0.20f, 0.012f, 0.38f));

            // Monolith 2: Center-left high step
            Box(trans, "Pillar2", M("Stone"), new Vector3(-0.16f, 0.12f, 0.18f), new Vector3(0.24f, 0.44f, 0.48f));
            Box(trans, "Cap2", M("Violet"), new Vector3(-0.16f, 0.32f, 0.18f), new Vector3(0.26f, 0.04f, 0.50f));
            Box(trans, "Trim2", M("WaterBright"), new Vector3(-0.16f, 0.34f, 0.18f), new Vector3(0.20f, 0.012f, 0.42f));

            // Monolith 3: Grand Central Keystone with embedded ballast plug
            Box(trans, "Pillar3", M("Stone"), new Vector3(0.04f, 0.14f, 0.22f), new Vector3(0.30f, 0.46f, 0.54f));
            Box(trans, "Cap3", M("Violet"), new Vector3(0.04f, 0.34f, 0.22f), new Vector3(0.32f, 0.04f, 0.56f));
            Box(trans, "MooredPlug", M("Stone"), new Vector3(0.04f, 0.37f, 0.22f), new Vector3(0.22f, 0.06f, 0.22f));
            Box(trans, "KeystoneWell", M("WaterBright"), new Vector3(0.04f, 0.36f, 0.22f), new Vector3(0.26f, 0.016f, 0.46f));

            // Monolith 4: Center-right high step
            Box(trans, "Pillar4", M("Stone"), new Vector3(0.24f, 0.12f, 0.26f), new Vector3(0.24f, 0.44f, 0.48f));
            Box(trans, "Cap4", M("Violet"), new Vector3(0.24f, 0.32f, 0.26f), new Vector3(0.26f, 0.04f, 0.50f));
            Box(trans, "Trim4", M("WaterBright"), new Vector3(0.24f, 0.34f, 0.26f), new Vector3(0.20f, 0.012f, 0.42f));

            // Monolith 5: Right step ascent
            Box(trans, "Pillar5", M("Ink"), new Vector3(0.43f, 0.08f, 0.30f), new Vector3(0.24f, 0.38f, 0.44f));
            Box(trans, "Cap5", M("Violet"), new Vector3(0.43f, 0.28f, 0.30f), new Vector3(0.26f, 0.04f, 0.46f));
            Box(trans, "Trim5", M("WaterBright"), new Vector3(0.43f, 0.30f, 0.30f), new Vector3(0.20f, 0.012f, 0.38f));

            // Tectonic foundation underpinnings bridging the abyss
            Box(trans, "TectonicArch", M("WoodDark"), new Vector3(0.04f, -0.04f, 0.22f), new Vector3(1.10f, 0.16f, 0.60f));
            Box(trans, "DeepBase", M("Ink"), new Vector3(0.04f, -0.22f, 0.22f), new Vector3(1.18f, 0.22f, 0.80f));

            // Bioluminescent polyps and anemones flourishing along the newly formed basalt ridge
            for (var i = 0; i < 10; i++)
            {
                var x = -0.42f + i * 0.09f;
                var z = 0.12f + (i % 3) * 0.10f;
                Ball(trans, "LuminousPolyp", i % 2 == 0 ? M("AccentLight") : M("WaterBright"),
                    new Vector3(x, 0.32f + (i % 2) * 0.04f, z), Vector3.one * (0.05f + (i % 2) * 0.02f));
            }

            // Beacon landmarks
            Ball(trans, "LeftBeacon", M("Candle"), new Vector3(-0.56f, 0.40f, 0.10f), Vector3.one * 0.08f);
            Ball(trans, "RightBeacon", M("Candle"), new Vector3(0.58f, 0.40f, 0.34f), Vector3.one * 0.08f);
            Ball(trans, "CenterBeacon", M("Candle"), new Vector3(0.04f, 0.46f, 0.22f), Vector3.one * 0.08f);

            Worlds.Peps(t, new Vector3(-0.52f, 0.28f, 0.10f), new Vector3(0.54f, 0.28f, 0.34f),
                new Vector3(0.04f, 0.34f, 0.22f));
            Worlds.Slots(t, new Vector3(-0.40f, 0.05f, -1.20f), new Vector3(0.42f, 0.05f, -1.20f),
                new Vector3(0f, 0.05f, -1.46f));
            Worlds.Finish(root, Worlds.Abyss, _dir);
        }

        // ===================================================================
        // World 10 — Orbit. Nothing falls and nothing stops.
        // ===================================================================

        /// <summary>Movers: Adrift, SpinHalo, Tether.</summary>
        private static void OrbitDrift()
        {
            var root = Worlds.Begin(Worlds.Orbit, "Drift");
            var t = root.transform;

            // A marker ring where Pep A is hanging, with a snapped tether
            // trailing from it: the only explanation the scene needs for why
            // someone is in the middle of the air with nothing under them.
            var adrift = Mover(t, "Adrift");
            var turn = Idle(adrift, AmbientMode.Spin, 22f, 0.08f, Vector3.up);
            turn.localPosition = new Vector3(0.02f, 0.58f, 0.24f);
            BlockRing(turn, "Halo", M("Violet"), Vector3.zero, new Vector2(0.30f, 0.30f), 10, 0.02f, 0.02f);

            var tether = Mover(t, "Tether");
            Box(tether, "Cut", M("Cream"), new Vector3(0.16f, 0.44f, 0.52f), new Vector3(0.42f, 0.012f, 0.012f),
                new Vector3(0f, 34f, -26f));
            Ball(tether, "Clip", M("Stone"), new Vector3(0.32f, 0.33f, 0.68f), Vector3.one * 0.045f);

            Box(t, "PushOff", M("Accent"), new Vector3(0f, 0.16f, -0.72f), new Vector3(0.50f, 0.03f, 0.03f));

            Worlds.Peps(t, new Vector3(0.02f, 0.58f, 0.24f), new Vector3(0f, 0.05f, 1.28f),
                new Vector3(0f, 0.05f, 1.06f));
            Worlds.Slots(t, new Vector3(-0.40f, 0.05f, -1.14f), new Vector3(0.40f, 0.05f, -1.14f),
                new Vector3(0f, 0.05f, -1.50f));
            Worlds.Finish(root, Worlds.Orbit, _dir);
        }

        /// <summary>
        /// R10.2 — Open-space system event: Stranded Pep tumbling in orbital debris in deep space.
        /// Movers: Gantry, DebrisField, Backpack, MagneticFlux (hidden), Handrail.
        /// </summary>
        private static void OrbitTumble()
        {
            var root = Worlds.Begin(Worlds.Orbit, "Tumble");
            var t = root.transform;

            // Catwalk handrail along the left station truss
            var rail = Mover(t, "Handrail");
            Box(rail, "Bar", M("Accent"), new Vector3(-0.40f, 0.22f, 0.10f), new Vector3(0.03f, 0.03f, 0.82f));
            foreach (var z in new[] { -0.25f, 0.10f, 0.45f })
            {
                Box(rail, "Stanchion", M("Stone"), new Vector3(-0.40f, 0.12f, z), new Vector3(0.025f, 0.18f, 0.025f));
            }

            // Articulated Electromagnetic Recovery Crane Gantry extending diagonally into the void
            var gantry = Mover(t, "Gantry");
            Box(gantry, "Base", M("Stone"), new Vector3(-0.48f, 0.12f, 0.15f), new Vector3(0.12f, 0.14f, 0.12f));
            Box(gantry, "Mast", M("StoneLight"), new Vector3(-0.48f, 0.32f, 0.15f), new Vector3(0.06f, 0.38f, 0.06f));
            Box(gantry, "Arm", M("StoneLight"), new Vector3(-0.25f, 0.52f, 0.38f), new Vector3(0.55f, 0.04f, 0.05f),
                new Vector3(-12f, 40f, 18f));
            Box(gantry, "Strut", M("Accent"), new Vector3(-0.35f, 0.42f, 0.28f), new Vector3(0.32f, 0.025f, 0.025f),
                new Vector3(-10f, 40f, 42f));
            var coil = Box(gantry, "CoilRing", M("Violet"), new Vector3(-0.04f, 0.62f, 0.55f), new Vector3(0.14f, 0.14f, 0.08f),
                new Vector3(0f, 40f, 0f));
            Box(coil.transform, "Trim", M("AccentLight"), Vector3.zero, new Vector3(0.16f, 0.035f, 0.09f));

            // Drifting orbital debris belt tumbling across the wide diagonal void
            var debris = Mover(t, "DebrisField");
            var debrisTumble = Idle(debris, AmbientMode.Spin, 24f, 0.15f, new Vector3(0.5f, 1f, 0.3f));
            Box(debrisTumble, "PlateA", M("Stone"), new Vector3(0.18f, 0.62f, 0.88f), new Vector3(0.22f, 0.12f, 0.03f),
                new Vector3(20f, 45f, 30f));
            Box(debrisTumble, "StrutB", M("Ink"), new Vector3(0.38f, 0.78f, 1.18f), new Vector3(0.30f, 0.03f, 0.03f),
                new Vector3(-25f, 65f, 15f));
            Box(debrisTumble, "SolarShard", M("AccentLight"), new Vector3(0.48f, 0.95f, 1.35f), new Vector3(0.18f, 0.14f, 0.02f),
                new Vector3(35f, -20f, 40f));

            // Steel-backed magnetic pack where Pep B is tumbling in deep open space
            var pack = Mover(t, "Backpack");
            pack.parent.localPosition = new Vector3(0.65f, 1.05f, 1.55f);
            var tumble = Idle(pack, AmbientMode.Spin, 32f, 0.16f, new Vector3(0.4f, 1f, 0.2f));
            Box(tumble, "Shell", M("StoneLight"), Vector3.zero, new Vector3(0.24f, 0.26f, 0.14f));
            Box(tumble, "Plate", M("Stone"), new Vector3(0f, 0f, -0.075f), new Vector3(0.19f, 0.21f, 0.02f));
            foreach (var y in new[] { -0.07f, 0.07f })
            {
                Box(tumble, "Strap", M("Violet"), new Vector3(0f, y, 0.075f), new Vector3(0.20f, 0.03f, 0.02f));
            }

            // Magnetic Flux Field — multi-node high-energy arc across the vast diagonal void
            var arc = Hidden(Mover(t, "MagneticFlux"));
            for (var i = 0; i < 9; i++)
            {
                var progress = i / 8.0f;
                var arcPos = Vector3.Lerp(new Vector3(-0.04f, 0.62f, 0.55f), new Vector3(0.65f, 1.05f, 1.55f), progress);
                arcPos.y += Mathf.Sin(progress * Mathf.PI) * 0.16f;
                Ball(arc, "Node", M("WaterBright"), arcPos, Vector3.one * (0.045f + (i % 2) * 0.02f));
                if (i % 2 == 1)
                {
                    Box(arc, "Rays", M("WaterLight"), arcPos, new Vector3(0.10f, 0.10f, 0.02f));
                }
            }

            Worlds.Peps(t, new Vector3(-0.35f, 0.08f, 0.10f), new Vector3(0.65f, 1.05f, 1.55f),
                new Vector3(-0.35f, 0.08f, 0.26f));
            Worlds.Slots(t, new Vector3(-0.40f, 0.05f, -1.14f), new Vector3(0.40f, 0.05f, -1.14f),
                new Vector3(0f, 0.05f, -1.50f));
            Worlds.Finish(root, Worlds.Orbit, _dir);
        }

        /// <summary>
        /// R10.3 Station-Wide World Event: Catastrophic decompression breach transformed into docked megastructure.
        /// Movers: BreachedStationWorld, DockedStationWorld (hidden), BreachCollar, DecompressionVortex, HazardPanel.
        /// </summary>
        private static void OrbitAirlock()
        {
            var root = Worlds.Begin(Worlds.Orbit, "Airlock");
            var t = root.transform;

            // ===================================================================
            // BEFORE STATE: BreachedStationWorld
            // Visibly severed, misaligned, broken station wings separated by an empty chasm.
            // ===================================================================
            var breached = Mover(t, "BreachedStationWorld");

            // LEFT WING MODULE: Tilted habitat cylinder/box on the left
            var habWing = Child(breached, "HabWing");
            habWing.localPosition = new Vector3(-0.62f, 0.30f, 0.45f);
            habWing.localRotation = Quaternion.Euler(0f, -22f, -15f);

            Box(habWing, "HabHull", M("StoneLight"), Vector3.zero, new Vector3(0.48f, 0.58f, 0.88f));
            Box(habWing, "HabSkirt", M("Stone"), new Vector3(0f, -0.32f, 0f), new Vector3(0.44f, 0.12f, 0.82f));
            Box(habWing, "HabStripe", M("Accent"), new Vector3(0f, 0.30f, 0f), new Vector3(0.46f, 0.02f, 0.86f));
            // Deck where Pep A stands
            Box(habWing, "HabDeck", M("StoneLight"), new Vector3(0.18f, -0.15f, -0.28f), new Vector3(0.38f, 0.06f, 0.40f));
            Box(habWing, "HabRail", M("Accent"), new Vector3(0.35f, -0.05f, -0.28f), new Vector3(0.025f, 0.20f, 0.38f));

            // Jagged broken structural ribs protruding toward chasm
            Box(habWing, "BrokenRibA", M("Ink"), new Vector3(0.26f, 0.15f, 0.22f), new Vector3(0.18f, 0.04f, 0.04f),
                new Vector3(0f, 15f, -20f));
            Box(habWing, "BrokenRibB", M("Stone"), new Vector3(0.24f, -0.05f, 0.38f), new Vector3(0.16f, 0.035f, 0.035f),
                new Vector3(10f, -10f, 15f));

            // Severed umbilical power cable
            Box(habWing, "CableA", M("Accent"), new Vector3(0.28f, -0.22f, 0.15f), new Vector3(0.025f, 0.30f, 0.025f),
                new Vector3(25f, 0f, 35f));

            // Flashing amber emergency beacons on left wing
            var beaconL = Living(habWing, "BeaconL", AmbientMode.Flicker, 0.50f, 0.85f, Vector3.up);
            Ball(beaconL, "LampL", M("AccentLight"), new Vector3(-0.20f, 0.32f, 0.38f), Vector3.one * 0.08f);

            // RIGHT WING MODULE: Detached solar hub module floating far to the right and back
            var solarWing = Child(breached, "SolarWing");
            solarWing.localPosition = new Vector3(0.68f, 0.50f, 1.35f);
            solarWing.localRotation = Quaternion.Euler(18f, 26f, 12f);

            Box(solarWing, "SolarHull", M("StoneLight"), Vector3.zero, new Vector3(0.50f, 0.65f, 0.85f));
            Box(solarWing, "SolarSkirt", M("Stone"), new Vector3(0f, -0.35f, 0f), new Vector3(0.46f, 0.12f, 0.78f));
            Box(solarWing, "SolarStripe", M("Accent"), new Vector3(0f, 0.34f, 0f), new Vector3(0.48f, 0.02f, 0.82f));
            // Deck where Pep B is stranded
            Box(solarWing, "SolarDeck", M("StoneLight"), new Vector3(-0.16f, -0.20f, 0f), new Vector3(0.36f, 0.06f, 0.42f));
            Box(solarWing, "SolarRail", M("Accent"), new Vector3(-0.32f, -0.10f, 0f), new Vector3(0.025f, 0.20f, 0.40f));

            // Severed solar mast stub & broken panel
            Box(solarWing, "BrokenMast", M("Stone"), new Vector3(0.28f, 0.20f, 0f), new Vector3(0.12f, 0.45f, 0.05f),
                new Vector3(0f, 0f, -25f));
            Box(solarWing, "BrokenPanel", M("Ink"), new Vector3(0.42f, 0.35f, 0f), new Vector3(0.24f, 0.32f, 0.02f),
                new Vector3(0f, 0f, -25f));

            // Severed umbilical cable
            Box(solarWing, "CableB", M("Ink"), new Vector3(-0.26f, -0.25f, 0.18f), new Vector3(0.025f, 0.32f, 0.025f),
                new Vector3(-20f, 0f, -40f));

            // Flashing amber emergency beacon on right wing
            var beaconR = Living(solarWing, "BeaconR", AmbientMode.Flicker, 0.50f, 0.85f, Vector3.up);
            Ball(beaconR, "LampR", M("AccentLight"), new Vector3(0.22f, 0.35f, -0.32f), Vector3.one * 0.08f);

            // JAGGED AIRLOCK CONDUIT COLLAR: Framing the breach facing the vacuum chasm
            var collar = Mover(breached, "BreachCollar");
            collar.parent.localPosition = new Vector3(-0.18f, 0.26f, 0.45f);
            collar.parent.localRotation = Quaternion.Euler(0f, -22f, -15f);

            Box(collar, "FrameLeft", M("Stone"), new Vector3(-0.16f, 0f, 0f), new Vector3(0.08f, 0.45f, 0.16f));
            Box(collar, "FrameRight", M("Stone"), new Vector3(0.16f, 0f, 0f), new Vector3(0.08f, 0.45f, 0.16f));
            Box(collar, "FrameTop", M("StoneLight"), new Vector3(0f, 0.24f, 0f), new Vector3(0.40f, 0.08f, 0.18f));
            Box(collar, "HazardTrim", M("Accent"), new Vector3(0f, 0.28f, 0f), new Vector3(0.44f, 0.025f, 0.20f));
            Box(collar, "Threshold", M("Ink"), new Vector3(0f, -0.22f, 0f), new Vector3(0.38f, 0.06f, 0.16f));

            // Violent Decompression Vortex: atmosphere streaming backwards into vacuum
            var vortex = Mover(breached, "DecompressionVortex");
            var stream = Living(vortex, "Stream", AmbientMode.Drift, -1.90f, 1.05f, Vector3.forward, stagger: true);
            for (var i = 0; i < 9; i++)
            {
                var xOff = -0.18f + (i % 3 - 1) * 0.10f;
                var yOff = 0.22f + (i / 3) * 0.08f;
                Box(stream, $"Jet_{i}", M("WaterBright"), new Vector3(xOff, yOff, 0.35f),
                    new Vector3(0.035f, 0.035f, 0.40f));
            }

            // Spinning hazard panel caught in suction
            var hazard = Mover(breached, "HazardPanel");
            var hazardSpin = Idle(hazard, AmbientMode.Spin, 45f, 0.16f, new Vector3(0.5f, 0.2f, 1f));
            Box(hazardSpin, "Plate", M("StoneLight"), new Vector3(-0.08f, 0.30f, 0.22f), new Vector3(0.22f, 0.16f, 0.025f),
                new Vector3(25f, 15f, -30f));

            // ===================================================================
            // AFTER STATE: DockedStationWorld (Hidden initially)
            // Monolithic, symmetrical orbital megastructure with giant solar wings,
            // central habitat ring, and enclosed transit concourse.
            // ===================================================================
            var docked = Hidden(Mover(t, "DockedStationWorld"));

            // Central Pressurized Transit Concourse bridging the chasm
            Box(docked, "KeelTruss", M("Stone"), new Vector3(0f, -0.04f, 0.72f), new Vector3(0.85f, 0.18f, 1.68f));
            Box(docked, "ConcourseDeck", M("StoneLight"), new Vector3(0f, 0.07f, 0.72f), new Vector3(0.78f, 0.05f, 1.60f));
            Box(docked, "GuidanceTrackL", M("Accent"), new Vector3(-0.30f, 0.10f, 0.72f), new Vector3(0.04f, 0.02f, 1.56f));
            Box(docked, "GuidanceTrackR", M("Accent"), new Vector3(0.30f, 0.10f, 0.72f), new Vector3(0.04f, 0.02f, 1.56f));
            Box(docked, "CenterGlow", M("WaterBright"), new Vector3(0f, 0.098f, 0.72f), new Vector3(0.14f, 0.01f, 1.50f));

            // Colossal Habitat Docking Ring encircling the central hub
            var ring = Child(docked, "StationRing");
            ring.localPosition = new Vector3(0f, 0.58f, 0.72f);
            BlockRing(ring.transform, "Ring", M("Stone"), Vector3.zero, new Vector2(0.68f, 0.68f), 16, 0.10f, 0.12f);
            BlockRing(ring.transform, "RingTrim", M("Accent"), Vector3.zero, new Vector2(0.70f, 0.70f), 16, 0.02f, 0.13f);

            // Heavy structural containment arches enclosing the concourse
            for (var i = 0; i < 5; i++)
            {
                var zArch = -0.02f + i * 0.37f;
                Box(docked, $"ArchL_{i}", M("Stone"), new Vector3(-0.38f, 0.32f, zArch), new Vector3(0.08f, 0.46f, 0.10f));
                Box(docked, $"ArchR_{i}", M("Stone"), new Vector3(0.38f, 0.32f, zArch), new Vector3(0.08f, 0.46f, 0.10f));
                Box(docked, $"ArchTop_{i}", M("StoneLight"), new Vector3(0f, 0.56f, zArch), new Vector3(0.82f, 0.08f, 0.10f));
                Box(docked, $"Viewport_{i}", M("WaterLight"), new Vector3(0f, 0.54f, zArch), new Vector3(0.56f, 0.02f, 0.08f));
            }

            // Massive Twin Outward Solar Wings spanning the full frame
            foreach (var (sign, name) in new[] { (-1f, "SolarWingL"), (1f, "SolarWingR") })
            {
                var wingX = sign * 0.98f;
                Box(docked, $"{name}_Truss", M("Stone"), new Vector3(wingX, 0.75f, 0.72f), new Vector3(0.85f, 0.06f, 0.06f));
                Box(docked, $"{name}_Panel", M("Ink"), new Vector3(wingX, 0.75f, 0.72f), new Vector3(0.78f, 0.36f, 0.03f));
                Box(docked, $"{name}_Cells", M("AccentLight"), new Vector3(wingX, 0.75f, 0.74f), new Vector3(0.72f, 0.30f, 0.015f));
            }

            // Motorized Hydraulic Docking Clamps locked into place
            foreach (var (cx, sign) in new[] { (-0.45f, 1f), (0.45f, -1f) })
            {
                Box(docked, "ClampArm", M("Stone"), new Vector3(cx, 0.32f, 0.72f), new Vector3(0.12f, 0.52f, 0.20f));
                Box(docked, "ClampPiston", M("Accent"), new Vector3(cx, 0.32f, 0.72f), new Vector3(0.06f, 0.38f, 0.12f));
                Ball(docked, "ClampJoint", M("AccentDeep"), new Vector3(cx, 0.58f, 0.72f), Vector3.one * 0.12f);
            }

            // High-gain orbital telemetry antenna atop the locked module
            Rod(docked, "AntennaMast", M("StoneLight"), new Vector3(0f, 1.15f, 0.72f), new Vector3(0.035f, 0.34f, 0.035f));
            Ball(docked, "AntennaDish", M("WaterBright"), new Vector3(0f, 1.34f, 0.72f), Vector3.one * 0.12f);

            Worlds.Peps(t, new Vector3(-0.42f, 0.10f, 0.10f), new Vector3(0.55f, 0.24f, 1.35f),
                new Vector3(0f, 0.10f, 0.72f));
            Worlds.Slots(t, new Vector3(-0.40f, 0.05f, -1.16f), new Vector3(0.40f, 0.05f, -1.16f),
                new Vector3(0f, 0.05f, -1.50f));
            Worlds.Finish(root, Worlds.Orbit, _dir);
        }

        // ===================================================================
        // World 11 — Foundry. The machine is running; time it, feed it or jam it.
        // ===================================================================

        /// <summary>Movers: Belt, Scanner, ScanBeam (hidden), ShutterGate.</summary>
        private static void ForgeConveyor()
        {
            var root = Worlds.Begin(Worlds.Forge, "Conveyor");
            var t = root.transform;

            var belt = Mover(t, "Belt");
            Box(belt, "Frame", M("Ink"), new Vector3(0f, 0.16f, 0.02f), new Vector3(0.60f, 0.10f, 2.30f));
            var slats = Living(belt, "Slats", AmbientMode.Drift, 0.34f, 0.55f, Vector3.forward, stagger: true);
            for (var i = 0; i < 8; i++)
            {
                Box(slats, "Slat", i % 2 == 0 ? M("Stone") : M("StoneLight"),
                    new Vector3(0f, 0.215f, -1.05f + i * 0.30f), new Vector3(0.56f, 0.03f, 0.20f));
            }

            foreach (var z in new[] { -1.06f, 1.06f })
            {
                Rod(t, "Roller", M("Accent"), new Vector3(0f, 0.20f, z), new Vector3(0.14f, 0.30f, 0.14f),
                    new Vector3(0f, 0f, 90f));
            }

            var scanner = Mover(t, "Scanner");
            foreach (var x in new[] { -0.42f, 0.42f })
            {
                Box(scanner, "Post", M("Stone"), new Vector3(x, 0.44f, 0.70f), new Vector3(0.07f, 0.60f, 0.09f));
            }

            Box(scanner, "Head", M("StoneLight"), new Vector3(0f, 0.76f, 0.70f), new Vector3(0.92f, 0.11f, 0.12f));
            Ball(scanner, "Eye", M("Ink"), new Vector3(0f, 0.70f, 0.63f), new Vector3(0.14f, 0.10f, 0.04f));

            var scan = Hidden(Mover(t, "ScanBeam"));
            Box(scan, "Sheet", M("AccentLight"), new Vector3(0f, 0.46f, 0.70f), new Vector3(0.84f, 0.50f, 0.02f));

            var gate = Mover(t, "ShutterGate");
            Box(gate, "Shutter", M("Stone"), new Vector3(0f, 0.50f, 1.42f), new Vector3(1.10f, 0.62f, 0.08f));
            for (var i = 0; i < 4; i++)
            {
                Box(gate, "Corrugation", M("Ink"), new Vector3(0f, 0.28f + i * 0.15f, 1.37f),
                    new Vector3(1.08f, 0.03f, 0.02f));
            }

            Worlds.Peps(t, new Vector3(-0.38f, 0.195f, -1.02f), new Vector3(0.34f, 0.195f, 1.24f),
                new Vector3(0f, 0.195f, 0.98f));
            Worlds.Slots(t, new Vector3(-0.48f, 0.195f, -1.40f), new Vector3(0.48f, 0.195f, -1.40f),
                new Vector3(0f, 0.195f, -1.12f));
            Worlds.Finish(root, Worlds.Forge, _dir);
        }

        /// <summary>Movers: Spill, Ladle, Crust (hidden), Steam (hidden).</summary>
        private static void ForgeSpill()
        {
            var root = Worlds.Begin(Worlds.Forge, "Spill");
            var t = root.transform;

            var ladle = Mover(t, "Ladle");
            Rod(ladle, "Pot", M("Stone"), new Vector3(-0.66f, 0.70f, 0.30f), new Vector3(0.46f, 0.26f, 0.46f),
                new Vector3(0f, 0f, 38f));
            Rod(ladle, "PotLip", M("StoneLight"), new Vector3(-0.78f, 0.88f, 0.30f), new Vector3(0.40f, 0.03f, 0.40f),
                new Vector3(0f, 0f, 38f));
            Box(ladle, "Arm", M("StoneLight"), new Vector3(-0.32f, 0.84f, 0.30f), new Vector3(0.46f, 0.06f, 0.06f));
            Box(ladle, "Drip", M("AccentDeep"), new Vector3(-0.74f, 0.52f, 0.30f), new Vector3(0.06f, 0.34f, 0.06f));

            // A river of metal across the only walkway, drawn hot in the
            // middle and cooling at the edges so "solid" is a believable end
            // state rather than a colour change.
            var spill = Mover(t, "Spill");
            var glow = Idle(spill, AmbientMode.Pulse, 0.035f, 0.44f, Vector3.up);
            Box(glow, "Flow", M("AccentDeep"), new Vector3(0.02f, 0.20f, 0.02f), new Vector3(1.06f, 0.05f, 0.86f));
            Box(glow, "Core", M("AccentLight"), new Vector3(0.02f, 0.23f, 0.02f), new Vector3(0.74f, 0.02f, 0.54f));
            foreach (var (x, z) in new[] { (-0.42f, -0.26f), (0.40f, 0.30f) })
            {
                Ball(glow, "Skin", M("EarthDark"), new Vector3(x, 0.225f, z), new Vector3(0.30f, 0.02f, 0.24f));
            }

            var crust = Hidden(Mover(t, "Crust"));
            Box(crust, "Solid", M("Abyss"), new Vector3(0.02f, 0.215f, 0.02f), new Vector3(1.08f, 0.06f, 0.88f));
            for (var i = 0; i < 4; i++)
            {
                Box(crust, "Seam", M("AccentDeep"), new Vector3(-0.36f + i * 0.26f, 0.248f, 0.02f),
                    new Vector3(0.03f, 0.01f, 0.80f), new Vector3(0f, i * 6f - 9f, 0f));
            }

            var steam = Hidden(Mover(t, "Steam"));
            foreach (var (x, y, s) in new[] { (-0.30f, 0.42f, 0.34f), (0.06f, 0.54f, 0.42f), (0.36f, 0.40f, 0.30f) })
            {
                Ball(steam, "Cloud", M("StoneLight"), new Vector3(x, y, 0.02f), Vector3.one * s);
            }

            Worlds.Peps(t, new Vector3(-0.32f, 0.195f, -0.96f), new Vector3(0.30f, 0.195f, 1.18f),
                new Vector3(0.02f, 0.195f, 0.92f));
            Worlds.Slots(t, new Vector3(-0.48f, 0.195f, -1.36f), new Vector3(0.48f, 0.195f, -1.44f),
                new Vector3(0f, 0.195f, -1.10f));
            Worlds.Finish(root, Worlds.Forge, _dir);
        }

        /// <summary>Movers: ActivePressComplex, TransformedForgeWorld (hidden), Linkage, Sparks (hidden), SteamBurst (hidden).</summary>
        private static void ForgePiston()
        {
            var root = Worlds.Begin(Worlds.Forge, "Piston");
            var t = root.transform;

            // BEFORE STATE: A violent, roaring heavy hydraulic press hammering rhythmically over
            // the smelting furnace abyss with dual spinning gear trains, glowing billet on anvil, and venting steam.
            var active = Mover(t, "ActivePressComplex");

            // Heavy steel press upright columns & cross-braced arch
            Box(active, "FrameL", M("Stone"), new Vector3(-0.42f, 0.98f, 0.08f), new Vector3(0.14f, 1.48f, 0.18f));
            Box(active, "FrameR", M("Stone"), new Vector3(0.42f, 0.98f, 0.08f), new Vector3(0.14f, 1.48f, 0.18f));
            Box(active, "TopArch", M("Ink"), new Vector3(0f, 1.68f, 0.08f), new Vector3(1.02f, 0.18f, 0.24f));
            Box(active, "ArchTrim", M("Accent"), new Vector3(0f, 1.76f, 0.08f), new Vector3(0.96f, 0.04f, 0.22f));

            // Diagonal truss braces
            Box(active, "BraceL", M("Accent"), new Vector3(-0.32f, 1.54f, 0.08f), new Vector3(0.24f, 0.05f, 0.05f),
                new Vector3(0f, 0f, 38f));
            Box(active, "BraceR", M("Accent"), new Vector3(0.32f, 1.54f, 0.08f), new Vector3(0.24f, 0.05f, 0.05f),
                new Vector3(0f, 0f, -38f));

            // Main hydraulic cylinder & chrome tie rods
            Box(active, "Cylinder", M("Stone"), new Vector3(0f, 1.34f, 0.08f), new Vector3(0.46f, 0.54f, 0.46f));
            Box(active, "CylinderBand", M("Accent"), new Vector3(0f, 1.10f, 0.08f), new Vector3(0.50f, 0.06f, 0.50f));
            Box(active, "TieRodL", M("StoneLight"), new Vector3(-0.28f, 1.16f, 0.08f), new Vector3(0.04f, 0.96f, 0.04f));
            Box(active, "TieRodR", M("StoneLight"), new Vector3(0.28f, 1.16f, 0.08f), new Vector3(0.04f, 0.96f, 0.04f));

            // Base Anvil & White-Hot Forging Ingot Billet
            Box(active, "AnvilBase", M("Ink"), new Vector3(0f, 0.24f, 0.08f), new Vector3(0.62f, 0.24f, 0.62f));
            Box(active, "AnvilCap", M("EarthDark"), new Vector3(0f, 0.38f, 0.08f), new Vector3(0.52f, 0.06f, 0.52f));
            var ingotShimmer = Living(active, "HotBillet", AmbientMode.Pulse, 0.04f, 0.25f, Vector3.up);
            Box(ingotShimmer, "BilletCore", M("Candle"), new Vector3(0f, 0.43f, 0.08f), new Vector3(0.28f, 0.08f, 0.28f));
            Box(ingotShimmer, "BilletGlow", M("AccentLight"), new Vector3(0f, 0.43f, 0.08f), new Vector3(0.34f, 0.04f, 0.34f));

            // Hammering heavy ram with rhythmic pounding stroke
            var stroke = Living(active, "Stroke", AmbientMode.Beat, -0.42f, 0.65f, Vector3.up);
            Box(stroke, "RamShaft", M("StoneLight"), new Vector3(0f, 0.94f, 0.08f), new Vector3(0.18f, 0.42f, 0.18f));
            Box(stroke, "HammerHead", M("Ink"), new Vector3(0f, 0.68f, 0.08f), new Vector3(0.52f, 0.18f, 0.52f));
            Box(stroke, "DiePlate", M("Accent"), new Vector3(0f, 0.57f, 0.08f), new Vector3(0.46f, 0.04f, 0.46f));

            // Dual high-speed spinning transmission flywheels & gear trains
            var flywheelR = Living(active, "FlywheelR", AmbientMode.Spin, 360f, 0.85f, Vector3.forward);
            Rod(flywheelR, "WheelR", M("Accent"), new Vector3(0.46f, 1.50f, 0.08f), new Vector3(0.38f, 0.06f, 0.38f),
                new Vector3(90f, 0f, 0f));
            Box(flywheelR, "SpokeR1", M("StoneLight"), new Vector3(0.46f, 1.50f, 0.08f), new Vector3(0.36f, 0.04f, 0.03f));
            Box(flywheelR, "SpokeR2", M("StoneLight"), new Vector3(0.46f, 1.50f, 0.08f), new Vector3(0.04f, 0.36f, 0.03f));

            var flywheelL = Living(active, "FlywheelL", AmbientMode.Spin, -360f, 0.85f, Vector3.forward);
            Rod(flywheelL, "WheelL", M("Accent"), new Vector3(-0.46f, 1.50f, 0.08f), new Vector3(0.38f, 0.06f, 0.38f),
                new Vector3(90f, 0f, 0f));
            Box(flywheelL, "SpokeL1", M("StoneLight"), new Vector3(-0.46f, 1.50f, 0.08f), new Vector3(0.36f, 0.04f, 0.03f));
            Box(flywheelL, "SpokeL2", M("StoneLight"), new Vector3(-0.46f, 1.50f, 0.08f), new Vector3(0.04f, 0.36f, 0.03f));

            // Exhaust steam stacks with rhythmic venting
            Rod(active, "StackL", M("Ink"), new Vector3(-0.30f, 1.82f, 0.08f), new Vector3(0.08f, 0.20f, 0.08f));
            Rod(active, "StackR", M("Ink"), new Vector3(0.30f, 1.82f, 0.08f), new Vector3(0.08f, 0.20f, 0.08f));
            var steamFlow = Living(active, "ExhaustSteam", AmbientMode.Pulse, 0.06f, 0.38f, Vector3.up);
            Ball(steamFlow, "PuffL", M("StoneLight"), new Vector3(-0.30f, 1.96f, 0.08f), Vector3.one * 0.14f);
            Ball(steamFlow, "PuffR", M("StoneLight"), new Vector3(0.30f, 1.96f, 0.08f), Vector3.one * 0.14f);

            // Retracted overhead catwalk suspended high under crane hoists
            Box(active, "SuspendedCatwalk", M("Stone"), new Vector3(0f, 1.76f, 0.08f), new Vector3(0.56f, 0.06f, 1.40f));
            Box(active, "HoistCableL", M("StoneLight"), new Vector3(-0.24f, 1.88f, 0.08f), new Vector3(0.02f, 0.20f, 0.02f));
            Box(active, "HoistCableR", M("StoneLight"), new Vector3(0.24f, 1.88f, 0.08f), new Vector3(0.02f, 0.20f, 0.02f));

            // Linkage Mover
            var linkage = Mover(t, "Linkage");
            Box(linkage, "Arm", M("Accent"), new Vector3(0.40f, 0.96f, 0.08f), new Vector3(0.40f, 0.06f, 0.06f),
                new Vector3(0f, 0f, -18f));
            Rod(linkage, "Pin", M("AccentLight"), new Vector3(0.22f, 1.01f, 0.08f),
                new Vector3(0.09f, 0.05f, 0.09f), new Vector3(90f, 0f, 0f));
            Box(linkage, "Yoke", M("Ink"), new Vector3(0.56f, 0.90f, 0.08f), new Vector3(0.10f, 0.22f, 0.10f));

            // Multi-directional sparks burst on jam
            var sparks = Hidden(Mover(t, "Sparks"));
            for (var i = 0; i < 14; i++)
            {
                var a = i * Mathf.PI * 2f / 14f;
                var dist = 0.18f + (i % 3) * 0.08f;
                Ball(sparks, "Spark", i % 2 == 0 ? M("Candle") : M("AccentLight"),
                    new Vector3(0.22f + Mathf.Cos(a) * dist, 1.01f + Mathf.Sin(a) * dist, 0.08f + Mathf.Sin(a * 2f) * 0.10f),
                    Vector3.one * (0.045f + (i % 2) * 0.02f));
            }

            // Giant emergency steam blowout
            var steamBurst = Hidden(Mover(t, "SteamBurst"));
            for (var i = 0; i < 10; i++)
            {
                var xOff = -0.32f + (i % 5) * 0.16f;
                var yOff = 1.60f + i * 0.08f;
                Ball(steamBurst, "BurstCloud", M("StoneLight"),
                    new Vector3(xOff, yOff, 0.08f), Vector3.one * (0.22f + (i % 3) * 0.06f));
            }

            // AFTER STATE: WORLD EVENT. Jamming the linkage trips factory emergency brakes, locks the press high,
            // blows safety valves, and swings down a grand illuminated industrial steel-truss suspension skywalk!
            var trans = Hidden(Mover(t, "TransformedForgeWorld"));

            // Locked press frame & wedged ram jammed high
            Box(trans, "FrameL", M("Stone"), new Vector3(-0.42f, 0.98f, 0.08f), new Vector3(0.14f, 1.48f, 0.18f));
            Box(trans, "FrameR", M("Stone"), new Vector3(0.42f, 0.98f, 0.08f), new Vector3(0.14f, 1.48f, 0.18f));
            Box(trans, "TopArch", M("Ink"), new Vector3(0f, 1.68f, 0.08f), new Vector3(1.02f, 0.18f, 0.24f));
            Box(trans, "ArchTrim", M("Accent"), new Vector3(0f, 1.76f, 0.08f), new Vector3(0.96f, 0.04f, 0.22f));

            // Emergency flashing amber hazard beacons
            Ball(trans, "BeaconL", M("Candle"), new Vector3(-0.42f, 1.84f, 0.08f), Vector3.one * 0.09f);
            Ball(trans, "BeaconR", M("Candle"), new Vector3(0.42f, 1.84f, 0.08f), Vector3.one * 0.09f);

            Box(trans, "Cylinder", M("Stone"), new Vector3(0f, 1.34f, 0.08f), new Vector3(0.46f, 0.54f, 0.46f));
            Box(trans, "CylinderBand", M("Accent"), new Vector3(0f, 1.10f, 0.08f), new Vector3(0.50f, 0.06f, 0.50f));
            Box(trans, "RamShaft", M("StoneLight"), new Vector3(0f, 1.22f, 0.08f), new Vector3(0.18f, 0.42f, 0.18f));
            Box(trans, "HammerHead", M("Ink"), new Vector3(0f, 1.10f, 0.08f), new Vector3(0.52f, 0.18f, 0.52f));
            Box(trans, "DiePlate", M("Accent"), new Vector3(0f, 0.99f, 0.08f), new Vector3(0.46f, 0.04f, 0.46f));

            // Jammed linkage with mangled spanner
            Box(trans, "JammedArm", M("Accent"), new Vector3(0.40f, 0.98f, 0.08f), new Vector3(0.40f, 0.06f, 0.06f),
                new Vector3(0f, 0f, -5f));
            Rod(trans, "JammedPin", M("AccentLight"), new Vector3(0.22f, 1.00f, 0.08f), new Vector3(0.09f, 0.05f, 0.09f),
                new Vector3(90f, 0f, 0f));

            // Locked flywheels
            Rod(trans, "WheelR", M("Accent"), new Vector3(0.46f, 1.50f, 0.08f), new Vector3(0.38f, 0.06f, 0.38f),
                new Vector3(90f, 0f, 15f));
            Rod(trans, "WheelL", M("Accent"), new Vector3(-0.46f, 1.50f, 0.08f), new Vector3(0.38f, 0.06f, 0.38f),
                new Vector3(90f, 0f, -15f));

            // Grand Steel Truss Suspension Skywalk Bridge spanning lower intake to upper observation tower
            Box(trans, "BridgeDeck", M("Stone"), new Vector3(0f, 0.37f, 0.18f), new Vector3(0.64f, 0.06f, 1.84f),
                new Vector3(11.5f, 0f, 0f));
            Box(trans, "RailingL", M("Accent"), new Vector3(-0.32f, 0.51f, 0.18f), new Vector3(0.04f, 0.24f, 1.84f),
                new Vector3(11.5f, 0f, 0f));
            Box(trans, "RailingR", M("Accent"), new Vector3(0.32f, 0.51f, 0.18f), new Vector3(0.04f, 0.24f, 1.84f),
                new Vector3(11.5f, 0f, 0f));

            // Steel diamond tread plates with hazard indicator edges
            for (var i = 0; i < 7; i++)
            {
                var stepZ = -0.65f + i * 0.26f;
                var stepY = 0.21f + i * 0.050f;
                Box(trans, "TreadPlate", M("StoneLight"),
                    new Vector3(0f, stepY, stepZ), new Vector3(0.58f, 0.016f, 0.18f), new Vector3(11.5f, 0f, 0f));
                // Illuminated runway edge beacons
                Ball(trans, "EdgeLightL", M("Candle"),
                    new Vector3(-0.29f, stepY + 0.03f, stepZ), Vector3.one * 0.035f);
                Ball(trans, "EdgeLightR", M("Candle"),
                    new Vector3(0.29f, stepY + 0.03f, stepZ), Vector3.one * 0.035f);
            }

            // Heavy suspension cables anchoring bridge to upper crane girders
            for (var i = 0; i < 3; i++)
            {
                var cableZ = -0.30f + i * 0.44f;
                var cableY = 0.30f + i * 0.09f;
                Rod(trans, "SuspensionCableL", M("StoneLight"),
                    new Vector3(-0.32f, cableY + 0.52f, cableZ), new Vector3(0.02f, 1.05f, 0.02f),
                    new Vector3(12f, 0f, -8f));
                Rod(trans, "SuspensionCableR", M("StoneLight"),
                    new Vector3(0.32f, cableY + 0.52f, cableZ), new Vector3(0.02f, 1.05f, 0.02f),
                    new Vector3(12f, 0f, 8f));
            }

            // Hydraulic shock absorbers & foundation anchors
            Rod(trans, "DamperL", M("StoneLight"), new Vector3(-0.36f, 0.22f, 0.18f), new Vector3(0.09f, 0.34f, 0.09f));
            Rod(trans, "DamperR", M("StoneLight"), new Vector3(0.36f, 0.22f, 0.18f), new Vector3(0.09f, 0.34f, 0.09f));

            Worlds.Peps(t, new Vector3(-0.34f, 0.195f, -0.92f), new Vector3(0.28f, 0.535f, 1.20f),
                new Vector3(0f, 0.365f, 0.18f));
            Worlds.Slots(t, new Vector3(-0.48f, 0.195f, -1.38f), new Vector3(0.48f, 0.195f, -1.30f),
                new Vector3(0f, 0.195f, -1.10f));
            Worlds.Finish(root, Worlds.Forge, _dir);
        }

        // ===================================================================
        // World 12 — Neon skyline. The city moves; catch it at the right moment.
        // ===================================================================

        /// <summary>Movers: SignFrame, Socket, SignGlow (hidden), FireEscape (hidden), AlleyDark.</summary>
        private static void NeonSign()
        {
            var root = Worlds.Begin(Worlds.Neon, "Sign");
            var t = root.transform;

            var frame = Mover(t, "SignFrame");
            Box(frame, "Panel", M("Ink"), new Vector3(0.16f, 1.16f, 0.28f), new Vector3(0.92f, 0.86f, 0.10f));
            Box(frame, "Bezel", M("Stone"), new Vector3(0.16f, 1.16f, 0.33f), new Vector3(0.98f, 0.92f, 0.03f));
            foreach (var (x, y) in new[] { (-0.16f, 1.34f), (0.24f, 1.34f), (0.04f, 1.02f) })
            {
                Box(frame, "DeadTube", M("Violet"), new Vector3(x, y, 0.22f), new Vector3(0.05f, 0.30f, 0.05f),
                    new Vector3(0f, 0f, 18f));
            }

            var socket = Mover(t, "Socket");
            Box(socket, "Housing", M("Stone"), new Vector3(0.44f, 1.05f, 0.22f), new Vector3(0.13f, 0.30f, 0.09f));
            Box(socket, "Empty", M("Abyss"), new Vector3(0.44f, 1.05f, 0.17f), new Vector3(0.07f, 0.24f, 0.03f));

            var glow = Hidden(Mover(t, "SignGlow"));
            Box(glow, "Wash", M("WaterBright"), new Vector3(0.16f, 1.16f, 0.20f), new Vector3(1.00f, 0.94f, 0.02f));
            foreach (var (x, y) in new[] { (-0.16f, 1.34f), (0.24f, 1.34f), (0.04f, 1.02f), (0.44f, 1.05f) })
            {
                Box(glow, "LitTube", M("AccentLight"), new Vector3(x, y, 0.18f), new Vector3(0.07f, 0.32f, 0.05f),
                    new Vector3(0f, 0f, 18f));
            }

            // The alley reads as a hole in the world until the sign lights it,
            // and then it is obviously a staircase.
            var dark = Mover(t, "AlleyDark");
            Box(dark, "Shadow", M("Abyss"), new Vector3(-0.06f, 0.30f, -0.42f), new Vector3(1.10f, 0.90f, 1.10f));

            var stair = Hidden(Mover(t, "FireEscape"));
            for (var i = 0; i < 5; i++)
            {
                Box(stair, "Tread", M("Stone"), new Vector3(-0.22f + i * 0.11f, 0.10f + i * 0.14f, -0.62f + i * 0.20f),
                    new Vector3(0.34f, 0.02f, 0.16f));
                Box(stair, "Rail", M("StoneLight"),
                    new Vector3(-0.22f + i * 0.11f, 0.20f + i * 0.14f, -0.70f + i * 0.20f),
                    new Vector3(0.34f, 0.015f, 0.015f));
            }

            Worlds.Peps(t, new Vector3(-0.22f, 0.045f, -1.06f), new Vector3(0.30f, 0.665f, 0.32f),
                new Vector3(0.16f, 0.665f, 0.16f));
            Worlds.Slots(t, new Vector3(-0.42f, 0.045f, -1.44f), new Vector3(0.42f, 0.045f, -1.44f),
                new Vector3(0f, 0.045f, -1.14f));
            Worlds.Finish(root, Worlds.Neon, _dir);
        }

        /// <summary>Movers: TramCar, RailSpark (hidden), SignalLight (hidden), Ledge.</summary>
        private static void NeonTransit()
        {
            var root = Worlds.Begin(Worlds.Neon, "Transit");
            var t = root.transform;

            // Overhead transit girder & gantry
            var track = Child(t, "TransitTrack");
            Box(track, "Beam", M("Stone"), new Vector3(0f, 0.79f, 0.62f), new Vector3(3.60f, 0.08f, 0.14f));
            Box(track, "PowerRail", M("AccentLight"), new Vector3(0f, 0.745f, 0.62f), new Vector3(3.60f, 0.025f, 0.04f));
            Box(track, "Gantry_L", M("Stone"), new Vector3(-1.15f, 0.40f, 0.62f), new Vector3(0.09f, 0.85f, 0.09f));
            Box(track, "Gantry_R", M("Stone"), new Vector3(1.15f, 0.40f, 0.62f), new Vector3(0.09f, 0.85f, 0.09f));
            Ball(track, "Beacon_L", M("AccentDeep"), new Vector3(-1.15f, 0.84f, 0.62f), Vector3.one * 0.06f);
            Ball(track, "Beacon_R", M("AccentDeep"), new Vector3(1.15f, 0.84f, 0.62f), Vector3.one * 0.06f);

            var signal = Hidden(Mover(t, "SignalLight"));
            Ball(signal, "Green_L", M("WaterBright"), new Vector3(-1.15f, 0.84f, 0.62f), Vector3.one * 0.08f);
            Ball(signal, "Green_R", M("WaterBright"), new Vector3(1.15f, 0.84f, 0.62f), Vector3.one * 0.08f);

            var ledge = Mover(t, "Ledge");
            Box(ledge, "Lip", M("Stone"), new Vector3(-0.22f, 0.10f, -0.66f), new Vector3(0.66f, 0.06f, 0.10f));
            Box(ledge, "Stripe", M("Candle"), new Vector3(-0.22f, 0.12f, -0.62f), new Vector3(0.60f, 0.01f, 0.03f));

            // Aerodynamic Sky-Tram car that locks onto the rail and accelerates across the skyway
            var tram = Mover(t, "TramCar");
            tram.parent.localPosition = new Vector3(-0.85f, 0.68f, 0.62f);
            Box(tram, "Car", M("StoneLight"), Vector3.zero, new Vector3(0.68f, 0.24f, 0.24f), new Vector3(0f, 10f, 0f));
            Box(tram, "Skirt", M("Stone"), new Vector3(0f, -0.13f, 0f), new Vector3(0.62f, 0.06f, 0.20f),
                new Vector3(0f, 10f, 0f));
            Box(tram, "Windows", M("WaterBright"), new Vector3(0f, 0.03f, -0.125f), new Vector3(0.56f, 0.11f, 0.02f),
                new Vector3(0f, 10f, 0f));
            Box(tram, "Headlight", M("Candle"), new Vector3(0.35f, -0.02f, 0f), Vector3.one * 0.07f);
            Box(tram, "Grab", M("AccentDeep"), new Vector3(0f, 0.15f, 0f), new Vector3(0.58f, 0.035f, 0.035f),
                new Vector3(0f, 10f, 0f));
            Box(tram, "Hanger", M("Stone"), new Vector3(0f, 0.115f, 0f), new Vector3(0.07f, 0.09f, 0.07f));

            var spark = Hidden(Mover(t, "RailSpark"));
            for (var i = 0; i < 10; i++)
            {
                var x = -0.50f + (i % 5) * 0.25f;
                var y = 0.70f + (i / 5) * 0.08f;
                Ball(spark, "Flash", i % 2 == 0 ? M("AccentLight") : M("WaterBright"),
                    new Vector3(x, y, 0.58f), Vector3.one * 0.065f);
            }

            Worlds.Peps(t, new Vector3(-0.28f, 0.045f, -0.98f), new Vector3(0.34f, 0.665f, 0.36f),
                new Vector3(0.18f, 0.665f, 0.20f));
            Worlds.Slots(t, new Vector3(-0.42f, 0.045f, -1.40f), new Vector3(0.42f, 0.045f, -1.34f),
                new Vector3(0f, 0.045f, -1.12f));
            Worlds.Finish(root, Worlds.Neon, _dir);
        }

        /// <summary>Movers: Antenna, VentGlow (hidden), HelipadBeacon (hidden), Searchlight_L (hidden), Searchlight_R (hidden), FireworksPrimary (hidden), FireworksSecondary (hidden), CelebrationSparks (hidden), Skyline.</summary>
        private static void NeonSkyline()
        {
            var root = Worlds.Begin(Worlds.Neon, "Skyline");
            var t = root.transform;

            // Launch deck details on mid block with thermal ventilation exhaust
            var launch = Child(t, "LaunchRooftop");
            Box(launch, "Rail_L", M("Stone"), new Vector3(0.04f, 0.72f, 0.28f), new Vector3(0.02f, 0.12f, 0.86f));
            Box(launch, "Rail_R", M("Stone"), new Vector3(0.56f, 0.72f, 0.28f), new Vector3(0.02f, 0.12f, 0.86f));
            Ball(launch, "Light_L", M("Candle"), new Vector3(0.04f, 0.79f, 0.64f), Vector3.one * 0.045f);
            Ball(launch, "Light_R", M("Candle"), new Vector3(0.56f, 0.79f, 0.64f), Vector3.one * 0.045f);

            // Active rooftop ventilation exhaust turbine
            Box(launch, "VentCore", M("Stone"), new Vector3(0.28f, 0.68f, 0.28f), new Vector3(0.36f, 0.06f, 0.36f));
            Box(launch, "VentGrille", M("AccentDeep"), new Vector3(0.28f, 0.715f, 0.28f), new Vector3(0.30f, 0.02f, 0.30f));

            // Thermal updraft rising column
            var ventGlow = Hidden(Mover(t, "VentGlow"));
            for (var i = 0; i < 4; i++)
            {
                var y = 0.80f + i * 0.22f;
                Box(ventGlow, "Ring", i % 2 == 0 ? M("WaterBright") : M("AccentLight"),
                    new Vector3(0.28f, y, 0.28f), new Vector3(0.28f + i * 0.04f, 0.02f, 0.28f + i * 0.04f));
            }

            // High summit antenna & communication mast
            var antenna = Mover(t, "Antenna");
            Box(antenna, "Mast", M("Stone"), new Vector3(-0.36f, 1.82f, 1.34f), new Vector3(0.055f, 0.96f, 0.055f));
            foreach (var y in new[] { 1.55f, 1.78f, 2.02f, 2.22f })
            {
                Box(antenna, "Ring", M("AccentDeep"), new Vector3(-0.36f, y, 1.34f),
                    new Vector3(0.26f - (y - 1.55f) * 0.22f, 0.02f, 0.26f - (y - 1.55f) * 0.22f));
            }
            Ball(antenna, "Beacon", M("AccentLight"), new Vector3(-0.36f, 2.34f, 1.34f), Vector3.one * 0.09f);

            // Summit helipad with beacon array
            Box(t, "LandingPad", M("Stone"), new Vector3(-0.36f, 1.30f, 1.34f), new Vector3(0.72f, 0.04f, 0.72f));
            Box(t, "PadH", M("Candle"), new Vector3(-0.36f, 1.325f, 1.34f), new Vector3(0.42f, 0.01f, 0.08f));
            Box(t, "PadH_L", M("Candle"), new Vector3(-0.53f, 1.325f, 1.34f), new Vector3(0.08f, 0.01f, 0.38f));
            Box(t, "PadH_R", M("Candle"), new Vector3(-0.19f, 1.325f, 1.34f), new Vector3(0.08f, 0.01f, 0.38f));

            foreach (var i in new[] { 0, 1, 2, 3 })
            {
                var px = -0.36f + (i % 2 == 0 ? -0.30f : 0.30f);
                var pz = 1.34f + (i < 2 ? -0.30f : 0.30f);
                Ball(t, "PadLight", M("WaterBright"), new Vector3(px, 1.33f, pz), Vector3.one * 0.05f);
            }

            // Helipad arrival beacon burst
            var helipadBeacon = Hidden(Mover(t, "HelipadBeacon"));
            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI / 4f;
                Ball(helipadBeacon, "BeaconFlare", i % 2 == 0 ? M("WaterBright") : M("Candle"),
                    new Vector3(-0.36f + Mathf.Cos(a) * 0.34f, 1.34f, 1.34f + Mathf.Sin(a) * 0.34f),
                    Vector3.one * 0.065f);
            }

            // Dense metropolitan skyline towers with glowing crowns & ribbons
            var skyline = Mover(t, "Skyline");
            for (var i = 0; i < 7; i++)
            {
                var x = -1.25f + i * 0.42f;
                var z = 1.95f + (i % 2) * 0.22f;
                var h = 1.35f + (i % 3) * 0.50f;
                var y = -0.30f + (i % 3) * 0.20f;
                Box(skyline, "Tower", M("Violet"), new Vector3(x, y, z), new Vector3(0.29f, h, 0.25f));
                Box(skyline, "Crown", i % 2 == 0 ? M("WaterBright") : M("AccentLight"),
                    new Vector3(x, y + h * 0.5f + 0.015f, z), new Vector3(0.31f, 0.035f, 0.27f));
                Box(skyline, "Spire", M("StoneLight"), new Vector3(x, y + h * 0.5f + 0.14f, z),
                    new Vector3(0.025f, 0.25f, 0.025f));
            }

            // Luminous twin sweeping searchlight beams
            var searchlightL = Hidden(Mover(t, "Searchlight_L"));
            Box(searchlightL, "BeamL", M("WaterBright"),
                new Vector3(-0.70f, 1.75f, 1.45f), new Vector3(0.045f, 1.60f, 0.045f), new Vector3(0f, 0f, 30f));

            var searchlightR = Hidden(Mover(t, "Searchlight_R"));
            Box(searchlightR, "BeamR", M("AccentLight"),
                new Vector3(0.60f, 1.75f, 1.45f), new Vector3(0.045f, 1.60f, 0.045f), new Vector3(0f, 0f, -30f));

            // MONUMENTAL GRAND FINALE FIREWORKS - Central Mega-Burst
            var fireworksPrimary = Hidden(Mover(t, "FireworksPrimary"));
            for (var i = 0; i < 20; i++)
            {
                var a = i * Mathf.PI / 10f;
                var r = 0.55f + (i % 2) * 0.40f;
                var col = (i % 4) switch
                {
                    0 => M("AccentLight"),
                    1 => M("WaterBright"),
                    2 => M("Candle"),
                    _ => M("AccentDeep")
                };
                Ball(fireworksPrimary, "BurstCore", col,
                    new Vector3(-0.06f + Mathf.Cos(a) * r, 1.72f + Mathf.Sin(a) * (r * 0.72f), 1.05f),
                    Vector3.one * 0.08f);
                Box(fireworksPrimary, "Ray", col,
                    new Vector3(-0.06f + Mathf.Cos(a) * (r * 0.70f), 1.72f + Mathf.Sin(a) * (r * 0.50f), 1.05f),
                    new Vector3(0.025f, 0.18f, 0.025f),
                    new Vector3(0f, 0f, a * Mathf.Rad2Deg + 90f));
            }

            // Flanking Secondary Fireworks Explosions
            var fireworksSecondary = Hidden(Mover(t, "FireworksSecondary"));
            for (var i = 0; i < 12; i++)
            {
                var a = i * Mathf.PI / 6f;
                Ball(fireworksSecondary, "BurstLeft", i % 2 == 0 ? M("WaterBright") : M("Candle"),
                    new Vector3(-0.75f + Mathf.Cos(a) * 0.40f, 1.85f + Mathf.Sin(a) * 0.32f, 1.30f),
                    Vector3.one * 0.065f);
                Ball(fireworksSecondary, "BurstRight", i % 2 == 0 ? M("AccentLight") : M("AccentDeep"),
                    new Vector3(0.65f + Mathf.Cos(a) * 0.40f, 1.85f + Mathf.Sin(a) * 0.32f, 1.30f),
                    Vector3.one * 0.065f);
            }

            // Confetti and celebration shower falling across the skyline
            var celebrationSparks = Hidden(Mover(t, "CelebrationSparks"));
            for (var i = 0; i < 16; i++)
            {
                var px = -0.90f + (i % 8) * 0.26f;
                var py = 1.45f + (i / 8) * 0.40f;
                Ball(celebrationSparks, "Spark", (i % 3) switch
                {
                    0 => M("Candle"),
                    1 => M("WaterBright"),
                    _ => M("AccentLight")
                }, new Vector3(px, py, 1.25f), Vector3.one * 0.05f);
            }

            Worlds.Peps(t, new Vector3(0.28f, 0.665f, 0.28f), new Vector3(-0.36f, 1.34f, 1.34f),
                new Vector3(-0.28f, 1.34f, 1.22f));
            Worlds.Slots(t, new Vector3(-0.42f, 0.045f, -1.38f), new Vector3(0.42f, 0.045f, -1.46f),
                new Vector3(0f, 0.045f, -1.10f));
            Worlds.Finish(root, Worlds.Neon, _dir);
        }
    }
}
