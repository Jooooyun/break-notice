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
    public sealed class ContestUnarmedCombatAudioPlayModeTests
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
        public IEnumerator PlayerRapidCombo_PlaysOnlyAcceptedJabAndCrossContacts()
        {
            yield return LoadScene();

            ShowcaseUnarmedCombatAudioController audio =
                SceneComponents<ShowcaseUnarmedCombatAudioController>().Single();
            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>().Single();
            ShowcaseRunnerAgent target = pressure.Runners.Single(runner => !runner.IsElite);
            ShowcaseBoneCombatRig rig = combat.GetComponent<ShowcaseBoneCombatRig>();
            ShowcaseCharacterAnimationDriver driver =
                combat.GetComponent<ShowcaseCharacterAnimationDriver>();
            ShowcaseAnimationEventReceiver receiver =
                driver.Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            yield return WaitUntilOrFail(
                () => driver.IsRuntimeConfigured &&
                      driver.CurrentBaseStateName == "Idle",
                1f,
                "The player animation driver was not settled before the audio combo test.");
            yield return new WaitForFixedUpdate();
            foreach (ShowcaseRunnerAgent runner in pressure.Runners)
            {
                runner.enabled = false;
            }
            PlaceRunnerInFront(combat.transform, target);

            Assert.That(interaction.ProcessAttackPressed(), Is.True);
            Assert.That(pressure.TryPlayerShove(), Is.True,
                "The rapid second press must reserve Cross.");
            Assert.That(combat.TryRequestMelee(target), Is.True,
                "The rapid third press must reserve Hook.");
            Assert.That(audio.ConfirmedImpactPlaybackCount, Is.Zero,
                "Starting an attack must remain silent until physical contact is accepted.");
            Assert.That(audio.AuthoredVoices.All(voice => voice.clip == null), Is.True);
            float comboDeadline = Time.realtimeSinceStartup + 3f;
            while ((receiver.AttackWindowOpenInvocationCount < 3 ||
                    combat.IsActionLocked ||
                    audio.ConfirmedImpactPlaybackCount < 2) &&
                   Time.realtimeSinceStartup < comboDeadline)
            {
                yield return null;
            }
            Assert.That(
                receiver.AttackWindowOpenInvocationCount >= 3 &&
                !combat.IsActionLocked &&
                rig.AcceptedHitCount == 2 &&
                audio.ConfirmedImpactPlaybackCount == 2,
                Is.True,
                $"The natural Jab/Cross/Jab contacts did not trigger their impact tails. " +
                $"hits={rig.AcceptedHitCount}, contacts={audio.AcceptedContactCount}, " +
                $"played={audio.ConfirmedImpactPlaybackCount}, cue={combat.AnimationCue}, " +
                $"step={combat.ActiveMeleeStep}, locked={combat.IsActionLocked}, " +
                $"state={driver.CurrentBaseStateName}, " +
                $"openEvents={receiver.AttackWindowOpenInvocationCount}.");

            Assert.That(audio.GetConfirmedTakeCount(
                ShowcaseUnarmedCombatAudioTake.P08Jab), Is.EqualTo(1));
            Assert.That(audio.GetConfirmedTakeCount(
                ShowcaseUnarmedCombatAudioTake.P03Cross), Is.EqualTo(1));
            Assert.That(audio.GetConfirmedTakeCount(
                ShowcaseUnarmedCombatAudioTake.P09HeavyHook), Is.Zero);
            Assert.That(audio.ConfirmedImpactPlaybackCount, Is.EqualTo(2));
            Assert.That(audio.VoiceExhaustionCount, Is.Zero);
            Assert.That(new[]
            {
                ShowcaseUnarmedCombatAudioTake.P08Jab,
                ShowcaseUnarmedCombatAudioTake.P03Cross
            }, Does.Contain(audio.LastConfirmedTake));
            Assert.That(audio.LastConfirmedStartSample,
                Is.EqualTo(audio.LastConfirmedTake ==
                    ShowcaseUnarmedCombatAudioTake.P08Jab
                        ? ShowcaseUnarmedCombatAudioTiming.P08LeadSamples
                        : ShowcaseUnarmedCombatAudioTiming.P03LeadSamples));
            Assert.That(audio.LastConfirmedVolume,
                Is.EqualTo(ShowcaseUnarmedCombatAudioController.
                    ConfirmedImpactVolume));
            Assert.That(target.Health.CurrentHealth, Is.EqualTo(65));
        }

        [UnityTest]
        public IEnumerator RunnerMelee_PlaysAlternatingFaceAndBodyTakesOnContact()
        {
            yield return LoadScene();
            ShowcaseUnarmedCombatAudioController audio =
                SceneComponents<ShowcaseUnarmedCombatAudioController>().Single();
            ShowcasePlayerCombatController player =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent normal = pressure.Runners.Single(runner => !runner.IsElite);
            foreach (ShowcaseRunnerAgent runner in pressure.Runners)
            {
                runner.enabled = runner == normal;
            }
            PositionRunnerAtAttackStandoff(player.transform, normal);
            SetPrivateIntField(normal, "punchesUntilKick", 2);
            Assert.That(pressure.TryAcquireAttackToken(normal), Is.True);
            normal.SendMessage("CommitNextAttack", SendMessageOptions.RequireReceiver);

            yield return WaitUntilOrFail(
                () => audio.GetConfirmedTakeCount(
                          ShowcaseUnarmedCombatAudioTake.P03Cross) == 1,
                3f,
                "The first accepted Runner Boxing contact did not play P03.");
            yield return WaitUntilOrFail(
                () => audio.GetConfirmedTakeCount(
                          ShowcaseUnarmedCombatAudioTake.P04AlternateCross) == 1 &&
                      audio.AcceptedContactCount >= 2,
                4f,
                "The second accepted Runner Boxing contact did not alternate to P04.");

            yield return ReloadScene();
            audio = SceneComponents<ShowcaseUnarmedCombatAudioController>().Single();
            player = SceneComponents<ShowcasePlayerCombatController>().Single();
            pressure = SceneComponents<ShowcaseRunnerPressureController>().Single();
            normal = pressure.Runners.Single(runner => !runner.IsElite);
            foreach (ShowcaseRunnerAgent runner in pressure.Runners)
            {
                runner.enabled = runner == normal;
            }
            SeedKick(normal, 0);
            PositionRunnerAtAttackStandoff(player.transform, normal);
            Assert.That(pressure.TryAcquireAttackToken(normal), Is.True);
            normal.SendMessage("CommitNextAttack", SendMessageOptions.RequireReceiver);
            Assert.That(normal.AnimationCue, Is.EqualTo(ShowcaseRunnerAnimationCue.Kicking));
            yield return WaitUntilOrFail(
                () => audio.GetConfirmedTakeCount(
                          ShowcaseUnarmedCombatAudioTake.P01DeepBody) == 1 &&
                      audio.AcceptedContactCount == 1,
                3f,
                "The heavy body kick did not use P01 at its natural contact.");

            yield return ReloadScene();
            audio = SceneComponents<ShowcaseUnarmedCombatAudioController>().Single();
            player = SceneComponents<ShowcasePlayerCombatController>().Single();
            pressure = SceneComponents<ShowcaseRunnerPressureController>().Single();
            normal = pressure.Runners.Single(runner => !runner.IsElite);
            foreach (ShowcaseRunnerAgent runner in pressure.Runners)
            {
                runner.enabled = runner == normal;
            }
            SeedKick(normal, 1);
            PositionRunnerAtAttackStandoff(player.transform, normal);
            Assert.That(pressure.TryAcquireAttackToken(normal), Is.True);
            normal.SendMessage("CommitNextAttack", SendMessageOptions.RequireReceiver);
            Assert.That(normal.AnimationCue, Is.EqualTo(ShowcaseRunnerAnimationCue.Kicking2));
            yield return WaitUntilOrFail(
                () => audio.GetConfirmedTakeCount(
                          ShowcaseUnarmedCombatAudioTake.P10FastBody) == 1 &&
                      audio.AcceptedContactCount == 1,
                3f,
                "The short body kick did not use P10 at its natural contact.");
            Assert.That(audio.VoiceExhaustionCount, Is.Zero);
            Assert.That(audio.LastConfirmedVolume,
                Is.EqualTo(ShowcaseUnarmedCombatAudioController.
                    ConfirmedImpactVolume));
        }

        [UnityTest]
        public IEnumerator PlayerAndRunnerWhiffs_RemainCompletelySilent()
        {
            yield return LoadScene();

            ShowcaseUnarmedCombatAudioController audio =
                SceneComponents<ShowcaseUnarmedCombatAudioController>().Single();
            ShowcasePlayerCombatController combat =
                SceneComponents<ShowcasePlayerCombatController>().Single();
            ShowcaseRunnerPressureController pressure =
                SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent target = pressure.Runners.Single(runner => !runner.IsElite);
            foreach (ShowcaseRunnerAgent runner in pressure.Runners)
            {
                runner.enabled = false;
            }

            Rigidbody targetBody = target.GetComponent<Rigidbody>();
            Vector3 farPosition = combat.transform.position + combat.transform.forward * 4f;
            targetBody.position = farPosition;
            target.transform.position = farPosition;
            Physics.SyncTransforms();
            ShowcaseAnimationEventReceiver playerReceiver = combat
                .GetComponent<ShowcaseCharacterAnimationDriver>()
                .Animator.GetComponent<ShowcaseAnimationEventReceiver>();

            Assert.That(combat.TryRequestMelee(null), Is.True);
            yield return WaitUntilOrFail(
                () => playerReceiver.AttackWindowOpenInvocationCount >= 1,
                2f,
                "The player whiff window did not open.");
            yield return WaitUntilOrFail(
                () => !combat.IsActionLocked,
                2f,
                "The player whiff did not complete.");
            AssertWhiffStayedSilent(audio, "player");

            yield return ReloadScene();
            audio = SceneComponents<ShowcaseUnarmedCombatAudioController>().Single();
            combat = SceneComponents<ShowcasePlayerCombatController>().Single();
            pressure = SceneComponents<ShowcaseRunnerPressureController>().Single();
            ShowcaseRunnerAgent normal = pressure.Runners.Single(runner => !runner.IsElite);
            foreach (ShowcaseRunnerAgent runner in pressure.Runners)
            {
                runner.enabled = runner == normal;
            }
            PositionRunnerAtAttackStandoff(combat.transform, normal);
            SetPrivateIntField(normal, "punchesUntilKick", 2);
            ShowcaseAnimationEventReceiver runnerReceiver = normal
                .GetComponent<ShowcaseCharacterAnimationDriver>()
                .Animator.GetComponent<ShowcaseAnimationEventReceiver>();
            int startingHealth = combat.Health.CurrentHealth;
            Assert.That(pressure.TryAcquireAttackToken(normal), Is.True);
            normal.SendMessage("CommitNextAttack", SendMessageOptions.RequireReceiver);

            ThirdPersonLocomotion locomotion = combat.GetComponent<ThirdPersonLocomotion>();
            locomotion.enabled = false;
            combat.transform.position += Vector3.right * 4f;
            Physics.SyncTransforms();
            normal.enabled = false;
            yield return WaitUntilOrFail(
                () => runnerReceiver.AttackWindowOpenInvocationCount >= 1,
                3f,
                "The Runner whiff window did not open.");
            yield return WaitUntilOrFail(
                () => runnerReceiver.ActionCompleteInvocationCount >= 1,
                3f,
                "The Runner whiff did not complete.");

            Assert.That(combat.Health.CurrentHealth, Is.EqualTo(startingHealth));
            AssertWhiffStayedSilent(audio, "Runner");
        }

        private static void AssertWhiffStayedSilent(
            ShowcaseUnarmedCombatAudioController audio,
            string attackerLabel)
        {
            Assert.That(audio.AcceptedContactCount, Is.Zero,
                $"{attackerLabel} whiff must not produce an accepted contact.");
            Assert.That(audio.ConfirmedImpactPlaybackCount, Is.Zero,
                $"{attackerLabel} whiff must not play an impact take.");
            Assert.That(audio.AuthoredVoices.All(voice => voice.clip == null), Is.True,
                $"{attackerLabel} whiff must not allocate an audible voice.");
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

        private IEnumerator ReloadScene()
        {
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync(loadedScene);
                if (unload != null)
                {
                    yield return unload;
                }
                loadedScene = default;
                yield return null;
            }
            yield return LoadScene();
        }

        private static T[] SceneComponents<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        }

        private static IEnumerator WaitUntilOrFail(
            Func<bool> predicate,
            float timeout,
            string message)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!predicate() && Time.realtimeSinceStartup < deadline)
            {
                yield return new WaitForSecondsRealtime(0.02f);
            }
            Assert.That(predicate(), Is.True, message);
        }

        private static void PlaceRunnerInFront(
            Transform player,
            ShowcaseRunnerAgent runner)
        {
            Vector3 position = player.position + player.forward * 0.89f;
            Rigidbody body = runner.GetComponent<Rigidbody>();
            body.position = position;
            runner.transform.position = position;
            Physics.SyncTransforms();
        }

        private static void PositionRunnerAtAttackStandoff(
            Transform player,
            ShowcaseRunnerAgent runner)
        {
            Vector3 position = player.position +
                player.forward * runner.AttackStandoffDistance;
            Quaternion rotation = Quaternion.LookRotation(
                player.position - position,
                Vector3.up);
            Rigidbody body = runner.GetComponent<Rigidbody>();
            body.position = position;
            body.rotation = rotation;
            runner.transform.SetPositionAndRotation(position, rotation);
            Physics.SyncTransforms();
        }

        private static void SeedKick(ShowcaseRunnerAgent runner, int slot)
        {
            SetPrivateIntField(runner, "punchesUntilKick", 0);
            SetPrivateUIntField(
                runner,
                "attackRandomState",
                FindRandomStateForSlot(slot, 3));
        }

        private static void SetPrivateIntField(
            ShowcaseRunnerAgent runner,
            string fieldName,
            int value)
        {
            FieldInfo field = typeof(ShowcaseRunnerAgent).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(runner, value);
        }

        private static void SetPrivateUIntField(
            ShowcaseRunnerAgent runner,
            string fieldName,
            uint value)
        {
            FieldInfo field = typeof(ShowcaseRunnerAgent).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(runner, value);
        }


        private static uint FindRandomStateForSlot(int slot, int modulus)
        {
            for (uint seed = 1; seed < 100000; seed++)
            {
                uint state = seed;
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                if (state % (uint)modulus == (uint)slot)
                {
                    return seed;
                }
            }
            Assert.Fail($"No deterministic selector seed found for slot {slot}/{modulus}.");
            return 1;
        }
    }
}
