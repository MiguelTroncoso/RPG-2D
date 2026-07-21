using System;
using System.Linq;
using Lumbre.Game.Client;
using Lumbre.Game.Client.Camera;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Client.World;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Lumbre.Game.Editor
{
    public static class H3PlayerBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;
        private const string GreyboxMaterialPath = "Assets/Game/Greybox/Materials/PlayerGreyboxMaterial.mat";
        private const string InputActionsPath = "Assets/Game/Client/Input/PlayerInputActions.inputactions";

        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var greyboxRoot = GameObject.Find("H2_Greybox");
            if (greyboxRoot == null)
            {
                throw new InvalidOperationException("H3 requires the H2 greybox root in VerticalSlice.unity.");
            }

            ConfigureOfficialLayers(greyboxRoot);
            AddGreyboxGizmos(greyboxRoot);
            var player = BuildPlayer();
            BuildCamera(player.transform);
            BuildMobileControls();

            var marker = greyboxRoot.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H3PlayerControl";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void ConfigureOfficialLayers(GameObject greyboxRoot)
        {
            SetLayerRecursive(greyboxRoot.transform.Find("Zone_Plaza"), ProjectLayers.WorldGround);
            SetLayerRecursive(greyboxRoot.transform.Find("Zone_Sendero"), ProjectLayers.WorldGround);
            SetLayerRecursive(greyboxRoot.transform.Find("Zone_Cueva"), ProjectLayers.WorldGround);
            SetLayerRecursive(greyboxRoot.transform.Find("H2_Collisions"), ProjectLayers.WorldObstacle);
            SetLayerRecursive(greyboxRoot.transform.Find("H2_NavigationPreview"), ProjectLayers.NavigationDebug);
        }

        private static void AddGreyboxGizmos(GameObject greyboxRoot)
        {
            if (greyboxRoot.GetComponent<GreyboxDebugGizmos>() == null)
            {
                greyboxRoot.AddComponent<GreyboxDebugGizmos>();
            }
        }

        private static GameObject BuildPlayer()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = new GameObject("Player_Avatar");
            }

            player.tag = "Player";
            SetLayerRecursive(player.transform, ProjectLayers.Player);
            player.transform.position = GetStartWorldPosition(-0.25f);

            var body = player.GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = player.AddComponent<Rigidbody2D>();
            }

            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = player.GetComponent<CircleCollider2D>();
            if (collider == null)
            {
                collider = player.AddComponent<CircleCollider2D>();
            }

            collider.radius = 0.2f;
            collider.isTrigger = false;

            var inputReader = player.GetComponent<H3PlayerInputReader>();
            if (inputReader == null)
            {
                inputReader = player.AddComponent<H3PlayerInputReader>();
            }

            inputReader.ActionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);

            var respawn = BuildRespawnPoint();
            var controller = player.GetComponent<H3PlayerController>();
            if (controller == null)
            {
                controller = player.AddComponent<H3PlayerController>();
            }

            controller.InputReader = inputReader;
            controller.RespawnPoint = respawn.transform;

            BuildPlayerVisual(player.transform);
            return player;
        }

        private static GameObject BuildRespawnPoint()
        {
            var respawn = GameObject.Find("Player_RespawnPoint");
            if (respawn == null)
            {
                respawn = new GameObject("Player_RespawnPoint");
            }

            respawn.layer = ProjectLayers.PlayerRespawn;
            respawn.transform.position = GetStartWorldPosition(-0.2f);
            if (respawn.GetComponent<H3RespawnPoint>() == null)
            {
                respawn.AddComponent<H3RespawnPoint>();
            }

            return respawn;
        }

        private static void BuildPlayerVisual(Transform player)
        {
            var visual = player.Find("Player_Visual");
            if (visual == null)
            {
                var visualObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                visualObject.name = "Player_Visual";
                visualObject.transform.SetParent(player, false);
                visual = visualObject.transform;
                var meshCollider = visualObject.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(meshCollider);
                }
            }

            visual.localPosition = new Vector3(0f, 0f, -0.1f);
            visual.localRotation = Quaternion.identity;
            visual.localScale = new Vector3(0.42f, 0.42f, 1f);
            SetLayerRecursive(visual, ProjectLayers.Player);

            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreatePlayerMaterial();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        private static Material CreatePlayerMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(GreyboxMaterialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default"))
                {
                    name = "PlayerGreyboxMaterial"
                };
                AssetDatabase.CreateAsset(material, GreyboxMaterialPath);
            }

            material.color = new Color(1f, 0.86f, 0.18f, 1f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildCamera(Transform player)
        {
            var cameraObject = GameObject.Find("CM_OfficialCamera");
            if (cameraObject == null)
            {
                throw new InvalidOperationException("H3 requires CM_OfficialCamera from H2.");
            }

            var followTarget = cameraObject.GetComponent<H3CinemachineFollowTarget>()
                ?? cameraObject.AddComponent<H3CinemachineFollowTarget>();
            followTarget.Target = player;

            var follow = AddComponentByTypeName(cameraObject, "Unity.Cinemachine.CinemachineFollow");
            var offsetField = follow.GetType().GetField("FollowOffset");
            offsetField?.SetValue(follow, new Vector3(0f, 0f, -20f));
        }

        private static void BuildMobileControls()
        {
            var eventSystem = GameObject.Find("EventSystem");
            if (eventSystem == null)
            {
                eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
            }

            if (eventSystem.GetComponent("UnityEngine.InputSystem.UI.InputSystemUIInputModule") == null)
            {
                AddComponentByTypeName(eventSystem, "UnityEngine.InputSystem.UI.InputSystemUIInputModule");
            }

            var canvasObject = GameObject.Find("H3_MobileControls");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("H3_MobileControls");
                canvasObject.AddComponent<RectTransform>();
                var canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                var scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            var canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasObject.layer = ProjectLayers.PlayerUi;
            var baseObject = canvasObject.transform.Find("VirtualJoystick_Base")?.gameObject;
            if (baseObject == null)
            {
                baseObject = CreateUiObject(
                    "VirtualJoystick_Base", canvasRect, new Vector2(210f, 210f), new Vector2(150f, 150f));
                var baseImage = baseObject.GetComponent<Image>();
                baseImage.color = new Color(0.08f, 0.12f, 0.2f, 0.5f);
                baseImage.sprite = GetUiSprite();
                baseImage.raycastTarget = false;

                var handle = CreateUiObject(
                    "VirtualJoystick_Handle",
                    baseObject.GetComponent<RectTransform>(),
                    new Vector2(110f, 110f),
                    Vector2.zero);
                var handleImage = handle.GetComponent<Image>();
                handleImage.color = new Color(0.22f, 0.8f, 1f, 0.75f);
                handleImage.sprite = GetUiSprite();
                handleImage.raycastTarget = true;

                var stick = AddComponentByTypeName(handle, "UnityEngine.InputSystem.OnScreen.OnScreenStick");
                SetProperty(stick, "controlPath", "<Gamepad>/leftStick");
                SetProperty(stick, "movementRange", 72f);
                SetProperty(stick, "behaviour", Enum.Parse(
                    stick.GetType().GetNestedType("Behaviour"), "RelativePositionWithStaticOrigin"));
                SetProperty(stick, "useIsolatedInputActions", true);
            }

            var baseRect = baseObject.GetComponent<RectTransform>();
            baseRect.anchoredPosition = new Vector2(150f, 150f);
            var existingHandle = baseObject.transform.Find("VirtualJoystick_Handle");
            if (existingHandle != null)
            {
                existingHandle.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }

        private static GameObject CreateUiObject(
            string name,
            RectTransform parent,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            gameObject.layer = ProjectLayers.PlayerUi;
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return gameObject;
        }

        private static Sprite GetUiSprite()
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        }

        private static Vector3 GetStartWorldPosition(float z)
        {
            return GreyboxWorldCoordinates.ToWorld(
                new GridPosition(ProjectConstants.NavigationStartX, ProjectConstants.NavigationStartY), z);
        }

        private static void SetLayerRecursive(Transform root, int layer)
        {
            if (root == null)
            {
                return;
            }

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
                    $"Required H3 component type '{fullTypeName}' was not found.");
            }

            return gameObject.GetComponent(type) ?? gameObject.AddComponent(type);
        }

        private static void SetProperty(Component component, string propertyName, object value)
        {
            var property = component.GetType().GetProperty(propertyName);
            if (property == null || !property.CanWrite)
            {
                throw new InvalidOperationException(
                    $"H3 component '{component.GetType().Name}' does not expose '{propertyName}'.");
            }

            property.SetValue(component, value);
        }
    }
}
