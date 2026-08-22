using System.Collections.Generic;
using SavePeps.Rescue;
using UnityEngine;

namespace SavePeps.Core
{
    /// <summary>
    /// The small shared layer between a data-authored outcome and the phone:
    /// camera acknowledgement, impact response, and the reunion's heart and
    /// confetti. Keeping it universal makes every new rescue inherit the same
    /// finish without adding choreography or rescue-specific behaviour.
    /// </summary>
    public sealed class GameFeel : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        private readonly List<Material> _ownedMaterials = new();
        private Transform _heart;
        private Transform[] _sparkles;
        private Vector3[] _sparkDirections;
        private Vector3 _cameraRestPosition;
        private Quaternion _cameraRestRotation;
        private float _cameraRestFov;
        private Vector3 _focusOffset;
        private float _kick;
        private float _shakeClock;
        private float _zoom;
        private float _fxClock = -1f;
        private Vector3 _fxOrigin;

        private void Awake()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            // Unity's unspecified mobile default settles at 30 fps. These
            // dioramas are intentionally lightweight, and the entire game is
            // short character/prop motion, so a 60 fps request buys more feel
            // than additional geometry would. Android may choose the closest
            // supported cadence for the panel (45 on some 90 Hz devices).
            Application.targetFrameRate = 60;
#endif

            if (_camera == null) _camera = Camera.main;
            if (_camera != null)
            {
                _cameraRestPosition = _camera.transform.localPosition;
                _cameraRestRotation = _camera.transform.localRotation;
                _cameraRestFov = _camera.fieldOfView;
            }

