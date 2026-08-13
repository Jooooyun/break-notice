using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BreakNotice.LocomotionCamera.Tests
{
    public sealed class ShowcaseVfxImportContractTests
    {
        private const string ReviewRootPath =
            "Assets/_Project/Art/VFX/Showcase";

        private const string PremiumRootPath =
            ReviewRootPath + "/Premium_v002";

        private const string HitRootPath =
            ReviewRootPath + "/HitProduction_v005";

        private const string ReviewScenePath =
            "Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseVFX.unity";

        private const string ParticleShaderName =
            "Particles/Standard Unlit";

        private static readonly Candidate[] Candidates =
        {
            C("EdgeTrace", "Textures/Premium_v002/T_VFX_Showcase_InteractionTarget_EdgeTrace_v002.png", "df111ad53afd19b4c969d602a55af34a2d7c9fd7714d3e298c928ab47f6681cf", 12534, false, 0, 0),
            C("ContactBracket", "Textures/Premium_v002/T_VFX_Showcase_InteractionTarget_ContactBracket_v002.png", "8c93d16ef05caecfe520833815b82fa31daff2a4dcffc0e712504f7e6eeb3305", 2225, false, 0, 0),
            C("MaterialWink", "Textures/Premium_v002/T_VFX_Showcase_InteractionTarget_MaterialWink_v002.png", "1d12623fb2704e174d3a698c7952d5296c485a72d7e9b2ddf9b530a6d0c5632d", 4409, false, 0, 0),
            C("ReticleInterruption", "Textures/Premium_v002/T_VFX_Showcase_InvalidRejected_ReticleInterruption_v002.png", "e88d16b245093a07981373c024e1eb9cad49f25c794cff0cc1a9d331a386d67e", 1948, false, 0, 0),
            C("TargetDropout", "Textures/Premium_v002/T_VFX_Showcase_InvalidRejected_TargetDropout_v002.png", "102616fd9b7005ea26525c09ed2dbc64188838641de4c1b82a0abae8ecb2b497", 8356, false, 0, 0),
            C("ReverseEcho", "Textures/Premium_v002/T_VFX_Showcase_InvalidRejected_ReverseEcho_v002.png", "c2703f43cddb5c837e9cc28c98f46d6cf54c79139d3e532293bb98cdab3dadf6", 4134, false, 0, 0),
            C("GazeTick", "Textures/Premium_v002/T_VFX_Showcase_AllyReady_GazeTick_v002.png", "fb81a61753af29484b4b97f2179f2354655d306325be03c1b06f59979fddb195", 4236, false, 0, 0),
            C("PostureCue", "Textures/Premium_v002/T_VFX_Showcase_AllyReady_PostureCue_v002.png", "5db1ba0ba62570b4607a3635c7f61d88ad1e71434ef6a4516a82db3180c65472", 6905, false, 0, 0),
            C("FootContact", "Textures/Premium_v002/T_VFX_Showcase_AllyReady_FootContact_v002.png", "3a264c5002377de8df1818352cfffe1a035faa5c55b7e076032f97949486dba3", 4574, false, 0, 0),
            C("PracticalLamp", "Textures/Premium_v002/T_VFX_Showcase_ExitRoute_PracticalLamp_v002.png", "8e708912bab299391f19d84c04f9aa34a8533c064febb9b0bb45e7af3b272fc5", 3388, false, 0, 0),
            C("SignHardware", "Textures/Premium_v002/T_VFX_Showcase_ExitRoute_SignHardware_v002.png", "aded013427a768ee976ec98d58f583b2d78a6c64a279b90f8df0b5e2487ada4b", 3381, false, 0, 0),
            C("RouteEdge", "Textures/Premium_v002/T_VFX_Showcase_ExitRoute_RouteEdge_v002.png", "4b181f45e5fe97363b88fdb4b56b42689551638046ec22ee1ded186b16d7cb15", 2487, false, 0, 0),
            C("PrimaryCompression", "Flipbooks/Premium_v002/T_VFX_Showcase_CatchConfirm_PrimaryCompression_4x4_v002.png", "c23113c23901186530947dbbfd0a7441f1df17e2f468a7c050335816dc79b36b", 76643, true, 1030, 1364),
            C("PaperSnap", "Flipbooks/Premium_v002/T_VFX_Showcase_CatchConfirm_PaperSnap_4x4_v002.png", "43cc158b384896129206790b9b89cea2b81053f29bed6a0d1050325209d8a692", 162694, true, 888, 3555),
            C("DirectionalSettle", "Flipbooks/Premium_v002/T_VFX_Showcase_CatchConfirm_DirectionalSettle_4x4_v002.png", "c4f38878e2564b978fb54520cead9f9665ba0c8fedd640904c9d33c7179a8c92", 95666, true, 672, 873),
            C("PrimaryTransient", "Flipbooks/Premium_v002/T_VFX_Showcase_BinderImpact_PrimaryTransient_4x4_v002.png", "a68394590156dd33106575ef75ca11a2538f3a592ec17f5e30297cd0c32f4736", 100773, true, 866, 1447),
            C("DirectionalDust", "Flipbooks/Premium_v002/T_VFX_Showcase_BinderImpact_DirectionalDust_4x4_v002.png", "e907e62aa992c6c85e5296014b872c0dcd11b2a99d02f56eec6bea442396655f", 350863, true, 1032, 3764),
            C("FiberSettle", "Flipbooks/Premium_v002/T_VFX_Showcase_BinderImpact_FiberSettle_4x4_v002.png", "977c863ebef821c9a6207b6f83b2c513fd2e84aab3173633f2722ceec9623cee", 139004, true, 1883, 2446),
            C("CasterBite", "Flipbooks/Premium_v002/T_VFX_Showcase_ChairSkid_CasterBite_4x4_v002.png", "e6810154751610466f9503cf451d330cc268f4a61bef0187712ea3e21808776c", 87962, true, 2627, 1879),
            C("RubberBody", "Flipbooks/Premium_v002/T_VFX_Showcase_ChairSkid_RubberBody_4x4_v002.png", "d5dac3dc71422cc8235f0e8e7a46473faa396db50e866a2011503706f0877c34", 320458, true, 2549, 6075),
            C("ContactSettle", "Flipbooks/Premium_v002/T_VFX_Showcase_ChairSkid_ContactSettle_4x4_v002.png", "05f8ed93d906694d718db95a706559d1f8f18db94477d4beb22f29f8450d81c6", 216785, true, 1684, 2894),
            C("PrimarySnap", "Flipbooks/Premium_v002/T_VFX_Showcase_PaperBurst_PrimarySnap_4x4_v002.png", "ffd7141d55894e7c6b0b204c68a75027ff7762131d9c1b18d57654723d8e7188", 102670, true, 1391, 1366),
            C("PageFan", "Flipbooks/Premium_v002/T_VFX_Showcase_PaperBurst_PageFan_4x4_v002.png", "1f0549a0ab3eea22e215730c40c497564ef58d2076678c5f30d7ea1cc88d5f02", 470061, true, 9099, 14298),
            C("FlutterSettle", "Flipbooks/Premium_v002/T_VFX_Showcase_PaperBurst_FlutterSettle_4x4_v002.png", "88d64f8c75879b2186dbea2b0ad9890d381f4625fe0aa3f211ad58db5181b59b", 107177, true, 1295, 1242)
        };

        private static readonly PrefabContract[] Prefabs =
        {
            P("InteractionTarget", false),
            P("InvalidRejected", false),
            P("AllyReady", false),
            P("ExitRoute", false),
            P("CatchConfirm", true, "A"), P("CatchConfirm", true, "B"), P("CatchConfirm", true, "C"),
            P("BinderImpact", true, "A"), P("BinderImpact", true, "B"), P("BinderImpact", true, "C"),
            P("ChairSkid", true, "A"), P("ChairSkid", true, "B"), P("ChairSkid", true, "C"),
            P("PaperBurst", true, "A"), P("PaperBurst", true, "B"), P("PaperBurst", true, "C")
        };

        private static readonly string[] HitIntensities = { "Light", "Medium", "Heavy" };
        private static readonly string[] HitVariations = { "A", "B", "C" };
        private static readonly string[] HitLayers = { "Contact", "DirectionalBody", "HitStopEcho", "MaterialSettle" };

        private static IEnumerable CandidateCases => Candidates
            .Select(candidate => new TestCaseData(candidate)
                .SetName("PremiumBytes_" + candidate.Name));

        private static IEnumerable ImporterCases => Candidates
            .Select(candidate => new TestCaseData(candidate)
                .SetName("PremiumImporter_" + candidate.Name));

        private static IEnumerable MaterialCases => Candidates
            .Select(candidate => new TestCaseData(candidate)
                .SetName("PremiumMaterial_" + candidate.Name));

        private static IEnumerable FlipbookCases => Candidates
            .Where(candidate => candidate.IsFlipbook)
            .Select(candidate => new TestCaseData(candidate)
                .SetName("PremiumFlipbook_" + candidate.Name));

        private static IEnumerable PrefabCases => Prefabs
            .Select(prefab => new TestCaseData(prefab)
                .SetName("PremiumPrefab_" + prefab.Name + "_" + prefab.Variation));

        [TestCaseSource(nameof(CandidateCases))]
        public void PremiumCandidateBytes_AreExactRgbaPngsWithSafeAlpha(Candidate candidate)
        {
            string physicalPath = ToPhysicalPath(candidate.TexturePath);
            Assert.That(File.Exists(physicalPath), Is.True, candidate.TexturePath);
            byte[] bytes = File.ReadAllBytes(physicalPath);
            Assert.That(bytes.Length, Is.EqualTo(candidate.ExpectedBytes));
            Assert.That(
                bytes.Take(8).ToArray(),
                Is.EqualTo(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }));
            Assert.That(Sha256(bytes), Is.EqualTo(candidate.ExpectedSha256));

            Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            try
            {
                Assert.That(ImageConversion.LoadImage(decoded, bytes, false), Is.True);
                int dimension = candidate.IsFlipbook ? 2048 : 512;
                Assert.That(decoded.width, Is.EqualTo(dimension));
                Assert.That(decoded.height, Is.EqualTo(dimension));
                Color32[] pixels = decoded.GetPixels32();
                Assert.That(pixels.Count(pixel => pixel.a > 0), Is.GreaterThan(0));
                Assert.That(pixels.Count(pixel => pixel.a > 0 && pixel.a < 255), Is.GreaterThan(0));
                Assert.That(pixels.Count(IsMissingTextureMagenta), Is.Zero);
                Assert.That(pixels.Count(IsNeonCyan), Is.Zero);
                Assert.That(pixels.Count(IsGreenSpill), Is.Zero);
                Assert.That(
                    pixels.Count(pixel => pixel.a > 8 && pixel.r < 8 && pixel.g < 8 && pixel.b < 8),
                    Is.Zero,
                    "Visible-alpha pure black edge pixels would risk a black halo.");
                if (!candidate.IsFlipbook)
                {
                    AssertTransparentOuterMargin(pixels, decoded.width, decoded.height, 28);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(decoded);
            }
        }

        [TestCaseSource(nameof(ImporterCases))]
        public void PremiumImporters_PreserveStraightAlphaAndBoundReviewCost(Candidate candidate)
        {
            TextureImporter importer = AssetImporter.GetAtPath(candidate.TexturePath) as TextureImporter;
            Assert.That(importer, Is.Not.Null, candidate.TexturePath);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Default));
            Assert.That(importer.textureShape, Is.EqualTo(TextureImporterShape.Texture2D));
            Assert.That(importer.sRGBTexture, Is.True);
            Assert.That(importer.alphaSource, Is.EqualTo(TextureImporterAlphaSource.FromInput));
            Assert.That(importer.alphaIsTransparency, Is.True);
            Assert.That(importer.isReadable, Is.False);
            Assert.That(importer.mipmapEnabled, Is.EqualTo(candidate.IsFlipbook));
            Assert.That(importer.borderMipmap, Is.EqualTo(candidate.IsFlipbook));
            Assert.That(importer.streamingMipmaps, Is.False);
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.anisoLevel, Is.EqualTo(1));
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.crunchedCompression, Is.False);
            Assert.That(importer.maxTextureSize, Is.EqualTo(candidate.IsFlipbook ? 2048 : 512));
        }

        [TestCaseSource(nameof(MaterialCases))]
        public void PremiumMaterials_UseBuiltInParticleUnlitAndStraightAlpha(Candidate candidate)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(candidate.MaterialPath);
            Assert.That(material, Is.Not.Null, candidate.MaterialPath);
            Assert.That(material.shader, Is.Not.Null);
            Assert.That(material.shader.name, Is.EqualTo(ParticleShaderName));
            Assert.That(material.HasProperty("_MainTex"), Is.True);
            Assert.That(
                AssetDatabase.GetAssetPath(material.GetTexture("_MainTex")),
                Is.EqualTo(candidate.TexturePath));
            Assert.That(material.GetFloat("_Mode"), Is.EqualTo(2f).Within(0.001f));
            Assert.That(material.GetFloat("_SrcBlend"),
                Is.EqualTo((float)BlendMode.SrcAlpha).Within(0.001f));
            Assert.That(material.GetFloat("_DstBlend"),
                Is.EqualTo((float)BlendMode.OneMinusSrcAlpha).Within(0.001f));
            Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(0f).Within(0.001f));
            Assert.That(material.GetTag("RenderType", false), Is.EqualTo("Transparent"));
            Assert.That(material.renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
            Assert.That(material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON"), Is.False);
            Assert.That(material.GetShaderPassEnabled("ShadowCaster"), Is.False);
        }

        [TestCaseSource(nameof(FlipbookCases))]
        public void PremiumFlipbooks_HaveUnityOrderAndSixteenUniqueSafeCells(Candidate candidate)
        {
            byte[] bytes = File.ReadAllBytes(ToPhysicalPath(candidate.TexturePath));
            Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            try
            {
                Assert.That(ImageConversion.LoadImage(decoded, bytes, false), Is.True);
                Color32[] pixels = decoded.GetPixels32();
                var hashes = new HashSet<string>(StringComparer.Ordinal);
                var alphaCounts = new List<int>();
                const int cellSize = 512;
                const int margin = 28;
                for (int frame = 0; frame < 16; frame++)
                {
                    int cellX = frame % 4;
                    int cellY = frame / 4;
                    byte[] cellBytes = new byte[cellSize * cellSize * 4];
                    int write = 0;
                    int alphaCount = 0;
                    for (int localY = 0; localY < cellSize; localY++)
                    {
                        for (int localX = 0; localX < cellSize; localX++)
                        {
                            Color32 pixel = pixels[
                                ((cellY * cellSize) + localY) * decoded.width +
                                (cellX * cellSize) + localX];
                            if (pixel.a > 0)
                            {
                                alphaCount++;
                            }
                            if (localX < margin || localX >= cellSize - margin ||
                                localY < margin || localY >= cellSize - margin)
                            {
                                Assert.That(pixel.a, Is.Zero,
                                    $"{candidate.Name} Unity frame {frame} cell margin");
                            }
                            cellBytes[write++] = pixel.r;
                            cellBytes[write++] = pixel.g;
                            cellBytes[write++] = pixel.b;
                            cellBytes[write++] = pixel.a;
                        }
                    }
                    Assert.That(alphaCount, Is.GreaterThan(31));
                    alphaCounts.Add(alphaCount);
                    hashes.Add(Sha256(cellBytes));
                }
                Assert.That(hashes.Count, Is.EqualTo(16));
                Assert.That(alphaCounts[0], Is.EqualTo(candidate.ExpectedFrameZeroAlpha));
                Assert.That(alphaCounts[15], Is.EqualTo(candidate.ExpectedFrameFifteenAlpha));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(decoded);
            }
        }

        [TestCaseSource(nameof(PrefabCases))]
        public void PremiumPrefabs_AreThreeLayerReviewOnlyComposites(PrefabContract contract)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(contract.Path);
            Assert.That(prefab, Is.Not.Null, contract.Path);
            Assert.That(prefab.transform.childCount, Is.EqualTo(3));
            ParticleSystem[] systems = prefab.GetComponentsInChildren<ParticleSystem>(true);
            Assert.That(systems.Length, Is.EqualTo(3));
            Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(systems.All(system => system.main.loop), Is.True);
            Assert.That(systems.All(system => system.main.playOnAwake), Is.True);
            Assert.That(systems.All(system => !system.main.prewarm), Is.True);
            Assert.That(
                systems.All(system =>
                    system.main.cullingMode == ParticleSystemCullingMode.AlwaysSimulate),
                Is.True);
            Assert.That(systems.All(system => system.main.maxParticles == 1), Is.True);
            Assert.That(systems.All(system => system.main.startSpeed.constant == 0f), Is.True);
            Assert.That(systems.All(system => system.emission.burstCount == 1), Is.True);
            Assert.That(systems.All(system => !system.shape.enabled), Is.True);
            Assert.That(systems.All(system => !system.collision.enabled), Is.True);
            Assert.That(systems.All(system => !system.trigger.enabled), Is.True);
            Assert.That(systems.All(system => !system.noise.enabled), Is.True);
            Assert.That(systems.All(system => !system.trails.enabled), Is.True);
            Assert.That(
                systems.All(system => system.textureSheetAnimation.enabled == contract.IsPhysical),
                Is.True);

            foreach (ParticleSystem system in systems)
            {
                ParticleSystemRenderer renderer = system.GetComponent<ParticleSystemRenderer>();
                Assert.That(renderer, Is.Not.Null);
                Assert.That(renderer.renderMode, Is.EqualTo(ParticleSystemRenderMode.Billboard));
                Assert.That(renderer.alignment, Is.EqualTo(ParticleSystemRenderSpace.View));
                Assert.That(renderer.shadowCastingMode, Is.EqualTo(ShadowCastingMode.Off));
                Assert.That(renderer.receiveShadows, Is.False);
                Assert.That(
                    AssetDatabase.GetAssetPath(renderer.sharedMaterial),
                    Does.StartWith(PremiumRootPath + "/Materials/"));
                if (contract.IsPhysical)
                {
                    ParticleSystem.TextureSheetAnimationModule sheet = system.textureSheetAnimation;
                    Assert.That(sheet.numTilesX, Is.EqualTo(4));
                    Assert.That(sheet.numTilesY, Is.EqualTo(4));
                    Assert.That(sheet.animation, Is.EqualTo(ParticleSystemAnimationType.WholeSheet));
                    Assert.That(sheet.cycleCount, Is.EqualTo(1));
                    Assert.That(sheet.startFrame.constant, Is.EqualTo(0f).Within(0.001f));
                    Assert.That(sheet.frameOverTime.curve.Evaluate(0f), Is.EqualTo(0f).Within(0.001f));
                    Assert.That(sheet.frameOverTime.curve.Evaluate(1f), Is.EqualTo(0.999f).Within(0.002f));
                }
            }
        }

        [Test]
        public void PhysicalPrefabVariations_HaveDistinctDeterministicCompositions()
        {
            foreach (string effect in new[] { "CatchConfirm", "BinderImpact", "ChairSkid", "PaperBurst" })
            {
                string[] signatures = new[] { "A", "B", "C" }
                    .Select(variation =>
                    {
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                            PremiumRootPath + "/Prefabs/PF_VFX_Showcase_" + effect +
                            "_PREMIUM_REVIEW_ONLY_VAR_" + variation + "_v002.prefab");
                        Assert.That(prefab, Is.Not.Null);
                        return string.Join("|", prefab.GetComponentsInChildren<ParticleSystem>(true)
                            .Select(system =>
                            {
                                Transform transform = system.transform;
                                ParticleSystem.Burst burst = system.emission.GetBurst(0);
                                return $"{transform.localPosition:F3};{transform.localEulerAngles.z:F3};" +
                                       $"{transform.localScale.x:F3};{burst.time:F3}";
                            }));
                    })
                    .ToArray();
                Assert.That(signatures.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(3), effect);
            }
        }

        [Test]
        public void ReviewScene_ContainsAllPremiumReviewZonesAndNoGameplayHook()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ReviewScenePath), Is.Not.Null);
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = scene.GetRootGameObjects()
                    .SingleOrDefault(item => item.name == "LOCAL_REVIEW_ONLY_ShowcaseVFX_ROOT");
                Assert.That(root, Is.Not.Null);
                Transform oldNew = root.transform.Find("ZONE_01_OLD_VS_NEW");
                Transform layers = root.transform.Find("ZONE_02_SOLO_LAYER_AND_COMPOSITE");
                Transform variations = root.transform.Find("ZONE_03_PHYSICAL_VARIATION_AUDITION");
                Transform gameplay = root.transform.Find("ZONE_04_GAMEPLAY_CAMERA_EQUIVALENT");
                Transform hits = root.transform.Find("ZONE_05_HIT_PRODUCTION_v005");
                Assert.That(oldNew, Is.Not.Null);
                Assert.That(layers, Is.Not.Null);
                Assert.That(variations, Is.Not.Null);
                Assert.That(gameplay, Is.Not.Null);
                Assert.That(hits, Is.Not.Null);

                Transform[] stations = oldNew.Cast<Transform>()
                    .Where(child => child.name.StartsWith("STATION_", StringComparison.Ordinal))
                    .ToArray();
                Assert.That(stations.Length, Is.EqualTo(8));
                Assert.That(stations.All(station => station.Find("OLD_V001") != null), Is.True);
                Assert.That(stations.All(station => station.Find("NEW_PREMIUM_V002_COMPOSITE") != null), Is.True);
                Assert.That(stations.All(station => station.Find("LABEL") != null), Is.True);
                Assert.That(stations.All(station => station.Find("REVIEW_BACKDROP") != null), Is.True);

                Assert.That(layers.Cast<Transform>().Count(child => child.name.StartsWith("LAYER_STATION_", StringComparison.Ordinal)), Is.EqualTo(8));
                Assert.That(variations.Cast<Transform>().Count(child => child.name.StartsWith("VARIATION_STATION_", StringComparison.Ordinal)), Is.EqualTo(4));
                Assert.That(gameplay.GetComponentsInChildren<ParticleSystem>(true).Length, Is.EqualTo(24));
                Assert.That(hits.GetComponentsInChildren<ParticleSystem>(true).Length, Is.EqualTo(84));
                Assert.That(hits.Cast<Transform>().Count(child => child.name.StartsWith("HIT_STATION_", StringComparison.Ordinal)), Is.EqualTo(3));
                Assert.That(root.GetComponentsInChildren<ParticleSystem>(true).Length, Is.EqualTo(272));
                Assert.That(root.GetComponentsInChildren<ParticleSystem>(false).Length, Is.EqualTo(188));
                MonoBehaviour[] reviewComponents =
                    root.GetComponentsInChildren<MonoBehaviour>(true);
                Assert.That(reviewComponents.Length, Is.EqualTo(1));
                Assert.That(
                    reviewComponents[0].GetType().FullName,
                    Is.EqualTo("BreakNotice.ContestReview.ShowcaseVfxReviewPlayback"));
                Assert.That(root.GetComponentsInChildren<Camera>(true).Length, Is.EqualTo(1));
                Assert.That(root.GetComponentsInChildren<AudioListener>(true), Is.Empty);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            Assert.That(EditorBuildSettings.scenes.Any(item =>
                string.Equals(item.path, ReviewScenePath, StringComparison.OrdinalIgnoreCase)), Is.False);
            string[] dependencies = AssetDatabase.GetDependencies(ReviewScenePath, true);
            string[] runtimeDependencies = dependencies
                .Where(path => path.Contains("/Code/Runtime/"))
                .ToArray();
            Assert.That(
                runtimeDependencies,
                Is.EquivalentTo(new[]
                {
                    "Assets/_Project/Code/Runtime/ContestReview/ShowcaseVfxReviewPlayback.cs"
                }));
            Assert.That(dependencies.Any(path => path.StartsWith("Assets/_Project/Prefabs/", StringComparison.OrdinalIgnoreCase)), Is.False);
            Assert.That(dependencies.Any(path => path.StartsWith("Assets/_Project/Scenes/10_Prototype/", StringComparison.OrdinalIgnoreCase)), Is.False);
            Assert.That(AssetDatabase.FindAssets("t:Shader", new[] { ReviewRootPath }), Is.Empty);
        }

        [Test]
        public void ReviewScene_UsesPositiveHandedFrontFacingTransformsAndUprightText()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = FindReviewRoot(scene);
                Assert.That(root.transform.position, Is.EqualTo(Vector3.zero));
                Assert.That(root.transform.rotation, Is.EqualTo(Quaternion.identity));
                Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));

                foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
                {
                    Assert.That(transform.localScale.x, Is.GreaterThan(0f), transform.name);
                    Assert.That(transform.localScale.y, Is.GreaterThan(0f), transform.name);
                    Assert.That(transform.localScale.z, Is.GreaterThan(0f), transform.name);
                    Assert.That(
                        transform.localToWorldMatrix.determinant,
                        Is.GreaterThan(0f),
                        transform.name);
                }

                Camera camera = root.GetComponentsInChildren<Camera>(true).Single();
                Assert.That(camera.transform.position, Is.EqualTo(new Vector3(400f, 0.65f, -21.5f)));
                Assert.That(Vector3.Dot(camera.transform.forward, Vector3.forward), Is.GreaterThan(0.999f));
                Assert.That(Vector3.Dot(camera.transform.up, Vector3.up), Is.GreaterThan(0.999f));
                Assert.That(Vector3.Dot(camera.transform.right, Vector3.right), Is.GreaterThan(0.999f));
                Assert.That(camera.fieldOfView, Is.EqualTo(40f).Within(0.001f));
                Assert.That(camera.nearClipPlane, Is.EqualTo(0.1f).Within(0.001f));
                Assert.That(camera.farClipPlane, Is.EqualTo(80f).Within(0.001f));

                TextMesh[] labels = root.GetComponentsInChildren<TextMesh>(true);
                Assert.That(labels.Length, Is.GreaterThan(0));
                foreach (TextMesh label in labels)
                {
                    Assert.That(label.transform.localRotation, Is.EqualTo(Quaternion.identity), label.text);
                    Assert.That(label.transform.localScale, Is.EqualTo(Vector3.one), label.text);
                    Assert.That(
                        Vector3.Dot(label.transform.forward, camera.transform.forward),
                        Is.GreaterThan(0.999f),
                        label.text);
                    Assert.That(
                        camera.WorldToViewportPoint(label.transform.position).z,
                        Is.GreaterThan(0f),
                        label.text);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ReviewCamera_ContainsAllThreeHitProductionStationsAtSixteenByNine()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = FindReviewRoot(scene);
                Camera camera = root.GetComponentsInChildren<Camera>(true).Single();
                camera.aspect = 16f / 9f;
                Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(camera);
                Transform hitZone = root.transform.Find("ZONE_05_HIT_PRODUCTION_v005");
                Transform[] stations = hitZone.Cast<Transform>()
                    .Where(child => child.name.StartsWith("HIT_STATION_", StringComparison.Ordinal))
                    .ToArray();
                Assert.That(stations.Length, Is.EqualTo(3));

                foreach (Transform station in stations)
                {
                    Renderer backdrop = station.GetComponentsInChildren<Renderer>(true)
                        .Single(item => item.gameObject.name == "REVIEW_BACKDROP");
                    Assert.That(
                        GeometryUtility.TestPlanesAABB(frustum, backdrop.bounds),
                        Is.True,
                        station.name);
                    Vector3 viewport = camera.WorldToViewportPoint(backdrop.bounds.center);
                    Assert.That(viewport.z, Is.GreaterThan(camera.nearClipPlane), station.name);
                    Assert.That(viewport.x, Is.InRange(0f, 1f), station.name);
                    Assert.That(viewport.y, Is.InRange(0f, 1f), station.name);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ReviewPlaybackBootstrap_RestartsAllActiveParticleSystems()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = FindReviewRoot(scene);
                MonoBehaviour playback = root.GetComponents<MonoBehaviour>()
                    .Single(item =>
                        item.GetType().FullName ==
                        "BreakNotice.ContestReview.ShowcaseVfxReviewPlayback");
                MethodInfo restart = playback.GetType().GetMethod("RestartAllNow");
                Assert.That(restart, Is.Not.Null);
                int started = (int)restart.Invoke(playback, null);
                Assert.That(started, Is.EqualTo(188));
                Assert.That(
                    root.GetComponentsInChildren<ParticleSystem>(false)
                        .All(system => system.isPlaying),
                    Is.True);

                Transform hitZone = root.transform.Find("ZONE_05_HIT_PRODUCTION_v005");
                ParticleSystem[] soloSystems = hitZone
                    .GetComponentsInChildren<ParticleSystem>(false)
                    .Where(system => system.transform.parent != null &&
                        system.transform.parent.name.StartsWith("SOLO_0", StringComparison.Ordinal))
                    .ToArray();
                Assert.That(soloSystems.Length, Is.EqualTo(12));
                Assert.That(soloSystems.All(system => system.particleCount > 0), Is.True);

                Transform oldNew = root.transform.Find("ZONE_01_OLD_VS_NEW");
                Transform[] stations = oldNew.Cast<Transform>()
                    .Where(child => child.name.StartsWith("STATION_", StringComparison.Ordinal))
                    .ToArray();
                foreach (Transform station in stations)
                {
                    Transform composite = station.Find("NEW_PREMIUM_V002_COMPOSITE");
                    ParticleSystem[] systems =
                        composite.GetComponentsInChildren<ParticleSystem>(false);
                    Assert.That(systems.Length, Is.EqualTo(3), station.name);
                    foreach (ParticleSystem system in systems)
                    {
                        system.Simulate(0.42f, false, true, false);
                        Assert.That(system.particleCount, Is.GreaterThan(0), system.name);
                    }
                }

                ParticleSystem[] hitSystems =
                    hitZone.GetComponentsInChildren<ParticleSystem>(false);
                Assert.That(hitSystems.Length, Is.EqualTo(48));
                foreach (ParticleSystem system in hitSystems)
                {
                    system.Simulate(0.14f, false, true, false);
                    Assert.That(system.particleCount, Is.GreaterThan(0), system.name);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void ReviewScene_RenderersHaveNoMissingMaterialTextureOrShader()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = FindReviewRoot(scene);
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    Assert.That(renderer.sharedMaterials, Is.Not.Empty, renderer.name);
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        Assert.That(material, Is.Not.Null, renderer.name);
                        Assert.That(material.shader, Is.Not.Null, material.name);
                        Assert.That(
                            material.shader.name,
                            Is.Not.EqualTo("Hidden/InternalErrorShader"),
                            material.name);
                    }

                    if (renderer is ParticleSystemRenderer)
                    {
                        Material material = renderer.sharedMaterial;
                        Assert.That(material.HasProperty("_MainTex"), Is.True, material.name);
                        Assert.That(material.GetTexture("_MainTex"), Is.Not.Null, material.name);
                    }
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void PremiumDelivery_HasExpectedCountsBytesAndUniqueGuids()
        {
            Assert.That(Candidates.Length, Is.EqualTo(24));
            Assert.That(Candidates.Count(candidate => candidate.IsFlipbook), Is.EqualTo(12));
            Assert.That(Candidates.Count(candidate => !candidate.IsFlipbook), Is.EqualTo(12));
            Assert.That(Candidates.Sum(candidate => candidate.ExpectedBytes), Is.EqualTo(2289333));
            Assert.That(Candidates.Select(candidate => candidate.ExpectedSha256).Distinct().Count(), Is.EqualTo(24));
            Assert.That(Prefabs.Length, Is.EqualTo(16));
            string[] paths = Candidates.Select(candidate => candidate.TexturePath)
                .Concat(Candidates.Select(candidate => candidate.MaterialPath))
                .Concat(Prefabs.Select(prefab => prefab.Path))
                .ToArray();
            Assert.That(paths.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(paths.Length));
            string[] guids = paths.Select(AssetDatabase.AssetPathToGUID).ToArray();
            Assert.That(guids.All(guid => !string.IsNullOrWhiteSpace(guid)), Is.True);
            Assert.That(guids.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(guids.Length));
        }

        [Test]
        public void HitProductionV005_HasThirtySixByteIdenticalSafeFlipbooksAndNinePrefabs()
        {
            var texturePaths = new List<string>();
            var materialPaths = new List<string>();
            var prefabPaths = new List<string>();
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string repositoryRoot = Path.GetFullPath(Path.Combine(projectRoot, ".."));

            foreach (string intensity in HitIntensities)
            {
                foreach (string variation in HitVariations)
                {
                    string prefabPath = HitRootPath + "/Prefabs/PF_VFX_Showcase_Hit" +
                        intensity + "_" + variation + "_REVIEW_ONLY_v005.prefab";
                    prefabPaths.Add(prefabPath);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    Assert.That(prefab, Is.Not.Null, prefabPath);
                    Assert.That(prefab.GetComponentsInChildren<ParticleSystem>(true).Length, Is.EqualTo(4), prefabPath);

                    foreach (string layer in HitLayers)
                    {
                        string name = "Hit" + intensity + "_" + variation + "_" + layer;
                        string texturePath = HitRootPath + "/Flipbooks/T_VFX_Showcase_" + name + "_4x4_v005.png";
                        string materialPath = HitRootPath + "/Materials/M_VFX_Showcase_Premium_" + name + "_v005.mat";
                        texturePaths.Add(texturePath);
                        materialPaths.Add(materialPath);

                        string sourceRelative = texturePath.Replace(
                            "Assets/_Project/Art/VFX/Showcase/HitProduction_v005/Flipbooks/",
                            "ArtSource/VFX/Showcase/Flipbooks/HitProduction_v005/");
                        string sourcePath = Path.Combine(repositoryRoot, sourceRelative.Replace('/', Path.DirectorySeparatorChar));
                        string runtimePath = ToPhysicalPath(texturePath);
                        Assert.That(File.Exists(sourcePath), Is.True, sourcePath);
                        Assert.That(File.Exists(runtimePath), Is.True, runtimePath);
                        byte[] source = File.ReadAllBytes(sourcePath);
                        byte[] runtime = File.ReadAllBytes(runtimePath);
                        Assert.That(runtime, Is.EqualTo(source), texturePath);

                        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
                        Assert.That(importer, Is.Not.Null, texturePath);
                        Assert.That(importer.alphaSource, Is.EqualTo(TextureImporterAlphaSource.FromInput));
                        Assert.That(importer.alphaIsTransparency, Is.True);
                        Assert.That(importer.mipmapEnabled, Is.True);
                        Assert.That(importer.borderMipmap, Is.True);
                        Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
                        Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
                        Assert.That(importer.maxTextureSize, Is.EqualTo(2048));
                        Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));

                        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                        Assert.That(material, Is.Not.Null, materialPath);
                        Assert.That(material.shader, Is.Not.Null);
                        Assert.That(material.shader.name, Is.EqualTo(ParticleShaderName));
                        Assert.That(AssetDatabase.GetAssetPath(material.GetTexture("_MainTex")), Is.EqualTo(texturePath));

                        Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                        try
                        {
                            Assert.That(ImageConversion.LoadImage(decoded, runtime, false), Is.True);
                            Assert.That(decoded.width, Is.EqualTo(2048));
                            Assert.That(decoded.height, Is.EqualTo(2048));
                            Color32[] pixels = decoded.GetPixels32();
                            Assert.That(pixels.Count(IsMissingTextureMagenta), Is.Zero, texturePath);
                            Assert.That(pixels.Count(IsNeonCyan), Is.Zero, texturePath);
                            Assert.That(pixels.Count(IsGreenSpill), Is.Zero, texturePath);
                            Assert.That(
                                pixels.Count(pixel => pixel.a > 8 && pixel.r < 8 && pixel.g < 8 && pixel.b < 8),
                                Is.Zero,
                                texturePath);
                        }
                        finally
                        {
                            UnityEngine.Object.DestroyImmediate(decoded);
                        }
                    }
                }
            }

            Assert.That(texturePaths.Count, Is.EqualTo(36));
            Assert.That(materialPaths.Count, Is.EqualTo(36));
            Assert.That(prefabPaths.Count, Is.EqualTo(9));
            string[] allPaths = texturePaths.Concat(materialPaths).Concat(prefabPaths).ToArray();
            Assert.That(allPaths.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(allPaths.Length));
            string[] guids = allPaths.Select(AssetDatabase.AssetPathToGUID).ToArray();
            Assert.That(guids.All(guid => !string.IsNullOrWhiteSpace(guid)), Is.True);
            Assert.That(guids.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(allPaths.Length));
        }

        [Test]
        public void HitProductionV005_ReviewZoneHasNineCompositeAndTwelveSoloViews()
        {
            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = FindReviewRoot(scene);
                Transform zone = root.transform.Find("ZONE_05_HIT_PRODUCTION_v005");
                Assert.That(zone, Is.Not.Null);
                Transform[] stations = zone.Cast<Transform>()
                    .Where(child => child.name.StartsWith("HIT_STATION_", StringComparison.Ordinal))
                    .ToArray();
                Assert.That(stations.Length, Is.EqualTo(3));
                foreach (Transform station in stations)
                {
                    Assert.That(station.Cast<Transform>().Count(child => child.name.StartsWith("COMPOSITE_", StringComparison.Ordinal)), Is.EqualTo(3), station.name);
                    Assert.That(station.Cast<Transform>().Count(child => child.name.StartsWith("SOLO_0", StringComparison.Ordinal)), Is.EqualTo(4), station.name);
                    Assert.That(station.Cast<Transform>().Count(child => child.name.StartsWith("CONTACT_SURFACE_PROXY_", StringComparison.Ordinal)), Is.EqualTo(3), station.name);
                    Assert.That(station.GetComponentsInChildren<ParticleSystem>(false).Length, Is.EqualTo(16), station.name);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [TestCaseSource(nameof(CandidateCases))]
        public void RuntimeCandidates_AreByteIdenticalToArtSource(Candidate candidate)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string repositoryRoot = Path.GetFullPath(Path.Combine(projectRoot, ".."));
            string sourceRelative = candidate.TexturePath
                .Replace("Assets/_Project/Art/VFX/Showcase/", "ArtSource/VFX/Showcase/");
            string sourcePath = Path.Combine(
                repositoryRoot,
                sourceRelative.Replace('/', Path.DirectorySeparatorChar));
            byte[] source = File.ReadAllBytes(sourcePath);
            byte[] runtime = File.ReadAllBytes(ToPhysicalPath(candidate.TexturePath));
            Assert.That(runtime, Is.EqualTo(source));
            Assert.That(Sha256(runtime), Is.EqualTo(candidate.ExpectedSha256));
        }

        private static GameObject FindReviewRoot(Scene scene)
        {
            GameObject root = scene.GetRootGameObjects()
                .SingleOrDefault(item =>
                    item.name == "LOCAL_REVIEW_ONLY_ShowcaseVFX_ROOT");
            Assert.That(root, Is.Not.Null);
            return root;
        }

        private static Candidate C(
            string name,
            string relativeTexturePath,
            string sha,
            int bytes,
            bool flipbook,
            int frameZeroAlpha,
            int frameFifteenAlpha)
        {
            return new Candidate(
                name,
                ReviewRootPath + "/" + relativeTexturePath,
                PremiumRootPath + "/Materials/M_VFX_Showcase_Premium_" + name + "_v002.mat",
                sha,
                bytes,
                flipbook,
                frameZeroAlpha,
                frameFifteenAlpha);
        }

        private static PrefabContract P(string name, bool physical, string variation = null)
        {
            string suffix = physical ? "_VAR_" + variation : string.Empty;
            return new PrefabContract(
                name,
                physical,
                variation ?? "STATIC",
                PremiumRootPath + "/Prefabs/PF_VFX_Showcase_" + name +
                "_PREMIUM_REVIEW_ONLY" + suffix + "_v002.prefab");
        }

        private static bool IsMissingTextureMagenta(Color32 pixel)
        {
            return pixel.a > 0 && pixel.r >= 240 && pixel.g <= 16 && pixel.b >= 240;
        }

        private static bool IsNeonCyan(Color32 pixel)
        {
            return pixel.a > 8 && pixel.r < 90 && pixel.g > 200 && pixel.b > 200;
        }

        private static bool IsGreenSpill(Color32 pixel)
        {
            return pixel.a > 8 && pixel.g > pixel.r * 1.45f && pixel.g > pixel.b * 1.45f;
        }

        private static void AssertTransparentOuterMargin(
            Color32[] pixels,
            int width,
            int height,
            int margin)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < margin; x++)
                {
                    Assert.That(pixels[y * width + x].a, Is.Zero);
                    Assert.That(pixels[y * width + (width - 1 - x)].a, Is.Zero);
                }
            }
            for (int y = 0; y < margin; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Assert.That(pixels[y * width + x].a, Is.Zero);
                    Assert.That(pixels[(height - 1 - y) * width + x].a, Is.Zero);
                }
            }
        }

        private static string ToPhysicalPath(string assetPath)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            return Path.GetFullPath(Path.Combine(
                projectRoot,
                assetPath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string Sha256(byte[] bytes)
        {
            using SHA256 sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(bytes))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        public sealed class Candidate
        {
            public Candidate(
                string name,
                string texturePath,
                string materialPath,
                string expectedSha256,
                int expectedBytes,
                bool isFlipbook,
                int expectedFrameZeroAlpha,
                int expectedFrameFifteenAlpha)
            {
                Name = name;
                TexturePath = texturePath;
                MaterialPath = materialPath;
                ExpectedSha256 = expectedSha256;
                ExpectedBytes = expectedBytes;
                IsFlipbook = isFlipbook;
                ExpectedFrameZeroAlpha = expectedFrameZeroAlpha;
                ExpectedFrameFifteenAlpha = expectedFrameFifteenAlpha;
            }

            public string Name { get; }
            public string TexturePath { get; }
            public string MaterialPath { get; }
            public string ExpectedSha256 { get; }
            public int ExpectedBytes { get; }
            public bool IsFlipbook { get; }
            public int ExpectedFrameZeroAlpha { get; }
            public int ExpectedFrameFifteenAlpha { get; }
            public override string ToString() => Name;
        }

        public sealed class PrefabContract
        {
            public PrefabContract(string name, bool isPhysical, string variation, string path)
            {
                Name = name;
                IsPhysical = isPhysical;
                Variation = variation;
                Path = path;
            }

            public string Name { get; }
            public bool IsPhysical { get; }
            public string Variation { get; }
            public string Path { get; }
            public override string ToString() => Name + "_" + Variation;
        }
    }
}
