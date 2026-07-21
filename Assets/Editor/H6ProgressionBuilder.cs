using System;
using Lumbre.Game.Client;
using Lumbre.Game.Client.Missions;
using Lumbre.Game.Client.Progression;
using Lumbre.Game.Domain.Constants;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Lumbre.Game.Editor
{
    public static class H6ProgressionBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;

        public static void Build()
        {
            H5MissionBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var runtimeObject = FindOrCreate("H6_ProgressionRuntime");
            var runtime = GetOrAdd<H6ProgressionRuntime>(runtimeObject);
            runtime.ConfigureSaveFileName(H6ProgressionRuntime.DefaultSaveFileName);

            BuildProgressionHud(runtime);
            var marker = GameObject.Find("H2_Greybox")?.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H6ProgressionPersistence";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void BuildProgressionHud(H6ProgressionRuntime runtime)
        {
            var canvas = GameObject.Find("H3_MobileControls");
            if (canvas == null)
            {
                throw new InvalidOperationException("H6 requires the H3 mobile controls canvas.");
            }

            var hudObject = canvas.transform.Find("H6_ProgressionHud")?.gameObject;
            if (hudObject == null)
            {
                hudObject = new GameObject("H6_ProgressionHud", typeof(RectTransform), typeof(Text));
                hudObject.transform.SetParent(canvas.transform, false);
            }

            hudObject.layer = ProjectLayers.PlayerUi;
            var rect = hudObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(30f, -255f);
            rect.sizeDelta = new Vector2(1100f, 50f);

            var label = hudObject.GetComponent<Text>();
            label.alignment = TextAnchor.MiddleLeft;
            label.fontSize = 22;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(1f, 0.9f, 0.35f, 1f);
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.raycastTarget = false;

            var hud = GetOrAdd<H6ProgressionHud>(hudObject);
            hud.Configure(runtime, label);
        }

        private static GameObject FindOrCreate(string name)
        {
            var gameObject = GameObject.Find(name);
            return gameObject ?? new GameObject(name);
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }
    }
}
