using System.Collections;
using System.Linq;
using BreakNotice.Gameplay.Encounter;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseS5PlayModeTests
    {
        private const string S4ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v004.unity";
        private const string S5ScenePath =
            "Assets/_Project/Scenes/20_Showcase/Contest_OperationsFloor_Showcase_v005.unity";

        [UnityTearDown]
        public IEnumerator TearDownLoadedShowcaseScene()
        {
            Scene cleanup = SceneManager.CreateScene("S5_TEST_CLEANUP");
            SceneManager.SetActiveScene(cleanup);
            for (int index = SceneManager.sceneCount - 1; index >= 0; index--)
            {
                Scene loaded = SceneManager.GetSceneAt(index);
                if (loaded == cleanup || !loaded.isLoaded)
                {
                    continue;
                }

                AsyncOperation unload = SceneManager.UnloadSceneAsync(loaded);
                if (unload != null)
                {
                    yield return unload;
                }
            }
        }

        [UnityTest]
        public IEnumerator S4RemainsPreservedAndS5HasSingularStrapComposition()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S4ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            Assert.That(SceneComponents<ShowcaseCargoStrapController>(), Is.Empty);

            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S5ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>().Single();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>().Single();

            Assert.That(interaction.ShowcaseCargoStrapController, Is.SameAs(cargo));
            Assert.That(team.CargoStrapController, Is.SameAs(cargo));
            Assert.That(SceneComponents<LineRenderer>().Count(line =>
                line.name == "GEAR01_SEGMENTED_STRAP_GRAYBOX"), Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator AssistAttachAwardsBraceAndExplicitReleaseRestoresPhysicalState()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S5ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            PrototypeInteractionController interaction =
                SceneComponents<PrototypeInteractionController>().Single();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            ShowcaseMobileAllyController mobile =
                SceneComponents<ShowcaseMobileAllyController>().Single();
            PrototypeInteractionTarget chair = SceneComponents<PrototypeInteractionTarget>()
                .First(target => target.Role == PrototypeInteractionRole.DirectChair);
            Rigidbody body = chair.GetComponent<Rigidbody>();
            Collider chairCollider = chair.GetComponent<Collider>();
            Collider playerCollider = owner.GetComponent<Collider>();
            RigidbodyConstraints originalConstraints = body.constraints;
            CollisionDetectionMode originalCollision = body.collisionDetectionMode;
            RigidbodyInterpolation originalInterpolation = body.interpolation;

            owner.transform.position = chair.transform.position + new Vector3(0f, 0.9f, -2f);
            mobile.transform.position = chair.transform.position + new Vector3(1.5f, 0f, 0f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(cargo.CanAssistAttach(), Is.True);
            Assert.That(team.ResolveCurrentQContext(), Is.EqualTo(ShowcaseQContext.AssistAttach));
            Assert.That(interaction.ProcessTeamContextQ(), Is.True);
            Assert.That(owner.StrapTarget, Is.SameAs(chair));
            Assert.That(cargo.ActiveTarget, Is.SameAs(chair));
            Assert.That(team.HasBrace, Is.True);
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.True);

            Assert.That(interaction.ProcessStrapRelease(), Is.True);
            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(cargo.ActiveTarget, Is.Null);
            Assert.That(body.constraints, Is.EqualTo(originalConstraints));
            Assert.That(body.collisionDetectionMode, Is.EqualTo(originalCollision));
            Assert.That(body.interpolation, Is.EqualTo(originalInterpolation));
            Assert.That(Physics.GetIgnoreCollision(playerCollider, chairCollider), Is.False);
        }

        [UnityTest]
        public IEnumerator AuthoredResetClearsStrapAndBraceReadiness()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                S5ScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
            ShowcaseCargoStrapController cargo =
                SceneComponents<ShowcaseCargoStrapController>().Single();
            ShowcaseTeamContextController team =
                SceneComponents<ShowcaseTeamContextController>().Single();
            PrototypeInteractionStateOwner owner =
                SceneComponents<PrototypeInteractionStateOwner>().Single();
            AuthoredEncounterResetController reset =
                SceneComponents<AuthoredEncounterResetController>().Single();
            PrototypeInteractionTarget chair = SceneComponents<PrototypeInteractionTarget>()
                .First(target => target.Role == PrototypeInteractionRole.DirectChair);

            owner.transform.position = chair.transform.position + new Vector3(0f, 0.9f, -2f);
            SceneComponents<ShowcaseMobileAllyController>().Single().transform.position =
                chair.transform.position + new Vector3(1.5f, 0f, 0f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();
            Assert.That(team.TryExecuteAssistAttach(), Is.True);
            Assert.That(team.HasBrace, Is.True);

            Assert.That(reset.RequestReset(EncounterResetCause.ManualInput), Is.True);
            yield return null;

            Assert.That(owner.StrapState, Is.EqualTo(PrototypeStrapOwnershipState.Stored));
            Assert.That(cargo.ActiveTarget, Is.Null);
            Assert.That(team.Badges, Is.EqualTo(ShowcaseTeamReadinessBadge.None));
        }

        private static T[] SceneComponents<T>() where T : Component
        {
            Scene scene = SceneManager.GetActiveScene();
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<T>(true))
                .ToArray();
        }
    }
}
