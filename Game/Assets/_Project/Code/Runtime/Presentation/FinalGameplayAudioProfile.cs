using System;
using System.Collections.Generic;
using UnityEngine;

namespace BreakNotice.Presentation
{
    [CreateAssetMenu(
        fileName = "FinalGameplayAudioProfile",
        menuName = "BREAK NOTICE/Audio/Final Gameplay Audio Profile")]
    public sealed class FinalGameplayAudioProfile : ScriptableObject
    {
        [Header("Binder")]
        [SerializeField] private AudioClip binderPickupClip;
        [SerializeField] private AudioClip binderThrowClip;
        [SerializeField] private AudioClip binderPlaceDownClip;
        [SerializeField] private AudioClip binderDirectHitClip;
        [SerializeField] private AudioClip binderWallHitClip;
        [SerializeField] private AudioClip binderFloorDropClip;
        [SerializeField] private AudioClip binderCatchClip;

        [Header("Chair")]
        [SerializeField] private AudioClip chairPickupClip;
        [SerializeField] private AudioClip chairReleaseClip;
        [SerializeField] private AudioClip chairThrowClip;
        [SerializeField] private AudioClip chairSwingWhooshClip;
        [SerializeField] private AudioClip chairRollLoopClip;
        [SerializeField] private AudioClip chairSkidClip;
        [SerializeField] private AudioClip chairEnemyImpactClip;
        [SerializeField] private AudioClip chairWorldImpactClip;
        [SerializeField] private AudioClip[] chairStopPool = Array.Empty<AudioClip>();

        [Header("Strap")]
        [SerializeField] private AudioClip strapExtendPart1Clip;
        [SerializeField] private AudioClip strapExtendPart2Clip;
        [SerializeField] private AudioClip strapLockClip;
        [SerializeField] private AudioClip strapTensionClip;

        [Header("Movement")]
        [SerializeField] private AudioClip[] footstepPool = Array.Empty<AudioClip>();
        [SerializeField] private AudioClip smallObjectSpinLoopClip;

        [Header("Player Voice")]
        [SerializeField] private AudioClip[] playerLightHurtPool = Array.Empty<AudioClip>();
        [SerializeField] private AudioClip playerHeavyHurtClip;
        [SerializeField] private AudioClip[] playerKoPool = Array.Empty<AudioClip>();

        [Header("Enemy Voice")]
        [SerializeField] private AudioClip[] enemyLightHurtPool = Array.Empty<AudioClip>();
        [SerializeField] private AudioClip[] enemyKoPool = Array.Empty<AudioClip>();

        public AudioClip BinderPickupClip => binderPickupClip;
        public AudioClip BinderThrowClip => binderThrowClip;
        public AudioClip BinderPlaceDownClip => binderPlaceDownClip;
        public AudioClip BinderDirectHitClip => binderDirectHitClip;
        public AudioClip BinderWallHitClip => binderWallHitClip;
        public AudioClip BinderFloorDropClip => binderFloorDropClip;
        public AudioClip BinderCatchClip => binderCatchClip;
        public AudioClip ChairPickupClip => chairPickupClip;
        public AudioClip ChairReleaseClip => chairReleaseClip;
        public AudioClip ChairThrowClip => chairThrowClip;
        public AudioClip ChairSwingWhooshClip => chairSwingWhooshClip;
        public AudioClip ChairRollLoopClip => chairRollLoopClip;
        public AudioClip ChairSkidClip => chairSkidClip;
        public AudioClip ChairEnemyImpactClip => chairEnemyImpactClip;
        public AudioClip ChairWorldImpactClip => chairWorldImpactClip;
        public AudioClip[] ChairStopPool => chairStopPool;
        public AudioClip StrapExtendPart1Clip => strapExtendPart1Clip;
        public AudioClip StrapExtendPart2Clip => strapExtendPart2Clip;
        public AudioClip StrapLockClip => strapLockClip;
        public AudioClip StrapTensionClip => strapTensionClip;
        public AudioClip[] FootstepPool => footstepPool;
        public AudioClip SmallObjectSpinLoopClip => smallObjectSpinLoopClip;
        public AudioClip[] PlayerLightHurtPool => playerLightHurtPool;
        public AudioClip PlayerHeavyHurtClip => playerHeavyHurtClip;
        public AudioClip[] PlayerKoPool => playerKoPool;
        public AudioClip[] EnemyLightHurtPool => enemyLightHurtPool;
        public AudioClip[] EnemyKoPool => enemyKoPool;

        public IEnumerable<AudioClip> EnumerateRequiredClips()
        {
            yield return binderPickupClip;
            yield return binderThrowClip;
            yield return binderPlaceDownClip;
            yield return binderDirectHitClip;
            yield return binderWallHitClip;
            yield return binderFloorDropClip;
            yield return binderCatchClip;
            yield return chairPickupClip;
            yield return chairReleaseClip;
            yield return chairThrowClip;
            yield return chairSwingWhooshClip;
            yield return chairRollLoopClip;
            yield return chairSkidClip;
            yield return chairEnemyImpactClip;
            yield return chairWorldImpactClip;
            foreach (AudioClip clip in chairStopPool) yield return clip;
            yield return strapExtendPart1Clip;
            yield return strapExtendPart2Clip;
            yield return strapLockClip;
            yield return strapTensionClip;
            foreach (AudioClip clip in footstepPool) yield return clip;
            yield return smallObjectSpinLoopClip;
            foreach (AudioClip clip in playerLightHurtPool) yield return clip;
            yield return playerHeavyHurtClip;
            foreach (AudioClip clip in playerKoPool) yield return clip;
            foreach (AudioClip clip in enemyLightHurtPool) yield return clip;
            foreach (AudioClip clip in enemyKoPool) yield return clip;
        }
    }
}
