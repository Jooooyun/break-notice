using System;
using System.Collections.Generic;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using UnityEngine;

namespace BreakNotice.Presentation
{
    public enum ShowcaseHitVfxTier
    {
        Light,
        Medium,
        Heavy
    }

    [DefaultExecutionOrder(206)]
    [DisallowMultipleComponent]
    public sealed class ShowcaseCombatHitVfxPresenter : MonoBehaviour
    {
        private const int VariationCount = 3;
        private const int CopiesPerVariation = 2;
        private const float SurfaceOffset = 0.12f;
        private const float VisibleSimulationTime = 0.04f;

        private sealed class PoolSlot
        {
            public GameObject Instance;
            public ParticleSystem[] Systems;
            public float DisableAt;
            public float Lifetime;
        }

        [SerializeField]
        private ShowcaseRunnerPressureController pressureController;

        [SerializeField]
        private GameObject[] lightVariants = Array.Empty<GameObject>();

        [SerializeField]
        private GameObject[] mediumVariants = Array.Empty<GameObject>();

        [SerializeField]
        private GameObject[] heavyVariants = Array.Empty<GameObject>();

        private readonly List<PoolSlot>[,] pools =
            new List<PoolSlot>[3, VariationCount];
        private uint acceptedImpactSequence;
        private bool poolBuilt;

        public static ShowcaseCombatHitVfxPresenter Active { get; private set; }

        public int PlayedImpactCount { get; private set; }
        public ShowcaseHitVfxTier LastTier { get; private set; }
        public int LastVariationIndex { get; private set; } = -1;
        public Vector3 LastWorldPoint { get; private set; }

        public void Configure(
            ShowcaseRunnerPressureController pressure,
            GameObject[] light,
            GameObject[] medium,
            GameObject[] heavy)
        {
            Unsubscribe();
            pressureController = pressure;
            lightVariants = NormalizeVariants(light);
            mediumVariants = NormalizeVariants(medium);
            heavyVariants = NormalizeVariants(heavy);
            Subscribe();
            if (Application.isPlaying)
            {
                BuildPool();
            }
        }

        private void Awake()
        {
            BuildPool();
        }

        private void OnEnable()
        {
            Active = this;
            Subscribe();
        }

        private void OnDisable()
        {
            if (Active == this)
            {
                Active = null;
            }
            Unsubscribe();
            ClearPool();
        }

        private void Update()
        {
            float now = Time.unscaledTime;
            foreach (List<PoolSlot> slots in pools)
            {
                if (slots == null)
                {
                    continue;
                }
                foreach (PoolSlot slot in slots)
                {
                    if (slot.Instance.activeSelf && now >= slot.DisableAt)
                    {
                        slot.Instance.SetActive(false);
                    }
                }
            }
        }

        private void HandleCombatContact(ShowcaseCombatContactResult result)
        {
            if (!result.Accepted || result.Attacker == null)
            {
                return;
            }

            ShowcaseHitVfxTier tier = ResolveTier(result.Attacker);
            int variation = (int)(acceptedImpactSequence % VariationCount);
            acceptedImpactSequence++;
            PoolSlot slot = AcquireSlot(tier, variation);
            if (slot == null)
            {
                return;
            }

            Vector3 direction = result.Victim != null
                ? Vector3.ProjectOnPlane(
                    result.Victim.position - result.Attacker.position,
                    Vector3.up)
                : result.Attacker.forward;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = result.Attacker.forward;
            }

            Vector3 worldPoint = ResolveVisibleWorldPoint(result);
            slot.Instance.transform.SetPositionAndRotation(
                worldPoint,
                Quaternion.LookRotation(direction.normalized, Vector3.up));
            slot.Instance.transform.localScale = Vector3.one * ResolveScale(tier);
            slot.Instance.SetActive(true);
            foreach (ParticleSystem system in slot.Systems)
            {
                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                system.Clear(true);
                system.Simulate(
                    VisibleSimulationTime,
                    true,
                    true,
                    false);
                if (system.particleCount == 0)
                {
                    system.Emit(1);
                }
                system.Play(true);
            }
            slot.DisableAt = Time.unscaledTime + slot.Lifetime;

            PlayedImpactCount++;
            LastTier = tier;
            LastVariationIndex = variation;
            LastWorldPoint = worldPoint;
        }

        private static float ResolveScale(ShowcaseHitVfxTier tier)
        {
            return tier switch
            {
                ShowcaseHitVfxTier.Light => 1.15f,
                ShowcaseHitVfxTier.Medium => 1.35f,
                ShowcaseHitVfxTier.Heavy => 1.55f,
                _ => 1.25f
            };
        }

