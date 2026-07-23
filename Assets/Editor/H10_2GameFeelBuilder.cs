using System;
using System.IO;
using System.Linq;
using Lumbre.Game.Client;
using Lumbre.Game.Client.Combat;
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
    /// H10.2 presentation configuration pass. It extends the H10 builder and
    /// only persists design configuration; all sequence state is rebuilt at
    /// runtime.
    /// </summary>
    public static class H10_2GameFeelBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;
        private const string AndroidOutput =
            "Builds/Android/LumbreDeNacar-v0.8.1-H10.2.apk";
        private const float AttackAnticipation = 0.1f;
        private const float AttackRecovery = 0.18f;
        private const float AreaAnticipation = 0.14f;
        private const float AreaRecovery = 0.12f;

        public static void Build()
        {
            H10PlayerControlBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = GameObject.FindWithTag("Player");
            var combat = player?.GetComponent<H4PlayerCombatController>();
            var abilities = player?.GetComponent<H4BPlayerAbilityController>();
            if (player == null || combat == null || abilities == null)
            {
                throw new InvalidOperationException("H10.2 requires the H10 player combat stack.");
            }

            combat.ConfigureTiming(AttackAnticipation, AttackRecovery);
            abilities.ConfigurePresentation(AreaAnticipation, AreaRecovery);

            if (player.GetComponent<H7CharacterView>() == null
                || player.GetComponent<H4CombatFeedback>() == null
                || player.GetComponent<H4BAbilityFeedback>() == null)
            {
                throw new InvalidOperationException(
                    "H10.2 requires the existing character and combat feedback components.");
            }

            var marker = GameObject.Find("H2_Greybox")?.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H10_2GameFeelCombatFeedback";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        public static void Validate()
        {
            H10PlayerControlBuilder.Validate();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = GameObject.FindWithTag("Player");
            var combat = player?.GetComponent<H4PlayerCombatController>();
            var abilities = player?.GetComponent<H4BPlayerAbilityController>();
            var view = player?.GetComponent<H7CharacterView>();
            var feedback = player?.GetComponent<H4CombatFeedback>();
            var abilityFeedback = player?.GetComponent<H4BAbilityFeedback>();
            if (player == null || combat == null || abilities == null || view == null
                || feedback == null || abilityFeedback == null)
            {
                throw new InvalidOperationException("H10.2 player feedback references are incomplete.");
            }

            if (player.GetComponents<H4PlayerCombatController>().Length != 1
                || player.GetComponents<H4BPlayerAbilityController>().Length != 1
                || player.GetComponents<H4CombatFeedback>().Length != 1
                || player.GetComponents<H4BAbilityFeedback>().Length != 1
                || player.GetComponents<H7CharacterView>().Length != 1)
            {
                throw new InvalidOperationException("H10.2 detected duplicated player feedback components.");
            }

            if (combat.AttackAnticipationDuration <= 0f || combat.AttackRecoveryDuration <= 0f
                || abilities.AreaAnticipationDuration <= 0f || abilities.AreaRecoveryDuration <= 0f
                || abilities.AreaRadius <= 0.1f || view.Animator == null
                || view.Animator.runtimeAnimatorController == null)
            {
                throw new InvalidOperationException("H10.2 timing, AOE or animation configuration is invalid.");
            }

            var presentation = GameObject.Find("H7_Presentation")
                ?.GetComponent<H7PresentationRuntime>();
            var camera = GameObject.Find("CM_OfficialCamera")
                ?.GetComponent<H7CameraPolish>();
            if (presentation == null || !presentation.IsConfigured || camera == null
                || !camera.IsConfigured)
            {
                throw new InvalidOperationException("H10.2 presentation or Cinemachine references are incomplete.");
            }

            var inputButtonCount = GameObject.Find("H9_SafeAreaRoot")?.GetComponentsInChildren<
                H10TouchButtonFeedback>(true).Length ?? 0;
            if (inputButtonCount != 5)
            {
                throw new InvalidOperationException(
                    $"H10.2 expected five touch feedback bindings, found {inputButtonCount}.");
            }

            Debug.Log($"H10.2 game feel validation passed in {scene.name}: "
                + $"attack={combat.AttackAnticipationDuration:0.00}/"
                + $"{combat.AttackRecoveryDuration:0.00}, "
                + $"area={abilities.AreaRadius:0.00}/"
                + $"{abilities.AreaAnticipationDuration:0.00}.");
        }

        public static void BuildAndroidDevelopment()
        {
            Build();
            Validate();
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new InvalidOperationException("H10.2 Android build requires enabled scenes.");
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
                        $"H10.2 Android build failed: {report.summary.result} ({report.summary.totalErrors} errors).");
                }

                Debug.Log($"H10.2 Android development build succeeded: {outputPath} "
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
    }
}
