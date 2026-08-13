using System;
using System.Collections.Generic;
using BreakNotice.Gameplay.Input;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Gameplay.Encounter
{
    public enum EncounterResetCause
    {
        ManualInput,
        AutomaticFailureRestart
    }

    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    public sealed class AuthoredEncounterResetController : MonoBehaviour
    {
        [SerializeField]
        private InputSystemLogicalInputAdapter logicalInput;

        [SerializeField]
        private PrototypeInteractionStateOwner interactionOwner;

        [SerializeField]
        private PrototypePhysicalObjectLifecycle physicalObjectLifecycle;

        [SerializeField]
        private PrototypeAllyRelayController allyRelayController;

        [SerializeField]
        private ThirdPersonLocomotion locomotion;

        [SerializeField]
        private Transform playerTransform;

        [SerializeField]
        private Transform[] authoredActors = Array.Empty<Transform>();

        [SerializeField]
        private PrototypeInteractionTarget[] authoredObjects =
            Array.Empty<PrototypeInteractionTarget>();

        [SerializeField, Min(1)]
        private int expectedObjectCount = 7;

        private TransformBaseline playerBaseline;
        private TransformBaseline[] actorBaselines = Array.Empty<TransformBaseline>();
        private ObjectBaseline[] objectBaselines = Array.Empty<ObjectBaseline>();
        private bool baselineCaptured;
        private bool completionPending;

        public EncounterResetCause LastResetCause { get; private set; }

        public int ResetCount { get; private set; }

        public int UnexpectedBaselineDifferenceCount { get; private set; }

        public int DuplicateHandlerCount { get; private set; }

        public int ObjectLossCount { get; private set; }

        public int ObjectDuplicationCount { get; private set; }

        public int OwnershipLeakCount { get; private set; }

        public int FaultQuarantineCount { get; private set; }

        public int SubscriptionCount => 0;

        public event Action<EncounterResetCause> ResetPerformed;

        public int BaselineObjectCount => objectBaselines.Length;

        public int ExpectedObjectCount => expectedObjectCount;

        public bool IsResetInProgress => completionPending;

        private void Awake()
        {
            CaptureBaselineIfReady();
        }

        private void Update()
        {
            if (logicalInput != null && logicalInput.Snapshot.ResetEncounter)
            {
                RequestReset(EncounterResetCause.ManualInput);
            }
        }

        private void LateUpdate()
        {
            if (!completionPending)
            {
                return;
            }

            bool restored = interactionOwner != null &&
                            interactionOwner.RestoreInteractionBaseline();
            if (!restored || !HasBaselineOwnership())
            {
                OwnershipLeakCount++;
            }

            completionPending = false;
        }

        public void Configure(
            InputSystemLogicalInputAdapter input,
            PrototypeInteractionStateOwner owner,
            PrototypePhysicalObjectLifecycle lifecycle,
            ThirdPersonLocomotion playerLocomotion,
            Transform player,
            Transform[] actors,
            PrototypeInteractionTarget[] objects,
            PrototypeAllyRelayController relayController = null,
            int authoredExpectedObjectCount = 7)
        {
            logicalInput = input;
            interactionOwner = owner;
            physicalObjectLifecycle = lifecycle;
            locomotion = playerLocomotion;
            playerTransform = player;
            authoredActors = actors ?? Array.Empty<Transform>();
            authoredObjects = objects ?? Array.Empty<PrototypeInteractionTarget>();
            allyRelayController = relayController;
            expectedObjectCount = Mathf.Max(1, authoredExpectedObjectCount);

            if (Application.isPlaying)
            {
                CaptureBaseline();
            }
        }

        public void CaptureBaseline()
        {
            if (playerTransform == null || interactionOwner == null)
            {
                throw new InvalidOperationException(
                    "Player transform and interaction owner are required for authored Reset.");
            }

            if (authoredObjects.Length != expectedObjectCount)
            {
                throw new InvalidOperationException(
                    $"The authored Reset requires exactly {expectedObjectCount} objects, " +
                    $"found {authoredObjects.Length}.");
            }

            if (authoredActors.Length != 3)
            {
                throw new InvalidOperationException(
                    $"The authored prototype Reset requires ally plus two runners, found {authoredActors.Length}.");
            }

            playerBaseline = TransformBaseline.Capture(playerTransform);
            actorBaselines = new TransformBaseline[authoredActors.Length];
            for (int index = 0; index < authoredActors.Length; index++)
            {
                actorBaselines[index] = TransformBaseline.Capture(authoredActors[index]);
            }

            objectBaselines = new ObjectBaseline[authoredObjects.Length];
            for (int index = 0; index < authoredObjects.Length; index++)
            {
                objectBaselines[index] = ObjectBaseline.Capture(authoredObjects[index]);
            }
            baselineCaptured = true;
        }

        public bool RequestReset(EncounterResetCause cause)
        {
            if (!baselineCaptured || completionPending || interactionOwner == null)
            {
                return false;
            }

            if (!interactionOwner.TryBeginReset())
            {
                return false;
            }

            completionPending = true;
            LastResetCause = cause;
            ResetCount++;

            physicalObjectLifecycle?.DetachForReset();
            allyRelayController?.ResetToAuthoredBaseline();
            RestorePlayer();

            foreach (TransformBaseline baseline in actorBaselines)
            {
                baseline.Restore();
            }

            foreach (ObjectBaseline baseline in objectBaselines)
            {
                baseline.Restore();
            }

            physicalObjectLifecycle?.ClearThrowOriginForReset();
            Physics.SyncTransforms();
            AuditRestoredBaseline();
            ResetPerformed?.Invoke(cause);
            return true;
        }

        private void CaptureBaselineIfReady()
        {
            if (playerTransform != null &&
                interactionOwner != null &&
                authoredActors != null &&
                authoredActors.Length == 3 &&
                authoredObjects != null &&
                authoredObjects.Length == expectedObjectCount)
            {
                CaptureBaseline();
            }
        }

        private void RestorePlayer()
        {
            CharacterController controller = playerTransform.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (wasEnabled)
            {
                controller.enabled = false;
            }

            playerBaseline.Restore();
            locomotion?.ResetMotionState();

            if (wasEnabled)
            {
                controller.enabled = true;
            }
        }

        private void AuditRestoredBaseline()
        {
            int differences = playerBaseline.HasDifference() ? 1 : 0;
            for (int index = 0; index < actorBaselines.Length; index++)
            {
                if (actorBaselines[index].HasDifference())
                {
                    differences++;
                }
            }

            for (int index = 0; index < objectBaselines.Length; index++)
            {
                ObjectBaseline baseline = objectBaselines[index];
                if (baseline.Target == null)
                {
                    ObjectLossCount++;
                }
                else if (baseline.HasDifference())
                {
                    differences++;
                }
            }

            UnexpectedBaselineDifferenceCount += differences;

            Scene scene = gameObject.scene;
            if (scene.IsValid() && scene.isLoaded)
            {
                var keyCounts = new Dictionary<string, int>(StringComparer.Ordinal);
                int duplicateKeys = 0;
                GameObject[] roots = scene.GetRootGameObjects();
                for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                {
                    PrototypeInteractionTarget[] sceneTargets =
                        roots[rootIndex].GetComponentsInChildren<PrototypeInteractionTarget>(true);
                    for (int targetIndex = 0; targetIndex < sceneTargets.Length; targetIndex++)
                    {
                        PrototypeInteractionTarget target = sceneTargets[targetIndex];
                        if (target == null)
                        {
                            continue;
                        }

                        string key = target.RuntimeInstanceKey ?? string.Empty;
                        if (keyCounts.TryGetValue(key, out int count))
                        {
                            keyCounts[key] = count + 1;
                            duplicateKeys++;
                        }
                        else
                        {
                            keyCounts.Add(key, 1);
                        }
                    }
                }

                ObjectDuplicationCount += duplicateKeys;
            }

            FaultQuarantineCount = 0;
            DuplicateHandlerCount = 0;
        }

        private bool HasBaselineOwnership()
        {
            return interactionOwner.GlobalState == PrototypeGlobalInteractionState.Playable &&
                   interactionOwner.HandState == PrototypeHandOwnershipState.Free &&
                   interactionOwner.StrapState == PrototypeStrapOwnershipState.Stored &&
                   interactionOwner.RelayState == PrototypeRelayReservationState.None &&
                   interactionOwner.CarriedTarget == null &&
                   interactionOwner.ThrowCommittedTarget == null &&
                   interactionOwner.StrapTarget == null &&
                   interactionOwner.RelayReservedTarget == null &&
                   !interactionOwner.QueuedThrowArmed &&
                   (allyRelayController == null || allyRelayController.HasBaselineState);
        }

        private readonly struct TransformBaseline
        {
            private TransformBaseline(
                Transform target,
                Vector3 position,
                Quaternion rotation,
                Vector3 scale,
                bool active)
            {
                Target = target;
                Position = position;
                Rotation = rotation;
                Scale = scale;
                Active = active;
            }

            public Transform Target { get; }
            private Vector3 Position { get; }
            private Quaternion Rotation { get; }
            private Vector3 Scale { get; }
            private bool Active { get; }

            public static TransformBaseline Capture(Transform target)
            {
                if (target == null)
                {
                    throw new InvalidOperationException("An authored Reset target is missing.");
                }

                return new TransformBaseline(
                    target,
                    target.position,
                    target.rotation,
                    target.localScale,
                    target.gameObject.activeSelf);
            }

            public void Restore()
            {
                if (Target == null)
                {
                    return;
                }

                Target.gameObject.SetActive(Active);
                Target.SetPositionAndRotation(Position, Rotation);
                Target.localScale = Scale;
            }

            public bool HasDifference()
            {
                return Target == null ||
                       Target.gameObject.activeSelf != Active ||
                       Vector3.Distance(Target.position, Position) > 0.001f ||
                       Quaternion.Angle(Target.rotation, Rotation) > 0.1f ||
                       Vector3.Distance(Target.localScale, Scale) > 0.001f;
            }
        }

        private readonly struct ObjectBaseline
        {
            private ObjectBaseline(
                PrototypeInteractionTarget target,
                TransformBaseline transform,
                Rigidbody body,
                bool isKinematic,
                bool useGravity,
                RigidbodyConstraints constraints,
                RigidbodyInterpolation interpolation,
                CollisionDetectionMode collisionMode,
                Vector3 velocity,
                Vector3 angularVelocity,
                bool sleeping)
            {
                Target = target;
                Transform = transform;
                Body = body;
                IsKinematic = isKinematic;
                UseGravity = useGravity;
                Constraints = constraints;
                Interpolation = interpolation;
                CollisionMode = collisionMode;
                Velocity = velocity;
                AngularVelocity = angularVelocity;
                Sleeping = sleeping;
            }

            public PrototypeInteractionTarget Target { get; }
            private TransformBaseline Transform { get; }
            private Rigidbody Body { get; }
            private bool IsKinematic { get; }
            private bool UseGravity { get; }
            private RigidbodyConstraints Constraints { get; }
            private RigidbodyInterpolation Interpolation { get; }
            private CollisionDetectionMode CollisionMode { get; }
            private Vector3 Velocity { get; }
            private Vector3 AngularVelocity { get; }
            private bool Sleeping { get; }

            public static ObjectBaseline Capture(PrototypeInteractionTarget target)
            {
                if (target == null)
                {
                    throw new InvalidOperationException("An authored object target is missing.");
                }

                Rigidbody body = target.GetComponent<Rigidbody>();
                return new ObjectBaseline(
                    target,
                    TransformBaseline.Capture(target.transform),
                    body,
                    body != null && body.isKinematic,
                    body != null && body.useGravity,
                    body != null ? body.constraints : RigidbodyConstraints.None,
                    body != null ? body.interpolation : RigidbodyInterpolation.None,
                    body != null ? body.collisionDetectionMode : CollisionDetectionMode.Discrete,
                    body != null ? body.linearVelocity : Vector3.zero,
                    body != null ? body.angularVelocity : Vector3.zero,
                    body != null && body.IsSleeping());
            }

            public void Restore()
            {
                if (Target == null)
                {
                    return;
                }

                if (Body == null)
                {
                    Transform.Restore();
                    return;
                }

                Body.isKinematic = true;
                Body.interpolation = RigidbodyInterpolation.None;
                Transform.Restore();
                Vector3 restoredPosition = Target.transform.position;
                Quaternion restoredRotation = Target.transform.rotation;
                Body.position = restoredPosition;
                Body.rotation = restoredRotation;
                Body.constraints = Constraints;
                Body.collisionDetectionMode = CollisionMode;
                Body.useGravity = UseGravity;
                Body.isKinematic = IsKinematic;
                if (!Body.isKinematic)
                {
                    Body.linearVelocity = Velocity;
                    Body.angularVelocity = AngularVelocity;
                }
                if (Sleeping)
                {
                    Body.Sleep();
                }
                else
                {
                    Body.WakeUp();
                }

                // Reset the interpolation history as well as the physics pose.
                // Otherwise a released relay object can render one stale flight
                // frame after authored Reset even though Rigidbody.position was
                // already restored.
                Body.position = restoredPosition;
                Body.rotation = restoredRotation;
                Target.transform.SetPositionAndRotation(restoredPosition, restoredRotation);
                Body.interpolation = Interpolation;
            }

            public bool HasDifference()
            {
                if (Target == null || Transform.HasDifference())
                {
                    return true;
                }

                if (Body == null)
                {
                    return false;
                }

                return Body.isKinematic != IsKinematic ||
                       Body.useGravity != UseGravity ||
                       Body.constraints != Constraints ||
                       Body.interpolation != Interpolation ||
                       Body.collisionDetectionMode != CollisionMode ||
                       Vector3.Distance(Body.linearVelocity, Velocity) > 0.001f ||
                       Vector3.Distance(Body.angularVelocity, AngularVelocity) > 0.001f ||
                       Body.IsSleeping() != Sleeping;
            }
        }
    }
}
