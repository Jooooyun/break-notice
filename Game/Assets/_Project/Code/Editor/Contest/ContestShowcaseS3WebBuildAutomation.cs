using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BreakNotice.Contest.Editor
{
    public static class ContestShowcaseS3WebBuildAutomation
    {
        [MenuItem("BREAK NOTICE/Contest/Build S3 Showcase Web Development")]
        public static void BuildWebDevelopment()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(
                    ContestShowcaseSceneAuthoring.ShowcaseS3ScenePath) == null)
            {
                throw new FileNotFoundException(
                    "Prepared S3 Showcase Scene is missing: " +
                    ContestShowcaseSceneAuthoring.ShowcaseS3ScenePath);
            }

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                throw new InvalidOperationException(
                    "Web Build Support for exact Unity 6000.3.20f1 is unavailable.");
            }

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string buildRoot = Path.GetFullPath(Path.Combine(
                projectRoot,
                ContestC0Automation.DevelopmentBuildPath));
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
                scenes = new[] { ContestShowcaseSceneAuthoring.ShowcaseS3ScenePath },
                locationPathName = buildRoot,
                target = BuildTarget.WebGL,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"S3 Showcase Web Development build failed: {summary.result}, " +
                    $"{summary.totalErrors} errors.");
            }

            string metadata =
                "{\n" +
                $"  \"unityVersion\": \"{Application.unityVersion}\",\n" +
                $"  \"scene\": \"{ContestShowcaseSceneAuthoring.ShowcaseS3ScenePath}\",\n" +
                "  \"showcaseCheckpoint\": \"S3\",\n" +
                $"  \"buildGuid\": \"{summary.guid}\",\n" +
                $"  \"result\": \"{summary.result}\",\n" +
                $"  \"totalBytes\": {summary.totalSize},\n" +
                $"  \"totalWarnings\": {summary.totalWarnings},\n" +
                $"  \"totalErrors\": {summary.totalErrors}\n" +
                "}\n";
            File.WriteAllText(Path.Combine(buildRoot, "BuildMetadata.json"), metadata);

            Debug.Log(
                $"S3 Showcase Web Development build succeeded at {buildRoot}; " +
                $"bytes={summary.totalSize}; guid={summary.guid}.");
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
