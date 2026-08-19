using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ContestNonChr00AnimationDeliveryContractTests
    {
        private const string Chr01Root =
            "Assets/_Project/Art/Animation/CHR01";
        private const string Eny01NormalRoot =
            "Assets/_Project/Art/Animation/ENY01/Normal";
        private const string Eny01EliteRoot =
            "Assets/_Project/Art/Animation/ENY01/Elite";
        private const string ControllerRoot =
            "Assets/_Project/Art/Animation/DesignerAControllers";

        private static readonly string[] Chr01Basenames =
        {
            "CHR01_Getup_B",
            "CHR01_Getup_F",
            "CHR01_Idle",
            "CHR01_Idle_Variant_Stretch",
            "CHR01_Jump",
            "CHR01_Knockdown_B",
            "CHR01_Knockdown_F",
            "CHR01_Melee_Hit_Body",
            "CHR01_Run_F",
            "CHR01_Run_Stop",
            "CHR01_Strafe_L",
            "CHR01_Strafe_R",
            "CHR01_Supply_Binder_Release",
            "CHR01_Supply_Binder_Windup",
            "CHR01_Turn_180_Running",
            "CHR01_Turn_180_Walking",
            "CHR01_Turn_L_90",
            "CHR01_Turn_R_90",
            "CHR01_Walk_F",
            "CHR01_Walk_Stop"
        };

        private static readonly string[] EnemySuffixes =
        {
            "Hit_BigHeadHit",
            "Hit_Body",
            "Hit_Head",
            "Hit_Reaction",
            "Hit_Rib",
            "Hit_Stomach",
            "Idle",
            "Injured_Backwards_Turn_Left",
            "Injured_Backwards_Turn_Right",
            "Injured_Hurting_Idle",
            "Injured_Idle",
            "Injured_Run",
            "Injured_Run_Backwards",
            "Injured_Run_Backwards_Left_Turn",
            "Injured_Run_Backwards_Right_Turn",
            "Injured_Run_Jump",
            "Injured_Run_Left_Turn",
            "Injured_Run_Right_Turn",
            "Injured_Standing_Jump",
            "Injured_Stumble_Idle",
            "Injured_Turn_Left",
            "Injured_Turn_Right",
            "Injured_Walk",
            "Injured_Walk_Backwards",
            "Injured_Walk_Left_Turn",
            "Injured_Walk_Right_Turn",
            "Injured_Wave_Idle",
            "Jump",
            "Left_Strafe",
            "Left_Strafe_Walking",
            "Left_Turn_90",
            "Lod_Mutant_Punch",
            "Lod_Mutant_Swiping",
            "Lod_Mutant_Walking",
            "Lod_One_Hand_Club_Combo",
            "Melee_Candidate_Boxing",
            "Melee_Candidate_Boxing_2",
            "Melee_Candidate_Kicking",
            "Melee_Candidate_Kicking_2",
            "Right_Strafe",
            "Right_Strafe_Walking",
            "Right_Turn_90",
            "Run_Backwards",
            "Standard_Run",
            "Turn_Left",
            "Turning_Right_45_Degrees",
            "Walking",
            "Walking_Backwards"
        };

        private static readonly string[] CandidateBasenames =
        {
            "ENY01_Normal_Melee_Candidate_Boxing",
            "ENY01_Normal_Melee_Candidate_Boxing_2",
            "ENY01_Normal_Melee_Candidate_Kicking",
            "ENY01_Normal_Melee_Candidate_Kicking_2",
            "ENY01_Elite_Melee_Candidate_Boxing",
            "ENY01_Elite_Melee_Candidate_Boxing_2",
            "ENY01_Elite_Melee_Candidate_Kicking",
            "ENY01_Elite_Melee_Candidate_Kicking_2"
        };

        [Test]
        public void Delivery_HasExactPerCharacterInventoryAndEnemySymmetry()
        {
            string[] chr01 = DirectFbxPaths(Chr01Root);
            string[] normal = DirectFbxPaths(Eny01NormalRoot);
            string[] elite = DirectFbxPaths(Eny01EliteRoot);

            Assert.That(chr01, Has.Length.EqualTo(20));
            Assert.That(normal, Has.Length.EqualTo(48));
            Assert.That(elite, Has.Length.EqualTo(48));
            Assert.That(Basenames(chr01), Is.EquivalentTo(Chr01Basenames));
            Assert.That(
                Basenames(normal),
                Is.EquivalentTo(EnemySuffixes.Select(suffix =>
                    "ENY01_Normal_" + suffix)));
            Assert.That(
                Basenames(elite),
                Is.EquivalentTo(EnemySuffixes.Select(suffix =>
                    "ENY01_Elite_" + suffix)));

            Assert.That(
                EnemySuffixSet(normal, "ENY01_Normal_"),
                Is.EquivalentTo(EnemySuffixes));
            Assert.That(
                EnemySuffixSet(elite, "ENY01_Elite_"),
                Is.EquivalentTo(EnemySuffixes));
            Assert.That(
                EnemySuffixSet(normal, "ENY01_Normal_"),
                Is.EquivalentTo(EnemySuffixSet(elite, "ENY01_Elite_")));
        }

        [Test]
        public void Delivery_UsesIndependentHumanoidAvatarImportContract()
        {
            string[] paths = AllDeliveryPaths();
            Assert.That(paths, Has.Length.EqualTo(116));

            foreach (string path in paths)
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.importAnimation, Is.True, path);
                Assert.That(
                    importer.animationType,
                    Is.EqualTo(ModelImporterAnimationType.Human),
                    path);
                Assert.That(
                    importer.avatarSetup,
                    Is.EqualTo(ModelImporterAvatarSetup.CreateFromThisModel),
                    path);
                Assert.That(importer.sourceAvatar, Is.Null, path);
                Assert.That(
                    importer.materialImportMode,
                    Is.EqualTo(ModelImporterMaterialImportMode.None),
                    path);
                Assert.That(importer.importCameras, Is.False, path);
                Assert.That(importer.importLights, Is.False, path);

                Avatar[] avatars = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<Avatar>()
                    .ToArray();
                Assert.That(avatars, Has.Length.EqualTo(1), path);
                Assert.That(avatars[0].isValid, Is.True, path);
                Assert.That(avatars[0].isHuman, Is.True, path);

                AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(path)
                    .OfType<AnimationClip>()
                    .Where(clip => !clip.name.StartsWith(
                        "__preview__",
                        StringComparison.Ordinal))
                    .ToArray();
                Assert.That(clips, Is.Not.Empty, path);
                bool oneFrameReadyPose = string.Equals(
                    Path.GetFileNameWithoutExtension(path),
                    "CHR01_Supply_Binder_Windup",
                    StringComparison.Ordinal);
                Assert.That(
                    clips.All(clip => clip.isHumanMotion) || oneFrameReadyPose,
                    Is.True,
                    path);
            }
        }

        [Test]
        public void Delivery_ContainsExactlyEightExplicitMeleeCandidates()
        {
            string[] paths = AllDeliveryPaths();
            string[] candidates = Basenames(paths)
                .Where(name => name.Contains(
                    "_Melee_Candidate_",
                    StringComparison.Ordinal))
                .ToArray();

            Assert.That(candidates, Has.Length.EqualTo(8));
            Assert.That(candidates, Is.EquivalentTo(CandidateBasenames));
            Assert.That(paths.All(path => !path.Contains(
                "/CHR00/",
                StringComparison.Ordinal)), Is.True);
            Assert.That(Basenames(paths).All(name => !name.StartsWith(
                "CHR00_",
                StringComparison.Ordinal)), Is.True);
        }

        [Test]
        public void RuntimeControllers_KeepEliteTechniquesExclusiveAndExcludePropMotion()
        {
            string[] forbiddenSuffixes =
            {
                "_Walking.fbx", "_Walking_Backwards.fbx", "_Run_Backwards.fbx",
                "_Turn_Left.fbx", "_Turning_Right_45_Degrees.fbx",
                "_Hit_Reaction.fbx", "_Lod_Mutant_Walking.fbx",
                "_Lod_One_Hand_Club_Combo.fbx"
            };

            foreach (string variant in new[] { "Normal", "Elite" })
            {
                string path = ControllerRoot + "/ENY01_" + variant + ".controller";
                AnimatorController controller = AssetDatabase.LoadAssetAtPath<
                    AnimatorController>(path);
                Assert.That(controller, Is.Not.Null, path);
                string[] motionPaths = controller.layers
                    .SelectMany(layer => layer.stateMachine.states)
                    .Select(child => AssetDatabase.GetAssetPath(child.state.motion))
                    .Where(value => !string.IsNullOrEmpty(value))
                    .ToArray();

                Assert.That(motionPaths.Any(value => forbiddenSuffixes.Any(
                    suffix => value.EndsWith(suffix, StringComparison.Ordinal))),
                    Is.False,
                    variant);
                Assert.That(motionPaths.Any(value => value.Contains(
                    "/Legacy/Dying",
                    StringComparison.Ordinal)), Is.False, variant);
                Assert.That(motionPaths.Any(value => value.EndsWith(
                    "_Standard_Run.fbx",
                    StringComparison.Ordinal)), Is.True, variant);
                Assert.That(motionPaths.Any(value => value.EndsWith(
                    "_Hit_Head.fbx",
                    StringComparison.Ordinal)), Is.True, variant);
                Assert.That(motionPaths.Any(value => value.EndsWith(
                    "_Lod_Mutant_Punch.fbx",
                    StringComparison.Ordinal)), Is.EqualTo(variant == "Elite"), variant);
                Assert.That(motionPaths.Any(value => value.EndsWith(
                    "_Lod_Mutant_Swiping.fbx",
                    StringComparison.Ordinal)), Is.EqualTo(variant == "Elite"), variant);
            }
        }

        private static string[] AllDeliveryPaths()
        {
            return DirectFbxPaths(Chr01Root)
                .Concat(DirectFbxPaths(Eny01NormalRoot))
                .Concat(DirectFbxPaths(Eny01EliteRoot))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }

        private static string[] DirectFbxPaths(string root)
        {
            return AssetDatabase.FindAssets("t:Model", new[] { root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(
                    ".fbx",
                    StringComparison.OrdinalIgnoreCase))
                .Where(path => Normalize(Path.GetDirectoryName(path)) == root)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
        }

        private static string[] Basenames(IEnumerable<string> paths)
        {
            return paths
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        }

        private static string[] EnemySuffixSet(
            IEnumerable<string> paths,
            string prefix)
        {
            return Basenames(paths)
                .Select(name =>
                {
                    Assert.That(name, Does.StartWith(prefix));
                    return name.Substring(prefix.Length);
                })
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        }

        private static string Normalize(string path)
        {
            return (path ?? string.Empty).Replace('\\', '/');
        }
    }
}
