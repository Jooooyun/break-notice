using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using BreakNotice.Gameplay;
using BreakNotice.Gameplay.Combat;
using BreakNotice.Gameplay.Interaction;
using BreakNotice.Presentation;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestFinalAudioV004PlayModeTests
    {
        private const string ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v007.unity";

        private Scene loadedScene;

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(loadedScene);
                if (unload != null)
                {
                    yield return unload;
                }
            }

            loadedScene = default;
        }

        [UnityTest]
        public IEnumerator Scene_ComposesFinalProfileSourcesAndSingleListenerWithoutAwakePlayback()
        {
            yield return LoadScene();

            ShowcaseFinalGameplayAudioController audio =
                SceneComponents<ShowcaseFinalGameplayAudioController>().Single();
            Assert.That(audio.Profile, Is.Not.Null);

            AudioClip[] required = audio.Profile.EnumerateRequiredClips().ToArray();
            Assert.That(required, Has.Length.EqualTo(40));
            Assert.That(required, Has.None.Null);
            Assert.That(required.Distinct().Count(), Is.EqualTo(40),
                "Every final v004 role must retain its distinct approved WAV asset.");

            AudioListener[] listeners = SceneComponents<AudioListener>();
            Assert.That(listeners, Has.Length.EqualTo(1));
            Assert.That(listeners[0].enabled, Is.True);

            AudioSource[] sceneSources = SceneComponents<AudioSource>();
            Assert.That(sceneSources, Is.Not.Empty);
            Assert.That(sceneSources.All(source => !source.playOnAwake), Is.True,
                "No authored gameplay source may emit audio while the scene wakes up.");

            AssertSources(audio.TransientSources, loop: false, "transient");
            AssertSource(audio.StrapPart1Source, loop: false, "strap Z01");
            AssertSource(audio.StrapPart2Source, loop: false, "strap Z02");
            AssertSource(audio.PlayerFootstepSource, loop: false, "player footstep");
            AssertSources(audio.EnemyFootstepSources, loop: false, "enemy footstep");
            AssertSource(audio.PlayerVoiceSource, loop: false, "player voice");
            AssertSources(audio.EnemyVoiceSources, loop: false, "enemy voice");
            AssertSources(audio.ChairRollSources, loop: true, "chair roll");
            AssertSources(audio.SmallSpinSources, loop: true, "small spin");

            Assert.That(audio.ChairTargets, Has.Length.EqualTo(
                audio.ChairRollSources.Length));
            Assert.That(audio.LightTargets, Has.Length.EqualTo(
                audio.SmallSpinSources.Length));
            Assert.That(audio.Runners, Has.Length.EqualTo(
                audio.EnemyVoiceSources.Length));
            Assert.That(audio.Runners, Has.Length.EqualTo(
                audio.EnemyFootstepSources.Length));
        }

        [UnityTest]
        public IEnumerator DamageAccepted_KoReplacesHurtPerActorAndBaselineResetClearsVoices()
        {
            yield return LoadScene();
            DisableRunners();

            ShowcaseFinalGameplayAudioController audio =
                SceneComponents<ShowcaseFinalGameplayAudioController>().Single();
            SetPrivateFloat(audio, "playerLightHurtProbability", 1f);
            SetPrivateFloat(audio, "enemyLightHurtProbability", 1f);

            ShowcaseCombatHealth playerHealth = audio.PlayerHealth;
            AudioSource playerVoice = audio.PlayerVoiceSource;
            playerHealth.ResetToBaseline();

            Assert.That(playerHealth.TryApplyDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Enemy,
                "FINAL_AUDIO_TEST_ENEMY",
                1u,
                1,
                ShowcaseHitReaction.Light,
                Vector3.back), 10f), Is.True);
            Assert.That(audio.VoicePlaybackCount, Is.EqualTo(1));
            Assert.That(audio.Profile.PlayerLightHurtPool,
                Does.Contain(playerVoice.clip));
            AudioClip playerHurt = playerVoice.clip;

            Assert.That(playerHealth.TryApplyDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Enemy,
                "FINAL_AUDIO_TEST_ENEMY",
                2u,
                1,
                ShowcaseHitReaction.Light,
                Vector3.back), 10.7f), Is.True);
            Assert.That(audio.VoicePlaybackCount, Is.EqualTo(1),
                "A second accepted Light hit inside actor cooldown must not stack voice.");

            Assert.That(playerHealth.TryApplyDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Enemy,
                "FINAL_AUDIO_TEST_ENEMY",
                3u,
                25,
                ShowcaseHitReaction.Knockdown,
                Vector3.back), 20f), Is.True);
            Assert.That(playerHealth.IsDead, Is.False);
            Assert.That(audio.VoicePlaybackCount, Is.EqualTo(2));
            Assert.That(playerVoice.clip,
                Is.SameAs(audio.Profile.PlayerHeavyHurtClip),
                "Heavy Hurt must interrupt a lower-priority Light voice and cooldown.");

            Assert.That(playerHealth.TryApplyDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Enemy,
                "FINAL_AUDIO_TEST_ENEMY",
                4u,
                playerHealth.MaximumHealth,
                ShowcaseHitReaction.Knockdown,
                Vector3.back), 30f), Is.True);
            Assert.That(playerHealth.IsDead, Is.True);
            Assert.That(audio.VoicePlaybackCount, Is.EqualTo(3));
            Assert.That(audio.Profile.PlayerKoPool, Does.Contain(playerVoice.clip));
            Assert.That(playerVoice.clip, Is.Not.SameAs(playerHurt),
                "KO must replace the in-flight Hurt voice on the actor source.");
            Assert.That(audio.EnemyVoiceSources.All(source => !source.isPlaying), Is.True);

            audio.PressureController.ResetToRunBaseline();
            audio.PlayerCombat.ResetToRunBaseline();
            yield return null;
            Assert.That(playerHealth.CurrentHealth, Is.EqualTo(playerHealth.MaximumHealth));
            Assert.That(AllVoiceSources(audio).All(source => !source.isPlaying), Is.True);
            Assert.That(audio.VoicePlaybackCount, Is.Zero,
                "Baseline reset must also clear final-audio diagnostics.");

            ShowcaseRunnerAgent normal = audio.Runners.Single(runner => !runner.IsElite);
            int normalIndex = Array.IndexOf(audio.Runners, normal);
            Assert.That(normalIndex, Is.GreaterThanOrEqualTo(0));
            AudioSource enemyVoice = audio.EnemyVoiceSources[normalIndex];
            ShowcaseCombatHealth enemyHealth = normal.Health;

            Assert.That(enemyHealth.TryApplyDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Player,
                "FINAL_AUDIO_TEST_PLAYER",
                5u,
                1,
                ShowcaseHitReaction.Light,
                Vector3.forward), 30f), Is.True);
            Assert.That(audio.Profile.EnemyLightHurtPool,
                Does.Contain(enemyVoice.clip));
            AudioClip enemyHurt = enemyVoice.clip;

            Assert.That(enemyHealth.TryApplyDamage(new ShowcaseDamageRequest(
                ShowcaseCombatTeam.Player,
                "FINAL_AUDIO_TEST_PLAYER",
                6u,
                enemyHealth.MaximumHealth,
                ShowcaseHitReaction.Knockdown,
                Vector3.forward), 40f), Is.True);
            Assert.That(enemyHealth.IsDead, Is.True);
            Assert.That(audio.Profile.EnemyKoPool, Does.Contain(enemyVoice.clip));
            Assert.That(enemyVoice.clip, Is.Not.SameAs(enemyHurt));
            Assert.That(audio.EnemyVoiceSources.Where((_, index) => index != normalIndex)
                .All(source => !source.isPlaying), Is.True,
                "A KO transaction must not allocate another enemy actor's voice source.");

            audio.PressureController.ResetToRunBaseline();
            yield return null;
            Assert.That(enemyHealth.CurrentHealth, Is.EqualTo(enemyHealth.MaximumHealth));
            Assert.That(AllVoiceSources(audio).All(source => !source.isPlaying), Is.True);
        }

        [UnityTest]
        public IEnumerator TargetPairCollision_UsesAuthoritativeReleasedObjectImpactFamily()
        {
            yield return LoadScene();
            DisableRunners();

            ShowcaseFinalGameplayAudioController audio =
                SceneComponents<ShowcaseFinalGameplayAudioController>().Single();
            PrototypeInteractionTarget chair = audio.ChairTargets.First();
            PrototypeInteractionTarget binder = audio.LightTargets.First();
            Rigidbody chairBody = chair.GetComponent<Rigidbody>();
            Rigidbody binderBody = binder.GetComponent<Rigidbody>();
            Collider chairCollider = chair.GetComponent<Collider>();
            Collider binderCollider = binder.GetComponent<Collider>();
            chairBody.isKinematic = false;
            binderBody.isKinematic = false;
            chairBody.linearVelocity = chair.transform.forward;
            binderBody.linearVelocity = binder.transform.forward * 3f;
            InvokePrivate(audio, "HandlePlayerThrowCommitted",
                new PhysicalReleaseSnapshot(
                    Time.frameCount,
                    699u,
                    chair,
                    chair.transform.position,
                    chair.transform.rotation,
                    chairBody.linearVelocity,
                    Vector3.zero,
                    0f));

            int frame = Time.frameCount;
            int before = audio.OneShotPlaybackCount;
            InvokePrivate(audio, "HandleCollisionEntered",
                new PrototypeInteractionCollisionSnapshot(
                    chair,
                    binderCollider,
                    chair.transform.position,
                    Vector3.forward,
                    3f,
                    frame,
                    700u));
            InvokePrivate(audio, "HandleCollisionEntered",
                new PrototypeInteractionCollisionSnapshot(
                    binder,
                    chairCollider,
                    binder.transform.position,
                    Vector3.back,
                    3f,
                    frame,
                    701u));
            InvokePrivate(audio, "ProcessPendingImpacts");

            Assert.That(audio.OneShotPlaybackCount, Is.EqualTo(before + 1));
            Assert.That(audio.LastImpactFamily,
                Is.EqualTo(FinalGameplayAudioImpactFamily.ChairWorld));
            Assert.That(audio.LastPlayedClip,
                Is.SameAs(audio.Profile.ChairWorldImpactClip));
        }

        [UnityTest]
        public IEnumerator QueuedBinderCatch_PlaysAtHandCommitBeforeThrowAndNotAtSettlement()
        {
            yield return LoadScene();
            DisableRunners();

            ShowcaseFinalGameplayAudioController audio =
                SceneComponents<ShowcaseFinalGameplayAudioController>().Single();
            PrototypeInteractionTarget binder = audio.LightTargets.First(target =>
                target.Role == PrototypeInteractionRole.AllyBinder);
            int before = audio.OneShotPlaybackCount;

            InvokePrivate(audio, "HandleQueuedRelayCatchCommitted", binder);
            Assert.That(audio.OneShotPlaybackCount, Is.EqualTo(before + 1));
            Assert.That(audio.LastPlayedClip, Is.SameAs(audio.Profile.BinderCatchClip));

            InvokePrivate(audio, "HandlePlayerThrowCommitted",
                new PhysicalReleaseSnapshot(
                    Time.frameCount,
                    812u,
                    binder,
                    binder.transform.position,
                    binder.transform.rotation,
                    Vector3.forward * 8f,
                    Vector3.zero,
                    0f));
            Assert.That(audio.OneShotPlaybackCount, Is.EqualTo(before + 2));
            Assert.That(audio.LastPlayedClip, Is.SameAs(audio.Profile.BinderThrowClip));

            InvokePrivate(audio, "HandleRelayCatchCompleted", binder);
            Assert.That(audio.OneShotPlaybackCount, Is.EqualTo(before + 2),
                "Release-time relay settlement must not replay queued Catch audio.");
            Assert.That(audio.LastPlayedClip, Is.SameAs(audio.Profile.BinderThrowClip),
                "Queued playback order must remain Catch then Throw.");
        }

        [UnityTest]
        public IEnumerator Footsteps_UseGroundedActorLocalStateAndEnemyCommandedSpeed()
        {
            yield return LoadScene();

            ShowcaseFinalGameplayAudioController audio =
                SceneComponents<ShowcaseFinalGameplayAudioController>().Single();
            ShowcaseAnimationEventReceiver playerReceiver = audio.Receivers.Single(
                receiver => receiver.GetComponentInParent<
                    ShowcasePlayerCombatController>() != null);
            CharacterController playerController =
                audio.Locomotion.GetComponent<CharacterController>();
            Assert.That(playerController, Is.Not.Null);

            playerController.Move(audio.PlayerCombat.transform.forward * 0.08f);
            int before = audio.FootstepPlaybackCount;
            playerReceiver.ShowcaseLeftFootstep();
            Assert.That(audio.FootstepPlaybackCount, Is.EqualTo(before + 1));
            int playerIndex = audio.GetActorLastFootstepIndex(
                audio.PlayerCombat.transform);
            Assert.That(playerIndex, Is.InRange(0, 7));

            ShowcaseRunnerAgent runner = audio.Runners.First();
            runner.enabled = false;
            SetRunnerCommandedSpeed(runner, 2.8f);
            ShowcaseAnimationEventReceiver runnerReceiver = audio.Receivers.Single(
                receiver => receiver.GetComponentInParent<ShowcaseRunnerAgent>() ==
                    runner);
            runnerReceiver.ShowcaseRightFootstep();
            Assert.That(audio.FootstepPlaybackCount, Is.EqualTo(before + 2));
            Assert.That(audio.GetActorLastFootstepIndex(runner.transform),
                Is.InRange(0, 7));
            Assert.That(audio.GetActorLastFootstepIndex(audio.PlayerCombat.transform),
                Is.EqualTo(playerIndex),
                "Enemy selection must not mutate the Player's random-pool state.");

            playerController.enabled = false;
            audio.PlayerCombat.transform.position += Vector3.up * 2f;
            playerController.enabled = true;
            playerController.Move(audio.PlayerCombat.transform.forward * 0.08f);
            Physics.SyncTransforms();
            playerReceiver.ShowcaseRightFootstep();
            Assert.That(audio.FootstepPlaybackCount, Is.EqualTo(before + 2),
                "Airborne animation contacts must remain silent.");
        }

        [UnityTest]
        public IEnumerator ChairMotion_RollsStopsWithoutRepeatAndDefersStopAfterWorldImpact()
        {
            yield return LoadScene();
            DisableRunners();

            ShowcaseFinalGameplayAudioController audio =
                SceneComponents<ShowcaseFinalGameplayAudioController>().Single();
            PrototypeInteractionTarget chair = audio.ChairTargets.First(target =>
                target.Role == PrototypeInteractionRole.DirectChair);
            Rigidbody body = chair.GetComponent<Rigidbody>();
            Collider chairCollider = chair.GetComponent<Collider>();
            Assert.That(body, Is.Not.Null);
            Assert.That(chairCollider, Is.Not.Null);

            body.isKinematic = false;
            body.useGravity = false;
            body.detectCollisions = false;
            body.constraints |= RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezeRotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            yield return new WaitForFixedUpdate();

            int beforeSidewaysMotion = audio.OneShotPlaybackCount;
            body.linearVelocity = chair.transform.right * 3f;
            yield return null;
            Assert.That(audio.OneShotPlaybackCount,
                Is.EqualTo(beforeSidewaysMotion),
                "Sideways Chair movement must not emit the removed Skid " +
                "one-shot; only the quieter persistent Roll loop remains.");

            int beforeFirstStop = audio.OneShotPlaybackCount;
            yield return BeginChairRoll(audio, chair, body);
            body.linearVelocity = Vector3.zero;
            yield return WaitUntilOrFail(
                () => !audio.IsChairRollPlaying(chair) &&
                      audio.Profile.ChairStopPool.Contains(audio.LastPlayedClip),
                1f,
                "The first Moving-to-Stopped transition did not emit one stop take.");
            AudioClip firstStop = audio.LastPlayedClip;
            Assert.That(audio.OneShotPlaybackCount,
                Is.EqualTo(beforeFirstStop + 1));

            yield return BeginChairRoll(audio, chair, body);
            int beforeSecondStop = audio.OneShotPlaybackCount;
            body.linearVelocity = Vector3.zero;
            yield return WaitUntilOrFail(
                () => !audio.IsChairRollPlaying(chair) &&
                      audio.Profile.ChairStopPool.Contains(audio.LastPlayedClip) &&
                      audio.LastPlayedClip != firstStop,
                1f,
                "The second stop did not choose the alternate non-repeating take.");
            AudioClip secondStop = audio.LastPlayedClip;
            Assert.That(audio.OneShotPlaybackCount,
                Is.EqualTo(beforeSecondStop + 1),
                "Chair Stop A/B must select exactly one clip per transition.");
            Assert.That(secondStop, Is.Not.SameAs(firstStop));

            yield return BeginChairRoll(audio, chair, body);
            body.linearVelocity = Vector3.zero;
            yield return new WaitForSecondsRealtime(0.10f);

            Collider floor = Physics.RaycastAll(
                    chairCollider.bounds.center + Vector3.up * 0.25f,
                    Vector3.down,
                    2f,
                    ~0,
                    QueryTriggerInteraction.Ignore)
                .Select(hit => hit.collider)
                .FirstOrDefault(candidate => candidate != null &&
                    candidate.transform != chair.transform &&
                    !candidate.transform.IsChildOf(chair.transform) &&
                    candidate.GetComponentInParent<ShowcaseRunnerAgent>() == null);
            Assert.That(floor, Is.Not.Null,
                "The authored floor collider was not available to route a Chair World impact.");

            var worldImpact = new PrototypeInteractionCollisionSnapshot(
                chair,
                floor,
                chairCollider.bounds.min,
                Vector3.up,
                3f,
                Time.frameCount,
                991u);
            InvokePrivate(audio, "HandleCollisionEntered", worldImpact);
            InvokePrivate(audio, "ProcessPendingImpacts");
            Assert.That(audio.LastImpactFamily,
                Is.EqualTo(FinalGameplayAudioImpactFamily.ChairWorld));
            Assert.That(audio.LastPlayedClip,
                Is.SameAs(audio.Profile.ChairWorldImpactClip));
            int afterWorldImpact = audio.OneShotPlaybackCount;

            yield return new WaitForSecondsRealtime(0.09f);
            Assert.That(audio.OneShotPlaybackCount, Is.EqualTo(afterWorldImpact),
                "Stop must remain suppressed while the world-impact guard is active.");
            Assert.That(audio.LastPlayedClip,
                Is.SameAs(audio.Profile.ChairWorldImpactClip));

            yield return WaitUntilOrFail(
                () => audio.Profile.ChairStopPool.Contains(audio.LastPlayedClip),
                0.5f,
                "The deferred stop did not play after the world-impact guard expired.");
            Assert.That(audio.OneShotPlaybackCount, Is.EqualTo(afterWorldImpact + 1));
            Assert.That(audio.LastPlayedClip, Is.Not.SameAs(secondStop),
                "The actual stop pool must retain no-immediate-repeat state.");
        }

        [UnityTest]
        public IEnumerator StrapAndSmallSpin_ScheduleInOrderAndLeaveNoAudioAfterDisableOrReset()
        {
            yield return LoadScene();
            DisableRunners();

            ShowcaseFinalGameplayAudioController audio =
                SceneComponents<ShowcaseFinalGameplayAudioController>().Single();
            ShowcaseCargoStrapController cargo = audio.CargoStrap;
            PrototypeInteractionTarget light = audio.LightTargets.First(target =>
                target.Role == PrototypeInteractionRole.DirectBinder);
            Rigidbody body = light.GetComponent<Rigidbody>();
            Assert.That(body, Is.Not.Null);
            Vector3 authoredPosition = light.transform.position;
            Quaternion authoredRotation = light.transform.rotation;

            PositionPlayerForDirectAttach(audio.PlayerCombat.transform, light);
            yield return new WaitForFixedUpdate();
            Assert.That(cargo.TryBeginDirectAttach(light), Is.True);
            Assert.That(audio.StrapSequencePlaybackCount, Is.EqualTo(1));
            Assert.That(audio.StrapPart1Source.clip,
                Is.SameAs(audio.Profile.StrapExtendPart1Clip));
            Assert.That(audio.StrapPart2Source.clip,
                Is.SameAs(audio.Profile.StrapExtendPart2Clip));

            CompleteStrapAttachSequence(cargo);
            Assert.That(cargo.IsSmallSpinning, Is.True);
            body.angularVelocity = Vector3.up * 5f;

            yield return WaitUntilOrFail(
                () => audio.StrapPart1Source.timeSamples > 0,
                0.35f,
                "Strap Z01 did not enter DSP playback.");
            Assert.That(audio.StrapPart2Source.timeSamples, Is.Zero,
                "Z02 must remain at sample zero while Z01 is playing.");
            yield return WaitUntilOrFail(
                () => audio.StrapPart2Source.timeSamples > 0,
                1f,
                "Strap Z02 did not begin after the scheduled Z01 duration.");
            yield return WaitUntilOrFail(
                () => audio.IsSmallSpinPlaying(light),
                0.5f,
                "The attached Small object did not enter its dedicated spin loop.");

            audio.enabled = false;
            yield return null;
            Assert.That(AllAuthoredSources(audio).All(source => !source.isPlaying), Is.True);
            Assert.That(audio.IsSmallSpinPlaying(light), Is.False);

            audio.enabled = true;
            audio.PressureController.ResetToRunBaseline();
            cargo.ResetToRunBaseline();
            body.position = authoredPosition;
            body.rotation = authoredRotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            light.transform.SetPositionAndRotation(authoredPosition, authoredRotation);
            PositionPlayerForDirectAttach(audio.PlayerCombat.transform, light);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(cargo.TryBeginDirectAttach(light), Is.True);
            Assert.That(audio.StrapSequencePlaybackCount, Is.EqualTo(1));
            audio.PressureController.ResetToRunBaseline();
            yield return null;

            Assert.That(audio.StrapSequencePlaybackCount, Is.Zero);
            Assert.That(AllAuthoredSources(audio).All(source => !source.isPlaying), Is.True,
                "Baseline reset must cancel scheduled segments and every motion loop.");
            Assert.That(audio.IsSmallSpinPlaying(light), Is.False);
        }

        private IEnumerator LoadScene()
        {
            Scene existing = SceneManager.GetSceneByPath(ScenePath);
            if (existing.IsValid() && existing.isLoaded)
            {
                AsyncOperation unloadExisting = SceneManager.UnloadSceneAsync(existing);
                if (unloadExisting != null)
                {
                    yield return unloadExisting;
                }
                yield return null;
            }

            AsyncOperation load = EditorSceneManager.LoadSceneAsyncInPlayMode(
                ScenePath,
                new LoadSceneParameters(LoadSceneMode.Additive));
            Assert.That(load, Is.Not.Null);
            yield return load;
            loadedScene = SceneManager.GetSceneByPath(ScenePath);
            Assert.That(loadedScene.IsValid() && loadedScene.isLoaded, Is.True);
            yield return null;
        }

        private T[] SceneComponents<T>() where T : Component
        {
            return UnityEngine.Object.FindObjectsByType<T>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(component => component.gameObject.scene == loadedScene)
                .ToArray();
        }

        private void DisableRunners()
        {
            foreach (ShowcaseRunnerAgent runner in SceneComponents<ShowcaseRunnerAgent>())
            {
                runner.enabled = false;
            }
        }

        private static IEnumerator BeginChairRoll(
            ShowcaseFinalGameplayAudioController audio,
            PrototypeInteractionTarget chair,
            Rigidbody body)
        {
            body.linearVelocity = chair.transform.forward * 2f;
            yield return WaitUntilOrFail(
                () => audio.IsChairRollPlaying(chair),
                0.5f,
                "A grounded moving Chair did not enter its persistent roll loop.");
            Assert.That(audio.ChairRollSources[
                Array.IndexOf(audio.ChairTargets, chair)].loop, Is.True);
        }

        private static void CompleteStrapAttachSequence(
            ShowcaseCargoStrapController cargo)
        {
            uint prepareRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(prepareRevision), Is.True);
            Assert.That(cargo.CompleteAnimationStrapAction(prepareRevision), Is.True);

            uint throwRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapThrowRelease(throwRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapAttachContact(throwRevision), Is.True);
            Assert.That(cargo.CompleteAnimationStrapAction(throwRevision), Is.True);

            uint pullRevision = cargo.AnimationCueRevision;
            Assert.That(cargo.NotifyAnimationStateEntered(pullRevision), Is.True);
            Assert.That(cargo.CommitAnimationStrapPullComplete(pullRevision), Is.True);
        }

        private static void PositionPlayerForDirectAttach(
            Transform player,
            PrototypeInteractionTarget target)
        {
            player.position = target.transform.position + new Vector3(0f, 0.9f, -2f);
            Physics.SyncTransforms();
        }

        private static AudioSource[] AllVoiceSources(
            ShowcaseFinalGameplayAudioController audio)
        {
            return new[] { audio.PlayerVoiceSource }
                .Concat(audio.EnemyVoiceSources)
                .Where(source => source != null)
                .ToArray();
        }

        private static AudioSource[] AllAuthoredSources(
            ShowcaseFinalGameplayAudioController audio)
        {
            return audio.TransientSources
                .Concat(new[]
                {
                    audio.StrapPart1Source,
                    audio.StrapPart2Source,
                    audio.PlayerFootstepSource,
                    audio.PlayerVoiceSource
                })
                .Concat(audio.EnemyFootstepSources)
                .Concat(audio.EnemyVoiceSources)
                .Concat(audio.ChairRollSources)
                .Concat(audio.SmallSpinSources)
                .Where(source => source != null)
                .Distinct()
                .ToArray();
        }

        private static void AssertSources(
            AudioSource[] sources,
            bool loop,
            string label)
        {
            Assert.That(sources, Is.Not.Null, label);
            Assert.That(sources, Is.Not.Empty, label);
            foreach (AudioSource source in sources)
            {
                AssertSource(source, loop, label);
            }
        }

        private static void AssertSource(
            AudioSource source,
            bool loop,
            string label)
        {
            Assert.That(source, Is.Not.Null, label);
            Assert.That(source.playOnAwake, Is.False, label);
            Assert.That(source.loop, Is.EqualTo(loop), label);
        }

        private static IEnumerator WaitUntilOrFail(
            Func<bool> predicate,
            float timeout,
            string message)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!predicate() && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }
            Assert.That(predicate(), Is.True, message);
        }

        private static void SetPrivateFloat(
            ShowcaseFinalGameplayAudioController target,
            string fieldName,
            float value)
        {
            FieldInfo field = typeof(ShowcaseFinalGameplayAudioController).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static void SetRunnerCommandedSpeed(
            ShowcaseRunnerAgent runner,
            float value)
        {
            PropertyInfo property = typeof(ShowcaseRunnerAgent).GetProperty(
                nameof(ShowcaseRunnerAgent.CommandedPlanarSpeed),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null);
            property.SetValue(runner, value);
        }

        private static object InvokePrivate(
            ShowcaseFinalGameplayAudioController target,
            string methodName,
            params object[] arguments)
        {
            MethodInfo method = typeof(ShowcaseFinalGameplayAudioController).GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, methodName);
            return method.Invoke(target, arguments);
        }
    }
}
