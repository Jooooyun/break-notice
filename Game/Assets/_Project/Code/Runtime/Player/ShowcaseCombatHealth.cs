using System;
using UnityEngine;

namespace BreakNotice.Gameplay.Combat
{
    public enum ShowcaseCombatTeam
    {
        Player,
        Ally,
        Enemy
    }

    public enum ShowcaseHitReaction
    {
        Light,
        Body,
        Rib,
        Stomach,
        BigHead,
        Knockdown
    }

    public readonly struct ShowcaseDamageRequest
    {
        public ShowcaseDamageRequest(
            ShowcaseCombatTeam sourceTeam,
            string sourceId,
            uint attackSequence,
            int amount,
            ShowcaseHitReaction reaction,
            Vector3 impactDirection)
        {
            SourceTeam = sourceTeam;
            SourceId = sourceId ?? string.Empty;
            AttackSequence = attackSequence;
            Amount = Mathf.Max(0, amount);
            Reaction = reaction;
            ImpactDirection = impactDirection;
        }

        public ShowcaseCombatTeam SourceTeam { get; }
        public string SourceId { get; }
        public uint AttackSequence { get; }
        public int Amount { get; }
        public ShowcaseHitReaction Reaction { get; }
        public Vector3 ImpactDirection { get; }
    }

    public readonly struct ShowcaseDamageResult
    {
        public ShowcaseDamageResult(
            ShowcaseDamageRequest request,
            int previousHealth,
            int currentHealth)
        {
            Request = request;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
        }

        public ShowcaseDamageRequest Request { get; }
        public int PreviousHealth { get; }
        public int CurrentHealth { get; }
        public bool IsLethal => PreviousHealth > 0 && CurrentHealth == 0;
    }

    [DisallowMultipleComponent]
    public sealed class ShowcaseCombatHealth : MonoBehaviour
    {
        [SerializeField]
        private ShowcaseCombatTeam team;

        [SerializeField, Min(1)]
        private int maximumHealth = 100;

        [SerializeField, Min(0f)]
        private float invulnerabilitySeconds = 0.6f;

        private string lastAcceptedSourceId = string.Empty;
        private uint lastAcceptedAttackSequence;
        private float invulnerableUntil;

        public ShowcaseCombatTeam Team => team;
        public int MaximumHealth => maximumHealth;
        public int CurrentHealth { get; private set; }
        public float NormalizedHealth => maximumHealth > 0
            ? (float)CurrentHealth / maximumHealth
            : 0f;
        public bool IsDead => CurrentHealth <= 0;
        public int AcceptedDamageCount { get; private set; }
        public int RejectedDamageCount { get; private set; }
        public float InvulnerabilitySeconds => invulnerabilitySeconds;

        public event Action<ShowcaseDamageResult> DamageAccepted;
        public event Action<int, int> HealthChanged;
        public event Action Died;

        private void Awake()
        {
            if (CurrentHealth <= 0)
            {
                CurrentHealth = maximumHealth;
            }
        }

        public void Configure(
            ShowcaseCombatTeam authoredTeam,
            int authoredMaximumHealth,
            float authoredInvulnerabilitySeconds)
        {
            team = authoredTeam;
            maximumHealth = Mathf.Max(1, authoredMaximumHealth);
            invulnerabilitySeconds = Mathf.Max(0f, authoredInvulnerabilitySeconds);
            ResetToBaseline();
        }

        public bool TryApplyDamage(ShowcaseDamageRequest request)
        {
            return TryApplyDamage(request, Time.time);
        }

        public bool TryApplyDamage(ShowcaseDamageRequest request, float timestamp)
        {
            bool duplicate = request.SourceId == lastAcceptedSourceId &&
                request.AttackSequence == lastAcceptedAttackSequence;
            if (request.Amount <= 0 || IsDead || request.SourceTeam == team || duplicate ||
                timestamp < invulnerableUntil)
            {
                RejectedDamageCount++;
                return false;
            }

            int previous = CurrentHealth;
            CurrentHealth = Mathf.Max(0, CurrentHealth - request.Amount);
            lastAcceptedSourceId = request.SourceId;
            lastAcceptedAttackSequence = request.AttackSequence;
            invulnerableUntil = timestamp + invulnerabilitySeconds;
            AcceptedDamageCount++;

            var result = new ShowcaseDamageResult(request, previous, CurrentHealth);
            DamageAccepted?.Invoke(result);
            HealthChanged?.Invoke(CurrentHealth, maximumHealth);
            if (result.IsLethal)
            {
                Died?.Invoke();
            }
            return true;
        }

        public void ResetToBaseline()
        {
            CurrentHealth = Mathf.Max(1, maximumHealth);
            lastAcceptedSourceId = string.Empty;
            lastAcceptedAttackSequence = 0;
            invulnerableUntil = float.NegativeInfinity;
            AcceptedDamageCount = 0;
            RejectedDamageCount = 0;
            HealthChanged?.Invoke(CurrentHealth, maximumHealth);
        }
    }
}
