using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BreakNotice.Stage.Editor
{
    public static class Stage01FluorescentLightInstaller
    {
        private const string FixtureName = "ENV_light_v001";
        private const string LightObjectName = "BN_FluorescentLight";

        [MenuItem("BREAK NOTICE/Stage01/Install Fluorescent Lights")]
        public static void InstallInActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("Stage01 fluorescent light installation requires a loaded scene.");
                return;
            }

            ConfigureAmbientLighting();

            GameObject[] fixtures = UnityEngine.Object
                .FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(candidate => IsFixtureRoot(candidate, scene))
                .ToArray();

            int created = 0;
            int updated = 0;
            foreach (GameObject fixture in fixtures)
            {
                Transform lightTransform = fixture.transform.Find(LightObjectName);
                if (lightTransform == null)
                {
                    GameObject lightObject = new GameObject(LightObjectName);
                    Undo.RegisterCreatedObjectUndo(lightObject, "Install fluorescent light");
                    lightTransform = lightObject.transform;
                    lightTransform.SetParent(fixture.transform, true);
                    created++;
                }
                else
                {
                    updated++;
                }

                PositionBelowFixture(fixture, lightTransform);
                ConfigureLight(lightTransform.gameObject);
                EditorUtility.SetDirty(lightTransform.gameObject);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log(
                $"Stage01 fluorescent lights installed: {fixtures.Length} fixtures, " +
                $"{created} created, {updated} updated.");
        }

        private static bool IsFixtureRoot(GameObject candidate, Scene scene)
        {
            if (candidate.scene != scene ||
                !(string.Equals(candidate.name, FixtureName, StringComparison.Ordinal) ||
                  candidate.name.StartsWith(FixtureName + " (", StringComparison.Ordinal)))
            {
                return false;
            }

            return candidate.GetComponentInChildren<Renderer>(true) != null;
        }

        private static void PositionBelowFixture(GameObject fixture, Transform lightTransform)
        {
            Renderer[] renderers = fixture.GetComponentsInChildren<Renderer>(true);
            Vector3 position = fixture.transform.position + Vector3.down * 0.08f;
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int index = 1; index < renderers.Length; index++)
                {
                    bounds.Encapsulate(renderers[index].bounds);
                }

                position = new Vector3(bounds.center.x, bounds.min.y - 0.12f, bounds.center.z);
            }

            lightTransform.position = position;
            lightTransform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            lightTransform.localScale = Vector3.one;
        }

        private static void ConfigureLight(GameObject lightObject)
        {
            Light light = lightObject.GetComponent<Light>();
            if (light == null)
            {
                light = Undo.AddComponent<Light>(lightObject);
            }

            light.type = LightType.Spot;
            light.color = Color.white;
            light.useColorTemperature = true;
            light.colorTemperature = 4000f;
            light.intensity = 4f;
            light.range = 4.5f;
            light.spotAngle = 160f;
            light.innerSpotAngle = 120f;
            light.bounceIntensity = 0f;
            light.shadows = LightShadows.None;
            light.renderMode = LightRenderMode.Auto;
        }

        private static void ConfigureAmbientLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.72f, 0.65f, 0.54f);
            RenderSettings.ambientEquatorColor = new Color(0.56f, 0.50f, 0.42f);
            RenderSettings.ambientGroundColor = new Color(0.40f, 0.36f, 0.31f);
            RenderSettings.ambientIntensity = 1.55f;
        }
    }
}
