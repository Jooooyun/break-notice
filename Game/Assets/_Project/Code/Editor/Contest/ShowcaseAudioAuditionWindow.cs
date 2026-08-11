using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BreakNotice.Contest.Editor
{
    public sealed class ShowcaseAudioAuditionWindow : EditorWindow
    {
        private static readonly string[] PlayMethodNames =
        {
            "PlayPreviewClip",
            "PlayClip"
        };

        private static readonly string[] StopMethodNames =
        {
            "StopAllPreviewClips",
            "StopAllClips"
        };

        private int selectedIndex;
        private Vector2 scrollPosition;

        [MenuItem("BREAK NOTICE/Audio Review/Open Showcase Audio Audition")]
        public static void Open()
        {
            ShowcaseAudioAuditionWindow window = GetWindow<ShowcaseAudioAuditionWindow>();
            window.titleContent = new GUIContent("Showcase Audio Review");
            window.minSize = new Vector2(720f, 420f);
            window.Show();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Showcase Audio Review");
            selectedIndex = Mathf.Clamp(
                selectedIndex,
                0,
                Math.Max(ShowcaseAudioReviewAuthoring.Clips.Count - 1, 0));
        }

        private void OnDisable()
        {
            StopPreview();
        }

        private void OnGUI()
        {
            IReadOnlyList<ShowcaseAudioReviewAuthoring.ClipDefinition> definitions =
                ShowcaseAudioReviewAuthoring.Clips;

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "LOCAL REVIEW ONLY — no Gameplay Runtime or AudioMixer integration",
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "23 byte-identical review clips; duration is read from the imported AudioClip.",
                EditorStyles.miniLabel);
            EditorGUILayout.Space(6f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Previous", GUILayout.Height(28f)))
                {
                    SelectRelative(-1, true);
                }

                if (GUILayout.Button("Play", GUILayout.Height(28f)))
                {
                    PlaySelected();
                }

                if (GUILayout.Button("Stop", GUILayout.Height(28f)))
                {
                    StopPreview();
                }

                if (GUILayout.Button("Next", GUILayout.Height(28f)))
                {
                    SelectRelative(1, true);
                }
            }

            if (definitions.Count == 0)
            {
                EditorGUILayout.HelpBox("No authorized review clips are defined.", MessageType.Error);
                return;
            }

            selectedIndex = Mathf.Clamp(selectedIndex, 0, definitions.Count - 1);
            DrawSelected(definitions[selectedIndex]);
            EditorGUILayout.Space(6f);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int index = 0; index < definitions.Count; index++)
            {
                DrawClipRow(index, definitions[index]);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSelected(ShowcaseAudioReviewAuthoring.ClipDefinition definition)
        {
            AudioClip clip = ShowcaseAudioReviewAuthoring.LoadClip(definition);
            string duration = clip == null ? "MISSING" : $"{clip.length:0.000} s";
            string channels = clip == null ? "?" : clip.channels.ToString();
            string loop = definition.LoopIntent ? "LOOP REVIEW" : "ONE-SHOT";

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    $"{definition.Order:D2} / {ShowcaseAudioReviewAuthoring.Clips.Count:D2}  " +
                    definition.FileName,
                    EditorStyles.boldLabel);
                EditorGUILayout.LabelField(
                    $"Category: {definition.Category}    Duration: {duration}    " +
                    $"Channels: {channels}    Intent: {loop}");
                EditorGUILayout.LabelField(definition.AssetPath, EditorStyles.miniLabel);
            }
        }

        private void DrawClipRow(
            int index,
            ShowcaseAudioReviewAuthoring.ClipDefinition definition)
        {
            AudioClip clip = ShowcaseAudioReviewAuthoring.LoadClip(definition);
            string duration = clip == null ? "MISSING" : $"{clip.length:0.000} s";
            GUIStyle style = index == selectedIndex
                ? EditorStyles.toolbarButton
                : EditorStyles.miniButton;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button($"{definition.Order:D2}", style, GUILayout.Width(42f)))
                {
                    selectedIndex = index;
                    Repaint();
                }

                EditorGUILayout.LabelField(definition.Category, GUILayout.Width(74f));
                EditorGUILayout.LabelField(definition.FileName, GUILayout.MinWidth(360f));
                EditorGUILayout.LabelField(duration, GUILayout.Width(74f));
                EditorGUILayout.LabelField(
                    definition.LoopIntent ? "Loop" : "One-shot",
                    GUILayout.Width(70f));

                using (new EditorGUI.DisabledScope(clip == null))
                {
                    if (GUILayout.Button("Play", GUILayout.Width(52f)))
                    {
                        selectedIndex = index;
                        PlaySelected();
                    }
                }
            }
        }

        private void SelectRelative(int offset, bool play)
        {
            int count = ShowcaseAudioReviewAuthoring.Clips.Count;
            if (count == 0)
            {
                return;
            }

            selectedIndex = (selectedIndex + offset + count) % count;
            Repaint();
            if (play)
            {
                PlaySelected();
            }
        }

        private void PlaySelected()
        {
            IReadOnlyList<ShowcaseAudioReviewAuthoring.ClipDefinition> definitions =
                ShowcaseAudioReviewAuthoring.Clips;
            if (definitions.Count == 0)
            {
                return;
            }

            ShowcaseAudioReviewAuthoring.ClipDefinition definition = definitions[selectedIndex];
            AudioClip clip = ShowcaseAudioReviewAuthoring.LoadClip(definition);
            if (clip == null)
            {
                ShowNotification(new GUIContent($"Missing clip: {definition.FileName}"));
                return;
            }

            StopPreview();
            MethodInfo playMethod = FindAudioUtilMethod(PlayMethodNames, 3);
            if (playMethod == null)
            {
                Debug.LogError("UnityEditor.AudioUtil preview method was not found.");
                ShowNotification(new GUIContent("Audio preview API unavailable"));
                return;
            }

            playMethod.Invoke(null, new object[] { clip, 0, definition.LoopIntent });
        }

        internal static void ValidatePreviewApi()
        {
            if (FindAudioUtilMethod(PlayMethodNames, 3) == null ||
                FindAudioUtilMethod(StopMethodNames, 0) == null)
            {
                throw new InvalidOperationException(
                    "UnityEditor.AudioUtil preview methods required by the Showcase audition window " +
                    "were not found in this Unity Editor version.");
            }
        }

        private static void StopPreview()
        {
            MethodInfo stopMethod = FindAudioUtilMethod(StopMethodNames, 0);
            stopMethod?.Invoke(null, null);
        }

        private static MethodInfo FindAudioUtilMethod(
            IEnumerable<string> names,
            int parameterCount)
        {
            Type audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            if (audioUtilType == null)
            {
                return null;
            }

            return audioUtilType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(method =>
                    names.Contains(method.Name, StringComparer.Ordinal) &&
                    method.GetParameters().Length == parameterCount);
        }
    }
}
