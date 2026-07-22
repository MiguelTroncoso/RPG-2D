using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lumbre.Game.Client;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Client.Presentation;
using Lumbre.Game.Domain.Constants;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Lumbre.Game.Editor
{
    /// <summary>
    /// H10 pass for the existing vertical slice. It is intentionally
    /// idempotent: no gameplay object is duplicated and all input remains on
    /// the existing PlayerInputActions asset.
    /// </summary>
    public static class H10PlayerControlBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;
        private const string AndroidOutput =
            "Builds/Android/LumbreDeNacar-v0.8.1-H10.apk";

        public static void Build()
        {
            H9VerticalSlicePolishBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = GameObject.FindWithTag("Player");
            var controller = player?.GetComponent<H3PlayerController>();
            if (player == null || controller == null)
            {
                throw new InvalidOperationException("H10 requires the existing H3 player.");
            }

            controller.ConfigureLocomotion(2.4f, 64f, 128f, 0.12f, 1f);
            GetOrAdd<H10PlayerActionStateController>(player);

            var safeRoot = GameObject.Find("H9_SafeAreaRoot")?.transform;
            if (safeRoot == null)
            {
                throw new InvalidOperationException("H10 requires the H9 safe area root.");
            }

            foreach (var controlName in new[]
            {
                "H4_AttackButton", "H4B_DefenseButton", "H4B_AreaButton",
                "H5_InteractButton", "H5_EquipButton"
            })
            {
                var control = FindChild(controlName, safeRoot);
                if (control != null)
                {
                    GetOrAdd<H10TouchButtonFeedback>(control.gameObject);
                }
            }

            var marker = GameObject.Find("H2_Greybox")?.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H10PlayerControlAndTouchCombat";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        public static void Validate()
        {
            H9VerticalSlicePolishBuilder.Validate();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = GameObject.FindWithTag("Player");
            var input = player?.GetComponent<H3PlayerInputReader>();
            var controller = player?.GetComponent<H3PlayerController>();
            var actionState = player?.GetComponent<H10PlayerActionStateController>();
            if (player == null || input == null || controller == null || actionState == null)
            {
                throw new InvalidOperationException("H10 player control components are incomplete.");
            }

            var safeRoot = GameObject.Find("H9_SafeAreaRoot")?.transform;
            var expectedControls = new Dictionary<string, string>
            {
                ["H4_AttackButton"] = "<Gamepad>/buttonSouth",
                ["H4B_DefenseButton"] = "<Gamepad>/buttonEast",
                ["H4B_AreaButton"] = "<Gamepad>/buttonWest",
                ["H5_InteractButton"] = "<Gamepad>/buttonNorth",
                ["H5_EquipButton"] = "<Gamepad>/leftShoulder"
            };
            foreach (var expected in expectedControls)
            {
                var control = FindChild(expected.Key, safeRoot);
                var onScreen = control?.GetComponent(
                    "UnityEngine.InputSystem.OnScreen.OnScreenButton");
                var path = onScreen?.GetType().GetProperty("controlPath")?.GetValue(onScreen)
                    as string;
                if (control == null || control.GetComponent<H10TouchButtonFeedback>() == null
                    || !string.Equals(path, expected.Value, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"H10 control '{expected.Key}' is not linked to '{expected.Value}'.");
                }
            }

            Debug.Log($"H10 player control validation passed in {scene.name}: "
                + $"speed={controller.MaxSpeed:0.00}, deadZone={controller.InputDeadZone:0.00}, "
                + $"response={controller.AnalogResponse:0.00}.");
        }

        public static void BuildAndroidDevelopment()
        {
            Build();
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("H10 Android build requires enabled scenes.");
            }

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            var outputPath = Path.Combine(projectRoot ?? string.Empty, AndroidOutput);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var originalArchitectures = PlayerSettings.Android.targetArchitectures;
            var originalUseDefaultGraphics = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android);
            var originalGraphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
            try
            {
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
                    new[] { GraphicsDeviceType.OpenGLES3 });
                var options = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = outputPath,
                    target = BuildTarget.Android,
                    options = BuildOptions.Development | BuildOptions.AllowDebugging
                        | BuildOptions.ConnectWithProfiler | BuildOptions.CleanBuildCache
                };
                var report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"H10 Android build failed: {report.summary.result} ({report.summary.totalErrors} errors).");
                }

                Debug.Log($"H10 Android development build succeeded: {outputPath} "
                    + $"({report.summary.totalSize} bytes).");
            }
            finally
            {
                PlayerSettings.Android.targetArchitectures = originalArchitectures;
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, originalUseDefaultGraphics);
                if (originalUseDefaultGraphics)
                {
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, Array.Empty<GraphicsDeviceType>());
                }
                else if (originalGraphics != null && originalGraphics.Length > 0)
                {
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, originalGraphics);
                }

                AssetDatabase.SaveAssets();
            }
        }

        private static Transform FindChild(string name, Transform root)
        {
            if (root == null)
            {
                return null;
            }

            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(candidate => candidate.name == name);
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
