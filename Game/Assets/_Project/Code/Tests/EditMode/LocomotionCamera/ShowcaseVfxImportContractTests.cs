using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private const string ReviewScenePath =
            "Assets/_Project/Scenes/90_Review/LOCAL_REVIEW_ONLY_ShowcaseVFX.unity";

        private const string ParticleShaderName =
            "Particles/Standard Unlit";

        private static readonly Candidate[] Candidates =
        {
            new Candidate(
                "InteractionSoftRing",
                "Assets/_Project/Art/VFX/Showcase/Textures/T_VFX_Showcase_InteractionSoftRing_v001.png",
                "Assets/_Project/Art/VFX/Showcase/Materials/M_VFX_Showcase_InteractionSoftRing_v001.mat",
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_InteractionSoftRing_LOCAL_REVIEW_ONLY_v001.prefab",
                "3cdefac76ca1fcde0d35d58b602e9c6ea6a4f5abaad43fff49b0b0c5add91841",
                21311,
                512,
                false,
                1.35f,
                1.75f),
            new Candidate(
                "PickupCatchConfirm",
                "Assets/_Project/Art/VFX/Showcase/Textures/T_VFX_Showcase_PickupCatchConfirm_v001.png",
                "Assets/_Project/Art/VFX/Showcase/Materials/M_VFX_Showcase_PickupCatchConfirm_v001.mat",
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_PickupCatchConfirm_LOCAL_REVIEW_ONLY_v001.prefab",
                "01c9410152bf89e0ba35c0897d879ba6144bb80a307ef349ccb3d504f7b7b0df",
                12333,
                512,
                false,
                0.72f,
                1.28f),
            new Candidate(
                "InteractionRejected",
                "Assets/_Project/Art/VFX/Showcase/Textures/T_VFX_Showcase_InteractionRejected_v001.png",
                "Assets/_Project/Art/VFX/Showcase/Materials/M_VFX_Showcase_InteractionRejected_v001.mat",
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_InteractionRejected_LOCAL_REVIEW_ONLY_v001.prefab",
                "79a350a04630a07f06d9af12c52eb7202cd091615a9452577208e6ee709ca02a",
                8266,
                512,
                false,
                0.78f,
                1.36f),
            new Candidate(
                "LightContactDust",
                "Assets/_Project/Art/VFX/Showcase/Flipbooks/T_VFX_Showcase_LightContactDust_4x4_v001.png",
                "Assets/_Project/Art/VFX/Showcase/Materials/M_VFX_Showcase_LightContactDust_4x4_v001.mat",
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_LightContactDust_4x4_LOCAL_REVIEW_ONLY_v001.prefab",
                "db85987ec72d905fdd75dc6ea20a8598f0075493b4e2c71fde1c744beda70f33",
                533923,
                2048,
                true,
                0.72f,
                1.24f),
            new Candidate(
                "ChairWheelContact",
                "Assets/_Project/Art/VFX/Showcase/Flipbooks/T_VFX_Showcase_ChairWheelContact_4x4_v001.png",
                "Assets/_Project/Art/VFX/Showcase/Materials/M_VFX_Showcase_ChairWheelContact_4x4_v001.mat",
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_ChairWheelContact_4x4_LOCAL_REVIEW_ONLY_v001.prefab",
                "4e528117f430367a1f3b69df330145584f5b440c54e80e63c69696ac538d0970",
                506688,
                2048,
                true,
                0.96f,
                1.52f),
            new Candidate(
                "RestrainedPaperBurst",
                "Assets/_Project/Art/VFX/Showcase/Flipbooks/T_VFX_Showcase_RestrainedPaperBurst_4x4_v001.png",
                "Assets/_Project/Art/VFX/Showcase/Materials/M_VFX_Showcase_RestrainedPaperBurst_4x4_v001.mat",
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_RestrainedPaperBurst_4x4_LOCAL_REVIEW_ONLY_v001.prefab",
                "07e97069f6c25bc077971ffda8dd34add53b795522039e459df655d1e120af76",
                285606,
                2048,
                true,
                1.12f,
                1.72f),
            new Candidate(
                "AllyReadySupport",
                "Assets/_Project/Art/VFX/Showcase/Textures/T_VFX_Showcase_AllyReadySupport_v001.png",
                "Assets/_Project/Art/VFX/Showcase/Materials/M_VFX_Showcase_AllyReadySupport_v001.mat",
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_AllyReadySupport_LOCAL_REVIEW_ONLY_v001.prefab",
                "73f389e9d608274eda96a9a7f497163e5e0c9b7c7675759647840ca982e93d27",
                7929,
                512,
                false,
                0.96f,
                1.48f),
            new Candidate(
                "ExitActivation",
                "Assets/_Project/Art/VFX/Showcase/Textures/T_VFX_Showcase_ExitActivation_v001.png",
                "Assets/_Project/Art/VFX/Showcase/Materials/M_VFX_Showcase_ExitActivation_v001.mat",
                "Assets/_Project/Art/VFX/Showcase/Prefabs/PF_VFX_Showcase_ExitActivation_LOCAL_REVIEW_ONLY_v001.prefab",
                "14e9c3daeb0704e64092e23614cb92a40328b95945c39930bd702c4c8ecdc398",
                7040,
                512,
                false,
                1.36f,
                1.82f)
        };

        private static IEnumerable CandidateCases => Candidates
            .Select(candidate => new TestCaseData(candidate)
                .SetName($"CandidateBytes_{candidate.Name}"));

        private static IEnumerable ImporterCases => Candidates
            .Select(candidate => new TestCaseData(candidate)
                .SetName($"ImporterContract_{candidate.Name}"));

        private static IEnumerable MaterialCases => Candidates
            .Select(candidate => new TestCaseData(candidate)
                .SetName($"MaterialContract_{candidate.Name}"));

        private static IEnumerable PrefabCases => Candidates
            .Select(candidate => new TestCaseData(candidate)
                .SetName($"PrefabContract_{candidate.Name}"));

        private static IEnumerable FlipbookCases => Candidates
            .Where(candidate => candidate.IsFlipbook)
            .Select(candidate => new TestCaseData(candidate)
                .SetName($"FlipbookGrid_{candidate.Name}"));

        [TestCaseSource(nameof(CandidateCases))]
        public void CandidateBytes_AreExpectedRgbaPngsWithTransparentMargins(
            Candidate candidate)
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
                Assert.That(decoded.width, Is.EqualTo(candidate.Dimension));
                Assert.That(decoded.height, Is.EqualTo(candidate.Dimension));

                Color32[] pixels = decoded.GetPixels32();
                int alphaNonZero = pixels.Count(pixel => pixel.a > 0);
                int alphaPartial = pixels.Count(pixel => pixel.a > 0 && pixel.a < 255);
                int missingTextureMagenta = pixels.Count(
                    pixel => pixel.r >= 240 && pixel.g <= 16 && pixel.b >= 240 && pixel.a > 0);

                Assert.That(alphaNonZero, Is.GreaterThan(0));
                Assert.That(alphaPartial, Is.GreaterThan(0));
                Assert.That(missingTextureMagenta, Is.Zero);
                AssertTransparentOuterMargin(pixels, decoded.width, decoded.height, 24);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(decoded);
            }
        }

        [TestCaseSource(nameof(ImporterCases))]
        public void TextureImporters_PreserveStraightAlphaAndBoundReviewCost(
            Candidate candidate)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(candidate.TexturePath) as TextureImporter;
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
            Assert.That(importer.npotScale, Is.EqualTo(TextureImporterNPOTScale.None));
            Assert.That(
                importer.textureCompression,
                Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.crunchedCompression, Is.False);
            Assert.That(importer.maxTextureSize, Is.EqualTo(candidate.Dimension));

            Texture2D imported = AssetDatabase.LoadAssetAtPath<Texture2D>(candidate.TexturePath);
            Assert.That(imported, Is.Not.Null);
            Assert.That(imported.width, Is.EqualTo(candidate.Dimension));
            Assert.That(imported.height, Is.EqualTo(candidate.Dimension));
        }

        [TestCaseSource(nameof(MaterialCases))]
        public void Materials_UseActivePipelineBuiltInParticleUnlitWithStraightAlpha(
            Candidate candidate)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(candidate.MaterialPath);
            Assert.That(material, Is.Not.Null, candidate.MaterialPath);
            Assert.That(material.shader, Is.Not.Null);
            Assert.That(material.shader.name, Is.EqualTo(ParticleShaderName));
            Assert.That(material.HasProperty("_MainTex"), Is.True);
            Assert.That(
                AssetDatabase.GetAssetPath(material.GetTexture("_MainTex")),
                Is.EqualTo(candidate.TexturePath));
            Assert.That(material.HasProperty("_Mode"), Is.True);
            Assert.That(material.GetFloat("_Mode"), Is.EqualTo(2f).Within(0.001f));
            Assert.That(
                material.GetFloat("_SrcBlend"),
                Is.EqualTo((float)BlendMode.SrcAlpha).Within(0.001f));
            Assert.That(
                material.GetFloat("_DstBlend"),
                Is.EqualTo((float)BlendMode.OneMinusSrcAlpha).Within(0.001f));
            Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(0f).Within(0.001f));
            Assert.That(material.GetTag("RenderType", false), Is.EqualTo("Transparent"));
            Assert.That(material.renderQueue, Is.EqualTo((int)RenderQueue.Transparent));
            Assert.That(material.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON"), Is.False);
            Assert.That(material.GetShaderPassEnabled("ShadowCaster"), Is.False);
        }

        [TestCaseSource(nameof(PrefabCases))]
        public void Prefabs_AreSelfLoopingReviewOnlyParticlePresenters(
            Candidate candidate)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(candidate.PrefabPath);
            Assert.That(prefab, Is.Not.Null, candidate.PrefabPath);

            ParticleSystem particleSystem = prefab.GetComponent<ParticleSystem>();
            Assert.That(particleSystem, Is.Not.Null);

            ParticleSystem.MainModule main = particleSystem.main;
            Assert.That(main.loop, Is.True);
            Assert.That(main.playOnAwake, Is.True);
            Assert.That(main.prewarm, Is.False);
            Assert.That(main.simulationSpace, Is.EqualTo(ParticleSystemSimulationSpace.Local));
            Assert.That(main.maxParticles, Is.EqualTo(1));
            Assert.That(main.startSpeed.constant, Is.EqualTo(0f).Within(0.001f));
            Assert.That(main.startLifetime.constant, Is.EqualTo(candidate.Lifetime).Within(0.001f));
            Assert.That(main.duration, Is.EqualTo(candidate.CycleDuration).Within(0.001f));

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            Assert.That(emission.enabled, Is.True);
            Assert.That(emission.rateOverTime.constant, Is.EqualTo(0f).Within(0.001f));
            Assert.That(emission.rateOverDistance.constant, Is.EqualTo(0f).Within(0.001f));
            Assert.That(emission.burstCount, Is.EqualTo(1));

            Assert.That(particleSystem.shape.enabled, Is.False);
            Assert.That(particleSystem.collision.enabled, Is.False);
            Assert.That(particleSystem.trigger.enabled, Is.False);
            Assert.That(particleSystem.noise.enabled, Is.False);
            Assert.That(particleSystem.trails.enabled, Is.False);

            ParticleSystem.TextureSheetAnimationModule sheet =
                particleSystem.textureSheetAnimation;
            Assert.That(sheet.enabled, Is.EqualTo(candidate.IsFlipbook));
            if (candidate.IsFlipbook)
            {
                Assert.That(sheet.mode, Is.EqualTo(ParticleSystemAnimationMode.Grid));
                Assert.That(sheet.numTilesX, Is.EqualTo(4));
                Assert.That(sheet.numTilesY, Is.EqualTo(4));
                Assert.That(sheet.animation, Is.EqualTo(ParticleSystemAnimationType.WholeSheet));
                Assert.That(sheet.cycleCount, Is.EqualTo(1));
                Assert.That(sheet.startFrame.constant, Is.EqualTo(0f).Within(0.001f));
                Assert.That(sheet.frameOverTime.curve, Is.Not.Null);
                Assert.That(
                    sheet.frameOverTime.curve.Evaluate(0f),
                    Is.EqualTo(0f).Within(0.001f));
                Assert.That(
                    sheet.frameOverTime.curve.Evaluate(1f),
                    Is.EqualTo(1f).Within(0.001f));
                Assert.That(
                    candidate.Lifetime / 16f,
                    Is.InRange(0.045f, 0.070f));
            }

            ParticleSystemRenderer renderer = prefab.GetComponent<ParticleSystemRenderer>();
            Assert.That(renderer, Is.Not.Null);
            Assert.That(renderer.renderMode, Is.EqualTo(ParticleSystemRenderMode.Billboard));
            Assert.That(renderer.alignment, Is.EqualTo(ParticleSystemRenderSpace.View));
            Assert.That(renderer.shadowCastingMode, Is.EqualTo(ShadowCastingMode.Off));
            Assert.That(renderer.receiveShadows, Is.False);
            Assert.That(
                AssetDatabase.GetAssetPath(renderer.sharedMaterial),
                Is.EqualTo(candidate.MaterialPath));

            Assert.That(prefab.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
        }

        [TestCaseSource(nameof(FlipbookCases))]
        public void Flipbooks_HaveSixteenUniquePopulatedCellsWithSafeCellMargins(
            Candidate candidate)
        {
            byte[] bytes = File.ReadAllBytes(ToPhysicalPath(candidate.TexturePath));
            Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            try
            {
                Assert.That(ImageConversion.LoadImage(decoded, bytes, false), Is.True);
                Assert.That(decoded.width, Is.EqualTo(2048));
                Assert.That(decoded.height, Is.EqualTo(2048));

                const int grid = 4;
                const int cellSize = 512;
                const int cellMargin = 20;
                Color32[] pixels = decoded.GetPixels32();
                var cellHashes = new HashSet<string>(StringComparer.Ordinal);

                for (int cellY = 0; cellY < grid; cellY++)
                {
                    for (int cellX = 0; cellX < grid; cellX++)
                    {
                        int populated = 0;
                        byte[] cellBytes = new byte[cellSize * cellSize * 4];
                        int writeIndex = 0;

                        for (int localY = 0; localY < cellSize; localY++)
                        {
                            for (int localX = 0; localX < cellSize; localX++)
                            {
                                Color32 pixel = pixels[
                                    ((cellY * cellSize) + localY) * decoded.width +
                                    (cellX * cellSize) + localX];
                                if (pixel.a > 0)
                                {
                                    populated++;
                                }

                                if (localX < cellMargin ||
                                    localX >= cellSize - cellMargin ||
                                    localY < cellMargin ||
                                    localY >= cellSize - cellMargin)
                                {
                                    Assert.That(
                                        pixel.a,
                                        Is.Zero,
                                        $"{candidate.Name} cell ({cellX},{cellY}) margin");
                                }

                                cellBytes[writeIndex++] = pixel.r;
                                cellBytes[writeIndex++] = pixel.g;
                                cellBytes[writeIndex++] = pixel.b;
                                cellBytes[writeIndex++] = pixel.a;
                            }
                        }

                        Assert.That(populated, Is.GreaterThan(0));
                        cellHashes.Add(Sha256(cellBytes));
                    }
                }

                Assert.That(cellHashes.Count, Is.EqualTo(16));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(decoded);
            }
        }

        [Test]
        public void ReviewScene_ContainsEightIsolatedStationsAndNoGameplayHook()
        {
            Assert.That(
                AssetDatabase.LoadAssetAtPath<SceneAsset>(ReviewScenePath),
                Is.Not.Null);

            Scene scene = EditorSceneManager.OpenScene(ReviewScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject root = scene.GetRootGameObjects()
                    .SingleOrDefault(candidate =>
                        candidate.name == "LOCAL_REVIEW_ONLY_ShowcaseVFX_ROOT");
                Assert.That(root, Is.Not.Null);

                Transform[] stations = root.transform.Cast<Transform>()
                    .Where(child => child.name.StartsWith("STATION_", StringComparison.Ordinal))
                    .ToArray();
                Assert.That(stations.Length, Is.EqualTo(8));

                ParticleSystem[] particleSystems =
                    root.GetComponentsInChildren<ParticleSystem>(true);
                Assert.That(particleSystems.Length, Is.EqualTo(8));
                Assert.That(particleSystems.All(system => system.main.loop), Is.True);
                Assert.That(particleSystems.All(system => system.main.playOnAwake), Is.True);
                Assert.That(
                    particleSystems.Count(system => system.textureSheetAnimation.enabled),
                    Is.EqualTo(3));

                foreach (Transform station in stations)
                {
                    Assert.That(
                        station.GetComponentsInChildren<ParticleSystem>(true).Length,
                        Is.EqualTo(1),
                        station.name);
                    Assert.That(station.Find("LABEL"), Is.Not.Null, station.name);
                    Assert.That(station.Find("REVIEW_BACKDROP"), Is.Not.Null, station.name);
                }

                string[] prefabPaths = particleSystems
                    .Select(system =>
                        PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(system.gameObject))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                Assert.That(prefabPaths.Length, Is.EqualTo(8));
                Assert.That(
                    prefabPaths.All(path => path.StartsWith(
                        $"{ReviewRootPath}/Prefabs/",
                        StringComparison.Ordinal)),
                    Is.True);

                Assert.That(root.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
                Assert.That(root.GetComponentsInChildren<Camera>(true).Length, Is.EqualTo(1));
                Assert.That(root.GetComponentsInChildren<AudioListener>(true).Length, Is.EqualTo(1));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }

            Assert.That(
                EditorBuildSettings.scenes.Any(scene =>
                    string.Equals(scene.path, ReviewScenePath, StringComparison.OrdinalIgnoreCase)),
                Is.False);

            string[] dependencies = AssetDatabase.GetDependencies(ReviewScenePath, true);
            Assert.That(
                dependencies.Any(path => path.StartsWith(
                    "Assets/_Project/Scenes/10_Prototype/",
                    StringComparison.OrdinalIgnoreCase)),
                Is.False);
            Assert.That(
                dependencies.Any(path => path.Contains("/Code/Runtime/")),
                Is.False);
            Assert.That(
                dependencies.Any(path => path.StartsWith(
                    "Assets/_Project/Prefabs/",
                    StringComparison.OrdinalIgnoreCase)),
                Is.False);
            Assert.That(
                AssetDatabase.FindAssets("t:Shader", new[] { ReviewRootPath }),
                Is.Empty);
        }

        [Test]
        public void DeliverySet_HasEightCandidatesAndExpectedTotalBytes()
        {
            Assert.That(Candidates.Length, Is.EqualTo(8));
            Assert.That(Candidates.Count(candidate => candidate.IsFlipbook), Is.EqualTo(3));
            Assert.That(Candidates.Count(candidate => !candidate.IsFlipbook), Is.EqualTo(5));
            Assert.That(Candidates.Sum(candidate => candidate.ExpectedBytes), Is.EqualTo(1383096));
            Assert.That(Candidates.Select(candidate => candidate.ExpectedSha256).Distinct().Count(),
                Is.EqualTo(8));
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
                string prefabPath,
                string expectedSha256,
                int expectedBytes,
                int dimension,
                bool isFlipbook,
                float lifetime,
                float cycleDuration)
            {
                Name = name;
                TexturePath = texturePath;
                MaterialPath = materialPath;
                PrefabPath = prefabPath;
                ExpectedSha256 = expectedSha256;
                ExpectedBytes = expectedBytes;
                Dimension = dimension;
                IsFlipbook = isFlipbook;
                Lifetime = lifetime;
                CycleDuration = cycleDuration;
            }

            public string Name { get; }
            public string TexturePath { get; }
            public string MaterialPath { get; }
            public string PrefabPath { get; }
            public string ExpectedSha256 { get; }
            public int ExpectedBytes { get; }
            public int Dimension { get; }
            public bool IsFlipbook { get; }
            public float Lifetime { get; }
            public float CycleDuration { get; }

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
