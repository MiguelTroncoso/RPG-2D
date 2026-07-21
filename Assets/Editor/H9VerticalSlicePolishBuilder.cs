using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lumbre.Game.Client;
using Lumbre.Game.Client.Camera;
using Lumbre.Game.Client.Presentation;
using Lumbre.Game.Domain.Constants;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Lumbre.Game.Editor
{
    /// <summary>
    /// Idempotent H9 presentation pass. It only changes presentation objects,
    /// camera framing and platform-facing UI layout in the existing slice.
    /// </summary>
    public static class H9VerticalSlicePolishBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;
        private const float CameraZoom = 8.2f;
        private const float SafeAreaPadding = 18f;
        private const float BackdropWidth = 42f;
        private const float JoystickTouchSize = 228f;
        private const float JoystickVisualSize = 168f;

        public static void Build()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvasObject = GameObject.Find("H3_MobileControls");
            var canvas = canvasObject?.GetComponent<Canvas>();
            if (canvas == null)
            {
                throw new InvalidOperationException("H9 requires H3_MobileControls.");
            }

            PlayerSettings.bundleVersion = "0.8.1 Alpha";
            ConfigureCanvas(canvas);
            var safeRoot = ConfigureSafeArea(canvas.transform);
            ConfigureControls(safeRoot.transform);
            ConfigureHud(safeRoot.transform);
            ConfigureMenus(safeRoot.transform);
            ConfigureCamera();
            ConfigureBackdrop();

            var marker = GameObject.Find("H2_Greybox")?.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H9VerticalSlicePolish";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        public static void Validate()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("H3_MobileControls")?.GetComponent<Canvas>();
            var safeRoot = FindUi("H9_SafeAreaRoot", canvas?.transform);
            var camera = GameObject.Find("CM_OfficialCamera");
            var confiner = camera?.GetComponent<CinemachineConfiner2D>();
            var bounds = GameObject.Find("H9_CameraBounds")?.GetComponent<Collider2D>();
            if (canvas == null || safeRoot == null || safeRoot.GetComponent<H9SafeAreaLayout>() == null
                || camera == null || confiner == null || confiner.BoundingShape2D != bounds)
            {
                throw new InvalidOperationException("H9 presentation validation failed.");
            }

            ValidateLayout(canvas, safeRoot.transform);
            ValidateSpawns(bounds);

            Debug.Log($"H9 presentation validation passed in {scene.name}.");
        }

        private static void ValidateLayout(Canvas canvas, Transform root)
        {
            var requiredUi = new[]
            {
                "VirtualJoystick_Base", "H4_AttackButton", "H4B_DefenseButton", "H4B_AreaButton",
                "H5_InteractButton", "H5_EquipButton", "H7_StatusPanel", "H7_MissionPanel",
                "H5_MissionHud"
            };
            foreach (var name in requiredUi)
            {
                if (CountUi(name, root) != 1)
                {
                    throw new InvalidOperationException($"H9 layout contains an invalid '{name}' count.");
                }
            }

            var camera = GameObject.Find("Main Camera")?.GetComponent<Camera>();
            if (camera == null || camera.rect != new Rect(0f, 0f, 1f, 1f))
            {
                throw new InvalidOperationException("H9 camera viewport must fill the screen.");
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null || scaler.referenceResolution != new Vector2(1920f, 1080f))
            {
                throw new InvalidOperationException("H9 layout requires the 1920x1080 reference canvas.");
            }

            var resolutions = new[]
            {
                new Vector2(1920f, 1080f),
                new Vector2(2400f, 1080f),
                new Vector2(2340f, 1080f),
                new Vector2(2560f, 1080f),
                new Vector2(1280f, 720f)
            };
            foreach (var resolution in resolutions)
            {
                var scale = CalculateCanvasScale(resolution, scaler);
                var safeSize = resolution / scale - Vector2.one * (SafeAreaPadding * 2f);
                ValidateTopRight(root, "H4_AttackButton", new Vector2(-120f, -112f), safeSize,
                    resolution);
                ValidateTopRight(root, "H4B_DefenseButton", new Vector2(-284f, -112f), safeSize,
                    resolution);
                ValidateTopRight(root, "H4B_AreaButton", new Vector2(-120f, -276f), safeSize,
                    resolution);
                ValidateTopRight(root, "H5_InteractButton", new Vector2(-284f, -276f), safeSize,
                    resolution);
                ValidateTopRight(root, "H5_EquipButton", new Vector2(-448f, -276f), safeSize,
                    resolution);
                ValidateBottomLeft(root, "VirtualJoystick_Base", new Vector2(150f, 150f),
                    new Vector2(JoystickTouchSize, JoystickTouchSize), safeSize, resolution);
                ValidateTopLeft(root, "H7_StatusPanel", new Vector2(24f, -24f),
                    new Vector2(610f, 146f), safeSize, resolution);
                ValidateTopLeft(root, "H7_MissionPanel", new Vector2(24f, -188f),
                    new Vector2(650f, 70f), safeSize, resolution);
                ValidateTopLeft(root, "H5_MissionHud", new Vector2(24f, -270f),
                    new Vector2(700f, 48f), safeSize, resolution);
            }
        }

        private static void ValidateSpawns(Collider2D cameraBounds)
        {
            var spawnNames = new[]
            {
                "Player_RespawnPoint", "Mordeluz_1_SpawnPoint", "Mordeluz_2_SpawnPoint",
                "Mordeluz_3_SpawnPoint", "Mordeluz_Resonante_SpawnPoint"
            };
            var obstacleMask = 1 << ProjectLayers.WorldObstacle;
            foreach (var name in spawnNames)
            {
                var spawn = GameObject.Find(name);
                if (spawn == null)
                {
                    throw new InvalidOperationException($"H9 spawn '{name}' is missing.");
                }

                var point = spawn.transform.position;
                var cameraBoundsWorld = cameraBounds.bounds;
                var insideCameraBounds = point.x >= cameraBoundsWorld.min.x
                    && point.x <= cameraBoundsWorld.max.x
                    && point.y >= cameraBoundsWorld.min.y
                    && point.y <= cameraBoundsWorld.max.y;
                if (!insideCameraBounds)
                {
                    throw new InvalidOperationException($"H9 spawn '{name}' is outside camera bounds.");
                }

                var overlapsObstacle = Physics2D.OverlapCircleAll(point, 0.2f, obstacleMask);
                if (overlapsObstacle.Length > 0)
                {
                    throw new InvalidOperationException($"H9 spawn '{name}' overlaps a world obstacle.");
                }
            }
        }

        private static void ValidateTopRight(Transform root, string name, Vector2 position,
            Vector2 safeSize, Vector2 resolution)
        {
            var rect = FindUi(name, root)?.GetComponent<RectTransform>();
            if (rect == null)
            {
                throw new InvalidOperationException($"H9 layout element '{name}' is missing.");
            }

            var right = safeSize.x + position.x;
            var top = safeSize.y + position.y;
            if (right - rect.sizeDelta.x < 0f || top - rect.sizeDelta.y < 0f
                || right > safeSize.x || top > safeSize.y)
            {
                throw new InvalidOperationException(
                    $"H9 '{name}' leaves the safe area at {resolution.x:0}x{resolution.y:0}.");
            }
        }

        private static void ValidateBottomLeft(Transform root, string name, Vector2 position,
            Vector2 size, Vector2 safeSize, Vector2 resolution)
        {
            if (position.x < 0f || position.y < 0f || position.x + size.x > safeSize.x
                || position.y + size.y > safeSize.y)
            {
                throw new InvalidOperationException(
                    $"H9 '{name}' leaves the safe area at {resolution.x:0}x{resolution.y:0}.");
            }
        }

        private static void ValidateTopLeft(Transform root, string name, Vector2 position,
            Vector2 size, Vector2 safeSize, Vector2 resolution)
        {
            if (position.x < 0f || safeSize.y + position.y - size.y < 0f
                || position.x + size.x > safeSize.x)
            {
                throw new InvalidOperationException(
                    $"H9 '{name}' leaves the safe area at {resolution.x:0}x{resolution.y:0}.");
            }
        }

        private static float CalculateCanvasScale(Vector2 resolution, CanvasScaler scaler)
        {
            var widthScale = resolution.x / scaler.referenceResolution.x;
            var heightScale = resolution.y / scaler.referenceResolution.y;
            return Mathf.Pow(widthScale, 1f - scaler.matchWidthOrHeight)
                * Mathf.Pow(heightScale, scaler.matchWidthOrHeight);
        }

        private static int CountUi(string name, Transform root)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .Count(transform => transform.name == name);
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
                throw new InvalidOperationException("H9 Android build requires enabled scenes.");
            }

            const string outputPath = "/tmp/lumbre-h9-android.apk";
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
                        $"H9 Android build failed: {report.summary.result} ({report.summary.totalErrors} errors).");
                }

                Debug.Log($"H9 Android development build succeeded: {outputPath} "
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

        public static void CaptureAndroidLayoutPreview()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var canvas = GameObject.Find("H3_MobileControls")?.GetComponent<Canvas>();
            var camera = Camera.main ?? UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (canvas == null || camera == null)
            {
                throw new InvalidOperationException("H9 preview requires the scene Canvas and main camera.");
            }

            var renderTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            var preview = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
            var originalMode = canvas.renderMode;
            var originalCanvasCamera = canvas.worldCamera;
            var originalPlaneDistance = canvas.planeDistance;
            var originalTarget = camera.targetTexture;
            var originalAspect = camera.aspect;
            var originalClearFlags = camera.clearFlags;
            var originalBackground = camera.backgroundColor;

            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 10f;
                camera.targetTexture = renderTexture;
                camera.aspect = 1920f / 1080f;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.Render();

                RenderTexture.active = renderTexture;
                preview.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
                preview.Apply();

                var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
                var outputPath = projectRoot == null
                    ? "docs/captures/h9-android-layout-preview.png"
                    : Path.Combine(projectRoot, "docs/captures/h9-android-layout-preview.png");
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                File.WriteAllBytes(outputPath, preview.EncodeToPNG());
                Debug.Log($"H9 Android 16:9 layout preview written: {outputPath} ({scene.name}).");
            }
            finally
            {
                RenderTexture.active = null;
                camera.targetTexture = originalTarget;
                camera.aspect = originalAspect;
                camera.clearFlags = originalClearFlags;
                camera.backgroundColor = originalBackground;
                canvas.renderMode = originalMode;
                canvas.worldCamera = originalCanvasCamera;
                canvas.planeDistance = originalPlaneDistance;
                UnityEngine.Object.DestroyImmediate(preview);
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }

        private static void ConfigureCanvas(Canvas canvas)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = GetOrAdd<CanvasScaler>(canvas.gameObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.45f;
            canvas.gameObject.layer = ProjectLayers.PlayerUi;
        }

        private static GameObject ConfigureSafeArea(Transform canvas)
        {
            var root = FindUi("H9_SafeAreaRoot", canvas);
            if (root == null)
            {
                root = new GameObject("H9_SafeAreaRoot", typeof(RectTransform));
                root.transform.SetParent(canvas, false);
            }

            SetFullScreen(root);
            root.layer = ProjectLayers.PlayerUi;
            SetLayerRecursive(root.transform, ProjectLayers.PlayerUi);

            var existingChildren = new List<Transform>();
            for (var index = 0; index < canvas.childCount; index++)
            {
                var child = canvas.GetChild(index);
                if (child != root.transform)
                {
                    existingChildren.Add(child);
                }
            }

            foreach (var child in existingChildren)
            {
                child.SetParent(root.transform, true);
            }

            var safeArea = GetOrAdd<H9SafeAreaLayout>(root);
            safeArea.Configure(root.GetComponent<RectTransform>(), SafeAreaPadding);
            return root;
        }

        private static void ConfigureControls(Transform root)
        {
            var joystick = FindUi("VirtualJoystick_Base", root);
            if (joystick != null)
            {
                SetBottomLeft(joystick, new Vector2(150f, 150f),
                    new Vector2(JoystickTouchSize, JoystickTouchSize));
                var image = joystick.GetComponent<Image>();
                if (image != null)
                {
                    image.color = Color.clear;
                    image.raycastTarget = false;
                    var visual = FindUi("VirtualJoystick_Visual", joystick.transform);
                    if (visual == null)
                    {
                        visual = new GameObject("VirtualJoystick_Visual", typeof(RectTransform), typeof(Image));
                        visual.transform.SetParent(joystick.transform, false);
                    }

                    visual.layer = ProjectLayers.PlayerUi;
                    SetCenter(visual, Vector2.zero, new Vector2(JoystickVisualSize, JoystickVisualSize));
                    var visualImage = GetOrAdd<Image>(visual);
                    visualImage.sprite = image.sprite;
                    visualImage.type = image.type;
                    visualImage.color = new Color(0.08f, 0.12f, 0.2f, 0.58f);
                    visualImage.raycastTarget = false;
                }

                var handle = FindUi("VirtualJoystick_Handle", joystick.transform);
                if (handle != null)
                {
                    var handleRect = handle.GetComponent<RectTransform>();
                    handleRect.anchorMin = new Vector2(0.5f, 0.5f);
                    handleRect.anchorMax = new Vector2(0.5f, 0.5f);
                    handleRect.pivot = new Vector2(0.5f, 0.5f);
                    handleRect.anchoredPosition = Vector2.zero;
                    handleRect.sizeDelta = new Vector2(96f, 96f);
                    SetProperty(handle.GetComponent("UnityEngine.InputSystem.OnScreen.OnScreenStick"),
                        "movementRange", 82f);
                }
            }

            ConfigureButton(root, "H4_AttackButton", new Vector2(-120f, -112f));
            ConfigureButton(root, "H4B_DefenseButton", new Vector2(-284f, -112f));
            ConfigureButton(root, "H4B_AreaButton", new Vector2(-120f, -276f));
            ConfigureButton(root, "H5_InteractButton", new Vector2(-284f, -276f));
            ConfigureButton(root, "H5_EquipButton", new Vector2(-448f, -276f));
        }

        private static void ConfigureButton(Transform root, string name, Vector2 position)
        {
            var button = FindUi(name, root);
            if (button == null)
            {
                return;
            }

            SetTopRight(button, position, new Vector2(148f, 148f));
            var control = button.GetComponent<Button>();
            if (control != null)
            {
                control.transition = Selectable.Transition.ColorTint;
                var colors = control.colors;
                colors.normalColor = new Color(0.08f, 0.16f, 0.25f, 0.88f);
                colors.highlightedColor = new Color(0.18f, 0.48f, 0.7f, 0.98f);
                colors.pressedColor = new Color(1f, 0.68f, 0.22f, 1f);
                colors.selectedColor = colors.highlightedColor;
                colors.fadeDuration = 0.08f;
                control.colors = colors;
            }
        }

        private static void ConfigureHud(Transform root)
        {
            var statusPanel = FindUi("H7_StatusPanel", root);
            if (statusPanel != null)
            {
                SetTopLeft(statusPanel, new Vector2(24f, -24f), new Vector2(610f, 146f));
                ConfigureText(FindUi("Status", statusPanel.transform), 19, new Vector2(18f, -12f),
                    new Vector2(574f, 50f));
                ConfigureBar(FindUi("LifeBar", statusPanel.transform), new Vector2(18f, -68f), 568f);
                ConfigureBar(FindUi("HeatBar", statusPanel.transform), new Vector2(18f, -92f), 568f);
                ConfigureBar(FindUi("ExperienceBar", statusPanel.transform),
                    new Vector2(18f, -116f), 568f);
            }

            var missionPanel = FindUi("H7_MissionPanel", root);
            if (missionPanel != null)
            {
                SetTopLeft(missionPanel, new Vector2(24f, -188f), new Vector2(650f, 70f));
                ConfigureText(FindUi("Mission", missionPanel.transform), 18,
                    new Vector2(18f, -10f), new Vector2(614f, 50f));
            }

            var compactMission = FindUi("H5_MissionHud", root);
            if (compactMission != null)
            {
                SetTopLeft(compactMission, new Vector2(24f, -270f), new Vector2(700f, 48f));
                ConfigureText(compactMission, 16, new Vector2(12f, -6f), new Vector2(676f, 38f));
            }
        }

        private static void ConfigureMenus(Transform root)
        {
            var pauseCard = FindUi("H8_PauseCard", root);
            if (pauseCard != null)
            {
                SetCenter(pauseCard, Vector2.zero, new Vector2(520f, 480f));
                SetCenterChild(pauseCard, "Title", new Vector2(0f, -34f), new Vector2(470f, 48f));
                SetCenterChild(pauseCard, "Subtitle", new Vector2(0f, -88f), new Vector2(470f, 36f));
                SetCenterChild(pauseCard, "H8_ContinueButton", new Vector2(0f, -150f),
                    new Vector2(360f, 58f));
                SetCenterChild(pauseCard, "H8_OptionsButton", new Vector2(0f, -222f),
                    new Vector2(360f, 58f));
                SetCenterChild(pauseCard, "H8_ExitButton", new Vector2(0f, -294f),
                    new Vector2(360f, 58f));
                SetCenterChild(pauseCard, "Version", new Vector2(0f, -410f), new Vector2(460f, 28f));
            }

            var optionsCard = FindUi("H8_OptionsCard", root);
            if (optionsCard == null)
            {
                return;
            }

            SetCenter(optionsCard, Vector2.zero, new Vector2(650f, 650f));
            SetCenterChild(optionsCard, "Title", new Vector2(0f, -30f), new Vector2(580f, 44f));
            SetCenterChild(optionsCard, "MusicLabel", new Vector2(-220f, -96f), new Vector2(220f, 30f));
            SetCenterChild(optionsCard, "H8_MusicSlider", new Vector2(55f, -96f), new Vector2(350f, 28f));
            SetCenterChild(optionsCard, "FxLabel", new Vector2(-220f, -150f), new Vector2(220f, 30f));
            SetCenterChild(optionsCard, "H8_FxSlider", new Vector2(55f, -150f), new Vector2(350f, 28f));
            SetCenterChild(optionsCard, "H8_VibrationToggle", new Vector2(-180f, -220f),
                new Vector2(360f, 42f));
            SetCenterChild(optionsCard, "H8_FpsToggle", new Vector2(-180f, -270f), new Vector2(360f, 42f));
            SetCenterChild(optionsCard, "H8_DebugToggle", new Vector2(-180f, -320f),
                new Vector2(360f, 42f));
            SetCenterChild(optionsCard, "H8_GraphicsButton", new Vector2(0f, -392f),
                new Vector2(390f, 54f));
            SetCenterChild(optionsCard, "H8_BackButton", new Vector2(0f, -478f), new Vector2(280f, 54f));
            SetCenterChild(optionsCard, "H8_TooltipPanel", new Vector2(0f, 176f), new Vector2(550f, 54f));
            SetCenterChild(optionsCard, "Version", new Vector2(0f, -590f), new Vector2(580f, 26f));
        }

        private static void ConfigureCamera()
        {
            var cameraObject = GameObject.Find("CM_OfficialCamera");
            var cmCamera = cameraObject?.GetComponent<CinemachineCamera>();
            var follow = cameraObject?.GetComponent<CinemachineFollow>();
            var polish = cameraObject?.GetComponent<H7CameraPolish>();
            if (cameraObject == null || cmCamera == null || follow == null || polish == null)
            {
                throw new InvalidOperationException("H9 requires the official Cinemachine camera.");
            }

            var lens = cmCamera.Lens;
            lens.OrthographicSize = CameraZoom;
            cmCamera.Lens = lens;
            var compositionOffset = new Vector3(0f, 0.65f, -20f);
            follow.FollowOffset = compositionOffset;
            follow.TrackerSettings.PositionDamping = new Vector3(0.35f, 0.35f, 0.35f);
            var followTarget = cameraObject.GetComponent<H3CinemachineFollowTarget>();
            if (followTarget != null)
            {
                var serializedFollowTarget = new SerializedObject(followTarget);
                var serializedOffset = serializedFollowTarget.FindProperty("followOffset");
                if (serializedOffset != null)
                {
                    serializedOffset.vector3Value = compositionOffset;
                    serializedFollowTarget.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            polish.Configure(cmCamera, follow);
            polish.ConfigureH9(CameraZoom, follow.FollowOffset, follow.TrackerSettings.PositionDamping);

            var boundsObject = GameObject.Find("H9_CameraBounds");
            if (boundsObject == null)
            {
                boundsObject = new GameObject("H9_CameraBounds");
            }

            boundsObject.layer = ProjectLayers.NavigationDebug;
            boundsObject.transform.position = new Vector3(3.6f, 7.5f, 0f);
            var bounds = GetOrAdd<BoxCollider2D>(boundsObject);
            bounds.isTrigger = true;
            bounds.size = new Vector2(40f, 24f);
            var confiner = GetOrAdd<CinemachineConfiner2D>(cameraObject);
            confiner.BoundingShape2D = bounds;
            confiner.Damping = 0.35f;
            confiner.SlowingDistance = 0.8f;
            confiner.InvalidateBoundingShapeCache();

            var realCamera = Camera.main;
            if (realCamera != null)
            {
                realCamera.orthographic = true;
                realCamera.orthographicSize = CameraZoom;
            }
        }

        private static void ConfigureBackdrop()
        {
            var backdrop = GameObject.Find("Lumbre_World_Backdrop")?.GetComponent<SpriteRenderer>();
            if (backdrop == null || backdrop.sprite == null)
            {
                return;
            }

            var targetWidth = BackdropWidth;
            var spriteWidth = Mathf.Max(0.01f, backdrop.sprite.bounds.size.x);
            var scale = targetWidth / spriteWidth;
            backdrop.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private static void SetCenterChild(GameObject parent, string name, Vector2 position, Vector2 size)
        {
            var child = FindUi(name, parent.transform);
            if (child != null)
            {
                SetCenter(child, position, size);
            }
        }

        private static void ConfigureText(GameObject textObject, int fontSize, Vector2 position, Vector2 size)
        {
            if (textObject == null)
            {
                return;
            }

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = textObject.GetComponent<Text>();
            if (text != null)
            {
                text.fontSize = fontSize;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Truncate;
            }
        }

        private static void ConfigureBar(GameObject barObject, Vector2 position, float width)
        {
            if (barObject == null)
            {
                return;
            }

            var rect = barObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(width, 14f);
        }

        private static void SetBottomLeft(GameObject gameObject, Vector2 position, Vector2 size)
        {
            ConfigureRect(gameObject, new Vector2(0f, 0f), new Vector2(0f, 0f),
                Vector2.zero, position, size);
        }

        private static void SetTopRight(GameObject gameObject, Vector2 position, Vector2 size)
        {
            ConfigureRect(gameObject, Vector2.one, Vector2.one, Vector2.one, position, size);
        }

        private static void SetTopLeft(GameObject gameObject, Vector2 position, Vector2 size)
        {
            ConfigureRect(gameObject, new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), position, size);
        }

        private static void SetCenter(GameObject gameObject, Vector2 position, Vector2 size)
        {
            ConfigureRect(gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), position, size);
        }

        private static void SetFullScreen(GameObject gameObject)
        {
            ConfigureRect(gameObject, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
        }

        private static void ConfigureRect(GameObject gameObject, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size)
        {
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static GameObject FindUi(string name, Transform root)
        {
            if (root == null)
            {
                return null;
            }

            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(transform => transform.name == name)?.gameObject;
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component ? component : gameObject.AddComponent<T>();
        }

        private static void SetProperty(Component component, string propertyName, object value)
        {
            if (component == null)
            {
                return;
            }

            var property = component.GetType().GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(component, value);
                return;
            }

            var field = component.GetType().GetField(propertyName);
            field?.SetValue(component, value);
        }

        private static void SetLayerRecursive(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            for (var index = 0; index < root.childCount; index++)
            {
                SetLayerRecursive(root.GetChild(index), layer);
            }
        }
    }
}
