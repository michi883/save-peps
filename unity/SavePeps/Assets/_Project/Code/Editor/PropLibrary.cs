using System.Collections.Generic;
using UnityEngine;

using static SavePeps.EditorTools.Toy;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Every tappable object in the game.
    ///
    /// The first fourteen props were a general-purpose kit — a plank, a
    /// pillow, a bone — reused everywhere, and reuse is still the rule: the
    /// umbrella shelters in the weather round and glides in the canyon, and a
    /// prop that always means one thing teaches the player to stop reading the
    /// scene. But a bone in a space station and a watering can on the sea bed
    /// undercut the world more than the reuse saves, so each world also brings
    /// two or three objects that could only be there. That is why there are
    /// thirty-six.
    ///
    /// Two palette rules bind everything here. Rows 7 and 8 — coral and mint —
    /// belong to the Peps alone, so no prop wears them; that is what keeps the
    /// two characters the first thing the eye finds. Row 6, warm accent, means
    /// "pay attention", so it appears as a highlight and a heat glow, never as
    /// a body colour.
    /// </summary>
    internal static class PropLibrary
    {
        private static IReadOnlyDictionary<string, Material> _m;
        private static string _dir;

        private static Material Mat(string key) => _m[key];

        internal static void BuildAll(IReadOnlyDictionary<string, Material> materials, string propDir)
        {
            _m = materials;
            _dir = propDir;

            // The shared nine: they turn up across worlds, and each one is the
            // answer somewhere and a joke somewhere else.
            Plank();
            Balloon();
            Fan();
            Stone();
            Leaf();
            Umbrella();
            Rope();
            Bell();
            Pillow();
            Scissors();
            WateringCan();
            Bone();
            Mirror();
            HairDryer();

            // World vocabulary.
            Gear();
            Wrench();
            Magnet();
            Feather();
            Weight();
            Grapple();
            Bucket();
            Oar();
            Net();
            Buoy();
            Sandbag();
            LightningRod();
            Lantern();
            Pickaxe();
            ChimeCrystal();
            Sled();
            BubbleShell();
            GlowJelly();
            Thruster();
            Crate();
            ZipGrip();
            NeonTube();
        }

        private static Transform Begin(string id, Vector3 tapSize, float tapCentreY, out GameObject root)
        {
            root = NewProp(id, tapSize, tapCentreY);
            return root.transform.Find("Choreo/Visual");
        }

        private static GameObject End(GameObject root, string id) => SavePrefab(root, $"{_dir}/{id}.prefab");

        // -------------------------------------------------------------------
        // The shared kit
        // -------------------------------------------------------------------

        private static void Plank()
        {
            var v = Begin("plank", new Vector3(0.48f, 0.30f, 0.96f), 0.04f, out var root);
            // Long axis along Z so it bridges without needing to be rotated.
            Box(v, "Board", Mat("Wood"), new Vector3(0f, 0.035f, 0f), new Vector3(0.25f, 0.07f, 0.82f));
            // Raised grain and end bands keep the board from reading as an
            // anonymous brown cuboid at phone scale.
            foreach (var side in new[] { -1f, 1f })
            {
                Box(v, "Grain", Mat("WoodDark"), new Vector3(side * 0.065f, 0.073f, 0f),
                    new Vector3(0.018f, 0.012f, 0.62f));
            }

            foreach (var z in new[] { -0.32f, 0.32f })
            {
                Box(v, "EndBand", Mat("WoodDark"), new Vector3(0f, 0.076f, z), new Vector3(0.27f, 0.014f, 0.035f));
            }

            End(root, "plank");
        }

        private static void Balloon()
        {
            var v = Begin("balloon", new Vector3(0.36f, 0.46f, 0.36f), 0.17f, out var root);
            Ball(v, "Bulb", Mat("Accent"), new Vector3(0f, 0.24f, 0f), new Vector3(0.23f, 0.29f, 0.23f));
            Ball(v, "Highlight", Mat("AccentPale"), new Vector3(-0.065f, 0.32f, -0.105f),
                new Vector3(0.035f, 0.070f, 0.014f));
            Box(v, "Knot", Mat("Accent"), new Vector3(0f, 0.075f, 0f), Vector3.one * 0.03f);
            Box(v, "String", Mat("Ink"), new Vector3(0f, -0.015f, 0f), new Vector3(0.008f, 0.17f, 0.008f));
            End(root, "balloon");
        }

        private static void Fan()
        {
            var v = Begin("fan", new Vector3(0.52f, 0.52f, 0.38f), 0.20f, out var root);
            var model = Child(v, "Model");
            model.localScale = Vector3.one * 0.84f;

            Box(model, "Stand", Mat("Stone"), new Vector3(0f, 0.035f, 0f), new Vector3(0.24f, 0.07f, 0.15f));
            Box(model, "Neck", Mat("Stone"), new Vector3(0f, 0.12f, 0f), new Vector3(0.055f, 0.18f, 0.055f));
            BlockRing(model, "Housing", Mat("Stone"), new Vector3(0f, 0.28f, 0f),
                new Vector2(0.22f, 0.22f), 12, 0.035f, 0.035f);
            Rod(model, "Hub", Mat("Stone"), new Vector3(0f, 0.28f, -0.025f),
                new Vector3(0.065f, 0.035f, 0.065f), new Vector3(90f, 0f, 0f));

            // Blades live under their own transform so a Spin step can turn
            // them without moving the housing.
            var blades = Child(model, "Blades");
            blades.localPosition = new Vector3(0f, 0.28f, -0.035f);
            for (var i = 0; i < 4; i++)
            {
                var b = Primitive(PrimitiveType.Cube, $"Blade_{i}", blades, Mat("Cream"));
                b.transform.localRotation = Quaternion.Euler(0f, 0f, i * 90f + 18f);
                b.transform.localPosition = Quaternion.Euler(0f, 0f, i * 90f) * new Vector3(0f, 0.085f, 0f);
                b.transform.localScale = new Vector3(0.065f, 0.16f, 0.018f);
            }

            End(root, "fan");
        }

        /// <summary>
        /// Squat and heavy-looking on purpose: it has to read as something that
        /// would stay put, or the solution is not legible before the tap.
        /// </summary>
        private static void Stone()
        {
            var v = Begin("stone", new Vector3(0.38f, 0.34f, 0.38f), 0.09f, out var root);
            Ball(v, "Mass", Mat("Stone"), new Vector3(0f, 0.085f, 0f), new Vector3(0.24f, 0.17f, 0.21f));
            Ball(v, "Shoulder", Mat("StoneLight"), new Vector3(0.06f, 0.05f, -0.04f),
                new Vector3(0.13f, 0.10f, 0.12f));
            Box(v, "Crack_A", Mat("Ink"), new Vector3(-0.025f, 0.13f, -0.102f),
                new Vector3(0.012f, 0.09f, 0.009f), new Vector3(0f, 0f, -28f));
            Box(v, "Crack_B", Mat("Ink"), new Vector3(0.012f, 0.095f, -0.108f),
                new Vector3(0.012f, 0.055f, 0.009f), new Vector3(0f, 0f, 32f));
            End(root, "stone");
        }

        private static void Leaf()
        {
            var v = Begin("leaf", new Vector3(0.5f, 0.24f, 0.56f), 0.03f, out var root);
            Ball(v, "Blade", Mat("FoliageLight"), new Vector3(0f, 0.02f, 0f), new Vector3(0.30f, 0.035f, 0.42f));
            Box(v, "Stem", Mat("Foliage"), new Vector3(0f, 0.035f, -0.24f), new Vector3(0.018f, 0.012f, 0.14f));
            End(root, "leaf");
        }

        /// <summary>
        /// A wrong answer that is always *nearly* right, which is what makes it
        /// worth having in six lineups. It shelters, it catches wind, it does
        /// everything except close a gap — and in vacuum it does nothing at all.
        /// </summary>
        private static void Umbrella()
        {
            var v = Begin("umbrella", new Vector3(0.42f, 0.52f, 0.42f), 0.20f, out var root);
            Ball(v, "Canopy", Mat("Accent"), new Vector3(0f, 0.30f, 0f), new Vector3(0.32f, 0.15f, 0.32f));
            Box(v, "Shaft", Mat("Ink"), new Vector3(0f, 0.15f, 0f), new Vector3(0.016f, 0.30f, 0.016f));
            Box(v, "Hook", Mat("Ink"), new Vector3(0.03f, 0.015f, 0f), new Vector3(0.06f, 0.016f, 0.016f));
            End(root, "umbrella");
        }

        /// <summary>
        /// Reads as "this is long" from a silhouette that is entirely compact,
        /// which is the whole trick — the player has to believe it spans the
        /// traverse before they tap it.
        /// </summary>
        private static void Rope()
        {
            var v = Begin("rope", new Vector3(0.36f, 0.26f, 0.36f), 0.06f, out var root);
            var radii = new[] { 0.22f, 0.17f, 0.12f };
            for (var i = 0; i < radii.Length; i++)
            {
                Rod(v, $"Coil_{i}", i % 2 == 0 ? Mat("Earth") : Mat("EarthLight"),
                    new Vector3(0f, 0.03f + i * 0.032f, 0f), new Vector3(radii[i], 0.016f, radii[i]));
            }

            End(root, "rope");
        }

        private static void Bell()
        {
            var v = Begin("bell", new Vector3(0.46f, 0.54f, 0.38f), 0.22f, out var root);
            Ball(v, "Dome", Mat("Accent"), new Vector3(0f, 0.27f, 0f), new Vector3(0.17f, 0.16f, 0.16f));
            for (var i = 0; i < 3; i++)
            {
                var radius = 0.16f + i * 0.035f;
                Rod(v, $"Skirt_{i}", Mat("Accent"), new Vector3(0f, 0.21f - i * 0.045f, 0f),
                    new Vector3(radius, 0.025f, radius));
            }

            Rod(v, "Rim", Mat("Ink"), new Vector3(0f, 0.105f, 0f), new Vector3(0.235f, 0.018f, 0.205f));
            Ball(v, "Clapper", Mat("Ink"), new Vector3(0f, 0.075f, -0.015f), Vector3.one * 0.065f);
            BlockRing(v, "Handle", Mat("Accent"), new Vector3(0f, 0.40f, 0f),
                new Vector2(0.08f, 0.075f), 8, 0.025f, 0.045f);
            End(root, "bell");
        }

        private static void Pillow()
        {
            var v = Begin("pillow", new Vector3(0.52f, 0.30f, 0.46f), 0.07f, out var root);

            // A sphere squashed into an oval read as an egg on the Pixel 4. A
            // rectangular core, four lobes and visible piping preserve the
            // pillow silhouette from this steep camera.
            Box(v, "Cushion", Mat("Cream"), new Vector3(0f, 0.065f, 0f), new Vector3(0.40f, 0.10f, 0.28f));
            foreach (var (x, z) in new[] { (-0.16f, -0.11f), (0.16f, -0.11f), (-0.16f, 0.11f), (0.16f, 0.11f) })
            {
                Ball(v, "Puff", Mat("Cream"), new Vector3(x, 0.085f, z), new Vector3(0.18f, 0.10f, 0.15f));
            }

            foreach (var z in new[] { -0.135f, 0.135f })
            {
                Box(v, "Piping_X", Mat("WaterDeep"), new Vector3(0f, 0.122f, z), new Vector3(0.36f, 0.012f, 0.014f));
            }

            foreach (var x in new[] { -0.195f, 0.195f })
            {
                Box(v, "Piping_Z", Mat("WaterDeep"), new Vector3(x, 0.122f, 0f), new Vector3(0.014f, 0.012f, 0.25f));
            }

            Ball(v, "Tuft", Mat("WaterDeep"), new Vector3(0f, 0.125f, -0.01f), new Vector3(0.035f, 0.018f, 0.035f));
            End(root, "pillow");
        }

        private static void Scissors()
        {
            var v = Begin("scissors", new Vector3(0.50f, 0.58f, 0.34f), 0.22f, out var root);
            BlockRing(v, "Handle_L", Mat("Violet"), new Vector3(-0.095f, 0.085f, 0f),
                new Vector2(0.075f, 0.09f), 8, 0.028f, 0.038f);
            BlockRing(v, "Handle_R", Mat("Violet"), new Vector3(0.095f, 0.085f, 0f),
                new Vector2(0.075f, 0.09f), 8, 0.028f, 0.038f);

            foreach (var side in new[] { -1f, 1f })
            {
                Box(v, "Blade", Mat("Cream"), new Vector3(side * 0.065f, 0.30f, 0f),
                    new Vector3(0.055f, 0.31f, 0.035f), new Vector3(0f, 0f, side * 13f));
            }

            Rod(v, "Pivot", Mat("Ink"), new Vector3(0f, 0.18f, -0.025f),
                new Vector3(0.045f, 0.025f, 0.045f), new Vector3(90f, 0f, 0f));
            End(root, "scissors");
        }

        private static void WateringCan()
        {
            var v = Begin("watering_can", new Vector3(0.58f, 0.50f, 0.40f), 0.18f, out var root);
            Rod(v, "Can", Mat("Water"), new Vector3(-0.04f, 0.16f, 0f), new Vector3(0.18f, 0.17f, 0.18f));
            BlockRing(v, "Handle", Mat("Cream"), new Vector3(-0.04f, 0.28f, 0.035f),
                new Vector2(0.20f, 0.17f), 10, 0.028f, 0.035f);
            Box(v, "Spout", Mat("Water"), new Vector3(0.22f, 0.20f, 0f), new Vector3(0.30f, 0.07f, 0.08f),
                new Vector3(0f, 0f, -24f));
            Rod(v, "Rose", Mat("Cream"), new Vector3(0.36f, 0.265f, 0f), new Vector3(0.09f, 0.025f, 0.09f),
                new Vector3(0f, 0f, -24f));
            End(root, "watering_can");
        }

        private static void Bone()
        {
            var v = Begin("bone", new Vector3(0.58f, 0.30f, 0.36f), 0.09f, out var root);
            Rod(v, "Shaft", Mat("Cream"), new Vector3(0f, 0.10f, 0f), new Vector3(0.065f, 0.19f, 0.065f),
                new Vector3(0f, 0f, 90f));
            foreach (var x in new[] { -0.20f, 0.20f })
            foreach (var y in new[] { 0.055f, 0.145f })
            {
                Ball(v, "Knob", Mat("Cream"), new Vector3(x, y, 0f), Vector3.one * 0.105f);
            }

            Box(v, "Underside", Mat("Sand"), new Vector3(0f, 0.055f, 0.035f), new Vector3(0.28f, 0.018f, 0.035f));
            End(root, "bone");
        }

        private static void Mirror()
        {
            var v = Begin("mirror", new Vector3(0.50f, 0.62f, 0.36f), 0.25f, out var root);
            Box(v, "Handle", Mat("Stone"), new Vector3(0f, 0.10f, 0.025f), new Vector3(0.075f, 0.22f, 0.075f));
            Ball(v, "Frame", Mat("Stone"), new Vector3(0f, 0.34f, 0.025f), new Vector3(0.25f, 0.30f, 0.065f));
            Ball(v, "Glass", Mat("WaterBright"), new Vector3(0f, 0.34f, -0.018f), new Vector3(0.205f, 0.25f, 0.028f));
            Box(v, "Gleam", Mat("Cream"), new Vector3(-0.065f, 0.41f, -0.04f), new Vector3(0.022f, 0.13f, 0.012f),
                new Vector3(0f, 0f, -35f));
            End(root, "mirror");
        }

        private static void HairDryer()
        {
            var v = Begin("hair_dryer", new Vector3(0.58f, 0.56f, 0.38f), 0.22f, out var root);
            Rod(v, "Barrel", Mat("Clay"), new Vector3(0.02f, 0.31f, 0f), new Vector3(0.14f, 0.19f, 0.14f),
                new Vector3(0f, 0f, 90f));
            Rod(v, "Nozzle", Mat("Cream"), new Vector3(-0.22f, 0.31f, 0f), new Vector3(0.10f, 0.11f, 0.10f),
                new Vector3(0f, 0f, 90f));
            Rod(v, "NozzleMouth", Mat("Ink"), new Vector3(-0.335f, 0.31f, 0f), new Vector3(0.078f, 0.018f, 0.078f),
                new Vector3(0f, 0f, 90f));
            Rod(v, "RearVent", Mat("Ink"), new Vector3(0.215f, 0.31f, 0f), new Vector3(0.105f, 0.018f, 0.105f),
                new Vector3(0f, 0f, 90f));
            foreach (var angle in new[] { 0f, 45f, 90f, 135f })
            {
                Box(v, "VentSlot", Mat("Cream"), new Vector3(0.236f, 0.31f, -0.01f),
                    new Vector3(0.015f, 0.085f, 0.012f), new Vector3(angle, 90f, 0f));
            }

            Box(v, "Grip", Mat("Clay"), new Vector3(0.08f, 0.13f, 0f), new Vector3(0.10f, 0.27f, 0.11f),
                new Vector3(0f, 0f, -14f));
            Box(v, "HeatButton", Mat("AccentLight"), new Vector3(0.005f, 0.17f, -0.065f),
                new Vector3(0.035f, 0.055f, 0.025f), new Vector3(0f, 0f, -14f));

            // Three warm dashes make function visible before the player has
            // learned the silhouette.
            for (var i = 0; i < 3; i++)
            {
                Box(v, "WarmAir", Mat("AccentLight"),
                    new Vector3(-0.41f - i * 0.065f, 0.31f + (i - 1) * 0.045f, 0f),
                    new Vector3(0.055f, 0.016f, 0.018f));
            }

            End(root, "hair_dryer");
        }

        // -------------------------------------------------------------------
        // Clockwork courtyard
        // -------------------------------------------------------------------

        /// <summary>The missing tooth in a machine. Stands upright so the teeth read.</summary>
        private static void Gear()
        {
            var v = Begin("gear", new Vector3(0.40f, 0.44f, 0.28f), 0.20f, out var root);
            Cog(v, "Cog", Mat("Accent"), Mat("AccentDeep"), new Vector3(0f, 0.21f, 0f), 0.145f, 10, 0.06f);
            Rod(v, "Boss", Mat("AccentLight"), new Vector3(0f, 0.21f, -0.03f),
                new Vector3(0.08f, 0.02f, 0.08f), new Vector3(90f, 0f, 0f));
            Rod(v, "Bore", Mat("Ink"), new Vector3(0f, 0.21f, -0.04f),
                new Vector3(0.04f, 0.02f, 0.04f), new Vector3(90f, 0f, 0f));
            Box(v, "Rest", Mat("Stone"), new Vector3(0f, 0.02f, 0f), new Vector3(0.20f, 0.04f, 0.10f));
            End(root, "gear");
        }

        /// <summary>An open-jaw spanner. The C is the whole silhouette.</summary>
        private static void Wrench()
        {
            var v = Begin("wrench", new Vector3(0.34f, 0.56f, 0.28f), 0.22f, out var root);
            Box(v, "Shaft", Mat("StoneLight"), new Vector3(0f, 0.20f, 0f), new Vector3(0.055f, 0.34f, 0.045f));
            Box(v, "Head", Mat("StoneLight"), new Vector3(0f, 0.40f, 0f), new Vector3(0.17f, 0.09f, 0.05f));
            foreach (var x in new[] { -0.055f, 0.055f })
            {
                Box(v, "Jaw", Mat("StoneLight"), new Vector3(x, 0.47f, 0f), new Vector3(0.055f, 0.09f, 0.05f));
            }

            Box(v, "Grip", Mat("Violet"), new Vector3(0f, 0.12f, 0f), new Vector3(0.07f, 0.15f, 0.055f));
            End(root, "wrench");
        }

        /// <summary>A horseshoe magnet with painted tips — a toy-box classic.</summary>
        private static void Magnet()
        {
            var v = Begin("magnet", new Vector3(0.42f, 0.46f, 0.30f), 0.20f, out var root);
            foreach (var x in new[] { -0.105f, 0.105f })
            {
                Box(v, "Leg", Mat("StoneLight"), new Vector3(x, 0.20f, 0f), new Vector3(0.085f, 0.28f, 0.10f));
                Box(v, "Tip", Mat("AccentDeep"), new Vector3(x, 0.055f, 0f), new Vector3(0.09f, 0.10f, 0.105f));
            }

            Box(v, "Arch", Mat("StoneLight"), new Vector3(0f, 0.345f, 0f), new Vector3(0.29f, 0.085f, 0.10f));
            Box(v, "ArchCap", Mat("Stone"), new Vector3(0f, 0.39f, 0f), new Vector3(0.25f, 0.03f, 0.105f));
            End(root, "magnet");
        }

        /// <summary>Weightless, and the canyon's way of showing you the wind.</summary>
        private static void Feather()
        {
            var v = Begin("feather", new Vector3(0.34f, 0.40f, 0.30f), 0.15f, out var root);
            Box(v, "Quill", Mat("Cream"), new Vector3(0f, 0.19f, 0f), new Vector3(0.014f, 0.34f, 0.014f),
                new Vector3(0f, 0f, 12f));
            for (var i = 0; i < 5; i++)
            {
                var y = 0.14f + i * 0.065f;
                var w = 0.11f - i * 0.014f;
                Ball(v, "Vane", i % 2 == 0 ? Mat("Cream") : Mat("Sand"), new Vector3(-0.03f - i * 0.008f, y, 0f),
                    new Vector3(w, 0.035f, 0.012f), new Vector3(0f, 0f, 24f));
            }

            End(root, "feather");
        }

        /// <summary>
        /// A machined iron mass with a lifting ring. Deliberately not the
        /// stone: this one obviously came off a scale, and it hangs.
        /// </summary>
        private static void Weight()
        {
            var v = Begin("weight", new Vector3(0.34f, 0.42f, 0.34f), 0.14f, out var root);
            Box(v, "Body", Mat("Ink"), new Vector3(0f, 0.115f, 0f), new Vector3(0.19f, 0.19f, 0.17f));
            Box(v, "Shoulder", Mat("Ink"), new Vector3(0f, 0.215f, 0f), new Vector3(0.13f, 0.05f, 0.12f));
            Box(v, "Band", Mat("StoneLight"), new Vector3(0f, 0.09f, 0f), new Vector3(0.20f, 0.022f, 0.18f));
            BlockRing(v, "Ring", Mat("StoneLight"), new Vector3(0f, 0.30f, 0f),
                new Vector2(0.055f, 0.06f), 8, 0.02f, 0.03f);
            End(root, "weight");
        }

        /// <summary>Three flukes and a coil: the only thing here that grabs at a distance.</summary>
        private static void Grapple()
        {
            var v = Begin("grapple", new Vector3(0.40f, 0.50f, 0.36f), 0.18f, out var root);
            Box(v, "Shank", Mat("StoneLight"), new Vector3(0f, 0.26f, 0f), new Vector3(0.04f, 0.24f, 0.04f));
            for (var i = 0; i < 3; i++)
            {
                var a = i * Mathf.PI * 2f / 3f;
                Box(v, "Fluke", Mat("Stone"),
                    new Vector3(Mathf.Cos(a) * 0.09f, 0.40f, Mathf.Sin(a) * 0.09f),
                    new Vector3(0.035f, 0.15f, 0.035f),
                    new Vector3(Mathf.Sin(a) * 34f, 0f, -Mathf.Cos(a) * 34f));
            }

            foreach (var i in new[] { 0, 1 })
            {
                Rod(v, "Line", Mat("Earth"), new Vector3(0f, 0.035f + i * 0.03f, 0f),
                    new Vector3(0.17f - i * 0.04f, 0.014f, 0.17f - i * 0.04f));
            }

            End(root, "grapple");
        }

        // -------------------------------------------------------------------
        // Tidewater docks
        // -------------------------------------------------------------------

        private static void Bucket()
        {
            var v = Begin("bucket", new Vector3(0.40f, 0.42f, 0.40f), 0.13f, out var root);
            Rod(v, "Body", Mat("Wood"), new Vector3(0f, 0.11f, 0f), new Vector3(0.20f, 0.11f, 0.20f));
            Rod(v, "Base", Mat("WoodDark"), new Vector3(0f, 0.015f, 0f), new Vector3(0.165f, 0.015f, 0.165f));
            Rod(v, "Rim", Mat("WoodDark"), new Vector3(0f, 0.225f, 0f), new Vector3(0.215f, 0.016f, 0.215f));
            Rod(v, "Water", Mat("Water"), new Vector3(0f, 0.205f, 0f), new Vector3(0.185f, 0.010f, 0.185f));
            BlockRing(v, "Handle", Mat("StoneLight"), new Vector3(0f, 0.235f, 0f),
                new Vector2(0.115f, 0.10f), 8, 0.018f, 0.022f);
            End(root, "bucket");
        }

        private static void Oar()
        {
            var v = Begin("oar", new Vector3(0.34f, 0.30f, 0.88f), 0.06f, out var root);
            Rod(v, "Shaft", Mat("Wood"), new Vector3(0f, 0.06f, -0.06f), new Vector3(0.03f, 0.30f, 0.03f),
                new Vector3(90f, 0f, 0f));
            Ball(v, "Blade", Mat("Sand"), new Vector3(0f, 0.055f, 0.34f), new Vector3(0.16f, 0.028f, 0.30f));
            Box(v, "BladeSpine", Mat("WoodDark"), new Vector3(0f, 0.072f, 0.30f),
                new Vector3(0.018f, 0.012f, 0.22f));
            Box(v, "Grip", Mat("Violet"), new Vector3(0f, 0.06f, -0.34f), new Vector3(0.045f, 0.045f, 0.09f));
            End(root, "oar");
        }

        private static void Net()
        {
            var v = Begin("net", new Vector3(0.42f, 0.54f, 0.30f), 0.22f, out var root);
            Box(v, "Handle", Mat("Wood"), new Vector3(0f, 0.12f, 0f), new Vector3(0.035f, 0.24f, 0.035f));
            BlockRing(v, "Hoop", Mat("WoodDark"), new Vector3(0f, 0.36f, 0f),
                new Vector2(0.155f, 0.145f), 10, 0.026f, 0.03f);
            for (var i = -1; i <= 1; i++)
            {
                Box(v, "Mesh_V", Mat("Cream"), new Vector3(i * 0.075f, 0.35f, 0f),
                    new Vector3(0.008f, 0.24f, 0.008f));
                Box(v, "Mesh_H", Mat("Cream"), new Vector3(0f, 0.35f + i * 0.075f, 0f),
                    new Vector3(0.26f, 0.008f, 0.008f));
            }

            End(root, "net");
        }

        private static void Buoy()
        {
            var v = Begin("buoy", new Vector3(0.38f, 0.52f, 0.38f), 0.20f, out var root);
            Ball(v, "Float", Mat("AccentDeep"), new Vector3(0f, 0.15f, 0f), new Vector3(0.25f, 0.22f, 0.25f));
            Rod(v, "Waterline", Mat("Cream"), new Vector3(0f, 0.15f, 0f), new Vector3(0.26f, 0.014f, 0.26f));
            Box(v, "Mast", Mat("StoneLight"), new Vector3(0f, 0.33f, 0f), new Vector3(0.024f, 0.17f, 0.024f));
            Ball(v, "Lamp", Mat("AccentLight"), new Vector3(0f, 0.43f, 0f), Vector3.one * 0.065f);
            Rod(v, "Tail", Mat("Earth"), new Vector3(0f, 0.022f, 0.09f), new Vector3(0.13f, 0.012f, 0.13f));
            End(root, "buoy");
        }

        // -------------------------------------------------------------------
        // Storm rooftop
        // -------------------------------------------------------------------

        private static void Sandbag()
        {
            var v = Begin("sandbag", new Vector3(0.48f, 0.30f, 0.38f), 0.08f, out var root);
            Ball(v, "Body", Mat("Sand"), new Vector3(0f, 0.085f, 0f), new Vector3(0.34f, 0.15f, 0.24f));
            Ball(v, "Slump", Mat("WoodMid"), new Vector3(0.04f, 0.055f, 0.03f), new Vector3(0.28f, 0.10f, 0.20f));
            Box(v, "Neck", Mat("WoodMid"), new Vector3(-0.17f, 0.11f, 0f), new Vector3(0.07f, 0.07f, 0.07f),
                new Vector3(0f, 0f, 22f));
            Box(v, "Tie", Mat("Earth"), new Vector3(-0.145f, 0.105f, 0f), new Vector3(0.016f, 0.09f, 0.09f));
            End(root, "sandbag");
        }

        private static void LightningRod()
        {
            var v = Begin("lightning_rod", new Vector3(0.34f, 0.62f, 0.34f), 0.26f, out var root);
            Rod(v, "Base", Mat("Stone"), new Vector3(0f, 0.025f, 0f), new Vector3(0.20f, 0.025f, 0.20f));
            Box(v, "Mast", Mat("StoneLight"), new Vector3(0f, 0.26f, 0f), new Vector3(0.035f, 0.24f, 0.035f));
            Box(v, "Spike", Mat("AccentDeep"), new Vector3(0f, 0.49f, 0f), new Vector3(0.022f, 0.13f, 0.022f));
            Ball(v, "Point", Mat("AccentLight"), new Vector3(0f, 0.565f, 0f), Vector3.one * 0.038f);
            // The earth strap is the clue: it is the thing that goes *down*.
            Box(v, "Strap", Mat("AccentDeep"), new Vector3(0.055f, 0.16f, 0f), new Vector3(0.016f, 0.28f, 0.016f),
                new Vector3(0f, 0f, -14f));
            End(root, "lightning_rod");
        }

        // -------------------------------------------------------------------
        // Crystal cave
        // -------------------------------------------------------------------

        private static void Lantern()
        {
            var v = Begin("lantern", new Vector3(0.38f, 0.54f, 0.34f), 0.22f, out var root);
            Rod(v, "Base", Mat("Stone"), new Vector3(0f, 0.035f, 0f), new Vector3(0.17f, 0.035f, 0.17f));
            Ball(v, "Flame", Mat("AccentLight"), new Vector3(0f, 0.20f, 0f), new Vector3(0.13f, 0.16f, 0.13f));
            foreach (var (x, z) in new[] { (-0.075f, -0.075f), (0.075f, -0.075f), (-0.075f, 0.075f), (0.075f, 0.075f) })
            {
                Box(v, "Post", Mat("StoneLight"), new Vector3(x, 0.20f, z), new Vector3(0.018f, 0.24f, 0.018f));
            }

            Rod(v, "Cap", Mat("Stone"), new Vector3(0f, 0.335f, 0f), new Vector3(0.19f, 0.028f, 0.19f));
            BlockRing(v, "Bail", Mat("StoneLight"), new Vector3(0f, 0.42f, 0f),
                new Vector2(0.065f, 0.06f), 8, 0.018f, 0.024f);
            End(root, "lantern");
        }

        private static void Pickaxe()
        {
            var v = Begin("pickaxe", new Vector3(0.46f, 0.54f, 0.30f), 0.22f, out var root);
            Box(v, "Haft", Mat("Wood"), new Vector3(0f, 0.20f, 0f), new Vector3(0.035f, 0.38f, 0.035f));
            Box(v, "Grip", Mat("Earth"), new Vector3(0f, 0.07f, 0f), new Vector3(0.045f, 0.12f, 0.045f));
            Box(v, "Head", Mat("StoneLight"), new Vector3(0f, 0.44f, 0f), new Vector3(0.30f, 0.05f, 0.05f));
            Box(v, "PointL", Mat("Stone"), new Vector3(-0.185f, 0.415f, 0f), new Vector3(0.10f, 0.045f, 0.045f),
                new Vector3(0f, 0f, 22f));
            Box(v, "PointR", Mat("Stone"), new Vector3(0.185f, 0.415f, 0f), new Vector3(0.10f, 0.045f, 0.045f),
                new Vector3(0f, 0f, -22f));
            End(root, "pickaxe");
        }

        /// <summary>
        /// A tuned shard and its striker. The mallet is what says "hit this",
        /// which the player has to read before they can guess that pitch matters.
        /// </summary>
        private static void ChimeCrystal()
        {
            var v = Begin("chime_crystal", new Vector3(0.44f, 0.54f, 0.34f), 0.22f, out var root);
            Box(v, "Foot", Mat("Ink"), new Vector3(0f, 0.03f, 0f), new Vector3(0.22f, 0.06f, 0.16f));
            foreach (var (x, h, tilt) in new[] { (-0.045f, 0.34f, -9f), (0.045f, 0.26f, 11f), (0f, 0.42f, 2f) })
            {
                Box(v, "Shard", Mat("WaterBright"), new Vector3(x, 0.06f + h * 0.5f, 0f),
                    new Vector3(0.085f, h * 0.5f, 0.085f), new Vector3(0f, 45f, tilt));
            }

            Ball(v, "Glint", Mat("Cream"), new Vector3(0.01f, 0.40f, -0.045f), new Vector3(0.03f, 0.06f, 0.02f));
            Box(v, "MalletHandle", Mat("Wood"), new Vector3(0.145f, 0.13f, -0.06f),
                new Vector3(0.020f, 0.20f, 0.020f), new Vector3(0f, 0f, -16f));
            Ball(v, "MalletHead", Mat("Cream"), new Vector3(0.19f, 0.235f, -0.06f), Vector3.one * 0.055f);
            End(root, "chime_crystal");
        }

        // -------------------------------------------------------------------
        // Snowpeak
        // -------------------------------------------------------------------

        private static void Sled()
        {
            var v = Begin("sled", new Vector3(0.46f, 0.28f, 0.72f), 0.07f, out var root);
            Box(v, "Deck", Mat("Wood"), new Vector3(0f, 0.085f, 0f), new Vector3(0.26f, 0.030f, 0.54f));
            for (var i = -2; i <= 2; i++)
            {
                Box(v, "Slat", Mat("Sand"), new Vector3(0f, 0.102f, i * 0.10f),
                    new Vector3(0.25f, 0.014f, 0.06f));
            }

            foreach (var x in new[] { -0.10f, 0.10f })
            {
                Box(v, "Runner", Mat("StoneLight"), new Vector3(x, 0.035f, 0f), new Vector3(0.030f, 0.030f, 0.58f));
                Box(v, "Prow", Mat("StoneLight"), new Vector3(x, 0.085f, 0.30f), new Vector3(0.030f, 0.030f, 0.14f),
                    new Vector3(-38f, 0f, 0f));
            }

            Box(v, "Rein", Mat("Earth"), new Vector3(0f, 0.13f, 0.30f), new Vector3(0.20f, 0.012f, 0.012f));
            End(root, "sled");
        }

        // -------------------------------------------------------------------
        // Deep ocean
        // -------------------------------------------------------------------

        /// <summary>A clam holding one caught bubble. The only lift available down here.</summary>
        private static void BubbleShell()
        {
            var v = Begin("bubble_shell", new Vector3(0.42f, 0.44f, 0.40f), 0.15f, out var root);
            Ball(v, "LowerShell", Mat("Sand"), new Vector3(0f, 0.075f, 0f), new Vector3(0.30f, 0.11f, 0.26f));
            Ball(v, "UpperShell", Mat("Cream"), new Vector3(0f, 0.175f, -0.02f), new Vector3(0.28f, 0.10f, 0.24f),
                new Vector3(-14f, 0f, 0f));
            for (var i = -2; i <= 2; i++)
            {
                Box(v, "Rib", Mat("WoodMid"), new Vector3(i * 0.055f, 0.215f, -0.02f),
                    new Vector3(0.012f, 0.012f, 0.20f), new Vector3(-14f, i * 8f, 0f));
            }

            Ball(v, "Air", Mat("WaterBright"), new Vector3(0f, 0.135f, 0.02f), Vector3.one * 0.115f);
            Ball(v, "AirGlint", Mat("Cream"), new Vector3(-0.035f, 0.165f, -0.02f), Vector3.one * 0.028f);
            End(root, "bubble_shell");
        }

        /// <summary>A small drifting light with a body. Bait that nothing has to smell.</summary>
        private static void GlowJelly()
        {
            var v = Begin("glow_jelly", new Vector3(0.38f, 0.50f, 0.38f), 0.20f, out var root);
            Ball(v, "Bell", Mat("WaterBright"), new Vector3(0f, 0.31f, 0f), new Vector3(0.24f, 0.19f, 0.24f));
            Ball(v, "Core", Mat("AccentLight"), new Vector3(0f, 0.29f, 0f), new Vector3(0.13f, 0.10f, 0.13f));
            Rod(v, "Fringe", Mat("Cream"), new Vector3(0f, 0.235f, 0f), new Vector3(0.22f, 0.010f, 0.22f));
            for (var i = 0; i < 5; i++)
            {
                var a = i * Mathf.PI * 2f / 5f;
                Box(v, "Tentacle", Mat("WaterLight"),
                    new Vector3(Mathf.Cos(a) * 0.065f, 0.13f, Mathf.Sin(a) * 0.065f),
                    new Vector3(0.012f, 0.21f, 0.012f), new Vector3(Mathf.Sin(a) * 12f, 0f, Mathf.Cos(a) * 12f));
            }

            End(root, "glow_jelly");
        }

        // -------------------------------------------------------------------
        // Orbit
        // -------------------------------------------------------------------

        private static void Thruster()
        {
            var v = Begin("thruster", new Vector3(0.36f, 0.50f, 0.34f), 0.20f, out var root);
            Rod(v, "Bottle", Mat("Cream"), new Vector3(0f, 0.19f, 0f), new Vector3(0.16f, 0.16f, 0.16f));
            Rod(v, "Band", Mat("AccentDeep"), new Vector3(0f, 0.26f, 0f), new Vector3(0.17f, 0.020f, 0.17f));
            Rod(v, "Neck", Mat("Stone"), new Vector3(0f, 0.375f, 0f), new Vector3(0.075f, 0.045f, 0.075f));
            Rod(v, "Nozzle", Mat("StoneLight"), new Vector3(0f, 0.435f, 0f), new Vector3(0.115f, 0.030f, 0.115f));
            Box(v, "Trigger", Mat("Violet"), new Vector3(0.10f, 0.30f, 0f), new Vector3(0.075f, 0.045f, 0.05f));
            Box(v, "Gauge", Mat("WaterBright"), new Vector3(0f, 0.235f, -0.085f), new Vector3(0.06f, 0.06f, 0.02f));
            End(root, "thruster");
        }

        // -------------------------------------------------------------------
        // Foundry
        // -------------------------------------------------------------------

        private static void Crate()
        {
            var v = Begin("crate", new Vector3(0.44f, 0.44f, 0.44f), 0.15f, out var root);
            var body = Box(v, "Body", Mat("Wood"), new Vector3(0f, 0.15f, 0f), new Vector3(0.28f, 0.28f, 0.28f));
            Round(body);
            foreach (var z in new[] { -0.146f, 0.146f })
            {
                Box(v, "Batten_X", Mat("WoodDark"), new Vector3(0f, 0.15f, z), new Vector3(0.29f, 0.03f, 0.012f));
                Box(v, "Batten_D", Mat("WoodDark"), new Vector3(0f, 0.15f, z), new Vector3(0.36f, 0.028f, 0.012f),
                    new Vector3(0f, 0f, 45f));
            }

            Box(v, "Lid", Mat("Sand"), new Vector3(0f, 0.296f, 0f), new Vector3(0.29f, 0.02f, 0.29f));
            Box(v, "Stencil", Mat("AccentDeep"), new Vector3(0f, 0.19f, -0.148f), new Vector3(0.09f, 0.09f, 0.008f));
            End(root, "crate");
        }

        // -------------------------------------------------------------------
        // Neon city
        // -------------------------------------------------------------------

        /// <summary>A trolley grip: a wheel that runs on a rail, and a T to hold.</summary>
        private static void ZipGrip()
        {
            var v = Begin("zip_grip", new Vector3(0.40f, 0.50f, 0.32f), 0.20f, out var root);
            Rod(v, "Wheel", Mat("StoneLight"), new Vector3(0f, 0.38f, 0f), new Vector3(0.15f, 0.035f, 0.15f),
                new Vector3(90f, 0f, 0f));
            Rod(v, "Hub", Mat("WaterBright"), new Vector3(0f, 0.38f, -0.028f), new Vector3(0.06f, 0.02f, 0.06f),
                new Vector3(90f, 0f, 0f));
            foreach (var x in new[] { -0.055f, 0.055f })
            {
                Box(v, "Cheek", Mat("Stone"), new Vector3(x, 0.36f, 0f), new Vector3(0.022f, 0.13f, 0.06f));
            }

            Box(v, "Stem", Mat("Stone"), new Vector3(0f, 0.235f, 0f), new Vector3(0.03f, 0.14f, 0.03f));
            Box(v, "Bar", Mat("AccentDeep"), new Vector3(0f, 0.15f, 0f), new Vector3(0.26f, 0.05f, 0.05f));
            foreach (var x in new[] { -0.10f, 0.10f })
            {
                Box(v, "Grip", Mat("Violet"), new Vector3(x, 0.15f, 0f), new Vector3(0.07f, 0.058f, 0.058f));
            }

            End(root, "zip_grip");
        }

        /// <summary>A bent glass tube with two end pins: obviously a part, obviously missing.</summary>
        private static void NeonTube()
        {
            var v = Begin("neon_tube", new Vector3(0.44f, 0.52f, 0.28f), 0.22f, out var root);
            foreach (var (pos, scale, euler) in new[]
                     {
                         (new Vector3(-0.09f, 0.36f, 0f), new Vector3(0.030f, 0.13f, 0.030f), new Vector3(0f, 0f, 26f)),
                         (new Vector3(0.01f, 0.25f, 0f), new Vector3(0.030f, 0.14f, 0.030f), new Vector3(0f, 0f, -34f)),
                         (new Vector3(0.10f, 0.14f, 0f), new Vector3(0.030f, 0.13f, 0.030f), new Vector3(0f, 0f, 26f)),
                     })
            {
                Rod(v, "Tube", Mat("WaterBright"), pos, scale, euler);
            }

            Rod(v, "Cap_Top", Mat("Stone"), new Vector3(-0.125f, 0.425f, 0f), new Vector3(0.045f, 0.022f, 0.045f),
                new Vector3(0f, 0f, 26f));
            Rod(v, "Cap_Bottom", Mat("Stone"), new Vector3(0.135f, 0.075f, 0f), new Vector3(0.045f, 0.022f, 0.045f),
                new Vector3(0f, 0f, 26f));
            Box(v, "Pin_Top", Mat("AccentDeep"), new Vector3(-0.15f, 0.475f, 0f), new Vector3(0.016f, 0.05f, 0.016f));
            Box(v, "Pin_Bottom", Mat("AccentDeep"), new Vector3(0.155f, 0.03f, 0f), new Vector3(0.016f, 0.05f, 0.016f));
            End(root, "neon_tube");
        }
    }
}
