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

            Ball(t, "FrozenPatch", M("WaterLight"), new Vector3(0.30f, 0.710f, 1.22f),
                new Vector3(0.52f, 0.016f, 0.46f));

            var shell = Mover(t, "IceShell");
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
            Ball(puddle, "Puddle", M("Water"), new Vector3(0.30f, 0.712f, 1.22f), new Vector3(0.50f, 0.018f, 0.42f));

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

        /// <summary>Movers: Plant.</summary>
        private static void WeatherBloom()
        {
            Worlds.WeatherTint = "sun";
            var root = Worlds.Begin(Worlds.Weather, "Bloom");
            var t = root.transform;

            Rod(t, "FlowerPot", M("Clay"), new Vector3(0.34f, 0.465f, 0.30f), new Vector3(0.22f, 0.09f, 0.22f));
            Rod(t, "PotRim", M("EarthLight"), new Vector3(0.34f, 0.515f, 0.30f), new Vector3(0.245f, 0.022f, 0.245f));
            Box(t, "TerraceEdge", M("EarthLight"), new Vector3(-0.02f, 0.56f, 1.15f),
                new Vector3(0.06f, 0.28f, 1.08f));

            // The plant's container sits inside the pot rim, so a Resize step
            // grows it upward out of the pot cleanly.
            var plant = Mover(t, "Plant");
            plant.parent.localPosition = new Vector3(0.34f, 0.43f, 0.30f);
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

            // Small floral garden dressing along the middle terrace
            Ball(t, "DecoFlowerA", M("AccentLight"), new Vector3(0.12f, 0.435f, -0.22f), Vector3.one * 0.06f);
            Ball(t, "DecoFlowerB", M("Accent"), new Vector3(0.52f, 0.435f, -0.15f), Vector3.one * 0.055f);
            Ball(t, "DecoFlowerC", M("WaterBright"), new Vector3(0.48f, 0.435f, 0.58f), Vector3.one * 0.065f);

            Worlds.Peps(t, new Vector3(-0.34f, 0.70f, 0.78f), new Vector3(0.34f, 0.58f, 0.30f),
                new Vector3(-0.06f, 0.70f, 0.72f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.15f, -1.44f), new Vector3(0.46f, 0.15f, -1.44f),
                new Vector3(0f, 0.15f, -1.06f));
            Worlds.Finish(root, Worlds.Weather, _dir);
        }

        /// <summary>Movers: Cloud, Rain, Awning, FloodStream, Rainbow, SunGlow.</summary>
        private static void WeatherDownpour()
        {
            Worlds.WeatherTint = "rain";
            var root = Worlds.Begin(Worlds.Weather, "Downpour");
            var t = root.transform;

            // Flooded torrent channel separating the two sides of the terrace
            var flood = Mover(t, "FloodStream");
            flood.parent.localPosition = new Vector3(-0.02f, 0.428f, 0.02f);
            Box(flood, "TorrentWater", M("Water"), Vector3.zero, new Vector3(0.44f, 0.014f, 1.12f));

            // Cobblestone walkway stepping stones on the dry left side
            Box(t, "PathStone1", M("StoneLight"), new Vector3(-0.38f, 0.427f, 0.18f),
                new Vector3(0.24f, 0.012f, 0.32f));
            Box(t, "PathStone2", M("StoneLight"), new Vector3(-0.38f, 0.427f, -0.18f),
                new Vector3(0.24f, 0.012f, 0.32f));

            // Sluice gate / gutter mechanism
            Box(t, "SluiceGate", M("WoodDark"), new Vector3(-0.23f, 0.48f, 0.48f),
                new Vector3(0.06f, 0.12f, 0.22f));

            // Weather vane on the lower terrace spinning rapidly in the storm
            var vane = Living(t, "StormVane", AmbientMode.Spin, 240f, 0.05f, Vector3.up);
            vane.localPosition = new Vector3(0.50f, 0.24f, -1.15f);
            Rod(vane, "VanePost", M("Stone"), new Vector3(0f, 0f, 0f), new Vector3(0.02f, 0.18f, 0.02f));
            Box(vane, "VaneFinA", M("Accent"), new Vector3(0.06f, 0.06f, 0f), new Vector3(0.12f, 0.04f, 0.01f));
            Box(vane, "VaneFinB", M("Accent"), new Vector3(-0.06f, 0.06f, 0f), new Vector3(0.08f, 0.02f, 0.01f));

            var awning = Mover(t, "Awning");
            foreach (var x in new[] { -0.58f, -0.20f })
            {
                Box(awning, "AwningPost", M("Wood"), new Vector3(x, 0.67f, -0.30f),
                    new Vector3(0.045f, 0.50f, 0.045f));
            }

            Box(awning, "Canopy", M("Accent"), new Vector3(-0.39f, 0.94f, -0.30f),
                new Vector3(0.48f, 0.065f, 0.46f), new Vector3(-8f, 0f, 0f));

            // Dark storm cloud system
            var cloud = Mover(t, "Cloud");
            cloud.parent.localPosition = new Vector3(0.30f, 1.18f, 0.30f);
            var drift = Idle(cloud, AmbientMode.Bob, 0.025f, 0.22f, Vector3.up);
            foreach (var (x, y, s) in new[]
                     {
                         (-0.22f, -0.02f, 0.20f), (-0.08f, 0.05f, 0.28f), (0.10f, 0.07f, 0.30f),
                         (0.24f, -0.01f, 0.22f), (0f, -0.05f, 0.36f)
                     })
            {
                Ball(drift, "CloudPuff", M("Stone"), new Vector3(x, y, 0f),
                    new Vector3(s, s * 0.70f, s * 0.78f));
            }

            Box(drift, "CloudBase", M("Ink"), new Vector3(0.01f, -0.065f, 0f), new Vector3(0.55f, 0.08f, 0.26f));

            // Torrential rain streaks
            var rain = Mover(t, "Rain");
            rain.parent.localPosition = new Vector3(0.34f, 0.45f, 0.30f);
            var fall = Living(rain, "Fall", AmbientMode.Drift, -0.52f, 1.25f, Vector3.up, stagger: true);
            for (var i = 0; i < 9; i++)
            {
                Box(fall, "RainDrop", M("WaterBright"),
                    new Vector3(-0.25f + i * 0.062f, 0.36f + (i % 3) * 0.12f, ((i % 2) - 0.5f) * 0.08f),
                    new Vector3(0.018f, 0.18f, 0.018f), new Vector3(0f, 0f, -11f));
            }

            foreach (var (x, z, s) in new[] { (0.33f, 0.26f, 0.28f), (0.49f, 0.05f, 0.16f) })
            {
                Ball(t, "Puddle", M("Water"), new Vector3(x, 0.428f, z), new Vector3(s, 0.014f, s * 0.68f));
            }

            // Rainbow arch (hidden initially, revealed during climax transformation)
            var rainbow = Hidden(Mover(t, "Rainbow"));
            rainbow.parent.localPosition = new Vector3(0f, 1.25f, 0.90f);
            var colors = new[] { M("Accent"), M("AccentLight"), M("WaterBright"), M("Violet") };
            for (var c = 0; c < 4; c++)
            {
                var radius = 0.58f + c * 0.045f;
                for (var seg = 0; seg < 9; seg++)
                {
                    var angle = (seg / 8f) * Mathf.PI; // 0 to 180 degrees arch
                    var x = -Mathf.Cos(angle) * radius;
                    var y = Mathf.Sin(angle) * (radius * 0.75f);
                    Ball(rainbow, $"RainbowSeg_{c}_{seg}", colors[c], new Vector3(x, y, c * -0.015f),
                        new Vector3(0.12f, 0.040f, 0.035f), new Vector3(0f, 0f, (angle * Mathf.Rad2Deg) - 90f));
                }
            }

            // Sun glow disk behind rainbow
            var sunglow = Hidden(Mover(t, "SunGlow"));
            sunglow.parent.localPosition = new Vector3(0f, 1.40f, 0.95f);
            Ball(sunglow, "GlowCore", M("AccentPale"), Vector3.zero, new Vector3(0.48f, 0.48f, 0.05f));

            Worlds.Peps(t, new Vector3(-0.39f, 0.42f, -0.34f), new Vector3(0.34f, 0.42f, 0.30f),
                new Vector3(-0.10f, 0.42f, -0.16f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.15f, -1.44f), new Vector3(0.46f, 0.15f, -1.44f),
                new Vector3(0f, 0.15f, -1.06f));
            Worlds.Finish(root, Worlds.Weather, _dir);
        }

        // ===================================================================
        // World 4 — Windrock canyon. The gap is vertical too, and the air moves.
        // ===================================================================

        /// <summary>Movers: Thermal, RimGrass.</summary>
        private static void CanyonUpdraft()
        {
            var root = Worlds.Begin(Worlds.Canyon, "Updraft");
            var t = root.transform;

            // The updraft is drawn, not implied. Three pale columns rising out
            // of the chasm are the difference between "there is a gap" and
            // "there is a gap and the air is going up".
            var thermal = Mover(t, "Thermal");
            var rise = Living(thermal, "Rise", AmbientMode.Drift, 1.05f, 0.34f, Vector3.up, stagger: true);
            foreach (var (x, phase) in new[] { (-0.34f, 0f), (0.02f, 0.3f), (0.36f, 0.6f) })
            {
                Box(rise, "Column", M("AccentPale"), new Vector3(x, -0.42f + phase, 0f),
                    new Vector3(0.10f, 0.34f, 0.10f));
            }

            Box(t, "LaunchLedge", M("Clay"), new Vector3(0f, 0.15f, -0.50f), new Vector3(0.70f, 0.06f, 0.22f));
            foreach (var x in new[] { -0.30f, 0.30f })
            {
                Box(t, "LandingPost", M("WoodDark"), new Vector3(x, 0.50f, 0.62f),
                    new Vector3(0.05f, 0.16f, 0.05f));
            }

            Box(t, "LandingRail", M("Wood"), new Vector3(0f, 0.57f, 0.62f), new Vector3(0.66f, 0.035f, 0.05f));

            Worlds.Peps(t, new Vector3(0f, 0.18f, -0.66f), new Vector3(0f, 0.42f, 0.86f),
                new Vector3(0f, 0.42f, 0.74f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.18f, -1.32f), new Vector3(0.46f, 0.18f, -1.44f),
                new Vector3(0f, 0.18f, -1.06f));
            Worlds.Finish(root, Worlds.Canyon, _dir);
        }

        /// <summary>Movers: Basket, SpanCable.</summary>
        private static void CanyonCablecar()
        {
            var root = Worlds.Begin(Worlds.Canyon, "Cablecar");
            var t = root.transform;

            Box(t, "MastNear", M("WoodDark"), new Vector3(0f, 0.52f, -0.52f), new Vector3(0.09f, 0.70f, 0.09f));
            Box(t, "MastFar", M("WoodDark"), new Vector3(0f, 0.72f, 0.62f), new Vector3(0.09f, 0.62f, 0.09f));

            var cable = Mover(t, "SpanCable");
            Box(cable, "Line", M("Stone"), new Vector3(0f, 0.92f, 0.05f), new Vector3(0.022f, 0.022f, 1.20f),
                new Vector3(-6f, 0f, 0f));

            // The car hangs and swings on its own. Its rest pose is the
            // problem, which means the idle has to be big enough to read as
            // "you cannot step onto that".
            var basket = Mover(t, "Basket");
            basket.parent.localPosition = new Vector3(0f, 0.90f, 0.05f);
            var swing = Idle(basket, AmbientMode.Sway, 15f, 0.55f, Vector3.forward);
            Box(swing, "Hanger", M("Stone"), new Vector3(0f, -0.16f, 0f), new Vector3(0.02f, 0.32f, 0.02f));
            Box(swing, "Car", M("Wood"), new Vector3(0f, -0.40f, 0f), new Vector3(0.42f, 0.16f, 0.40f));
            Box(swing, "CarFloor", M("Sand"), new Vector3(0f, -0.33f, 0f), new Vector3(0.38f, 0.02f, 0.36f));
            foreach (var x in new[] { -0.20f, 0.20f })
            {
                Box(swing, "CarRail", M("WoodDark"), new Vector3(x, -0.30f, 0f), new Vector3(0.025f, 0.16f, 0.38f));
            }

            // Choreography composes *onto* an idle and cannot switch one off,
            // so "it stops swinging" is a still twin revealed in place. Same
            // pattern as the reflected beam: the state change is a swap.
            var steady = Hidden(Mover(t, "SteadyCar"));
            Box(steady, "Hanger", M("Stone"), new Vector3(0f, 0.74f, 0.05f), new Vector3(0.02f, 0.32f, 0.02f));
            Box(steady, "Car", M("Wood"), new Vector3(0f, 0.50f, 0.05f), new Vector3(0.42f, 0.16f, 0.40f));
            Box(steady, "CarFloor", M("Sand"), new Vector3(0f, 0.57f, 0.05f), new Vector3(0.38f, 0.02f, 0.36f));
            foreach (var x in new[] { -0.20f, 0.20f })
            {
                Box(steady, "CarRail", M("WoodDark"), new Vector3(x, 0.60f, 0.05f),
                    new Vector3(0.025f, 0.16f, 0.38f));
            }

            Worlds.Peps(t, new Vector3(0.02f, 0.18f, -0.70f), new Vector3(-0.05f, 0.42f, 0.92f),
                new Vector3(-0.02f, 0.42f, 0.80f));
            Worlds.Slots(t, new Vector3(-0.46f, 0.18f, -1.26f), new Vector3(0.46f, 0.18f, -1.40f),
                new Vector3(0f, 0.18f, -1.04f));
            Worlds.Finish(root, Worlds.Canyon, _dir);
        }

        /// <summary>Movers: Spire, SpireDust (hidden).</summary>
        private static void CanyonSpire()
        {
            var root = Worlds.Begin(Worlds.Canyon, "Spire");
            var t = root.transform;

            // A finger of rock standing in the chasm, taller than both rims.
            // Nothing else in the game is asking to be pulled over.
            var spire = Mover(t, "Spire");
            spire.parent.localPosition = new Vector3(0.02f, -0.40f, 0.02f);
            Box(spire, "Shaft", M("Clay"), new Vector3(0f, 0.82f, 0f), new Vector3(0.28f, 1.64f, 0.26f),
                new Vector3(-3f, 0f, 2f));
            Box(spire, "ShaftDark", M("EarthDark"), new Vector3(-0.11f, 0.82f, 0f),
                new Vector3(0.08f, 1.60f, 0.27f), new Vector3(-3f, 0f, 2f));
            Box(spire, "Cap", M("Sand"), new Vector3(0.01f, 1.68f, 0.01f), new Vector3(0.34f, 0.12f, 0.32f));
            foreach (var (y, s) in new[] { (0.34f, 0.36f), (0.92f, 0.32f), (1.42f, 0.30f) })
            {
                Box(spire, "Collar", M("Earth"), new Vector3(0f, y, 0f), new Vector3(s, 0.07f, s * 0.9f));
            }

            var dust = Hidden(Mover(t, "SpireDust"));
            foreach (var (x, z, s) in new[] { (-0.40f, 0.10f, 0.30f), (0.34f, -0.16f, 0.24f), (0.02f, 0.34f, 0.34f) })
            {
                Ball(dust, "Plume", M("Sand"), new Vector3(x, 0.10f, z), new Vector3(s, s * 0.55f, s * 0.8f));
            }

            var span = Hidden(Mover(t, "FallenSpan"));
            Box(span, "Column", M("Clay"), new Vector3(0.02f, 0.24f, 0.04f), new Vector3(0.26f, 0.22f, 1.36f),
                new Vector3(6f, 0f, 2f));
            Box(span, "ColumnTop", M("EarthLight"), new Vector3(0.02f, 0.345f, 0.04f),
                new Vector3(0.22f, 0.03f, 1.30f), new Vector3(6f, 0f, 2f));
            foreach (var z in new[] { -0.44f, 0.50f })
            {
                Box(span, "Collar", M("EarthDark"), new Vector3(0.02f, 0.25f, z),
                    new Vector3(0.30f, 0.26f, 0.06f), new Vector3(6f, 0f, 2f));
            }

            Worlds.Peps(t, new Vector3(-0.30f, 0.18f, -0.70f), new Vector3(0.28f, 0.42f, 0.92f),
                new Vector3(0.12f, 0.42f, 0.78f));
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

        /// <summary>Movers: Raft, Mooring.</summary>
        private static void TideChannel()
        {
            var root = Worlds.Begin(Worlds.Tide, "Channel");
            var t = root.transform;

            Box(t, "NearDock", M("Wood"), new Vector3(-0.46f, 0.26f, -0.50f), new Vector3(0.66f, 0.06f, 1.00f));
            Box(t, "FarDock", M("Wood"), new Vector3(0.34f, 0.26f, 1.22f), new Vector3(0.82f, 0.06f, 0.92f));
            foreach (var (x, z) in new[] { (-0.72f, -0.88f), (-0.20f, -0.88f), (0.02f, 1.55f), (0.66f, 1.55f) })
            {
                Rod(t, "Piling", M("WoodDark"), new Vector3(x, 0.13f, z), new Vector3(0.075f, 0.22f, 0.075f));
            }

            var raft = Mover(t, "Raft");
            raft.parent.localPosition = new Vector3(-0.02f, 0.07f, 0.15f);
            var bob = Idle(raft, AmbientMode.Bob, 0.012f, 0.40f, Vector3.up);
            for (var i = -2; i <= 2; i++)
            {
                Box(bob, "Log", i % 2 == 0 ? M("Wood") : M("WoodMid"), new Vector3(i * 0.11f, 0.03f, 0f),
                    new Vector3(0.10f, 0.06f, 0.64f));
            }

            Box(bob, "Lashing", M("Earth"), new Vector3(0f, 0.065f, 0.22f), new Vector3(0.56f, 0.014f, 0.014f));
            Box(bob, "Post", M("WoodDark"), new Vector3(0.20f, 0.14f, -0.22f), new Vector3(0.035f, 0.18f, 0.035f));

            var mooring = Mover(t, "Mooring");
            Box(mooring, "Line", M("Cream"), new Vector3(-0.22f, 0.16f, -0.16f),
                new Vector3(0.40f, 0.012f, 0.012f), new Vector3(0f, 34f, -12f));

            Worlds.Peps(t, new Vector3(-0.46f, 0.29f, -0.55f), new Vector3(0.34f, 0.29f, 1.22f),
                new Vector3(0.20f, 0.29f, 1.06f));
            Worlds.Slots(t, new Vector3(-0.52f, 0.095f, -1.58f), new Vector3(0.54f, 0.095f, -1.48f),
                new Vector3(0.02f, 0.095f, -1.28f));
            Worlds.Finish(root, Worlds.Tide, _dir);
        }

        /// <summary>Movers: Current, MooringPost, Swing (hidden).</summary>
        private static void TideCurrent()
        {
            var root = Worlds.Begin(Worlds.Tide, "Current");
            var t = root.transform;

            Box(t, "LeftDock", M("Wood"), new Vector3(-0.58f, 0.26f, 0.15f), new Vector3(0.52f, 0.06f, 1.40f));
            Box(t, "RightDock", M("Wood"), new Vector3(0.60f, 0.26f, 0.38f), new Vector3(0.52f, 0.06f, 1.40f));
            foreach (var (x, z) in new[] { (-0.76f, -0.42f), (-0.76f, 0.72f), (0.78f, -0.20f), (0.78f, 0.94f) })
            {
                Rod(t, "Piling", M("WoodDark"), new Vector3(x, 0.13f, z), new Vector3(0.075f, 0.22f, 0.075f));
            }

            // A channel of moving water between the two docks, drawn as
            // chevrons so its direction is unmistakable before anything moves.
            var current = Mover(t, "Current");
            var flow = Living(current, "Flow", AmbientMode.Drift, 1.30f, 0.45f, Vector3.right, stagger: true);
            for (var i = 0; i < 6; i++)
            {
                Box(flow, "Chevron", M("WaterBright"),
                    new Vector3(-0.62f + (i % 3) * 0.22f, 0.075f, -0.10f + (i / 3) * 0.42f),
                    new Vector3(0.20f, 0.012f, 0.05f), new Vector3(0f, 26f, 0f));
            }

            var post = Mover(t, "MooringPost");
            Rod(post, "Post", M("WoodDark"), new Vector3(-0.30f, 0.22f, -0.70f), new Vector3(0.08f, 0.22f, 0.08f));
            Rod(post, "Cap", M("Wood"), new Vector3(-0.30f, 0.34f, -0.70f), new Vector3(0.11f, 0.02f, 0.11f));

            var swing = Hidden(Mover(t, "Swing"));
            Box(swing, "Line", M("Cream"), new Vector3(-0.02f, 0.24f, -0.28f),
                new Vector3(0.90f, 0.012f, 0.012f), new Vector3(0f, 42f, -8f));

            Worlds.Peps(t, new Vector3(-0.58f, 0.29f, 0.10f), new Vector3(0.60f, 0.29f, 0.40f),
                new Vector3(0.34f, 0.29f, 0.34f));
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

        /// <summary>Movers: Mast, Arc (hidden), Strike (hidden), Scorch.</summary>
        private static void StormMast()
        {
            var root = Worlds.Begin(Worlds.Storm, "Mast");
            var t = root.transform;

            Box(t, "Walkway", M("StoneLight"), new Vector3(0.02f, 0.075f, 0.24f), new Vector3(0.46f, 0.02f, 1.80f));
            foreach (var z in new[] { -0.40f, 0.30f, 1.00f })
            {
                Box(t, "Tread", M("Stone"), new Vector3(0.02f, 0.088f, z), new Vector3(0.44f, 0.012f, 0.07f));
            }

            var mast = Mover(t, "Mast");
            Box(mast, "Pole", M("Stone"), new Vector3(0.44f, 0.66f, 0.86f), new Vector3(0.05f, 1.18f, 0.05f));
            foreach (var y in new[] { 0.86f, 1.06f })
            {
                Box(mast, "Crossarm", M("StoneLight"), new Vector3(0.44f, y, 0.86f),
                    new Vector3(0.34f, 0.025f, 0.025f));
            }

            Ball(mast, "Finial", M("Stone"), new Vector3(0.44f, 1.26f, 0.86f), Vector3.one * 0.06f);

            // The bolt and its afterglow are hidden until the rod earns them.
            var arc = Hidden(Mover(t, "Arc"));
            foreach (var (x, y, tilt) in new[] { (0.30f, 1.62f, 14f), (0.40f, 1.30f, -18f), (0.44f, 1.04f, 10f) })
            {
                Box(arc, "Bolt", M("Candle"), new Vector3(x, y, 0.86f), new Vector3(0.05f, 0.34f, 0.05f),
                    new Vector3(0f, 0f, tilt));
            }

            var strike = Hidden(Mover(t, "Strike"));
            Ball(strike, "Flash", M("Candle"), new Vector3(0.20f, 0.60f, 0.60f), Vector3.one * 1.30f);

            var scorch = Mover(t, "Scorch");
            foreach (var (x, z) in new[] { (-0.14f, 0.10f), (0.16f, 0.66f) })
            {
                Ball(scorch, "Mark", M("Abyss"), new Vector3(x, 0.09f, z), new Vector3(0.16f, 0.008f, 0.12f));
            }

            Worlds.Peps(t, new Vector3(-0.24f, 0.07f, -0.58f), new Vector3(0.22f, 0.07f, 1.14f),
                new Vector3(-0.02f, 0.07f, 0.36f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.07f, -0.94f), new Vector3(0.44f, 0.07f, -0.86f),
                new Vector3(0f, 0.07f, -1.18f));
            Worlds.Finish(root, Worlds.Storm, _dir);
        }

        /// <summary>Movers: Gutter, Spray (hidden).</summary>
        private static void StormGutter()
        {
            var root = Worlds.Begin(Worlds.Storm, "Gutter");
            var t = root.transform;

            // A lower annex roof beyond the parapet, and a steep tiled gutter
            // running down to it. Everything here is about the drop.
            Box(t, "Annex", M("Ink"), new Vector3(0.08f, -0.72f, 1.92f), new Vector3(1.16f, 1.30f, 0.98f));
            Box(t, "AnnexRoof", M("Violet"), new Vector3(0.08f, -0.06f, 1.92f), new Vector3(1.22f, 0.10f, 1.04f));
            Box(t, "AnnexLip", M("Stone"), new Vector3(0.08f, 0.02f, 1.46f), new Vector3(1.22f, 0.07f, 0.06f));

            var gutter = Mover(t, "Gutter");
            Box(gutter, "Trough", M("Stone"), new Vector3(0.08f, 0.06f, 1.16f),
                new Vector3(0.34f, 0.05f, 0.88f), new Vector3(28f, 0f, 0f));
            foreach (var x in new[] { -0.09f, 0.25f })
            {
                Box(gutter, "TroughWall", M("StoneLight"), new Vector3(x, 0.11f, 1.16f),
                    new Vector3(0.04f, 0.10f, 0.88f), new Vector3(28f, 0f, 0f));
            }

            var spray = Hidden(Mover(t, "Spray"));
            foreach (var (x, y, z, s) in new[]
                     {
                         (-0.06f, 0.10f, 0.90f, 0.14f), (0.14f, 0.02f, 1.24f, 0.18f), (0.06f, -0.06f, 1.52f, 0.16f),
                     })
            {
                Ball(spray, "Splash", M("WaterBright"), new Vector3(x, y, z), new Vector3(s, s * 0.5f, s * 0.7f));
            }

            Worlds.Peps(t, new Vector3(-0.24f, 0.07f, -0.42f), new Vector3(0.14f, -0.01f, 1.96f),
                new Vector3(0.02f, -0.01f, 1.82f));
            Worlds.Slots(t, new Vector3(-0.44f, 0.07f, -0.88f), new Vector3(0.44f, 0.07f, -0.96f),
                new Vector3(0f, 0.07f, -1.20f));
            Worlds.Finish(root, Worlds.Storm, _dir);
        }
    }
}
