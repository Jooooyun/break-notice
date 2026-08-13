using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BreakNotice.Gameplay.Interaction;
using NUnit.Framework;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestShowcaseS7ContractTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int index = createdObjects.Count - 1; index >= 0; index--)
            {
                UnityEngine.Object.DestroyImmediate(createdObjects[index]);
            }
            createdObjects.Clear();
        }

        [Test]
        public void DrawerSolutionContract_ContainsFourDistinctAuthoredSolutions()
        {
            ShowcaseDrawerSolution[] solutions = Enum
                .GetValues(typeof(ShowcaseDrawerSolution))
                .Cast<ShowcaseDrawerSolution>()
                .Where(solution => solution != ShowcaseDrawerSolution.None)
                .ToArray();

            Assert.That(solutions, Has.Length.EqualTo(4));
            Assert.That(solutions.Select(ShowcaseObjectiveController.GetSolutionLabel)
                .Distinct().Count(), Is.EqualTo(4));
        }

        [Test]
        public void DrawerImpact_RejectsSlowInputAndAppliesBoundedPhysicalVelocity()
        {
            ShowcaseObjectiveController objective = CreateObjective(null, out Rigidbody drawer);

            Assert.That(objective.TryRegisterDrawerImpact(
                ShowcaseDrawerSolution.DirectChairThrow,
                Vector3.right,
                objective.MinimumDrawerImpactSpeed - 0.01f), Is.False);
            Assert.That(objective.TryRegisterDrawerImpact(
                ShowcaseDrawerSolution.DirectChairThrow,
                Vector3.right,
                6f), Is.True);
            Assert.That(drawer.isKinematic, Is.False);
            Assert.That(drawer.linearVelocity.x, Is.GreaterThan(3.7f));
            Assert.That(drawer.linearVelocity.magnitude, Is.LessThanOrEqualTo(7.21f));
            Assert.That(objective.PhysicalSolutionsDiscovered, Is.EqualTo(1));
        }

        [Test]
        public void TeamFinisherHook_RequiresTwoReadinessBadges()
        {
            GameObject teamObject = CreateObject("Team");
            ShowcaseTeamContextController team =
                teamObject.AddComponent<ShowcaseTeamContextController>();
            ShowcaseObjectiveController objective = CreateObjective(team, out _);

            Assert.That(objective.NotifyTeamFinisherDrawerImpact(Vector3.right, 6f), Is.False);
            Assert.That(team.RecordPassSuccess(), Is.True);
            Assert.That(team.RecordBraceSuccess(), Is.True);
            Assert.That(team.IsTeamReady, Is.True);
            Assert.That(objective.NotifyTeamFinisherDrawerImpact(Vector3.right, 6f), Is.True);
        }

        [Test]
        public void S7Source_DefersS8SequenceAndContainsResultCardFields()
        {
            string objectiveSource = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Player/ShowcaseObjectiveController.cs");
            string presenterSource = File.ReadAllText(
                "Assets/_Project/Code/Runtime/Presentation/ShowcaseObjectivePresenter.cs");

            StringAssert.Contains("NotifyTeamFinisherDrawerImpact", objectiveSource);
            StringAssert.DoesNotContain("Cinemachine", objectiveSource);
            StringAssert.DoesNotContain("HANDOVER CRASH", objectiveSource);
            StringAssert.Contains("CLEAR TIME", presenterSource);
            StringAssert.Contains("DRAWER SOLUTION", presenterSource);
            StringAssert.Contains("PHYSICAL SOLUTIONS DISCOVERED", presenterSource);
            StringAssert.Contains("REPLAY — NEW PRESSURE VARIANT", presenterSource);
        }

        private ShowcaseObjectiveController CreateObjective(
            ShowcaseTeamContextController team,
            out Rigidbody drawerBody)
        {
            GameObject ownerObject = CreateObject("Owner");
            PrototypeInteractionStateOwner owner =
                ownerObject.AddComponent<PrototypeInteractionStateOwner>();
            GameObject drawerObject = CreateObject("Drawer");
            drawerObject.AddComponent<BoxCollider>();
            drawerBody = drawerObject.AddComponent<Rigidbody>();
            PrototypeInteractionTarget target =
                drawerObject.AddComponent<PrototypeInteractionTarget>();
            target.Configure(
                "OBJ-P-M-002",
                "DRAWER-TEST",
                "Drawer",
                PrototypeWeightClass.Medium,
                PrototypeInteractionRole.Drawer,
                false,
                false,
                false,
                false);
            GameObject parking = CreateObject("Parking");
            parking.transform.position = Vector3.right * 4f;
            GameObject exit = CreateObject("Exit");
            GameObject objectiveObject = CreateObject("Objective");
            ShowcaseObjectiveController objective =
                objectiveObject.AddComponent<ShowcaseObjectiveController>();
            objective.Configure(
                owner,
                null,
                ownerObject.transform,
                null,
                team,
                null,
                target,
                drawerBody,
                parking.transform,
                exit.transform,
                null);
            return objective;
        }

        private GameObject CreateObject(string name)
        {
            var created = new GameObject(name);
            createdObjects.Add(created);
            return created;
        }
    }
}
