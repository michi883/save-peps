using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Tools and automation for per-round escalation auditing, reseeding,
    /// validation, and preview capture.
    ///
    /// Provides isolated single-round operations to prevent cross-round regressions.
    /// </summary>
    public static class EscalationWorkflow
    {
        public sealed class RoundAudit
        {
            public int RoundNumber;
            public string WorldName;
            public RescueAudit R1;
            public RescueAudit R2;
            public RescueAudit R3;
            public bool HasProgression;
            public List<string> Observations = new();

            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"=== ESCALATION AUDIT: ROUND {RoundNumber} ({WorldName}) ===");
                sb.AppendLine($"[R{RoundNumber}.1 INTRODUCE] {R1.Id} ({R1.Verb}) -> Dur: {R1.Duration:F1}s | Steps: {R1.StepCount} | Movers: {R1.UniqueTargets} ({string.Join(", ", R1.Targets)})");
                sb.AppendLine($"[R{RoundNumber}.2 EXPAND]    {R2.Id} ({R2.Verb}) -> Dur: {R2.Duration:F1}s | Steps: {R2.StepCount} | Movers: {R2.UniqueTargets} ({string.Join(", ", R2.Targets)})");
                sb.AppendLine($"[R{RoundNumber}.3 CLIMAX]    {R3.Id} ({R3.Verb}) -> Dur: {R3.Duration:F1}s | Steps: {R3.StepCount} | Movers: {R3.UniqueTargets} ({string.Join(", ", R3.Targets)})");
                sb.AppendLine($"Progression Check (R1 <= R2 <= R3): {(HasProgression ? "PASS" : "NEEDS ESCALATION")}");
                if (Observations.Count > 0)
                {
                    sb.AppendLine("Notes / Recommendations:");
                    foreach (var obs in Observations) sb.AppendLine("  * " + obs);
                }
                return sb.ToString();
            }
        }

        public sealed class RescueAudit
        {
            public string Id;
            public string Verb;
            public float Duration;
            public int StepCount;
            public int UniqueTargets;
            public List<string> Targets = new();
        }

        [MenuItem("Tools/Save Peps/Escalation/Audit All Rounds")]
        public static void AuditAllRounds()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(ContentPaths.CatalogPath);
            if (catalog == null)
            {
                Debug.LogError($"[SavePeps] No catalog at {ContentPaths.CatalogPath}");
                return;
            }

            for (var i = 1; i <= catalog.RoundCount; i++)
            {
                var audit = AuditRound(i);
                if (audit != null) Debug.Log(audit.ToString());
            }
        }

        public static RoundAudit AuditRound(int roundNumber)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(ContentPaths.CatalogPath);
            if (catalog == null) return null;

            var round = catalog.Round(roundNumber);
            if (round == null || round.Rescues == null || round.Rescues.Length < 3)
            {
                Debug.LogError($"[SavePeps] Round {roundNumber} is invalid or has fewer than 3 rescues.");
                return null;
            }

            var audit = new RoundAudit
            {
                RoundNumber = roundNumber,
                R1 = AuditRescue(round.Rescues[0]),
                R2 = AuditRescue(round.Rescues[1]),
                R3 = AuditRescue(round.Rescues[2]),
            };

            var worldId = round.Rescues[0]?.Environment != null
                ? round.Rescues[0].Environment.GetComponent<DioramaAtmosphere>()?.WorldId
                : "Unknown";
            audit.WorldName = worldId ?? "Unknown";

            // Evaluate progression
            var durAscending = audit.R1.Duration <= audit.R2.Duration + 0.1f && audit.R2.Duration <= audit.R3.Duration + 0.1f;
            var stepAscending = audit.R1.StepCount <= audit.R2.StepCount && audit.R2.StepCount <= audit.R3.StepCount;
            var moversAscending = audit.R1.UniqueTargets <= audit.R2.UniqueTargets && audit.R2.UniqueTargets <= audit.R3.UniqueTargets;

            audit.HasProgression = durAscending && stepAscending && moversAscending && (audit.R3.StepCount > audit.R1.StepCount);

            if (audit.R1.Duration > 2.6f)
                audit.Observations.Add($"R{roundNumber}.1 duration ({audit.R1.Duration:F1}s) is relatively high for an Introduce beat (target: 2.0-2.4s).");
            if (audit.R3.Duration < 3.2f)
                audit.Observations.Add($"R{roundNumber}.3 duration ({audit.R3.Duration:F1}s) could be extended for a richer Climax beat (target: 3.2-3.6s).");
            if (audit.R3.UniqueTargets <= 3)
                audit.Observations.Add($"R{roundNumber}.3 only moves {audit.R3.UniqueTargets} targets; consider engaging environment machinery/features.");

            return audit;
        }

        private static RescueAudit AuditRescue(RescueDefinition rescue)
        {
            if (rescue == null) return new RescueAudit { Id = "None", Verb = "None" };

            var correct = rescue.Correct;
            var targets = new HashSet<string>();
            var stepCount = 0;
            var duration = 0f;

            if (correct != null)
            {
                duration = correct.Duration;
                if (correct.Steps != null)
                {
                    stepCount = correct.Steps.Length;
                    foreach (var s in correct.Steps)
                    {
                        if (!string.IsNullOrEmpty(s.Target)) targets.Add(s.Target);
                    }
                }
            }

            return new RescueAudit
            {
                Id = rescue.Id,
                Verb = rescue.Verb,
                Duration = duration,
                StepCount = stepCount,
                UniqueTargets = targets.Count,
                Targets = targets.OrderBy(t => t).ToList(),
            };
        }

        /// <summary>
        /// Reseeds ONLY the specified round, protecting all other rounds from accidental overwrites.
        /// </summary>
        public static bool ReseedRound(int roundNumber)
        {
            var log = new ContentSeeder.SeedLog();
            RoundDefinition round = null;

            switch (roundNumber)
            {
                case 1: round = RoundOneRescues.SeedRound(true, log); break;
                case 2: round = RoundTwoRescues.SeedRound(true, log); break;
                case 3: round = RoundThreeRescues.SeedRound(true, log); break;
                case 4: round = RoundFourRescues.SeedRound(true, log); break;
                case 5: round = RoundFiveRescues.SeedRound(true, log); break;
                case 6: round = RoundSixRescues.SeedRound(true, log); break;
                case 7: round = RoundSevenRescues.SeedRound(true, log); break;
                case 8: round = RoundEightRescues.SeedRound(true, log); break;
                case 9: round = RoundNineRescues.SeedRound(true, log); break;
                case 10: round = RoundTenRescues.SeedRound(true, log); break;
                case 11: round = RoundElevenRescues.SeedRound(true, log); break;
                case 12: round = RoundTwelveRescues.SeedRound(true, log); break;
                default:
                    Debug.LogError($"[SavePeps] Unknown round number: {roundNumber}");
                    return false;
            }

            if (round != null)
            {
                EditorUtility.SetDirty(round);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[SavePeps] Round {roundNumber} reseeded cleanly: {log}");
                return true;
            }

            Debug.LogError($"[SavePeps] Failed to reseed round {roundNumber}.");
            return false;
        }

        /// <summary>
        /// Renders stage PNGs for the 3 rescues in Round N.
        /// </summary>
        public static void CaptureRoundStages(int roundNumber, string outputDir)
        {
            if (string.IsNullOrEmpty(outputDir)) outputDir = "Temp/RoundStages";
            Directory.CreateDirectory(outputDir);

            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(ContentPaths.CatalogPath);
            if (catalog == null) return;

            var round = catalog.Round(roundNumber);
            if (round == null) return;

            // Use StageContactSheet methods via reflection or shared rig
            for (var i = 0; i < round.Rescues.Length; i++)
            {
                var rescue = round.Rescues[i];
                if (rescue == null) continue;
                var outPath = Path.Combine(outputDir, $"round{roundNumber}_{i + 1}_{rescue.Id}_{rescue.Verb}.png");
                RenderRescueStage(rescue, outPath);
            }
            Debug.Log($"[SavePeps] Rendered Round {roundNumber} stages to {outputDir}");
        }

        private static void RenderRescueStage(RescueDefinition rescue, string path)
        {
            if (rescue?.Environment == null) return;

            var rig = new GameObject("RoundSheetRig");
            var cam = rig.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 40f;

            var sunGo = new GameObject("RoundSheetSun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;

            var fillGo = new GameObject("RoundSheetFill");
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;

            var stage = (GameObject)PrefabUtility.InstantiatePrefab(rescue.Environment);
            var spawned = new List<GameObject> { stage };

            try
            {
                var anchors = new Dictionary<string, Transform>();
                foreach (var t in stage.GetComponentsInChildren<Transform>(includeInactive: true))
                    anchors[t.name] = t;

                Place(rescue.PepAPrefab, anchors, rescue.PepAAnchor, spawned);
                Place(rescue.PepBPrefab, anchors, rescue.PepBAnchor, spawned);
                foreach (var obj in rescue.Objects ?? Array.Empty<RescueObject>())
                {
                    if (obj != null) Place(obj.Prop, anchors, obj.AnchorId, spawned);
                }

                var atmosphere = stage.GetComponent<DioramaAtmosphere>();
                if (atmosphere != null)
                {
                    atmosphere.Framing(out var pos, out var rot, out var fov);
                    cam.transform.SetPositionAndRotation(pos, rot);
                    cam.fieldOfView = fov;
                    cam.backgroundColor = atmosphere.Sky;

                    RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                    RenderSettings.ambientSkyColor = atmosphere.AmbientSky;
                    RenderSettings.ambientEquatorColor = atmosphere.AmbientEquator;
                    RenderSettings.ambientGroundColor = atmosphere.AmbientGround;
                    RenderSettings.fogMode = FogMode.ExponentialSquared;
                    RenderSettings.fogColor = atmosphere.Fog;
                    RenderSettings.fogDensity = atmosphere.UseFog ? atmosphere.FogDensity : 0f;
                    RenderSettings.fog = atmosphere.UseFog;

                    sun.color = atmosphere.SunColor;
                    sun.intensity = atmosphere.SunIntensity;
                    sun.transform.rotation = Quaternion.Euler(atmosphere.SunAngles);

                    fill.color = atmosphere.FillColor;
                    fill.intensity = atmosphere.FillIntensity;
                    fill.transform.rotation = Quaternion.Euler(atmosphere.FillAngles);
                }

                var rt = new RenderTexture(540, 1140, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
                cam.targetTexture = rt;
                cam.Render();
                cam.Render();

                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var shot = new Texture2D(540, 1140, TextureFormat.RGB24, mipChain: false);
                shot.ReadPixels(new Rect(0, 0, 540, 1140), 0, 0);
                shot.Apply();
                RenderTexture.active = prev;

                File.WriteAllBytes(path, shot.EncodeToPNG());
                cam.targetTexture = null;
                RenderTexture.active = null;
                UnityEngine.Object.DestroyImmediate(shot);
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
            finally
            {
                foreach (var go in spawned) if (go != null) UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(rig);
                UnityEngine.Object.DestroyImmediate(sunGo);
                UnityEngine.Object.DestroyImmediate(fillGo);
                RenderSettings.fog = false;
            }
        }

        private static void Place(GameObject prefab, IReadOnlyDictionary<string, Transform> anchors, string anchorId, List<GameObject> spawned)
        {
            if (prefab == null || string.IsNullOrEmpty(anchorId) || !anchors.TryGetValue(anchorId, out var anchor)) return;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            inst.transform.SetParent(anchor, false);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            spawned.Add(inst);
        }

        // -------------------------------------------------------------------
        // Batchmode CLI Entry Points
        // -------------------------------------------------------------------

        public static void AuditFromCli()
        {
            var roundStr = Environment.GetEnvironmentVariable("ROUND_NUM");
            if (int.TryParse(roundStr, out var roundNum))
            {
                var audit = AuditRound(roundNum);
                if (audit != null) Debug.Log($"[SavePeps AUDIT]\n{audit}");
            }
            else
            {
                AuditAllRounds();
            }
        }

        public static void ReseedFromCli()
        {
            var roundStr = Environment.GetEnvironmentVariable("ROUND_NUM");
            if (int.TryParse(roundStr, out var roundNum))
            {
                ReseedRound(roundNum);
            }
            else
            {
                Debug.LogError("[SavePeps] ROUND_NUM environment variable required for ReseedFromCli.");
            }
        }

        public static void CaptureFromCli()
        {
            var roundStr = Environment.GetEnvironmentVariable("ROUND_NUM");
            var outDir = Environment.GetEnvironmentVariable("OUTPUT_DIR") ?? "Temp/RoundStages";
            if (int.TryParse(roundStr, out var roundNum))
            {
                CaptureRoundStages(roundNum, outDir);
            }
            else
            {
                Debug.LogError("[SavePeps] ROUND_NUM environment variable required for CaptureFromCli.");
            }
        }
    }
}
