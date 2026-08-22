using System.Collections.Generic;
using System.Linq;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Checks the authored catalogue against the rules a rescue has to obey to
    /// be playable and fair.
    ///
    /// This is the heir to Save Pip's catalog tests, and the highest-return
    /// tooling in the project: nearly every one of these rules exists because
    /// breaking it produces a rescue that *looks* fine in the inspector and
    /// fails silently at the worst moment — a step aimed at a misspelled
    /// anchor simply does nothing, and you find out while somebody is
    /// watching.
    ///
    /// Errors block; warnings are judgement calls that a human should see and
    /// may legitimately override.
    /// </summary>
    public static class ContentValidator
    {
        public sealed class Report
        {
            public readonly List<string> Errors = new();
            public readonly List<string> Warnings = new();
            public bool Ok => Errors.Count == 0;

            public override string ToString()
            {
                var lines = new List<string>();
                foreach (var e in Errors) lines.Add("ERROR   " + e);
                foreach (var w in Warnings) lines.Add("warning " + w);
                return lines.Count == 0 ? "No issues." : string.Join("\n", lines);
            }
        }

        /// <summary>Outcome length band. Shorter reads as a glitch, longer outstays the gag.</summary>
        private const float MinDuration = 2.0f;
        private const float MaxDuration = 3.6f;

        [MenuItem("Tools/Save Peps/Validate Content")]
        public static void ValidateFromMenu()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(ContentPaths.CatalogPath);
            if (catalog == null)
            {
                Debug.LogError($"[SavePeps] No catalogue at {ContentPaths.CatalogPath}.");
                return;
            }

            var report = Validate(catalog);
            if (report.Ok && report.Warnings.Count == 0) Debug.Log("[SavePeps] Content validated clean.");
            else if (report.Ok) Debug.LogWarning("[SavePeps] Content validated with warnings:\n" + report);
            else Debug.LogError("[SavePeps] Content is invalid:\n" + report);
        }

        /// <summary>
        /// Validates one rescue on its own. The catalogue-wide rules (unique
        /// verbs, the protean-object rule, round composition) cannot be
        /// checked from here and are not — this is what the inspector shows
        /// while a single asset is being authored.
        /// </summary>
        public static Report Validate(RescueDefinition rescue)
        {
            var report = new Report();
            if (rescue == null)
            {
                report.Errors.Add("There is no rescue.");
                return report;
            }

            ValidateRescue(rescue, report);
            return report;
        }

        public static Report Validate(Catalog catalog)
        {
            var report = new Report();
            if (catalog == null)
            {
                report.Errors.Add("There is no catalogue.");
                return report;
            }

            if (catalog.RoundCount == 0) report.Errors.Add("The catalogue has no rounds.");

            var verbs = new Dictionary<string, string>();
            var goals = new Dictionary<string, string>();
            var seen = new HashSet<RescueDefinition>();
            var ordered = new List<RescueDefinition>();

            for (var i = 0; i < catalog.RoundCount; i++)
            {
                var round = catalog.Round(i + 1);
                if (round == null)
                {
                    report.Errors.Add($"Catalogue slot {i + 1} is empty.");
                    continue;
                }

                ValidateRound(round, report);

                foreach (var rescue in round.Rescues ?? System.Array.Empty<RescueDefinition>())
                {
                    if (rescue == null || !seen.Add(rescue)) continue;
                    ordered.Add(rescue);
                    ValidateRescue(rescue, report);
                    TrackUnique(rescue, verbs, goals, report);
                }
            }

            ValidateAdjacentReasoning(ordered, report);
            ValidateProtean(seen, report);
            ValidateWorlds(catalog, report);
            ValidateStagesAreUnique(catalog, report);
            ValidateSolutionsAreUnique(ordered, report);
            return report;
        }

        /// <summary>
        /// One round is one world, and no world is visited twice.
        ///
        /// This is the rule the first catalogue most needed and did not have.
        /// Round 4 was "Canyon" and contained one canyon rescue plus two
        /// garden ones borrowed from round 1; round 7 was three scenes
        /// borrowed from rounds 2 and 3. Every individual rescue passed
        /// validation, and the round still felt like nowhere. The world id
        /// lives on the environment prefab's <see cref="DioramaAtmosphere"/>,
        /// beside the light and sky it names.
        /// </summary>
        private static void ValidateWorlds(Catalog catalog, Report report)
        {
            var owners = new Dictionary<string, int>();

            for (var number = 1; number <= catalog.RoundCount; number++)
            {
                var round = catalog.Round(number);
                if (round == null) continue;

                string world = null;
                foreach (var rescue in round.Rescues ?? System.Array.Empty<RescueDefinition>())
                {
                    if (rescue?.Environment == null) continue;

                    var atmosphere = rescue.Environment.GetComponent<DioramaAtmosphere>();
                    if (atmosphere == null || string.IsNullOrWhiteSpace(atmosphere.WorldId))
                    {
                        report.Errors.Add(
                            $"{rescue.Id}: '{rescue.Environment.name}' has no DioramaAtmosphere world id — " +
                            "it would play under the previous world's sky.");
                        continue;
                    }

                    var id = atmosphere.WorldId.Trim();
                    if (world == null) world = id;
                    else if (world != id)
                    {
                        report.Errors.Add(
                            $"Round {number}: '{rescue.Id}' is set in '{id}' while the round is '{world}' — " +
                            "a round is one world.");
                    }
                }

                if (world == null) continue;

                if (owners.TryGetValue(world, out var other))
                {
                    report.Errors.Add(
                        $"Round {number} revisits the world '{world}', already used by round {other}.");
                }
                else
                {
                    owners[world] = number;
                }
            }
        }

        /// <summary>
        /// No two rescues share a diorama.
        ///
        /// Reusing an environment was the original content plan — eight
        /// dioramas were budgeted for thirty-six rescues — and it is exactly
        /// what made neighbouring rounds indistinguishable: the same slab,
        /// the same three slots, the same camera, a different prop. A world
        /// may repeat its palette and its base; it must not repeat its stage.
        /// </summary>
        private static void ValidateStagesAreUnique(Catalog catalog, Report report)
        {
            var owners = new Dictionary<GameObject, string>();

            for (var number = 1; number <= catalog.RoundCount; number++)
            {
                foreach (var rescue in catalog.Round(number)?.Rescues ?? System.Array.Empty<RescueDefinition>())
                {
                    if (rescue?.Environment == null) continue;

                    if (owners.TryGetValue(rescue.Environment, out var owner))
                    {
                        report.Errors.Add(
                            $"{rescue.Id}: stages '{rescue.Environment.name}', already used by {owner} — " +
                            "two rescues on identical geometry read as one rescue asked twice.");
                    }
                    else
                    {
                        owners[rescue.Environment] = rescue.Id;
                    }
                }
            }
        }

        /// <summary>
        /// No prop may solve the same physical idea twice.
        ///
        /// Verb uniqueness never caught this and neither did the reasoning
        /// rules: "melt the ice" and "melt the frost" had different verbs, sat
        /// four rounds apart, and were the same tap on the same hair dryer
        /// producing the same shrinking shell. A prop may absolutely recur —
        /// the umbrella shelters in one world and glides in another — but the
        /// pair of (what solves it, why it solves it) has to be new.
        /// </summary>
        private static void ValidateSolutionsAreUnique(IReadOnlyList<RescueDefinition> rescues, Report report)
        {
            var owners = new Dictionary<string, string>();

            foreach (var rescue in rescues)
            {
                var prop = rescue.Correct?.Id;
                if (string.IsNullOrEmpty(prop)) continue;

                var key = $"{prop}+{rescue.Reasoning}";
                if (owners.TryGetValue(key, out var owner))
                {
                    report.Errors.Add(
                        $"{rescue.Id}: solved by '{prop}' using {rescue.Reasoning} reasoning, exactly like " +
                        $"{owner} — that is the same puzzle in different scenery.");
                }
                else
                {
                    owners[key] = rescue.Id;
                }
            }
        }

        // -------------------------------------------------------------------

        private static void ValidateRound(RoundDefinition round, Report report)
        {
            var rescues = round.Rescues ?? System.Array.Empty<RescueDefinition>();
            if (rescues.Length != RoundDefinition.RescuesPerRound)
            {
                report.Errors.Add($"Round {round.Number} has {rescues.Length} rescues, not {RoundDefinition.RescuesPerRound}.");
            }

            for (var i = 0; i < rescues.Length; i++)
            {
                if (rescues[i] == null)
                {
                    report.Errors.Add($"Round {round.Number} slot {i + 1} is empty.");
                    continue;
                }

                for (var j = i + 1; j < rescues.Length; j++)
                {
                    if (rescues[j] == null) continue;

                    var correctA = rescues[i].Correct?.Id;
                    var correctB = rescues[j].Correct?.Id;
                    if (!string.IsNullOrEmpty(correctA) && correctA == correctB)
                    {
                        report.Errors.Add(
                            $"Round {round.Number}: '{rescues[i].Id}' and '{rescues[j].Id}' are both solved by " +
                            $"'{correctA}' — the second is answerable without looking.");
                    }

                    if (!string.IsNullOrEmpty(rescues[i].Verb) && rescues[i].Verb == rescues[j].Verb)
                    {
                        report.Warnings.Add(
                            $"Round {round.Number}: '{rescues[i].Id}' and '{rescues[j].Id}' share the verb " +
                            $"'{rescues[i].Verb}'.");
                    }

                    if (rescues[i].Reasoning == rescues[j].Reasoning)
                    {
                        report.Errors.Add(
                            $"Round {round.Number}: '{rescues[i].Id}' and '{rescues[j].Id}' both use " +
                            $"{rescues[i].Reasoning} reasoning — different verbs cannot disguise the same puzzle.");
                    }
                }
            }

            if (rescues.Length > 1 && rescues.Where(r => r != null).Select(r => r.Difficulty).Distinct().Count() == 1)
            {
                report.Warnings.Add($"Round {round.Number} is all one difficulty.");
            }

            ValidateAnswerPositions(round, rescues, report);
        }

        /// <summary>
        /// The boundary between rounds is part of the player's sequence too.
        /// Round N ending and Round N+1 beginning with the same physical idea
        /// makes the second round feel like more wallpaper, even when each
        /// round is internally varied.
        /// </summary>
        private static void ValidateAdjacentReasoning(IReadOnlyList<RescueDefinition> rescues, Report report)
        {
            for (var i = 1; i < rescues.Count; i++)
            {
                var previous = rescues[i - 1];
                var current = rescues[i];
                if (previous.Reasoning != current.Reasoning) continue;

                report.Errors.Add(
                    $"{current.Id}: repeats {current.Reasoning} reasoning immediately after {previous.Id}.");
            }
        }

        /// <summary>
        /// The answer must move around the scene between rescues.
        ///
        /// Found on a device: all three of round one's answers sat in Slot_1,
        /// and the round could be won by tapping the same spot three times
        /// without looking at anything. That defeats the entire game, and no
        /// other rule catches it — the objects differed, the verbs differed,
        /// only the position gave it away.
        /// </summary>
        private static void ValidateAnswerPositions(RoundDefinition round, RescueDefinition[] rescues, Report report)
        {
            var anchors = new List<string>();
            foreach (var rescue in rescues)
            {
                var anchor = rescue?.Correct?.AnchorId;
                if (!string.IsNullOrEmpty(anchor)) anchors.Add(anchor);
            }

            if (anchors.Count < 2) return;

            var distinct = anchors.Distinct().Count();

            if (distinct == 1)
            {
                report.Errors.Add(
                    $"Round {round.Number}: every answer is at '{anchors[0]}' — the round can be won by " +
                    "tapping the same place repeatedly, without reading a single scene.");
                return;
            }

            // Two of three in one spot is not fatal, but it is the start of the
            // same problem and worth seeing while the round is still cheap to change.
            foreach (var group in anchors.GroupBy(a => a))
            {
                if (group.Count() > anchors.Count / 2)
                {
                    report.Warnings.Add(
                        $"Round {round.Number}: {group.Count()} of {anchors.Count} answers sit at '{group.Key}'.");
                }
            }
        }

        private static void ValidateRescue(RescueDefinition rescue, Report report)
        {
            var id = string.IsNullOrEmpty(rescue.Id) ? rescue.name : rescue.Id;

            if (string.IsNullOrWhiteSpace(rescue.Id)) report.Errors.Add($"'{rescue.name}' has no id.");
            if (string.IsNullOrWhiteSpace(rescue.Verb)) report.Errors.Add($"{id}: no verb.");
            if (rescue.Environment == null) report.Errors.Add($"{id}: no environment prefab.");
            if (rescue.PepAPrefab == null || rescue.PepBPrefab == null) report.Errors.Add($"{id}: a Pep prefab is missing.");

            var goalWords = (rescue.Goal ?? string.Empty).Split(' ', System.StringSplitOptions.RemoveEmptyEntries).Length;
            if (goalWords is < 2 or > 5) report.Warnings.Add($"{id}: goal is {goalWords} words; 2-4 reads best over a scene.");

            var objects = rescue.Objects ?? System.Array.Empty<RescueObject>();
            if (objects.Length != 3)
            {
                report.Errors.Add($"{id}: {objects.Length} objects, must be exactly 3.");
            }

            if (rescue.CorrectIndex < 0 || rescue.CorrectIndex >= objects.Length)
            {
                report.Errors.Add($"{id}: CorrectIndex {rescue.CorrectIndex} is out of range.");
            }

            // Names the choreography is allowed to aim at.
            var names = CollectNames(rescue);
            var animatedNames = CollectAnimatedNames(rescue);

            var ids = new HashSet<string>();
            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    report.Errors.Add($"{id}: an object slot is empty.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(obj.Id)) report.Errors.Add($"{id}: an object has no id.");
                else if (!ids.Add(obj.Id)) report.Errors.Add($"{id}: two objects share the id '{obj.Id}'.");

                if (obj.Prop == null) report.Errors.Add($"{id}/{obj.Id}: no prop prefab.");

                if (!string.IsNullOrEmpty(obj.AnchorId) && !names.Contains(obj.AnchorId))
                {
                    report.Errors.Add($"{id}/{obj.Id}: anchor '{obj.AnchorId}' is not in the environment.");
                }

                var correct = rescue.IsCorrect(obj);
                if (!correct && string.IsNullOrWhiteSpace(obj.Quip))
                {
                    report.Errors.Add($"{id}/{obj.Id}: a wrong object with no quip — failure has to land as a joke.");
                }
                else if (!correct)
                {
                    var quip = obj.Quip.Trim();
                    if (quip.Contains("\n") || quip.Contains("\r"))
                    {
                        report.Errors.Add($"{id}/{obj.Id}: quip must stay on one line.");
                    }

                    if (quip.Length > RescueObject.MaxQuipCharacters)
                    {
                        report.Errors.Add(
                            $"{id}/{obj.Id}: quip is {quip.Length} characters; " +
                            $"{RescueObject.MaxQuipCharacters} is the Pixel 4 one-line limit.");
                    }
                }

                if (obj.Duration < MinDuration || obj.Duration > MaxDuration)
                {
                    report.Warnings.Add($"{id}/{obj.Id}: {obj.Duration:0.##}s is outside the {MinDuration}-{MaxDuration}s band.");
                }

                ValidateSteps(rescue, obj, correct, animatedNames, report);
            }
        }

        private static void ValidateSteps(RescueDefinition rescue, RescueObject obj, bool correct,
            HashSet<string> animatedNames, Report report)
        {
            var id = rescue.Id;
            var steps = obj.Steps ?? System.Array.Empty<OutcomeStep>();

            if (steps.Length == 0)
            {
                report.Errors.Add($"{id}/{obj.Id}: no steps — the tap would do nothing.");
                return;
            }

            var meets = 0;
            foreach (var step in steps)
            {
                if (step == null)
                {
                    report.Errors.Add($"{id}/{obj.Id}: an empty step.");
                    continue;
                }

                if (step.At < 0f) report.Errors.Add($"{id}/{obj.Id}: a step starts at {step.At:0.##}s.");

                // A step running past the outcome window is cut off mid-motion
                // when the result appears — the single most common authoring
                // slip, and invisible until you watch it at full speed.
                if (step.EndTime > obj.Duration + 0.001f)
                {
                    report.Errors.Add(
                        $"{id}/{obj.Id}: a {step.Kind} step ends at {step.EndTime:0.##}s, past the " +
                        $"{obj.Duration:0.##}s outcome.");
                }

                if (!SceneRef.IsReserved(step.Target) && !animatedNames.Contains(step.Target))
                {
                    report.Errors.Add(
                        $"{id}/{obj.Id}: step target '{step.Target}' has no AnimTarget — it will silently do nothing.");
                }

                if (step.Kind == StepKind.Meet) meets++;

                if (step.Kind == StepKind.Face && !System.Enum.TryParse<PepFace>(step.Param, true, out _))
                {
                    report.Errors.Add($"{id}/{obj.Id}: '{step.Param}' is not a face.");
                }
            }

            if (correct && meets == 0)
            {
                report.Errors.Add($"{id}/{obj.Id}: the correct object never reunites the Peps (no Meet step).");
            }

            if (!correct && meets > 0)
            {
                report.Errors.Add($"{id}/{obj.Id}: a wrong object reunites the Peps.");
            }
        }

        /// <summary>
        /// Save Pip's protean-object rule: no prop may be correct everywhere it
        /// appears, or wrong everywhere it appears. A prop with a fixed answer
        /// teaches the player to stop reading the scene, which is the one
        /// failure this game cannot afford.
        ///
        /// A warning rather than an error — it is unsatisfiable until a prop
        /// has appeared in enough lineups, and shouting about it during the
        /// content sprint would train everyone to ignore the validator.
        /// </summary>
        private static void ValidateProtean(IEnumerable<RescueDefinition> rescues, Report report)
        {
            var correctCount = new Dictionary<string, int>();
            var totalCount = new Dictionary<string, int>();

            foreach (var rescue in rescues)
            {
                foreach (var obj in rescue.Objects ?? System.Array.Empty<RescueObject>())
                {
                    if (obj == null || string.IsNullOrEmpty(obj.Id)) continue;
                    totalCount.TryGetValue(obj.Id, out var t);
                    totalCount[obj.Id] = t + 1;
                    if (!rescue.IsCorrect(obj)) continue;
                    correctCount.TryGetValue(obj.Id, out var c);
                    correctCount[obj.Id] = c + 1;
                }
            }

            foreach (var (propId, total) in totalCount)
            {
                if (total < 3) continue;   // too few appearances to draw a conclusion
                correctCount.TryGetValue(propId, out var correct);

                if (correct == total)
                    report.Warnings.Add($"'{propId}' is the answer every time it appears ({total}).");
                else if (correct == 0)
                    report.Warnings.Add($"'{propId}' is never the answer across {total} appearances.");
            }
        }

        private static void TrackUnique(RescueDefinition rescue, IDictionary<string, string> verbs,
            IDictionary<string, string> goals, Report report)
        {
            if (!string.IsNullOrWhiteSpace(rescue.Verb))
            {
                if (verbs.TryGetValue(rescue.Verb, out var owner))
                    report.Errors.Add($"{rescue.Id}: verb '{rescue.Verb}' is already used by {owner}.");
                else verbs[rescue.Verb] = rescue.Id;
            }

            if (string.IsNullOrWhiteSpace(rescue.Goal)) return;

            var goal = rescue.Goal.Trim();
            if (goals.TryGetValue(goal, out var goalOwner))
                report.Warnings.Add($"{rescue.Id}: goal '{goal}' is already used by {goalOwner}.");
            else goals[goal] = rescue.Id;
        }

        /// <summary>
        /// Everything a step target may name: the reserved refs are handled
        /// separately, so this is the environment's transform names plus the
        /// rescue's own object ids, which RescueRunner registers by id.
        /// </summary>
        private static HashSet<string> CollectNames(RescueDefinition rescue)
        {
            var names = new HashSet<string>();

            if (rescue.Environment != null)
            {
                foreach (var t in rescue.Environment.GetComponentsInChildren<Transform>(true))
                {
                    names.Add(t.name);
                }
            }

            foreach (var obj in rescue.Objects ?? System.Array.Empty<RescueObject>())
            {
                if (obj != null && !string.IsNullOrEmpty(obj.Id)) names.Add(obj.Id);
            }

            return names;
        }

        /// <summary>
        /// Mirrors RescueRunner's runtime lookup exactly. A transform merely
        /// existing in a prefab does not make it animatable; only the parent
        /// name of an AnimTarget (plus spawned prop ids) enters the resolver.
        /// The old all-transform check let choreography aimed at static
        /// scenery validate cleanly and then disappear at runtime.
        /// </summary>
        private static HashSet<string> CollectAnimatedNames(RescueDefinition rescue)
        {
            var names = new HashSet<string>();

            if (rescue.Environment != null)
            {
                foreach (var target in rescue.Environment.GetComponentsInChildren<AnimTarget>(true))
                {
                    names.Add(target.transform.parent != null ? target.transform.parent.name : target.name);
                }
            }

            foreach (var obj in rescue.Objects ?? System.Array.Empty<RescueObject>())
            {
                if (obj != null && !string.IsNullOrEmpty(obj.Id)) names.Add(obj.Id);
            }

            return names;
        }
    }
}
