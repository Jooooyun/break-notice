using System;
using System.Collections.Generic;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    public enum FinalGameplayAudioImpactFamily
    {
        None,
        BinderEnemy,
        BinderWall,
        BinderFloor,
        ChairEnemy,
        ChairWorld
    }

    internal enum FinalGameplayVoiceTier
    {
        None,
        Light,
        Heavy,
        Ko
    }

    [DefaultExecutionOrder(140)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseFinalGameplayAudioController : MonoBehaviour
    {
        private const float DspScheduleLeadSeconds = 0.025f;
        private const float MotionLoopFadeSeconds = 0.08f;
        private const float StopConfirmationSeconds = 0.16f;
        private const float StopAfterImpactGuardSeconds = 0.14f;
        private const float MinimumGroundProbeDistance = 0.22f;
        private const int MaximumGroundHits = 12;
        private const string EnemyFootstepSourceName =
            "FINAL_AUDIO_ENEMY_FOOTSTEP";
        private const string EnemyVoiceSourceName =
            "FINAL_AUDIO_ENEMY_VOICE";

        private static readonly RaycastHit[] GroundHits =
            new RaycastHit[MaximumGroundHits];

        [Header("Authority")]
        [SerializeField] private FinalGameplayAudioProfile profile;
        [SerializeField] private ShowcasePlayerCombatController playerCombat;
        [SerializeField] private ShowcaseCombatHealth playerHealth;
        [SerializeField] private ThirdPersonLocomotion locomotion;
        [SerializeField] private PrototypePhysicalObjectLifecycle physicalLifecycle;
        [SerializeField] private PrototypeAllyRelayController allyRelay;
        [SerializeField] private ShowcaseCargoStrapController cargoStrap;
        [SerializeField] private ShowcaseRunnerPressureController pressureController;
        [SerializeField] private ShowcaseRunnerAgent[] runners = Array.Empty<ShowcaseRunnerAgent>();
        [SerializeField] private PrototypeInteractionTarget[] targets = Array.Empty<PrototypeInteractionTarget>();
        [SerializeField] private ShowcaseAnimationEventReceiver[] receivers = Array.Empty<ShowcaseAnimationEventReceiver>();

        [Header("Authored Sources")]
        [SerializeField] private AudioSource[] transientSources = Array.Empty<AudioSource>();
        [SerializeField] private AudioSource strapPart1Source;
        [SerializeField] private AudioSource strapPart2Source;
        [SerializeField] private AudioSource playerFootstepSource;
        [SerializeField] private AudioSource[] enemyFootstepSources = Array.Empty<AudioSource>();
        [SerializeField] private AudioSource playerVoiceSource;
        [SerializeField] private AudioSource[] enemyVoiceSources = Array.Empty<AudioSource>();
        [SerializeField] private PrototypeInteractionTarget[] chairTargets = Array.Empty<PrototypeInteractionTarget>();
        [SerializeField] private AudioSource[] chairRollSources = Array.Empty<AudioSource>();
        [SerializeField] private PrototypeInteractionTarget[] lightTargets = Array.Empty<PrototypeInteractionTarget>();
        [SerializeField] private AudioSource[] smallSpinSources = Array.Empty<AudioSource>();

        [Header("One-shot")]
        [SerializeField, Min(0f)] private float minimumImpactSpeed = 0.8f;
        [SerializeField, Min(0.01f)] private float objectImpactCooldown = 0.10f;
        [SerializeField, Range(-1f, 1f)] private float floorNormalThreshold = 0.65f;
        [SerializeField, Range(0f, 1f)] private float foleyVolume = 0.70f;
        [SerializeField, Range(0f, 1f)] private float impactVolume = 0.78f;
        [SerializeField, Range(0f, 1f)] private float motionOneShotVolume = 0.68f;

        [Header("Chair Motion")]
        [SerializeField, Min(0f)] private float chairRollStartSpeed = 0.75f;
        [SerializeField, Min(0f)] private float chairRollStopSpeed = 0.30f;
        [SerializeField, Min(0.1f)] private float chairRollFullVolumeSpeed = 4.5f;
        [SerializeField, Range(0f, 1f)] private float chairRollMaximumVolume = 0.55f;

        [Header("Small Spin")]
        [SerializeField, Min(0f)] private float smallSpinStartAngularSpeed = 1.2f;
        [SerializeField, Min(0f)] private float smallSpinStopAngularSpeed = 0.5f;
        [SerializeField, Min(0.1f)] private float smallSpinFullVolumeAngularSpeed = 10f;
        [SerializeField, Range(0f, 1f)] private float smallSpinMaximumVolume = 0.58f;

        [Header("Footsteps")]
        [SerializeField, Range(0f, 1f)] private float playerFootstepVolume = 0.48f;
        [SerializeField, Range(0f, 1f)] private float enemyFootstepVolume = 0.44f;
        [SerializeField, Min(1)] private int maximumEnemyFootstepVoices = 4;

        [Header("Voices")]
        [SerializeField, Range(0f, 1f)] private float playerLightHurtProbability = 0.38f;
        [SerializeField, Range(0f, 1f)] private float playerHeavyHurtProbability = 0.78f;
        [SerializeField, Range(0f, 1f)] private float enemyLightHurtProbability = 0.33f;
        [SerializeField, Min(0f)] private float playerVoiceCooldown = 0.75f;
        [SerializeField, Min(0f)] private float enemyVoiceCooldown = 0.82f;
        [SerializeField, Min(1)] private int maximumEnemyVoiceConcurrency = 4;
        [SerializeField, Min(1)] private int heavyDamageThreshold = 25;
        [SerializeField, Range(0f, 1f)] private float playerVoiceVolume = 0.76f;
        [SerializeField, Range(0f, 1f)] private float enemyVoiceVolume = 0.72f;

        private readonly Dictionary<int, TargetRuntime> targetRuntime = new();
        private readonly Dictionary<int, ActorRuntime> actorRuntime = new();
        private readonly Dictionary<int, PendingImpact> pendingImpacts = new();
        private readonly Dictionary<ShowcaseCombatHealth, Action<ShowcaseDamageResult>>
            damageHandlers = new();
        private readonly HashSet<int> queuedCatchPendingSettlement = new();
        private TransientVoice[] transientVoices = Array.Empty<TransientVoice>();
        private int lastAcquisitionCatchTargetId;
        private int lastAcquisitionCatchFrame = -1;

        public FinalGameplayAudioProfile Profile => profile;
        public ShowcasePlayerCombatController PlayerCombat => playerCombat;
        public ShowcaseCombatHealth PlayerHealth => playerHealth;
        public ThirdPersonLocomotion Locomotion => locomotion;
        public PrototypePhysicalObjectLifecycle PhysicalLifecycle => physicalLifecycle;
        public PrototypeAllyRelayController AllyRelay => allyRelay;
        public ShowcaseCargoStrapController CargoStrap => cargoStrap;
        public ShowcaseRunnerPressureController PressureController => pressureController;
        public ShowcaseRunnerAgent[] Runners => ResolveRuntimeRunners();
        public PrototypeInteractionTarget[] Targets => targets;
        public ShowcaseAnimationEventReceiver[] Receivers => receivers;
        public AudioSource[] TransientSources => transientSources;
        public AudioSource StrapPart1Source => strapPart1Source;
        public AudioSource StrapPart2Source => strapPart2Source;
        public AudioSource PlayerFootstepSource => playerFootstepSource;
        public AudioSource[] EnemyFootstepSources => enemyFootstepSources;
        public AudioSource PlayerVoiceSource => playerVoiceSource;
        public AudioSource[] EnemyVoiceSources => enemyVoiceSources;
        public PrototypeInteractionTarget[] ChairTargets => chairTargets;
        public AudioSource[] ChairRollSources => chairRollSources;
        public PrototypeInteractionTarget[] LightTargets => lightTargets;
        public AudioSource[] SmallSpinSources => smallSpinSources;
        public int OneShotPlaybackCount { get; private set; }
        public int FootstepPlaybackCount { get; private set; }
        public int VoicePlaybackCount { get; private set; }
        public int LoopStartCount { get; private set; }
        public int LoopStopCount { get; private set; }
        public int DuplicateSuppressionCount { get; private set; }
        public int VoiceCooldownSuppressionCount { get; private set; }
        public int StrapSequencePlaybackCount { get; private set; }
        public AudioClip LastPlayedClip { get; private set; }
        public FinalGameplayAudioImpactFamily LastImpactFamily { get; private set; }

        private sealed class TransientVoice
        {
            public AudioSource Source;
            public double BusyUntil;
        }

        private sealed class TargetRuntime
        {
            public PrototypeInteractionTarget Target;
            public Rigidbody Body;
            public Collider Collider;
            public AudioSource LoopSource;
            public bool IsChair;
            public bool IsLight;
            public bool BinderFloorArmed;
            public bool BinderFloorConsumed;
            public bool RollPlaying;
            public bool SpinPlaying;
            public bool WasMoving;
            public bool StopCandidate;
            public float StopCandidateStartedAt;
            public float LastImpactAt = float.NegativeInfinity;
            public int LastImpactFrame = -1;
            public int LastAcceptedEnemyImpactFrame = -1;
            public int LastStopIndex = -1;
            public float ReleaseAuthorityUntil = float.NegativeInfinity;
        }

        private sealed class ActorRuntime
        {
            public Transform Actor;
            public ShowcaseCombatHealth Health;
            public ShowcaseAnimationEventReceiver Receiver;
            public ThirdPersonLocomotion Locomotion;
            public ShowcaseRunnerAgent Runner;
            public AudioSource FootstepSource;
            public AudioSource VoiceSource;
            public int LastFootstepIndex = -1;
            public int LastLightVoiceIndex = -1;
            public int LastKoIndex = -1;
            public int LastFootstepFrame = -1;
            public int LastFoot = -1;
            public float VoiceCooldownUntil = float.NegativeInfinity;
            public uint RandomState;
            public FinalGameplayVoiceTier CurrentVoiceTier;
        }

        private readonly struct PendingImpact
        {
            public PendingImpact(
                PrototypeInteractionCollisionSnapshot snapshot,
                FinalGameplayAudioImpactFamily family)
            {
                Snapshot = snapshot;
                Family = family;
            }

            public PrototypeInteractionCollisionSnapshot Snapshot { get; }
            public FinalGameplayAudioImpactFamily Family { get; }
        }

        public void Configure(
            FinalGameplayAudioProfile authoredProfile,
            ShowcasePlayerCombatController authoredPlayerCombat,
            ShowcaseCombatHealth authoredPlayerHealth,
            ThirdPersonLocomotion authoredLocomotion,
            PrototypePhysicalObjectLifecycle authoredPhysicalLifecycle,
            PrototypeAllyRelayController authoredAllyRelay,
            ShowcaseCargoStrapController authoredCargoStrap,
            ShowcaseRunnerPressureController authoredPressureController,
            ShowcaseRunnerAgent[] authoredRunners,
            PrototypeInteractionTarget[] authoredTargets,
            ShowcaseAnimationEventReceiver[] authoredReceivers,
            AudioSource[] authoredTransientSources,
            AudioSource authoredStrapPart1Source,
            AudioSource authoredStrapPart2Source,
            AudioSource authoredPlayerFootstepSource,
            AudioSource[] authoredEnemyFootstepSources,
            AudioSource authoredPlayerVoiceSource,
            AudioSource[] authoredEnemyVoiceSources,
            PrototypeInteractionTarget[] authoredChairTargets,
            AudioSource[] authoredChairRollSources,
            PrototypeInteractionTarget[] authoredLightTargets,
            AudioSource[] authoredSmallSpinSources)
        {
            Unsubscribe();
            profile = authoredProfile;
            playerCombat = authoredPlayerCombat;
            playerHealth = authoredPlayerHealth;
            locomotion = authoredLocomotion;
            physicalLifecycle = authoredPhysicalLifecycle;
            allyRelay = authoredAllyRelay;
            cargoStrap = authoredCargoStrap;
            pressureController = authoredPressureController;
            runners = authoredRunners ?? Array.Empty<ShowcaseRunnerAgent>();
            targets = authoredTargets ?? Array.Empty<PrototypeInteractionTarget>();
            receivers = authoredReceivers ?? Array.Empty<ShowcaseAnimationEventReceiver>();
            transientSources = authoredTransientSources ?? Array.Empty<AudioSource>();
            strapPart1Source = authoredStrapPart1Source;
            strapPart2Source = authoredStrapPart2Source;
            playerFootstepSource = authoredPlayerFootstepSource;
            enemyFootstepSources = authoredEnemyFootstepSources ?? Array.Empty<AudioSource>();
            playerVoiceSource = authoredPlayerVoiceSource;
            enemyVoiceSources = authoredEnemyVoiceSources ?? Array.Empty<AudioSource>();
            chairTargets = authoredChairTargets ?? Array.Empty<PrototypeInteractionTarget>();
            chairRollSources = authoredChairRollSources ?? Array.Empty<AudioSource>();
            lightTargets = authoredLightTargets ?? Array.Empty<PrototypeInteractionTarget>();
            smallSpinSources = authoredSmallSpinSources ?? Array.Empty<AudioSource>();
            BuildRuntime();
            Subscribe();
        }

        public static int SelectNonRepeatingIndex(
            int count,
            int previousIndex,
            ref uint randomState)
        {
            if (count <= 0)
            {
                return -1;
            }
            if (count == 1)
            {
                randomState = NextRandom(randomState);
                return 0;
            }

            randomState = NextRandom(randomState);
            if (previousIndex < 0 || previousIndex >= count)
            {
                return (int)(randomState % (uint)count);
            }
            int candidate = (int)(randomState % (uint)(count - 1));
            return candidate >= previousIndex ? candidate + 1 : candidate;
        }

        public bool IsChairRollPlaying(PrototypeInteractionTarget target)
        {
            return TryGetTargetRuntime(target, out TargetRuntime state) &&
                state.RollPlaying;
        }

        public bool IsSmallSpinPlaying(PrototypeInteractionTarget target)
        {
            return TryGetTargetRuntime(target, out TargetRuntime state) &&
                state.SpinPlaying;
        }

        public int GetActorLastFootstepIndex(Transform actor)
        {
            return actor != null && actorRuntime.TryGetValue(
                actor.GetInstanceID(), out ActorRuntime state)
                    ? state.LastFootstepIndex
                    : -1;
        }

        private void Awake()
        {
            BuildRuntime();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopAllAudio();
        }

        private void Update()
        {
            ReleaseFinishedTransientVoices();
            UpdateChairMotion(Time.unscaledDeltaTime);
            UpdateSmallSpin(Time.unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            ProcessPendingImpacts();
        }

        private void BuildRuntime()
        {
            StopAllAudio();
            targetRuntime.Clear();
            actorRuntime.Clear();
            pendingImpacts.Clear();

            ShowcaseRunnerAgent[] runtimeRunners = ResolveRuntimeRunners();

            transientVoices = new TransientVoice[transientSources?.Length ?? 0];
            for (int index = 0; index < transientVoices.Length; index++)
            {
                AudioSource source = transientSources[index];
                if (source != null)
                {
                    source.playOnAwake = false;
                    source.loop = false;
                }
                transientVoices[index] = new TransientVoice { Source = source };
            }

            for (int index = 0; index < targets.Length; index++)
            {
                PrototypeInteractionTarget target = targets[index];
                if (target == null)
                {
                    continue;
                }

                var state = new TargetRuntime
                {
                    Target = target,
                    Body = target.GetComponent<Rigidbody>(),
                    Collider = target.GetComponent<Collider>(),
                    IsChair = IsChair(target),
                    IsLight = target.WeightClass == PrototypeWeightClass.Light
                };
                int chairIndex = Array.IndexOf(chairTargets, target);
                if (chairIndex >= 0 && chairIndex < chairRollSources.Length)
                {
                    state.LoopSource = chairRollSources[chairIndex];
                    ConfigureLoopSource(state.LoopSource, profile?.ChairRollLoopClip);
                }
                int lightIndex = Array.IndexOf(lightTargets, target);
                if (lightIndex >= 0 && lightIndex < smallSpinSources.Length)
                {
                    AudioSource spin = smallSpinSources[lightIndex];
                    ConfigureLoopSource(spin, profile?.SmallObjectSpinLoopClip);
                    if (!state.IsChair)
                    {
                        state.LoopSource = spin;
                    }
                }
                targetRuntime[target.GetInstanceID()] = state;
            }

            ShowcaseAnimationEventReceiver playerReceiver = FindReceiver(playerCombat);
            AddActorRuntime(
                playerCombat != null ? playerCombat.transform : null,
                playerHealth,
                playerReceiver,
                locomotion,
                null,
                playerFootstepSource,
                playerVoiceSource,
                0xC001D00Du);

            for (int index = 0; index < runtimeRunners.Length; index++)
            {
                ShowcaseRunnerAgent runner = runtimeRunners[index];
                if (runner == null)
                {
                    continue;
                }
                AudioSource ownedFootstep = ResolveOwnedSource(
                    runner.transform,
                    EnemyFootstepSourceName);
                AudioSource ownedVoice = ResolveOwnedSource(
                    runner.transform,
                    EnemyVoiceSourceName);
                AddActorRuntime(
                    runner.transform,
                    runner.Health,
                    FindReceiver(runner),
                    null,
                    runner,
                    ownedFootstep != null
                        ? ownedFootstep
                        : index < enemyFootstepSources.Length
                        ? enemyFootstepSources[index]
                        : null,
                    ownedVoice != null
                        ? ownedVoice
                        : index < enemyVoiceSources.Length
                        ? enemyVoiceSources[index]
                        : null,
                    0xE0010001u + (uint)index * 0x9E3779B9u);
            }

            ConfigureSequenceSource(strapPart1Source, profile?.StrapExtendPart1Clip);
            ConfigureSequenceSource(strapPart2Source, profile?.StrapExtendPart2Clip);
        }

        private void AddActorRuntime(
            Transform actor,
            ShowcaseCombatHealth health,
            ShowcaseAnimationEventReceiver receiver,
            ThirdPersonLocomotion actorLocomotion,
            ShowcaseRunnerAgent runner,
            AudioSource footstepSource,
            AudioSource voiceSource,
            uint seed)
        {
            if (actor == null)
            {
                return;
            }
            ConfigureOneShotSource(footstepSource);
            ConfigureOneShotSource(voiceSource);
            actorRuntime[actor.GetInstanceID()] = new ActorRuntime
            {
                Actor = actor,
                Health = health,
                Receiver = receiver,
                Locomotion = actorLocomotion,
                Runner = runner,
                FootstepSource = footstepSource,
                VoiceSource = voiceSource,
                RandomState = seed == 0 ? 1u : seed
            };
        }

        private ShowcaseAnimationEventReceiver FindReceiver(Component owner)
        {
            if (owner == null)
            {
                return null;
            }
            foreach (ShowcaseAnimationEventReceiver receiver in receivers)
            {
                if (receiver != null && receiver.transform.IsChildOf(owner.transform))
                {
                    return receiver;
                }
            }
            return owner.GetComponentInChildren<
                ShowcaseAnimationEventReceiver>(true);
        }

        private ShowcaseRunnerAgent[] ResolveRuntimeRunners()
        {
            ShowcaseRunnerAgent[] liveRunners = pressureController?.Runners;
            return liveRunners != null && liveRunners.Length > 0
                ? liveRunners
                : runners ?? Array.Empty<ShowcaseRunnerAgent>();
        }

        private static AudioSource ResolveOwnedSource(
            Transform owner,
            string sourceName)
        {
            Transform source = owner != null
                ? owner.Find(sourceName)
                : null;
            return source != null
                ? source.GetComponent<AudioSource>()
                : null;
        }

        private IEnumerable<ShowcaseAnimationEventReceiver>
            EnumerateRuntimeReceivers()
        {
            var seen = new HashSet<int>();
            if (receivers != null)
            {
                for (int index = 0; index < receivers.Length; index++)
                {
                    ShowcaseAnimationEventReceiver receiver = receivers[index];
                    if (receiver != null && seen.Add(receiver.GetInstanceID()))
                    {
                        yield return receiver;
                    }
                }
            }

            foreach (ActorRuntime actor in actorRuntime.Values)
            {
                ShowcaseAnimationEventReceiver receiver = actor.Receiver;
                if (receiver != null && seen.Add(receiver.GetInstanceID()))
                {
                    yield return receiver;
                }
            }
        }

        private void Subscribe()
        {
            if (playerCombat != null)
            {
                playerCombat.AcquisitionCommitted -= HandleAcquisitionCommitted;
                playerCombat.ChairSwingWhooshCommitted -= HandleChairSwingWhoosh;
                playerCombat.QueuedRelayCatchCommitted -=
                    HandleQueuedRelayCatchCommitted;
                playerCombat.QueuedRelayCatchCancelled -=
                    HandleQueuedRelayCatchCancelled;
                playerCombat.AcquisitionCommitted += HandleAcquisitionCommitted;
                playerCombat.ChairSwingWhooshCommitted += HandleChairSwingWhoosh;
                playerCombat.QueuedRelayCatchCommitted +=
                    HandleQueuedRelayCatchCommitted;
                playerCombat.QueuedRelayCatchCancelled +=
                    HandleQueuedRelayCatchCancelled;
            }
            if (physicalLifecycle != null)
            {
                physicalLifecycle.CarriedTargetDropped -= HandleCarriedTargetDropped;
                physicalLifecycle.PlayerThrowCommitted -= HandlePlayerThrowCommitted;
                physicalLifecycle.CarriedTargetDropped += HandleCarriedTargetDropped;
                physicalLifecycle.PlayerThrowCommitted += HandlePlayerThrowCommitted;
            }
            if (allyRelay != null)
            {
                allyRelay.SupplyReleased -= HandleSupplyReleased;
                allyRelay.CatchCompleted -= HandleRelayCatchCompleted;
                allyRelay.SupplyReleased += HandleSupplyReleased;
                allyRelay.CatchCompleted += HandleRelayCatchCompleted;
            }
            if (cargoStrap != null)
            {
                cargoStrap.StrapExtendStarted -= HandleStrapExtendStarted;
                cargoStrap.StrapLockCommitted -= HandleStrapLockCommitted;
                cargoStrap.StrapTensionCommitted -= HandleStrapTensionCommitted;
                cargoStrap.StrapExtendStarted += HandleStrapExtendStarted;
                cargoStrap.StrapLockCommitted += HandleStrapLockCommitted;
                cargoStrap.StrapTensionCommitted += HandleStrapTensionCommitted;
            }
            if (pressureController != null)
            {
                pressureController.CombatContactResolved -= HandleCombatContact;
                pressureController.BaselineReset -= HandleBaselineReset;
                pressureController.CombatContactResolved += HandleCombatContact;
                pressureController.BaselineReset += HandleBaselineReset;
            }
            foreach (PrototypeInteractionTarget target in targets)
            {
                if (target == null) continue;
                target.CollisionEntered -= HandleCollisionEntered;
                target.CollisionEntered += HandleCollisionEntered;
            }
            foreach (ShowcaseAnimationEventReceiver receiver in
                     EnumerateRuntimeReceivers())
            {
                if (receiver == null) continue;
                receiver.FootstepContact -= HandleFootstepContact;
                receiver.FootstepContact += HandleFootstepContact;
            }
            SubscribeDamageHandlers();
        }

        private void Unsubscribe()
        {
            if (playerCombat != null)
            {
                playerCombat.AcquisitionCommitted -= HandleAcquisitionCommitted;
                playerCombat.ChairSwingWhooshCommitted -= HandleChairSwingWhoosh;
                playerCombat.QueuedRelayCatchCommitted -=
                    HandleQueuedRelayCatchCommitted;
                playerCombat.QueuedRelayCatchCancelled -=
                    HandleQueuedRelayCatchCancelled;
            }
            if (physicalLifecycle != null)
            {
                physicalLifecycle.CarriedTargetDropped -= HandleCarriedTargetDropped;
                physicalLifecycle.PlayerThrowCommitted -= HandlePlayerThrowCommitted;
            }
            if (allyRelay != null)
            {
                allyRelay.SupplyReleased -= HandleSupplyReleased;
                allyRelay.CatchCompleted -= HandleRelayCatchCompleted;
            }
            if (cargoStrap != null)
            {
                cargoStrap.StrapExtendStarted -= HandleStrapExtendStarted;
                cargoStrap.StrapLockCommitted -= HandleStrapLockCommitted;
                cargoStrap.StrapTensionCommitted -= HandleStrapTensionCommitted;
            }
            if (pressureController != null)
            {
                pressureController.CombatContactResolved -= HandleCombatContact;
                pressureController.BaselineReset -= HandleBaselineReset;
            }
            foreach (PrototypeInteractionTarget target in targets)
            {
                if (target != null) target.CollisionEntered -= HandleCollisionEntered;
            }
            foreach (ShowcaseAnimationEventReceiver receiver in
                     EnumerateRuntimeReceivers())
            {
                if (receiver != null) receiver.FootstepContact -= HandleFootstepContact;
            }
            foreach (KeyValuePair<ShowcaseCombatHealth, Action<ShowcaseDamageResult>> pair
                     in damageHandlers)
            {
                if (pair.Key != null) pair.Key.DamageAccepted -= pair.Value;
            }
            damageHandlers.Clear();
        }

        private void SubscribeDamageHandlers()
        {
            foreach (ActorRuntime actor in actorRuntime.Values)
            {
                if (actor.Health == null || damageHandlers.ContainsKey(actor.Health))
                {
                    continue;
                }
                ActorRuntime captured = actor;
                Action<ShowcaseDamageResult> handler = result =>
                    HandleDamageAccepted(captured, result);
                damageHandlers.Add(actor.Health, handler);
                actor.Health.DamageAccepted += handler;
            }
        }

        private void HandleAcquisitionCommitted(
            PrototypeInteractionTarget target,
            PrototypeInteractionOutcome outcome)
        {
            if (target == null) return;
            ResetTargetMotionCycle(target);
            AudioClip clip;
            if (outcome == PrototypeInteractionOutcome.Catch)
            {
                if (lastAcquisitionCatchTargetId == target.GetInstanceID() &&
                    lastAcquisitionCatchFrame == Time.frameCount)
                {
                    DuplicateSuppressionCount++;
                    return;
                }
                lastAcquisitionCatchTargetId = target.GetInstanceID();
                lastAcquisitionCatchFrame = Time.frameCount;
                clip = target.WeightClass == PrototypeWeightClass.Light
                    ? profile?.BinderCatchClip
                    : profile?.ChairPickupClip;
            }
            else
            {
                clip = target.WeightClass == PrototypeWeightClass.Light
                    ? profile?.BinderPickupClip
                    : profile?.ChairPickupClip;
            }
            PlayTransient(clip, target.transform.position, foleyVolume, 96);
        }

        private void HandleRelayCatchCompleted(PrototypeInteractionTarget target)
        {
            if (target == null)
            {
                return;
            }
            if (lastAcquisitionCatchTargetId == target.GetInstanceID() &&
                lastAcquisitionCatchFrame == Time.frameCount)
            {
                DuplicateSuppressionCount++;
                return;
            }
            if (queuedCatchPendingSettlement.Remove(target.GetInstanceID()))
            {
                DuplicateSuppressionCount++;
                return;
            }
            ResetTargetMotionCycle(target);
            lastAcquisitionCatchTargetId = target.GetInstanceID();
            lastAcquisitionCatchFrame = Time.frameCount;
            AudioClip clip = target.WeightClass == PrototypeWeightClass.Light
                ? profile?.BinderCatchClip
                : profile?.ChairPickupClip;
            PlayTransient(clip, target.transform.position, foleyVolume, 96);
        }

        private void HandleQueuedRelayCatchCommitted(
            PrototypeInteractionTarget target)
        {
            if (target == null)
            {
                return;
            }
            ResetTargetMotionCycle(target);
            queuedCatchPendingSettlement.Add(target.GetInstanceID());
            lastAcquisitionCatchTargetId = target.GetInstanceID();
            lastAcquisitionCatchFrame = Time.frameCount;
            AudioClip clip = target.WeightClass == PrototypeWeightClass.Light
                ? profile?.BinderCatchClip
                : profile?.ChairPickupClip;
            PlayTransient(clip, target.transform.position, foleyVolume, 72);
        }

        private void HandleQueuedRelayCatchCancelled(
            PrototypeInteractionTarget target)
        {
            if (target != null)
            {
                queuedCatchPendingSettlement.Remove(target.GetInstanceID());
            }
        }

        private void HandleCarriedTargetDropped(PrototypeInteractionTarget target)
        {
            if (target == null) return;
            if (TryGetTargetRuntime(target, out TargetRuntime state))
            {
                state.BinderFloorArmed = false;
                state.ReleaseAuthorityUntil = float.NegativeInfinity;
            }
            AudioClip clip = target.WeightClass == PrototypeWeightClass.Light
                ? profile?.BinderPlaceDownClip
                : profile?.ChairReleaseClip;
            PlayTransient(clip, target.transform.position, foleyVolume, 96);
        }

        private void HandlePlayerThrowCommitted(PhysicalReleaseSnapshot snapshot)
        {
            PrototypeInteractionTarget target = snapshot.Target;
            if (target == null) return;
            ResetTargetMotionCycle(target);
            if (TryGetTargetRuntime(target, out TargetRuntime state))
            {
                state.BinderFloorArmed = target.WeightClass ==
                    PrototypeWeightClass.Light;
                state.ReleaseAuthorityUntil = Time.unscaledTime + 3f;
            }
            AudioClip clip = target.WeightClass == PrototypeWeightClass.Light
                ? profile?.BinderThrowClip
                : profile?.ChairThrowClip;
            PlayTransient(clip, snapshot.Position, foleyVolume, 80);
        }

        private void HandleSupplyReleased(PrototypeInteractionTarget target)
        {
            if (target == null) return;
            ResetTargetMotionCycle(target);
            if (TryGetTargetRuntime(target, out TargetRuntime state))
            {
                state.BinderFloorArmed = target.WeightClass ==
                    PrototypeWeightClass.Light;
                state.ReleaseAuthorityUntil = Time.unscaledTime + 3f;
            }
            AudioClip clip = target.WeightClass == PrototypeWeightClass.Light
                ? profile?.BinderThrowClip
                : profile?.ChairReleaseClip;
            PlayTransient(clip, target.transform.position, foleyVolume, 96);
        }

        private void HandleChairSwingWhoosh(PrototypeInteractionTarget target)
        {
            if (target == null || !IsChair(target)) return;
            PlayTransient(
                profile?.ChairSwingWhooshClip,
                target.transform.position,
                motionOneShotVolume,
                72);
        }

        private void HandleStrapExtendStarted(PrototypeInteractionTarget target)
        {
            ScheduleStrapExtend(target != null ? target.transform.position : transform.position);
        }

        private void HandleStrapLockCommitted(PrototypeInteractionTarget target)
        {
            PlayTransient(
                profile?.StrapLockClip,
                target != null ? target.transform.position : transform.position,
                foleyVolume,
                72);
        }

        private void HandleStrapTensionCommitted(PrototypeInteractionTarget target)
        {
            PlayTransient(
                profile?.StrapTensionClip,
                target != null ? target.transform.position : transform.position,
                foleyVolume,
                80);
        }

        private void ScheduleStrapExtend(Vector3 position)
        {
            if (profile?.StrapExtendPart1Clip == null ||
                profile.StrapExtendPart2Clip == null ||
                strapPart1Source == null || strapPart2Source == null)
            {
                return;
            }
            strapPart1Source.Stop();
            strapPart2Source.Stop();
            strapPart1Source.clip = profile.StrapExtendPart1Clip;
            strapPart2Source.clip = profile.StrapExtendPart2Clip;
            strapPart1Source.transform.position = position;
            strapPart2Source.transform.position = position;
            double start = AudioSettings.dspTime + DspScheduleLeadSeconds;
            double part1Duration = profile.StrapExtendPart1Clip.samples /
                (double)Mathf.Max(1, profile.StrapExtendPart1Clip.frequency);
            strapPart1Source.PlayScheduled(start);
            strapPart2Source.PlayScheduled(start + part1Duration);
            StrapSequencePlaybackCount++;
            LastPlayedClip = profile.StrapExtendPart2Clip;
        }

        private void HandleCombatContact(ShowcaseCombatContactResult result)
        {
            if (!result.Accepted || result.Attacker == null)
            {
                return;
            }
            PrototypeInteractionTarget target =
                result.Attacker.GetComponent<PrototypeInteractionTarget>() ??
                result.Attacker.GetComponentInParent<PrototypeInteractionTarget>();
            if (!TryGetTargetRuntime(target, out TargetRuntime state))
            {
                return;
            }
            if (!state.IsChair && !state.IsLight)
            {
                return;
            }
            state.LastAcceptedEnemyImpactFrame = Time.frameCount;
            pendingImpacts.Remove(target.GetInstanceID());
            FinalGameplayAudioImpactFamily family = state.IsChair
                ? FinalGameplayAudioImpactFamily.ChairEnemy
                : FinalGameplayAudioImpactFamily.BinderEnemy;
            AudioClip clip = state.IsChair
                ? profile?.ChairEnemyImpactClip
                : profile?.BinderDirectHitClip;
            if (PlayImpact(state, family, clip, result.WorldPoint))
            {
                state.LastAcceptedEnemyImpactFrame = Time.frameCount;
            }
        }

        private void HandleCollisionEntered(
            PrototypeInteractionCollisionSnapshot snapshot)
        {
            if (!TryGetTargetRuntime(snapshot.Target, out TargetRuntime state) ||
                (!state.IsChair && !state.IsLight) ||
                snapshot.RelativeSpeed < minimumImpactSpeed ||
                snapshot.OtherCollider == null)
            {
                return;
            }
            if (snapshot.OtherCollider.GetComponentInParent<ShowcaseRunnerAgent>() != null)
            {
                // Enemy audio is owned by the accepted combat transaction.
                return;
            }
            PrototypeInteractionTarget otherTarget =
                snapshot.OtherCollider.GetComponentInParent<PrototypeInteractionTarget>();
            if (otherTarget != null && otherTarget != snapshot.Target)
            {
                bool ownCargoAuthority = cargoStrap != null &&
                    cargoStrap.ActiveTarget == snapshot.Target;
                bool otherCargoAuthority = cargoStrap != null &&
                    cargoStrap.ActiveTarget == otherTarget;
                TryGetTargetRuntime(otherTarget, out TargetRuntime otherState);
                bool ownReleaseAuthority = state.ReleaseAuthorityUntil >=
                    Time.unscaledTime;
                bool otherReleaseAuthority = otherState != null &&
                    otherState.ReleaseAuthorityUntil >= Time.unscaledTime;
                if ((otherCargoAuthority && !ownCargoAuthority) ||
                    (otherReleaseAuthority && !ownReleaseAuthority))
                {
                    DuplicateSuppressionCount++;
                    return;
                }
                Rigidbody otherBody = otherTarget.GetComponent<Rigidbody>();
                float ownSpeed = state.Body != null
                    ? state.Body.linearVelocity.sqrMagnitude
                    : 0f;
                float otherSpeed = otherBody != null
                    ? otherBody.linearVelocity.sqrMagnitude
                    : 0f;
                if ((!ownCargoAuthority && !ownReleaseAuthority &&
                     ownSpeed + 0.01f < otherSpeed) ||
                    (Mathf.Abs(ownSpeed - otherSpeed) <= 0.01f &&
                     !ownCargoAuthority && !ownReleaseAuthority &&
                     snapshot.Target.GetInstanceID() > otherTarget.GetInstanceID()))
                {
                    DuplicateSuppressionCount++;
                    return;
                }
            }
            if (physicalLifecycle != null &&
                physicalLifecycle.CarriedTarget == snapshot.Target)
            {
                return;
            }

            FinalGameplayAudioImpactFamily family = state.IsChair
                ? FinalGameplayAudioImpactFamily.ChairWorld
                : snapshot.Normal.y >= floorNormalThreshold
                    ? FinalGameplayAudioImpactFamily.BinderFloor
                    : FinalGameplayAudioImpactFamily.BinderWall;
            int id = snapshot.Target.GetInstanceID();
            if (pendingImpacts.TryGetValue(id, out PendingImpact pending) &&
                pending.Snapshot.Frame == snapshot.Frame)
            {
                DuplicateSuppressionCount++;
                if (ImpactPriority(pending.Family) >= ImpactPriority(family))
                {
                    return;
                }
            }
            pendingImpacts[id] = new PendingImpact(snapshot, family);
        }

        private void ProcessPendingImpacts()
        {
            if (pendingImpacts.Count == 0)
            {
                return;
            }
            var pending = new List<PendingImpact>(pendingImpacts.Values);
            pendingImpacts.Clear();
            foreach (PendingImpact item in pending)
            {
                if (!TryGetTargetRuntime(item.Snapshot.Target, out TargetRuntime state))
                {
                    continue;
                }
                if (state.LastAcceptedEnemyImpactFrame == item.Snapshot.Frame)
                {
                    DuplicateSuppressionCount++;
                    continue;
                }
                if (item.Family == FinalGameplayAudioImpactFamily.BinderFloor &&
                    (!state.BinderFloorArmed || state.BinderFloorConsumed))
                {
                    DuplicateSuppressionCount++;
                    continue;
                }
                AudioClip clip = item.Family switch
                {
                    FinalGameplayAudioImpactFamily.BinderFloor =>
                        profile?.BinderFloorDropClip,
                    FinalGameplayAudioImpactFamily.BinderWall =>
                        profile?.BinderWallHitClip,
                    FinalGameplayAudioImpactFamily.ChairWorld =>
                        profile?.ChairWorldImpactClip,
                    _ => null
                };
                if (PlayImpact(state, item.Family, clip, item.Snapshot.Point) &&
                    item.Family == FinalGameplayAudioImpactFamily.BinderFloor)
                {
                    state.BinderFloorConsumed = true;
                    state.BinderFloorArmed = false;
                }
            }
        }

        private bool PlayImpact(
            TargetRuntime state,
            FinalGameplayAudioImpactFamily family,
            AudioClip clip,
            Vector3 point)
        {
            float now = Time.unscaledTime;
            if (state == null || now - state.LastImpactAt < objectImpactCooldown)
            {
                DuplicateSuppressionCount++;
                return false;
            }
            if (!PlayTransient(clip, point, impactVolume, 48))
            {
                return false;
            }
            state.LastImpactAt = now;
            state.LastImpactFrame = Time.frameCount;
            state.StopCandidate = false;
            LastImpactFamily = family;
            return true;
        }

        private void HandleFootstepContact(
            ShowcaseAnimationEventReceiver receiver,
            int foot)
        {
            ActorRuntime actor = FindActorRuntime(receiver);
            if (actor == null || actor.FootstepSource == null ||
                profile?.FootstepPool == null || profile.FootstepPool.Length == 0 ||
                !IsActorGrounded(actor) || !IsActorMoving(actor))
            {
                return;
            }
            if (actor.LastFootstepFrame == Time.frameCount)
            {
                DuplicateSuppressionCount++;
                return;
            }
            if (actor.Runner != null && CountPlayingEnemyFootsteps() >=
                maximumEnemyFootstepVoices)
            {
                return;
            }
            int index = SelectNonRepeatingIndex(
                profile.FootstepPool.Length,
                actor.LastFootstepIndex,
                ref actor.RandomState);
            if (index < 0 || profile.FootstepPool[index] == null)
            {
                return;
            }
            actor.FootstepSource.transform.position = ResolveFootPosition(actor, foot);
            actor.FootstepSource.PlayOneShot(
                profile.FootstepPool[index],
                actor.Runner == null ? playerFootstepVolume : enemyFootstepVolume);
            actor.LastFootstepIndex = index;
            actor.LastFootstepFrame = Time.frameCount;
            actor.LastFoot = foot;
            FootstepPlaybackCount++;
            LastPlayedClip = profile.FootstepPool[index];
        }

        private void HandleDamageAccepted(
            ActorRuntime actor,
            ShowcaseDamageResult result)
        {
            if (actor == null || actor.VoiceSource == null || profile == null)
            {
                return;
            }
            bool lethal = result.IsLethal;
            bool playerActor = actor.Runner == null;
            bool heavy = !lethal && playerActor &&
                (result.Request.Reaction == ShowcaseHitReaction.Knockdown ||
                 result.Request.Amount >= heavyDamageThreshold);
            float now = Time.unscaledTime;
            bool voiceBusy = now < actor.VoiceCooldownUntil ||
                actor.VoiceSource.isPlaying;
            bool higherPriorityInterrupt = heavy &&
                actor.CurrentVoiceTier == FinalGameplayVoiceTier.Light;
            if (!lethal && voiceBusy && !higherPriorityInterrupt)
            {
                VoiceCooldownSuppressionCount++;
                return;
            }
            if (actor.Runner != null && !lethal &&
                CountPlayingEnemyVoices() >= maximumEnemyVoiceConcurrency)
            {
                VoiceCooldownSuppressionCount++;
                return;
            }
            float probability = actor.Runner == null
                ? heavy ? playerHeavyHurtProbability : playerLightHurtProbability
                : enemyLightHurtProbability;
            if (!lethal && Next01(ref actor.RandomState) > probability)
            {
                return;
            }

            AudioClip clip;
            if (lethal)
            {
                AudioClip[] pool = actor.Runner == null
                    ? profile.PlayerKoPool
                    : profile.EnemyKoPool;
                int index = SelectNonRepeatingIndex(
                    pool?.Length ?? 0,
                    actor.LastKoIndex,
                    ref actor.RandomState);
                clip = index >= 0 ? pool[index] : null;
                actor.LastKoIndex = index;
            }
            else if (heavy && actor.Runner == null)
            {
                clip = profile.PlayerHeavyHurtClip;
            }
            else
            {
                AudioClip[] pool = actor.Runner == null
                    ? profile.PlayerLightHurtPool
                    : profile.EnemyLightHurtPool;
                int index = SelectNonRepeatingIndex(
                    pool?.Length ?? 0,
                    actor.LastLightVoiceIndex,
                    ref actor.RandomState);
                clip = index >= 0 ? pool[index] : null;
                actor.LastLightVoiceIndex = index;
            }
            if (clip == null)
            {
                return;
            }
            actor.VoiceSource.Stop();
            actor.VoiceSource.clip = clip;
            actor.VoiceSource.pitch = 1f;
            actor.VoiceSource.volume = actor.Runner == null
                ? playerVoiceVolume
                : enemyVoiceVolume;
            actor.VoiceSource.Play();
            actor.CurrentVoiceTier = lethal
                ? FinalGameplayVoiceTier.Ko
                : heavy
                    ? FinalGameplayVoiceTier.Heavy
                    : FinalGameplayVoiceTier.Light;
            actor.VoiceCooldownUntil = now +
                (actor.Runner == null ? playerVoiceCooldown : enemyVoiceCooldown);
            VoicePlaybackCount++;
            LastPlayedClip = clip;
        }

        private void UpdateChairMotion(float deltaTime)
        {
            foreach (PrototypeInteractionTarget target in chairTargets)
            {
                if (!TryGetTargetRuntime(target, out TargetRuntime state) ||
                    state.Body == null || state.LoopSource == null)
                {
                    continue;
                }
                bool available = target.gameObject.activeInHierarchy &&
                    !state.Body.isKinematic;
                bool grounded = available && IsTargetGrounded(state);
                Vector3 planarVelocity = Vector3.ProjectOnPlane(
                    state.Body.linearVelocity,
                    Vector3.up);
                float speed = planarVelocity.magnitude;
                bool wantsRoll = grounded && speed >=
                    (state.RollPlaying ? chairRollStopSpeed : chairRollStartSpeed);
                float targetVolume = wantsRoll
                    ? Mathf.InverseLerp(
                        chairRollStartSpeed,
                        chairRollFullVolumeSpeed,
                        speed) * chairRollMaximumVolume
                    : 0f;
                UpdateLoop(
                    state,
                    state.LoopSource,
                    profile?.ChairRollLoopClip,
                    targetVolume,
                    deltaTime,
                    true);

                bool moving = speed >= chairRollStartSpeed;
                bool stopped = grounded && speed <= chairRollStopSpeed;
                if (moving)
                {
                    state.WasMoving = true;
                    state.StopCandidate = false;
                }
                else if (state.WasMoving && stopped && !state.StopCandidate)
                {
                    state.StopCandidate = true;
                    state.StopCandidateStartedAt = Time.unscaledTime;
                }
                if (state.StopCandidate &&
                    Time.unscaledTime - state.StopCandidateStartedAt >=
                        StopConfirmationSeconds &&
                    Time.unscaledTime - state.LastImpactAt >=
                        StopAfterImpactGuardSeconds)
                {
                    PlayChairStop(state);
                    state.StopCandidate = false;
                    state.WasMoving = false;
                }
            }
        }

        private void UpdateSmallSpin(float deltaTime)
        {
            foreach (PrototypeInteractionTarget target in lightTargets)
            {
                if (!TryGetTargetRuntime(target, out TargetRuntime state))
                {
                    continue;
                }
                int index = Array.IndexOf(lightTargets, target);
                AudioSource source = index >= 0 && index < smallSpinSources.Length
                    ? smallSpinSources[index]
                    : null;
                if (source == null || state.Body == null)
                {
                    continue;
                }
                bool phaseActive = cargoStrap != null &&
                    cargoStrap.IsSmallSpinning && cargoStrap.ActiveTarget == target &&
                    target.gameObject.activeInHierarchy;
                float effectiveAngularSpeed = 0f;
                if (phaseActive)
                {
                    effectiveAngularSpeed = Mathf.Max(
                        state.Body.angularVelocity.magnitude,
                        Vector3.ProjectOnPlane(state.Body.linearVelocity, Vector3.up)
                            .magnitude / Mathf.Max(0.1f, cargoStrap.SmallSpinRadius));
                }
                bool wantsSpin = phaseActive && effectiveAngularSpeed >=
                    (state.SpinPlaying
                        ? smallSpinStopAngularSpeed
                        : smallSpinStartAngularSpeed);
                float targetVolume = wantsSpin
                    ? Mathf.InverseLerp(
                        smallSpinStartAngularSpeed,
                        smallSpinFullVolumeAngularSpeed,
                        effectiveAngularSpeed) * smallSpinMaximumVolume
                    : 0f;
                UpdateLoop(
                    state,
                    source,
                    profile?.SmallObjectSpinLoopClip,
                    targetVolume,
                    deltaTime,
                    false);
            }
        }

        private void UpdateLoop(
            TargetRuntime state,
            AudioSource source,
            AudioClip clip,
            float targetVolume,
            float deltaTime,
            bool chair)
        {
            if (source == null || clip == null)
            {
                return;
            }
            source.transform.position = state.Target.transform.position;
            bool playing = chair ? state.RollPlaying : state.SpinPlaying;
            if (!playing && targetVolume > 0.001f)
            {
                source.clip = clip;
                source.loop = true;
                source.volume = 0f;
                source.Play();
                playing = true;
                LoopStartCount++;
            }
            source.volume = Mathf.MoveTowards(
                source.volume,
                targetVolume,
                deltaTime / MotionLoopFadeSeconds);
            if (playing && targetVolume <= 0.001f && source.volume <= 0.001f)
            {
                source.Stop();
                source.volume = 0f;
                playing = false;
                LoopStopCount++;
            }
            if (chair) state.RollPlaying = playing;
            else state.SpinPlaying = playing;
        }

        private void PlayChairStop(TargetRuntime state)
        {
            AudioClip[] pool = profile?.ChairStopPool;
            uint random = (uint)state.Target.GetInstanceID() * 2654435761u;
            int index = SelectNonRepeatingIndex(
                pool?.Length ?? 0,
                state.LastStopIndex,
                ref random);
            if (index < 0 || pool[index] == null)
            {
                return;
            }
            if (PlayTransient(
                    pool[index],
                    state.Target.transform.position,
                    impactVolume,
                    96))
            {
                state.LastStopIndex = index;
            }
        }

        private bool PlayTransient(
            AudioClip clip,
            Vector3 position,
            float volume,
            int priority)
        {
            if (clip == null)
            {
                return false;
            }
            double now = AudioSettings.dspTime;
            TransientVoice selected = null;
            foreach (TransientVoice voice in transientVoices)
            {
                if (voice?.Source != null && voice.BusyUntil <= now)
                {
                    selected = voice;
                    break;
                }
            }
            if (selected == null)
            {
                return false;
            }
            AudioSource source = selected.Source;
            source.Stop();
            source.clip = clip;
            source.transform.position = position;
            source.volume = Mathf.Clamp01(volume);
            source.pitch = 1f;
            source.priority = Mathf.Clamp(priority, 0, 256);
            source.Play();
            selected.BusyUntil = now + clip.samples /
                (double)Mathf.Max(1, clip.frequency) + 0.02;
            OneShotPlaybackCount++;
            LastPlayedClip = clip;
            return true;
        }

        private void ReleaseFinishedTransientVoices()
        {
            double now = AudioSettings.dspTime;
            foreach (TransientVoice voice in transientVoices)
            {
                if (voice?.Source == null || voice.BusyUntil <= 0d ||
                    voice.BusyUntil > now)
                {
                    continue;
                }
                voice.Source.Stop();
                voice.Source.clip = null;
                voice.BusyUntil = 0d;
            }
        }

        private bool IsActorGrounded(ActorRuntime actor)
        {
            if (actor.Locomotion != null)
            {
                return actor.Locomotion.IsGrounded;
            }
            Collider collider = actor.Actor.GetComponent<Collider>();
            return IsSupported(actor.Actor, collider);
        }

        private bool IsActorMoving(ActorRuntime actor)
        {
            if (actor.Locomotion != null)
            {
                CharacterController controller =
                    actor.Locomotion.GetComponent<CharacterController>();
                return controller != null &&
                    Vector3.ProjectOnPlane(controller.velocity, Vector3.up)
                        .sqrMagnitude > 0.01f;
            }
            if (actor.Runner != null)
            {
                return actor.Runner.CommandedPlanarSpeed > 0.1f;
            }
            Rigidbody body = actor.Actor.GetComponent<Rigidbody>();
            return body != null &&
                Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up)
                    .sqrMagnitude > 0.01f;
        }

        private bool IsTargetGrounded(TargetRuntime state)
        {
            return state != null && IsSupported(
                state.Target.transform,
                state.Collider);
        }

        private static bool IsSupported(Transform root, Collider collider)
        {
            if (root == null || collider == null)
            {
                return false;
            }
            Bounds bounds = collider.bounds;
            Vector3 origin = new(
                bounds.center.x,
                bounds.min.y + 0.03f,
                bounds.center.z);
            int count = Physics.RaycastNonAlloc(
                origin,
                Vector3.down,
                GroundHits,
                MinimumGroundProbeDistance,
                ~0,
                QueryTriggerInteraction.Ignore);
            for (int index = 0; index < count; index++)
            {
                Collider hit = GroundHits[index].collider;
                if (hit != null && hit.transform != root &&
                    !hit.transform.IsChildOf(root))
                {
                    return true;
                }
            }
            return false;
        }

        private Vector3 ResolveFootPosition(ActorRuntime actor, int foot)
        {
            Animator animator = actor.Actor.GetComponentInChildren<Animator>(true);
            if (animator != null && animator.isHuman)
            {
                Transform bone = animator.GetBoneTransform(
                    foot == 0 ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot);
                if (bone != null) return bone.position;
            }
            return actor.Actor.position;
        }

        private int CountPlayingEnemyFootsteps()
        {
            int count = 0;
            foreach (ActorRuntime actor in actorRuntime.Values)
            {
                if (actor.Runner != null && actor.FootstepSource != null &&
                    actor.FootstepSource.isPlaying)
                {
                    count++;
                }
            }
            return count;
        }

        private int CountPlayingEnemyVoices()
        {
            int count = 0;
            foreach (ActorRuntime actor in actorRuntime.Values)
            {
                if (actor.Runner != null && actor.VoiceSource != null &&
                    actor.VoiceSource.isPlaying)
                {
                    count++;
                }
            }
            return count;
        }

        private ActorRuntime FindActorRuntime(ShowcaseAnimationEventReceiver receiver)
        {
            if (receiver == null) return null;
            foreach (ActorRuntime actor in actorRuntime.Values)
            {
                if (actor.Receiver == receiver ||
                    receiver.transform.IsChildOf(actor.Actor))
                {
                    return actor;
                }
            }
            return null;
        }

        private bool TryGetTargetRuntime(
            PrototypeInteractionTarget target,
            out TargetRuntime state)
        {
            state = null;
            return target != null && targetRuntime.TryGetValue(
                target.GetInstanceID(), out state);
        }

        private void ResetTargetMotionCycle(PrototypeInteractionTarget target)
        {
            if (!TryGetTargetRuntime(target, out TargetRuntime state)) return;
            state.BinderFloorConsumed = false;
            state.BinderFloorArmed = false;
            state.ReleaseAuthorityUntil = float.NegativeInfinity;
            state.StopCandidate = false;
            state.WasMoving = false;
        }

        private static int ImpactPriority(FinalGameplayAudioImpactFamily family)
        {
            return family switch
            {
                FinalGameplayAudioImpactFamily.BinderEnemy => 3,
                FinalGameplayAudioImpactFamily.ChairEnemy => 3,
                FinalGameplayAudioImpactFamily.BinderWall => 2,
                FinalGameplayAudioImpactFamily.ChairWorld => 2,
                FinalGameplayAudioImpactFamily.BinderFloor => 1,
                _ => 0
            };
        }

        private static bool IsChair(PrototypeInteractionTarget target)
        {
            return target != null &&
                (target.Role == PrototypeInteractionRole.DirectChair ||
                 target.Role == PrototypeInteractionRole.AllyChair);
        }

        private static uint NextRandom(uint state)
        {
            if (state == 0) state = 0xA341316Cu;
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        private static float Next01(ref uint state)
        {
            state = NextRandom(state);
            return (state & 0x00FFFFFFu) / 16777216f;
        }

        private static void ConfigureOneShotSource(AudioSource source)
        {
            if (source == null) return;
            source.playOnAwake = false;
            source.loop = false;
            source.pitch = 1f;
        }

        private static void ConfigureLoopSource(AudioSource source, AudioClip clip)
        {
            if (source == null) return;
            source.Stop();
            source.playOnAwake = false;
            source.loop = true;
            source.clip = clip;
            source.volume = 0f;
            source.pitch = 1f;
        }

        private static void ConfigureSequenceSource(AudioSource source, AudioClip clip)
        {
            if (source == null) return;
            source.Stop();
            source.playOnAwake = false;
            source.loop = false;
            source.clip = clip;
            source.pitch = 1f;
        }

        private void HandleBaselineReset()
        {
            StopAllAudio();
            foreach (TargetRuntime state in targetRuntime.Values)
            {
                state.BinderFloorConsumed = false;
                state.BinderFloorArmed = false;
                state.WasMoving = false;
                state.StopCandidate = false;
                state.LastImpactAt = float.NegativeInfinity;
                state.LastImpactFrame = -1;
                state.LastAcceptedEnemyImpactFrame = -1;
                state.LastStopIndex = -1;
                state.ReleaseAuthorityUntil = float.NegativeInfinity;
            }
            foreach (ActorRuntime actor in actorRuntime.Values)
            {
                actor.LastFootstepIndex = -1;
                actor.LastLightVoiceIndex = -1;
                actor.LastKoIndex = -1;
                actor.LastFootstepFrame = -1;
                actor.LastFoot = -1;
                actor.VoiceCooldownUntil = float.NegativeInfinity;
                actor.CurrentVoiceTier = FinalGameplayVoiceTier.None;
            }
            lastAcquisitionCatchTargetId = 0;
            lastAcquisitionCatchFrame = -1;
            queuedCatchPendingSettlement.Clear();
            ResetDiagnostics();
        }

        private void ResetDiagnostics()
        {
            OneShotPlaybackCount = 0;
            FootstepPlaybackCount = 0;
            VoicePlaybackCount = 0;
            LoopStartCount = 0;
            LoopStopCount = 0;
            DuplicateSuppressionCount = 0;
            VoiceCooldownSuppressionCount = 0;
            StrapSequencePlaybackCount = 0;
            LastPlayedClip = null;
            LastImpactFamily = FinalGameplayAudioImpactFamily.None;
        }

        private void StopAllAudio()
        {
            pendingImpacts.Clear();
            foreach (TransientVoice voice in transientVoices)
            {
                if (voice?.Source == null) continue;
                voice.Source.Stop();
                voice.Source.clip = null;
                voice.BusyUntil = 0d;
            }
            StopSource(strapPart1Source, false);
            StopSource(strapPart2Source, false);
            StopSource(playerFootstepSource, false);
            StopSource(playerVoiceSource, false);
            foreach (AudioSource source in enemyFootstepSources) StopSource(source, false);
            foreach (AudioSource source in enemyVoiceSources) StopSource(source, false);
            foreach (AudioSource source in chairRollSources) StopSource(source, true);
            foreach (AudioSource source in smallSpinSources) StopSource(source, true);
            foreach (TargetRuntime state in targetRuntime.Values)
            {
                state.RollPlaying = false;
                state.SpinPlaying = false;
            }
        }

        private static void StopSource(AudioSource source, bool resetVolume)
        {
            if (source == null) return;
            source.Stop();
            if (resetVolume) source.volume = 0f;
        }
    }
}
