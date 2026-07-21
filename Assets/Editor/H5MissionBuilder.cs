using System;
using System.Linq;
using Lumbre.Game.Client;
using Lumbre.Game.Client.Missions;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Client.World;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Events;
using Lumbre.Game.Domain.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Lumbre.Game.Editor
{
    public static class H5MissionBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;
        private const string NaraMaterialPath =
            "Assets/Game/Greybox/Materials/NaraVelaquietaGreyboxMaterial.mat";

        public static void Build()
        {
            H4BCombatBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = GameObject.FindWithTag("Player");
            var inputReader = player?.GetComponent<H3PlayerInputReader>();
            if (player == null || inputReader == null)
            {
                throw new InvalidOperationException("H5 requires the H4B player and input reader.");
            }

            var runtime = FindOrCreate("H5_MissionRuntime");
            var missionRuntime = GetOrAdd<H5MissionRuntime>(runtime);
            missionRuntime.ConfigureInventoryCapacity(ProjectConstants.H5InventoryCapacity);

            var nara = BuildNara(missionRuntime);
            var playerMission = GetOrAdd<H5PlayerMissionController>(player);
            playerMission.Configure(inputReader, missionRuntime, nara);

            ConfigureCombatEventBridge("Mordeluz", missionRuntime, CombatantKind.Mordeluz);
            ConfigureCombatEventBridge("Mordeluz_2", missionRuntime, CombatantKind.Mordeluz);
            ConfigureCombatEventBridge("Mordeluz_3", missionRuntime, CombatantKind.Mordeluz);
            ConfigureCombatEventBridge(
                "Mordeluz_Resonante", missionRuntime, CombatantKind.MordeluzResonante);

            BuildInteractionButton("H5_InteractButton", "HABLAR", "<Gamepad>/buttonNorth",
                new Vector2(-335f, 335f), new Color(0.12f, 0.65f, 0.85f, 0.88f));
            BuildInteractionButton("H5_EquipButton", "EQUIP", "<Gamepad>/leftShoulder",
                new Vector2(-520f, 335f), new Color(0.55f, 0.28f, 0.85f, 0.88f));
            BuildMissionHud(missionRuntime, nara, player.transform);

            var marker = GameObject.Find("H2_Greybox")?.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H5MissionReward";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static H5NaraController BuildNara(H5MissionRuntime runtime)
        {
            var nara = FindOrCreate("Nara_Velaquieta");
            nara.layer = ProjectLayers.WorldGround;
            nara.transform.position = GreyboxWorldCoordinates.ToWorld(
                new GridPosition(ProjectConstants.H5NaraGridX, ProjectConstants.H5NaraGridY),
                -0.3f);

            var controller = GetOrAdd<H5NaraController>(nara);
            controller.Configure(runtime, ProjectConstants.H5InteractionRange);
            BuildNaraVisual(nara.transform);
            return controller;
        }

        private static void BuildNaraVisual(Transform nara)
        {
            var visual = nara.Find("Nara_Velaquieta_Visual");
            if (visual == null)
            {
                var visualObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                visualObject.name = "Nara_Velaquieta_Visual";
                visualObject.transform.SetParent(nara, false);
                visual = visualObject.transform;
                var meshCollider = visualObject.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(meshCollider);
                }
            }

            visual.localPosition = new Vector3(0f, 0f, -0.16f);
            visual.localRotation = Quaternion.identity;
            visual.localScale = new Vector3(0.5f, 0.72f, 1f);
            SetLayerRecursive(visual, ProjectLayers.WorldGround);
            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateNaraMaterial();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static Material CreateNaraMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(NaraMaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default"))
                {
                    name = "NaraVelaquietaGreyboxMaterial"
                };
                AssetDatabase.CreateAsset(material, NaraMaterialPath);
            }

            material.color = new Color(0.2f, 0.78f, 0.92f, 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ConfigureCombatEventBridge(
            string enemyName, H5MissionRuntime runtime, CombatantKind kind)
        {
            var enemy = GameObject.Find(enemyName);
            if (enemy == null)
            {
                throw new InvalidOperationException($"H5 requires combat target '{enemyName}'.");
            }

            var bridge = GetOrAdd<H5CombatEventBridge>(enemy);
            bridge.Configure(runtime, kind);
        }

        private static void BuildInteractionButton(string name, string labelText, string controlPath,
            Vector2 position, Color color)
        {
            var canvas = GameObject.Find("H3_MobileControls");
            if (canvas == null)
            {
                throw new InvalidOperationException("H5 requires the H3 mobile controls canvas.");
            }

            var button = canvas.transform.Find(name)?.gameObject;
            if (button == null)
            {
                button = new GameObject(name, typeof(RectTransform), typeof(Image));
                button.transform.SetParent(canvas.transform, false);
            }

            button.layer = ProjectLayers.PlayerUi;
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(165f, 165f);

            var image = button.GetComponent<Image>();
            image.color = color;
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.raycastTarget = true;

            var onScreenButton = AddComponentByTypeName(
                button, "UnityEngine.InputSystem.OnScreen.OnScreenButton");
            SetProperty(onScreenButton, "controlPath", controlPath);

            var label = button.transform.Find("Label")?.gameObject;
            if (label == null)
            {
                label = new GameObject("Label", typeof(RectTransform), typeof(Text));
                label.transform.SetParent(button.transform, false);
            }

            label.layer = ProjectLayers.PlayerUi;
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var text = label.GetComponent<Text>();
            text.text = labelText;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 28;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void BuildMissionHud(
            H5MissionRuntime runtime, H5NaraController nara, Transform player)
        {
            var canvas = GameObject.Find("H3_MobileControls");
            if (canvas == null)
            {
                throw new InvalidOperationException("H5 requires the H3 mobile controls canvas.");
            }

            var hudObject = canvas.transform.Find("H5_MissionHud")?.gameObject;
            if (hudObject == null)
            {
                hudObject = new GameObject("H5_MissionHud", typeof(RectTransform), typeof(Text));
                hudObject.transform.SetParent(canvas.transform, false);
            }

            hudObject.layer = ProjectLayers.PlayerUi;
            var rect = hudObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(30f, -100f);
            rect.sizeDelta = new Vector2(1750f, 145f);

            var label = hudObject.GetComponent<Text>();
            label.alignment = TextAnchor.UpperLeft;
            label.fontSize = 20;
            label.fontStyle = FontStyle.Bold;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.raycastTarget = false;

            var hud = GetOrAdd<H5MissionHud>(hudObject);
            hud.Configure(runtime, nara, player, label, player.GetComponent<H3PlayerInputReader>());
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

        private static void SetLayerRecursive(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            foreach (Transform child in root)
            {
                SetLayerRecursive(child, layer);
            }
        }

        private static Component AddComponentByTypeName(GameObject gameObject, string fullTypeName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullTypeName, false))
                .FirstOrDefault(candidate => candidate != null);
            if (type == null)
            {
                throw new InvalidOperationException(
                    $"Required H5 component type '{fullTypeName}' was not found.");
            }

            var component = gameObject.GetComponent(type);
            return component ?? gameObject.AddComponent(type);
        }

        private static void SetProperty(Component component, string propertyName, object value)
        {
            var property = component.GetType().GetProperty(propertyName);
            if (property == null || !property.CanWrite)
            {
                throw new InvalidOperationException(
                    $"H5 component '{component.GetType().Name}' does not expose '{propertyName}'.");
            }

            property.SetValue(component, value);
        }
    }
}
