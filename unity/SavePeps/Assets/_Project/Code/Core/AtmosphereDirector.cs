using SavePeps.Rescue;
using UnityEngine;

namespace SavePeps.Core
{
    /// <summary>
    /// The one writer of scene-wide light, sky, haze, framing and ambience.
    ///
    /// Twelve worlds cannot read as twelve worlds while the sky, the sun and
    /// the camera are constants baked into the Game scene. This takes the
    /// <see cref="DioramaAtmosphere"/> a staged environment carries and blends
    /// the scene to it; when nothing is staged it blends back to the values
    /// the scene builder authored, which is what the home tableau wants.
    ///
    /// It is a *director*, not a per-rescue behaviour: it never learns a
    /// rescue id, and a new world costs nothing here.
    /// </summary>
    public sealed class AtmosphereDirector : MonoBehaviour
    {
        /// <summary>
        /// Long enough to read as weather changing rather than a cut, short
        /// enough to finish inside the 0.42s diorama entrance.
        /// </summary>
        private const float BlendDuration = 0.38f;

        [SerializeField] private Camera _camera;
        [SerializeField] private Light _sun;
        [SerializeField] private Light _fill;
        [SerializeField] private GameFeel _gameFeel;
        [SerializeField] private Feedback _feedback;

        /// <summary>Everything that can be blended, in one struct so the lerp is one function.</summary>
        private struct Mood
        {
            public Color Sky, AmbientSky, AmbientEquator, AmbientGround, Fog, SunColor, FillColor;
            public float FogDensity, SunIntensity, FillIntensity, Fov;
            public Vector3 SunAngles, FillAngles, CameraPosition;
            public Quaternion CameraRotation;
            public bool UseFog;

            public static Mood Lerp(Mood a, Mood b, float t) => new()
            {
                Sky = Color.Lerp(a.Sky, b.Sky, t),
                AmbientSky = Color.Lerp(a.AmbientSky, b.AmbientSky, t),
                AmbientEquator = Color.Lerp(a.AmbientEquator, b.AmbientEquator, t),
                AmbientGround = Color.Lerp(a.AmbientGround, b.AmbientGround, t),
                Fog = Color.Lerp(a.Fog, b.Fog, t),
                SunColor = Color.Lerp(a.SunColor, b.SunColor, t),
                FillColor = Color.Lerp(a.FillColor, b.FillColor, t),
                // Fog fades to zero density on whichever side does not use it,
                // so entering orbit's vacuum from a hazy canyon reads as the
                // haze clearing rather than as haze snapping off.
                FogDensity = Mathf.Lerp(a.UseFog ? a.FogDensity : 0f, b.UseFog ? b.FogDensity : 0f, t),
                SunIntensity = Mathf.Lerp(a.SunIntensity, b.SunIntensity, t),
                FillIntensity = Mathf.Lerp(a.FillIntensity, b.FillIntensity, t),
                Fov = Mathf.Lerp(a.Fov, b.Fov, t),
                SunAngles = Vector3.Lerp(a.SunAngles, b.SunAngles, t),
                FillAngles = Vector3.Lerp(a.FillAngles, b.FillAngles, t),
                CameraPosition = Vector3.Lerp(a.CameraPosition, b.CameraPosition, t),
                CameraRotation = Quaternion.Slerp(a.CameraRotation, b.CameraRotation, t),
                UseFog = a.UseFog || b.UseFog,
            };
        }

        private Mood _default;
        private Mood _from;
        private Mood _to;
        private float _clock = 1f;
        private string _ambience = "";
        private bool _captured;

        private void Awake() => Capture();

        /// <summary>
        /// Reads the scene's authored lighting as the resting mood. Lazily,
        /// because <see cref="Apply"/> can arrive from another component's
        /// Awake and Unity gives no ordering guarantee between the two.
        /// </summary>
        private void Capture()
        {
            if (_captured) return;
            _captured = true;

            if (_camera == null) _camera = Camera.main;

            _default = new Mood
            {
                Sky = _camera != null ? _camera.backgroundColor : Color.grey,
                AmbientSky = RenderSettings.ambientSkyColor,
                AmbientEquator = RenderSettings.ambientEquatorColor,
                AmbientGround = RenderSettings.ambientGroundColor,
                Fog = RenderSettings.fogColor,
                FogDensity = 0f,
                UseFog = false,
                SunColor = _sun != null ? _sun.color : Color.white,
                SunIntensity = _sun != null ? _sun.intensity : 1f,
                SunAngles = _sun != null ? _sun.transform.rotation.eulerAngles : new Vector3(50f, -35f, 0f),
                FillColor = _fill != null ? _fill.color : Color.white,
                FillIntensity = _fill != null ? _fill.intensity : 0.26f,
                FillAngles = _fill != null ? _fill.transform.rotation.eulerAngles : new Vector3(35f, 145f, 0f),
                CameraPosition = _camera != null ? _camera.transform.localPosition : Vector3.zero,
                CameraRotation = _camera != null ? _camera.transform.localRotation : Quaternion.identity,
                Fov = _camera != null ? _camera.fieldOfView : 30f,
            };

            _from = _default;
            _to = _default;
            _clock = BlendDuration;
        }