        private static Vector3 ResolveVisibleWorldPoint(
            ShowcaseCombatContactResult result)
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                Vector3 towardCamera = camera.transform.position - result.WorldPoint;
                if (towardCamera.sqrMagnitude > 0.001f)
                {
                    return result.WorldPoint +
                        (towardCamera.normalized * SurfaceOffset);
                }
            }

            Vector3 awayFromVictim = result.Victim != null
                ? result.WorldPoint - result.Victim.position
                : -result.Attacker.forward;
            return awayFromVictim.sqrMagnitude > 0.001f
                ? result.WorldPoint + (awayFromVictim.normalized * SurfaceOffset)
                : result.WorldPoint;
        }

        public static ShowcaseHitVfxTier ResolveTier(Transform attacker)
        {
            ShowcasePlayerCombatController player =
                attacker.GetComponent<ShowcasePlayerCombatController>();
            if (player != null)
            {
                return player.AnimationCue switch
                {
                    ShowcasePlayerAnimationCue.MeleeJab => ShowcaseHitVfxTier.Light,
                    ShowcasePlayerAnimationCue.MeleeCross => ShowcaseHitVfxTier.Medium,
                    ShowcasePlayerAnimationCue.MeleeHook => ShowcaseHitVfxTier.Heavy,
                    _ => ShowcaseHitVfxTier.Medium
                };
            }

            ShowcaseRunnerAgent runner = attacker.GetComponent<ShowcaseRunnerAgent>();
            if (runner != null)
            {
                return runner.AnimationCue switch
                {
                    ShowcaseRunnerAnimationCue.Boxing => ShowcaseHitVfxTier.Light,
                    ShowcaseRunnerAnimationCue.EliteMutantPunch => ShowcaseHitVfxTier.Medium,
                    ShowcaseRunnerAnimationCue.EliteMutantSwiping => ShowcaseHitVfxTier.Medium,
                    ShowcaseRunnerAnimationCue.Boxing2 => ShowcaseHitVfxTier.Heavy,
                    ShowcaseRunnerAnimationCue.Kicking => ShowcaseHitVfxTier.Heavy,
                    ShowcaseRunnerAnimationCue.Kicking2 => ShowcaseHitVfxTier.Heavy,
                    _ => ShowcaseHitVfxTier.Medium
                };
            }

            PrototypeInteractionTarget objectSource =
                attacker.GetComponent<PrototypeInteractionTarget>();
            return objectSource != null &&
                   objectSource.WeightClass == PrototypeWeightClass.Light
                ? ShowcaseHitVfxTier.Light
                : ShowcaseHitVfxTier.Medium;
        }

        private void BuildPool()
        {
            if (poolBuilt || !Application.isPlaying)
            {
                return;
            }

            GameObject[][] variants =
            {
                lightVariants,
                mediumVariants,
                heavyVariants
            };
            for (int tier = 0; tier < variants.Length; tier++)
            {
                for (int variation = 0; variation < VariationCount; variation++)
                {
                    pools[tier, variation] = new List<PoolSlot>(CopiesPerVariation);
                    GameObject prefab = variants[tier][variation];
                    if (prefab == null)
                    {
                        continue;
                    }

                    for (int copy = 0; copy < CopiesPerVariation; copy++)
                    {
                        GameObject instance = Instantiate(prefab, transform);
                        instance.name = $"{prefab.name}_POOL_{copy + 1:00}";
                        ParticleSystem[] systems = instance.GetComponentsInChildren<
                            ParticleSystem>(true);
                        float lifetime = 0.1f;
                        foreach (ParticleSystem system in systems)
                        {
                            ParticleSystem.MainModule main = system.main;
                            main.loop = false;
                            main.useUnscaledTime = true;
                            main.stopAction = ParticleSystemStopAction.None;
                            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
                            lifetime = Mathf.Max(
                                lifetime,
                                main.duration +
                                main.startDelay.constantMax +
                                main.startLifetime.constantMax + 0.05f);
                        }
                        instance.SetActive(false);
                        pools[tier, variation].Add(new PoolSlot
                        {
                            Instance = instance,
                            Systems = systems,
                            Lifetime = lifetime
                        });
                    }
                }
            }
            poolBuilt = true;
        }

        private PoolSlot AcquireSlot(ShowcaseHitVfxTier tier, int variation)
        {
            BuildPool();
            List<PoolSlot> slots = pools[(int)tier, variation];
            if (slots == null || slots.Count == 0)
            {
                return null;
            }

            foreach (PoolSlot slot in slots)
            {
                if (!slot.Instance.activeSelf)
                {
                    return slot;
                }
            }

            PoolSlot oldest = slots[0];
            for (int index = 1; index < slots.Count; index++)
            {
                if (slots[index].DisableAt < oldest.DisableAt)
                {
                    oldest = slots[index];
                }
            }
            oldest.Instance.SetActive(false);
            return oldest;
        }

        private void HandleBaselineReset()
        {
            acceptedImpactSequence = 0;
            PlayedImpactCount = 0;
            LastVariationIndex = -1;
            LastWorldPoint = Vector3.zero;
            ClearPool();
        }

        private void ClearPool()
        {
            foreach (List<PoolSlot> slots in pools)
            {
                if (slots == null)
                {
                    continue;
                }
                foreach (PoolSlot slot in slots)
                {
                    if (slot.Instance != null)
                    {
                        slot.Instance.SetActive(false);
                    }
                }
            }
        }

        private static GameObject[] NormalizeVariants(GameObject[] source)
        {
            var result = new GameObject[VariationCount];
            if (source != null)
            {
                Array.Copy(source, result, Mathf.Min(source.Length, result.Length));
            }
            return result;
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
