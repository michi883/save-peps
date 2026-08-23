using UnityEngine;
using SavePeps.Rescue;

using static SavePeps.EditorTools.Toy;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Thirty-six stages: one per rescue, three per world.
    ///
    /// The old catalogue reused fourteen environments across thirty-six
    /// rescues, and reuse at that ratio is what made rounds interchangeable —
    /// the same slab, the same slots, the same camera, a different prop on it.
    /// A stage is now cheap because <see cref="Worlds"/> supplies the base,
    /// the dressing, the light and the sound: what is written here is only the
    /// problem, which is the part that has to be different every time.
    ///
    /// Mover names are the contract with the choreography in
    /// <c>Round*Rescues</c>. A step aimed at a name that is not a mover here
    /// fails content validation rather than silently doing nothing on stage,
    /// which is why they are worth reading as a list.
    /// </summary>
    internal static partial class DioramaLibrary
    {
        private static string _dir;

        internal static int BuildAll(string environmentDir)
        {
            _dir = environmentDir;

            GardenBrook();
            GardenGate();
            GardenTrellis();

            ClockPulley();
            ClockGearwall();
            ClockOptics();

            WeatherFrost();
            WeatherBloom();
            WeatherDownpour();

            CanyonUpdraft();
            CanyonCablecar();
            CanyonSpire();

            TidePunt();
            TideChannel();
            TideCurrent();

            StormTarp();
            StormMast();
            StormGutter();

            CaveDark();
            CaveVein();
            CaveCart();

            PeakPowder();
            PeakChute();
            PeakTraverse();

            AbyssFloor();
            AbyssWreck();
            AbyssCurrent();

            OrbitDrift();
            OrbitTumble();
            OrbitAirlock();

            ForgeConveyor();
            ForgeSpill();
            ForgePiston();

            NeonSign();
            NeonTransit();
            NeonSkyline();

            return 36;
        }

        /// <summary>
        /// Regenerates only the six environments in the escalation pilot.
        /// Keeping this path separate prevents an R3/R4 iteration from
        /// serialising unrelated prefab churn across the other ten worlds.
        /// </summary>
        internal static int BuildRoundsThreeAndFour(string environmentDir)
        {
            var stages = BuildRoundThree(environmentDir);
            CanyonUpdraft();
            CanyonCablecar();
            CanyonSpire();
            return stages + 3;
        }

        /// <summary>Regenerates only the three stages owned by Round 3.</summary>
        internal static int BuildRoundThree(string environmentDir)
        {
            _dir = environmentDir;
            WeatherFrost();
            WeatherBloom();
            WeatherDownpour();
            return 3;
        }

        /// <summary>Regenerates only the three stages owned by Round 6.</summary>
        internal static int BuildRoundSix(string environmentDir)
        {
            _dir = environmentDir;
            StormTarp();
            StormMast();
            StormGutter();
            return 3;
        }

        /// <summary>Regenerates only the three stages owned by Round 7.</summary>
        internal static int BuildRoundSeven(string environmentDir)
        {
            _dir = environmentDir;
            CaveDark();
            CaveVein();
            CaveCart();
            return 3;
        }

        /// <summary>Regenerates only the three stages owned by Round 8.</summary>
        internal static int BuildRoundEight(string environmentDir)
        {
            _dir = environmentDir;
            PeakPowder();
            PeakChute();
            PeakTraverse();
            return 3;
        }

        /// <summary>Regenerates only the three stages owned by Round 9.</summary>
        internal static int BuildRoundNine(string environmentDir)
        {
            _dir = environmentDir;
            AbyssFloor();
            AbyssWreck();
            AbyssCurrent();
            return 3;
        }

        /// <summary>
        /// Regenerates only the six stages in the second escalation pilot.
        /// R3 remains the visual benchmark; R6-R12 are deliberately outside
        /// this write boundary.
        /// </summary>
        internal static int BuildRoundsFourAndFive(string environmentDir)
        {
            _dir = environmentDir;
            CanyonUpdraft();
            CanyonCablecar();
            CanyonSpire();
            TidePunt();
            TideChannel();
            TideCurrent();
            return 6;
        }

        private static Material M(string key) => Worlds.M[key];

        /// <summary>Hides a mover until choreography reveals it — beams, glows, melt puddles.</summary>
        private static Transform Hidden(Transform choreo) => Reveal(choreo);

        // ===================================================================
        // World 1 — Garden. Simple things do simple jobs.
        // ===================================================================

        /// <summary>Movers: Water.</summary>
        private static void GardenBrook()
        {
            var root = Worlds.Begin(Worlds.Garden, "Brook");
            var t = root.transform;

            Box(t, "Bank_Near", M("FoliageLight"), new Vector3(0f, 0.075f, -1.00f),
                new Vector3(1.35f, 0.15f, 1.40f));
            Box(t, "Bank_Far", M("Foliage"), new Vector3(0f, 0.075f, 1.00f),
                new Vector3(1.35f, 0.15f, 1.40f));
            Box(t, "StreamBed", M("Earth"), new Vector3(0f, 0.02f, 0f), new Vector3(1.35f, 0.04f, 0.66f));

            var water = Idle(Mover(t, "Water"), AmbientMode.Bob, 0.006f, 0.42f, Vector3.up);
            Box(water, "Surface", M("Water"), new Vector3(0f, 0.045f, 0f), new Vector3(1.35f, 0.07f, 0.62f));
            foreach (var (x, z, w) in new[] { (-0.38f, -0.10f, 0.30f), (0.28f, 0.08f, 0.36f), (-0.05f, 0.22f, 0.22f) })
            {
                Box(water, "Ripple", M("WaterLight"), new Vector3(x, 0.088f, z),
                    new Vector3(w, 0.012f, 0.026f), new Vector3(0f, x * 22f, 0f));
            }

            foreach (var (x, z, s) in new[] { (-0.52f, -0.42f, 0.10f), (0.50f, 0.45f, 0.08f) })
            {
                Ball(t, "Rock", M("Stone"), new Vector3(x, 0.15f, z), new Vector3(s, s * 0.7f, s));
            }

            Worlds.Peps(t, new Vector3(0f, 0.15f, -0.62f), new Vector3(0f, 0.15f, 0.62f),
                new Vector3(0f, 0.15f, 0.50f));
            Worlds.Slots(t, new Vector3(-0.42f, 0.15f, -1.25f), new Vector3(0.45f, 0.15f, -1.38f),
                new Vector3(-0.45f, 0.15f, 1.30f));
            Worlds.Finish(root, Worlds.Garden, _dir);
        }

        /// <summary>Movers: Gate, Helper, SleepMask, Zzz, Lever.</summary>
        private static void GardenGate()
        {
            var root = Worlds.Begin(Worlds.Garden, "Gate");
            var t = root.transform;

            Box(t, "Lawn", M("FoliageLight"), new Vector3(0f, 0.075f, 0f), new Vector3(1.35f, 0.15f, 3.15f));
            Box(t, "Path", M("Sand"), new Vector3(-0.04f, 0.16f, 0.10f), new Vector3(0.44f, 0.025f, 2.15f),
                new Vector3(0f, -18f, 0f));

            foreach (var x in new[] { -0.62f, -0.43f, 0.36f, 0.60f })
            {
                Box(t, "FencePost", M("Wood"), new Vector3(x, 0.34f, 0.18f), new Vector3(0.055f, 0.45f, 0.055f));
            }

            foreach (var x in new[] { -0.52f, 0.48f })
            {
                Box(t, "FenceRail", M("Wood"), new Vector3(x, 0.34f, 0.18f), new Vector3(0.30f, 0.055f, 0.055f));
            }

            var gate = Mover(t, "Gate");
            foreach (var x in new[] { -0.22f, -0.06f, 0.10f, 0.26f })
            {
                Box(gate, "GateBar", M("Clay"), new Vector3(x, 0.36f, 0.17f), new Vector3(0.045f, 0.44f, 0.05f));
            }

            foreach (var y in new[] { 0.23f, 0.45f })
            {
                Box(gate, "GateCrossbar", M("Clay"), new Vector3(0.02f, y, 0.17f),
                    new Vector3(0.52f, 0.045f, 0.055f));
            }

            // A toy robot sleeps beside the lever. Open eyes sit underneath a
            // movable mask, so Hide reveals wakefulness with no bespoke code.
            var helper = Mover(t, "Helper");
            Box(helper, "Body", M("Stone"), new Vector3(0.47f, 0.28f, 0.30f), new Vector3(0.29f, 0.28f, 0.22f));
            Box(helper, "Head", M("Cream"), new Vector3(0.47f, 0.48f, 0.28f), new Vector3(0.27f, 0.19f, 0.22f));
            foreach (var x in new[] { 0.41f, 0.53f })
            {
                Ball(helper, "OpenEye", M("Ink"), new Vector3(x, 0.49f, 0.165f), Vector3.one * 0.045f);
            }

            Ball(helper, "Antenna", M("Accent"), new Vector3(0.47f, 0.63f, 0.28f), Vector3.one * 0.07f);

            var mask = Mover(t, "SleepMask");
            foreach (var x in new[] { 0.41f, 0.53f })
            {
                Box(mask, "EyePatch", M("Cream"), new Vector3(x, 0.49f, 0.145f), new Vector3(0.09f, 0.075f, 0.022f));
                Box(mask, "ClosedEye", M("Ink"), new Vector3(x, 0.49f, 0.128f),
                    new Vector3(0.07f, 0.018f, 0.018f), new Vector3(0f, 0f, x < 0.47f ? -8f : 8f));
            }

            Rod(t, "LeverBase", M("Stone"), new Vector3(0.67f, 0.19f, 0.20f), new Vector3(0.11f, 0.04f, 0.11f));
            var lever = Mover(t, "Lever");
            Box(lever, "LeverHandle", M("Accent"), new Vector3(0.62f, 0.32f, 0.20f), new Vector3(0.04f, 0.28f, 0.04f),
                new Vector3(0f, 0f, 24f));
            Ball(lever, "LeverKnob", M("AccentLight"), new Vector3(0.56f, 0.44f, 0.20f), Vector3.one * 0.08f);

            var zzz = Mover(t, "Zzz");
            AddZ(zzz, M("Ink"), new Vector3(0.29f, 0.69f, 0.24f), 0.09f);
            AddZ(zzz, M("Ink"), new Vector3(0.43f, 0.81f, 0.24f), 0.12f);
            AddZ(zzz, M("Ink"), new Vector3(0.60f, 0.96f, 0.24f), 0.15f);

            Worlds.Peps(t, new Vector3(0.35f, 0.15f, -0.48f), new Vector3(-0.30f, 0.15f, 0.72f),
                new Vector3(0.10f, 0.15f, -0.22f));
            Worlds.Slots(t, new Vector3(-0.40f, 0.15f, -1.18f), new Vector3(0.40f, 0.15f, -1.18f),
                new Vector3(0f, 0.15f, -0.92f));
            Worlds.Finish(root, Worlds.Garden, _dir);
        }

        /// <summary>Movers: Vines, TrellisLeft, TrellisRight, Blooms, FoliageSide.</summary>
        private static void GardenTrellis()
        {
            var root = Worlds.Begin(Worlds.Garden, "Trellis");
            var t = root.transform;

            // Grand garden ground with paved terrace
            Box(t, "Garden", M("Foliage"), new Vector3(0f, 0.075f, 0f), new Vector3(1.35f, 0.15f, 3.15f));
            Box(t, "FrontPath", M("Sand"), new Vector3(-0.15f, 0.155f, -0.35f), new Vector3(0.65f, 0.02f, 1.10f));
            Box(t, "BackTerrace", M("StoneLight"), new Vector3(0.10f, 0.155f, 0.75f), new Vector3(0.95f, 0.02f, 0.95f));
            Box(t, "Bed_L", M("Earth"), new Vector3(-0.52f, 0.16f, 0.20f), new Vector3(0.28f, 0.03f, 1.60f));
            Box(t, "Bed_R", M("Earth"), new Vector3(0.52f, 0.16f, 0.20f), new Vector3(0.28f, 0.03f, 1.60f));

            // Grand Pergola / Trellis Arch Structure
            Box(t, "PergolaTop", M("Wood"), new Vector3(0.04f, 0.88f, 0.22f), new Vector3(1.15f, 0.08f, 0.16f));
            foreach (var z in new[] { 0.14f, 0.22f, 0.30f })
            {
                Box(t, "Rafter", M("Wood"), new Vector3(0.04f, 0.94f, z), new Vector3(1.05f, 0.04f, 0.04f));
            }
            Box(t, "Pillar_Left", M("Stone"), new Vector3(-0.48f, 0.48f, 0.22f), new Vector3(0.14f, 0.72f, 0.14f));
            Box(t, "Pillar_Right", M("Stone"), new Vector3(0.56f, 0.48f, 0.22f), new Vector3(0.14f, 0.72f, 0.14f));

            // Trellis Gates (Wing Left and Wing Right with outer pillar hinges) - swing open cleanly
            var trellisL = Mover(t, "TrellisLeft");
            trellisL.parent.localPosition = new Vector3(-0.42f, 0.50f, 0.22f);
            Box(trellisL, "WingL_Frame", M("Wood"), new Vector3(0.18f, 0f, 0f), new Vector3(0.36f, 0.62f, 0.04f));
            for (var y = -0.24f; y <= 0.26f; y += 0.13f)
            {
                Box(trellisL, "WingL_Slat", M("Wood"), new Vector3(0.18f, y, 0f), new Vector3(0.34f, 0.025f, 0.05f));
            }

            var trellisR = Mover(t, "TrellisRight");
            trellisR.parent.localPosition = new Vector3(0.50f, 0.50f, 0.22f);
            Box(trellisR, "WingR_Frame", M("Wood"), new Vector3(-0.18f, 0f, 0f), new Vector3(0.36f, 0.62f, 0.04f));
            for (var y = -0.24f; y <= 0.26f; y += 0.13f)
            {
                Box(trellisR, "WingR_Slat", M("Wood"), new Vector3(-0.18f, y, 0f), new Vector3(0.34f, 0.025f, 0.05f));
            }

            // The Massive Overgrowth (Vines Mover): dense interlocking vine barrier blocking everything
            var vines = Mover(t, "Vines");
            // Main vertical vine trunks
            foreach (var (x, y, h, thick) in new[] {
                (-0.35f, 0.50f, 0.68f, 0.065f),
                (-0.18f, 0.52f, 0.72f, 0.075f),
                (0.04f, 0.54f, 0.78f, 0.085f),
                (0.24f, 0.51f, 0.70f, 0.070f),
                (0.42f, 0.48f, 0.65f, 0.060f)
            })
            {
                Rod(vines, "VineTrunk", M("FoliageDark"), new Vector3(x, y, 0.20f), new Vector3(thick, h, thick));
            }

            // Diagonal cross-vines and interlocking brambles
            foreach (var (x, y, angle, len) in new[] {
                (0.04f, 0.55f, 38f, 0.95f),
                (0.04f, 0.55f, -38f, 0.95f),
                (-0.15f, 0.40f, 52f, 0.65f),
                (0.22f, 0.40f, -52f, 0.65f),
                (0.04f, 0.70f, 15f, 0.80f),
                (0.04f, 0.70f, -15f, 0.80f)
            })
            {
                Box(vines, "CrossVine", M("Foliage"), new Vector3(x, y, 0.18f),
                    new Vector3(0.055f, len, 0.055f), new Vector3(0f, 0f, angle));
            }

            // Central Keystone Knot (the focal cut point)
            Ball(vines, "KeystoneKnot", M("FoliageLight"), new Vector3(0.04f, 0.54f, 0.12f), Vector3.one * 0.16f);
            Rod(vines, "KeystoneBand", M("AccentDeep"), new Vector3(0.04f, 0.54f, 0.11f),
                new Vector3(0.18f, 0.04f, 0.18f), new Vector3(90f, 0f, 0f));

            // Dense foliage clusters and leaf canopies along the vines
            foreach (var (x, y, z, sx, sy, sz) in new[] {
                (-0.30f, 0.38f, 0.16f, 0.22f, 0.16f, 0.14f),
                (-0.08f, 0.68f, 0.15f, 0.26f, 0.18f, 0.15f),
                (0.18f, 0.72f, 0.15f, 0.24f, 0.18f, 0.14f),
                (0.38f, 0.42f, 0.16f, 0.25f, 0.17f, 0.15f),
                (0.04f, 0.30f, 0.17f, 0.28f, 0.18f, 0.16f),
                (-0.22f, 0.78f, 0.16f, 0.20f, 0.15f, 0.12f),
                (0.30f, 0.80f, 0.16f, 0.22f, 0.15f, 0.12f)
            })
            {
                Ball(vines, "LeafCluster", M("FoliageBright"), new Vector3(x, y, z), new Vector3(sx, sy, sz));
            }

            // Hidden Cascading Flower Blooms: burst open when the overgrowth collapses!
            var blooms = Hidden(Mover(t, "Blooms"));
            foreach (var (x, y, z, mat, scale) in new[] {
                (0.04f, 0.88f, 0.18f, "Accent", 0.15f),
                (-0.25f, 0.84f, 0.18f, "PepA", 0.14f),
                (0.33f, 0.84f, 0.18f, "AccentLight", 0.14f),
                (-0.46f, 0.72f, 0.18f, "Cream", 0.13f),
                (0.54f, 0.72f, 0.18f, "PepA", 0.13f),
                (-0.48f, 0.52f, 0.18f, "Accent", 0.12f),
                (0.56f, 0.52f, 0.18f, "Cream", 0.12f),
                (-0.12f, 0.90f, 0.20f, "PepALight", 0.11f),
                (0.20f, 0.90f, 0.20f, "AccentPale", 0.11f)
            })
            {
                Ball(blooms, "FlowerHead", M(mat), new Vector3(x, y, z), Vector3.one * scale);
                Ball(blooms, "FlowerPetal", M("FoliageLight"), new Vector3(x, y - scale * 0.35f, z + 0.01f),
                    new Vector3(scale * 1.3f, scale * 0.4f, scale * 0.8f));
            }

            // Side Foliage Bushes (reacts during collapse)
            var foliageSide = Mover(t, "FoliageSide");
            Ball(foliageSide, "Bush_L1", M("FoliageDark"), new Vector3(-0.54f, 0.24f, 0.18f), new Vector3(0.26f, 0.24f, 0.26f));
            Ball(foliageSide, "Bush_L2", M("Foliage"), new Vector3(-0.50f, 0.36f, 0.24f), new Vector3(0.22f, 0.20f, 0.22f));
            Ball(foliageSide, "Bush_R1", M("FoliageDark"), new Vector3(0.58f, 0.24f, 0.18f), new Vector3(0.26f, 0.24f, 0.26f));
            Ball(foliageSide, "Bush_R2", M("Foliage"), new Vector3(0.54f, 0.36f, 0.24f), new Vector3(0.22f, 0.20f, 0.22f));

            Worlds.Peps(t, new Vector3(-0.35f, 0.15f, -0.42f), new Vector3(0.22f, 0.15f, 0.78f),
                new Vector3(-0.06f, 0.15f, 0.18f));
            Worlds.Slots(t, new Vector3(-0.32f, 0.15f, -1.20f), new Vector3(0.40f, 0.15f, -1.20f),
                new Vector3(0f, 0.15f, -0.86f));
            Worlds.Finish(root, Worlds.Garden, _dir);
        }

        // ===================================================================
        // World 2 — Clockwork courtyard. Nothing moves until a linkage moves it.
        // ===================================================================

        /// <summary>Movers: LiftPlatform, Counterweight, Pulley.</summary>
        private static void ClockPulley()
        {
            var root = Worlds.Begin(Worlds.Clock, "Pulley");
            var t = root.transform;

            Box(t, "UpperDeck", M("Wood"), new Vector3(-0.35f, 0.27f, -0.02f), new Vector3(0.66f, 0.24f, 1.55f));
            Box(t, "DeckEdge", M("WoodDark"), new Vector3(-0.35f, 0.395f, -0.02f),
                new Vector3(0.68f, 0.02f, 1.57f));
            Box(t, "LiftPit", M("Ink"), new Vector3(0.30f, 0.155f, 0.43f), new Vector3(0.54f, 0.02f, 0.62f));

            var lift = Mover(t, "LiftPlatform");
            Box(lift, "LiftDeck", M("Accent"), new Vector3(0.30f, 0.20f, 0.43f), new Vector3(0.48f, 0.10f, 0.48f));
            foreach (var x in new[] { 0.10f, 0.50f })
            {
                Box(lift, "LiftRail", M("Cream"), new Vector3(x, 0.44f, 0.57f), new Vector3(0.035f, 0.46f, 0.035f));
            }

            var counterweight = Mover(t, "Counterweight");
            Box(counterweight, "WeightTray", M("Wood"), new Vector3(-0.52f, 0.69f, 0.48f),
                new Vector3(0.40f, 0.08f, 0.42f));
            foreach (var x in new[] { -0.70f, -0.34f })
            {
                Box(counterweight, "TrayLip", M("WoodDark"), new Vector3(x, 0.77f, 0.48f),
                    new Vector3(0.04f, 0.17f, 0.42f));
            }

            foreach (var x in new[] { -0.52f, 0.30f })
            {
                Box(t, "LiftRope", M("Cream"), new Vector3(x, 0.86f, 0.48f),
                    new Vector3(0.026f, x < 0f ? 0.38f : 0.95f, 0.026f));
            }

            Box(t, "Gantry", M("Wood"), new Vector3(-0.11f, 1.02f, 0.48f), new Vector3(1.05f, 0.07f, 0.08f));
            foreach (var x in new[] { -0.70f, 0.50f })
            {
                Box(t, "Tower", M("Wood"), new Vector3(x, 0.58f, 0.55f), new Vector3(0.07f, 0.95f, 0.08f));
            }

            var pulley = Mover(t, "Pulley");
            BlockRing(pulley, "PulleyWheel", M("Accent"), new Vector3(-0.11f, 1.02f, 0.42f),
                new Vector2(0.17f, 0.17f), 10, 0.035f, 0.05f);
            Rod(pulley, "Axle", M("Ink"), new Vector3(-0.11f, 1.02f, 0.39f),
                new Vector3(0.045f, 0.04f, 0.045f), new Vector3(90f, 0f, 0f));

            Worlds.Peps(t, new Vector3(-0.30f, 0.39f, -0.24f), new Vector3(0.30f, 0.25f, 0.43f),
                new Vector3(-0.08f, 0.39f, -0.05f));
            Worlds.Slots(t, new Vector3(0.44f, 0.15f, -1.08f), new Vector3(-0.44f, 0.15f, -1.08f),
                new Vector3(0f, 0.15f, -0.80f));
            Worlds.Finish(root, Worlds.Clock, _dir);
        }

        /// <summary>Movers: GearTrain, UpperGears, Governor, LinkageArm, Portcullis.</summary>
        private static void ClockGearwall()
        {
            var root = Worlds.Begin(Worlds.Clock, "Gearwall");
            var t = root.transform;

            // A wall of gearing across the back, with one bare shaft in the
            // middle of the train. The missing tooth is the whole puzzle, so
            // the socket has to be the brightest hole on the wall.
            Box(t, "GearHousing", M("Stone"), new Vector3(0.12f, 0.62f, 1.20f), new Vector3(1.34f, 1.00f, 0.18f));
            Box(t, "HousingTrim", M("StoneLight"), new Vector3(0.12f, 1.12f, 1.20f),
                new Vector3(1.38f, 0.05f, 0.20f));

            var train = Mover(t, "GearTrain");
            Cog(train, "DriveCog", M("Accent"), M("AccentDeep"), new Vector3(-0.30f, 0.66f, 1.09f), 0.21f, 11);
            Cog(train, "OutputCog", M("Accent"), M("AccentDeep"), new Vector3(0.54f, 0.66f, 1.09f), 0.21f, 11);

            // Counter-rotating upper pinions that mesh with the drive and output cogs
            var upperGears = Mover(t, "UpperGears");
            Cog(upperGears, "PinionLeft", M("AccentLight"), M("Accent"), new Vector3(-0.30f, 0.98f, 1.09f), 0.12f, 8);
            Cog(upperGears, "PinionRight", M("AccentLight"), M("Accent"), new Vector3(0.54f, 0.98f, 1.09f), 0.12f, 8);

            Rod(t, "EmptyShaft", M("StoneLight"), new Vector3(0.12f, 0.66f, 1.10f),
                new Vector3(0.06f, 0.035f, 0.06f), new Vector3(90f, 0f, 0f));
            Rod(t, "ShaftSocket", M("Ink"), new Vector3(0.12f, 0.66f, 1.06f),
                new Vector3(0.16f, 0.02f, 0.16f), new Vector3(90f, 0f, 0f));

            // A flyball governor idles while the train is dead and spins up
            // when it meshes: the readable "the machine is running now" tell.
            var governor = Mover(t, "Governor");
            Box(governor, "Spindle", M("AccentLight"), new Vector3(0.12f, 1.24f, 1.10f),
                new Vector3(0.03f, 0.22f, 0.03f));
            foreach (var x in new[] { -0.10f, 0.10f })
            {
                Ball(governor, "Ball", M("Accent"), new Vector3(0.12f + x, 1.20f, 1.10f), Vector3.one * 0.07f);
                Box(governor, "Arm", M("AccentLight"), new Vector3(0.12f + x * 0.5f, 1.27f, 1.10f),
                    new Vector3(0.11f, 0.016f, 0.016f), new Vector3(0f, 0f, x > 0f ? -26f : 26f));
            }

            // A mechanical linkage arm transmitting movement from the gearwall to the gate latch
            var linkage = Mover(t, "LinkageArm");
            Box(linkage, "RockerBar", M("Accent"), new Vector3(0.46f, 0.62f, 0.82f), new Vector3(0.04f, 0.32f, 0.04f),
                new Vector3(25f, 0f, 0f));
            Rod(linkage, "PivotPin", M("Ink"), new Vector3(0.46f, 0.62f, 0.82f), new Vector3(0.06f, 0.05f, 0.06f),
                new Vector3(0f, 0f, 90f));
            Box(linkage, "Pawl", M("AccentLight"), new Vector3(0.38f, 0.46f, 0.68f), new Vector3(0.12f, 0.03f, 0.04f));

            var portcullis = Mover(t, "Portcullis");
            foreach (var x in new[] { -0.26f, -0.06f, 0.14f, 0.34f })
            {
                Box(portcullis, "Bar", M("Stone"), new Vector3(x, 0.42f, 0.52f), new Vector3(0.05f, 0.84f, 0.05f));
                Box(portcullis, "Spike", M("StoneLight"), new Vector3(x, 0.02f, 0.52f),
                    new Vector3(0.05f, 0.10f, 0.05f), new Vector3(0f, 45f, 0f));
            }

            foreach (var y in new[] { 0.28f, 0.64f })
            {
                Box(portcullis, "BarBeam", M("StoneLight"), new Vector3(0.04f, y, 0.52f),
                    new Vector3(0.76f, 0.05f, 0.06f));
            }

            Worlds.Peps(t, new Vector3(-0.34f, 0.15f, -0.55f), new Vector3(0.30f, 0.15f, 0.90f),
                new Vector3(-0.06f, 0.15f, -0.12f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.15f, -1.14f), new Vector3(0.44f, 0.15f, -1.14f),
                new Vector3(0f, 0.15f, -0.86f));
            Worlds.Finish(root, Worlds.Clock, _dir);
        }

        /// <summary>Movers: BeamIn, BeamBounce (hidden), SensorGlow (hidden), CourtyardLights (hidden), GreatClockGears, OverheadPistons, IrisGate, ClockBridge.</summary>
        private static void ClockOptics()
        {
            var root = Worlds.Begin(Worlds.Clock, "Optics");
            var t = root.transform;

            // A sunken mechanism chasm between the front terrace and the rear observatory
            Box(t, "ChasmPit", M("Ink"), new Vector3(0f, -0.05f, 0.35f), new Vector3(1.40f, 0.20f, 0.70f));
            Box(t, "TerraceFront", M("Stone"), new Vector3(-0.35f, 0.15f, -0.40f), new Vector3(0.80f, 0.08f, 0.75f));
            Box(t, "TerraceRear", M("Stone"), new Vector3(0.30f, 0.15f, 1.05f), new Vector3(0.80f, 0.08f, 0.70f));

            // Lamp projector on the front left
            Rod(t, "LampBase", M("Stone"), new Vector3(-0.55f, 0.20f, 0.30f), new Vector3(0.16f, 0.05f, 0.16f));
            Box(t, "LampPost", M("Stone"), new Vector3(-0.55f, 0.36f, 0.30f), new Vector3(0.06f, 0.32f, 0.06f));
            Ball(t, "LampHousing", M("Accent"), new Vector3(-0.51f, 0.53f, 0.24f),
                new Vector3(0.19f, 0.16f, 0.16f));
            Ball(t, "Lamp", M("AccentLight"), new Vector3(-0.45f, 0.53f, 0.20f), new Vector3(0.11f, 0.10f, 0.10f));

            var beamIn = Mover(t, "BeamIn");
            Box(beamIn, "IncomingLight", M("AccentLight"), new Vector3(-0.25f, 0.36f, 0.15f),
                new Vector3(0.54f, 0.045f, 0.045f), new Vector3(0f, 0f, -13f));

            // Mirror pedestal in the courtyard focus
            Rod(t, "MirrorPedestal", M("Cream"), new Vector3(0.02f, 0.20f, 0.07f), new Vector3(0.15f, 0.06f, 0.15f));
            Rod(t, "PedestalCollar", M("Accent"), new Vector3(0.02f, 0.245f, 0.07f),
                new Vector3(0.10f, 0.015f, 0.10f));

            // Solar sensor on the right observatory pier
            Rod(t, "SensorHousing", M("Stone"), new Vector3(0.52f, 0.45f, 0.46f),
                new Vector3(0.19f, 0.05f, 0.19f), new Vector3(90f, 0f, 0f));
            Ball(t, "SensorDark", M("Ink"), new Vector3(0.52f, 0.45f, 0.355f), new Vector3(0.12f, 0.12f, 0.04f));

            var bounce = Hidden(Mover(t, "BeamBounce"));
            Box(bounce, "ReflectedLight", M("AccentLight"), new Vector3(0.27f, 0.39f, 0.25f),
                new Vector3(0.58f, 0.045f, 0.045f), new Vector3(0f, 0f, 28f));

            var glow = Hidden(Mover(t, "SensorGlow"));
            Ball(glow, "Glow", M("AccentLight"), new Vector3(0.52f, 0.45f, 0.33f), new Vector3(0.18f, 0.18f, 0.06f));

            // Courtyard power lanterns that illuminate when the solar sensor activates
            var courtyardLights = Hidden(Mover(t, "CourtyardLights"));
            foreach (var (x, y, z) in new[]
            {
                (-0.68f, 0.95f, 0.60f),
                (0.68f, 0.95f, 0.60f),
                (-0.65f, 0.42f, -0.20f),
                (0.65f, 0.42f, 0.90f)
            })
            {
                Ball(courtyardLights, "LanternBeacon", M("AccentLight"), new Vector3(x, y, z), Vector3.one * 0.14f);
                Rod(courtyardLights, "BeaconRing", M("Accent"), new Vector3(x, y, z), new Vector3(0.16f, 0.03f, 0.16f),
                    new Vector3(90f, 0f, 0f));
            }

            // Great Clockwork overhead machinery: massive interlocking gears across the upper courtyard
            var greatGears = Mover(t, "GreatClockGears");
            Cog(greatGears, "GreatGearCenter", M("Accent"), M("AccentDeep"), new Vector3(0.04f, 1.48f, 1.15f), 0.34f, 16);
            Cog(greatGears, "GreatGearLeft", M("AccentLight"), M("Accent"), new Vector3(-0.46f, 1.36f, 1.15f), 0.22f, 10);
            Cog(greatGears, "GreatGearRight", M("AccentLight"), M("Accent"), new Vector3(0.52f, 1.34f, 1.15f), 0.24f, 11);
            Rod(greatGears, "ClockCenterHub", M("StoneLight"), new Vector3(0.04f, 1.48f, 1.08f),
                new Vector3(0.12f, 0.04f, 0.12f), new Vector3(90f, 0f, 0f));
            Box(greatGears, "ClockHandHour", M("Ink"), new Vector3(0.04f, 1.56f, 1.06f), new Vector3(0.03f, 0.16f, 0.02f));
            Box(greatGears, "ClockHandMinute", M("Ink"), new Vector3(0.09f, 1.48f, 1.06f), new Vector3(0.18f, 0.025f, 0.02f));

            // Overhead rocker pistons & linkages
            var pistons = Mover(t, "OverheadPistons");
            Box(pistons, "PistonBarLeft", M("StoneLight"), new Vector3(-0.35f, 1.16f, 0.90f),
                new Vector3(0.04f, 0.26f, 0.04f), new Vector3(0f, 0f, 20f));
            Box(pistons, "PistonBarRight", M("StoneLight"), new Vector3(0.35f, 1.16f, 0.90f),
                new Vector3(0.04f, 0.26f, 0.04f), new Vector3(0f, 0f, -20f));
            Box(pistons, "CrossBeam", M("Accent"), new Vector3(0f, 1.08f, 0.90f), new Vector3(0.74f, 0.04f, 0.05f));

            // Central Iris gate
            var iris = Mover(t, "IrisGate");
            for (var i = 0; i < 6; i++)
            {
                var angle = i * 60f;
                Box(iris, "IrisLeaf", M("Accent"),
                    new Vector3(0.30f + Mathf.Cos(angle * Mathf.Deg2Rad) * 0.14f,
                        0.42f + Mathf.Sin(angle * Mathf.Deg2Rad) * 0.14f, 0.62f),
                    new Vector3(0.10f, 0.30f, 0.05f), new Vector3(0f, 0f, -angle));
            }

            // Grand folding clockwork drawbridge that unfolds across the chasm
            var bridge = Mover(t, "ClockBridge");
            bridge.parent.localPosition = new Vector3(-0.04f, 0.02f, 0.34f);
            Box(bridge, "BridgeDeck", M("WoodDark"), Vector3.zero, new Vector3(0.48f, 0.06f, 0.68f));
            for (var k = -2; k <= 2; k++)
            {
                Box(bridge, "BridgeTread", M("StoneLight"), new Vector3(0f, 0.038f, k * 0.13f),
                    new Vector3(0.44f, 0.015f, 0.06f));
                Box(bridge, "BrassInlay", M("Accent"), new Vector3(0f, 0.042f, k * 0.13f),
                    new Vector3(0.20f, 0.012f, 0.02f));
            }
            foreach (var side in new[] { -0.23f, 0.23f })
            {
                Box(bridge, "BridgeRail", M("Accent"), new Vector3(side, 0.10f, 0f),
                    new Vector3(0.03f, 0.14f, 0.68f));
            }

            Worlds.Peps(t, new Vector3(-0.38f, 0.15f, -0.40f), new Vector3(0.32f, 0.15f, 0.95f),
                new Vector3(-0.06f, 0.15f, 0.10f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.15f, -1.16f), new Vector3(0.44f, 0.15f, -1.16f),
                new Vector3(0f, 0.15f, -0.86f));
            Worlds.Finish(root, Worlds.Clock, _dir);
        }

        // ===================================================================
        // World 3 — Weather terrace. You never touch a Pep; you change the air.
        // Each stage carries its own sky, because that is the round's subject.
        // ===================================================================

        /// <summary>Movers: IceShell, MeltPuddle (hidden), Snowfall.</summary>
        private static void WeatherFrost()
        {
            Worlds.WeatherTint = "frost";
            var root = Worlds.Begin(Worlds.Weather, "Frost");
            var t = root.transform;

            var shell = Mover(t, "IceShell");
            // The frozen skin belongs to the shell so it shrinks away with
            // the crystals. The revealed puddle sits more than 0.01m above
            // the terrace; no coplanar water faces remain to flicker on phone.
            Ball(shell, "FrozenPatch", M("WaterLight"), new Vector3(0.30f, 0.708f, 1.22f),
                new Vector3(0.52f, 0.012f, 0.46f));
            foreach (var (x, y, angle, height) in new[]
                     {
                         (0.10f, 0.90f, -13f, 0.44f), (0.48f, 0.89f, 15f, 0.42f),
                         (0.20f, 1.05f, -38f, 0.35f), (0.41f, 1.06f, 40f, 0.34f),
                     })
            {
                Box(shell, "IceCrystal", M("Ice"), new Vector3(x, y, 1.20f),
                    new Vector3(0.105f, height, 0.11f), new Vector3(0f, 0f, angle));
            }

            Ball(shell, "IceCap", M("Ice"), new Vector3(0.30f, 1.04f, 1.24f), new Vector3(0.40f, 0.15f, 0.30f));

            var puddle = Hidden(Mover(t, "MeltPuddle"));
            Ball(puddle, "Puddle", M("Water"), new Vector3(0.30f, 0.717f, 1.22f),
                new Vector3(0.50f, 0.012f, 0.42f));

            var snow = Living(t, "Snowfall", AmbientMode.Drift, -0.95f, 0.30f, Vector3.up, stagger: true);
            for (var i = 0; i < 8; i++)
            {
                Ball(snow, "Flake", M("Cream"),
                    new Vector3(-0.55f + (i % 4) * 0.36f, 1.55f, 0.70f + (i / 4) * 0.55f),
                    Vector3.one * 0.022f);
            }

            Ball(t, "Snowball", M("Snow"), new Vector3(-0.50f, 0.76f, 1.36f), Vector3.one * 0.12f);

            Worlds.Peps(t, new Vector3(-0.36f, 0.70f, 0.92f), new Vector3(0.30f, 0.70f, 1.22f),
                new Vector3(-0.08f, 0.70f, 0.94f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.15f, -1.44f), new Vector3(0.46f, 0.15f, -1.44f),
                new Vector3(0f, 0.15f, -1.06f));
            Worlds.Finish(root, Worlds.Weather, _dir);
        }

        /// <summary>
        /// Movers: RootHeave, Plant, VineSpine, VineStep1/2/3 and VineCrown.
        /// The final six form a diagonal route across most of the hillside.
        /// </summary>
        private static void WeatherBloom()
        {
            Worlds.WeatherTint = "sun";
            var root = Worlds.Begin(Worlds.Weather, "Bloom");
            var t = root.transform;

            // R3.2 starts in a dry basin on the low near shelf, then grows a
            // living stair all the way to the snow ledge. The full diagonal
            // is deliberately unlike R3.1's close two-Pep composition.
            var rootHeave = Mover(t, "RootHeave");
            Ball(rootHeave, "DryBasin", M("EarthDark"), new Vector3(0.36f, 0.179f, -0.36f),
                new Vector3(0.58f, 0.035f, 0.48f));
            foreach (var (x, z, angle) in new[]
                     {
                         (0.18f, -0.48f, -24f), (0.38f, -0.26f, 18f), (0.52f, -0.45f, 38f),
                     })
            {
                Box(rootHeave, "DryCrack", M("WoodDark"), new Vector3(x, 0.205f, z),
                    new Vector3(0.16f, 0.012f, 0.025f), new Vector3(0f, angle, 0f));
            }

            Rod(t, "FlowerPot", M("Clay"), new Vector3(0.38f, 0.205f, -0.36f),
                new Vector3(0.22f, 0.09f, 0.22f));
            Rod(t, "PotRim", M("EarthLight"), new Vector3(0.38f, 0.255f, -0.36f),
                new Vector3(0.245f, 0.022f, 0.245f));

            // A tall ledge at the opposite corner makes the route, not just
            // the plant's scale, the problem to solve.
            Box(t, "HighLedgeFace", M("EarthLight"), new Vector3(-0.42f, 0.56f, 0.96f),
                new Vector3(0.48f, 0.28f, 0.56f));
            Box(t, "HighLedgeCap", M("FoliageBright"), new Vector3(-0.42f, 0.713f, 0.96f),
                new Vector3(0.50f, 0.016f, 0.58f));

            // The plant's container sits inside the pot rim, so a Resize step
            // begins the response before the landscape-scale vine takes over.
            var plant = Mover(t, "Plant");
            plant.parent.localPosition = new Vector3(0.38f, 0.17f, -0.36f);
            Rod(plant, "Stem", M("FoliageDark"), new Vector3(0f, 0.075f, 0f), new Vector3(0.028f, 0.075f, 0.028f));
            foreach (var side in new[] { -1f, 1f })
            {
                Ball(plant, "Leaf", M("Foliage"), new Vector3(side * 0.075f, 0.09f, 0f),
                    new Vector3(0.12f, 0.045f, 0.055f), new Vector3(0f, 0f, side * -25f));
            }

            Ball(plant, "FlowerDeck", M("Accent"), new Vector3(0f, 0.17f, 0f), new Vector3(0.26f, 0.055f, 0.24f));
            for (var i = 0; i < 6; i++)
            {
                var angle = i * Mathf.PI * 2f / 6f;
                Ball(plant, "Petal", M("AccentLight"),
                    new Vector3(Mathf.Cos(angle) * 0.13f, 0.18f, Mathf.Sin(angle) * 0.10f),
                    new Vector3(0.12f, 0.035f, 0.085f));
            }

            // The spine occupies most of the stage and physically connects
            // all three elevations. It begins buried and heaves into view.
            var spine = Hidden(Mover(t, "VineSpine"));
            foreach (var (x, y, z, angle, length) in new[]
                     {
                         (0.31f, 0.275f, -0.08f, -18f, 0.52f),
                         (0.10f, 0.405f, 0.34f, -24f, 0.52f),
                         (-0.13f, 0.545f, 0.73f, -29f, 0.48f),
                     })
            {
                Box(spine, "VineRun", M("FoliageDark"), new Vector3(x, y, z),
                    new Vector3(0.105f, 0.075f, length), new Vector3(0f, angle, 0f));
            }

            var step1 = Hidden(Mover(t, "VineStep1"));
            Ball(step1, "LeafPlatform", M("FoliageLight"), new Vector3(0.28f, 0.285f, -0.03f),
                new Vector3(0.38f, 0.055f, 0.23f), new Vector3(0f, -20f, -6f));
            var step2 = Hidden(Mover(t, "VineStep2"));
            Ball(step2, "LeafPlatform", M("Foliage"), new Vector3(0.07f, 0.415f, 0.35f),
                new Vector3(0.40f, 0.055f, 0.24f), new Vector3(0f, -25f, 5f));
            var step3 = Hidden(Mover(t, "VineStep3"));
            Ball(step3, "LeafPlatform", M("FoliageLight"), new Vector3(-0.16f, 0.555f, 0.72f),
                new Vector3(0.42f, 0.055f, 0.25f), new Vector3(0f, -30f, -5f));

            var crown = Hidden(Mover(t, "VineCrown"));
            Ball(crown, "Centre", M("Accent"), new Vector3(-0.34f, 0.585f, 1.02f),
                new Vector3(0.20f, 0.060f, 0.18f));
            for (var i = 0; i < 6; i++)
            {
                var angle = i * Mathf.PI * 2f / 6f;
                Ball(crown, "Petal", M("AccentLight"),
                    new Vector3(-0.34f + Mathf.Cos(angle) * 0.21f, 0.59f,
                        1.02f + Mathf.Sin(angle) * 0.16f),
                    new Vector3(0.17f, 0.038f, 0.11f));
            }

            // Sparse markers reinforce the empty before-state without
            // competing with the route when it erupts.
            Ball(t, "DecoFlowerA", M("AccentLight"), new Vector3(-0.58f, 0.443f, -0.20f),
                Vector3.one * 0.055f);
            Ball(t, "DecoFlowerB", M("WaterBright"), new Vector3(0.54f, 0.443f, 0.48f),
                Vector3.one * 0.060f);

            Worlds.Peps(t, new Vector3(-0.46f, 0.73f, 1.18f), new Vector3(0.38f, 0.34f, -0.36f),
                new Vector3(-0.34f, 0.73f, 1.08f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.15f, -1.44f), new Vector3(0.34f, 0.15f, -1.32f),
                new Vector3(0f, 0.15f, -1.06f));
            Worlds.Finish(root, Worlds.Weather, _dir);
        }

        /// <summary>
        /// Movers: StormBankLeft/Right, ClearBankLeft/Right, FloodStream,
        /// RiverThread, CausewayStep1-4, SluiceGate, DrainWheel, Cloud, Rain,
        /// GustBands, StormDebris, Awning, Rainbow, SunGlow, Sunbeams and
        /// MeadowBurst. The before-state and after-state are different worlds,
        /// not the same terrace with different weather particles.
        /// </summary>
        private static void WeatherDownpour()
        {
            Worlds.WeatherTint = "rain";
            var root = Worlds.Begin(Worlds.Weather, "Downpour");
            var t = root.transform;

            // Mud-choked banks sit proud of every terrace in the storm state.
            // Their bright replacements begin below the hillside and rise
            // inward, physically narrowing the flood into the final route.
            var stormBankLeft = Mover(t, "StormBankLeft");
            var stormBankRight = Mover(t, "StormBankRight");
            var clearBankLeft = Hidden(Mover(t, "ClearBankLeft"));
            var clearBankRight = Hidden(Mover(t, "ClearBankRight"));
            foreach (var (y, z, depth) in new[]
                     {
                         (0.185f, -1.08f, 0.88f), (0.455f, -0.02f, 1.02f), (0.735f, 1.04f, 0.88f),
                     })
            {
                Box(stormBankLeft, "StormShelf", M("EarthDark"), new Vector3(-0.49f, y, z),
                    new Vector3(0.39f, 0.08f, depth), new Vector3(0f, -4f, 0f));
                Box(stormBankRight, "StormShelf", M("WoodDark"), new Vector3(0.49f, y, z),
                    new Vector3(0.39f, 0.08f, depth), new Vector3(0f, 5f, 0f));

                Box(clearBankLeft, "ClearShelf", M("FoliageBright"), new Vector3(-0.49f, y - 0.12f, z),
                    new Vector3(0.55f, 0.14f, depth));
                Box(clearBankRight, "ClearShelf", M("FoliageLight"), new Vector3(0.49f, y - 0.12f, z),
                    new Vector3(0.55f, 0.14f, depth));
            }

            foreach (var (x, y, z, s) in new[]
                     {
                         (-0.48f, 0.24f, -0.96f, 0.12f), (0.46f, 0.51f, 0.16f, 0.15f),
                         (-0.42f, 0.79f, 1.18f, 0.14f),
                     })
            {
                Ball(x < 0f ? stormBankLeft : stormBankRight, "StormRock", M("Stone"),
                    new Vector3(x, y, z), Vector3.one * s);
            }

            // A continuous torrent cuts through all three elevations. Surface
            // streaks make its direction legible before the umbrella drives
            // the drainage mechanism.
            var flood = Mover(t, "FloodStream");
            foreach (var (y, z, depth) in new[]
                     {
                         (0.171f, -1.08f, 0.92f), (0.441f, -0.02f, 1.06f), (0.721f, 1.04f, 0.92f),
                     })
            {
                Box(flood, "TorrentWater", M("WaterDeep"), new Vector3(0f, y, z),
                    new Vector3(0.66f, 0.018f, depth));
            }

            var current = Living(flood, "FloodCurrent", AmbientMode.Drift, -0.52f, 0.62f,
                Vector3.forward, stagger: true, controlId: "FloodCurrent");
            for (var i = 0; i < 9; i++)
            {
                var tier = i / 3;
                var y = new[] { 0.184f, 0.454f, 0.734f }[tier];
                var z = new[] { -1.24f, -0.18f, 0.90f }[tier] + (i % 3) * 0.18f;
                Box(current, "WhiteWater", M("WaterBright"), new Vector3(-0.16f + (i % 2) * 0.30f, y, z),
                    new Vector3(0.22f, 0.010f, 0.035f));
            }

            var river = Hidden(Mover(t, "RiverThread"));
            foreach (var (y, z, depth) in new[]
                     {
                         (0.172f, -1.08f, 0.92f), (0.442f, -0.02f, 1.06f), (0.722f, 1.04f, 0.92f),
                     })
            {
                Box(river, "ClearWater", M("WaterLight"), new Vector3(0f, y, z),
                    new Vector3(0.16f, 0.020f, depth));
            }
            foreach (var (y, z) in new[] { (0.187f, -1.18f), (0.457f, -0.10f), (0.737f, 0.96f) })
            {
                Box(river, "Ripple", M("WaterBright"), new Vector3(0f, y, z),
                    new Vector3(0.13f, 0.009f, 0.030f));
            }

            // Four buried stones emerge as one long diagonal causeway. They
            // are the physical before/after difference the Peps then traverse.
            foreach (var (name, x, y, z, angle) in new[]
                     {
                         ("CausewayStep1", 0.38f, 0.59f, 0.78f, -14f),
                         ("CausewayStep2", 0.18f, 0.32f, 0.36f, -22f),
                         ("CausewayStep3", -0.04f, 0.32f, -0.06f, -28f),
                         ("CausewayStep4", -0.28f, 0.06f, -0.50f, -34f),
                     })
            {
                var step = Hidden(Mover(t, name));
                Box(step, "CausewayStone", M("StoneLight"), new Vector3(x, y, z),
                    new Vector3(0.34f, 0.10f, 0.30f), new Vector3(0f, angle, 0f));
                Box(step, "Moss", M("FoliageBright"), new Vector3(x, y + 0.058f, z),
                    new Vector3(0.24f, 0.018f, 0.20f), new Vector3(0f, angle, 0f));
            }

            // The umbrella catches the gale beside this oversized wheel. The
            // wheel's rotation pulls the gate out of the torrent, making the
            // drainage chain causal rather than a decorative weather cut.
            var sluice = Mover(t, "SluiceGate");
            Box(sluice, "Gate", M("WoodDark"), new Vector3(0f, 0.79f, 0.68f),
                new Vector3(0.70f, 0.13f, 0.11f));
            foreach (var x in new[] { -0.25f, 0f, 0.25f })
            {
                Box(sluice, "GateSlat", M("Wood"), new Vector3(x, 0.79f, 0.68f),
                    new Vector3(0.055f, 0.24f, 0.13f));
            }

            var wheel = Mover(t, "DrainWheel");
            Cog(wheel, "StormWheel", M("AccentDeep"), M("AccentLight"),
                new Vector3(0.50f, 0.93f, 0.76f), 0.20f, 8);
            Box(wheel, "WheelAxle", M("WoodDark"), new Vector3(0.50f, 0.93f, 0.76f),
                new Vector3(0.06f, 0.06f, 0.18f));

            // Weather vane on the lower terrace spinning rapidly in the storm
            var vane = Living(t, "StormVane", AmbientMode.Spin, 240f, 0.05f, Vector3.up,
                controlId: "StormVane");
            vane.localPosition = new Vector3(0.50f, 0.24f, -1.15f);
            Rod(vane, "VanePost", M("Stone"), new Vector3(0f, 0f, 0f), new Vector3(0.02f, 0.18f, 0.02f));
            Box(vane, "VaneFinA", M("Accent"), new Vector3(0.06f, 0.06f, 0f), new Vector3(0.12f, 0.04f, 0.01f));
            Box(vane, "VaneFinB", M("Accent"), new Vector3(-0.06f, 0.06f, 0f), new Vector3(0.08f, 0.02f, 0.01f));

            var awning = Mover(t, "Awning");
            foreach (var x in new[] { -0.58f, -0.20f })
            {
                Box(awning, "AwningPost", M("Wood"), new Vector3(x, 0.39f, -0.72f),
                    new Vector3(0.045f, 0.46f, 0.045f));
            }

            Box(awning, "Canopy", M("AccentDeep"), new Vector3(-0.39f, 0.64f, -0.72f),
                new Vector3(0.50f, 0.065f, 0.48f), new Vector3(-9f, 0f, 0f));

            // One cloud mass spans almost the full world silhouette. Its exit
            // therefore changes the composition rather than clearing one
            // corner above a Pep.
            var cloud = Mover(t, "Cloud");
            // Keep the storm mass enormous, but place it behind the upper
            // terrace so Pep B remains readable before the player acts.
            cloud.parent.localPosition = new Vector3(0f, 1.34f, 1.48f);
            var drift = Idle(cloud, AmbientMode.Bob, 0.045f, 0.22f, Vector3.up,
                controlId: "StormCloud");
            foreach (var (x, y, s) in new[]
                     {
                         (-0.62f, -0.02f, 0.30f), (-0.38f, 0.07f, 0.40f), (-0.10f, 0.10f, 0.44f),
                         (0.20f, 0.08f, 0.42f), (0.50f, 0.00f, 0.34f), (0f, -0.08f, 0.54f),
                     })
            {
                Ball(drift, "CloudPuff", M("Stone"), new Vector3(x, y, 0f),
                    new Vector3(s, s * 0.70f, s * 0.78f));
            }

            Box(drift, "CloudBase", M("Ink"), new Vector3(0f, -0.075f, 0f),
                new Vector3(1.35f, 0.11f, 0.34f));

            // Rain occupies all three shelves and the full screen width.
            var rain = Mover(t, "Rain");
            var fall = Living(rain, "Fall", AmbientMode.Drift, -1.15f, 1.15f, Vector3.up,
                stagger: true, controlId: "Rainfall");
            for (var i = 0; i < 18; i++)
            {
                var row = i / 6;
                Box(fall, "RainDrop", M("WaterBright"),
                    new Vector3(-0.66f + (i % 6) * 0.265f, 0.86f + (i % 3) * 0.23f,
                        -0.52f + row * 0.58f),
                    new Vector3(0.020f, 0.24f, 0.020f), new Vector3(0f, 0f, -13f));
            }

            var gusts = Mover(t, "GustBands");
            var gustFlow = Living(gusts, "GustFlow", AmbientMode.Drift, 1.55f, 0.34f,
                Vector3.right, stagger: true, controlId: "StormGusts");
            for (var i = 0; i < 6; i++)
            {
                Box(gustFlow, "Gust", M("StoneLight"),
                    new Vector3(-1.15f, 0.58f + (i % 3) * 0.28f, -0.70f + (i / 3) * 1.15f),
                    new Vector3(0.42f, 0.018f, 0.030f), new Vector3(0f, 0f, 8f));
            }

            var stormTrees = Living(t, "StormTrees", AmbientMode.Sway, 13f, 0.70f,
                Vector3.forward, stagger: true, controlId: "StormTrees");
            foreach (var (x, y, z, scale) in new[]
                     {
                         (-0.58f, 0.45f, 0.24f, 0.72f), (0.57f, 0.19f, -0.92f, 0.58f),
                     })
            {
                var tree = Child(stormTrees, "WindTree");
                tree.localPosition = new Vector3(x, y, z);
                Rod(tree, "Trunk", M("EarthDark"), new Vector3(0f, 0.16f * scale, 0f),
                    new Vector3(0.035f, 0.18f * scale, 0.035f));
                Ball(tree, "Crown", M("FoliageDark"), new Vector3(0f, 0.39f * scale, 0f),
                    new Vector3(0.24f * scale, 0.22f * scale, 0.18f * scale));
            }

            // Logs and stones are swept off only after the gate opens. Their
            // cross-screen exit is the first large consequence of drainage.
            var debris = Mover(t, "StormDebris");
            Box(debris, "WashedLog", M("WoodDark"), new Vector3(-0.04f, 0.49f, 0.20f),
                new Vector3(0.42f, 0.08f, 0.10f), new Vector3(0f, 24f, 12f));
            Box(debris, "WashedLog", M("Wood"), new Vector3(0.08f, 0.76f, 1.20f),
                new Vector3(0.34f, 0.065f, 0.09f), new Vector3(0f, -30f, -8f));
            Ball(debris, "DebrisRock", M("Stone"), new Vector3(0.16f, 0.22f, -0.92f),
                Vector3.one * 0.12f);

            // Rainbow arch (hidden initially, revealed during climax transformation)
            var rainbow = Hidden(Mover(t, "Rainbow"));
            rainbow.parent.localPosition = new Vector3(0f, 1.00f, 0.92f);
            var colors = new[] { M("Accent"), M("AccentLight"), M("WaterBright"), M("Violet") };
            for (var c = 0; c < 4; c++)
            {
                var radius = 0.52f + c * 0.040f;
                for (var seg = 0; seg < 9; seg++)
                {
                    var angle = (seg / 8f) * Mathf.PI; // 0 to 180 degrees arch
                    var x = -Mathf.Cos(angle) * radius;
                    var y = Mathf.Sin(angle) * (radius * 0.72f);
                    Ball(rainbow, $"RainbowSeg_{c}_{seg}", colors[c], new Vector3(x, y, c * -0.015f),
                        new Vector3(0.12f, 0.040f, 0.035f), new Vector3(0f, 0f, (angle * Mathf.Rad2Deg) - 90f));
                }
            }

            // Sun glow disk behind rainbow
            var sunglow = Hidden(Mover(t, "SunGlow"));
            sunglow.parent.localPosition = new Vector3(0f, 1.22f, 0.98f);
            Ball(sunglow, "GlowCore", M("AccentPale"), Vector3.zero, new Vector3(0.42f, 0.42f, 0.05f));

            var sunbeams = Hidden(Mover(t, "Sunbeams"));
            sunbeams.parent.localPosition = new Vector3(0f, 0.92f, 0.80f);
            foreach (var (x, angle) in new[]
                     {
                         (-0.52f, -22f), (-0.26f, -12f), (0f, 0f), (0.26f, 12f), (0.52f, 22f),
                     })
            {
                Box(sunbeams, "Ray", M("AccentPale"), new Vector3(x, 0f, 0f),
                    new Vector3(0.060f, 0.78f, 0.025f), new Vector3(0f, 0f, angle));
            }

            // One reveal paints all three shelves at once after the terrain
            // has moved. It is a consequence marker, not the main event.
            var meadow = Hidden(Mover(t, "MeadowBurst"));
            foreach (var (x, y, z, material) in new[]
                     {
                         (-0.58f, 0.11f, -1.15f, "AccentLight"),
                         (0.50f, 0.11f, -0.80f, "Accent"),
                         (-0.52f, 0.38f, -0.20f, "WaterBright"),
                         (0.48f, 0.38f, 0.20f, "AccentLight"),
                         (-0.50f, 0.66f, 0.92f, "Accent"),
                         (0.52f, 0.66f, 1.28f, "WaterBright"),
                     })
            {
                Rod(meadow, "Stem", M("FoliageDark"), new Vector3(x, y, z),
                    new Vector3(0.018f, 0.08f, 0.018f));
                Ball(meadow, "Bloom", M(material), new Vector3(x, y + 0.09f, z),
                    new Vector3(0.10f, 0.045f, 0.09f));
            }

            Worlds.Peps(t, new Vector3(-0.48f, 0.18f, -0.72f), new Vector3(0.48f, 0.73f, 1.18f),
                new Vector3(-0.36f, 0.18f, -0.60f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.15f, -1.44f), new Vector3(0.46f, 0.15f, -1.44f),
                new Vector3(0f, 0.15f, -1.06f));
            Worlds.Finish(root, Worlds.Weather, _dir);
        }

        // ===================================================================
        // World 4 — Windrock canyon. The gap is vertical too, and the air moves.
        // ===================================================================

        /// <summary>Movers: Thermal.</summary>
        private static void CanyonUpdraft()
        {
            var root = Worlds.Begin(Worlds.Canyon, "Updraft");
            var t = root.transform;

            // One compact thermal pocket: this is deliberately the smallest
            // event in the round, not a wind field spanning the whole chasm.
            var thermal = Mover(t, "Thermal");
            var rise = Living(thermal, "Rise", AmbientMode.Drift, 1.05f, 0.34f, Vector3.up, stagger: true);
            foreach (var (x, y, s) in new[]
                     {
                         (-0.10f, -0.46f, 0.10f), (0.04f, -0.20f, 0.08f), (-0.03f, 0.06f, 0.06f),
                     })
            {
                Box(rise, "Column", M("AccentPale"), new Vector3(x, y, -0.04f),
                    new Vector3(s, 0.24f, s));
            }

            Box(t, "LaunchLedge", M("Clay"), new Vector3(-0.10f, 0.15f, -0.50f),
                new Vector3(0.50f, 0.06f, 0.22f));
            foreach (var x in new[] { -0.30f, 0.30f })
            {
                Box(t, "LandingPost", M("WoodDark"), new Vector3(x, 0.50f, 0.62f),
                    new Vector3(0.05f, 0.16f, 0.05f));
            }

            Box(t, "LandingRail", M("Wood"), new Vector3(0f, 0.57f, 0.62f), new Vector3(0.66f, 0.035f, 0.05f));

            Worlds.Peps(t, new Vector3(-0.10f, 0.18f, -0.66f), new Vector3(0.12f, 0.42f, 0.86f),
                new Vector3(0.08f, 0.42f, 0.74f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.18f, -1.32f), new Vector3(0.46f, 0.18f, -1.44f),
                new Vector3(0f, 0.18f, -1.06f));
            Worlds.Finish(root, Worlds.Canyon, _dir);
        }

        /// <summary>
        /// Movers: NearTower, FarTower, SlackCable, TautCable (hidden), Basket,
        /// SteadyCar (hidden), CounterweightRig, Crosswind.
        /// </summary>
        private static void CanyonCablecar()
        {
            var root = Worlds.Begin(Worlds.Canyon, "Cablecar");
            var t = root.transform;

            Box(t, "NearLanding", M("Clay"), new Vector3(-0.48f, 0.19f, -0.52f),
                new Vector3(0.52f, 0.08f, 0.38f));
            Box(t, "FarLanding", M("Clay"), new Vector3(0.48f, 0.43f, 0.70f),
                new Vector3(0.54f, 0.08f, 0.40f));

            var nearTower = Mover(t, "NearTower");
            nearTower.parent.localPosition = new Vector3(-0.58f, 0.18f, -0.52f);
            Box(nearTower, "Mast", M("WoodDark"), new Vector3(0f, 0.48f, 0f),
                new Vector3(0.11f, 0.96f, 0.11f));
            Beam(nearTower, "Crossarm", M("Wood"), new Vector3(-0.25f, 0.88f, 0f),
                new Vector3(0.25f, 0.88f, 0f), 0.07f);
            foreach (var x in new[] { -0.20f, 0.20f })
                Rod(nearTower, "Pulley", M("Accent"), new Vector3(x, 0.88f, 0f),
                    new Vector3(0.09f, 0.025f, 0.09f), new Vector3(90f, 0f, 0f));

            var farTower = Mover(t, "FarTower");
            farTower.parent.localPosition = new Vector3(0.58f, 0.42f, 0.64f);
            Box(farTower, "Mast", M("WoodDark"), new Vector3(0f, 0.49f, 0f),
                new Vector3(0.11f, 0.98f, 0.11f));
            Beam(farTower, "Crossarm", M("Wood"), new Vector3(-0.25f, 0.91f, 0f),
                new Vector3(0.25f, 0.91f, 0f), 0.07f);
            foreach (var x in new[] { -0.20f, 0.20f })
                Rod(farTower, "Pulley", M("Accent"), new Vector3(x, 0.91f, 0f),
                    new Vector3(0.09f, 0.025f, 0.09f), new Vector3(90f, 0f, 0f));

            var nearTop = new Vector3(-0.58f, 1.06f, -0.52f);
            var farTop = new Vector3(0.58f, 1.33f, 0.64f);
            var sag = new Vector3(-0.06f, 0.78f, 0.02f);
            var slack = Mover(t, "SlackCable");
            Beam(slack, "CableNear", M("Stone"), nearTop, sag, 0.025f);
            Beam(slack, "CableFar", M("Stone"), sag, farTop, 0.025f);
            Beam(slack, "ReturnNear", M("Cream"), nearTop + Vector3.forward * 0.07f,
                sag + Vector3.forward * 0.07f, 0.012f);
            Beam(slack, "ReturnFar", M("Cream"), sag + Vector3.forward * 0.07f,
                farTop + Vector3.forward * 0.07f, 0.012f);

            var taut = Hidden(Mover(t, "TautCable"));
            Beam(taut, "MainCable", M("Stone"), nearTop, farTop, 0.030f);
            Beam(taut, "ReturnCable", M("Cream"), nearTop + Vector3.forward * 0.07f,
                farTop + Vector3.forward * 0.07f, 0.014f);

            // A diagonal cableway occupies both height and width. Its live car
            // is unsafe; the still twin becomes a route only after the whole
            // tower/cable/counterweight system reacts.
            var basket = Mover(t, "Basket");
            basket.parent.localPosition = new Vector3(-0.42f, 0.91f, -0.34f);
            var swing = Idle(basket, AmbientMode.Sway, 17f, 0.55f, Vector3.forward,
                controlId: "BasketSwing");
            Box(swing, "Hanger", M("Stone"), new Vector3(0f, -0.15f, 0f), new Vector3(0.025f, 0.30f, 0.025f));
            Box(swing, "Car", M("Wood"), new Vector3(0f, -0.39f, 0f), new Vector3(0.44f, 0.17f, 0.40f));
            Box(swing, "CarFloor", M("Sand"), new Vector3(0f, -0.30f, 0f), new Vector3(0.40f, 0.025f, 0.36f));
            foreach (var x in new[] { -0.20f, 0.20f })
            {
                Box(swing, "CarRail", M("WoodDark"), new Vector3(x, -0.25f, 0f),
                    new Vector3(0.025f, 0.18f, 0.38f));
            }

            var steady = Hidden(Mover(t, "SteadyCar"));
            steady.parent.localPosition = new Vector3(-0.42f, 0.91f, -0.34f);
            Box(steady, "Hanger", M("Stone"), new Vector3(0f, -0.15f, 0f),
                new Vector3(0.025f, 0.30f, 0.025f));
            Box(steady, "Car", M("Wood"), new Vector3(0f, -0.39f, 0f), new Vector3(0.44f, 0.17f, 0.40f));
            Box(steady, "CarFloor", M("Sand"), new Vector3(0f, -0.30f, 0f),
                new Vector3(0.40f, 0.025f, 0.36f));
            foreach (var x in new[] { -0.20f, 0.20f })
            {
                Box(steady, "CarRail", M("WoodDark"), new Vector3(x, -0.25f, 0f),
                    new Vector3(0.025f, 0.16f, 0.38f));
            }

            var rig = Mover(t, "CounterweightRig");
            rig.parent.localPosition = new Vector3(-0.76f, 0.74f, -0.50f);
            Beam(rig, "GuyLine", M("Cream"), new Vector3(0f, 0.36f, 0f),
                new Vector3(0f, -0.34f, 0f), 0.018f);
            Box(rig, "Cradle", M("Accent"), new Vector3(0f, -0.38f, 0f),
                new Vector3(0.25f, 0.08f, 0.22f));
            Box(rig, "BrakeArm", M("WoodDark"), new Vector3(0.13f, 0.18f, 0f),
                new Vector3(0.05f, 0.42f, 0.05f), new Vector3(0f, 0f, -24f));

            var crosswind = Mover(t, "Crosswind");
            var ribbons = Living(crosswind, "WindRibbons", AmbientMode.Drift, 1.55f, 0.38f,
                new Vector3(1f, 0.10f, 0f), stagger: true, controlId: "CableWind");
            for (var i = 0; i < 7; i++)
                Box(ribbons, "Ribbon", M("AccentPale"),
                    new Vector3(-0.78f + i * 0.25f, 0.60f + (i % 3) * 0.20f, -0.34f + (i % 2) * 0.68f),
                    new Vector3(0.24f, 0.018f, 0.035f), new Vector3(0f, 0f, 8f));

            Worlds.Peps(t, new Vector3(-0.58f, 0.22f, -0.74f), new Vector3(0.58f, 0.46f, 0.92f),
                new Vector3(0.48f, 0.46f, 0.82f));
            Worlds.Slots(t, new Vector3(-0.46f, 0.18f, -1.26f), new Vector3(0.46f, 0.18f, -1.40f),
                new Vector3(0f, 0.18f, -1.04f));
            Worlds.Finish(root, Worlds.Canyon, _dir);
        }

        /// <summary>
        /// Movers: Spire, GrappleLine, both RimCrowns and Rockfalls; hidden
        /// FallenSpan, AfterRims, RockSteps, FaultCracks and SpireDust.
        /// </summary>
        private static void CanyonSpire()
        {
            var root = Worlds.Begin(Worlds.Canyon, "Spire");
            var t = root.transform;

            // A leaning monolith and two hoodoos make a tall, broken skyline.
            // The successful state replaces that skyline with a broad diagonal
            // shelf, so the before/after reads even in silhouette.
            var spire = Mover(t, "Spire");
            spire.parent.localPosition = new Vector3(0.25f, -0.64f, 0.08f);
            Beam(spire, "Monolith", M("Clay"), Vector3.zero, new Vector3(-0.16f, 2.25f, 0.08f), 0.40f);
            Beam(spire, "DarkFace", M("EarthDark"), new Vector3(-0.16f, 0.10f, -0.12f),
                new Vector3(-0.30f, 2.16f, -0.04f), 0.15f);
            Box(spire, "Crown", M("Sand"), new Vector3(-0.17f, 2.18f, 0.08f),
                new Vector3(0.62f, 0.16f, 0.46f), new Vector3(0f, 0f, -8f));
            Box(spire, "Overhang", M("EarthLight"), new Vector3(0.12f, 1.70f, 0.04f),
                new Vector3(0.64f, 0.18f, 0.42f), new Vector3(0f, 0f, -18f));
            foreach (var y in new[] { 0.45f, 1.02f, 1.52f })
                Box(spire, "Stratum", M("Earth"), new Vector3(-0.08f, y, 0.08f),
                    new Vector3(0.46f, 0.075f, 0.43f), new Vector3(0f, 0f, -4f));

            var nearCrown = Mover(t, "RimCrownNear");
            nearCrown.parent.localPosition = new Vector3(-0.62f, 0.17f, -0.46f);
            Beam(nearCrown, "Hoodoo", M("EarthLight"), Vector3.zero, new Vector3(0.10f, 0.88f, 0.02f), 0.25f);
            Box(nearCrown, "Cap", M("Sand"), new Vector3(0.12f, 0.86f, 0.02f),
                new Vector3(0.42f, 0.12f, 0.32f), new Vector3(0f, 0f, 7f));

            var farCrown = Mover(t, "RimCrownFar");
            farCrown.parent.localPosition = new Vector3(0.63f, 0.41f, 0.62f);
            Beam(farCrown, "Hoodoo", M("Clay"), Vector3.zero, new Vector3(-0.12f, 1.02f, 0f), 0.28f);
            Box(farCrown, "Cap", M("Sand"), new Vector3(-0.14f, 1.00f, 0f),
                new Vector3(0.46f, 0.13f, 0.34f), new Vector3(0f, 0f, -8f));

            var grappleLine = Hidden(Mover(t, "GrappleLine"));
            Beam(grappleLine, "Rope", M("Cream"), new Vector3(-0.58f, 0.30f, -0.70f),
                new Vector3(0.08f, 1.55f, 0.15f), 0.024f);

            var fault = Hidden(Mover(t, "FaultCracks"));
            Beam(fault, "CrackNear", M("AccentPale"), new Vector3(-0.70f, 0.22f, -0.43f),
                new Vector3(-0.23f, 0.30f, -0.18f), 0.025f);
            Beam(fault, "CrackFar", M("AccentPale"), new Vector3(0.23f, 0.44f, 0.30f),
                new Vector3(0.72f, 0.50f, 0.60f), 0.025f);

            var rockfallNear = Mover(t, "RockfallNear");
            foreach (var (x, y, z, s) in new[]
                     {
                         (-0.64f, 0.98f, -0.46f, 0.16f), (-0.45f, 0.72f, -0.40f, 0.11f),
                     })
                Ball(rockfallNear, "Boulder", M("Clay"), new Vector3(x, y, z), Vector3.one * s);

            var rockfallFar = Mover(t, "RockfallFar");
            foreach (var (x, y, z, s) in new[]
                     {
                         (0.52f, 1.30f, 0.62f, 0.17f), (0.70f, 1.05f, 0.54f, 0.12f),
                     })
                Ball(rockfallFar, "Boulder", M("EarthLight"), new Vector3(x, y, z), Vector3.one * s);

            var dust = Hidden(Mover(t, "SpireDust"));
            foreach (var (x, z, s) in new[]
                     {
                         (-0.72f, -0.42f, 0.34f), (-0.30f, -0.08f, 0.30f),
                         (0.18f, 0.30f, 0.38f), (0.68f, 0.66f, 0.32f),
                     })
            {
                Ball(dust, "Plume", M("Sand"), new Vector3(x, 0.24f, z),
                    new Vector3(s, s * 0.60f, s * 0.8f));
            }

            var span = Hidden(Mover(t, "FallenSpan"));
            var spanFrom = new Vector3(-0.68f, 0.28f, -0.56f);
            var spanTo = new Vector3(0.68f, 0.52f, 0.70f);
            var spanJointOne = new Vector3(-0.22f, 0.37f, -0.15f);
            var spanJointTwo = new Vector3(0.26f, 0.45f, 0.32f);
            Beam(span, "MonolithBridgeNear", M("Clay"), spanFrom, spanJointOne, 0.44f);
            Beam(span, "MonolithBridgeMid", M("EarthLight"), spanJointOne, spanJointTwo, 0.36f);
            Beam(span, "MonolithBridgeFar", M("Clay"), spanJointTwo, spanTo, 0.41f);
            Beam(span, "WalkFacetNear", M("EarthLight"), spanFrom + Vector3.up * 0.13f,
                spanJointOne + Vector3.up * 0.13f, 0.29f);
            Beam(span, "WalkFacetMid", M("Clay"), spanJointOne + Vector3.up * 0.12f,
                spanJointTwo + Vector3.up * 0.12f, 0.25f);
            Beam(span, "WalkFacetFar", M("EarthLight"), spanJointTwo + Vector3.up * 0.13f,
                spanTo + Vector3.up * 0.13f, 0.28f);
            Beam(span, "DarkUnderbelly", M("EarthDark"), spanFrom + Vector3.down * 0.12f,
                spanTo + Vector3.down * 0.12f, 0.30f);
            Ball(span, "ImpactBoulder", M("Earth"), spanJointOne + Vector3.up * 0.18f,
                new Vector3(0.24f, 0.14f, 0.22f));
            Ball(span, "ImpactBoulder", M("Sand"), spanJointTwo + Vector3.up * 0.17f,
                new Vector3(0.20f, 0.12f, 0.18f));

            var afterNear = Hidden(Mover(t, "AfterRimNear"));
            Box(afterNear, "BrokenShelf", M("Clay"), new Vector3(-0.52f, 0.25f, -0.54f),
                new Vector3(0.72f, 0.18f, 0.52f), new Vector3(0f, 8f, 0f));
            Ball(afterNear, "Rubble", M("EarthLight"), new Vector3(-0.30f, 0.40f, -0.30f),
                new Vector3(0.20f, 0.12f, 0.18f));

            var afterFar = Hidden(Mover(t, "AfterRimFar"));
            Box(afterFar, "BrokenShelf", M("Clay"), new Vector3(0.50f, 0.48f, 0.62f),
                new Vector3(0.76f, 0.18f, 0.54f), new Vector3(0f, -10f, 0f));
            Ball(afterFar, "Rubble", M("EarthLight"), new Vector3(0.28f, 0.62f, 0.38f),
                new Vector3(0.22f, 0.13f, 0.20f));

            foreach (var (name, p, scale) in new[]
                     {
                         ("RockStepNear", new Vector3(-0.40f, 0.34f, -0.30f), new Vector3(0.36f, 0.13f, 0.32f)),
                         ("RockStepMid", new Vector3(0.00f, 0.42f, 0.06f), new Vector3(0.40f, 0.15f, 0.34f)),
                         ("RockStepFar", new Vector3(0.38f, 0.49f, 0.42f), new Vector3(0.36f, 0.13f, 0.32f)),
                     })
            {
                var step = Hidden(Mover(t, name));
                Box(step, "Slab", M("EarthLight"), p, scale, new Vector3(0f, 12f, 0f));
                Box(step, "Top", M("Sand"), p + Vector3.up * (scale.y * 0.52f),
                    new Vector3(scale.x * 0.88f, 0.025f, scale.z * 0.86f), new Vector3(0f, 12f, 0f));
            }

            Worlds.Peps(t, new Vector3(-0.58f, 0.22f, -0.72f), new Vector3(0.60f, 0.46f, 0.90f),
                new Vector3(0.50f, 0.46f, 0.80f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.18f, -1.34f), new Vector3(0.46f, 0.18f, -1.22f),
                new Vector3(0f, 0.18f, -1.00f));
            Worlds.Finish(root, Worlds.Canyon, _dir);
        }

        // ===================================================================
        // World 5 — Tidewater docks. Everything floats or sinks, and the sea moves.
        // ===================================================================

        /// <summary>Movers: Punt, Bilge.</summary>
        private static void TidePunt()
        {
            var root = Worlds.Begin(Worlds.Tide, "Punt");
            var t = root.transform;

            Box(t, "Jetty", M("Wood"), new Vector3(-0.42f, 0.30f, -0.55f), new Vector3(0.72f, 0.06f, 1.10f));
            Box(t, "FarDock", M("Wood"), new Vector3(0.34f, 0.30f, 1.10f), new Vector3(0.80f, 0.06f, 1.05f));
            foreach (var (x, z) in new[] { (-0.68f, -0.95f), (-0.16f, -0.95f), (0.02f, 1.45f), (0.66f, 1.45f) })
            {
                Rod(t, "Piling", M("WoodDark"), new Vector3(x, 0.14f, z), new Vector3(0.075f, 0.24f, 0.075f));
            }

            // Riding low and full of water. The gunwale sitting *below* the
            // jetty is the whole read: you cannot step down into a bath.
            var punt = Mover(t, "Punt");
            punt.parent.localPosition = new Vector3(0.10f, 0.03f, 0.20f);
            var bob = Idle(punt, AmbientMode.Bob, 0.010f, 0.36f, Vector3.up);
            Box(bob, "Hull", M("Wood"), new Vector3(0f, 0.05f, 0f), new Vector3(0.48f, 0.11f, 0.92f));
            Box(bob, "HullTrim", M("WoodDark"), new Vector3(0f, 0.105f, 0f), new Vector3(0.50f, 0.02f, 0.94f));
            foreach (var x in new[] { -0.24f, 0.24f })
            {
                Box(bob, "Gunwale", M("WoodMid"), new Vector3(x, 0.10f, 0f), new Vector3(0.04f, 0.09f, 0.92f));
            }

            var bilge = Mover(t, "Bilge");
            Box(bilge, "Water", M("Water"), new Vector3(0.10f, 0.10f, 0.20f), new Vector3(0.42f, 0.05f, 0.86f));
            Box(bilge, "Sheen", M("WaterBright"), new Vector3(0.10f, 0.126f, 0.10f),
                new Vector3(0.30f, 0.01f, 0.20f));

            Worlds.Peps(t, new Vector3(-0.42f, 0.33f, -0.62f), new Vector3(0.34f, 0.33f, 1.10f),
                new Vector3(0.18f, 0.33f, 0.92f));
            Worlds.Slots(t, new Vector3(-0.52f, 0.095f, -1.52f), new Vector3(0.54f, 0.095f, -1.62f),
                new Vector3(0.02f, 0.095f, -1.30f));
            Worlds.Finish(root, Worlds.Tide, _dir);
        }

        /// <summary>
        /// Movers: four lock gates, LockWaterLow/High, Raft, Mooring,
        /// Capstan, LevelMarker and Wake (hidden).
        /// </summary>
        private static void TideChannel()
        {
            var root = Worlds.Begin(Worlds.Tide, "Channel");
            var t = root.transform;

            // A real navigation system rather than open water: two long
            // embankments, two pairs of gates, a changing lock level and a
            // diagonal departure/arrival composition.
            Box(t, "LeftEmbankment", M("WoodMid"), new Vector3(-0.72f, 0.20f, 0.10f),
                new Vector3(0.56f, 0.20f, 2.72f));
            Box(t, "LeftDeck", M("Wood"), new Vector3(-0.72f, 0.315f, 0.10f),
                new Vector3(0.52f, 0.05f, 2.66f));
            Box(t, "RightEmbankment", M("WoodMid"), new Vector3(0.72f, 0.20f, 0.10f),
                new Vector3(0.56f, 0.20f, 2.72f));
            Box(t, "RightDeck", M("Wood"), new Vector3(0.72f, 0.315f, 0.10f),
                new Vector3(0.52f, 0.05f, 2.66f));
            foreach (var z in new[] { -1.06f, -0.34f, 0.62f, 1.30f })
            foreach (var x in new[] { -0.47f, 0.47f })
                Rod(t, "LockPiling", M("WoodDark"), new Vector3(x, 0.26f, z),
                    new Vector3(0.065f, 0.32f, 0.065f));

            var lowerLeft = Mover(t, "LowerGateLeft");
            lowerLeft.parent.localPosition = new Vector3(-0.45f, 0.12f, -0.36f);
            Box(lowerLeft, "GateLeaf", M("WoodDark"), new Vector3(0.22f, 0.16f, 0f),
                new Vector3(0.44f, 0.32f, 0.08f));
            Box(lowerLeft, "Brace", M("Accent"), new Vector3(0.22f, 0.18f, -0.05f),
                new Vector3(0.36f, 0.045f, 0.035f), new Vector3(0f, 0f, 18f));

            var lowerRight = Mover(t, "LowerGateRight");
            lowerRight.parent.localPosition = new Vector3(0.45f, 0.12f, -0.36f);
            Box(lowerRight, "GateLeaf", M("WoodDark"), new Vector3(-0.22f, 0.16f, 0f),
                new Vector3(0.44f, 0.32f, 0.08f));
            Box(lowerRight, "Brace", M("Accent"), new Vector3(-0.22f, 0.18f, -0.05f),
                new Vector3(0.36f, 0.045f, 0.035f), new Vector3(0f, 0f, -18f));

            var upperLeft = Mover(t, "UpperGateLeft");
            upperLeft.parent.localPosition = new Vector3(-0.45f, 0.14f, 0.68f);
            Box(upperLeft, "GateLeaf", M("WoodDark"), new Vector3(0.22f, 0.16f, 0f),
                new Vector3(0.44f, 0.32f, 0.08f));
            Box(upperLeft, "Brace", M("Accent"), new Vector3(0.22f, 0.18f, -0.05f),
                new Vector3(0.36f, 0.045f, 0.035f), new Vector3(0f, 0f, 18f));

            var upperRight = Mover(t, "UpperGateRight");
            upperRight.parent.localPosition = new Vector3(0.45f, 0.14f, 0.68f);
            Box(upperRight, "GateLeaf", M("WoodDark"), new Vector3(-0.22f, 0.16f, 0f),
                new Vector3(0.44f, 0.32f, 0.08f));
            Box(upperRight, "Brace", M("Accent"), new Vector3(-0.22f, 0.18f, -0.05f),
                new Vector3(0.36f, 0.045f, 0.035f), new Vector3(0f, 0f, -18f));

            var lowWater = Mover(t, "LockWaterLow");
            Box(lowWater, "LowPool", M("WaterDeep"), new Vector3(0f, 0.085f, 0.16f),
                new Vector3(0.86f, 0.05f, 0.98f));
            for (var i = 0; i < 3; i++)
                Box(lowWater, "LowMark", M("WaterBright"), new Vector3(-0.24f + i * 0.24f, 0.116f, 0.14f),
                    new Vector3(0.16f, 0.012f, 0.04f), new Vector3(0f, i * 10f - 10f, 0f));

            var highWater = Hidden(Mover(t, "LockWaterHigh"));
            Box(highWater, "HighPool", M("Water"), new Vector3(0f, 0.175f, 0.16f),
                new Vector3(0.88f, 0.14f, 1.02f));
            for (var i = 0; i < 4; i++)
                Box(highWater, "HighMark", M("WaterLight"), new Vector3(-0.30f + i * 0.20f, 0.251f, 0.12f),
                    new Vector3(0.15f, 0.012f, 0.04f), new Vector3(0f, i * 9f - 14f, 0f));

            var capstan = Mover(t, "Capstan");
            capstan.parent.localPosition = new Vector3(-0.72f, 0.44f, -0.05f);
            Rod(capstan, "Axle", M("WoodDark"), Vector3.zero, new Vector3(0.08f, 0.20f, 0.08f));
            Cog(capstan, "Wheel", M("Accent"), M("AccentLight"), new Vector3(0f, 0.15f, 0f), 0.22f, 8, 0.07f);
            Beam(capstan, "ChainToLower", M("Cream"), new Vector3(0.08f, 0.10f, 0f),
                new Vector3(0.28f, -0.18f, -0.31f), 0.015f);
            Beam(capstan, "ChainToUpper", M("Cream"), new Vector3(0.08f, 0.10f, 0f),
                new Vector3(0.28f, -0.18f, 0.73f), 0.015f);

            var level = Mover(t, "LevelMarker");
            level.parent.localPosition = new Vector3(0.58f, 0.24f, 0.15f);
            Box(level, "Gauge", M("Cream"), Vector3.zero, new Vector3(0.06f, 0.48f, 0.06f));
            Box(level, "Float", M("Accent"), new Vector3(0f, -0.18f, 0f), new Vector3(0.18f, 0.07f, 0.12f));

            var raft = Mover(t, "Raft");
            raft.parent.localPosition = new Vector3(-0.05f, 0.12f, -0.88f);
            var bob = Idle(raft, AmbientMode.Bob, 0.012f, 0.40f, Vector3.up);
            for (var i = -2; i <= 2; i++)
            {
                Box(bob, "Log", i % 2 == 0 ? M("Wood") : M("WoodMid"), new Vector3(i * 0.11f, 0.03f, 0f),
                    new Vector3(0.10f, 0.06f, 0.52f));
            }

            Box(bob, "Lashing", M("Earth"), new Vector3(0f, 0.065f, 0.18f), new Vector3(0.56f, 0.014f, 0.014f));
            Box(bob, "Post", M("WoodDark"), new Vector3(0.20f, 0.14f, -0.18f), new Vector3(0.035f, 0.18f, 0.035f));

            var mooring = Mover(t, "Mooring");
            Beam(mooring, "Line", M("Cream"), new Vector3(-0.24f, 0.20f, -0.90f),
                new Vector3(-0.50f, 0.26f, -0.98f), 0.015f);

            var wake = Hidden(Mover(t, "Wake"));
            for (var i = 0; i < 5; i++)
                Box(wake, "WakeLine", M("WaterLight"), new Vector3(-0.28f + i * 0.14f, 0.23f, -0.28f + i * 0.25f),
                    new Vector3(0.26f, 0.014f, 0.045f), new Vector3(0f, i * 8f - 18f, 0f));

            Worlds.Peps(t, new Vector3(-0.66f, 0.35f, -0.98f), new Vector3(0.66f, 0.35f, 1.10f),
                new Vector3(0.56f, 0.35f, 0.98f));
            Worlds.Slots(t, new Vector3(-0.52f, 0.095f, -1.58f), new Vector3(0.54f, 0.095f, -1.48f),
                new Vector3(0.02f, 0.095f, -1.28f));
            Worlds.Finish(root, Worlds.Tide, _dir);
        }

        /// <summary>
        /// Movers: LowTideWorld/HighTideWorld, TideGate, TideWheel,
        /// GateChains, SurgeFront, CurrentField, three stranded/floating
        /// harbor state pairs, and the refloated TideRaft.
        /// </summary>
        private static void TideCurrent()
        {
            var root = Worlds.Begin(Worlds.Tide, "Current");
            var t = root.transform;

            // BEFORE: exposed mud occupies most of the sea, vessels are
            // stranded at different angles, and the harbor gate seals the
            // horizon. This entire mover disappears when the tide arrives.
            var lowTide = Mover(t, "LowTideWorld");
            Box(lowTide, "MudLeft", M("WoodMid"), new Vector3(-0.67f, 0.13f, -0.05f),
                new Vector3(0.82f, 0.19f, 2.92f), new Vector3(0f, -4f, 0f));
            Box(lowTide, "MudRight", M("Sand"), new Vector3(0.67f, 0.14f, 0.08f),
                new Vector3(0.80f, 0.21f, 2.78f), new Vector3(0f, 5f, 0f));
            Box(lowTide, "MudTongueNear", M("EarthLight"), new Vector3(-0.22f, 0.13f, -0.82f),
                new Vector3(0.56f, 0.17f, 0.90f), new Vector3(0f, 18f, 0f));
            Box(lowTide, "MudTongueFar", M("WoodMid"), new Vector3(0.22f, 0.14f, 0.62f),
                new Vector3(0.54f, 0.18f, 0.88f), new Vector3(0f, -16f, 0f));
            Box(lowTide, "LowChannel", M("WaterDeep"), new Vector3(0f, 0.085f, 0f),
                new Vector3(0.42f, 0.055f, 3.18f), new Vector3(0f, -3f, 0f));
            for (var i = 0; i < 8; i++)
                Box(lowTide, "TideGroove", i % 2 == 0 ? M("Earth") : M("WoodDark"),
                    new Vector3(i % 2 == 0 ? -0.58f : 0.58f, 0.242f, -1.08f + i * 0.31f),
                    new Vector3(0.42f, 0.014f, 0.035f), new Vector3(0f, i % 2 == 0 ? -12f : 12f, 0f));

            // AFTER: a higher plane covers the full low-tide delta and moves
            // upward during the surge. Broad crests make the new water level
            // legible without relying on transparency or particles.
            var highTide = Hidden(Mover(t, "HighTideWorld"));
            Box(highTide, "HighSea", M("Water"), new Vector3(0f, 0.10f, 0f),
                new Vector3(2.22f, 0.18f, 3.55f));
            for (var i = 0; i < 7; i++)
                Box(highTide, "TideCrest", i % 2 == 0 ? M("WaterLight") : M("WaterBright"),
                    new Vector3(-0.76f + (i % 4) * 0.50f, 0.198f, -1.22f + (i / 4) * 1.75f + (i % 2) * 0.30f),
                    new Vector3(0.42f, 0.016f, 0.055f), new Vector3(0f, i * 8f - 20f, 0f));

            // A barrage across the horizon gives the tide a visible source.
            Box(t, "GateTowerLeft", M("WoodDark"), new Vector3(-0.92f, 0.46f, 1.28f),
                new Vector3(0.20f, 0.82f, 0.24f));
            Box(t, "GateTowerRight", M("WoodDark"), new Vector3(0.92f, 0.46f, 1.28f),
                new Vector3(0.20f, 0.82f, 0.24f));
            Box(t, "HarborBeam", M("Accent"), new Vector3(0f, 0.86f, 1.28f),
                new Vector3(2.02f, 0.12f, 0.20f));

            var gate = Mover(t, "TideGate");
            gate.parent.localPosition = new Vector3(0f, 0.10f, 1.27f);
            Box(gate, "GateSlab", M("Wood"), new Vector3(0f, 0.31f, 0f),
                new Vector3(1.64f, 0.62f, 0.14f));
            foreach (var x in new[] { -0.58f, -0.20f, 0.20f, 0.58f })
                Box(gate, "GateRib", M("WoodDark"), new Vector3(x, 0.31f, -0.08f),
                    new Vector3(0.07f, 0.58f, 0.05f));

            var wheel = Mover(t, "TideWheel");
            wheel.parent.localPosition = new Vector3(0.76f, 0.68f, 1.10f);
            Cog(wheel, "ReleaseWheel", M("Accent"), M("AccentLight"), Vector3.zero, 0.25f, 10, 0.08f);
            Rod(wheel, "FloatLatch", M("Cream"), new Vector3(0f, -0.30f, 0f),
                new Vector3(0.055f, 0.28f, 0.055f));

            var chains = Mover(t, "GateChains");
            Beam(chains, "ChainLeft", M("Cream"), new Vector3(-0.70f, 0.78f, 1.18f),
                new Vector3(-0.55f, 0.37f, 1.20f), 0.018f);
            Beam(chains, "ChainRight", M("Cream"), new Vector3(0.70f, 0.78f, 1.18f),
                new Vector3(0.55f, 0.37f, 1.20f), 0.018f);

            var surge = Hidden(Mover(t, "SurgeFront"));
            Box(surge, "WaveWall", M("WaterBright"), new Vector3(0f, 0.34f, 1.02f),
                new Vector3(2.16f, 0.26f, 0.14f), new Vector3(0f, 0f, -4f));
            for (var i = 0; i < 7; i++)
                Ball(surge, "Breaker", M("WaterLight"), new Vector3(-0.90f + i * 0.30f, 0.49f, 1.00f),
                    new Vector3(0.24f, 0.14f, 0.14f));

            var current = Hidden(Mover(t, "CurrentField"));
            var flow = Living(current, "WholeHarborCurrent", AmbientMode.Drift, 1.60f, 0.52f,
                new Vector3(0.55f, 0f, -1f), stagger: true, controlId: "HarborCurrent");
            for (var i = 0; i < 12; i++)
                Box(flow, "CurrentStripe", M("WaterLight"),
                    new Vector3(-0.82f + (i % 4) * 0.55f, 0.225f, -1.18f + (i / 4) * 0.88f),
                    new Vector3(0.34f, 0.015f, 0.055f), new Vector3(0f, -28f, 0f));

            // Three harbor structures each have an unmistakable stranded and
            // floating state. Their different rise/drift vectors make the
            // water causal across the screen instead of one swapped backdrop.
            var strandedLeft = Mover(t, "StrandedBoatLeft");
            Box(strandedLeft, "Hull", M("WoodDark"), new Vector3(-0.67f, 0.27f, 0.34f),
                new Vector3(0.42f, 0.14f, 0.72f), new Vector3(0f, -18f, -15f));
            Box(strandedLeft, "Trim", M("Cream"), new Vector3(-0.67f, 0.34f, 0.34f),
                new Vector3(0.38f, 0.04f, 0.68f), new Vector3(0f, -18f, -15f));
            var afloatLeft = Hidden(Mover(t, "FloatingBoatLeft"));
            Box(afloatLeft, "Hull", M("WoodDark"), new Vector3(-0.64f, 0.19f, 0.32f),
                new Vector3(0.42f, 0.14f, 0.72f), new Vector3(0f, -8f, 0f));
            Box(afloatLeft, "Trim", M("Cream"), new Vector3(-0.64f, 0.27f, 0.32f),
                new Vector3(0.38f, 0.04f, 0.68f), new Vector3(0f, -8f, 0f));

            var strandedRight = Mover(t, "StrandedBoatRight");
            Box(strandedRight, "Hull", M("Wood"), new Vector3(0.62f, 0.25f, -0.18f),
                new Vector3(0.40f, 0.13f, 0.66f), new Vector3(0f, 22f, 13f));
            Box(strandedRight, "Mast", M("WoodDark"), new Vector3(0.67f, 0.54f, -0.20f),
                new Vector3(0.045f, 0.54f, 0.045f), new Vector3(0f, 0f, 13f));
            var afloatRight = Hidden(Mover(t, "FloatingBoatRight"));
            Box(afloatRight, "Hull", M("Wood"), new Vector3(0.60f, 0.18f, -0.18f),
                new Vector3(0.40f, 0.13f, 0.66f), new Vector3(0f, 10f, 0f));
            Box(afloatRight, "Mast", M("WoodDark"), new Vector3(0.60f, 0.48f, -0.18f),
                new Vector3(0.045f, 0.54f, 0.045f));

            var collapsed = Mover(t, "CollapsedDock");
            Box(collapsed, "RootDeck", M("Wood"), new Vector3(-0.64f, 0.29f, -0.88f),
                new Vector3(0.58f, 0.10f, 0.62f), new Vector3(0f, -8f, 0f));
            Box(collapsed, "BrokenRun", M("WoodMid"), new Vector3(-0.40f, 0.19f, -0.43f),
                new Vector3(0.48f, 0.09f, 0.66f), new Vector3(20f, -16f, 10f));
            Box(collapsed, "BrokenTip", M("WoodDark"), new Vector3(-0.12f, 0.10f, -0.04f),
                new Vector3(0.42f, 0.08f, 0.46f), new Vector3(28f, 18f, -8f));

            var tideRaft = Hidden(Mover(t, "TideRaft"));
            tideRaft.parent.localPosition = new Vector3(-0.62f, 0.10f, -0.84f);
            var raftBob = Idle(tideRaft, AmbientMode.Bob, 0.018f, 0.38f, Vector3.up);
            for (var i = -3; i <= 3; i++)
                Box(raftBob, "PontoonLog", i % 2 == 0 ? M("Wood") : M("WoodMid"),
                    new Vector3(i * 0.09f, 0.04f, 0f), new Vector3(0.085f, 0.08f, 0.82f));
            Box(raftBob, "Deck", M("Sand"), new Vector3(0f, 0.105f, 0f),
                new Vector3(0.58f, 0.055f, 0.74f));
            foreach (var x in new[] { -0.32f, 0.32f })
                Rod(raftBob, "RailPost", M("WoodDark"), new Vector3(x, 0.21f, 0.20f),
                    new Vector3(0.035f, 0.14f, 0.035f));

            // The far harbor remains fixed: it is the only visual reference
            // that lets the player read how much every floating part moved.
            Box(t, "FarHarbor", M("Wood"), new Vector3(0.62f, 0.34f, 1.03f),
                new Vector3(0.74f, 0.10f, 0.68f));
            foreach (var x in new[] { 0.32f, 0.92f })
                Rod(t, "FarPiling", M("WoodDark"), new Vector3(x, 0.22f, 1.18f),
                    new Vector3(0.075f, 0.34f, 0.075f));

            Worlds.Peps(t, new Vector3(-0.66f, 0.35f, -0.92f), new Vector3(0.62f, 0.40f, 1.02f),
                new Vector3(0.52f, 0.40f, 0.92f));
            Worlds.Slots(t, new Vector3(-0.52f, 0.095f, -1.46f), new Vector3(0.02f, 0.095f, -1.68f),
                new Vector3(0.54f, 0.095f, -1.46f));
            Worlds.Finish(root, Worlds.Tide, _dir);
        }

        // ===================================================================
        // World 6 — Storm rooftop. The wind has a direction and takes things.
        // ===================================================================

        /// <summary>Movers: Tarp, TarpCorner.</summary>
        private static void StormTarp()
        {
            var root = Worlds.Begin(Worlds.Storm, "Tarp");
            var t = root.transform;

            Box(t, "Chimney", M("Violet"), new Vector3(-0.42f, 0.30f, 1.05f), new Vector3(0.26f, 0.44f, 0.26f));
            Box(t, "ChimneyCap", M("Stone"), new Vector3(-0.42f, 0.54f, 1.05f), new Vector3(0.32f, 0.05f, 0.32f));
            foreach (var z in new[] { -0.30f, 0.62f })
            {
                Box(t, "TarpPost", M("Stone"), new Vector3(0.46f, 0.26f, z), new Vector3(0.04f, 0.38f, 0.04f));
            }

            // Held on one side and loose on the other: the flapping corner is
            // the thing the sandbag is for, and it must be obviously loose.
            var tarp = Mover(t, "Tarp");
            tarp.parent.localPosition = new Vector3(0.10f, 0.42f, 0.16f);
            var flap = Idle(tarp, AmbientMode.Sway, 21f, 1.05f, Vector3.forward);
            Box(flap, "Sheet", M("Sand"), new Vector3(-0.22f, 0f, 0f), new Vector3(0.72f, 0.02f, 0.96f),
                new Vector3(0f, 0f, 9f));
            Box(flap, "Seam", M("WoodMid"), new Vector3(-0.22f, 0.014f, 0f), new Vector3(0.70f, 0.008f, 0.06f),
                new Vector3(0f, 0f, 9f));

            var corner = Mover(t, "TarpCorner");
            var whip = Idle(corner, AmbientMode.Sway, 34f, 1.4f, Vector3.right);
            Box(whip, "LooseCorner", M("Sand"), new Vector3(-0.34f, 0.30f, -0.44f),
                new Vector3(0.34f, 0.02f, 0.30f), new Vector3(0f, 0f, 22f));

            var taut = Hidden(Mover(t, "TautTarp"));
            Box(taut, "Sheet", M("Sand"), new Vector3(-0.12f, 0.44f, 0.16f), new Vector3(0.78f, 0.02f, 1.04f));
            Box(taut, "Seam", M("WoodMid"), new Vector3(-0.12f, 0.455f, 0.16f), new Vector3(0.76f, 0.008f, 0.06f));
            foreach (var z in new[] { -0.28f, 0.60f })
            {
                Box(taut, "Guy", M("Earth"), new Vector3(0.16f, 0.30f, z), new Vector3(0.012f, 0.30f, 0.012f),
                    new Vector3(0f, 0f, 14f));
            }

            var pinned = Hidden(Mover(t, "PinnedCorner"));
            Box(pinned, "Corner", M("Sand"), new Vector3(-0.34f, 0.14f, -0.44f), new Vector3(0.34f, 0.02f, 0.30f));

            Worlds.Peps(t, new Vector3(-0.02f, 0.07f, -0.72f), new Vector3(0.06f, 0.07f, 1.08f),
                new Vector3(0.02f, 0.07f, 0.86f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.07f, -0.90f), new Vector3(0.44f, 0.07f, -0.90f),
                new Vector3(0f, 0.07f, -1.16f));
            Worlds.Finish(root, Worlds.Storm, _dir);
        }

        /// <summary>
        /// Movers: Mast, Arc/Strike, LiveGrid/SafeGrid, GroundPulseNear/Mid/Far,
        /// Relay, ServiceBridgeLocked/Open, SignalBeacons and Scorch.
        /// </summary>
        private static void StormMast()
        {
            var root = Worlds.Begin(Worlds.Storm, "Mast");
            var t = root.transform;

            // Three roof zones and one live service trench make this a system
            // problem rather than a second local object placement.
            Box(t, "NearDeck", M("Violet"), new Vector3(-0.36f, 0.10f, -0.78f),
                new Vector3(0.68f, 0.09f, 0.78f));
            Box(t, "FarDeck", M("Stone"), new Vector3(0.34f, 0.12f, 0.90f),
                new Vector3(0.72f, 0.12f, 0.84f));
            Box(t, "ServiceSpine", M("StoneLight"), new Vector3(0.02f, 0.085f, 0.10f),
                new Vector3(0.50f, 0.03f, 2.14f));
            foreach (var z in new[] { -0.62f, -0.12f, 0.38f, 0.88f })
            {
                Box(t, "Tread", M("Stone"), new Vector3(0.02f, 0.106f, z),
                    new Vector3(0.48f, 0.018f, 0.07f));
            }

            var mast = Mover(t, "Mast");
            Box(mast, "Pole", M("Stone"), new Vector3(0.48f, 0.70f, 0.88f),
                new Vector3(0.06f, 1.28f, 0.06f));
            foreach (var y in new[] { 0.86f, 1.06f })
            {
                Box(mast, "Crossarm", M("StoneLight"), new Vector3(0.48f, y, 0.88f),
                    new Vector3(0.34f, 0.025f, 0.025f));
            }
            Ball(mast, "Finial", M("Stone"), new Vector3(0.48f, 1.34f, 0.88f), Vector3.one * 0.07f);

            var liveGrid = Mover(t, "LiveGrid");
            Box(liveGrid, "LiveTrench", M("Abyss"), new Vector3(0.02f, 0.115f, 0.10f),
                new Vector3(0.44f, 0.035f, 1.02f));
            foreach (var (x, z, tilt) in new[]
                     {
                         (-0.14f, -0.22f, 18f), (0.12f, 0.02f, -20f), (-0.10f, 0.32f, 14f),
                     })
            {
                Box(liveGrid, "LiveArc", M("Candle"), new Vector3(x, 0.16f, z),
                    new Vector3(0.035f, 0.035f, 0.24f), new Vector3(0f, tilt, 0f));
            }

            var safeGrid = Hidden(Mover(t, "SafeGrid"));
            Box(safeGrid, "GroundedDeck", M("StoneLight"), new Vector3(0.02f, 0.13f, 0.10f),
                new Vector3(0.56f, 0.055f, 1.12f));
            for (var i = 0; i < 5; i++)
                Box(safeGrid, "GroundedTread", i % 2 == 0 ? M("WaterBright") : M("Stone"),
                    new Vector3(0.02f, 0.162f, -0.34f + i * 0.22f), new Vector3(0.50f, 0.014f, 0.055f));

            var lockedBridge = Mover(t, "ServiceBridgeLocked");
            foreach (var x in new[] { -0.22f, 0.24f })
                Box(lockedBridge, "Barrier", M("Accent"), new Vector3(x, 0.35f, 0.08f),
                    new Vector3(0.055f, 0.42f, 0.055f));
            Box(lockedBridge, "BarrierTop", M("AccentLight"), new Vector3(0.01f, 0.55f, 0.08f),
                new Vector3(0.52f, 0.055f, 0.055f));

            var openBridge = Hidden(Mover(t, "ServiceBridgeOpen"));
            Box(openBridge, "BridgeDeck", M("StoneLight"), new Vector3(0.01f, 0.15f, 0.08f),
                new Vector3(0.62f, 0.07f, 0.58f));
            foreach (var x in new[] { -0.29f, 0.31f })
                Box(openBridge, "LowRail", M("AccentLight"), new Vector3(x, 0.25f, 0.08f),
                    new Vector3(0.035f, 0.20f, 0.56f));

            var relay = Mover(t, "Relay");
            relay.parent.localPosition = new Vector3(-0.46f, 0.30f, 0.58f);
            Box(relay, "RelayBox", M("Violet"), Vector3.zero, new Vector3(0.30f, 0.24f, 0.28f));
            Cog(relay, "RelayWheel", M("Accent"), M("AccentLight"), new Vector3(0f, 0.15f, -0.16f),
                0.17f, 8, 0.055f);

            var pulseNear = Hidden(Mover(t, "GroundPulseNear"));
            Beam(pulseNear, "Conduit", M("WaterBright"), new Vector3(0.44f, 1.10f, 0.88f),
                new Vector3(0.24f, 0.44f, 0.42f), 0.025f);
            var pulseMid = Hidden(Mover(t, "GroundPulseMid"));
            Beam(pulseMid, "Conduit", M("Candle"), new Vector3(0.24f, 0.44f, 0.42f),
                new Vector3(-0.18f, 0.18f, 0.08f), 0.028f);
            var pulseFar = Hidden(Mover(t, "GroundPulseFar"));
            Beam(pulseFar, "Conduit", M("WaterBright"), new Vector3(-0.18f, 0.18f, 0.08f),
                new Vector3(-0.46f, 0.30f, 0.58f), 0.025f);

            var beacons = Hidden(Mover(t, "SignalBeacons"));
            foreach (var (x, z) in new[] { (-0.50f, -0.78f), (0.48f, 0.88f), (0.50f, -0.72f) })
            {
                Rod(beacons, "BeaconPost", M("StoneLight"), new Vector3(x, 0.31f, z),
                    new Vector3(0.035f, 0.20f, 0.035f));
                Ball(beacons, "BeaconLamp", M("AccentLight"), new Vector3(x, 0.53f, z), Vector3.one * 0.075f);
            }

            // The bolt and its afterglow are hidden until the rod earns them.
            var arc = Hidden(Mover(t, "Arc"));
            foreach (var (x, y, tilt) in new[] { (0.28f, 1.78f, 14f), (0.42f, 1.46f, -18f), (0.48f, 1.14f, 10f) })
            {
                Box(arc, "Bolt", M("Candle"), new Vector3(x, y, 0.88f), new Vector3(0.055f, 0.36f, 0.055f),
                    new Vector3(0f, 0f, tilt));
            }

            var strike = Hidden(Mover(t, "Strike"));
            Ball(strike, "FlashCore", M("Candle"), new Vector3(0.48f, 1.32f, 0.88f),
                new Vector3(0.26f, 0.22f, 0.24f));
            foreach (var (x, y, tilt) in new[] { (0.16f, 1.36f, 34f), (0.78f, 1.28f, -30f), (0.48f, 0.94f, 8f) })
                Box(strike, "FlashRay", M("Candle"), new Vector3(x, y, 0.87f),
                    new Vector3(0.045f, 0.54f, 0.045f), new Vector3(0f, 0f, tilt));

            var scorch = Mover(t, "Scorch");
            foreach (var (x, z) in new[] { (-0.14f, 0.10f), (0.16f, 0.66f) })
            {
                Ball(scorch, "Mark", M("Abyss"), new Vector3(x, 0.09f, z), new Vector3(0.16f, 0.008f, 0.12f));
            }

            Worlds.Peps(t, new Vector3(-0.38f, 0.15f, -0.76f), new Vector3(0.34f, 0.19f, 0.90f),
                new Vector3(0.22f, 0.18f, 0.50f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.07f, -0.94f), new Vector3(0.44f, 0.07f, -0.86f),
                new Vector3(0f, 0.07f, -1.18f));
            Worlds.Finish(root, Worlds.Storm, _dir);
        }

        /// <summary>
        /// Movers: LockedRoofWorld/SpillwayWorld, StormTank, DrainWheel,
        /// SpillwayChains, FloodFront, TorrentField, StormDebris, WorldFlash,
        /// SafetyLights and LandingSpray.
        /// </summary>
        private static void StormGutter()
        {
            var root = Worlds.Begin(Worlds.Storm, "Gutter");
            var t = root.transform;

            // BEFORE: the upper cistern is locked behind tall folded storm
            // shutters. Three disconnected roof islands leave no downhill
            // route; the skyline is vertical and broken.
            var locked = Mover(t, "LockedRoofWorld");
            Box(locked, "UpperRoof", M("Violet"), new Vector3(-0.42f, 0.86f, 1.12f),
                new Vector3(0.82f, 0.18f, 0.78f));
            Box(locked, "MiddleRoof", M("Stone"), new Vector3(0.18f, 0.46f, 0.16f),
                new Vector3(0.72f, 0.14f, 0.66f));
            Box(locked, "LandingRoof", M("Violet"), new Vector3(0.40f, 0.14f, -0.90f),
                new Vector3(0.76f, 0.12f, 0.70f));
            Box(locked, "VoidFar", M("Abyss"), new Vector3(-0.12f, 0.13f, 0.66f),
                new Vector3(0.76f, 0.08f, 0.46f), new Vector3(0f, -10f, 0f));
            Box(locked, "VoidNear", M("Abyss"), new Vector3(0.22f, 0.12f, -0.38f),
                new Vector3(0.84f, 0.08f, 0.48f), new Vector3(0f, 12f, 0f));
            Box(locked, "FoldedFlumeFar", M("StoneLight"), new Vector3(-0.63f, 0.92f, 0.50f),
                new Vector3(0.16f, 1.06f, 0.66f), new Vector3(0f, 0f, -8f));
            Box(locked, "FoldedFlumeMid", M("Stone"), new Vector3(0.54f, 0.68f, -0.02f),
                new Vector3(0.18f, 0.86f, 0.72f), new Vector3(0f, 0f, 9f));
            Box(locked, "StormShutter", M("AccentDeep"), new Vector3(-0.12f, 1.20f, 1.34f),
                new Vector3(0.54f, 0.74f, 0.09f), new Vector3(0f, 0f, -12f));

            // AFTER: the locked vertical pieces become one broad diagonal
            // spillway spanning the entire roof. Opaque state twins make the
            // transformation atomic on mobile.
            var spillway = Hidden(Mover(t, "SpillwayWorld"));
            Box(spillway, "UpperFlume", M("WaterDeep"), new Vector3(-0.38f, 0.78f, 0.92f),
                new Vector3(0.66f, 0.10f, 0.94f), new Vector3(-24f, -10f, 0f));
            Box(spillway, "MiddleFlume", M("Water"), new Vector3(-0.04f, 0.47f, 0.12f),
                new Vector3(0.74f, 0.10f, 0.96f), new Vector3(-20f, -12f, 0f));
            Box(spillway, "LowerFlume", M("WaterDeep"), new Vector3(0.30f, 0.22f, -0.68f),
                new Vector3(0.84f, 0.10f, 0.98f), new Vector3(-14f, -12f, 0f));
            foreach (var (x, y, z, yaw) in new[]
                     {
                         (-0.69f, 0.88f, 0.91f, -10f), (-0.08f, 0.68f, 0.91f, -10f),
                         (-0.42f, 0.58f, 0.10f, -12f), (0.35f, 0.38f, 0.10f, -12f),
                         (-0.12f, 0.28f, -0.69f, -12f), (0.74f, 0.16f, -0.69f, -12f),
                     })
                Box(spillway, "FlumeRail", M("StoneLight"), new Vector3(x, y, z),
                    new Vector3(0.055f, 0.16f, 0.90f), new Vector3(-18f, yaw, 0f));
            Box(spillway, "LandingBasin", M("Water"), new Vector3(0.42f, 0.13f, -1.13f),
                new Vector3(1.00f, 0.12f, 0.54f));
            Box(spillway, "LandingLip", M("StoneLight"), new Vector3(0.42f, 0.23f, -1.36f),
                new Vector3(1.04f, 0.08f, 0.08f));

            var tank = Mover(t, "StormTank");
            tank.parent.localPosition = new Vector3(0.42f, 1.12f, 1.10f);
            Rod(tank, "Cistern", M("Stone"), Vector3.zero, new Vector3(0.36f, 0.44f, 0.36f),
                new Vector3(90f, 0f, 0f));
            Box(tank, "WaterGauge", M("WaterBright"), new Vector3(0f, 0.02f, -0.38f),
                new Vector3(0.46f, 0.10f, 0.035f));
            foreach (var x in new[] { -0.24f, 0.24f })
                Box(tank, "TankLeg", M("StoneLight"), new Vector3(x, -0.43f, 0f),
                    new Vector3(0.07f, 0.48f, 0.07f));

            var wheel = Mover(t, "DrainWheel");
            wheel.parent.localPosition = new Vector3(-0.16f, 0.42f, 0.74f);
            Cog(wheel, "Release", M("Accent"), M("AccentLight"), Vector3.zero, 0.22f, 9, 0.07f);

            var chains = Mover(t, "SpillwayChains");
            Beam(chains, "ChainLeft", M("Cream"), new Vector3(-0.40f, 0.68f, 0.70f),
                new Vector3(-0.62f, 1.20f, 0.52f), 0.018f);
            Beam(chains, "ChainRight", M("Cream"), new Vector3(0.28f, 0.56f, 0.05f),
                new Vector3(0.54f, 1.02f, -0.02f), 0.018f);

            var flood = Hidden(Mover(t, "FloodFront"));
            Box(flood, "WaterWall", M("WaterBright"), new Vector3(-0.18f, 0.78f, 0.82f),
                new Vector3(1.56f, 0.24f, 0.14f), new Vector3(0f, 0f, -7f));
            for (var i = 0; i < 6; i++)
                Ball(flood, "Breaker", M("WaterLight"), new Vector3(-0.76f + i * 0.30f, 0.94f, 0.80f),
                    new Vector3(0.24f, 0.13f, 0.15f));

            var torrent = Hidden(Mover(t, "TorrentField"));
            var runoff = Living(torrent, "Runoff", AmbientMode.Drift, -2.30f, 1.45f,
                new Vector3(0.36f, -0.35f, -1f), stagger: true, controlId: "RoofTorrent");
            for (var i = 0; i < 12; i++)
                Box(runoff, "TorrentStripe", i % 2 == 0 ? M("WaterLight") : M("WaterBright"),
                    new Vector3(-0.70f + (i % 4) * 0.45f, 0.22f + (i % 3) * 0.25f,
                        -1.10f + (i / 4) * 0.82f),
                    new Vector3(0.30f, 0.018f, 0.055f), new Vector3(-12f, -24f, 0f));

            var debris = Mover(t, "StormDebris");
            foreach (var (x, y, z, s) in new[]
                     {
                         (-0.62f, 1.42f, 0.48f, 0.13f), (0.54f, 1.14f, -0.02f, 0.10f),
                         (-0.08f, 1.52f, 1.32f, 0.12f),
                     })
                Box(debris, "LoosePanel", M("WoodMid"), new Vector3(x, y, z),
                    new Vector3(s * 1.8f, 0.035f, s), new Vector3(0f, y * 18f, 12f));

            var flash = Hidden(Mover(t, "WorldFlash"));
            foreach (var (x, y, z, tilt) in new[]
                     {
                         (-0.78f, 1.64f, 1.54f, 18f), (-0.58f, 1.22f, 1.50f, -22f),
                         (0.36f, 1.72f, 1.58f, -16f), (0.22f, 1.30f, 1.54f, 24f),
                     })
                Box(flash, "SkyBolt", M("Candle"), new Vector3(x, y, z),
                    new Vector3(0.055f, 0.52f, 0.055f), new Vector3(0f, 0f, tilt));
            Ball(flash, "ElectricCore", M("Candle"), new Vector3(-0.10f, 1.44f, 1.56f),
                new Vector3(0.30f, 0.22f, 0.16f));

            var lights = Hidden(Mover(t, "SafetyLights"));
            foreach (var (x, y, z) in new[]
                     {
                         (-0.58f, 0.98f, 1.00f), (-0.22f, 0.68f, 0.28f),
                         (0.18f, 0.40f, -0.38f), (0.58f, 0.25f, -1.02f),
                     })
                Ball(lights, "RouteLamp", M("AccentLight"), new Vector3(x, y, z), Vector3.one * 0.075f);

            var spray = Hidden(Mover(t, "LandingSpray"));
            foreach (var (x, y, z, s) in new[]
                     {
                         (0.05f, 0.24f, -1.02f, 0.26f), (0.38f, 0.34f, -1.16f, 0.34f),
                         (0.70f, 0.22f, -1.08f, 0.24f),
                     })
                Ball(spray, "Splash", M("WaterLight"), new Vector3(x, y, z),
                    new Vector3(s, s * 0.65f, s * 0.72f));

            Worlds.Peps(t, new Vector3(-0.43f, 1.02f, 1.08f), new Vector3(0.42f, 0.22f, -1.10f),
                new Vector3(0.34f, 0.22f, -1.02f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.07f, -0.88f), new Vector3(0.44f, 0.07f, -0.96f),
                new Vector3(0f, 0.07f, -1.20f));
            Worlds.Finish(root, Worlds.Storm, _dir);
        }
    }
}
