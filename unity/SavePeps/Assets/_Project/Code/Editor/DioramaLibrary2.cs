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

        /// <summary>Movers: Darkness, LampHook, LitPool (hidden), Shallows (hidden).</summary>
        private static void CaveDark()
        {
            var root = Worlds.Begin(Worlds.Cave, "Dark");
            var t = root.transform;

            Rod(t, "HookPost", M("Stone"), new Vector3(-0.58f, 0.30f, 0.10f), new Vector3(0.06f, 0.26f, 0.06f));
            var hook = Mover(t, "LampHook");
            Box(hook, "Arm", M("StoneLight"), new Vector3(-0.48f, 0.56f, 0.10f), new Vector3(0.22f, 0.03f, 0.03f));
            Box(hook, "Barb", M("StoneLight"), new Vector3(-0.38f, 0.51f, 0.10f), new Vector3(0.025f, 0.09f, 0.025f));

            // The floor between the Peps looks like a void. It is a puddle,
            // and finding that out is the entire rescue — so the lie has to be
            // opaque and the truth has to arrive all at once.
            var dark = Mover(t, "Darkness");
            Box(dark, "Void", M("Abyss"), new Vector3(0.02f, 0.06f, 0.16f), new Vector3(1.30f, 0.04f, 1.60f));
            foreach (var (x, z, s) in new[] { (-0.40f, 0.72f, 0.5f), (0.42f, -0.20f, 0.6f), (0.10f, 1.20f, 0.7f) })
            {
                Ball(dark, "Murk", M("Abyss"), new Vector3(x, 0.24f, z), new Vector3(0.60f * s, 0.40f * s, 0.60f * s));
            }

            var pool = Hidden(Mover(t, "LitPool"));
            Ball(pool, "Warmth", M("Candle"), new Vector3(-0.10f, 0.05f, 0.24f), new Vector3(1.35f, 0.010f, 1.70f));

            var shallows = Hidden(Mover(t, "Shallows"));
            Box(shallows, "Water", M("WaterDeep"), new Vector3(0.02f, 0.055f, 0.30f),
                new Vector3(0.92f, 0.02f, 0.90f));
            foreach (var (x, z) in new[] { (-0.22f, 0.10f), (0.20f, 0.48f), (-0.02f, 0.72f) })
            {
                Ball(shallows, "Steppingstone", M("Violet"), new Vector3(x, 0.085f, z),
                    new Vector3(0.24f, 0.05f, 0.22f));
            }

            Worlds.Peps(t, new Vector3(-0.34f, 0.04f, -0.72f), new Vector3(0.32f, 0.04f, 0.92f),
                new Vector3(-0.02f, 0.04f, 0.06f));
            Worlds.Slots(t, new Vector3(-0.46f, 0.04f, -1.14f), new Vector3(0f, 0.04f, -1.40f),
                new Vector3(0.48f, 0.04f, -1.14f));
            Worlds.Finish(root, Worlds.Cave, _dir);
        }

        /// <summary>Movers: CrystalVein, VeinRing (hidden), RockCurtain, Dust (hidden).</summary>
        private static void CaveVein()
        {
            var root = Worlds.Begin(Worlds.Cave, "Vein");
            var t = root.transform;

            // A seam of tuned crystal running the length of the ceiling. It is
            // the only ceiling-mounted mechanism in the game, which is why the
            // round's answer is a pitch rather than a push.
            var vein = Mover(t, "CrystalVein");
            for (var i = 0; i < 6; i++)
            {
                var z = -0.70f + i * 0.44f;
                Box(vein, "Shard", M("WaterBright"), new Vector3(-0.18f + (i % 2) * 0.30f, 1.14f, z),
                    new Vector3(0.10f, 0.30f, 0.10f), new Vector3(0f, 45f, i % 2 == 0 ? 12f : -12f));
            }

            // The seam that carries the shards. It has to be a line, not a
            // plate: a single long box at this height reads from the fixed
            // camera as a table lid covering the back half of the floor.
            for (var i = 0; i < 5; i++)
            {
                Box(vein, "Seam", M("Stone"), new Vector3(-0.06f + (i % 2) * 0.18f, 1.26f, -0.62f + i * 0.50f),
                    new Vector3(0.13f, 0.05f, 0.54f), new Vector3(0f, i % 2 == 0 ? 14f : -14f, 0f));
            }

            var ring = Hidden(Mover(t, "VeinRing"));
            for (var i = 0; i < 5; i++)
            {
                Ball(ring, "Ripple", M("Candle"), new Vector3(-0.14f + (i % 2) * 0.28f, 1.10f, -0.60f + i * 0.50f),
                    new Vector3(0.26f, 0.26f, 0.26f));
            }

            // Low enough that Pep B still reads over the top of it. The curtain
            // is the obstacle, but a Pep you cannot see is not a Pep you are
            // trying to save.
            var curtain = Mover(t, "RockCurtain");
            for (var i = 0; i < 5; i++)
            {
                Box(curtain, "Slab", i % 2 == 0 ? M("Violet") : M("Ink"),
                    new Vector3(-0.44f + i * 0.22f, 0.28f, 0.72f), new Vector3(0.22f, 0.56f, 0.20f),
                    new Vector3(0f, 0f, i % 2 == 0 ? 3f : -3f));
            }

            var dust = Hidden(Mover(t, "Dust"));
            foreach (var (x, z, s) in new[] { (-0.30f, 0.72f, 0.30f), (0.16f, 0.66f, 0.36f), (0.42f, 0.78f, 0.26f) })
            {
                Ball(dust, "Cloud", M("Violet"), new Vector3(x, 0.22f, z), new Vector3(s, s * 0.6f, s * 0.7f));
            }

            // Pep B is stranded on a shelf behind the curtain. The shelf is
            // what keeps them in shot: on the floor they sit entirely behind
            // the rock, and the round loses the face it is asking you to save.
            Box(t, "Ledge", M("Violet"), new Vector3(0.26f, 0.13f, 1.16f), new Vector3(0.62f, 0.26f, 0.54f));
            Box(t, "LedgeLip", M("Stone"), new Vector3(0.26f, 0.255f, 0.91f), new Vector3(0.62f, 0.03f, 0.06f));

            Worlds.Peps(t, new Vector3(-0.30f, 0.04f, -0.62f), new Vector3(0.26f, 0.30f, 1.14f),
                new Vector3(-0.05f, 0.04f, 0.06f));
            Worlds.Slots(t, new Vector3(-0.48f, 0.04f, -1.18f), new Vector3(0.48f, 0.04f, -1.18f),
                new Vector3(0f, 0.04f, -1.44f));
            Worlds.Finish(root, Worlds.Cave, _dir);
        }

        /// <summary>Movers: MineCart, Chock, Rail.</summary>
        private static void CaveCart()
        {
            var root = Worlds.Begin(Worlds.Cave, "Cart");
            var t = root.transform;

            var rail = Mover(t, "Rail");
            foreach (var x in new[] { -0.14f, 0.22f })
            {
                Box(rail, "Rail", M("StoneLight"), new Vector3(x, 0.06f, 0.30f), new Vector3(0.045f, 0.03f, 2.70f));
            }

            for (var i = 0; i < 8; i++)
            {
                Box(rail, "Sleeper", M("WoodDark"), new Vector3(0.04f, 0.045f, -0.95f + i * 0.34f),
                    new Vector3(0.46f, 0.025f, 0.09f));
            }

            // Loaded, on a rail, on a slope, held by one wedge. Everything
            // about the silhouette says "this wants to go".
            var cart = Mover(t, "MineCart");
            cart.parent.localPosition = new Vector3(0.04f, 0f, 0.06f);
            Box(cart, "Tub", M("Stone"), new Vector3(0f, 0.24f, 0f), new Vector3(0.46f, 0.28f, 0.56f));
            Box(cart, "TubLip", M("StoneLight"), new Vector3(0f, 0.385f, 0f), new Vector3(0.50f, 0.03f, 0.60f));
            foreach (var (x, z) in new[] { (-0.18f, -0.20f), (0.26f, -0.20f), (-0.18f, 0.20f), (0.26f, 0.20f) })
            {
                Rod(cart, "Wheel", M("Ink"), new Vector3(x, 0.09f, z), new Vector3(0.14f, 0.02f, 0.14f),
                    new Vector3(0f, 0f, 90f));
            }

            foreach (var (x, s) in new[] { (-0.10f, 0.9f), (0.08f, 1.1f), (0.20f, 0.8f) })
            {
                Ball(cart, "Ore", M("WaterBright"), new Vector3(x, 0.42f, 0f), Vector3.one * (0.10f * s));
            }

            var chock = Mover(t, "Chock");
            Box(chock, "Wedge", M("Wood"), new Vector3(-0.10f, 0.075f, -0.20f),
                new Vector3(0.13f, 0.10f, 0.18f), new Vector3(24f, 0f, 0f));

            Worlds.Peps(t, new Vector3(-0.44f, 0.04f, -0.76f), new Vector3(0.44f, 0.04f, 1.14f),
                new Vector3(0.10f, 0.04f, 0.24f));
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

            // Loose snow lying proud of the packed slope, in soft lumps. The
            // surface has to look like something you would sink into.
            var powder = Mover(t, "Powder");
            for (var i = 0; i < 7; i++)
            {
                var z = -0.30f + (i % 4) * 0.42f;
                var step = Mathf.Floor((z + 1.68f) / 0.48f);
                Ball(powder, "Drift", M("Cream"),
                    new Vector3(-0.44f + (i % 3) * 0.42f, 0.10f + step * 0.155f + 0.06f, z),
                    new Vector3(0.46f, 0.13f, 0.42f));
            }

            var crust = Hidden(Mover(t, "Crust"));
            Box(crust, "Glaze", M("Ice"), new Vector3(0f, 0.58f, 0.24f), new Vector3(1.10f, 0.02f, 1.70f),
                new Vector3(18f, 0f, 0f));
            for (var i = 0; i < 4; i++)
            {
                Box(crust, "WindRipple", M("WaterLight"), new Vector3(-0.30f + i * 0.22f, 0.60f, -0.10f + i * 0.42f),
                    new Vector3(0.46f, 0.008f, 0.05f), new Vector3(18f, 12f, 0f));
            }

            var hole = Mover(t, "Sinkhole");
            Ball(hole, "Pit", M("WaterLight"), new Vector3(-0.05f, 0.50f, 0.10f), new Vector3(0.34f, 0.05f, 0.30f));

            Worlds.Peps(t, new Vector3(-0.34f, 0.41f, -0.48f), new Vector3(0.32f, 0.875f, 0.96f),
                new Vector3(0.02f, 0.72f, 0.48f));
            Worlds.Slots(t, new Vector3(-0.46f, 0.10f, -1.44f), new Vector3(0.46f, 0.10f, -1.44f),
                new Vector3(0f, 0.255f, -0.96f));
            Worlds.Finish(root, Worlds.Peak, _dir);
        }

        /// <summary>Movers: Chute, Drift, Poof (hidden).</summary>
        private static void PeakChute()
        {
            var root = Worlds.Begin(Worlds.Peak, "Chute");
            var t = root.transform;

            // A carved run from the cornice to the valley: the only continuous
            // top-to-bottom line in the game, and it points at Pep B.
            var chute = Mover(t, "Chute");
            for (var i = 0; i < 7; i++)
            {
                Box(chute, "Bed", M("Ice"), new Vector3(0.16f - i * 0.045f, 0.115f + i * 0.155f, -1.44f + i * 0.48f),
                    new Vector3(0.46f, 0.02f, 0.50f));
                foreach (var side in new[] { -1f, 1f })
                {
                    Box(chute, "Berm", M("Cream"),
                        new Vector3(0.16f - i * 0.045f + side * 0.27f, 0.15f + i * 0.155f, -1.44f + i * 0.48f),
                        new Vector3(0.10f, 0.10f, 0.50f));
                }
            }

            var drift = Mover(t, "Drift");
            foreach (var (x, z, s) in new[] { (-0.16f, -1.30f, 1.1f), (0.16f, -1.10f, 0.9f), (0f, -1.48f, 1.2f) })
            {
                Ball(drift, "Mound", M("Snow"), new Vector3(x, 0.16f, z), new Vector3(0.52f * s, 0.20f, 0.44f * s));
            }

            var poof = Hidden(Mover(t, "Poof"));
            foreach (var (x, y, s) in new[] { (-0.26f, 0.28f, 0.30f), (0.06f, 0.38f, 0.36f), (0.26f, 0.26f, 0.28f) })
            {
                Ball(poof, "Spray", M("Cream"), new Vector3(x, y, -1.22f), Vector3.one * s);
            }

            Worlds.Peps(t, new Vector3(0.20f, 1.03f, 1.40f), new Vector3(-0.30f, 0.255f, -0.90f),
                new Vector3(-0.14f, 0.255f, -0.80f));
            Worlds.Slots(t, new Vector3(-0.46f, 0.10f, -1.48f), new Vector3(0.46f, 0.10f, -1.36f),
                new Vector3(0.30f, 0.255f, -0.98f));
            Worlds.Finish(root, Worlds.Peak, _dir);
        }

        /// <summary>Movers: RopeLine, IceBand, Bollard.</summary>
        private static void PeakTraverse()
        {
            var root = Worlds.Begin(Worlds.Peak, "Traverse");
            var t = root.transform;

            var band = Mover(t, "IceBand");
            Box(band, "Ice", M("Ice"), new Vector3(0f, 0.575f, 0f), new Vector3(1.34f, 0.02f, 0.46f));
            foreach (var (x, z) in new[] { (-0.30f, 0.10f), (0.18f, -0.12f) })
            {
                Ball(band, "Glaze", M("WaterLight"), new Vector3(x, 0.585f, z), new Vector3(0.30f, 0.008f, 0.22f));
            }

            var bollard = Mover(t, "Bollard");
            foreach (var x in new[] { -0.56f, 0.58f })
            {
                Rod(bollard, "Rock", M("Stone"), new Vector3(x, 0.65f, 0f), new Vector3(0.16f, 0.16f, 0.16f));
                Rod(bollard, "Collar", M("StoneLight"), new Vector3(x, 0.76f, 0f), new Vector3(0.12f, 0.02f, 0.12f));
            }

            // Slack at rest, and the sag is the tell: a line lying in the snow
            // is not something you can hold on to.
            var line = Mover(t, "RopeLine");
            Box(line, "Left", M("Earth"), new Vector3(-0.28f, 0.66f, 0f), new Vector3(0.60f, 0.016f, 0.016f),
                new Vector3(0f, 0f, -14f));
            Box(line, "Middle", M("Earth"), new Vector3(0.01f, 0.585f, 0f), new Vector3(0.42f, 0.016f, 0.016f));
            Box(line, "Right", M("Earth"), new Vector3(0.30f, 0.66f, 0f), new Vector3(0.60f, 0.016f, 0.016f),
                new Vector3(0f, 0f, 14f));

            var taut = Hidden(Mover(t, "TautLine"));
            Box(taut, "Line", M("Earth"), new Vector3(0.01f, 0.74f, 0f), new Vector3(1.20f, 0.018f, 0.018f));
            foreach (var x in new[] { -0.56f, 0.58f })
            {
                Box(taut, "Hitch", M("Cream"), new Vector3(x, 0.74f, 0f), new Vector3(0.06f, 0.04f, 0.04f));
            }

            Worlds.Peps(t, new Vector3(-0.44f, 0.565f, 0.02f), new Vector3(0.46f, 0.565f, -0.02f),
                new Vector3(0.10f, 0.565f, 0f));
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

        /// <summary>Movers: Wreck, Angler, AnglerLure.</summary>
        private static void AbyssWreck()
        {
            var root = Worlds.Begin(Worlds.Abyss, "Wreck");
            var t = root.transform;

            var wreck = Mover(t, "Wreck");
            Box(wreck, "Hull", M("WoodDark"), new Vector3(0.02f, 0.34f, 0.86f), new Vector3(1.02f, 0.68f, 0.72f),
                new Vector3(0f, 0f, -9f));
            Box(wreck, "Keel", M("Earth"), new Vector3(0.02f, 0.06f, 0.86f), new Vector3(1.06f, 0.10f, 0.78f));
            Box(wreck, "Mouth", M("Abyss"), new Vector3(0.04f, 0.24f, 0.50f), new Vector3(0.40f, 0.42f, 0.06f));
            foreach (var (x, y) in new[] { (-0.34f, 0.52f), (0.36f, 0.44f) })
            {
                Ball(wreck, "Porthole", M("WaterBright"), new Vector3(x, y, 0.50f),
                    new Vector3(0.10f, 0.10f, 0.04f));
            }

            // A benthic guard with a lamp of its own — which is exactly why a
            // brighter, drifting light will move it and a noise will not.
            var angler = Mover(t, "Angler");
            angler.parent.localPosition = new Vector3(0.06f, 0f, 0.34f);
            var hover = Idle(angler, AmbientMode.Bob, 0.030f, 0.34f, Vector3.up);
            Ball(hover, "Body", M("Violet"), new Vector3(0f, 0.26f, 0f), new Vector3(0.42f, 0.32f, 0.36f));
            Box(hover, "Jaw", M("Ink"), new Vector3(0f, 0.15f, -0.14f), new Vector3(0.34f, 0.06f, 0.12f));
            for (var i = 0; i < 5; i++)
            {
                Box(hover, "Tooth", M("Cream"), new Vector3(-0.12f + i * 0.06f, 0.19f, -0.17f),
                    new Vector3(0.024f, 0.06f, 0.024f));
            }

            Ball(hover, "Eye", M("Candle"), new Vector3(-0.12f, 0.33f, -0.13f), Vector3.one * 0.06f);
            Box(hover, "Tail", M("Violet"), new Vector3(0f, 0.28f, 0.24f), new Vector3(0.16f, 0.18f, 0.14f));

            var lure = Mover(t, "AnglerLure");
            lure.parent.localPosition = new Vector3(0.06f, 0f, 0.34f);
            var wave = Idle(lure, AmbientMode.Sway, 11f, 0.42f, Vector3.forward);
            Box(wave, "Stalk", M("Violet"), new Vector3(0f, 0.48f, -0.10f), new Vector3(0.02f, 0.22f, 0.02f),
                new Vector3(-24f, 0f, 0f));
            Ball(wave, "Bulb", M("AccentLight"), new Vector3(0f, 0.58f, -0.19f), Vector3.one * 0.075f);

            Worlds.Peps(t, new Vector3(-0.34f, 0.05f, -0.58f), new Vector3(0.20f, 0.05f, 1.34f),
                new Vector3(-0.02f, 0.05f, 0.02f));
            Worlds.Slots(t, new Vector3(-0.42f, 0.05f, -1.12f), new Vector3(0.44f, 0.05f, -1.26f),
                new Vector3(0f, 0.05f, -1.46f));
            Worlds.Finish(root, Worlds.Abyss, _dir);
        }

        /// <summary>Movers: Race, LeftLedge, RightLedge, HaulLine (hidden).</summary>
        private static void AbyssCurrent()
        {
            var root = Worlds.Begin(Worlds.Abyss, "Current");
            var t = root.transform;

            var left = Mover(t, "LeftLedge");
            Box(left, "Shelf", M("Violet"), new Vector3(-0.52f, 0.22f, 0.10f), new Vector3(0.46f, 0.12f, 1.10f));
            var right = Mover(t, "RightLedge");
            Box(right, "Shelf", M("Violet"), new Vector3(0.54f, 0.22f, 0.34f), new Vector3(0.46f, 0.12f, 1.10f));

            // A race running along the trench, fast enough that everything
            // loose in the round is already on its way somewhere else.
            var race = Mover(t, "Race");
            var flow = Living(race, "Flow", AmbientMode.Drift, 1.60f, 0.62f, Vector3.right, stagger: true);
            for (var i = 0; i < 8; i++)
            {
                Box(flow, "Streak", M("WaterBright"),
                    new Vector3(-0.70f + (i % 4) * 0.18f, 0.30f + (i / 4) * 0.26f, -0.10f + (i % 3) * 0.34f),
                    new Vector3(0.22f, 0.014f, 0.014f));
            }

            var haul = Hidden(Mover(t, "HaulLine"));
            Box(haul, "Line", M("Cream"), new Vector3(0.02f, 0.30f, 0.22f), new Vector3(1.06f, 0.014f, 0.014f),
                new Vector3(0f, 12f, 0f));

            Worlds.Peps(t, new Vector3(-0.52f, 0.28f, 0.10f), new Vector3(0.54f, 0.28f, 0.34f),
                new Vector3(0.28f, 0.28f, 0.30f));
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

        /// <summary>Movers: Backpack, Handrail, PullArc (hidden).</summary>
        private static void OrbitTumble()
        {
            var root = Worlds.Begin(Worlds.Orbit, "Tumble");
            var t = root.transform;

            var rail = Mover(t, "Handrail");
            Box(rail, "Bar", M("Accent"), new Vector3(0.05f, 0.18f, 0.16f), new Vector3(0.66f, 0.03f, 0.03f));
            foreach (var x in new[] { -0.24f, 0.34f })
            {
                Box(rail, "Stanchion", M("Stone"), new Vector3(x, 0.11f, 0.16f), new Vector3(0.03f, 0.16f, 0.03f));
            }

            // Steel, and visibly so — the magnet has to be readable as the
            // answer rather than as magic.
            var pack = Mover(t, "Backpack");
            pack.parent.localPosition = new Vector3(0.55f, 0.86f, 1.35f);
            var tumble = Idle(pack, AmbientMode.Spin, 34f, 0.14f, new Vector3(0.3f, 1f, 0.2f));
            Box(tumble, "Shell", M("StoneLight"), Vector3.zero, new Vector3(0.24f, 0.26f, 0.14f));
            Box(tumble, "Plate", M("Stone"), new Vector3(0f, 0f, -0.075f), new Vector3(0.19f, 0.21f, 0.02f));
            foreach (var y in new[] { -0.07f, 0.07f })
            {
                Box(tumble, "Strap", M("Violet"), new Vector3(0f, y, 0.075f), new Vector3(0.20f, 0.03f, 0.02f));
            }

            var arc = Hidden(Mover(t, "PullArc"));
            for (var i = 0; i < 5; i++)
            {
                Ball(arc, "Trace", M("WaterBright"),
                    new Vector3(0.14f + i * 0.10f, 0.24f + i * 0.15f, 0.34f + i * 0.24f),
                    Vector3.one * (0.030f + i * 0.004f));
            }

            Worlds.Peps(t, new Vector3(0.05f, 0.04f, 0.16f), new Vector3(0.55f, 0.86f, 1.35f),
                new Vector3(0.05f, 0.04f, 0.30f));
            Worlds.Slots(t, new Vector3(-0.40f, 0.05f, -1.14f), new Vector3(0.40f, 0.05f, -1.14f),
                new Vector3(0f, 0.05f, -1.50f));
            Worlds.Finish(root, Worlds.Orbit, _dir);
        }

        /// <summary>Movers: Hatch, Vent, Blast.</summary>
        private static void OrbitAirlock()
        {
            var root = Worlds.Begin(Worlds.Orbit, "Airlock");
            var t = root.transform;

            Box(t, "Collar", M("Stone"), new Vector3(0f, 0.24f, 0.74f), new Vector3(0.74f, 0.60f, 0.16f));
            Box(t, "CollarTrim", M("Accent"), new Vector3(0f, 0.52f, 0.74f), new Vector3(0.78f, 0.05f, 0.18f));

            var hatch = Mover(t, "Hatch");
            Box(hatch, "Door", M("StoneLight"), new Vector3(-0.30f, 0.24f, 0.74f), new Vector3(0.30f, 0.48f, 0.06f));
            Rod(hatch, "Wheel", M("Accent"), new Vector3(-0.30f, 0.24f, 0.68f),
                new Vector3(0.16f, 0.02f, 0.16f), new Vector3(90f, 0f, 0f));

            // The vent is the obstacle and the reason: a jet of atmosphere
            // walking Pep A backwards down the corridor, permanently.
            var vent = Mover(t, "Vent");
            Box(vent, "Grille", M("Ink"), new Vector3(0.24f, 0.20f, 0.74f), new Vector3(0.20f, 0.20f, 0.05f));
            foreach (var y in new[] { 0.14f, 0.20f, 0.26f })
            {
                Box(vent, "Slat", M("Stone"), new Vector3(0.24f, y, 0.70f), new Vector3(0.19f, 0.018f, 0.02f));
            }

            var blast = Mover(t, "Blast");
            var stream = Living(blast, "Stream", AmbientMode.Drift, -1.10f, 0.85f, Vector3.forward, stagger: true);
            for (var i = 0; i < 6; i++)
            {
                Box(stream, "Jet", M("WaterLight"), new Vector3(0.24f + (i % 3 - 1) * 0.06f, 0.20f, 0.60f),
                    new Vector3(0.02f, 0.02f, 0.26f));
            }

            Worlds.Peps(t, new Vector3(0f, 0.05f, -1.02f), new Vector3(0f, 0.05f, 1.32f),
                new Vector3(0f, 0.05f, 1.08f));
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

        /// <summary>Movers: Piston, Linkage, Walkway, Sparks (hidden).</summary>
        private static void ForgePiston()
        {
            var root = Worlds.Begin(Worlds.Forge, "Piston");
            var t = root.transform;

            var walkway = Mover(t, "Walkway");
            Box(walkway, "Grate", M("Stone"), new Vector3(0.02f, 0.19f, 0.02f), new Vector3(0.52f, 0.04f, 1.10f));
            for (var i = 0; i < 5; i++)
            {
                Box(walkway, "Slat", M("StoneLight"), new Vector3(0.02f, 0.215f, -0.40f + i * 0.20f),
                    new Vector3(0.50f, 0.012f, 0.09f));
            }

            // Hammering on a beat before the player has touched anything: the
            // round's rule is that the machine does not wait for you.
            var piston = Mover(t, "Piston");
            piston.parent.localPosition = new Vector3(0.02f, 0f, 0.02f);
            Box(piston, "Cylinder", M("Stone"), new Vector3(0f, 1.06f, 0f), new Vector3(0.36f, 0.56f, 0.36f));
            Box(piston, "CylinderBand", M("Accent"), new Vector3(0f, 0.82f, 0f), new Vector3(0.40f, 0.05f, 0.40f));
            foreach (var x in new[] { -0.26f, 0.26f })
            {
                Box(piston, "TieRod", M("StoneLight"), new Vector3(x, 0.86f, 0f), new Vector3(0.04f, 0.96f, 0.04f));
            }
            var stroke = Idle(piston, AmbientMode.Beat, -0.44f, 0.62f, Vector3.up);
            Box(stroke, "Ram", M("StoneLight"), new Vector3(0f, 0.74f, 0f), new Vector3(0.13f, 0.34f, 0.13f));
            Box(stroke, "Head", M("Ink"), new Vector3(0f, 0.54f, 0f), new Vector3(0.40f, 0.12f, 0.40f));

            var linkage = Mover(t, "Linkage");
            Box(linkage, "Arm", M("Accent"), new Vector3(0.36f, 0.94f, 0.02f), new Vector3(0.38f, 0.06f, 0.06f),
                new Vector3(0f, 0f, -18f));
            Rod(linkage, "Pin", M("AccentLight"), new Vector3(0.20f, 0.99f, 0.02f),
                new Vector3(0.09f, 0.05f, 0.09f), new Vector3(90f, 0f, 0f));
            Box(linkage, "Yoke", M("Ink"), new Vector3(0.52f, 0.88f, 0.02f), new Vector3(0.10f, 0.20f, 0.10f));

            var sparks = Hidden(Mover(t, "Sparks"));
            for (var i = 0; i < 6; i++)
            {
                var a = i * Mathf.PI / 3f;
                Ball(sparks, "Spark", M("AccentLight"),
                    new Vector3(0.22f + Mathf.Cos(a) * 0.16f, 0.96f + Mathf.Sin(a) * 0.16f, 0.02f),
                    Vector3.one * 0.035f);
            }

            var held = Hidden(Mover(t, "PistonHeld"));
            Box(held, "Cylinder", M("Stone"), new Vector3(0.02f, 1.06f, 0.02f), new Vector3(0.36f, 0.56f, 0.36f));
            Box(held, "CylinderBand", M("Accent"), new Vector3(0.02f, 0.82f, 0.02f), new Vector3(0.40f, 0.05f, 0.40f));
            Box(held, "Ram", M("StoneLight"), new Vector3(0.02f, 0.76f, 0.02f), new Vector3(0.13f, 0.34f, 0.13f));
            Box(held, "Head", M("Ink"), new Vector3(0.02f, 0.56f, 0.02f), new Vector3(0.40f, 0.12f, 0.40f));

            Worlds.Peps(t, new Vector3(-0.34f, 0.195f, -0.92f), new Vector3(0.32f, 0.195f, 1.20f),
                new Vector3(0f, 0.195f, 0.94f));
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

        /// <summary>Movers: TramCar, RailSpark (hidden), Ledge.</summary>
        private static void NeonTransit()
        {
            var root = Worlds.Begin(Worlds.Neon, "Transit");
            var t = root.transform;

            var ledge = Mover(t, "Ledge");
            Box(ledge, "Lip", M("Stone"), new Vector3(-0.22f, 0.10f, -0.66f), new Vector3(0.66f, 0.06f, 0.10f));

            // A car that never stops. The idle runs it past the roof edge on a
            // loop, so the player watches the timing problem before the choice
            // matters — and its speed is the reason a balloon cannot help.
            var tram = Mover(t, "TramCar");
            tram.parent.localPosition = new Vector3(0f, 0.68f, 0.62f);
            var run = Idle(tram, AmbientMode.Drift, -2.40f, 0.33f, Vector3.right);
            run.localPosition = new Vector3(1.22f, 0f, 0f);
            Box(run, "Car", M("StoneLight"), Vector3.zero, new Vector3(0.62f, 0.22f, 0.22f), new Vector3(0f, 12f, 0f));
            Box(run, "Skirt", M("Stone"), new Vector3(0f, -0.12f, 0f), new Vector3(0.56f, 0.06f, 0.18f),
                new Vector3(0f, 12f, 0f));
            Box(run, "Windows", M("WaterBright"), new Vector3(0f, 0.03f, -0.115f), new Vector3(0.50f, 0.10f, 0.02f),
                new Vector3(0f, 12f, 0f));
            Box(run, "Grab", M("AccentDeep"), new Vector3(0f, 0.14f, 0f), new Vector3(0.52f, 0.03f, 0.03f),
                new Vector3(0f, 12f, 0f));

            var spark = Hidden(Mover(t, "RailSpark"));
            for (var i = 0; i < 5; i++)
            {
                Ball(spark, "Flash", M("AccentLight"), new Vector3(-0.40f + i * 0.24f, 0.72f, 0.58f),
                    Vector3.one * 0.045f);
            }

            Worlds.Peps(t, new Vector3(-0.28f, 0.045f, -0.98f), new Vector3(0.34f, 0.665f, 0.36f),
                new Vector3(0.18f, 0.665f, 0.20f));
            Worlds.Slots(t, new Vector3(-0.42f, 0.045f, -1.40f), new Vector3(0.42f, 0.045f, -1.34f),
                new Vector3(0f, 0.045f, -1.12f));
            Worlds.Finish(root, Worlds.Neon, _dir);
        }

        /// <summary>Movers: Antenna, Skyline, Fireworks (hidden).</summary>
        private static void NeonSkyline()
        {
            var root = Worlds.Begin(Worlds.Neon, "Skyline");
            var t = root.transform;

            var antenna = Mover(t, "Antenna");
            Box(antenna, "Mast", M("Stone"), new Vector3(-0.36f, 1.70f, 1.34f), new Vector3(0.05f, 0.72f, 0.05f));
            foreach (var y in new[] { 1.60f, 1.86f })
            {
                Box(antenna, "Ring", M("AccentDeep"), new Vector3(-0.36f, y, 1.34f),
                    new Vector3(0.22f, 0.02f, 0.22f));
            }

            Ball(antenna, "Beacon", M("AccentLight"), new Vector3(-0.36f, 2.08f, 1.34f), Vector3.one * 0.07f);

            // The whole city, laid out to be crossed. This is the last thing
            // the player sees, so the far tower gets a landing pad and a lit
            // approach rather than being another rooftop.
            var skyline = Mover(t, "Skyline");
            for (var i = 0; i < 5; i++)
            {
                Box(skyline, "Tower", M("Violet"), new Vector3(-0.90f + i * 0.46f, -0.30f + (i % 2) * 0.30f, 1.90f),
                    new Vector3(0.26f, 1.40f + (i % 2) * 0.50f, 0.22f));
                Box(skyline, "Crown", i % 2 == 0 ? M("WaterBright") : M("AccentLight"),
                    new Vector3(-0.90f + i * 0.46f, 0.42f + (i % 2) * 0.55f, 1.90f),
                    new Vector3(0.28f, 0.03f, 0.24f));
            }

            Box(t, "LandingPad", M("Stone"), new Vector3(-0.36f, 1.30f, 1.34f), new Vector3(0.62f, 0.03f, 0.62f));
            foreach (var i in new[] { 0, 1, 2, 3 })
            {
                Ball(t, "PadLight", M("AccentLight"),
                    new Vector3(-0.36f + (i % 2 == 0 ? -0.26f : 0.26f), 1.32f, 1.34f + (i < 2 ? -0.26f : 0.26f)),
                    Vector3.one * 0.035f);
            }

            var fireworks = Hidden(Mover(t, "Fireworks"));
            for (var i = 0; i < 8; i++)
            {
                var a = i * Mathf.PI / 4f;
                Ball(fireworks, "Burst", i % 2 == 0 ? M("AccentLight") : M("WaterBright"),
                    new Vector3(-0.10f + Mathf.Cos(a) * 0.70f, 1.60f + Mathf.Sin(a) * 0.50f, 1.10f),
                    Vector3.one * 0.06f);
            }

            Worlds.Peps(t, new Vector3(0.28f, 0.665f, 0.28f), new Vector3(-0.36f, 1.32f, 1.34f),
                new Vector3(-0.30f, 1.32f, 1.24f));
            Worlds.Slots(t, new Vector3(-0.42f, 0.045f, -1.38f), new Vector3(0.42f, 0.045f, -1.46f),
                new Vector3(0f, 0.045f, -1.10f));
            Worlds.Finish(root, Worlds.Neon, _dir);
        }
    }
}
