using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Gameplay.Interaction
{
    [DefaultExecutionOrder(-25)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseRunnerValidationRosterSpawner : MonoBehaviour
    {
        private const string ValidationSceneName =
            "Contest_OperationsFloor_Showcase_v007";
        private const string RuntimeRootName =
            "TEMP_ATTACK_BUDGET_VALIDATION_ROSTER";

        [SerializeField]
        private ShowcaseRunnerPressureController pressureController;

        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private ShowcaseRunnerAgent normalTemplate;

        [SerializeField]
        private bool spawnInNonBatchPlay = true;

        [SerializeField]
        private bool hideStationaryChairValidationRunners = true;

        [SerializeField, Min(1)]
        private int desiredNormalCount = 3;

        [SerializeField, Min(0)]
        private int desiredEliteCount = 1;

        [SerializeField]
        private Vector3[] playerLocalOffsets =
        {
            new(-5.75f, 0f, 5.25f),
            new(5.75f, 0f, 6.50f),
            new(-7.75f, 0f, -0.75f),
            new(7.75f, 0f, -1.25f)
        };

        private readonly List<ShowcaseRunnerAgent> runtimeClones = new();
        private bool spawned;

        public ShowcaseRunnerPressureController PressureController =>
            pressureController;

        public bool Spawned => spawned;

        public int RuntimeCloneCount => runtimeClones.Count;

        public int ActiveRosterCount =>
            pressureController?.Runners?.Length ?? 0;

        public IReadOnlyList<ShowcaseRunnerAgent> RuntimeClones =>
            runtimeClones;

        public Vector3[] PlayerLocalOffsets => playerLocalOffsets;

        public static void EnsureForManualPlay(
            ShowcaseRunnerPressureController pressure)
        {
            if (pressure == null || Application.isBatchMode ||
                pressure.gameObject.scene.name != ValidationSceneName)
            {
                return;
            }

            ShowcaseRunnerValidationRosterSpawner spawner =
                pressure.GetComponent<ShowcaseRunnerValidationRosterSpawner>() ??
                pressure.gameObject.AddComponent<
                    ShowcaseRunnerValidationRosterSpawner>();
            spawner.Configure(
                pressure,
                pressure.PlayerTransform,
                ResolveNormalTemplate(pressure));
            spawner.TrySpawnManualValidationRoster(false);
        }

        private void Awake()
        {
            pressureController ??=
                GetComponent<ShowcaseRunnerPressureController>();
            playerTransform ??= pressureController?.PlayerTransform;
            normalTemplate ??= ResolveNormalTemplate(pressureController);
            TrySpawnManualValidationRoster(false);
        }

        public void Configure(
            ShowcaseRunnerPressureController pressure,
            Transform player,
            ShowcaseRunnerAgent template)
        {
            pressureController = pressure;
            playerTransform = player;
            normalTemplate = template;
        }

        public bool TrySpawnManualValidationRoster(
            bool bypassBatchModeGuard)
        {
            if (spawned)
            {
                return true;
            }
            if (!spawnInNonBatchPlay ||
                (Application.isBatchMode && !bypassBatchModeGuard))
            {
                return false;
            }

            pressureController ??=
                GetComponent<ShowcaseRunnerPressureController>();
            playerTransform ??= pressureController?.PlayerTransform;
            normalTemplate ??= ResolveNormalTemplate(pressureController);
            if (pressureController == null || playerTransform == null ||
                normalTemplate == null || normalTemplate.IsElite)
            {
                return false;
            }

            ShowcaseRunnerAgent[] authoredRunners =
                pressureController.Runners ?? Array.Empty<ShowcaseRunnerAgent>();
            var normalRunners = new List<ShowcaseRunnerAgent>();
            var eliteRunners = new List<ShowcaseRunnerAgent>();
            var activeRoster = new List<ShowcaseRunnerAgent>(
                Mathf.Max(4, authoredRunners.Length + 2));
            for (int index = 0; index < authoredRunners.Length; index++)
            {
                ShowcaseRunnerAgent runner = authoredRunners[index];
                if (runner == null)
                {
                    continue;
                }
                activeRoster.Add(runner);
                if (runner.IsElite)
                {
                    eliteRunners.Add(runner);
                }
                else
                {
                    normalRunners.Add(runner);
                }
            }

            if (normalRunners.Count == 0 ||
                eliteRunners.Count < Mathf.Max(0, desiredEliteCount))
            {
                return false;
            }

            DisableStationaryValidationRoster();
            Transform runtimeRoot = CreateRuntimeRoot();
            float runnerRootHeight = normalTemplate.transform.position.y;
            int nextRunnerNumber = 3;
            while (normalRunners.Count < Mathf.Max(1, desiredNormalCount))
            {
                ShowcaseRunnerAgent clone = CloneNormalRunner(
                    runtimeRoot,
                    nextRunnerNumber++);
                if (clone == null)
                {
                    return false;
                }
                runtimeClones.Add(clone);
                normalRunners.Add(clone);
                activeRoster.Add(clone);
            }

            pressureController.ConfigureRuntimeRunnerRoster(
                activeRoster.ToArray());
            ApplyValidationLayout(
                normalRunners,
                eliteRunners,
                runnerRootHeight);
            Physics.SyncTransforms();
            spawned = true;
            return true;
        }

        private void DisableStationaryValidationRoster()
        {
            if (!hideStationaryChairValidationRunners ||
                pressureController.ValidationRunners == null)
            {
                return;
            }

            ShowcaseRunnerAgent[] validationRunners =
                pressureController.ValidationRunners;
            for (int index = 0; index < validationRunners.Length; index++)
            {
                ShowcaseRunnerAgent runner = validationRunners[index];
                if (runner != null)
                {
                    runner.gameObject.SetActive(false);
                }
            }
            pressureController.ConfigureStationaryValidationRunners(
                Array.Empty<ShowcaseRunnerAgent>());
        }

        private Transform CreateRuntimeRoot()
        {
            Transform existing = transform.Find(RuntimeRootName);
            if (existing != null)
            {
                return existing;
            }

            var root = new GameObject(RuntimeRootName);
            SceneManager.MoveGameObjectToScene(root, gameObject.scene);
            root.transform.SetParent(transform, false);
            return root.transform;
        }

        private ShowcaseRunnerAgent CloneNormalRunner(
            Transform runtimeRoot,
            int runnerNumber)
        {
            GameObject cloneObject = Instantiate(normalTemplate.gameObject);
            if (cloneObject == null)
            {
                return null;
            }

            cloneObject.name =
                $"GAMEPLAY_ENY01_RUNNER_{runnerNumber:00}_TEMP_VALIDATION";
            SceneManager.MoveGameObjectToScene(cloneObject, gameObject.scene);
            cloneObject.transform.SetParent(runtimeRoot, true);
            cloneObject.SetActive(true);

            ShowcaseRunnerAgent clone =
                cloneObject.GetComponent<ShowcaseRunnerAgent>();
            Rigidbody cloneBody = cloneObject.GetComponent<Rigidbody>();
            if (clone == null || cloneBody == null)
            {
                Destroy(cloneObject);
                return null;
            }

            clone.Configure(
                $"RUNNER {runnerNumber}",
                pressureController,
                cloneBody,
                playerTransform,
                authoredHealth: clone.Health,
                authoredElite: false);
            clone.ConfigureAutonomousMovementSuppressed(false);
            clone.ConfigureBatchModeDisabled(false);
            return clone;
        }

        private void ApplyValidationLayout(
            List<ShowcaseRunnerAgent> normalRunners,
            List<ShowcaseRunnerAgent> eliteRunners,
            float runnerRootHeight)
        {
            Vector3 forward = Vector3.ProjectOnPlane(
                playerTransform.forward,
                Vector3.up).normalized;
            if (forward.sqrMagnitude <= 0.001f)
            {
                forward = Vector3.forward;
            }
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            int placementIndex = 0;
            int normalCount = Mathf.Min(
                normalRunners.Count,
                Mathf.Max(1, desiredNormalCount));
            for (int index = 0; index < normalCount; index++)
            {
                PlaceRunner(
                    normalRunners[index],
                    ResolveOffset(placementIndex++),
                    right,
                    forward,
                    runnerRootHeight);
            }

            int eliteCount = Mathf.Min(
                eliteRunners.Count,
                Mathf.Max(0, desiredEliteCount));
            for (int index = 0; index < eliteCount; index++)
            {
                PlaceRunner(
                    eliteRunners[index],
                    ResolveOffset(placementIndex++),
                    right,
                    forward,
                    runnerRootHeight);
            }
        }

        private Vector3 ResolveOffset(int index)
        {
            if (playerLocalOffsets != null &&
                index >= 0 && index < playerLocalOffsets.Length)
            {
                return playerLocalOffsets[index];
            }

            float angle = index * 90f * Mathf.Deg2Rad;
            return new Vector3(
                Mathf.Sin(angle) * 7f,
                0f,
                Mathf.Cos(angle) * 7f);
        }

        private void PlaceRunner(
            ShowcaseRunnerAgent runner,
            Vector3 localOffset,
            Vector3 right,
            Vector3 forward,
            float runnerRootHeight)
        {
            if (runner == null)
            {
                return;
            }

            Vector3 position = playerTransform.position +
                (right * localOffset.x) +
                (forward * localOffset.z);
            position.y = runnerRootHeight;
            Vector3 towardPlayer = Vector3.ProjectOnPlane(
                playerTransform.position - position,
                Vector3.up);
            Quaternion rotation = towardPlayer.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(towardPlayer.normalized, Vector3.up)
                : playerTransform.rotation;
            runner.ConfigureTemporaryValidationBaseline(position, rotation);
        }

        private static ShowcaseRunnerAgent ResolveNormalTemplate(
            ShowcaseRunnerPressureController pressure)
        {
            ShowcaseRunnerAgent[] candidates = pressure?.Runners;
            if (candidates == null)
            {
                return null;
            }
            for (int index = 0; index < candidates.Length; index++)
            {
                ShowcaseRunnerAgent candidate = candidates[index];
                if (candidate != null && !candidate.IsElite)
                {
                    return candidate;
                }
            }
            return null;
        }
    }
}
