using System;
using System.IO;
using System.Linq;
using BreakNotice.Contest;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BreakNotice.Contest.Editor
{
    public static class ContestC0Automation
    {
        public const string SourceScenePath =
            "Assets/_Project/Scenes/10_Prototype/G4A_LocomotionCameraGraybox.unity";

        public const string ContestScenePath =
            "Assets/_Project/Scenes/10_Prototype/Contest_SingleRoomGraybox.unity";

        public const string DevelopmentBuildPath = "Builds/WebGL/Development";

        private const string BrowserInputObjectName = "ContestBrowserInput";

        [MenuItem("BREAK NOTICE/Contest/Prepare C0 Assets")]
        public static void PrepareC0Assets()
        {
            RequireAsset(SourceScenePath);

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ContestScenePath) == null)
            {
                if (!AssetDatabase.CopyAsset(SourceScenePath, ContestScenePath))
                {
                    throw new InvalidOperationException(
                        $"Could not copy the contest scene from {SourceScenePath}.");
                }

                AssetDatabase.ImportAsset(ContestScenePath, ImportAssetOptions.ForceSynchronousImport);
            }

            Scene contestScene = EditorSceneManager.OpenScene(ContestScenePath, OpenSceneMode.Single);
            GameObject browserInput = contestScene.GetRootGameObjects()
                .FirstOrDefault(root => root.name == BrowserInputObjectName);

            if (browserInput == null)
            {
                browserInput = new GameObject(BrowserInputObjectName);
                SceneManager.MoveGameObjectToScene(browserInput, contestScene);
            }

            if (browserInput.GetComponent<BrowserPointerCapture>() == null)
            {
                browserInput.AddComponent<BrowserPointerCapture>();
            }

            EditorSceneManager.MarkSceneDirty(contestScene);
            if (!EditorSceneManager.SaveScene(contestScene, ContestScenePath))
            {
                throw new InvalidOperationException($"Could not save {ContestScenePath}.");
            }

            EditorBuildSettings.scenes = EditorBuildSettings.scenes
                .Where(scene => !string.Equals(
                    scene.path,
                    ContestScenePath,
                    StringComparison.OrdinalIgnoreCase))
                .Concat(new[] { new EditorBuildSettingsScene(ContestScenePath, true) })
                .ToArray();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"C0 assets prepared: {ContestScenePath}");
        }

        [MenuItem("BREAK NOTICE/Contest/Build Web Development")]
        public static void BuildWebDevelopment()
        {
            PrepareC0Assets();

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                throw new InvalidOperationException(
                    "Web Build Support for exact Unity 6000.3.20f1 is unavailable.");
            }

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string buildRoot = Path.GetFullPath(Path.Combine(projectRoot, DevelopmentBuildPath));
            EnsureChildPath(projectRoot, buildRoot);

            if (Directory.Exists(buildRoot))
            {
                Directory.Delete(buildRoot, true);
            }

            Directory.CreateDirectory(buildRoot);

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.runInBackground = true;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ContestScenePath },
                locationPathName = buildRoot,
                target = BuildTarget.WebGL,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Web Development build failed: {summary.result}, {summary.totalErrors} errors.");
            }

            string metadata =
                "{\n" +
                $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
                $"  \"buildGuid\": \"{summary.guid}\",\n" +
                $"  \"result\": \"{summary.result}\",\n" +
                $"  \"totalBytes\": {summary.totalSize},\n" +
                $"  \"totalWarnings\": {summary.totalWarnings},\n" +
                $"  \"totalErrors\": {summary.totalErrors}\n" +
                "}\n";
            File.WriteAllText(Path.Combine(buildRoot, "BuildMetadata.json"), metadata);

            Debug.Log(
                $"C0 Web Development build succeeded at {buildRoot}; " +
                $"bytes={summary.totalSize}; guid={summary.guid}.");
        }

        private static void RequireAsset(string path)
        {
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) == null)
            {
                throw new FileNotFoundException($"Required asset does not exist: {path}");
            }
        }

        private static void EnsureChildPath(string parent, string child)
        {
            string normalizedParent = parent.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!child.StartsWith(normalizedParent, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Unsafe build output path: {child}");
            }
        }
    }
}
