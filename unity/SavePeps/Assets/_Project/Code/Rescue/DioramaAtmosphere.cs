using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// A world's light, air, framing and sound bed, authored onto the diorama
    /// prefab itself.
    ///
    /// Before this existed the whole game shared one sky colour, one sun and
    /// one camera, set once in the Game scene. Twelve rounds that were meant
    /// to be twelve worlds therefore all rendered against the same pale blue
    /// noon, and the only thing separating a deep ocean trench from a neon
    /// rooftop was the colour of the boxes on the platform. Atmosphere is not
    /// decoration here: it is the first thing that answers "which round is
    /// this?" from a single screenshot.
    ///
    /// It lives on the environment prefab rather than in a table keyed by
    /// rescue id, so it stays data a designer edits next to the geometry it
    /// belongs to, and no runtime code ever learns the name of a round.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DioramaAtmosphere : MonoBehaviour
    {
        [Header("Identity")]
        [Tooltip("The world this stage belongs to: garden, clock, cave. Every rescue in a round " +
                 "must name the same world, and no two rounds may share one.")]
        public string WorldId = "garden";

        [Header("Sky and air")]
        [Tooltip("Camera clear colour — the sky behind the floating platform.")]
        public Color Sky = new(0.722f, 0.902f, 0.961f);

        public Color AmbientSky = new(0.722f, 0.902f, 0.961f);
        public Color AmbientEquator = new(0.969f, 0.953f, 0.910f);
        public Color AmbientGround = new(0.910f, 0.863f, 0.784f);

        [Tooltip("Depth haze. Off for the vacuum of orbit, heavy for a canyon or a trench.")]
        public bool UseFog;

        public Color Fog = new(0.722f, 0.902f, 0.961f);

        [Range(0f, 0.4f)] public float FogDensity = 0.06f;

        [Header("Key light")]
        public Color SunColor = new(1f, 0.953f, 0.808f);

        [Range(0f, 3f)] public float SunIntensity = 1.15f;

        public Vector3 SunAngles = new(50f, -35f, 0f);

        [Header("Fill light")]
        [Tooltip("The cool bounce that keeps unlit primitive faces from collapsing into one flat colour.")]
        public Color FillColor = new(0.737f, 0.918f, 0.961f);

        [Range(0f, 2f)] public float FillIntensity = 0.26f;

        public Vector3 FillAngles = new(35f, 145f, 0f);

        [Header("Framing")]
        [Tooltip("Downward camera tilt in degrees. 40 is the house default that reads as a toy on a table; " +
                 "past about 48 the silhouettes flatten out.")]
        [Range(24f, 50f)] public float CameraPitch = 40f;

        [Tooltip("Camera distance from the diorama centre.")]
        [Range(3.5f, 9f)] public float CameraDistance = 6.3f;

        [Tooltip("Height of the point the camera orbits, which is what raises or lowers the horizon.")]
        [Range(-0.6f, 1.2f)] public float CameraHeight = 0.1f;

        [Range(18f, 42f)] public float CameraFov = 30f;

        [Header("Sound")]
        [Tooltip("Looping ambience bed id, or empty for silence. See ToyAudioBank.")]
        public string Ambience = "";

        [Tooltip("Reverb-ish tail applied to this world's cues by choosing longer sfx variants. " +
                 "Purely informational for now; the bed carries the character.")]
        [Range(0f, 1f)] public float AmbienceVolume = 0.35f;

        /// <summary>
        /// The framing the fixed camera should hold for this world, in the
        /// diorama's own space. Returned rather than applied so the one
        /// component that owns the camera stays the only writer.
        /// </summary>
        public void Framing(out Vector3 position, out Quaternion rotation, out float fov)
        {
            var pitch = CameraPitch * Mathf.Deg2Rad;
            position = new Vector3(
                0f,
                CameraHeight + CameraDistance * Mathf.Sin(pitch),
                -CameraDistance * Mathf.Cos(pitch));
            rotation = Quaternion.Euler(CameraPitch, 0f, 0f);
            fov = CameraFov;
        }
    }
}
