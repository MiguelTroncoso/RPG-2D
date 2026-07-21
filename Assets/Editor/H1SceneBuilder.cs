using System;
using System.Linq;
using Lumbre.Game.Client;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lumbre.Game.Editor
{
    public static class H1SceneBuilder
    {
        private const string SceneDirectory = "Assets/Game/Scenes";
        private const string BootstrapScenePath = SceneDirectory + "/Bootstrap.unity";
        private const string VerticalSliceScenePath = SceneDirectory + "/VerticalSlice.unity";

        public static void Build()
        {
            EnsureFolder("Assets/Game");
            EnsureFolder(SceneDirectory);

            CreateScene("H1_Bootstrap", BootstrapScenePath, "BootstrapScene");
            CreateScene("H1_VerticalSlice_Empty", VerticalSliceScenePath, "VerticalSliceScene");

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapScenePath, true),
                new EditorBuildSettingsScene(VerticalSliceScenePath, true)
            };

            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateScene(string rootName, string path, string sceneMarker)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject(rootName);
            root.AddComponent<GameSceneMarker>().Marker = sceneMarker;

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.055f, 0.09f, 1f);

            AddComponentByTypeName(cameraObject, "Unity.Cinemachine.CinemachineBrain");

            var virtualCameraObject = new GameObject("CM_OfficialCamera");
            virtualCameraObject.transform.position = new Vector3(0f, 0f, -10f);
            AddComponentByTypeName(virtualCameraObject, "Unity.Cinemachine.CinemachineCamera");

            EditorSceneManager.SaveScene(scene, path);
        }

        private static Component AddComponentByTypeName(GameObject gameObject, string fullTypeName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullTypeName, false))
                .FirstOrDefault(candidate => candidate != null);

            if (type == null)
            {
                throw new InvalidOperationException(
                    $"Required package type '{fullTypeName}' was not found. Ensure the official Cinemachine package is resolved.");
            }

            return gameObject.AddComponent(type);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = path.Substring(0, path.LastIndexOf('/'));
            var folder = path.Substring(path.LastIndexOf('/') + 1);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
