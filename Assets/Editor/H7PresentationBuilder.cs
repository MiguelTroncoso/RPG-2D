using System;
using System.Collections.Generic;
using System.Linq;
using Lumbre.Game.Client;
using Lumbre.Game.Client.Camera;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Missions;
using Lumbre.Game.Client.Presentation;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Client.Progression;
using Lumbre.Game.Client.World;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Navigation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Lumbre.Game.Editor
{
    public static class H7PresentationBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;
        private const string CharacterArtRoot = "Assets/Game/Presentation/Art/Characters/";
        private const string WorldArtRoot = "Assets/Game/Presentation/Art/World/";
        private const string AnimationRoot = "Assets/Game/Presentation/Animations/";
        private const string PlayerSpritePath = CharacterArtRoot + "bastion-brasa.png";
        private const string NaraSpritePath = CharacterArtRoot + "nara-velaquieta.png";
        private const string MordeluzSpritePath = CharacterArtRoot + "mordeluz.png";
        private const string ResonantSpritePath = CharacterArtRoot + "mordeluz-resonante.png";
        private const string BackdropSpritePath = WorldArtRoot + "lumbre-world-backdrop.png";

        public static void Build()
        {
            H6ProgressionBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            ConfigureTexture(PlayerSpritePath, 512);
            ConfigureTexture(NaraSpritePath, 512);
            ConfigureTexture(MordeluzSpritePath, 512);
            ConfigureTexture(ResonantSpritePath, 512);
            ConfigureTexture(BackdropSpritePath, 1024);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var player = GameObject.FindWithTag("Player");
            var playerHealth = player?.GetComponent<H4CombatHealth>();
            var playerCombat = player?.GetComponent<H4PlayerCombatController>();
            var playerAbilities = player?.GetComponent<H4BPlayerAbilityController>();
            if (player == null || playerHealth == null || playerCombat == null || playerAbilities == null)
            {
                throw new InvalidOperationException("H7 requires the complete H6 player slice.");
            }

            BuildCharacterView(player, "Player_Visual", "H7_Player_Visual", "BastionBrasa",
                PlayerSpritePath, new[] { "Idle", "Walk", "Attack", "Defense", "Area", "Damage", "Death" },
                0.58f, ProjectLayers.Player, 30);

            BuildEnemyView("Mordeluz", "Mordeluz_1_Visual", MordeluzSpritePath, 0.38f);
            BuildEnemyView("Mordeluz_2", "Mordeluz_2_Visual", MordeluzSpritePath, 0.38f);
            BuildEnemyView("Mordeluz_3", "Mordeluz_3_Visual", MordeluzSpritePath, 0.38f);
            BuildEnemyView("Mordeluz_Resonante", "Mordeluz_Resonante_Visual", ResonantSpritePath, 0.5f,
                new[] { "Idle", "Walk", "Attack", "Telegraph", "Damage", "Death" });

            var mission = GameObject.Find("H5_MissionRuntime")?.GetComponent<H5MissionRuntime>();
            var progression = GameObject.Find("H6_ProgressionRuntime")?.GetComponent<H6ProgressionRuntime>();
            var nara = GameObject.Find("Nara_Velaquieta")?.GetComponent<H5NaraController>();
            if (mission == null || progression == null || nara == null)
            {
                throw new InvalidOperationException("H7 requires the H5/H6 mission and progression slice.");
            }

            BuildNaraView(nara, player.transform, mission);
            BuildWorldBackdrop();
            var cameraPolish = BuildCameraPolish(player.transform);

            var presentationRoot = FindOrCreate("H7_Presentation");
            var pool = GetOrAdd<H7VfxPool>(presentationRoot);
            pool.Configure(18, 24);
            var audio = GetOrAdd<H7AudioFeedback>(presentationRoot);
            audio.Configure(player.GetComponent<H3PlayerController>(),
                GreyboxWorldCoordinates.ToWorld(new GridPosition(6, 10)),
                GreyboxWorldCoordinates.ToWorld(new GridPosition(16, 10)),
                GreyboxWorldCoordinates.ToWorld(new GridPosition(27, 10)));
            var hud = BuildHud(playerHealth, playerAbilities, mission, progression);
            var runtime = GetOrAdd<H7PresentationRuntime>(presentationRoot);
            runtime.Configure(pool, audio, cameraPolish, hud, mission, progression,
                playerCombat, playerAbilities, playerHealth);

            var marker = GameObject.Find("H2_Greybox")?.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H7ArtAudioPolish";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static void BuildEnemyView(string objectName, string legacyVisualName,
            string spritePath, float scale, string[] states = null)
        {
            var enemy = GameObject.Find(objectName);
            if (enemy == null)
            {
                throw new InvalidOperationException($"H7 requires enemy '{objectName}'.");
            }

            BuildCharacterView(enemy, legacyVisualName, objectName + "_H7_Visual", objectName,
                spritePath, states ?? new[] { "Idle", "Walk", "Attack", "Damage", "Death" },
                scale, ProjectLayers.Enemy, 25);
        }

        private static void BuildCharacterView(GameObject owner, string legacyVisualName,
            string visualName, string characterId, string spritePath, string[] states,
            float visualScale, int layer, int sortingOrder)
        {
            var legacyVisual = owner.transform.Find(legacyVisualName);
            if (legacyVisual != null)
            {
                UnityEngine.Object.DestroyImmediate(legacyVisual.gameObject);
            }

            var visual = owner.transform.Find(visualName);
            if (visual == null)
            {
                visual = new GameObject(visualName).transform;
                visual.SetParent(owner.transform, false);
            }

            visual.localPosition = new Vector3(0f, 0f, -0.16f);
            visual.localRotation = Quaternion.identity;
            visual.localScale = Vector3.one * visualScale;
            SetLayerRecursive(visual, layer);
            // Unity can retain a managed wrapper for a component destroyed by a
            // previous failed builder pass. Treat Unity's fake-null state as
            // missing so the pass remains safe to rerun.
            var renderer = visual.gameObject.GetComponent<SpriteRenderer>();
            if (!renderer)
            {
                renderer = visual.gameObject.AddComponent<SpriteRenderer>();
            }
            renderer.sprite = LoadSprite(spritePath);
            renderer.sortingOrder = sortingOrder;
            renderer.drawMode = SpriteDrawMode.Simple;

            var controllerPath = AnimationRoot + characterId + "Controller.controller";
            var controller = CreateAnimatorController(controllerPath, visualName, renderer.sprite, states);
            var animator = GetOrAdd<Animator>(owner);
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

            var view = GetOrAdd<H7CharacterView>(owner);
            view.Configure(characterId, renderer.sprite, animator);
        }

        private static void BuildNaraView(H5NaraController nara, Transform player,
            H5MissionRuntime mission)
        {
            BuildCharacterView(nara.gameObject, "Nara_Velaquieta_Visual", "H7_Nara_Visual",
                "NaraVelaquieta", NaraSpritePath, new[] { "Idle", "Talk", "MissionReady" },
                0.36f, ProjectLayers.WorldGround, 24);

            var visual = nara.transform.Find("H7_Nara_Visual");
            var indicatorObject = nara.transform.Find("H7_Nara_MissionIndicator")?.gameObject;
            if (indicatorObject == null)
            {
                indicatorObject = new GameObject("H7_Nara_MissionIndicator");
                indicatorObject.transform.SetParent(nara.transform, false);
            }

            indicatorObject.transform.localPosition = new Vector3(0f, 1.25f, -0.25f);
            indicatorObject.transform.localScale = Vector3.one * 0.18f;
            SetLayerRecursive(indicatorObject.transform, ProjectLayers.WorldGround);
            var indicator = GetOrAdd<SpriteRenderer>(indicatorObject);
            indicator.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            indicator.sortingOrder = 35;
            var naraPresentation = GetOrAdd<H7NaraPresentation>(nara.gameObject);
            naraPresentation.Configure(nara, player, visual, indicator);
            var characterView = nara.GetComponent<H7CharacterView>();
            characterView?.SetState("Idle");
            _ = mission;
        }

        private static H7CameraPolish BuildCameraPolish(Transform player)
        {
            var cameraObject = GameObject.Find("CM_OfficialCamera");
            if (cameraObject == null)
            {
                throw new InvalidOperationException("H7 requires CM_OfficialCamera.");
            }

            var followTarget = GetOrAdd<H3CinemachineFollowTarget>(cameraObject);
            followTarget.Target = player;
            var camera = GetOrAdd<Unity.Cinemachine.CinemachineCamera>(cameraObject);
            var follow = GetOrAdd<Unity.Cinemachine.CinemachineFollow>(cameraObject);
            var polish = GetOrAdd<H7CameraPolish>(cameraObject);
            polish.Configure(camera, follow);
            return polish;
        }

        private static void BuildWorldBackdrop()
        {
            var greyboxRoot = GameObject.Find("H2_Greybox");
            if (greyboxRoot == null)
            {
                throw new InvalidOperationException("H7 requires H2_Greybox.");
            }

            foreach (var zoneName in new[] { "Zone_Plaza", "Zone_Sendero", "Zone_Cueva" })
            {
                var zone = greyboxRoot.transform.Find(zoneName);
                var meshRenderer = zone?.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = false;
                }
            }

            var navigationLine = greyboxRoot.transform.Find("H2_NavigationPreview")
                ?.GetComponent<LineRenderer>();
            if (navigationLine != null)
            {
                navigationLine.enabled = false;
            }

            var artRoot = FindOrCreate("H7_WorldArt");
            var rendererObject = artRoot.transform.Find("Lumbre_World_Backdrop")?.gameObject;
            if (rendererObject == null)
            {
                rendererObject = new GameObject("Lumbre_World_Backdrop");
                rendererObject.transform.SetParent(artRoot.transform, false);
            }

            var backdropRenderer = GetOrAdd<SpriteRenderer>(rendererObject);
            backdropRenderer.sprite = LoadSprite(BackdropSpritePath);
            backdropRenderer.sortingOrder = -100;
            backdropRenderer.transform.position = new Vector3(3.5f, 7.5f, 0.2f);
            var targetWidth = 31f;
            var spriteWidth = Mathf.Max(0.01f, backdropRenderer.sprite.bounds.size.x);
            var scale = targetWidth / spriteWidth;
            backdropRenderer.transform.localScale = new Vector3(scale, scale, 1f);
            var artLayer = GetOrAdd<H7WorldArtLayer>(artRoot);
            artLayer.Configure(backdropRenderer);
        }

        private static H7StatusHud BuildHud(H4CombatHealth health,
            H4BPlayerAbilityController abilities, H5MissionRuntime mission,
            H6ProgressionRuntime progression)
        {
            var canvas = GameObject.Find("H3_MobileControls")?.GetComponent<Canvas>();
            if (canvas == null)
            {
                throw new InvalidOperationException("H7 requires H3_MobileControls.");
            }

            var statusPanel = FindOrCreateUi("H7_StatusPanel", canvas.transform);
            ConfigurePanel(statusPanel, new Vector2(30f, -30f), new Vector2(680f, 215f),
                new Color(0.025f, 0.055f, 0.1f, 0.94f));
            var status = FindOrCreateText("Status", statusPanel.transform, 22, Color.white);
            ConfigureTextRect(status, new Vector2(24f, -18f), new Vector2(630f, 78f), TextAnchor.UpperLeft);
            var healthBar = CreateBar(statusPanel.transform, "LifeBar", new Vector2(24f, -106f),
                new Color(0.92f, 0.22f, 0.28f, 1f));
            var heatBar = CreateBar(statusPanel.transform, "HeatBar", new Vector2(24f, -140f),
                new Color(1f, 0.52f, 0.16f, 1f));
            var experienceBar = CreateBar(statusPanel.transform, "ExperienceBar", new Vector2(24f, -174f),
                new Color(0.3f, 0.78f, 1f, 1f));

            var missionPanel = FindOrCreateUi("H7_MissionPanel", canvas.transform);
            ConfigurePanel(missionPanel, new Vector2(30f, -258f), new Vector2(820f, 118f),
                new Color(0.03f, 0.08f, 0.12f, 0.92f));
            var missionText = FindOrCreateText("Mission", missionPanel.transform, 18,
                new Color(0.85f, 0.96f, 1f, 1f));
            ConfigureTextRect(missionText, new Vector2(24f, -18f), new Vector2(770f, 85f),
                TextAnchor.UpperLeft);

            var hud = GetOrAdd<H7StatusHud>(statusPanel);
            hud.Configure(health, abilities, mission, progression, status, missionText,
                healthBar, heatBar, experienceBar, statusPanel.GetComponent<RectTransform>());
            return hud;
        }

        private static Slider CreateBar(Transform parent, string name, Vector2 position, Color fillColor)
        {
            var barObject = parent.Find(name)?.gameObject;
            if (barObject == null)
            {
                barObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
                barObject.transform.SetParent(parent, false);
            }

            var rect = barObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(630f, 18f);
            var slider = barObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;
            slider.interactable = false;
            slider.fillRect = null;
            var background = GetOrAdd<Image>(barObject);
            background.color = new Color(0.1f, 0.15f, 0.2f, 1f);
            background.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            background.type = Image.Type.Sliced;
            var fillObject = barObject.transform.Find("Fill")?.gameObject;
            if (fillObject == null)
            {
                fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillObject.transform.SetParent(barObject.transform, false);
            }

            var fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);
            var fill = fillObject.GetComponent<Image>();
            fill.color = fillColor;
            fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            slider.fillRect = fillRect;
            return slider;
        }

        private static GameObject FindOrCreateUi(string name, Transform parent)
        {
            var existing = parent.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(transform => transform.name == name)?.gameObject;
            if (existing != null)
            {
                return existing;
            }

            var created = new GameObject(name, typeof(RectTransform), typeof(Image));
            created.transform.SetParent(parent, false);
            created.layer = ProjectLayers.PlayerUi;
            return created;
        }

        private static void ConfigurePanel(GameObject panel, Vector2 position, Vector2 size, Color color)
        {
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = panel.GetComponent<Image>();
            image.color = color;
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;
            AddOrGetOutline(panel);
        }

        private static Text FindOrCreateText(string name, Transform parent, int fontSize, Color color)
        {
            var textObject = parent.Find(name)?.gameObject;
            if (textObject == null)
            {
                textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
                textObject.transform.SetParent(parent, false);
                textObject.layer = ProjectLayers.PlayerUi;
            }

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static void ConfigureTextRect(Text text, Vector2 position, Vector2 size, TextAnchor anchor)
        {
            var rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            text.alignment = anchor;
            AddOrGetOutline(text.gameObject);
        }

        private static void AddOrGetOutline(GameObject gameObject)
        {
            var outline = GetOrAdd<Outline>(gameObject);
            outline.effectColor = new Color(0f, 0f, 0f, 0.65f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private static RuntimeAnimatorController CreateAnimatorController(string path,
            string visualName, Sprite sprite, string[] states)
        {
            EnsureFolder("Assets/Game/Presentation");
            EnsureFolder(AnimationRoot.TrimEnd('/'));
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            }

            var stateMachine = controller.layers[0].stateMachine;
            var previousStates = stateMachine.states;
            for (var index = 0; index < previousStates.Length; index++)
            {
                stateMachine.RemoveState(previousStates[index].state);
            }

            var parameters = controller.parameters;
            if (!parameters.Any(parameter => parameter.name == "Moving"))
            {
                controller.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            }

            for (var index = 0; index < states.Length; index++)
            {
                var stateName = states[index];
                var clipPath = AnimationRoot + controller.name + "_" + stateName + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null)
                {
                    clip = new AnimationClip { name = controller.name + "_" + stateName };
                    AssetDatabase.CreateAsset(clip, clipPath);
                }

                clip.ClearCurves();
                var frame = new ObjectReferenceKeyframe { time = 0f, value = sprite };
                AnimationUtility.SetObjectReferenceCurve(clip,
                    EditorCurveBinding.PPtrCurve(visualName, typeof(SpriteRenderer), "m_Sprite"),
                    new[] { frame });
                var pulse = stateName == "Attack" || stateName == "Area" || stateName == "Telegraph"
                    ? 1.12f : stateName == "Damage" ? 0.92f : 1f;
                var finalScale = stateName == "Death" ? 0.08f : pulse;
                AnimationUtility.SetEditorCurve(clip,
                    EditorCurveBinding.FloatCurve(visualName, typeof(Transform), "m_LocalScale.x"),
                    new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.16f, pulse),
                        new Keyframe(0.38f, finalScale)));
                AnimationUtility.SetEditorCurve(clip,
                    EditorCurveBinding.FloatCurve(visualName, typeof(Transform), "m_LocalScale.y"),
                    new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.16f, pulse),
                        new Keyframe(0.38f, finalScale)));
                var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
                clipSettings.loopTime = stateName == "Idle" || stateName == "Walk";
                AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
                EditorUtility.SetDirty(clip);
                var state = stateMachine.AddState(stateName);
                state.motion = clip;
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static Sprite LoadSprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                throw new InvalidOperationException($"H7 could not import sprite '{path}'.");
            }

            return sprite;
        }

        private static void ConfigureTexture(string path, int pixelsPerUnit)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static GameObject FindOrCreate(string name)
        {
            var gameObject = GameObject.Find(name);
            return gameObject ?? new GameObject(name);
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component ? component : gameObject.AddComponent<T>();
        }

        private static void SetLayerRecursive(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            foreach (Transform child in root)
            {
                SetLayerRecursive(child, layer);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = path.Substring(0, path.LastIndexOf('/'));
            var folder = path.Substring(path.LastIndexOf('/') + 1);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
