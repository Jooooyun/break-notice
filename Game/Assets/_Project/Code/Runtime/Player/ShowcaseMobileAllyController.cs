using System;
using System.Collections.Generic;
using BreakNotice.Gameplay.Encounter;
using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    public enum ShowcaseMobileAllyState
    {
        Following,
        CatchingUp,
        FastCatchingUp,
        Regrouping,
        Fetching,
        ReturningWithSupply,
        Blocked
    }

    public enum ShowcaseAllyFollowSide
    {
        Left,
        Right
    }

    [DefaultExecutionOrder(-70)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseMobileAllyController : MonoBehaviour
    {
        private enum MovementTask
        {
            None,
            Fetch,
            Return
        }

        private const int SweepHitCapacity = 64;
        private const int OverlapHitCapacity = 64;
        private const float MinimumPlanarDistance = 0.001f;

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private AuthoredEncounterResetController resetController;

        [SerializeField]
        private Transform allyTransform;

        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private CharacterController characterController;

        [SerializeField]
        private Transform[] followAnchors = Array.Empty<Transform>();

        [SerializeField]
        private Vector2Int[] anchorLinks = Array.Empty<Vector2Int>();

        [SerializeField, Min(2.5f)]
        private float followDistance = 2.7f;

        [SerializeField, Min(0f)]
        private float lateralOffset = 0.85f;

        [SerializeField, Min(0.1f)]
        private float normalSpeed = 2.8f;

        [SerializeField, Min(0.1f)]
        private float catchUpSpeed = 4.2f;

        [SerializeField, Min(0.1f)]
        private float fastCatchUpSpeed = 6f;

        [SerializeField, Min(0.1f)]
        private float catchUpDistance = 5.5f;

        [SerializeField, Min(0.1f)]
        private float fastCatchUpDistance = 8f;

        [SerializeField, Min(0.1f)]
        private float maximumNormalDistance = 12f;

        [SerializeField, Min(0.01f)]
        private float arrivalTolerance = 0.2f;

        [SerializeField, Min(1f)]
        private float turnSpeed = 540f;

        [SerializeField, Min(0f)]
        private float groundSnapSpeed = 1.5f;

        [SerializeField, Min(0f)]
        private float blockedConfirmationTime = 0.25f;

        [SerializeField, Min(0.1f)]
        private float regroupStateDuration = 1.25f;

        [SerializeField]
        private LayerMask obstacleMask = Physics.DefaultRaycastLayers;

        private readonly RaycastHit[] sweepHits = new RaycastHit[SweepHitCapacity];
        private readonly Collider[] overlapHits = new Collider[OverlapHitCapacity];
        private Vector3 baselinePosition;
        private Quaternion baselineRotation;
        private bool baselineCaptured;
        private float blockedElapsed;
        private float regroupUntilTime;
        private int observedResetCount;
        private MovementTask movementTask;
        private Transform fetchTarget;
        private float fetchStoppingDistance;
        private float taskAdditionalClearance;
        private Transform taskIgnoredPayload;
        private bool taskReached;
        private bool taskBlocked;

        public ShowcaseMobileAllyState State { get; private set; } =
            ShowcaseMobileAllyState.Following;

        public ShowcaseAllyFollowSide FollowSide { get; private set; } =
            ShowcaseAllyFollowSide.Left;

        public Vector3 CurrentMovementTarget { get; private set; }

        public int CurrentAnchorIndex { get; private set; } = -1;

        public bool IsUsingAnchorRoute => CurrentAnchorIndex >= 0;

        public bool OwnsRegroupFallback =>
            isActiveAndEnabled &&
            interactionOwner != null &&
            playerTransform != null &&
            allyTransform != null;

        public bool HasTaskMovement => movementTask != MovementTask.None;

        public bool IsFetchTask => movementTask == MovementTask.Fetch;

        public bool IsReturnTask => movementTask == MovementTask.Return;

        public bool TaskReached => taskReached;

        public bool TaskBlocked => taskBlocked;

        public Transform FetchTarget => fetchTarget;

        public uint RegroupAttemptSequence { get; private set; }

        public bool LastRegroupAccepted { get; private set; }

        public int SideSwitchCount { get; private set; }

        public int AnchorRouteCount { get; private set; }

        public int BlockedCount { get; private set; }

        public float TotalPathLength { get; private set; }

        public float LastStepDistance { get; private set; }

        public float MaximumObservedStepDistance { get; private set; }

        public float FollowDistance => followDistance;

        public float LateralOffset => lateralOffset;

        public float NormalSpeed => normalSpeed;

        public float CatchUpSpeed => catchUpSpeed;

        public float FastCatchUpSpeed => fastCatchUpSpeed;

        public float CatchUpDistance => catchUpDistance;

        public float FastCatchUpDistance => fastCatchUpDistance;

        public float MaximumNormalDistance => maximumNormalDistance;

        public float ArrivalTolerance => arrivalTolerance;

        public int AnchorCount => followAnchors?.Length ?? 0;

        public IReadOnlyList<Transform> FollowAnchors => followAnchors;

        public IReadOnlyList<Vector2Int> AnchorLinks => anchorLinks;

        private bool IsGlobalInteractionBlocked =>
            interactionOwner == null ||
            interactionOwner.GlobalState != PrototypeGlobalInteractionState.Playable;

        private void Awake()
        {
            allyTransform ??= transform;
            characterController ??= GetComponent<CharacterController>();
            CaptureBaseline();
            observedResetCount = resetController != null ? resetController.ResetCount : 0;
        }

        private void FixedUpdate()
        {
            if (!HasRequiredBindings())
            {
                LastStepDistance = 0f;
                return;
            }

            if (resetController != null && resetController.ResetCount != observedResetCount)
            {
                observedResetCount = resetController.ResetCount;
                ResetToAuthoredBaseline(false);
                return;
            }

            if (IsGlobalInteractionBlocked)
            {
                LastStepDistance = 0f;
                return;
            }

            if (!TryResolveActiveMovementTarget(
                    out Vector3 movementTarget,
                    out int anchorIndex))
            {
                RegisterBlockedStep();
                return;
            }

            blockedElapsed = 0f;
            taskBlocked = false;
            CurrentMovementTarget = movementTarget;
            if (anchorIndex != CurrentAnchorIndex && anchorIndex >= 0)
            {
                AnchorRouteCount++;
            }
            CurrentAnchorIndex = anchorIndex;

            Vector3 toTarget = movementTarget - allyTransform.position;
            toTarget.y = 0f;
            float remainingDistance = toTarget.magnitude;
            float playerDistance = PlanarDistance(allyTransform.position, playerTransform.position);
            ShowcaseMobileAllyState distanceState = EvaluateMovementState(playerDistance);
            State = movementTask switch
            {
                MovementTask.Fetch => ShowcaseMobileAllyState.Fetching,
                MovementTask.Return => ShowcaseMobileAllyState.ReturningWithSupply,
                _ => Time.time < regroupUntilTime
                    ? ShowcaseMobileAllyState.Regrouping
                    : distanceState
            };

            if (taskReached || remainingDistance <= arrivalTolerance)
            {
                LastStepDistance = 0f;
                return;
            }

            float speed = SelectSpeed(playerDistance);
            if (State == ShowcaseMobileAllyState.Regrouping)
            {
                speed = Mathf.Max(speed, catchUpSpeed);
            }

            Vector3 planarDelta = toTarget.normalized *
                                  Mathf.Min(remainingDistance, speed * Time.fixedDeltaTime);
            Vector3 before = allyTransform.position;
            CollisionFlags flags = characterController.Move(
                planarDelta + Vector3.down * groundSnapSpeed * Time.fixedDeltaTime);
            Vector3 actual = allyTransform.position - before;
            actual.y = 0f;
            LastStepDistance = actual.magnitude;
            TotalPathLength += LastStepDistance;
            MaximumObservedStepDistance = Mathf.Max(MaximumObservedStepDistance, LastStepDistance);

            if (planarDelta.sqrMagnitude > MinimumPlanarDistance &&
                LastStepDistance < planarDelta.magnitude * 0.1f &&
                (flags & CollisionFlags.Sides) != 0)
            {
                RegisterBlockedStep();
            }

            if (actual.sqrMagnitude > MinimumPlanarDistance)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(actual.normalized, Vector3.up);
                allyTransform.rotation = Quaternion.RotateTowards(
                    allyTransform.rotation,
                    desiredRotation,
                    turnSpeed * Time.fixedDeltaTime);
            }
        }

        public void Configure(
            PrototypeInteractionStateOwner owner,
            AuthoredEncounterResetController authoredReset,
            Transform authoredAlly,
            Transform player,
            CharacterController authoredController,
            Transform[] authoredAnchors,
            Vector2Int[] authoredLinks,
            float authoredFollowDistance = 2.7f,
            float authoredLateralOffset = 0.85f,
            float authoredNormalSpeed = 2.8f,
            float authoredCatchUpSpeed = 4.2f,
            float authoredFastCatchUpSpeed = 6f,
            float authoredCatchUpDistance = 5.5f,
            float authoredFastCatchUpDistance = 8f,
            float authoredMaximumNormalDistance = 12f)
        {
            interactionOwner = owner;
            resetController = authoredReset;
            allyTransform = authoredAlly != null ? authoredAlly : transform;
            playerTransform = player;
            characterController = authoredController;
            followAnchors = authoredAnchors ?? Array.Empty<Transform>();
            anchorLinks = authoredLinks ?? Array.Empty<Vector2Int>();
            followDistance = Mathf.Max(2.5f, authoredFollowDistance);
            lateralOffset = Mathf.Max(0f, authoredLateralOffset);
            normalSpeed = Mathf.Max(0.1f, authoredNormalSpeed);
            catchUpSpeed = Mathf.Max(normalSpeed, authoredCatchUpSpeed);
            fastCatchUpSpeed = Mathf.Max(catchUpSpeed, authoredFastCatchUpSpeed);
            catchUpDistance = Mathf.Max(0.1f, authoredCatchUpDistance);
            fastCatchUpDistance = Mathf.Max(catchUpDistance, authoredFastCatchUpDistance);
            maximumNormalDistance = Mathf.Max(
                fastCatchUpDistance,
                authoredMaximumNormalDistance);
            CaptureBaseline();
            observedResetCount = resetController != null ? resetController.ResetCount : 0;
        }

        public bool TryRequestRegroup()
        {
            RegroupAttemptSequence++;
            if (!OwnsRegroupFallback || IsGlobalInteractionBlocked || HasTaskMovement)
            {
                LastRegroupAccepted = false;
                return false;
            }

            ShowcaseAllyFollowSide previousSide = FollowSide;
            FollowSide = FollowSide == ShowcaseAllyFollowSide.Left
                ? ShowcaseAllyFollowSide.Right
                : ShowcaseAllyFollowSide.Left;
            if (FollowSide != previousSide)
            {
                SideSwitchCount++;
            }

            regroupUntilTime = Time.time + regroupStateDuration;
            LastRegroupAccepted = TryResolveMovementTarget(out _, out _);
            if (!LastRegroupAccepted)
            {
                FollowSide = previousSide;
                regroupUntilTime = 0f;
            }
            return LastRegroupAccepted;
        }

        public bool TryBeginFetchMovement(Transform target, float stoppingDistance)
        {
            if (!OwnsRegroupFallback || IsGlobalInteractionBlocked || target == null ||
                HasTaskMovement)
            {
                return false;
            }

            movementTask = MovementTask.Fetch;
            fetchTarget = target;
            fetchStoppingDistance = Mathf.Max(0.5f, stoppingDistance);
            taskAdditionalClearance = 0f;
            taskIgnoredPayload = null;
            taskReached = false;
            taskBlocked = false;
            blockedElapsed = 0f;
            regroupUntilTime = 0f;
            State = ShowcaseMobileAllyState.Fetching;
            return true;
        }

        public bool TryBeginReturnMovement(
            Transform ignoredPayload,
            float additionalClearance = 0f)
        {
            if (!OwnsRegroupFallback || IsGlobalInteractionBlocked ||
                movementTask != MovementTask.Fetch)
            {
                return false;
            }

            movementTask = MovementTask.Return;
            fetchTarget = null;
            taskAdditionalClearance = Mathf.Max(0f, additionalClearance);
            taskIgnoredPayload = ignoredPayload;
            taskReached = false;
            taskBlocked = false;
            blockedElapsed = 0f;
            State = ShowcaseMobileAllyState.ReturningWithSupply;
            return true;
        }

        public void ClearTaskMovement()
        {
            movementTask = MovementTask.None;
            fetchTarget = null;
            fetchStoppingDistance = 0f;
            taskAdditionalClearance = 0f;
            taskIgnoredPayload = null;
            taskReached = false;
            taskBlocked = false;
            blockedElapsed = 0f;
            CurrentAnchorIndex = -1;
            if (State == ShowcaseMobileAllyState.Fetching ||
                State == ShowcaseMobileAllyState.ReturningWithSupply ||
                State == ShowcaseMobileAllyState.Blocked)
            {
                State = ShowcaseMobileAllyState.Following;
            }
        }

        public bool IsFetching(Transform target)
        {
            return movementTask == MovementTask.Fetch && fetchTarget == target;
        }

        public Vector3 CalculateFollowPosition(ShowcaseAllyFollowSide side)
        {
            if (playerTransform == null)
            {
                return allyTransform != null ? allyTransform.position : transform.position;
            }

            Vector3 forward = playerTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < MinimumPlanarDistance)
            {
                forward = Vector3.forward;
            }
            forward.Normalize();
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            float sideSign = side == ShowcaseAllyFollowSide.Left ? -1f : 1f;
            Vector3 position = playerTransform.position - forward * followDistance +
                               right * lateralOffset * sideSign;
            position.y = allyTransform != null
                ? allyTransform.position.y
                : playerTransform.position.y;
            return position;
        }

        public ShowcaseMobileAllyState EvaluateMovementState(float playerDistance)
        {
            if (playerDistance >= fastCatchUpDistance)
            {
                return ShowcaseMobileAllyState.FastCatchingUp;
            }
            if (playerDistance >= catchUpDistance)
            {
                return ShowcaseMobileAllyState.CatchingUp;
            }
            return ShowcaseMobileAllyState.Following;
        }

        public float SelectSpeed(float playerDistance)
        {
            return EvaluateMovementState(playerDistance) switch
            {
                ShowcaseMobileAllyState.FastCatchingUp => fastCatchUpSpeed,
                ShowcaseMobileAllyState.CatchingUp => catchUpSpeed,
                _ => normalSpeed
            };
        }

        public bool IsAnchorNetworkConnected()
        {
            int count = AnchorCount;
            if (count == 0)
            {
                return false;
            }

            var adjacency = BuildAdjacency(count);
            var visited = new bool[count];
            var queue = new Queue<int>();
            visited[0] = true;
            queue.Enqueue(0);
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                foreach (int neighbor in adjacency[current])
                {
                    if (!visited[neighbor])
                    {
                        visited[neighbor] = true;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            for (int index = 0; index < visited.Length; index++)
            {
                if (!visited[index])
                {
                    return false;
                }
            }
            return true;
        }

        public void ResetToAuthoredBaseline(bool restoreTransform = true)
        {
            if (restoreTransform && baselineCaptured && allyTransform != null)
            {
                bool wasEnabled = characterController != null && characterController.enabled;
                if (wasEnabled)
                {
                    characterController.enabled = false;
                }
                allyTransform.SetPositionAndRotation(baselinePosition, baselineRotation);
                if (wasEnabled)
                {
                    characterController.enabled = true;
                }
            }

            State = ShowcaseMobileAllyState.Following;
            FollowSide = ShowcaseAllyFollowSide.Left;
            CurrentMovementTarget = baselineCaptured ? baselinePosition : Vector3.zero;
            CurrentAnchorIndex = -1;
            blockedElapsed = 0f;
            regroupUntilTime = 0f;
            RegroupAttemptSequence = 0;
            LastRegroupAccepted = false;
            SideSwitchCount = 0;
            AnchorRouteCount = 0;
            BlockedCount = 0;
            TotalPathLength = 0f;
            LastStepDistance = 0f;
            MaximumObservedStepDistance = 0f;
            movementTask = MovementTask.None;
            fetchTarget = null;
            fetchStoppingDistance = 0f;
            taskAdditionalClearance = 0f;
            taskIgnoredPayload = null;
            taskReached = false;
            taskBlocked = false;
        }

        private bool HasRequiredBindings()
        {
            return interactionOwner != null &&
                   allyTransform != null &&
                   playerTransform != null &&
                   characterController != null &&
                   characterController.enabled;
        }

        private void CaptureBaseline()
        {
            if (allyTransform == null)
            {
                return;
            }
            baselinePosition = allyTransform.position;
            baselineRotation = allyTransform.rotation;
            baselineCaptured = true;
            CurrentMovementTarget = baselinePosition;
        }

        private bool TryResolveMovementTarget(out Vector3 target, out int anchorIndex)
        {
            target = CalculateFollowPosition(FollowSide);
            anchorIndex = -1;
            if (HasClearPath(allyTransform.position, target))
            {
                return true;
            }

            ShowcaseAllyFollowSide alternateSide = FollowSide == ShowcaseAllyFollowSide.Left
                ? ShowcaseAllyFollowSide.Right
                : ShowcaseAllyFollowSide.Left;
            Vector3 alternateTarget = CalculateFollowPosition(alternateSide);
            if (HasClearPath(allyTransform.position, alternateTarget))
            {
                FollowSide = alternateSide;
                SideSwitchCount++;
                target = alternateTarget;
                return true;
            }

            bool hasPreferredRoute = TryPlanAnchorRoute(
                target,
                out int preferredAnchor,
                out int preferredLength);
            bool hasAlternateRoute = TryPlanAnchorRoute(
                alternateTarget,
                out int alternateAnchor,
                out int alternateLength);
            if (hasPreferredRoute || hasAlternateRoute)
            {
                bool useAlternate = !hasPreferredRoute ||
                                    (hasAlternateRoute && alternateLength < preferredLength);
                if (useAlternate)
                {
                    if (FollowSide != alternateSide)
                    {
                        FollowSide = alternateSide;
                        SideSwitchCount++;
                    }
                    anchorIndex = alternateAnchor;
                }
                else
                {
                    anchorIndex = preferredAnchor;
                }

                if (anchorIndex >= 0 && anchorIndex < AnchorCount && followAnchors[anchorIndex] != null)
                {
                    target = followAnchors[anchorIndex].position;
                    target.y = allyTransform.position.y;
                    return true;
                }
            }

            return false;
        }

        private bool TryResolveActiveMovementTarget(
            out Vector3 target,
            out int anchorIndex)
        {
            taskReached = false;
            if (movementTask == MovementTask.None)
            {
                return TryResolveMovementTarget(out target, out anchorIndex);
            }

            if (movementTask == MovementTask.Fetch)
            {
                target = allyTransform.position;
                anchorIndex = -1;
                if (fetchTarget == null)
                {
                    return false;
                }

                Vector3 toFetch = fetchTarget.position - allyTransform.position;
                toFetch.y = 0f;
                float distance = toFetch.magnitude;
                if (distance <= fetchStoppingDistance + arrivalTolerance)
                {
                    taskReached = true;
                    return true;
                }

                Vector3 desired = fetchTarget.position -
                                  toFetch.normalized * fetchStoppingDistance;
                desired.y = allyTransform.position.y;
                return TryResolveTaskDestination(desired, out target, out anchorIndex);
            }

            Vector3 preferred = CalculateFollowPosition(FollowSide);
            if (PlanarDistance(allyTransform.position, preferred) <= arrivalTolerance)
            {
                target = allyTransform.position;
                anchorIndex = -1;
                taskReached = true;
                return true;
            }

            if (TryResolveTaskDestination(preferred, out target, out anchorIndex))
            {
                return true;
            }

            ShowcaseAllyFollowSide alternate = FollowSide == ShowcaseAllyFollowSide.Left
                ? ShowcaseAllyFollowSide.Right
                : ShowcaseAllyFollowSide.Left;
            Vector3 alternateTarget = CalculateFollowPosition(alternate);
            if (!TryResolveTaskDestination(alternateTarget, out target, out anchorIndex))
            {
                return false;
            }

            FollowSide = alternate;
            SideSwitchCount++;
            return true;
        }

        private bool TryResolveTaskDestination(
            Vector3 desired,
            out Vector3 target,
            out int anchorIndex)
        {
            target = desired;
            anchorIndex = -1;
            if (HasClearPath(allyTransform.position, desired))
            {
                return true;
            }

            if (!TryPlanAnchorRoute(desired, out anchorIndex, out _))
            {
                return false;
            }

            target = followAnchors[anchorIndex].position;
            target.y = allyTransform.position.y;
            return true;
        }

        private bool TryPlanAnchorRoute(Vector3 desired, out int nextAnchor, out int pathLength)
        {
            nextAnchor = -1;
            pathLength = int.MaxValue;
            int count = AnchorCount;
            if (count == 0 || !IsAnchorNetworkConnected())
            {
                return false;
            }

            var starts = new List<int>();
            var goals = new HashSet<int>();
            for (int index = 0; index < count; index++)
            {
                Transform anchor = followAnchors[index];
                if (anchor == null)
                {
                    continue;
                }
                if (HasClearPath(allyTransform.position, anchor.position))
                {
                    starts.Add(index);
                }
                if (HasClearPath(anchor.position, desired))
                {
                    goals.Add(index);
                }
            }

            starts.Sort((left, right) =>
            {
                float leftDistance = PlanarDistance(allyTransform.position, followAnchors[left].position);
                float rightDistance = PlanarDistance(allyTransform.position, followAnchors[right].position);
                int compare = leftDistance.CompareTo(rightDistance);
                return compare != 0 ? compare : left.CompareTo(right);
            });
            if (starts.Count == 0 || goals.Count == 0)
            {
                return false;
            }

            List<int>[] adjacency = BuildAdjacency(count);
            int[] previous = new int[count];
            int[] distance = new int[count];
            for (int index = 0; index < count; index++)
            {
                previous[index] = -1;
                distance[index] = int.MaxValue;
            }

            var queue = new Queue<int>();
            foreach (int start in starts)
            {
                if (distance[start] == 0)
                {
                    continue;
                }
                distance[start] = 0;
                queue.Enqueue(start);
            }

            int foundGoal = -1;
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                if (goals.Contains(current))
                {
                    foundGoal = current;
                    break;
                }

                foreach (int neighbor in adjacency[current])
                {
                    if (distance[neighbor] != int.MaxValue ||
                        !HasClearPath(followAnchors[current].position, followAnchors[neighbor].position))
                    {
                        continue;
                    }
                    previous[neighbor] = current;
                    distance[neighbor] = distance[current] + 1;
                    queue.Enqueue(neighbor);
                }
            }

            if (foundGoal < 0)
            {
                return false;
            }

            int routeStart = foundGoal;
            int routeNext = foundGoal;
            while (previous[routeStart] >= 0)
            {
                routeNext = routeStart;
                routeStart = previous[routeStart];
            }

            if (PlanarDistance(allyTransform.position, followAnchors[routeStart].position) >
                arrivalTolerance * 2f)
            {
                routeNext = routeStart;
            }

            nextAnchor = routeNext;
            pathLength = distance[foundGoal];
            return true;
        }

        private List<int>[] BuildAdjacency(int count)
        {
            var adjacency = new List<int>[count];
            for (int index = 0; index < count; index++)
            {
                adjacency[index] = new List<int>();
            }

            foreach (Vector2Int link in anchorLinks ?? Array.Empty<Vector2Int>())
            {
                if (link.x < 0 || link.y < 0 || link.x >= count || link.y >= count ||
                    link.x == link.y)
                {
                    continue;
                }
                if (!adjacency[link.x].Contains(link.y))
                {
                    adjacency[link.x].Add(link.y);
                }
                if (!adjacency[link.y].Contains(link.x))
                {
                    adjacency[link.y].Add(link.x);
                }
            }

            foreach (List<int> neighbors in adjacency)
            {
                neighbors.Sort();
            }
            return adjacency;
        }

        private bool HasClearPath(Vector3 from, Vector3 to)
        {
            Vector3 displacement = to - from;
            displacement.y = 0f;
            float distance = displacement.magnitude;
            if (distance <= arrivalTolerance)
            {
                return IsCapsulePositionClear(to);
            }

            GetCapsuleAt(from, out Vector3 bottom, out Vector3 top, out float radius);
            int hitCount = Physics.CapsuleCastNonAlloc(
                bottom,
                top,
                radius * 0.92f,
                displacement / distance,
                sweepHits,
                Mathf.Max(0f, distance - arrivalTolerance),
                obstacleMask,
                QueryTriggerInteraction.Ignore);
            if (hitCount >= sweepHits.Length)
            {
                return false;
            }
            for (int index = 0; index < hitCount; index++)
            {
                Collider collider = sweepHits[index].collider;
                if (collider != null && !IsOwnedCollider(collider))
                {
                    return false;
                }
            }
            return IsCapsulePositionClear(to);
        }

        private bool IsCapsulePositionClear(Vector3 position)
        {
            GetCapsuleAt(position, out Vector3 bottom, out Vector3 top, out float radius);
            int count = Physics.OverlapCapsuleNonAlloc(
                bottom,
                top,
                radius * 0.9f,
                overlapHits,
                obstacleMask,
                QueryTriggerInteraction.Ignore);
            if (count >= overlapHits.Length)
            {
                return false;
            }
            for (int index = 0; index < count; index++)
            {
                Collider collider = overlapHits[index];
                if (collider != null && !IsOwnedCollider(collider))
                {
                    return false;
                }
            }
            return true;
        }

        private void GetCapsuleAt(
            Vector3 rootPosition,
            out Vector3 bottom,
            out Vector3 top,
            out float radius)
        {
            Vector3 scale = allyTransform.lossyScale;
            radius = characterController.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            if (HasTaskMovement)
            {
                radius += taskAdditionalClearance;
            }
            float height = Mathf.Max(
                characterController.height * Mathf.Abs(scale.y),
                radius * 2f);
            Vector3 center = rootPosition + Vector3.Scale(characterController.center, scale);
            float halfSegment = Mathf.Max(0f, height * 0.5f - radius);
            bottom = center - Vector3.up * halfSegment;
            top = center + Vector3.up * halfSegment;
        }

        private bool IsOwnedCollider(Collider collider)
        {
            return collider.transform == allyTransform ||
                   collider.transform.IsChildOf(allyTransform) ||
                   (taskIgnoredPayload != null &&
                    (collider.transform == taskIgnoredPayload ||
                     collider.transform.IsChildOf(taskIgnoredPayload)));
        }

        private void RegisterBlockedStep()
        {
            LastStepDistance = 0f;
            blockedElapsed += Time.fixedDeltaTime;
            CurrentAnchorIndex = -1;
            if (blockedElapsed >= blockedConfirmationTime && State != ShowcaseMobileAllyState.Blocked)
            {
                State = ShowcaseMobileAllyState.Blocked;
                taskBlocked = HasTaskMovement;
                BlockedCount++;
            }
        }

        private static float PlanarDistance(Vector3 first, Vector3 second)
        {
            first.y = 0f;
            second.y = 0f;
            return Vector3.Distance(first, second);
        }
    }
}
