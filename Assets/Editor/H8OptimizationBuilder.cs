using System;
using System.IO;
using System.Linq;
using Lumbre.Game.Client;
using Lumbre.Game.Client.Bootstrap;
using Lumbre.Game.Client.Presentation;
using Lumbre.Game.Domain.Constants;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Lumbre.Game.Editor
{
    public static class H8OptimizationBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;
        private const string CharacterArtRoot = "Assets/Game/Presentation/Art/Characters/";
        private const string WorldArtRoot = "Assets/Game/Presentation/Art/World/";

        public static void Build()
        {
            H7PresentationBuilder.Build();
            ConfigureBootstrapScene();
            ValidateBuildSettings();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ConfigureCanvas();
            ConfigureScenePresentation();

            var canvas = GameObject.Find("H3_MobileControls");
            if (canvas == null)
            {
                throw new InvalidOperationException("H8 requires H3_MobileControls.");
            }

            var root = FindOrCreateUi("H8_UXRoot", canvas.transform);
            SetFullScreen(root);

            var pauseButton = CreateButton(root.transform, "H8_PauseButton", "PAUSA",
                new Vector2(-30f, -30f), new Vector2(160f, 64f), Anchor.TopRight,
                new Color(0.06f, 0.16f, 0.24f, 0.96f));

            var pausePanel = CreatePanel(root.transform, "H8_PausePanel", Anchor.FullScreen,
                Vector2.zero, Vector2.zero, new Color(0.01f, 0.02f, 0.05f, 0.78f));
            var pauseCard = CreatePanel(pausePanel.transform, "H8_PauseCard", Anchor.Center,
                Vector2.zero, new Vector2(560f, 520f), new Color(0.03f, 0.09f, 0.15f, 0.98f));
            CreateText(pauseCard.transform, "Title", "LUMBRE DE NÁCAR", 32,
                new Vector2(0f, -42f), new Vector2(500f, 56f), TextAnchor.MiddleCenter,
                new Color(1f, 0.82f, 0.35f, 1f));
            CreateText(pauseCard.transform, "Subtitle", "PAUSA", 22,
                new Vector2(0f, -102f), new Vector2(500f, 40f), TextAnchor.MiddleCenter,
                Color.white);
            var continueButton = CreateButton(pauseCard.transform, "H8_ContinueButton", "CONTINUAR",
                new Vector2(0f, -188f), new Vector2(400f, 64f), Anchor.Center,
                new Color(0.12f, 0.48f, 0.42f, 1f));
            var optionsButton = CreateButton(pauseCard.transform, "H8_OptionsButton", "OPCIONES",
                new Vector2(0f, -270f), new Vector2(400f, 64f), Anchor.Center,
                new Color(0.12f, 0.29f, 0.48f, 1f));
            var exitButton = CreateButton(pauseCard.transform, "H8_ExitButton", "SALIR",
                new Vector2(0f, -352f), new Vector2(400f, 64f), Anchor.Center,
                new Color(0.48f, 0.16f, 0.2f, 1f));
            CreateText(pauseCard.transform, "Version", H8PauseController.VersionLabel, 16,
                new Vector2(0f, -450f), new Vector2(500f, 30f), TextAnchor.MiddleCenter,
                new Color(0.65f, 0.75f, 0.82f, 1f));

            var optionsPanel = CreatePanel(root.transform, "H8_OptionsPanel", Anchor.FullScreen,
                Vector2.zero, Vector2.zero, new Color(0.01f, 0.02f, 0.05f, 0.82f));
            var optionsCard = CreatePanel(optionsPanel.transform, "H8_OptionsCard", Anchor.Center,
                Vector2.zero, new Vector2(700f, 760f), new Color(0.03f, 0.09f, 0.15f, 0.99f));
            CreateText(optionsCard.transform, "Title", "OPCIONES", 30,
                new Vector2(0f, -38f), new Vector2(620f, 48f), TextAnchor.MiddleCenter,
                new Color(1f, 0.82f, 0.35f, 1f));
            CreateText(optionsCard.transform, "MusicLabel", "VOLUMEN MÚSICA", 17,
                new Vector2(-250f, -118f), new Vector2(250f, 32f), TextAnchor.MiddleLeft,
                Color.white);
            var musicSlider = CreateSlider(optionsCard.transform, "H8_MusicSlider",
                new Vector2(40f, -118f), new Color(0.3f, 0.78f, 1f, 1f));
            CreateText(optionsCard.transform, "FxLabel", "VOLUMEN EFECTOS", 17,
                new Vector2(-250f, -194f), new Vector2(250f, 32f), TextAnchor.MiddleLeft,
                Color.white);
            var fxSlider = CreateSlider(optionsCard.transform, "H8_FxSlider",
                new Vector2(40f, -194f), new Color(1f, 0.56f, 0.24f, 1f));
            var vibration = CreateToggle(optionsCard.transform, "H8_VibrationToggle", "VIBRACIÓN",
                new Vector2(-210f, -285f));
            var fps = CreateToggle(optionsCard.transform, "H8_FpsToggle", "MOSTRAR FPS (QA)",
                new Vector2(-210f, -345f));
            var debug = CreateToggle(optionsCard.transform, "H8_DebugToggle", "MOSTRAR DEBUG (QA)",
                new Vector2(-210f, -405f));
            var graphicsButton = CreateButton(optionsCard.transform, "H8_GraphicsButton", "GRÁFICOS: AUTO",
                new Vector2(0f, -500f), new Vector2(430f, 58f), Anchor.Center,
                new Color(0.12f, 0.29f, 0.48f, 1f));
            var graphicsLabel = graphicsButton.GetComponentInChildren<Text>();
            var backButton = CreateButton(optionsCard.transform, "H8_BackButton", "VOLVER",
                new Vector2(0f, -590f), new Vector2(300f, 58f), Anchor.Center,
                new Color(0.12f, 0.48f, 0.42f, 1f));
            var tooltipPanel = CreatePanel(optionsCard.transform, "H8_TooltipPanel", Anchor.Center,
                new Vector2(0f, 188f), new Vector2(570f, 58f), new Color(0.02f, 0.05f, 0.08f, 0.96f));
            var tooltipLabel = CreateText(tooltipPanel.transform, "Label", "", 15,
                Vector2.zero, new Vector2(530f, 48f), TextAnchor.MiddleCenter,
                new Color(0.85f, 0.96f, 1f, 1f));
            AddTooltip(musicSlider.gameObject, tooltipPanel, tooltipLabel,
                "La música ambiente cambia suavemente entre plaza, sendero y cueva.");
            AddTooltip(fxSlider.gameObject, tooltipPanel, tooltipLabel,
                "Controla pasos, ataques, impactos, misión y feedback de interfaz.");
            AddTooltip(graphicsButton.gameObject, tooltipPanel, tooltipLabel,
                "Cambia la calidad global para comparar rendimiento en Android.");
            AddTooltip(vibration.gameObject, tooltipPanel, tooltipLabel,
                "Reserva esta preferencia para feedback háptico futuro.");
            AddTooltip(fps.gameObject, tooltipPanel, tooltipLabel,
                "Muestra FPS, frame time, memoria y draw calls para QA.");
            var versionLabel = CreateText(optionsCard.transform, "Version", H8PauseController.VersionLabel,
                15, new Vector2(0f, -700f), new Vector2(620f, 28f), TextAnchor.MiddleCenter,
                new Color(0.65f, 0.75f, 0.82f, 1f));

            var overlayLabel = CreateText(root.transform, "H8_PerformanceOverlay", "", 16,
                new Vector2(30f, -30f), new Vector2(480f, 130f), TextAnchor.UpperLeft,
                new Color(0.62f, 1f, 0.72f, 1f));
            SetTopLeft(overlayLabel.gameObject, new Vector2(30f, -30f), new Vector2(480f, 130f));
            var overlay = GetOrAdd<H8PerformanceOverlay>(root);
            overlay.Configure(overlayLabel);

            var controller = GetOrAdd<H8PauseController>(root);
            controller.Configure(pausePanel, optionsPanel, pauseButton, continueButton,
                optionsButton, backButton, exitButton, graphicsButton, musicSlider, fxSlider,
                vibration, fps, debug, graphicsLabel, versionLabel,
                GameObject.Find("H7_Presentation")?.GetComponent<H7AudioFeedback>(), overlay);

            SetPanelVisibility(pausePanel, false);
            SetPanelVisibility(optionsPanel, false);
            overlayLabel.gameObject.SetActive(true);

            var marker = GameObject.Find("H2_Greybox")?.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H8OptimizationUx";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        public static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                throw new InvalidOperationException("H8.1 requires a non-empty Build Settings scene list.");
            }

            if (!scenes[0].enabled || scenes[0].path != ProjectConstants.BootstrapScenePath)
            {
                throw new InvalidOperationException(
                    $"H8.1 requires Bootstrap enabled at build index 0, found '{scenes[0].path}'.");
            }

            var verticalSlice = scenes.FirstOrDefault(scene =>
                scene.enabled && scene.path == ProjectConstants.VerticalSliceScenePath);
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            var verticalSliceFile = projectRoot == null
                ? string.Empty
                : Path.Combine(projectRoot, ProjectConstants.VerticalSliceScenePath);
            if (verticalSlice == null || !File.Exists(verticalSliceFile))
            {
                throw new InvalidOperationException("H8.1 requires enabled VerticalSlice in Build Settings.");
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ProjectConstants.BootstrapScenePath) == null
                || AssetDatabase.LoadAssetAtPath<SceneAsset>(ProjectConstants.VerticalSliceScenePath) == null)
            {
                throw new InvalidOperationException("H8.1 Build Settings references a missing scene asset.");
            }
        }

        public static void BuildAndroidBlackScreenDebug()
        {
            Build();
            ValidateBuildSettings();

            const string outputPath = "/tmp/lumbre-h8-1-black-screen-debug.apk";
            var originalVersion = PlayerSettings.bundleVersion;
            var originalArchitectures = PlayerSettings.Android.targetArchitectures;
            var originalUseDefaultGraphics = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android);
            var originalGraphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);

            try
            {
                PlayerSettings.bundleVersion = "0.8.1 Alpha-debug";
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
                    new[] { GraphicsDeviceType.OpenGLES3 });

                var options = new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes
                        .Where(scene => scene.enabled)
                        .Select(scene => scene.path)
                        .ToArray(),
                    locationPathName = outputPath,
                    target = BuildTarget.Android,
                    options = BuildOptions.Development | BuildOptions.AllowDebugging
                        | BuildOptions.ConnectWithProfiler
                };
                var report = BuildPipeline.BuildPlayer(options);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"H8.1 Android debug build failed: {report.summary.result} "
                        + $"({report.summary.totalErrors} errors).");
                }

                Debug.Log($"H8.1 Android black-screen debug build succeeded: {outputPath} "
                    + $"({report.summary.totalSize} bytes).");
            }
            finally
            {
                PlayerSettings.bundleVersion = originalVersion;
                PlayerSettings.Android.targetArchitectures = originalArchitectures;
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, originalUseDefaultGraphics);
                if (!originalUseDefaultGraphics && originalGraphics != null && originalGraphics.Length > 0)
                {
                    PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, originalGraphics);
                }

                AssetDatabase.SaveAssets();
            }
        }

        private static void ConfigureBootstrapScene()
        {
            var scene = EditorSceneManager.OpenScene(ProjectConstants.BootstrapScenePath, OpenSceneMode.Single);
            var loader = GameObject.Find("H8_1_BootstrapLoader");
            if (loader == null)
            {
                loader = new GameObject("H8_1_BootstrapLoader");
            }

            loader.layer = 0;
            GetOrAdd<BootstrapSceneLoader>(loader);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ProjectConstants.BootstrapScenePath);
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
                throw new InvalidOperationException("H8 Android build requires at least one enabled scene.");
            }

            const string outputPath = "/tmp/lumbre-h8-android.apk";
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.ConnectWithProfiler
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"H8 Android build failed: {report.summary.result} ({report.summary.totalErrors} errors).");
            }

            Debug.Log($"H8 Android development build succeeded: {outputPath} ({report.summary.totalSize} bytes).");
        }

        private static void ConfigureCanvas()
        {
            var canvasObject = GameObject.Find("H3_MobileControls");
            var scaler = canvasObject?.GetComponent<CanvasScaler>();
            var canvas = canvasObject?.GetComponent<Canvas>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (canvas != null)
            {
                canvas.pixelPerfect = false;
                canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.None;
            }
        }

        private static void ConfigureScenePresentation()
        {
            var texturePaths = new[]
            {
                CharacterArtRoot + "bastion-brasa.png",
                CharacterArtRoot + "nara-velaquieta.png",
                CharacterArtRoot + "mordeluz.png",
                CharacterArtRoot + "mordeluz-resonante.png",
                WorldArtRoot + "lumbre-world-backdrop.png"
            };
            foreach (var path in texturePaths)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.mipmapEnabled = false;
                importer.isReadable = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }

            var animators = UnityEngine.Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
            foreach (var animator in animators)
            {
                animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                animator.applyRootMotion = false;
            }

            QualitySettings.vSyncCount = 0;
        }

        private static Button CreateButton(Transform parent, string name, string label,
            Vector2 position, Vector2 size, Anchor anchor, Color color)
        {
            var buttonObject = FindOrCreateUi(name, parent);
            ConfigureRect(buttonObject, anchor, position, size);
            var image = GetOrAdd<Image>(buttonObject);
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = color;
            var button = GetOrAdd<Button>(buttonObject);
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.9f, 0.65f, 1f);
            colors.pressedColor = new Color(0.75f, 0.85f, 1f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            var text = CreateText(buttonObject.transform, "Label", label, 18,
                Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Color.white);
            SetFullScreen(text.gameObject);
            return button;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 position,
            Color fillColor)
        {
            var sliderObject = FindOrCreateUi(name, parent);
            ConfigureRect(sliderObject, Anchor.Center, position, new Vector2(370f, 26f));
            var slider = GetOrAdd<Slider>(sliderObject);
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.interactable = true;
            var background = GetOrAdd<Image>(sliderObject);
            background.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            background.type = Image.Type.Sliced;
            background.color = new Color(0.08f, 0.15f, 0.22f, 1f);
            var fillObject = FindOrCreateUi("Fill", sliderObject.transform);
            ConfigureRect(fillObject, Anchor.FullScreen, Vector2.zero, Vector2.zero);
            var fill = GetOrAdd<Image>(fillObject);
            fill.sprite = background.sprite;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.color = fillColor;
            slider.fillRect = fillObject.GetComponent<RectTransform>();
            return slider;
        }

        private static Toggle CreateToggle(Transform parent, string name, string label, Vector2 position)
        {
            var toggleObject = FindOrCreateUi(name, parent);
            ConfigureRect(toggleObject, Anchor.Center, position, new Vector2(420f, 48f));
            var background = GetOrAdd<Image>(toggleObject);
            background.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            background.type = Image.Type.Sliced;
            background.color = new Color(0.08f, 0.15f, 0.22f, 1f);
            var toggle = GetOrAdd<Toggle>(toggleObject);
            toggle.targetGraphic = background;
            var checkObject = FindOrCreateUi("Checkmark", toggleObject.transform);
            ConfigureRect(checkObject, Anchor.Left, new Vector2(22f, 0f), new Vector2(26f, 26f));
            var check = GetOrAdd<Image>(checkObject);
            check.sprite = background.sprite;
            check.color = new Color(0.36f, 0.9f, 0.65f, 1f);
            toggle.graphic = check;
            var text = CreateText(toggleObject.transform, "Label", label, 17,
                new Vector2(62f, 0f), new Vector2(330f, 44f), TextAnchor.MiddleLeft, Color.white);
            text.raycastTarget = false;
            return toggle;
        }

        private static void AddTooltip(GameObject target, CanvasGroup panel, Text label,
            string message)
        {
            var tooltip = GetOrAdd<H8Tooltip>(target);
            tooltip.Configure(panel, label, message);
        }

        private static CanvasGroup CreatePanel(Transform parent, string name, Anchor anchor,
            Vector2 position, Vector2 size, Color color)
        {
            var panel = FindOrCreateUi(name, parent);
            ConfigureRect(panel, anchor, position, size);
            var image = GetOrAdd<Image>(panel);
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            image.color = color;
            var group = GetOrAdd<CanvasGroup>(panel);
            return group;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize,
            Vector2 position, Vector2 size, TextAnchor anchor, Color color)
        {
            var textObject = FindOrCreateUi(name, parent);
            ConfigureRect(textObject, Anchor.Center, position, size);
            var text = GetOrAdd<Text>(textObject);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = anchor;
            text.color = color;
            text.text = content;
            text.raycastTarget = false;
            var outline = GetOrAdd<Outline>(textObject);
            outline.effectColor = new Color(0f, 0f, 0f, 0.72f);
            outline.effectDistance = new Vector2(2f, -2f);
            return text;
        }

        private static GameObject FindOrCreateUi(string name, Transform parent)
        {
            var existing = parent.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(transform => transform.name == name)?.gameObject;
            if (existing != null)
            {
                existing.layer = ProjectLayers.PlayerUi;
                return existing;
            }

            var created = new GameObject(name, typeof(RectTransform));
            created.transform.SetParent(parent, false);
            created.layer = ProjectLayers.PlayerUi;
            return created;
        }

        private static void ConfigureRect(GameObject gameObject, Anchor anchor,
            Vector2 position, Vector2 size)
        {
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor.Min;
            rect.anchorMax = anchor.Max;
            rect.pivot = anchor.Pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetFullScreen(GameObject gameObject)
        {
            ConfigureRect(gameObject, Anchor.FullScreen, Vector2.zero, Vector2.zero);
        }

        private static void SetTopLeft(GameObject gameObject, Vector2 position, Vector2 size)
        {
            ConfigureRect(gameObject, Anchor.TopLeft, position, size);
        }

        private static void SetPanelVisibility(CanvasGroup group, bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component ? component : gameObject.AddComponent<T>();
        }

        private readonly struct Anchor
        {
            private Anchor(Vector2 min, Vector2 max, Vector2 pivot)
            {
                Min = min;
                Max = max;
                Pivot = pivot;
            }

            public Vector2 Min { get; }
            public Vector2 Max { get; }
            public Vector2 Pivot { get; }

            public static Anchor FullScreen => new Anchor(Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f));
            public static Anchor Center => new Anchor(new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            public static Anchor TopRight => new Anchor(Vector2.one, Vector2.one, Vector2.one);
            public static Anchor TopLeft => new Anchor(new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(0f, 1f));
            public static Anchor Left => new Anchor(new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
        }
    }
}