            BuildCelebrationFx();
            ResetPresentation();
        }

        /// <summary>
        /// Moves the fixed camera's rest pose, which is what lets each world
        /// choose its own framing.
        ///
        /// It has to come through here rather than being written to the camera
        /// directly: this component rewrites the transform every frame from
        /// its cached rest pose, so an outside writer would be overwritten on
        /// the next Update and <see cref="ResetPresentation"/> would snap the
        /// camera back to whatever the scene shipped with.
        /// </summary>
        public void SetFraming(Vector3 localPosition, Quaternion localRotation, float fieldOfView)
        {
            _cameraRestPosition = localPosition;
            _cameraRestRotation = localRotation;
            _cameraRestFov = fieldOfView;
        }

        public void Tap(Vector3 worldPosition)
        {
            _kick = Mathf.Max(_kick, 0.32f);
            if (_camera == null) return;

            var viewport = _camera.WorldToViewportPoint(worldPosition);
            _focusOffset = new Vector3(
                Mathf.Clamp(viewport.x - 0.5f, -0.5f, 0.5f) * 0.025f,
                Mathf.Clamp(viewport.y - 0.5f, -0.5f, 0.5f) * 0.014f,
                0f);
        }

        public void Impact(float strength = 1f)
        {
            _kick = Mathf.Max(_kick, Mathf.Clamp(strength, 0.2f, 1.4f));
            _shakeClock = 0f;
        }

        public void Wrong(Vector3 worldPosition)
        {
            Tap(worldPosition);
            Impact(0.65f);
        }

        public void Reunion(Vector3 worldPosition)
        {
            _fxOrigin = worldPosition + Vector3.up * 0.45f;
            _fxClock = 0f;
            _zoom = 1.15f;
            _kick = Mathf.Max(_kick, 0.55f);
            if (_heart != null) _heart.gameObject.SetActive(true);
            foreach (var sparkle in _sparkles ?? System.Array.Empty<Transform>())
            {
                if (sparkle != null) sparkle.gameObject.SetActive(true);
            }
        }

        public void ResetPresentation()
        {
            _kick = 0f;
            _zoom = 0f;
            _shakeClock = 0f;
            _focusOffset = Vector3.zero;
            _fxClock = -1f;
            if (_camera != null)
            {
                _camera.transform.localPosition = _cameraRestPosition;
                _camera.transform.localRotation = _cameraRestRotation;
                _camera.fieldOfView = _cameraRestFov;
            }

            if (_heart != null) _heart.gameObject.SetActive(false);
            foreach (var sparkle in _sparkles ?? System.Array.Empty<Transform>())
            {
                if (sparkle != null) sparkle.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateCamera();
            UpdateCelebration();
        }

        private void UpdateCamera()
        {
            if (_camera == null) return;

            _shakeClock += Time.deltaTime;
            _kick = Mathf.MoveTowards(_kick, 0f, Time.deltaTime * 3.8f);
            _zoom = Mathf.MoveTowards(_zoom, 0f, Time.deltaTime * 0.72f);
            _focusOffset = Vector3.Lerp(_focusOffset, Vector3.zero, Time.deltaTime * 5f);

            var noise = new Vector3(
                Mathf.Sin(_shakeClock * 43f),
                Mathf.Sin(_shakeClock * 57f + 1.3f),
                0f) * (_kick * 0.012f);
            _camera.transform.localPosition = _cameraRestPosition + _focusOffset + noise;
            _camera.transform.localRotation = _cameraRestRotation *
                                              Quaternion.Euler(noise.y * 18f, noise.x * 18f, noise.x * 8f);
            _camera.fieldOfView = _cameraRestFov - _zoom - _kick * 0.12f;
        }

        private void UpdateCelebration()
        {
            if (_fxClock < 0f) return;

            _fxClock += Time.deltaTime;
            var t = Mathf.Clamp01(_fxClock / 1.15f);
            var appear = Easing.Evaluate(EaseKind.Back, Mathf.Clamp01(_fxClock / 0.24f));
            var vanish = t < 0.72f ? 1f : 1f - Easing.Evaluate(EaseKind.In, (t - 0.72f) / 0.28f);

            if (_heart != null)
            {
                _heart.position = _fxOrigin + Vector3.up * (t * 0.16f);
                _heart.localScale = Vector3.one * (appear * vanish * 0.95f);
                _heart.rotation = Quaternion.Euler(0f, _fxClock * 28f, Mathf.Sin(_fxClock * 7f) * 5f);
            }

            for (var i = 0; i < (_sparkles?.Length ?? 0); i++)
            {
                var sparkle = _sparkles[i];
                if (sparkle == null) continue;
                var delay = i * 0.018f;
                var p = Mathf.Clamp01((_fxClock - delay) / 0.72f);
                sparkle.position = _fxOrigin + _sparkDirections[i] * (Easing.Evaluate(EaseKind.Out, p) * 0.50f)
                                    + Vector3.up * (Mathf.Sin(p * Mathf.PI) * 0.10f);
                sparkle.localScale = Vector3.one * (0.038f * Mathf.Sin(p * Mathf.PI));
                sparkle.localRotation = Quaternion.Euler(p * 180f, p * 250f, i * 37f);
            }

            if (_fxClock < 1.15f) return;
            _fxClock = -1f;
            if (_heart != null) _heart.gameObject.SetActive(false);
            foreach (var sparkle in _sparkles) sparkle.gameObject.SetActive(false);
        }

        private void BuildCelebrationFx()
        {
            var coral = NewMaterial(new Color(1f, 0.35f, 0.30f));
            var gold = NewMaterial(new Color(1f, 0.71f, 0.24f));
            var cream = NewMaterial(new Color(1f, 0.97f, 0.84f));
            var mint = NewMaterial(new Color(0.18f, 0.77f, 0.71f));

            var root = new GameObject("ReunionFx");
            root.transform.SetParent(transform, false);

            _heart = new GameObject("Heart").transform;
            _heart.SetParent(root.transform, false);
            var left = Primitive("HeartLeft", _heart, coral, PrimitiveType.Sphere);
            left.localPosition = new Vector3(-0.045f, 0.045f, 0f);
            left.localScale = new Vector3(0.105f, 0.105f, 0.07f);
            var right = Primitive("HeartRight", _heart, coral, PrimitiveType.Sphere);
            right.localPosition = new Vector3(0.045f, 0.045f, 0f);
            right.localScale = new Vector3(0.105f, 0.105f, 0.07f);
            var point = Primitive("HeartPoint", _heart, coral, PrimitiveType.Cube);
            point.localPosition = new Vector3(0f, -0.025f, 0f);
            point.localRotation = Quaternion.Euler(0f, 0f, 45f);
            point.localScale = new Vector3(0.14f, 0.14f, 0.065f);

            var colors = new[] { gold, cream, mint, coral };
            _sparkles = new Transform[10];
            _sparkDirections = new Vector3[_sparkles.Length];
            for (var i = 0; i < _sparkles.Length; i++)
            {
                var sparkle = Primitive($"Sparkle_{i}", root.transform, colors[i % colors.Length],
                    i % 2 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere);
                _sparkles[i] = sparkle;
                var angle = (i / (float)_sparkles.Length) * Mathf.PI * 2f + 0.2f;
                _sparkDirections[i] = new Vector3(Mathf.Cos(angle), 0.35f + Mathf.Sin(angle) * 0.45f,
                    Mathf.Sin(angle) * 0.20f).normalized;
            }
        }

        private Material NewMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            _ownedMaterials.Add(material);
            return material;
        }

        private static Transform Primitive(string name, Transform parent, Material material, PrimitiveType type)
        {
            // GameObject.CreatePrimitive also tries to add a collider. Those
            // collider classes are correctly stripped from the release build
            // because celebration FX never use physics, which made every boot
            // emit a misleading Android error. Use the same built-in meshes
            // directly and create only the two components actually needed.
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.name = name;
            go.layer = 2; // Ignore Raycast: celebration must never steal a tap.
            go.transform.SetParent(parent, false);
            var meshName = type == PrimitiveType.Cube ? "Cube.fbx" : "Sphere.fbx";
            go.GetComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>(meshName);
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go.transform;
        }

        private void OnDestroy()
        {
            foreach (var material in _ownedMaterials)
            {
                if (material != null) Destroy(material);
            }
        }
    }
}