        /// <summary>Blends towards a staged world. Null returns to the scene default.</summary>
        public void Apply(DioramaAtmosphere atmosphere)
        {
            Capture();

            var target = _default;
            var ambience = "";
            var ambienceVolume = 0f;

            if (atmosphere != null)
            {
                atmosphere.Framing(out var position, out var rotation, out var fov);
                target = new Mood
                {
                    Sky = atmosphere.Sky,
                    AmbientSky = atmosphere.AmbientSky,
                    AmbientEquator = atmosphere.AmbientEquator,
                    AmbientGround = atmosphere.AmbientGround,
                    Fog = atmosphere.Fog,
                    FogDensity = atmosphere.FogDensity,
                    UseFog = atmosphere.UseFog,
                    SunColor = atmosphere.SunColor,
                    SunIntensity = atmosphere.SunIntensity,
                    SunAngles = atmosphere.SunAngles,
                    FillColor = atmosphere.FillColor,
                    FillIntensity = atmosphere.FillIntensity,
                    FillAngles = atmosphere.FillAngles,
                    CameraPosition = position,
                    CameraRotation = rotation,
                    Fov = fov,
                };
                ambience = atmosphere.Ambience ?? "";
                ambienceVolume = atmosphere.AmbienceVolume;
            }

            _from = Current();
            _to = target;
            _clock = 0f;

            if (_ambience != ambience)
            {
                _ambience = ambience;
                _feedback?.SetAmbience(ambience, ambienceVolume);
            }
        }

        /// <summary>Snaps back to the scene's authored mood — the shell and the home tableau.</summary>
        public void Restore() => Apply(null);

        /// <summary>
        /// Where the blend actually is, so an interrupted transition continues
        /// from what the player can see rather than from where the last one
        /// started. Rounds can be swapped faster than 0.38s from the picker.
        /// </summary>
        private Mood Current() =>
            _clock >= BlendDuration ? _to : Mood.Lerp(_from, _to, Ease(_clock / BlendDuration));

        private static float Ease(float t) => Easing.Evaluate(EaseKind.InOut, Mathf.Clamp01(t));

        private void LateUpdate()
        {
            if (_clock >= BlendDuration) return;

            _clock += Time.deltaTime;
            Write(Current());
        }

        private void Write(Mood mood)
        {
            if (_camera != null) _camera.backgroundColor = mood.Sky;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = mood.AmbientSky;
            RenderSettings.ambientEquatorColor = mood.AmbientEquator;
            RenderSettings.ambientGroundColor = mood.AmbientGround;

            // Exponential-squared over linear: the platform is only ~3.4 deep,
            // so linear fog needs start/end tuned per world while density
            // behaves the same everywhere.
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = mood.Fog;
            RenderSettings.fogDensity = mood.FogDensity;
            RenderSettings.fog = mood.FogDensity > 0.001f;

            if (_sun != null)
            {
                _sun.color = mood.SunColor;
                _sun.intensity = mood.SunIntensity;
                _sun.transform.rotation = Quaternion.Euler(mood.SunAngles);
            }

            if (_fill != null)
            {
                _fill.color = mood.FillColor;
                _fill.intensity = mood.FillIntensity;
                _fill.transform.rotation = Quaternion.Euler(mood.FillAngles);
            }

            // GameFeel owns the camera transform every frame for kick and
            // focus, so framing has to be handed to it rather than written
            // here — two writers and the shake would win.
            _gameFeel?.SetFraming(mood.CameraPosition, mood.CameraRotation, mood.Fov);
        }

        private void OnDisable()
        {
            // Leaving the game scene with a trench's fog still set would tint
            // whatever renders next.
            RenderSettings.fog = false;
        }
    }
}
