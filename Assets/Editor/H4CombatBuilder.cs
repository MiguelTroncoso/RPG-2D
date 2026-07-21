using System;
using Lumbre.Game.Client;
using System.Linq;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Client.World;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Lumbre.Game.Editor
{
    public static class H4CombatBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;
        private const string MordeluzMaterialPath =
            "Assets/Game/Greybox/Materials/MordeluzGreyboxMaterial.mat";

        public static void Build()
        {
            H3PlayerBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                throw new InvalidOperationException("H4 requires the H3 player in VerticalSlice.unity.");
            }

            var playerHealth = GetOrAdd<H4CombatHealth>(player);
            playerHealth.ConfigureMaxHealth(ProjectConstants.PlayerCombatMaxHealth);
            var playerAttacker = GetOrAdd<H4BasicAttacker>(player);
            playerAttacker.Configure(
                ProjectConstants.PlayerBasicAttackDamage,
                ProjectConstants.PlayerBasicAttackCooldown,
                "player-basic-attack");
            GetOrAdd<H4CombatFeedback>(player);

            var enemy = BuildMordeluz(player.transform, playerHealth);
            BuildAudioListener();
            var playerCombat = GetOrAdd<H4PlayerCombatController>(player);
            playerCombat.Configure(
                player.GetComponent<H3PlayerInputReader>(),
                playerAttacker,
                playerHealth,
                1 << ProjectLayers.Enemy,
                ProjectConstants.PlayerBasicAttackRange);
            BuildAttackButton();

            var marker = GameObject.Find("H2_Greybox")?.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H4CombatPrototype";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);

            if (enemy == null)
            {
                throw new InvalidOperationException("H4 failed to create Mordeluz.");
            }
        }

        private static GameObject BuildMordeluz(Transform player, H4CombatHealth playerHealth)
        {
            var enemy = GameObject.Find("Mordeluz") ?? GameObject.Find("Mordeluz_1");
            if (enemy == null)
            {
                enemy = new GameObject("Mordeluz");
            }

            enemy.layer = ProjectLayers.Enemy;
            enemy.transform.position = GreyboxWorldCoordinates.ToWorld(
                new GridPosition(16, 10), -0.3f);

            var body = GetOrAdd<Rigidbody2D>(enemy);
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = GetOrAdd<CircleCollider2D>(enemy);
            collider.radius = 0.28f;
            collider.isTrigger = false;

            BuildMordeluzVisual(enemy.transform);
            var spawn = BuildMordeluzSpawn(enemy.transform.position);
            var health = GetOrAdd<H4CombatHealth>(enemy);
            health.ConfigureMaxHealth(ProjectConstants.MordeluzMaxHealth);
            var attacker = GetOrAdd<H4BasicAttacker>(enemy);
            attacker.Configure(
                ProjectConstants.MordeluzBasicAttackDamage,
                ProjectConstants.MordeluzBasicAttackCooldown,
                "mordeluz-bite");
            GetOrAdd<H4CombatFeedback>(enemy);

            var controller = GetOrAdd<MordeluzController>(enemy);
            controller.Configure(
                player,
                spawn.transform,
                health,
                attacker,
                ProjectConstants.MordeluzDetectionRange,
                ProjectConstants.MordeluzAttackRange,
                ProjectConstants.MordeluzLeashRange,
                ProjectConstants.MordeluzMovementSpeed);

            return enemy;
        }

        private static GameObject BuildMordeluzSpawn(Vector3 position)
        {
            var spawn = GameObject.Find("Mordeluz_SpawnPoint");
            if (spawn == null)
            {
                spawn = new GameObject("Mordeluz_SpawnPoint");
            }

            spawn.layer = ProjectLayers.PlayerRespawn;
            spawn.transform.position = position;
            return spawn;
        }

        private static void BuildMordeluzVisual(Transform enemy)
        {
            var visual = enemy.Find("Mordeluz_Visual");
            if (visual == null)
            {
                var visualObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                visualObject.name = "Mordeluz_Visual";
                visualObject.transform.SetParent(enemy, false);
                visual = visualObject.transform;
                var meshCollider = visualObject.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(meshCollider);
                }
            }

            visual.localPosition = new Vector3(0f, 0f, -0.15f);
            visual.localRotation = Quaternion.identity;
            visual.localScale = new Vector3(0.58f, 0.58f, 1f);
            SetLayerRecursive(visual, ProjectLayers.Enemy);

            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateMordeluzMaterial();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static Material CreateMordeluzMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MordeluzMaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default"))
                {
                    name = "MordeluzGreyboxMaterial"
                };
                AssetDatabase.CreateAsset(material, MordeluzMaterialPath);
            }

            material.color = new Color(0.78f, 0.15f, 0.55f, 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildAttackButton()
        {
            var canvas = GameObject.Find("H3_MobileControls");
            if (canvas == null)
            {
                throw new InvalidOperationException("H4 requires the H3 mobile controls canvas.");
            }

            var button = canvas.transform.Find("H4_AttackButton")?.gameObject;
            if (button == null)
            {
                button = new GameObject("H4_AttackButton", typeof(RectTransform), typeof(Image));
                button.transform.SetParent(canvas.transform, false);
            }

            button.layer = ProjectLayers.PlayerUi;
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-150f, 150f);
            rect.sizeDelta = new Vector2(165f, 165f);

            var image = button.GetComponent<Image>();
            image.color = new Color(0.8f, 0.17f, 0.22f, 0.88f);
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.raycastTarget = true;

            var onScreenButton = AddComponentByTypeName(
                button,
                "UnityEngine.InputSystem.OnScreen.OnScreenButton");
            SetProperty(onScreenButton, "controlPath", "<Gamepad>/buttonSouth");

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
            text.text = "ATK";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 30;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void BuildAudioListener()
        {
            var camera = GameObject.Find("Main Camera");
            if (camera != null)
            {
                GetOrAdd<AudioListener>(camera);
            }
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
                    $"Required H4 component type '{fullTypeName}' was not found.");
            }

            return gameObject.GetComponent(type) ?? gameObject.AddComponent(type);
        }

        private static void SetProperty(Component component, string propertyName, object value)
        {
            var property = component.GetType().GetProperty(propertyName);
            if (property == null || !property.CanWrite)
            {
                throw new InvalidOperationException(
                    $"H4 component '{component.GetType().Name}' does not expose '{propertyName}'.");
            }

            property.SetValue(component, value);
        }
    }
}
