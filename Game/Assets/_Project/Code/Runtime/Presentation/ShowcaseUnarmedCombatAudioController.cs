using System;
using System.Collections.Generic;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    public enum ShowcaseUnarmedCombatAudioTake
    {
        None,
        P01DeepBody,
        P03Cross,
        P04AlternateCross,
        P08Jab,
        P09HeavyHook,
        P10FastBody
    }

    public readonly struct ShowcaseUnarmedCombatAudioPlan
    {
        public ShowcaseUnarmedCombatAudioPlan(
            ShowcaseUnarmedCombatAudioTake take,
            int leadSamples)
        {
            Take = take;
            LeadSamples = Mathf.Max(0, leadSamples);
        }

        public ShowcaseUnarmedCombatAudioTake Take { get; }
        public int LeadSamples { get; }
    }

    /// <summary>
    /// Exact 48 kHz impact-sample and routing authority for the six
    /// Human Owner-selected unarmed takes. Runtime playback begins only after
    /// an accepted physical combat contact, at the take's impact sample.
    /// </summary>
    public static class ShowcaseUnarmedCombatAudioTiming
    {
        public const int SampleRate = 48000;
        public const int P01LeadSamples = 5380;
        public const int P03LeadSamples = 10640;
        public const int P04LeadSamples = 15643;
        public const int P08LeadSamples = 8643;
        public const int P09LeadSamples = 25948;
        public const int P10LeadSamples = 3070;

        public static bool TryResolvePlayer(
            ShowcasePlayerAnimationCue cue,
            out ShowcaseUnarmedCombatAudioPlan plan)
        {
            switch (cue)
            {
                case ShowcasePlayerAnimationCue.MeleeJab:
                    plan = new ShowcaseUnarmedCombatAudioPlan(
                        ShowcaseUnarmedCombatAudioTake.P08Jab,
                        P08LeadSamples);
                    return true;
                case ShowcasePlayerAnimationCue.MeleeCross:
                    plan = new ShowcaseUnarmedCombatAudioPlan(
                        ShowcaseUnarmedCombatAudioTake.P03Cross,
                        P03LeadSamples);
                    return true;
                case ShowcasePlayerAnimationCue.MeleeHook:
                    plan = new ShowcaseUnarmedCombatAudioPlan(
                        ShowcaseUnarmedCombatAudioTake.P09HeavyHook,
                        P09LeadSamples);
                    return true;
                default:
                    plan = default;
                    return false;
            }
        }

        public static bool TryResolveRunner(
            ShowcaseRunnerAnimationCue cue,
            ShowcaseUnarmedCombatAudioTake faceTake,
            out ShowcaseUnarmedCombatAudioPlan plan)
        {
            switch (cue)
            {
                case ShowcaseRunnerAnimationCue.Boxing:
                    if (faceTake != ShowcaseUnarmedCombatAudioTake.P04AlternateCross)
                    {
                        faceTake = ShowcaseUnarmedCombatAudioTake.P03Cross;
                    }
                    plan = new ShowcaseUnarmedCombatAudioPlan(
                        faceTake,
                        faceTake == ShowcaseUnarmedCombatAudioTake.P04AlternateCross
                            ? P04LeadSamples
                            : P03LeadSamples);
                    return true;
                case ShowcaseRunnerAnimationCue.Boxing2:
                case ShowcaseRunnerAnimationCue.Kicking:
                case ShowcaseRunnerAnimationCue.EliteMutantSwiping:
                    plan = new ShowcaseUnarmedCombatAudioPlan(
                        ShowcaseUnarmedCombatAudioTake.P01DeepBody,
                        P01LeadSamples);
                    return true;
                case ShowcaseRunnerAnimationCue.Kicking2:
                case ShowcaseRunnerAnimationCue.EliteMutantPunch:
                    plan = new ShowcaseUnarmedCombatAudioPlan(
                        ShowcaseUnarmedCombatAudioTake.P10FastBody,
                        P10LeadSamples);
                    return true;
                default:
                    plan = default;
                    return false;
            }
        }
    }

    [DefaultExecutionOrder(130)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseUnarmedCombatAudioController : MonoBehaviour
    {
        public const float ConfirmedImpactVolume = 0.4f;

        private const double VoiceReleasePaddingSeconds = 0.05;

        private readonly struct PlaybackKey : IEquatable<PlaybackKey>
        {
            public PlaybackKey(int attackerInstanceId, uint attackSequence)
            {
                AttackerInstanceId = attackerInstanceId;
                AttackSequence = attackSequence;
            }

            private int AttackerInstanceId { get; }
            private uint AttackSequence { get; }

            public bool Equals(PlaybackKey other)
            {
                return AttackerInstanceId == other.AttackerInstanceId &&
                    AttackSequence == other.AttackSequence;
            }

            public override bool Equals(object obj)
            {
                return obj is PlaybackKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(AttackerInstanceId, AttackSequence);
            }
        }

        private sealed class Voice
        {
            public AudioSource Source;
            public double BusyUntilDsp;
        }

        [SerializeField]
        private ShowcasePlayerCombatController playerCombat;

        [SerializeField]
        private ShowcaseRunnerPressureController pressureController;

        [SerializeField]
        private ShowcaseRunnerAgent[] runners = Array.Empty<ShowcaseRunnerAgent>();

        [SerializeField]
        private AudioClip p01DeepBody;

        [SerializeField]
        private AudioClip p03Cross;

        [SerializeField]
        private AudioClip p04AlternateCross;

        [SerializeField]
        private AudioClip p08Jab;

        [SerializeField]
        private AudioClip p09HeavyHook;

        [SerializeField]
        private AudioClip p10FastBody;

        [SerializeField]
        private AudioSource[] authoredVoices = Array.Empty<AudioSource>();

        private readonly Dictionary<int, ShowcaseUnarmedCombatAudioTake>
            lastRunnerFaceTake = new();
        private readonly HashSet<PlaybackKey> playedAttacks = new();
        private readonly int[] confirmedTakeCounts = new int[7];
        private Voice[] voices = Array.Empty<Voice>();

        public ShowcasePlayerCombatController PlayerCombat => playerCombat;
        public ShowcaseRunnerPressureController PressureController => pressureController;
        public ShowcaseRunnerAgent[] Runners =>
            pressureController != null && pressureController.Runners != null &&
            pressureController.Runners.Length > 0
                ? pressureController.Runners
                : runners;
        public AudioSource[] AuthoredVoices => authoredVoices;
        public AudioClip P01DeepBody => p01DeepBody;
        public AudioClip P03Cross => p03Cross;
        public AudioClip P04AlternateCross => p04AlternateCross;
        public AudioClip P08Jab => p08Jab;
        public AudioClip P09HeavyHook => p09HeavyHook;
        public AudioClip P10FastBody => p10FastBody;
        public int AcceptedContactCount { get; private set; }
        public int ConfirmedImpactPlaybackCount { get; private set; }
        public int DuplicateContactSuppressionCount { get; private set; }
        public int VoiceExhaustionCount { get; private set; }
        public ShowcaseUnarmedCombatAudioTake LastConfirmedTake { get; private set; }
        public Transform LastConfirmedAttacker { get; private set; }
        public uint LastConfirmedAttackSequence { get; private set; }
        public Vector3 LastConfirmedWorldPoint { get; private set; }
        public int LastConfirmedStartSample { get; private set; }
        public float LastConfirmedVolume { get; private set; }

        public void Configure(
            ShowcasePlayerCombatController player,
            ShowcaseRunnerPressureController pressure,
            ShowcaseRunnerAgent[] authoredRunners,
            AudioClip p01,
            AudioClip p03,
            AudioClip p04,
            AudioClip p08,
            AudioClip p09,
            AudioClip p10,
            AudioSource[] voiceSources)
        {
            Unsubscribe();
            playerCombat = player;
            pressureController = pressure;
            runners = authoredRunners ?? Array.Empty<ShowcaseRunnerAgent>();
            p01DeepBody = p01;
            p03Cross = p03;
            p04AlternateCross = p04;
            p08Jab = p08;
            p09HeavyHook = p09;
            p10FastBody = p10;
            authoredVoices = voiceSources ?? Array.Empty<AudioSource>();
            BuildVoiceRuntime();
            Subscribe();
        }

        public int GetConfirmedTakeCount(ShowcaseUnarmedCombatAudioTake take)
        {
            int index = (int)take;
            return index >= 0 && index < confirmedTakeCounts.Length
                ? confirmedTakeCounts[index]
                : 0;
        }

        private void Awake()
        {
            BuildVoiceRuntime();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopAllVoices();
        }

        private void Update()
        {
            ReleaseFinishedVoices();
        }

        private void HandleCombatContact(ShowcaseCombatContactResult result)
        {
            if (!result.Accepted || result.Attacker == null ||
                !TryResolveAcceptedContact(
                    result.Attacker,
                    out ShowcaseUnarmedCombatAudioPlan plan,
                    out uint attackSequence,
                    out ShowcaseRunnerAgent runner))
            {
                return;
            }

            AcceptedContactCount++;
            var key = new PlaybackKey(
                result.Attacker.GetInstanceID(),
                attackSequence);
            if (playedAttacks.Contains(key))
            {
                DuplicateContactSuppressionCount++;
                return;
            }

            if (!PlayConfirmedImpact(
                    result.Attacker,
                    attackSequence,
                    result.WorldPoint,
                    plan))
            {
                return;
            }

            playedAttacks.Add(key);
            if (runner != null &&
                (plan.Take == ShowcaseUnarmedCombatAudioTake.P03Cross ||
                 plan.Take == ShowcaseUnarmedCombatAudioTake.P04AlternateCross))
            {
                lastRunnerFaceTake[runner.GetInstanceID()] = plan.Take;
            }
        }

        private bool TryResolveAcceptedContact(
            Transform attacker,
            out ShowcaseUnarmedCombatAudioPlan plan,
            out uint attackSequence,
            out ShowcaseRunnerAgent runner)
        {
            plan = default;
            attackSequence = 0;
            runner = null;
            if (playerCombat != null && attacker == playerCombat.transform)
            {
                attackSequence = playerCombat.ActiveAttackSequence;
                return attackSequence != 0 &&
                    ShowcaseUnarmedCombatAudioTiming.TryResolvePlayer(
                        playerCombat.AnimationCue,
                        out plan);
            }

            ShowcaseRunnerAgent[] runtimeRunners = Runners;
            if (runtimeRunners != null)
            {
                foreach (ShowcaseRunnerAgent candidate in runtimeRunners)
                {
                    if (candidate == null || attacker != candidate.transform)
                    {
                        continue;
                    }

                    runner = candidate;
                    attackSequence = candidate.ActiveAttackSequence;
                    ShowcaseUnarmedCombatAudioTake faceTake =
                        ResolveNextRunnerFaceTake(candidate);
                    return attackSequence != 0 &&
                        ShowcaseUnarmedCombatAudioTiming.TryResolveRunner(
                            candidate.AnimationCue,
                            faceTake,
                            out plan);
                }
            }

            return false;
        }

        private bool PlayConfirmedImpact(
            Transform attacker,
            uint attackSequence,
            Vector3 worldPoint,
            ShowcaseUnarmedCombatAudioPlan plan)
        {
            AudioClip clip = ResolveClip(plan.Take);
            if (clip == null)
            {
                return false;
            }

            int voiceIndex = AcquireVoiceIndex();
            if (voiceIndex < 0)
            {
                VoiceExhaustionCount++;
                return false;
            }

            Voice voice = voices[voiceIndex];
            AudioSource source = voice.Source;
            int impactSample = Mathf.Clamp(
                plan.LeadSamples,
                0,
                Mathf.Max(0, clip.samples - 1));
            source.Stop();
            source.clip = clip;
            source.timeSamples = impactSample;
            source.pitch = 1f;
            source.volume = ConfirmedImpactVolume;
            source.transform.position = worldPoint;
            source.Play();

            double remainingSeconds =
                (clip.samples - impactSample) / (double)Mathf.Max(1, clip.frequency);
            voice.BusyUntilDsp = AudioSettings.dspTime + remainingSeconds +
                VoiceReleasePaddingSeconds;
            confirmedTakeCounts[(int)plan.Take]++;
            ConfirmedImpactPlaybackCount++;
            LastConfirmedTake = plan.Take;
            LastConfirmedAttacker = attacker;
            LastConfirmedAttackSequence = attackSequence;
            LastConfirmedWorldPoint = worldPoint;
            LastConfirmedStartSample = impactSample;
            LastConfirmedVolume = source.volume;
            return true;
        }

        private ShowcaseUnarmedCombatAudioTake ResolveNextRunnerFaceTake(
            ShowcaseRunnerAgent runner)
        {
            if (runner != null && lastRunnerFaceTake.TryGetValue(
                    runner.GetInstanceID(),
                    out ShowcaseUnarmedCombatAudioTake last) &&
                last == ShowcaseUnarmedCombatAudioTake.P03Cross)
            {
                return ShowcaseUnarmedCombatAudioTake.P04AlternateCross;
            }
            return ShowcaseUnarmedCombatAudioTake.P03Cross;
        }

        private int AcquireVoiceIndex()
        {
            double now = AudioSettings.dspTime;
            for (int index = 0; index < voices.Length; index++)
            {
                Voice voice = voices[index];
                if (voice?.Source != null && voice.BusyUntilDsp <= now)
                {
                    return index;
                }
            }
            return -1;
        }

        private void ReleaseFinishedVoices()
        {
            double now = AudioSettings.dspTime;
            foreach (Voice voice in voices)
            {
                if (voice?.Source == null || voice.BusyUntilDsp <= 0.0 ||
                    now < voice.BusyUntilDsp)
                {
                    continue;
                }

                voice.Source.Stop();
                voice.Source.clip = null;
                voice.Source.volume = ConfirmedImpactVolume;
                voice.BusyUntilDsp = 0.0;
            }
        }

        private AudioClip ResolveClip(ShowcaseUnarmedCombatAudioTake take)
        {
            return take switch
            {
                ShowcaseUnarmedCombatAudioTake.P01DeepBody => p01DeepBody,
                ShowcaseUnarmedCombatAudioTake.P03Cross => p03Cross,
                ShowcaseUnarmedCombatAudioTake.P04AlternateCross => p04AlternateCross,
                ShowcaseUnarmedCombatAudioTake.P08Jab => p08Jab,
                ShowcaseUnarmedCombatAudioTake.P09HeavyHook => p09HeavyHook,
                ShowcaseUnarmedCombatAudioTake.P10FastBody => p10FastBody,
                _ => null
            };
        }

        private void BuildVoiceRuntime()
        {
            voices = new Voice[authoredVoices?.Length ?? 0];
            for (int index = 0; index < voices.Length; index++)
            {
                AudioSource source = authoredVoices[index];
                if (source != null)
                {
                    source.volume = ConfirmedImpactVolume;
                }
                voices[index] = new Voice { Source = source };
            }
        }

        private void HandleBaselineReset()
        {
            StopAllVoices();
            lastRunnerFaceTake.Clear();
            playedAttacks.Clear();
            Array.Clear(confirmedTakeCounts, 0, confirmedTakeCounts.Length);
            AcceptedContactCount = 0;
            ConfirmedImpactPlaybackCount = 0;
            DuplicateContactSuppressionCount = 0;
            VoiceExhaustionCount = 0;
            LastConfirmedTake = ShowcaseUnarmedCombatAudioTake.None;
            LastConfirmedAttacker = null;
            LastConfirmedAttackSequence = 0;
            LastConfirmedWorldPoint = Vector3.zero;
            LastConfirmedStartSample = 0;
            LastConfirmedVolume = 0f;
        }

        private void StopAllVoices()
        {
            foreach (Voice voice in voices)
            {
                if (voice?.Source == null)
                {
                    continue;
                }
                voice.Source.Stop();
                voice.Source.clip = null;
                voice.Source.volume = ConfirmedImpactVolume;
                voice.BusyUntilDsp = 0.0;
            }
        }

        private void Subscribe()
        {
            if (pressureController == null)
            {
                return;
            }
            pressureController.CombatContactResolved -= HandleCombatContact;
            pressureController.BaselineReset -= HandleBaselineReset;
            pressureController.CombatContactResolved += HandleCombatContact;
            pressureController.BaselineReset += HandleBaselineReset;
        }

        private void Unsubscribe()
        {
            if (pressureController == null)
            {
                return;
            }
            pressureController.CombatContactResolved -= HandleCombatContact;
            pressureController.BaselineReset -= HandleBaselineReset;
        }
    }
}
