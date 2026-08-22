using System.Collections.Generic;
using SavePeps.Rescue;
using UnityEngine;

using static SavePeps.EditorTools.Toy;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// The twelve worlds: what each one is made of, what shape it is, what
    /// light it sits in, and what it sounds like.
    ///
    /// The first catalogue's dioramas all shared one silhouette — a
    /// 1.5 x 3.4 slab, a chasm strip at the centre, three slots in the same
    /// three places, one camera. Ocean, space, factory and neon were that slab
    /// with different material assignments and two decorations, which is why
    /// they photographed identically. The fix is here rather than in the
    /// individual stages: a world owns its base *shape*, so three rescues in a
    /// round inherit one silhouette and neighbouring rounds cannot share one.
    ///
    /// A world supplies the base, the dressing, the atmosphere and the sound
    /// bed. A stage supplies the problem. That split is what keeps thirty-six
    /// environments to about forty lines of new geometry each.
    /// </summary>
    internal static class Worlds
    {
        internal const string Garden = "garden";
        internal const string Clock = "clock";
        internal const string Weather = "weather";
        internal const string Canyon = "canyon";
        internal const string Tide = "tide";
        internal const string Storm = "storm";
        internal const string Cave = "cave";
        internal const string Peak = "peak";
        internal const string Abyss = "abyss";
        internal const string Orbit = "orbit";
        internal const string Forge = "forge";
        internal const string Neon = "neon";

        internal static IReadOnlyDictionary<string, Material> M;

        private static Material Mat(string key) => M[key];

        // -------------------------------------------------------------------
        // Stage lifecycle
        // -------------------------------------------------------------------

        /// <summary>
        /// Opens a stage: names it, builds its world's base silhouette, and
        /// hands back the root for the stage's own geometry.
        /// </summary>
        internal static GameObject Begin(string world, string stage)
        {
            var root = new GameObject($"Diorama_{Title(world)}_{stage}");
            BuildBase(world, root.transform);
            return root;
        }

        /// <summary>Closes a stage: world dressing, the three choice pads, the atmosphere, and the prefab.</summary>
        internal static GameObject Finish(GameObject root, string world, string envDir)
        {
            Dress(world, root.transform);
            AddChoicePads(root.transform);
            Atmosphere(world, root);
            // Last, so every hidden mover's visuals already exist.
            ApplyReveals();
            return SavePrefab(root, $"{envDir}/{root.name}.prefab");
        }

        internal static void Peps(Transform root, Vector3 a, Vector3 b, Vector3 meet)
        {
            Anchor(root, "Anchor_PepA", a);
            Anchor(root, "Anchor_PepB", b);
            Anchor(root, "Anchor_Meet", meet);
        }

        internal static void Slots(Transform root, Vector3 one, Vector3 two, Vector3 three)
        {
            Anchor(root, "Slot_1", one);
            Anchor(root, "Slot_2", two);
            Anchor(root, "Slot_3", three);
        }

        private static string Title(string world) => char.ToUpperInvariant(world[0]) + world[1..];

        /// <summary>
        /// The one affordance that must never vary. Row 6 of the palette is
        /// reserved for attention, and the pad is how a player learns that a
        /// thing on a gold disc is a thing you can tap — in a cave as much as
        /// in a garden.
        /// </summary>
        private static void AddChoicePads(Transform root)
        {
            foreach (var anchorId in new[] { "Slot_1", "Slot_2", "Slot_3" })
            {
                var anchor = FindChild(root, anchorId);
                if (anchor == null) continue;

                var pad = Child(root, $"ChoicePad_{anchorId}");
                pad.localPosition = anchor.localPosition + Vector3.up * 0.004f;

                var halo = Rod(pad, "Halo", Mat("AccentLight"), Vector3.zero, new Vector3(0.30f, 0.008f, 0.25f));
                var surface = Rod(pad, "Surface", Mat("Cream"), Vector3.up * 0.012f,
                    new Vector3(0.255f, 0.008f, 0.21f));

                pad.gameObject.AddComponent<ChoicePad>().Configure(anchorId, halo.transform, surface.transform);
            }
        }

        // -------------------------------------------------------------------
        // Base silhouettes — the thing you recognise before you read anything
        // -------------------------------------------------------------------

        private static void BuildBase(string world, Transform root)
        {
            switch (world)
            {
                case Garden: GardenBase(root); break;
                case Clock: ClockBase(root); break;
                case Weather: WeatherBase(root); break;
                case Canyon: CanyonBase(root); break;
                case Tide: TideBase(root); break;
                case Storm: StormBase(root); break;
                case Cave: CaveBase(root); break;
                case Peak: PeakBase(root); break;
                case Abyss: AbyssBase(root); break;
                case Orbit: OrbitBase(root); break;
                case Forge: ForgeBase(root); break;
                case Neon: NeonBase(root); break;
            }
        }

        /// <summary>
        /// The toy-on-a-table plinth the game opens on: a soft slab with a
        /// visible lip and four brass pegs. Only the worlds that stand on
        /// solid ground use it, and each one changes its proportions — the
        /// pegs are the family resemblance, not the shape.
        /// </summary>
        private static void Plinth(Transform root, Material top, Material side, float width, float depth,
            float height, float lawnInset = 0.15f, bool pegs = true)
        {
            var slab = Box(root, "Platform", side, new Vector3(0f, -height * 0.5f, 0f),
                new Vector3(width, height, depth));
            Round(slab);

            Box(root, "InsetLip", Mat("EarthLight"), new Vector3(0f, -0.035f, 0f),
                new Vector3(width * 0.955f, 0.056f, depth * 0.962f));
            Box(root, "BaseFoot", Mat("Ink"), new Vector3(0f, -height - 0.025f, 0.03f),
                new Vector3(width * 0.905f, 0.055f, depth * 0.912f));

            // A null top means the stage lays its own ground, because it has
            // to cut a channel, a pit or a trench into it.
            if (top != null)
            {
                Box(root, "Ground", top, new Vector3(0f, 0.075f, 0f),
                    new Vector3(width - lawnInset, 0.15f, depth - lawnInset * 1.7f));
            }

            if (!pegs) return;
            foreach (var x in new[] { -width * 0.467f, width * 0.467f })
            foreach (var z in new[] { -depth * 0.459f, depth * 0.459f })
            {
                Rod(root, "CornerPeg", Mat("AccentLight"), new Vector3(x, -0.003f, z),
                    new Vector3(0.032f, 0.018f, 0.032f));
            }
        }

        private static void GardenBase(Transform root) =>
            Plinth(root, null, Mat("Earth"), 1.50f, 3.40f, 0.16f);

        /// <summary>
        /// A stone plinth inside a brass frame that runs off the top of the
        /// picture. The overhead beam is the point: it turns the open sky of
        /// the garden into the inside of a machine, and it is visible in every
        /// courtyard screenshot before any gear is.
        /// </summary>
        private static void ClockBase(Transform root)
        {
            Plinth(root, Mat("StoneLight"), Mat("Stone"), 1.62f, 3.10f, 0.30f, 0.18f);

            // Chequer, so the floor reads as laid stone rather than a plane.
            for (var i = 0; i < 5; i++)
            for (var j = 0; j < 3; j++)
            {
                if ((i + j) % 2 == 1) continue;
                Box(root, "FloorTile", Mat("Stone"),
                    new Vector3(-0.48f + j * 0.48f, 0.152f, -1.16f + i * 0.58f),
                    new Vector3(0.44f, 0.012f, 0.52f));
            }

            foreach (var x in new[] { -0.78f, 0.78f })
            {
                Box(root, "FramePost", Mat("Accent"), new Vector3(x, 0.72f, 0.62f),
                    new Vector3(0.075f, 1.30f, 0.10f));
                Rod(root, "FrameBolt", Mat("AccentLight"), new Vector3(x * 0.94f, 1.24f, 0.62f),
                    new Vector3(0.055f, 0.02f, 0.055f), new Vector3(0f, 0f, 90f));
            }

            Box(root, "FrameBeam", Mat("Accent"), new Vector3(0f, 1.34f, 0.62f),
                new Vector3(1.72f, 0.11f, 0.13f));
            Box(root, "FrameBeamTrim", Mat("AccentLight"), new Vector3(0f, 1.41f, 0.62f),
                new Vector3(1.60f, 0.035f, 0.14f));
        }

        /// <summary>
        /// Three terraces stepping down towards the camera. The staircase
        /// silhouette is what makes round three's three rescues read as three
        /// heights of one hillside rather than as three unrelated fields.
        /// </summary>
        private static void WeatherBase(Transform root)
        {
            Box(root, "Platform", Mat("Earth"), new Vector3(0f, -0.11f, 0f), new Vector3(1.52f, 0.22f, 3.40f));
            Box(root, "BaseFoot", Mat("Ink"), new Vector3(0f, -0.245f, 0.03f), new Vector3(1.38f, 0.055f, 3.10f));

            // Three shelves whose tops sit at exactly 0.15, 0.42 and 0.70, so
            // a stage can place a Pep on one by naming the number rather than
            // by re-deriving a centre and a half-height.
            var tops = new[] { 0.15f, 0.42f, 0.70f };
            var lanes = new[] { (-1.15f, 1.05f), (0f, 1.24f), (1.15f, 1.08f) };
            var skins = new[] { Mat("FoliageLight"), Mat("Foliage"), Mat("Snow") };

            for (var i = 0; i < 3; i++)
            {
                var (z, depth) = lanes[i];
                Box(root, $"Terrace_{i}", skins[i], new Vector3(0f, tops[i] * 0.5f, z),
                    new Vector3(1.36f, tops[i], depth));
                Box(root, $"Riser_{i}", Mat("EarthLight"),
                    new Vector3(0f, tops[i] * 0.5f, z - depth * 0.5f + 0.025f),
                    new Vector3(1.38f, tops[i] - 0.02f, 0.05f));
            }

            foreach (var x in new[] { -0.70f, 0.70f })
            foreach (var z in new[] { -1.56f, 1.56f })
            {
                Rod(root, "CornerPeg", Mat("AccentLight"), new Vector3(x, -0.003f, z),
                    new Vector3(0.032f, 0.018f, 0.032f));
            }
        }

        /// <summary>
        /// Two mesas and nothing between them. There is deliberately no slab
        /// joining the halves: sky under the middle of the frame is what makes
        /// the chasm a chasm rather than a painted stripe, and it is the only
        /// world where the ground has a hole all the way through.
        /// </summary>
        private static void CanyonBase(Transform root)
        {
            // Rim tops land on exactly 0.18 near and 0.42 far. The height
            // difference is deliberate: the far side is somewhere you have to
            // get *up* to, not merely across to, which is what separates this
            // world's crossings from the brook's.
            foreach (var (name, z, depth, rim, top) in new[]
                     {
                         ("Mesa_Near", -1.10f, 1.16f, 0.18f, Mat("Sand")),
                         ("Mesa_Far", 1.14f, 1.10f, 0.42f, Mat("Sand")),
                     })
            {
                Box(root, name, Mat("EarthDark"), new Vector3(0f, rim * 0.5f - 0.60f, z),
                    new Vector3(1.44f, rim + 1.20f, depth));
                // The cap sits a hair proud of the body. Flush, the two top
                // faces are coplanar and z-fight: the deck reads pale from one
                // camera angle and near-black from another.
                Box(root, $"{name}_Cap", top, new Vector3(0f, rim - 0.036f, z),
                    new Vector3(1.48f, 0.09f, depth + 0.05f));

                // Strata: shelves of colour down each wall, which is what
                // stops a tall box from reading as a tall box. Two bands, not
                // three, and only in the top third -- evenly spaced bands over
                // a whole face read as drawer fronts rather than as rock.
                for (var i = 0; i < 2; i++)
                {
                    Box(root, $"{name}_Strata", i == 0 ? Mat("Clay") : Mat("EarthLight"),
                        new Vector3(0f, rim - 0.17f - i * 0.21f, z),
                        new Vector3(1.46f - i * 0.03f, i == 0 ? 0.11f : 0.06f, depth + 0.02f));
                }

                // Vertical erosion flutes down the chasm-facing wall. Rock
                // weathers downwards; the verticals are what break the
                // horizontal banding, and this is the only wall you see.
                for (var i = 0; i < 8; i++)
                {
                    var x = -0.63f + i * 0.18f;
                    Box(root, $"{name}_Flute", i % 2 == 0 ? Mat("Earth") : Mat("Clay"),
                        new Vector3(x, rim - 0.52f, z + (z < 0f ? depth * 0.5f : -depth * 0.5f)),
                        new Vector3(0.07f + (i % 3) * 0.02f, 0.72f + (i % 4) * 0.09f, 0.04f));
                }

                Box(root, $"{name}_Rim", Mat("Clay"),
                    new Vector3(0f, rim - 0.002f, z + (z < 0f ? depth * 0.5f - 0.03f : -depth * 0.5f + 0.03f)),
                    new Vector3(1.48f, 0.02f, 0.08f));
            }

            // The hole itself. Two mesas with a gap between them read as a
            // staircase from any shallow angle; a dark floor a long way down,
            // with the walls narrowing towards it, is what makes it a drop.
            // High enough that the camera clears the near rim and sees it,
            // low enough that it is still a fall. A floor you cannot see is
            // indistinguishable from no floor, and both read as a step.
            Box(root, "ChasmFloor", Mat("Abyss"), new Vector3(0f, -0.72f, 0.02f),
                new Vector3(1.20f, 0.10f, 1.05f));
            Box(root, "ChasmSilt", Mat("Ink"), new Vector3(0f, -0.665f, 0.02f),
                new Vector3(1.06f, 0.02f, 0.88f));
            for (var i = 0; i < 3; i++)
            {
                Box(root, "ChasmWall", i % 2 == 0 ? Mat("EarthDark") : Mat("Ink"),
                    new Vector3(0f, -0.16f - i * 0.22f, -0.55f + i * 0.06f),
                    new Vector3(1.30f - i * 0.10f, 0.22f, 0.08f));
                Box(root, "ChasmWall", i % 2 == 0 ? Mat("EarthDark") : Mat("Ink"),
                    new Vector3(0f, -0.16f - i * 0.22f, 0.60f - i * 0.06f),
                    new Vector3(1.30f - i * 0.10f, 0.22f, 0.08f));
            }
        }

        /// <summary>
        /// Water that runs off the edge of the picture. The plane is wider and
        /// deeper than any platform in the game, so the docks read as standing
        /// in a sea rather than as a pond painted on a table top.
        /// </summary>
        private static void TideBase(Transform root)
        {
            Box(root, "SeaBed", Mat("WaterDeep"), new Vector3(0f, -0.20f, 0f), new Vector3(2.30f, 0.30f, 4.10f));

            var sea = Living(root, "Sea", AmbientMode.Bob, 0.012f, 0.28f, Vector3.up);
            Box(sea, "Surface", Mat("Water"), new Vector3(0f, 0.02f, 0f), new Vector3(2.26f, 0.10f, 4.06f));
            for (var i = 0; i < 5; i++)
            {
                Box(sea, "Swell", Mat("WaterBright"),
                    new Vector3(-0.80f + i * 0.42f, 0.076f, -1.30f + (i % 3) * 1.05f),
                    new Vector3(0.46f + i * 0.05f, 0.012f, 0.05f), new Vector3(0f, i * 9f - 18f, 0f));
            }

            Box(root, "SandBar", Mat("WoodMid"), new Vector3(0f, 0.04f, -1.50f), new Vector3(2.05f, 0.10f, 0.92f));
            Box(root, "SandTop", Mat("Sand"), new Vector3(0f, 0.086f, -1.52f), new Vector3(1.98f, 0.02f, 0.86f));
            Box(root, "TideLine", Mat("WoodDark"), new Vector3(0f, 0.098f, -1.06f), new Vector3(2.00f, 0.012f, 0.06f));
            foreach (var (x, z, s2) in new[] { (-0.72f, -1.66f, 0.10f), (0.66f, -1.42f, 0.08f), (0.02f, -1.72f, 0.07f) })
            {
                Ball(root, "Pebble", Mat("Stone"), new Vector3(x, 0.10f, z), new Vector3(s2, s2 * 0.5f, s2 * 0.8f));
            }
            Box(root, "BaseFoot", Mat("Ink"), new Vector3(0f, -0.36f, 0.03f), new Vector3(2.06f, 0.055f, 3.72f));
        }

        /// <summary>
        /// A narrow roof on a shaft that leaves the frame. Everything else in
        /// the game sits on something; this one sits above something, and the
        /// missing ground under the parapet is the whole feeling of the round.
        /// </summary>
        private static void StormBase(Transform root)
        {
            Box(root, "Tower", Mat("Ink"), new Vector3(0f, -1.10f, 0.12f), new Vector3(1.18f, 2.10f, 2.45f));
            Box(root, "TowerBand", Mat("Violet"), new Vector3(0f, -0.34f, 0.12f), new Vector3(1.22f, 0.07f, 2.49f));
            Box(root, "Roof", Mat("Stone"), new Vector3(0f, -0.02f, 0.12f), new Vector3(1.32f, 0.14f, 2.62f));

            // A wet roof: dark stone with a few standing slicks.
            Box(root, "RoofFelt", Mat("Violet"), new Vector3(0f, 0.055f, 0.12f), new Vector3(1.22f, 0.03f, 2.52f));
            foreach (var (x, z, s) in new[] { (-0.30f, 0.72f, 0.30f), (0.26f, -0.30f, 0.24f), (0.12f, 1.06f, 0.18f) })
            {
                Ball(root, "Slick", Mat("StoneLight"), new Vector3(x, 0.072f, z), new Vector3(s, 0.014f, s * 0.66f));
            }

            foreach (var x in new[] { -0.63f, 0.63f })
            {
                Box(root, "Parapet", Mat("Stone"), new Vector3(x, 0.14f, 0.12f), new Vector3(0.07f, 0.22f, 2.62f));
            }

            Box(root, "ParapetBack", Mat("Stone"), new Vector3(0f, 0.14f, 1.40f), new Vector3(1.32f, 0.22f, 0.07f));
        }

        /// <summary>
        /// The only enclosed world: floor, two rock walls and a ceiling with
        /// stalactites hanging into the top of the frame. A cave screenshot is
        /// identifiable from its edges alone, before any crystal lights up.
        /// </summary>
        private static void CaveBase(Transform root)
        {
            Box(root, "Floor", Mat("Ink"), new Vector3(0f, -0.06f, 0.10f), new Vector3(1.70f, 0.16f, 3.30f));
            // Cool rock, not mud. The floor is the only large mid-value in the
            // frame, so it is what the coral Peps have to read against.
            Box(root, "FloorDust", Mat("Night"), new Vector3(0f, 0.025f, 0.10f),
                new Vector3(1.54f, 0.03f, 3.10f));

            foreach (var x in new[] { -0.86f, 0.86f })
            {
                Box(root, "Wall", Mat("Abyss"), new Vector3(x, 0.62f, 0.20f), new Vector3(0.28f, 1.45f, 3.30f));
                for (var i = 0; i < 4; i++)
                {
                    Ball(root, "WallBoss", Mat("Violet"),
                        new Vector3(x - Mathf.Sign(x) * 0.13f, 0.20f + i * 0.32f, -1.10f + i * 0.78f),
                        new Vector3(0.22f, 0.30f, 0.46f));
                }
            }

            // The back wall is what makes this the only enclosed world. Without
            // it the top half of every cave frame is open sky, which is exactly
            // the thing a cave is not. It runs well above the top of frame: a
            // flat ceiling slab cannot work here, because the camera looks down
            // at 39 degrees and would see the slab's lid, not its underside.
            Box(root, "BackWall", Mat("Abyss"), new Vector3(0f, 1.10f, 1.94f), new Vector3(2.04f, 2.70f, 0.26f));
            foreach (var (x, y, s) in new[]
                     {
                         (-0.58f, 0.52f, 0.34f), (0.12f, 0.94f, 0.46f), (0.66f, 0.44f, 0.30f),
                         (-0.24f, 1.28f, 0.26f),
                     })
            {
                Ball(root, "BackBoss", Mat("Violet"), new Vector3(x, y, 1.78f),
                    new Vector3(s * 1.4f, s, 0.30f));
            }

            // A seam of lit crystal high on the back wall: the one bright thing
            // in the round, and the reason the eye goes up before it goes down.
            foreach (var (x, y, s) in new[]
                     {
                         (-0.72f, 1.14f, 0.13f), (-0.50f, 1.30f, 0.09f), (0.34f, 1.34f, 0.11f),
                         (0.58f, 1.16f, 0.08f), (0.86f, 1.02f, 0.10f),
                     })
            {
                Round(Box(root, "Seam", Mat("WaterBright"), new Vector3(x, y, 1.76f),
                    new Vector3(s, s * 1.9f, s), new Vector3(0f, 0f, x * 26f)));
            }

            // Stalactites hang where there is rock to hang them from: along the
            // top of the back wall, and off the inner lip of the side walls.
            // Pale, so they read as silhouette against the dark wall behind.
            foreach (var (x, y, z, h) in new[]
                     {
                         (-0.62f, 1.78f, 1.72f, 0.52f), (-0.18f, 1.80f, 1.74f, 0.40f),
                         (0.26f, 1.78f, 1.72f, 0.58f), (0.68f, 1.79f, 1.74f, 0.34f),
                         (-0.74f, 1.32f, 0.42f, 0.44f), (0.76f, 1.32f, 0.96f, 0.36f),
                     })
            {
                Rod(root, "Stalactite", Mat("Stone"), new Vector3(x, y - h * 0.5f, z),
                    new Vector3(0.14f, h * 0.5f, 0.14f), new Vector3(180f, 0f, 0f));
            }

            Box(root, "BaseFoot", Mat("Ink"), new Vector3(0f, -0.16f, 0.13f), new Vector3(1.56f, 0.055f, 3.06f));
        }

        /// <summary>
        /// One steep wedge, high at the back and low at the front. Everything
        /// in round eight either wants to slide down it or is trying not to.
        /// </summary>
        private static void PeakBase(Transform root)
        {
            Box(root, "Platform", Mat("Stone"), new Vector3(0f, -0.14f, 0f), new Vector3(1.52f, 0.28f, 3.40f));
            Box(root, "BaseFoot", Mat("Ink"), new Vector3(0f, -0.30f, 0.03f), new Vector3(1.38f, 0.055f, 3.10f));

            // The slope: a stack of shallow steps whose tops line up as a ramp.
            for (var i = 0; i < 7; i++)
            {
                var z = -1.44f + i * 0.48f;
                var height = 0.10f + i * 0.155f;
                Box(root, $"Slope_{i}", Mat("Snow"), new Vector3(0f, height * 0.5f, z),
                    new Vector3(1.36f, height, 0.52f));
                Box(root, $"SlopeEdge_{i}", Mat("Ice"), new Vector3(0f, height - 0.005f, z - 0.24f),
                    new Vector3(1.34f, 0.02f, 0.06f));
            }

            Box(root, "Cornice", Mat("Snow"), new Vector3(0f, 1.24f, 1.52f), new Vector3(1.44f, 0.26f, 0.46f));
            Box(root, "CorniceLip", Mat("Ice"), new Vector3(0f, 1.14f, 1.26f), new Vector3(1.40f, 0.10f, 0.16f));

            foreach (var x in new[] { -0.62f, 0.64f })
            {
                Rod(root, "Outcrop", Mat("Stone"), new Vector3(x, 0.70f, 1.15f), new Vector3(0.22f, 0.34f, 0.22f));
            }
        }

        /// <summary>
        /// A trench: two walls taller than the frame, a narrow floor, and no
        /// sky at all. Combined with the fog it is the only world where the
        /// player cannot see an edge of the world in any direction.
        /// </summary>
        private static void AbyssBase(Transform root)
        {
            Box(root, "TrenchFloor", Mat("Sand"), new Vector3(0f, -0.05f, 0.10f), new Vector3(1.30f, 0.16f, 3.40f));
            Box(root, "Silt", Mat("WoodDark"), new Vector3(0f, 0.035f, 0.10f), new Vector3(1.14f, 0.03f, 3.20f));

            foreach (var x in new[] { -0.88f, 0.88f })
            {
                Box(root, "TrenchWall", Mat("Ink"), new Vector3(x, 0.80f, 0.20f), new Vector3(0.42f, 1.90f, 3.40f));
                for (var i = 0; i < 5; i++)
                {
                    Box(root, "Shelf", Mat("Violet"),
                        new Vector3(x - Mathf.Sign(x) * 0.16f, 0.10f + i * 0.40f, -1.20f + i * 0.62f),
                        new Vector3(0.20f, 0.09f, 0.70f));
                }
            }

            // Bioluminescent seams pick the walls out of the fog.
            foreach (var (x, y, z) in new[] { (-0.70f, 0.62f, -0.30f), (0.72f, 0.94f, 0.80f), (-0.70f, 1.20f, 1.40f) })
            {
                var seam = Living(root, "Seam", AmbientMode.Pulse, 0.16f, 0.22f, Vector3.up);
                seam.localPosition = new Vector3(x, y, z);
                Box(seam, "Glow", Mat("WaterBright"), Vector3.zero, new Vector3(0.05f, 0.30f, 0.05f),
                    new Vector3(0f, 0f, 14f));
            }

            // Bubbles, everywhere, always. Nothing sells "under water" faster.
            var column = Living(root, "BubbleColumn", AmbientMode.Drift, 1.55f, 0.32f, Vector3.up, stagger: true);
            for (var i = 0; i < 6; i++)
            {
                Ball(column, "Bubble", Mat("WaterBright"),
                    new Vector3(-0.52f + (i % 3) * 0.55f, 0.10f, -0.90f + i * 0.42f),
                    Vector3.one * (0.035f + (i % 3) * 0.012f));
            }
        }

        /// <summary>
        /// No ground. Three hull sections hang in a starfield with real gaps
        /// between them, and everything turns slowly. Take the platform away
        /// and the round is unmistakable from the silhouette alone.
        /// </summary>
        private static void OrbitBase(Transform root)
        {
            var stars = Living(root, "Starfield", AmbientMode.Spin, 2.2f, 0.03f, Vector3.up);
            for (var i = 0; i < 26; i++)
            {
                var a = i * 2.399963f;
                var r = 2.6f + (i % 5) * 0.22f;
                Ball(stars, "Star", Mat("Cream"),
                    new Vector3(Mathf.Cos(a) * r, -0.6f + (i % 7) * 0.42f, Mathf.Sin(a) * r + 0.4f),
                    Vector3.one * (0.020f + (i % 3) * 0.008f));
            }

            foreach (var (name, pos, size) in new[]
                     {
                         ("Module_Near", new Vector3(0f, -0.06f, -1.16f), new Vector3(1.15f, 0.22f, 1.05f)),
                         ("Module_Mid", new Vector3(0.05f, -0.06f, 0.16f), new Vector3(0.62f, 0.20f, 0.62f)),
                         ("Module_Far", new Vector3(0f, -0.06f, 1.28f), new Vector3(1.20f, 0.22f, 1.00f)),
                     })
            {
                var hull = Box(root, name, Mat("StoneLight"), pos, size);
                Round(hull);
                Box(root, $"{name}_Skirt", Mat("Stone"), pos + Vector3.down * 0.14f,
                    new Vector3(size.x * 0.86f, 0.10f, size.z * 0.86f));
                Box(root, $"{name}_Stripe", Mat("Accent"), pos + Vector3.up * 0.115f,
                    new Vector3(size.x * 0.94f, 0.012f, 0.06f));
            }

            foreach (var (x, z) in new[] { (-0.46f, -1.16f), (0.46f, -1.16f), (-0.48f, 1.28f), (0.48f, 1.28f) })
            {
                Box(root, "Handrail", Mat("Accent"), new Vector3(x, 0.16f, z), new Vector3(0.03f, 0.28f, 0.03f));
            }

            var ring = Living(root, "StationRing", AmbientMode.Spin, 26f, 0.05f, Vector3.forward);
            ring.localPosition = new Vector3(-1.32f, 1.05f, 1.90f);
            BlockRing(ring, "Ring", Mat("Stone"), Vector3.zero, new Vector2(0.34f, 0.34f), 12, 0.07f, 0.09f);
        }

        /// <summary>
        /// A heavy riveted deck with a glowing trough cut through it and a
        /// gantry overhead. It is lit from below, which no other world is.
        /// </summary>
        private static void ForgeBase(Transform root)
        {
            Box(root, "Platform", Mat("EarthDark"), new Vector3(0f, -0.16f, 0f), new Vector3(1.66f, 0.32f, 3.40f));
            Box(root, "BaseFoot", Mat("Ink"), new Vector3(0f, -0.35f, 0.03f), new Vector3(1.50f, 0.055f, 3.12f));

            foreach (var z in new[] { -1.10f, 1.14f })
            {
                Box(root, "Deck", Mat("Ink"), new Vector3(0f, 0.09f, z), new Vector3(1.50f, 0.18f, 1.06f));
                Box(root, "DeckPlate", Mat("Stone"), new Vector3(0f, 0.185f, z),
                    new Vector3(1.40f, 0.02f, 0.96f));
                for (var i = 0; i < 5; i++)
                {
                    Rod(root, "Rivet", Mat("StoneLight"), new Vector3(-0.60f + i * 0.30f, 0.20f, z),
                        new Vector3(0.045f, 0.012f, 0.045f));
                }
            }

            // The crucible trough, and the reason this world glows upward.
            Box(root, "Trough", Mat("Ink"), new Vector3(0f, 0.02f, 0.02f), new Vector3(1.52f, 0.14f, 1.08f));
            var melt = Living(root, "Melt", AmbientMode.Pulse, 0.05f, 0.31f, Vector3.up);
            Box(melt, "Molten", Mat("AccentDeep"), new Vector3(0f, 0.06f, 0.02f), new Vector3(1.36f, 0.05f, 0.92f));
            Box(melt, "MoltenHot", Mat("AccentLight"), new Vector3(0f, 0.08f, 0.02f),
                new Vector3(1.02f, 0.02f, 0.58f));
            Box(melt, "MoltenCore", Mat("Candle"), new Vector3(0f, 0.092f, 0.02f),
                new Vector3(0.66f, 0.01f, 0.30f));
            // A cool skin at the edges: molten metal that is uniformly bright
            // reads as a painted stripe rather than as something dangerous.
            foreach (var x in new[] { -0.60f, 0.60f })
            {
                Box(root, "TroughLip", Mat("EarthDark"), new Vector3(x, 0.10f, 0.02f),
                    new Vector3(0.16f, 0.10f, 1.02f));
            }

            foreach (var x in new[] { -0.76f, 0.76f })
            {
                Box(root, "GantryLeg", Mat("Stone"), new Vector3(x, 0.72f, 0.86f), new Vector3(0.09f, 1.30f, 0.11f));
            }

            Box(root, "GantryBeam", Mat("Stone"), new Vector3(0f, 1.32f, 0.86f), new Vector3(1.68f, 0.12f, 0.14f));
            Box(root, "GantryRail", Mat("Accent"), new Vector3(0f, 1.22f, 0.86f), new Vector3(1.56f, 0.04f, 0.05f));
        }

        /// <summary>
        /// Three rooftops at three heights with the city glowing underneath
        /// and a transit beam crossing the frame. The most vertical silhouette
        /// in the game, and the only one with a horizon of its own.
        /// </summary>
        private static void NeonBase(Transform root)
        {
            foreach (var (name, pos, size, top) in new[]
                     {
                         ("Block_Low", new Vector3(-0.02f, -0.55f, -1.18f), new Vector3(1.32f, 1.10f, 1.10f),
                             Mat("Violet")),
                         ("Block_Mid", new Vector3(0.30f, -0.24f, 0.28f), new Vector3(0.90f, 1.72f, 0.94f),
                             Mat("Ink")),
                         ("Block_High", new Vector3(-0.36f, 0.10f, 1.34f), new Vector3(0.92f, 2.30f, 1.02f),
                             Mat("Violet")),
                     })
            {
                Box(root, name, Mat("Ink"), pos, size);
                Box(root, $"{name}_Deck", top, pos + new Vector3(0f, size.y * 0.5f, 0f),
                    new Vector3(size.x + 0.06f, 0.09f, size.z + 0.06f));

                // Lit windows: the city has people in it. A full grid, not a
                // handful -- a tower with six windows is a dark box, and the
                // sparkle of many small lights is the whole look of the round.
                var columns = 5;
                var rows = Mathf.Max(4, Mathf.RoundToInt(size.y * 4.4f));
                for (var row = 0; row < rows; row++)
                {
                    for (var col = 0; col < columns; col++)
                    {
                        // Deterministic "some flats are empty": the same cells
                        // are dark in every render, so a screenshot is stable.
                        if ((row * 7 + col * 3) % 5 == 0) continue;

                        var swatch = (row + col) % 3;
                        var lit = swatch == 0 ? Mat("AccentLight")
                            : swatch == 1 ? Mat("WaterBright") : Mat("Candle");
                        var y = size.y * 0.5f - 0.20f - row * (size.y - 0.34f) / Mathf.Max(1, rows - 1);
                        var x = (col - (columns - 1) * 0.5f) * size.x * 0.19f;
                        Box(root, "Window", lit, pos + new Vector3(x, y, -size.z * 0.5f - 0.006f),
                            new Vector3(0.055f, 0.075f, 0.02f));
                        Box(root, "WindowSide", lit,
                            pos + new Vector3(-size.x * 0.5f - 0.006f, y, x * 0.9f),
                            new Vector3(0.02f, 0.075f, 0.055f));
                    }
                }
            }

            // Far skyline: silhouettes below the roofline, which is what gives
            // the round its horizon -- lit, so the horizon is a city and not a
            // row of teeth.
            for (var i = 0; i < 7; i++)
            {
                var height = 1.60f + (i % 3) * 0.40f;
                var tower = Child(root, $"FarTower_{i}");
                tower.localPosition = new Vector3(-1.35f + i * 0.46f, -0.90f + (i % 3) * 0.22f, 2.30f);
                Box(tower, "Shaft", Mat("Ink"), Vector3.zero, new Vector3(0.30f, height, 0.24f));
                for (var row = 0; row < 7; row++)
                {
                    if ((i * 3 + row * 2) % 4 == 0) continue;
                    var lit = (i + row) % 2 == 0 ? Mat("AccentDeep") : Mat("Water");
                    Box(tower, "FarWindow", lit,
                        new Vector3(((row % 2) - 0.5f) * 0.12f, height * 0.5f - 0.16f - row * 0.20f, -0.126f),
                        new Vector3(0.06f, 0.05f, 0.015f));
                }
                Box(tower, "Mast", Mat("Stone"), new Vector3(0f, height * 0.5f + 0.12f, 0f),
                    new Vector3(0.03f, 0.24f, 0.03f));
            }

            var glow = Living(root, "CityGlow", AmbientMode.Pulse, 0.04f, 0.17f, Vector3.up);
            Box(glow, "Haze", Mat("Violet"), new Vector3(0f, -1.30f, 1.60f), new Vector3(3.00f, 0.10f, 1.60f));
            Box(glow, "HazeWarm", Mat("AccentDeep"), new Vector3(0f, -1.72f, 2.05f),
                new Vector3(3.20f, 0.26f, 0.10f));

            Box(root, "TransitBeam", Mat("Stone"), new Vector3(0.10f, 0.62f, 0.62f),
                new Vector3(2.60f, 0.09f, 0.16f), new Vector3(0f, 12f, 0f));
            Box(root, "TransitRail", Mat("WaterBright"), new Vector3(0.10f, 0.68f, 0.62f),
                new Vector3(2.58f, 0.02f, 0.05f), new Vector3(0f, 12f, 0f));
        }

        // -------------------------------------------------------------------
        // Dressing — the small always-moving things that make a place a place
        // -------------------------------------------------------------------

        private static void Dress(string world, Transform root)
        {
            switch (world)
            {
                case Garden:
                    Bush(root, new Vector3(-0.58f, 0.17f, 1.43f), 0.11f);
                    Bush(root, new Vector3(0.61f, 0.17f, 1.18f), 0.09f);
                    Flower(root, new Vector3(-0.58f, 0.16f, 0.72f), Mat("AccentLight"));
                    Flower(root, new Vector3(0.60f, 0.16f, 0.82f), Mat("Accent"));
                    Grass(root, new Vector3(-0.62f, 0.15f, -0.18f), 5, 0.9f);
                    Grass(root, new Vector3(0.63f, 0.15f, 0.28f), 4, 1.1f);
                    break;

                case Clock:
                    // An escapement that never stops, so the courtyard is
                    // audibly and visibly a mechanism at rest rather than idle.
                    {
                        var pendulum = Living(root, "Pendulum", AmbientMode.Sway, 13f, 0.42f, Vector3.forward);
                        pendulum.localPosition = new Vector3(-0.62f, 1.26f, 0.62f);
                        Box(pendulum, "Rod", Mat("AccentLight"), new Vector3(0f, -0.28f, 0f),
                            new Vector3(0.022f, 0.56f, 0.022f));
                        Rod(pendulum, "Bob", Mat("Accent"), new Vector3(0f, -0.58f, 0f),
                            new Vector3(0.16f, 0.02f, 0.16f), new Vector3(90f, 0f, 0f));

                        var idler = Living(root, "IdlerCog", AmbientMode.Spin, 34f, 0.20f, Vector3.forward);
                        idler.localPosition = new Vector3(0.66f, 1.10f, 0.62f);
                        Cog(idler, "Idler", Mat("Accent"), Mat("AccentLight"), Vector3.zero, 0.15f, 9);
                    }
                    break;

                case Weather:
                    Flower(root, new Vector3(-0.56f, 0.38f, -0.20f), Mat("Accent"));
                    Flower(root, new Vector3(0.58f, 0.38f, 0.05f), Mat("AccentLight"));
                    Grass(root, new Vector3(-0.60f, 0.16f, -1.34f), 5, 1.2f);
                    Pine(root, new Vector3(0.56f, 0.62f, 1.40f), 0.55f);
                    break;

                case Canyon:
                    // Dust rising out of the chasm on the thermal, and a bird
                    // riding it. Both read instantly at thumbnail size.
                    {
                        var dust = Living(root, "CanyonDust", AmbientMode.Drift, 1.40f, 0.20f, Vector3.up,
                            stagger: true);
                        for (var i = 0; i < 5; i++)
                        {
                            Ball(dust, "Mote", Mat("Sand"),
                                new Vector3(-0.50f + i * 0.26f, -0.30f, -0.18f + (i % 3) * 0.18f),
                                Vector3.one * (0.030f + (i % 2) * 0.014f));
                        }

                        var bird = Living(root, "Bird", AmbientMode.Spin, 24f, 0.06f, Vector3.up);
                        bird.localPosition = new Vector3(0f, 1.34f, 0.30f);
                        var wing = Living(bird, "Wings", AmbientMode.Sway, 16f, 1.5f, Vector3.forward);
                        wing.localPosition = new Vector3(1.05f, 0f, 0f);
                        Box(wing, "WingL", Mat("Ink"), new Vector3(-0.07f, 0f, 0f),
                            new Vector3(0.16f, 0.014f, 0.05f), new Vector3(0f, 0f, 16f));
                        Box(wing, "WingR", Mat("Ink"), new Vector3(0.07f, 0f, 0f),
                            new Vector3(0.16f, 0.014f, 0.05f), new Vector3(0f, 0f, -16f));
                    }
                    break;

                case Tide:
                    {
                        var gull = Living(root, "Gull", AmbientMode.Spin, 20f, 0.05f, Vector3.up);
                        gull.localPosition = new Vector3(0f, 1.20f, 0.90f);
                        var glide = Living(gull, "Glide", AmbientMode.Bob, 0.05f, 0.5f, Vector3.up);
                        glide.localPosition = new Vector3(1.30f, 0f, 0f);
                        Box(glide, "Body", Mat("Cream"), Vector3.zero, new Vector3(0.13f, 0.035f, 0.05f));
                        Box(glide, "Wing", Mat("Cream"), Vector3.zero, new Vector3(0.30f, 0.012f, 0.04f),
                            new Vector3(0f, 0f, 8f));

                        Rod(root, "Piling", Mat("WoodDark"), new Vector3(-0.86f, 0.16f, -0.40f),
                            new Vector3(0.10f, 0.34f, 0.10f));
                        Rod(root, "Piling", Mat("WoodDark"), new Vector3(0.90f, 0.14f, 0.62f),
                            new Vector3(0.10f, 0.30f, 0.10f));
                    }
                    break;

                case Storm:
                    {
                        // Rain across the whole frame, plus a lightning flash
                        // on a long duty cycle so a screenshot usually catches
                        // the dark and occasionally catches the strike.
                        var rain = Living(root, "Rain", AmbientMode.Drift, -1.70f, 0.75f,
                            new Vector3(0.22f, -1f, 0f), stagger: true);
                        for (var i = 0; i < 12; i++)
                        {
                            Box(rain, "Drop", Mat("WaterBright"),
                                new Vector3(-0.62f + (i % 6) * 0.24f, 1.55f, -1.05f + (i / 6) * 1.30f + (i % 3) * 0.30f),
                                new Vector3(0.014f, 0.16f, 0.014f), new Vector3(0f, 0f, -12f));
                        }

                        var flash = Living(root, "SkyFlash", AmbientMode.Flicker, 0.965f, 0.14f, Vector3.up);
                        Box(flash, "Bolt", Mat("Candle"), new Vector3(0.78f, 1.55f, 2.20f),
                            new Vector3(0.05f, 1.30f, 0.05f), new Vector3(0f, 0f, 12f));
                    }
                    break;

                case Cave:
                    {
                        var drips = Living(root, "Drips", AmbientMode.Drift, -1.15f, 0.40f, Vector3.up,
                            stagger: true);
                        for (var i = 0; i < 4; i++)
                        {
                            Ball(drips, "Drop", Mat("WaterBright"),
                                new Vector3(-0.42f + i * 0.32f, 1.16f, -0.55f + i * 0.62f),
                                Vector3.one * 0.026f);
                        }

                        foreach (var (x, y, z, s) in new[]
                                 {
                                     (-0.66f, 0.20f, -0.70f, 0.9f), (0.68f, 0.24f, 0.50f, 1.1f),
                                     (-0.62f, 0.18f, 1.20f, 0.7f),
                                 })
                        {
                            var vein = Living(root, "Vein", AmbientMode.Pulse, 0.12f, 0.18f, Vector3.up, phase: s);
                            vein.localPosition = new Vector3(x, y, z);
                            Ball(vein, "Crystal", Mat("WaterBright"), Vector3.zero,
                                new Vector3(0.07f * s, 0.16f * s, 0.07f * s), new Vector3(0f, 0f, 12f));
                        }
                    }
                    break;

                case Peak:
                    {
                        var spindrift = Living(root, "Spindrift", AmbientMode.Drift, 1.35f, 0.55f,
                            new Vector3(-1f, 0.18f, 0f), stagger: true);
                        for (var i = 0; i < 7; i++)
                        {
                            Ball(spindrift, "Flake", Mat("Cream"),
                                new Vector3(0.62f, 0.55f + (i % 4) * 0.28f, -0.90f + i * 0.42f),
                                Vector3.one * (0.022f + (i % 3) * 0.008f));
                        }

                        Box(root, "MarkerPole", Mat("Ink"), new Vector3(-0.58f, 0.92f, 1.30f),
                            new Vector3(0.026f, 0.42f, 0.026f));
                        var flag = Living(root, "Flag", AmbientMode.Sway, 22f, 1.6f, Vector3.up);
                        flag.localPosition = new Vector3(-0.58f, 1.08f, 1.30f);
                        Box(flag, "Cloth", Mat("Accent"), new Vector3(0.09f, 0f, 0f),
                            new Vector3(0.17f, 0.10f, 0.014f));
                    }
                    break;

                case Abyss:
                    {
                        foreach (var (x, z, h) in new[] { (-0.52f, 1.30f, 0.9f), (0.54f, -0.95f, 1.15f) })
                        {
                            var kelp = Living(root, "Kelp", AmbientMode.Sway, 9f, 0.22f, Vector3.forward,
                                phase: h);
                            kelp.localPosition = new Vector3(x, 0.06f, z);
                            for (var i = 0; i < 4; i++)
                            {
                                Box(kelp, "Frond", Mat("FoliageDark"),
                                    new Vector3(0f, 0.16f + i * 0.28f, 0f),
                                    new Vector3(0.05f, 0.16f * h, 0.03f), new Vector3(0f, 0f, i % 2 == 0 ? 7f : -7f));
                            }
                        }

                        var fish = Living(root, "Fish", AmbientMode.Spin, 18f, 0.055f, Vector3.up);
                        fish.localPosition = new Vector3(0f, 0.86f, 0.40f);
                        Box(fish, "Body", Mat("WaterBright"), new Vector3(0.70f, 0f, 0f),
                            new Vector3(0.11f, 0.05f, 0.04f));
                    }
                    break;

                case Orbit:
                    {
                        var tumbler = Living(root, "Debris", AmbientMode.Spin, 40f, 0.14f,
                            new Vector3(0.4f, 1f, 0.2f));
                        tumbler.localPosition = new Vector3(-0.72f, 0.72f, -0.30f);
                        Box(tumbler, "Bolt", Mat("Stone"), Vector3.zero, new Vector3(0.10f, 0.03f, 0.03f));

                        foreach (var (x, z) in new[] { (-0.44f, 0.16f), (0.52f, 1.28f) })
                        {
                            var beacon = Living(root, "Beacon", AmbientMode.Flicker, 0.72f, 0.6f, Vector3.up);
                            Ball(beacon, "Lamp", Mat("AccentLight"), new Vector3(x, 0.14f, z), Vector3.one * 0.05f);
                        }
                    }
                    break;

                case Forge:
                    {
                        foreach (var (x, z, phase) in new[] { (-0.70f, 1.42f, 0f), (0.72f, -1.40f, 1.3f) })
                        {
                            Rod(root, "Pipe", Mat("Stone"), new Vector3(x, 0.36f, z),
                                new Vector3(0.10f, 0.24f, 0.10f));
                            var steam = Living(root, "Steam", AmbientMode.Drift, 0.62f, 0.42f, Vector3.up,
                                stagger: true, phase: phase);
                            for (var i = 0; i < 3; i++)
                            {
                                Ball(steam, "Puff", Mat("StoneLight"),
                                    new Vector3(x, 0.62f + i * 0.02f, z), Vector3.one * (0.07f + i * 0.02f));
                            }
                        }

                        var trolley = Living(root, "CraneTrolley", AmbientMode.Bob, 0.42f, 0.11f, Vector3.right);
                        trolley.localPosition = new Vector3(-0.30f, 1.22f, 0.86f);
                        Box(trolley, "Body", Mat("Accent"), Vector3.zero, new Vector3(0.16f, 0.10f, 0.13f));
                        Box(trolley, "Hook", Mat("StoneLight"), new Vector3(0f, -0.16f, 0f),
                            new Vector3(0.02f, 0.22f, 0.02f));
                    }
                    break;

                case Neon:
                    {
                        // Far enough back to be skyline rather than stage: the
                        // transit round stages its own tram on the near beam.
                        var tram = Living(root, "SkylineTram", AmbientMode.Drift, -2.90f, 0.11f, Vector3.right);
                        tram.localPosition = new Vector3(1.50f, 0.22f, 2.05f);
                        Box(tram, "Car", Mat("StoneLight"), Vector3.zero, new Vector3(0.34f, 0.11f, 0.12f));
                        Box(tram, "Windows", Mat("WaterBright"), new Vector3(0f, 0.015f, -0.065f),
                            new Vector3(0.28f, 0.05f, 0.02f));

                        foreach (var (x, y, z, mat, duty) in new[]
                                 {
                                     (-0.86f, 0.86f, -0.60f, Mat("AccentLight"), 0.86f),
                                     (0.90f, 1.16f, 0.90f, Mat("WaterBright"), 0.92f),
                                     (-0.90f, 1.42f, 1.60f, Mat("AccentDeep"), 0.80f),
                                 })
                        {
                            var sign = Living(root, "SideSign", AmbientMode.Flicker, duty, 0.9f, Vector3.up);
                            Box(sign, "Tube", mat, new Vector3(x, y, z), new Vector3(0.05f, 0.34f, 0.05f));
                        }
                    }
                    break;
            }
        }

        private static void Bush(Transform root, Vector3 position, float size)
        {
            var holder = Living(root, "Bush", AmbientMode.Sway, 3.2f, 0.30f, Vector3.forward);
            holder.localPosition = position;
            for (var i = -1; i <= 1; i++)
            {
                Ball(holder, "Puff", i == 0 ? Mat("FoliageDark") : Mat("Foliage"),
                    new Vector3(i * size * 0.60f, Mathf.Abs(i) * -0.01f, 0f),
                    new Vector3(size, size * 0.75f, size * 0.85f));
            }
        }

        private static void Flower(Transform root, Vector3 position, Material blossom)
        {
            var holder = Living(root, "Flower", AmbientMode.Sway, 5f, 0.42f, Vector3.forward);
            holder.localPosition = position;
            Rod(holder, "Stem", Mat("FoliageDark"), Vector3.up * 0.045f, new Vector3(0.010f, 0.045f, 0.010f));
            Ball(holder, "Head", blossom, Vector3.up * 0.095f, new Vector3(0.045f, 0.032f, 0.045f));
        }

        private static void Grass(Transform root, Vector3 position, int blades, float speed)
        {
            var holder = Living(root, "GrassTuft", AmbientMode.Sway, 7f, 0.34f * speed, Vector3.forward);
            holder.localPosition = position;
            for (var i = 0; i < blades; i++)
            {
                Box(holder, "Blade", i % 2 == 0 ? Mat("Foliage") : Mat("FoliageBright"),
                    new Vector3(-0.05f + i * 0.025f, 0.045f, 0f),
                    new Vector3(0.012f, 0.09f, 0.012f), new Vector3(0f, 0f, -8f + i * 4f));
            }
        }

        private static void Pine(Transform root, Vector3 position, float scale)
        {
            Rod(root, "PineTrunk", Mat("WoodDark"), position + Vector3.up * (0.12f * scale),
                new Vector3(0.035f * scale, 0.12f * scale, 0.035f * scale));
            for (var i = 0; i < 3; i++)
            {
                var width = (0.24f - i * 0.045f) * scale;
                Ball(root, "PineCrown", i % 2 == 0 ? Mat("FoliageDark") : Mat("Foliage"),
                    position + Vector3.up * ((0.19f + i * 0.08f) * scale),
                    new Vector3(width, 0.12f * scale, width));
            }
        }

        // -------------------------------------------------------------------
        // Atmosphere — light, sky, haze, framing and the bed
        // -------------------------------------------------------------------

        /// <summary>
        /// Round three is the exception the world rule is built on: its three
        /// skies deliberately differ, because "the weather changes" *is* what
        /// that round teaches. Every other world blends to one mood.
        /// </summary>
        internal static string WeatherTint;

        private static void Atmosphere(string world, GameObject root)
        {
            var a = root.AddComponent<DioramaAtmosphere>();
            a.WorldId = world;

            switch (world)
            {
                case Garden:
                    a.Sky = Hex("B8E6F5");
                    a.AmbientSky = Hex("B8E6F5");
                    a.AmbientEquator = Hex("F7F3E8");
                    a.AmbientGround = Hex("E8DCC8");
                    a.SunColor = Hex("FFF3CE");
                    a.SunIntensity = 1.15f;
                    a.SunAngles = new Vector3(50f, -35f, 0f);
                    a.FillColor = Hex("BCEAF5");
                    a.FillIntensity = 0.26f;
                    a.CameraPitch = 40f;
                    a.CameraDistance = 6.3f;
                    a.CameraHeight = 0.10f;
                    a.CameraFov = 30f;
                    a.Ambience = "amb_garden";
                    a.AmbienceVolume = 0.26f;
                    break;

                case Clock:
                    a.Sky = Hex("CDEBF7");
                    a.AmbientSky = Hex("CDEBF7");
                    a.AmbientEquator = Hex("E8DCC8");
                    a.AmbientGround = Hex("B08F6C");
                    a.SunColor = Hex("F7F3E8");
                    a.SunIntensity = 1.05f;
                    a.SunAngles = new Vector3(58f, -22f, 0f);
                    a.FillColor = Hex("FFDE8A");
                    a.FillIntensity = 0.34f;
                    a.UseFog = true;
                    a.Fog = Hex("CDEBF7");
                    a.FogDensity = 0.020f;
                    a.CameraPitch = 38f;
                    a.CameraDistance = 6.05f;
                    a.CameraHeight = 0.22f;
                    a.CameraFov = 30f;
                    a.Ambience = "amb_clock";
                    a.AmbienceVolume = 0.24f;
                    break;

                case Weather:
                    // Filled in by the stage through WeatherTint, then reset,
                    // so a stage can never accidentally inherit the last one.
                    ApplyWeather(a, WeatherTint);
                    WeatherTint = null;
                    a.CameraPitch = 42f;
                    a.CameraDistance = 6.5f;
                    a.CameraHeight = 0.30f;
                    a.CameraFov = 30f;
                    a.Ambience = "amb_weather";
                    a.AmbienceVolume = 0.30f;
                    break;

                case Canyon:
                    // Warm rock against a cold sky. With a gold sky the mesas
                    // sat in the same value and hue family as the air behind
                    // them and the whole round read as a chest of drawers; the
                    // complementary split is what makes it outdoors.
                    a.Sky = Hex("8FD6F9");
                    a.AmbientSky = Hex("8FD6F9");
                    a.AmbientEquator = Hex("FFDE8A");
                    a.AmbientGround = Hex("9C6748");
                    a.SunColor = Hex("FFF3CE");
                    a.SunIntensity = 1.34f;
                    a.SunAngles = new Vector3(24f, -52f, 0f);
                    a.FillColor = Hex("6FC0E3");
                    a.FillIntensity = 0.34f;
                    a.UseFog = true;
                    a.Fog = Hex("8FD6F9");
                    a.FogDensity = 0.055f;
                    // Steeper than any other outdoor world. At 39 degrees the
                    // near rim hid the chasm floor entirely and the two mesas
                    // read as one step up; you have to be able to see down the
                    // hole for it to be a hole.
                    a.CameraPitch = 45f;
                    a.CameraDistance = 6.5f;
                    a.CameraHeight = 0.26f;
                    a.CameraFov = 31f;
                    a.Ambience = "amb_canyon";
                    a.AmbienceVolume = 0.32f;
                    break;

                case Tide:
                    a.Sky = Hex("8FD6F9");
                    a.AmbientSky = Hex("8FD6F9");
                    a.AmbientEquator = Hex("CDEBF7");
                    a.AmbientGround = Hex("E8DCC8");
                    a.SunColor = Hex("FFF3CE");
                    a.SunIntensity = 1.16f;
                    a.SunAngles = new Vector3(50f, -24f, 0f);
                    a.FillColor = Hex("5FB7D4");
                    a.FillIntensity = 0.34f;
                    a.UseFog = true;
                    a.Fog = Hex("CDEBF7");
                    a.FogDensity = 0.022f;
                    a.CameraPitch = 36f;
                    a.CameraDistance = 6.85f;
                    a.CameraHeight = 0.06f;
                    a.CameraFov = 31f;
                    a.Ambience = "amb_tide";
                    a.AmbienceVolume = 0.30f;
                    break;

                case Storm:
                    a.Sky = Hex("3D3354");
                    a.AmbientSky = Hex("57406B");
                    a.AmbientEquator = Hex("3D3354");
                    a.AmbientGround = Hex("221D33");
                    a.SunColor = Hex("8E8BA7");
                    a.SunIntensity = 0.58f;
                    a.SunAngles = new Vector3(28f, 24f, 0f);
                    a.FillColor = Hex("8FD6F9");
                    a.FillIntensity = 0.42f;
                    a.UseFog = true;
                    a.Fog = Hex("3D3354");
                    a.FogDensity = 0.095f;
                    a.CameraPitch = 31f;
                    a.CameraDistance = 6.15f;
                    a.CameraHeight = 0.34f;
                    a.CameraFov = 32f;
                    a.Ambience = "amb_storm";
                    a.AmbienceVolume = 0.34f;
                    break;

                case Cave:
                    a.Sky = Hex("221D33");
                    a.AmbientSky = Hex("8E8BA7");
                    a.AmbientEquator = Hex("57406B");
                    a.AmbientGround = Hex("3D3354");
                    a.SunColor = Hex("8FD6F9");
                    a.SunIntensity = 1.02f;
                    a.SunAngles = new Vector3(62f, -12f, 0f);
                    // Lantern-warm from below and in front: the cave has to be
                    // dark enough to be a cave and light enough to be a puzzle.
                    a.FillColor = Hex("FFCF56");
                    a.FillIntensity = 0.66f;
                    a.FillAngles = new Vector3(12f, -170f, 0f);
                    a.UseFog = true;
                    a.Fog = Hex("221D33");
                    a.FogDensity = 0.045f;
                    // Wider and further back than any other world: this is the
                    // only stage with a ceiling, and the ceiling has to be in
                    // shot or the enclosure does not read.
                    a.CameraPitch = 39f;
                    a.CameraDistance = 6.75f;
                    a.CameraHeight = 0.44f;
                    a.CameraFov = 34f;
                    a.Ambience = "amb_cave";
                    a.AmbienceVolume = 0.30f;
                    break;

                case Peak:
                    a.Sky = Hex("CDEBF7");
                    a.AmbientSky = Hex("CDEBF7");
                    a.AmbientEquator = Hex("FFF3CE");
                    a.AmbientGround = Hex("5FB7D4");
                    a.SunColor = Hex("FFF3CE");
                    a.SunIntensity = 1.06f;
                    a.SunAngles = new Vector3(44f, -62f, 0f);
                    a.FillColor = Hex("5FB7D4");
                    a.FillIntensity = 0.46f;
                    a.UseFog = true;
                    a.Fog = Hex("CDEBF7");
                    a.FogDensity = 0.034f;
                    a.CameraPitch = 36f;
                    a.CameraDistance = 6.45f;
                    a.CameraHeight = 0.40f;
                    a.CameraFov = 30f;
                    a.Ambience = "amb_peak";
                    a.AmbienceVolume = 0.32f;
                    break;

                case Abyss:
                    a.Sky = Hex("221D33");
                    a.AmbientSky = Hex("3D3354");
                    a.AmbientEquator = Hex("5FB7D4");
                    a.AmbientGround = Hex("221D33");
                    a.SunColor = Hex("8FD6F9");
                    a.SunIntensity = 0.62f;
                    a.SunAngles = new Vector3(74f, -8f, 0f);
                    a.FillColor = Hex("5FB7D4");
                    a.FillIntensity = 0.34f;
                    a.UseFog = true;
                    a.Fog = Color.Lerp(Hex("221D33"), Hex("5FB7D4"), 0.34f);
                    a.FogDensity = 0.165f;
                    a.CameraPitch = 40f;
                    a.CameraDistance = 6.0f;
                    a.CameraHeight = 0.42f;
                    a.CameraFov = 29f;
                    a.Ambience = "amb_abyss";
                    a.AmbienceVolume = 0.34f;
                    break;

                case Orbit:
                    a.Sky = Hex("221D33");
                    a.AmbientSky = Hex("221D33");
                    a.AmbientEquator = Hex("3D3354");
                    a.AmbientGround = Hex("221D33");
                    a.SunColor = Hex("FFFFFF");
                    a.SunIntensity = 1.62f;
                    a.SunAngles = new Vector3(18f, -70f, 0f);
                    // Almost no fill: vacuum has nothing to bounce off, and the
                    // hard black shadow side is the whole look of the round.
                    a.FillColor = Hex("57406B");
                    a.FillIntensity = 0.10f;
                    a.UseFog = false;
                    a.CameraPitch = 30f;
                    a.CameraDistance = 7.2f;
                    a.CameraHeight = 0.24f;
                    a.CameraFov = 33f;
                    a.Ambience = "amb_orbit";
                    a.AmbienceVolume = 0.22f;
                    break;

                case Forge:
                    a.Sky = Hex("3D3354");
                    a.AmbientSky = Hex("3D3354");
                    a.AmbientEquator = Hex("57402D");
                    // Lit from the floor: the crucible is under the deck.
                    a.AmbientGround = Hex("FFB53E");
                    a.SunColor = Hex("C3C0D5");
                    a.SunIntensity = 0.72f;
                    a.SunAngles = new Vector3(64f, -30f, 0f);
                    a.FillColor = Hex("FFB53E");
                    a.FillIntensity = 0.46f;
                    a.FillAngles = new Vector3(-30f, 160f, 0f);
                    a.UseFog = true;
                    a.Fog = Color.Lerp(Hex("3D3354"), Hex("E8B62D"), 0.22f);
                    a.FogDensity = 0.085f;
                    a.CameraPitch = 42f;
                    a.CameraDistance = 6.1f;
                    a.CameraHeight = 0.36f;
                    a.CameraFov = 29f;
                    a.Ambience = "amb_forge";
                    a.AmbienceVolume = 0.30f;
                    break;

                case Neon:
                    a.Sky = Hex("221D33");
                    a.AmbientSky = Hex("6FC0E3");
                    a.AmbientEquator = Hex("57406B");
                    a.AmbientGround = Hex("B27A58");
                    a.SunColor = Hex("8FD6F9");
                    a.SunIntensity = 0.96f;
                    a.SunAngles = new Vector3(34f, 42f, 0f);
                    // Sodium-orange bounce off the street, cold blue key from
                    // the signs: neon is a two-colour world or it is just night.
                    a.FillColor = Hex("FFB53E");
                    a.FillIntensity = 0.86f;
                    a.FillAngles = new Vector3(18f, -140f, 0f);
                    a.UseFog = true;
                    a.Fog = Color.Lerp(Hex("221D33"), Hex("57406B"), 0.5f);
                    a.FogDensity = 0.050f;
                    a.CameraPitch = 32f;
                    a.CameraDistance = 6.9f;
                    a.CameraHeight = 0.52f;
                    a.CameraFov = 32f;
                    a.Ambience = "amb_neon";
                    a.AmbienceVolume = 0.28f;
                    break;
            }
        }

        private static void ApplyWeather(DioramaAtmosphere a, string tint)
        {
            switch (tint)
            {
                case "frost":
                    a.Sky = Hex("CDEBF7");
                    a.AmbientSky = Hex("CDEBF7");
                    a.AmbientEquator = Hex("F7F3E8");
                    a.AmbientGround = Hex("8FD6F9");
                    a.SunColor = Hex("CDEBF7");
                    a.SunIntensity = 0.98f;
                    a.SunAngles = new Vector3(38f, -48f, 0f);
                    a.FillColor = Hex("8FD6F9");
                    a.FillIntensity = 0.40f;
                    a.UseFog = true;
                    a.Fog = Hex("CDEBF7");
                    a.FogDensity = 0.055f;
                    break;

                case "sun":
                    a.Sky = Hex("FFF3CE");
                    a.AmbientSky = Hex("FFDE8A");
                    a.AmbientEquator = Hex("FFF3CE");
                    a.AmbientGround = Hex("A9D488");
                    a.SunColor = Hex("FFDE8A");
                    a.SunIntensity = 1.42f;
                    a.SunAngles = new Vector3(54f, -28f, 0f);
                    a.FillColor = Hex("A9D488");
                    a.FillIntensity = 0.30f;
                    break;

                default: // "rain"
                    a.Sky = Hex("8E8BA7");
                    a.AmbientSky = Hex("8E8BA7");
                    a.AmbientEquator = Hex("C3C0D5");
                    a.AmbientGround = Hex("557F50");
                    a.SunColor = Hex("C3C0D5");
                    a.SunIntensity = 0.72f;
                    a.SunAngles = new Vector3(36f, 18f, 0f);
                    a.FillColor = Hex("8FD6F9");
                    a.FillIntensity = 0.34f;
                    a.UseFog = true;
                    a.Fog = Hex("8E8BA7");
                    a.FogDensity = 0.070f;
                    break;
            }
        }
    }
}
