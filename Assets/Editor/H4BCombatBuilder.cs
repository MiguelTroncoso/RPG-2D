using System;
using System.Linq;
using Lumbre.Game.Client;
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
    public static class H4BCombatBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;
        private const string MordeluzMaterialPath =
            "Assets/Game/Greybox/Materials/MordeluzGreyboxMaterial.mat";

        public static void Build()
        {
            H4CombatBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                throw new InvalidOperationException("H4B requires the H4 player in VerticalSlice.unity.");
            }

            var playerHealth = GetOrAdd<H4CombatHealth>(player);
            var playerCombat = GetOrAdd<H4PlayerCombatController>(player);
            var abilityFeedback = GetOrAdd<H4BAbilityFeedback>(player);
            var abilities = GetOrAdd<H4BPlayerAbilityController>(player);
            abilities.Configure(
                player.GetComponent<H3PlayerInputReader>(),
                playerCombat,
                playerHealth,
                abilityFeedback,
                1 << ProjectLayers.Enemy);

            var first = GameObject.Find("Mordeluz") ?? GameObject.Find("Mordeluz_1");
            if (first == null)
            {
                throw new InvalidOperationException("H4B requires the common Mordeluz base.");
            }

            if (first.name == "Mordeluz_1")
            {
                first.name = "Mordeluz";
            }

            BuildCommonEnemy(first, player.transform, 0);
            for (var index = 1; index < ProjectConstants.H4BCommonEnemyCount; index++)
            {
                var enemy = FindOrCreate($"Mordeluz_{index + 1}");
                BuildCommonEnemy(enemy, player.transform, index);
            }

            BuildResonantEnemy(player.transform);
            BuildAbilityButton("H4B_DefenseButton", "DEF", "<Gamepad>/buttonEast",
                new Vector2(-335f, 150f), new Color(0.1f, 0.55f, 0.9f, 0.88f));
            BuildAbilityButton("H4B_AreaButton", "AOE", "<Gamepad>/buttonWest",
                new Vector2(-150f, 335f), new Color(0.75f, 0.12f, 0.65f, 0.88f));
            BuildHeatHud(abilities);

            var marker = GameObject.Find("H2_Greybox")?.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H4BAbilitiesElite";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void BuildCommonEnemy(GameObject enemy, Transform player, int index)
        {
            enemy.layer = ProjectLayers.Enemy;
            var gridX = ProjectConstants.H4BCommonEnemyGridX + index * 3;
            var gridY = ProjectConstants.H4BCommonEnemyGridY;
            enemy.transform.position = GreyboxWorldCoordinates.ToWorld(
                new GridPosition(gridX, gridY), -0.3f);

            ConfigureEnemyPhysics(enemy);
            BuildEnemyVisual(enemy.transform, $"Mordeluz_{index + 1}_Visual", 0.58f);
            var spawn = BuildSpawnPoint(
                $"Mordeluz_{index + 1}_SpawnPoint", enemy.transform.position);
            var health = GetOrAdd<H4CombatHealth>(enemy);
            health.ConfigureMaxHealth(ProjectConstants.MordeluzMaxHealth);
            var attacker = GetOrAdd<H4BasicAttacker>(enemy);
            attacker.Configure(
                ProjectConstants.MordeluzBasicAttackDamage,
                ProjectConstants.MordeluzBasicAttackCooldown,
                $"mordeluz-{index + 1}-bite");
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
        }

        private static void BuildResonantEnemy(Transform player)
        {
            var enemy = FindOrCreate("Mordeluz_Resonante");
            enemy.layer = ProjectLayers.Enemy;
            enemy.transform.position = GreyboxWorldCoordinates.ToWorld(
                new GridPosition(ProjectConstants.H4BResonantGridX,
                    ProjectConstants.H4BResonantGridY), -0.3f);

            ConfigureEnemyPhysics(enemy);
            BuildEnemyVisual(enemy.transform, "Mordeluz_Resonante_Visual", 0.72f);
            var spawn = BuildSpawnPoint("Mordeluz_Resonante_SpawnPoint", enemy.transform.position);
            var health = GetOrAdd<H4CombatHealth>(enemy);
            health.ConfigureMaxHealth(ProjectConstants.MordeluzResonanteMaxHealth);
            var attacker = GetOrAdd<H4BasicAttacker>(enemy);
            attacker.Configure(
                ProjectConstants.MordeluzBasicAttackDamage,
                ProjectConstants.MordeluzBasicAttackCooldown,
                "mordeluz-resonant-basic");
            GetOrAdd<H4CombatFeedback>(enemy);
            var telegraph = GetOrAdd<H4BWaveTelegraph>(enemy);
            var feedback = GetOrAdd<H4BAbilityFeedback>(enemy);
            var controller = GetOrAdd<MordeluzResonanteController>(enemy);
            controller.Configure(
                player,
                spawn.transform,
                health,
                attacker,
                ProjectConstants.MordeluzDetectionRange,
                ProjectConstants.MordeluzAttackRange,
                ProjectConstants.MordeluzLeashRange,
                ProjectConstants.MordeluzMovementSpeed);
            controller.ConfigureElite(
                ProjectConstants.MordeluzResonanteWaveDamage,
                ProjectConstants.MordeluzResonanteWaveRadius,
                ProjectConstants.MordeluzResonanteWaveWindup,
                ProjectConstants.MordeluzResonanteWaveCooldown,
                telegraph,
                feedback);
        }

        private static void ConfigureEnemyPhysics(GameObject enemy)
        {
            var body = GetOrAdd<Rigidbody2D>(enemy);
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = GetOrAdd<CircleCollider2D>(enemy);
            collider.radius = 0.28f;
            collider.isTrigger = false;
        }

        private static void BuildEnemyVisual(Transform enemy, string visualName, float size)
        {
            var visual = enemy.Find(visualName);
            if (visual == null)
            {
                var visualObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                visualObject.name = visualName;
                visualObject.transform.SetParent(enemy, false);
                visual = visualObject.transform;
                var meshCollider = visualObject.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(meshCollider);
                }
            }

            var legacyVisual = enemy.Find("Mordeluz_Visual");
            if (visualName == "Mordeluz_1_Visual" && legacyVisual != null && legacyVisual != visual)
            {
                UnityEngine.Object.DestroyImmediate(legacyVisual.gameObject);
            }

            visual.localPosition = new Vector3(0f, 0f, -0.15f);
            visual.localRotation = Quaternion.identity;
            visual.localScale = new Vector3(size, size, 1f);
            SetLayerRecursive(visual, ProjectLayers.Enemy);
            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateEnemyMaterial();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static Material CreateEnemyMaterial()
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

        private static GameObject BuildSpawnPoint(string name, Vector3 position)
        {
            var spawn = FindOrCreate(name);
            spawn.layer = ProjectLayers.PlayerRespawn;
            spawn.transform.position = position;
            return spawn;
        }

        private static void BuildAbilityButton(string name, string labelText, string controlPath,
            Vector2 position, Color color)
        {
            var canvas = GameObject.Find("H3_MobileControls");
            if (canvas == null)
            {
                throw new InvalidOperationException("H4B requires the H3 mobile controls canvas.");
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
                button,
                "UnityEngine.InputSystem.OnScreen.OnScreenButton");
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
            text.fontSize = 30;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.raycastTarget = false;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void BuildHeatHud(H4BPlayerAbilityController abilities)
        {
            var canvas = GameObject.Find("H3_MobileControls");
            var labelObject = canvas.transform.Find("H4B_HeatHud")?.gameObject;
            if (labelObject == null)
            {
                labelObject = new GameObject("H4B_HeatHud", typeof(RectTransform), typeof(Text));
                labelObject.transform.SetParent(canvas.transform, false);
            }

            labelObject.layer = ProjectLayers.PlayerUi;
            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(30f, -30f);
            rect.sizeDelta = new Vector2(580f, 60f);
            var text = labelObject.GetComponent<Text>();
            text.alignment = TextAnchor.MiddleLeft;
            text.fontSize = 26;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var hud = GetOrAdd<H4BAbilityHud>(labelObject);
            SetPrivateField(hud, "abilities", abilities);
            SetPrivateField(hud, "label", text);
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
                    $"Required H4B component type '{fullTypeName}' was not found.");
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
                    $"H4B component '{component.GetType().Name}' does not expose '{propertyName}'.");
            }

            property.SetValue(component, value);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException(
                    $"H4B component '{target.GetType().Name}' does not expose '{fieldName}'.");
            }

            field.SetValue(target, value);
        }
    }
}
