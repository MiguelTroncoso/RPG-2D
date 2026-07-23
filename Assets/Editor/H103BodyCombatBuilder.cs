using System;
using Lumbre.Game.Client;
using Lumbre.Game.Client.Combat;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Client.Presentation;
using Lumbre.Game.Domain.Constants;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Lumbre.Game.Editor
{
    /// <summary>
    /// H10.3 pass: assembles the cutout body rig for the player from the
    /// sliced bastion-brasa parts. The pass is idempotent: rerunning it
    /// converges to the same scene without duplicating rig nodes. The single
    /// full-body renderer is kept (disabled) so the H7 presentation contract
    /// and its animator clips remain intact.
    /// </summary>
    public static class H103BodyCombatBuilder
    {
        private const string ScenePath = ProjectConstants.VerticalSliceScenePath;
        private const string PartsRoot =
            "Assets/Game/Presentation/Art/Characters/BastionParts/";

        private struct RigPart
        {
            public string NodeName;
            public string SpriteFile;
            public bool ChildOfTorso;
            public Vector2 LocalPosition;
            public Vector2 Pivot;
            public int SortingOrder;
        }

        // Positions place each part pivot (its joint) at the exact source
        // illustration location: (pixel - center 627) / 512 PPU, y up.
        private static readonly RigPart[] Parts =
        {
            new RigPart
            {
                NodeName = "Rig_Cape", SpriteFile = "bastion-cape.png",
                ChildOfTorso = false,
                LocalPosition = new Vector2(0.064453125f, 0.443359375f),
                Pivot = new Vector2(0.369914f, 0.816754f), SortingOrder = 28
            },
            new RigPart
            {
                NodeName = "Rig_LegFront", SpriteFile = "bastion-leg-l.png",
                ChildOfTorso = false,
                LocalPosition = new Vector2(-0.298828125f, -0.314453125f),
                Pivot = new Vector2(0.643172f, 0.984375f), SortingOrder = 30
            },
            new RigPart
            {
                NodeName = "Rig_LegBack", SpriteFile = "bastion-leg-r.png",
                ChildOfTorso = false,
                LocalPosition = new Vector2(0.052734375f, -0.314453125f),
                Pivot = new Vector2(0.155405f, 0.989822f), SortingOrder = 30
            },
            new RigPart
            {
                NodeName = "Rig_Torso", SpriteFile = "bastion-torso.png",
                ChildOfTorso = false,
                LocalPosition = new Vector2(-0.130859375f, -0.044921875f),
                Pivot = new Vector2(0.339744f, 0.441498f), SortingOrder = 31
            },
            new RigPart
            {
                NodeName = "Rig_Head", SpriteFile = "bastion-head.png",
                ChildOfTorso = true,
                LocalPosition = new Vector2(0.029296875f, 0.62109375f),
                Pivot = new Vector2(0.433824f, 0.078853f), SortingOrder = 29
            },
            new RigPart
            {
                NodeName = "Rig_ArmSword", SpriteFile = "bastion-arm-sword.png",
                ChildOfTorso = true,
                LocalPosition = new Vector2(-0.1796875f, 0.35546875f),
                Pivot = new Vector2(0.909091f, 0.918773f), SortingOrder = 32
            },
            new RigPart
            {
                NodeName = "Rig_Shield", SpriteFile = "bastion-shield.png",
                ChildOfTorso = true,
                LocalPosition = new Vector2(0.40625f, 0.21484375f),
                Pivot = new Vector2(0.446301f, 0.859189f), SortingOrder = 33
            }
        };

        public static void Build()
        {
            H10PlayerControlBuilder.Build();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            foreach (var part in Parts)
            {
                ConfigurePartTexture(PartsRoot + part.SpriteFile, part.Pivot);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            var player = GameObject.FindWithTag("Player");
            var visual = player?.transform.Find("H7_Player_Visual");
            if (player == null || visual == null)
            {
                throw new InvalidOperationException(
                    "H10.3 requires the H7 player visual.");
            }

            // The single-illustration renderer stays for the H7 contract and
            // its animator bindings, but the rig draws the body from now on.
            var baseRenderer = visual.GetComponent<SpriteRenderer>();
            if (baseRenderer != null)
            {
                baseRenderer.enabled = false;
            }

            var visualScale = visual.localScale.x;
            var visualZ = visual.localPosition.z;

            // The rig is a SIBLING of H7_Player_Visual, not a child: the H7
            // animator scales the visual transform, and nesting the rig there
            // would reintroduce a body-wide scale pulse. As a direct child of
            // the player the rig is driven only by the H10.3 skeletal poses.
            var rigRoot = player.transform.Find(H103BodyRigView.RigRootName);
            if (rigRoot == null)
            {
                rigRoot = new GameObject(H103BodyRigView.RigRootName).transform;
                rigRoot.SetParent(player.transform, false);
            }

            rigRoot.localPosition = new Vector3(0f, 0f, visualZ);
            rigRoot.localRotation = Quaternion.identity;
            rigRoot.localScale = Vector3.one * visualScale;

            var torso = BuildPart(rigRoot, FindPart("Rig_Torso"));
            BuildPart(rigRoot, FindPart("Rig_Cape"));
            BuildPart(rigRoot, FindPart("Rig_LegFront"));
            BuildPart(rigRoot, FindPart("Rig_LegBack"));
            BuildPart(torso, FindPart("Rig_Head"));
            BuildPart(torso, FindPart("Rig_ArmSword"));
            BuildPart(torso, FindPart("Rig_Shield"));
            SetLayerRecursive(rigRoot, ProjectLayers.Player);

            var rigView = GetOrAdd<H103BodyRigView>(player);
            rigView.Configure(rigRoot);

            var marker = GameObject.Find("H2_Greybox")?.GetComponent<GameSceneMarker>();
            if (marker != null)
            {
                marker.Marker = "H10_3BodyAnimationAndVisibleCombat";
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
            var visual = player?.transform.Find("H7_Player_Visual");
            var rigRoot = player?.transform.Find(H103BodyRigView.RigRootName);
            var rigView = player?.GetComponent<H103BodyRigView>();
            if (player == null || visual == null || rigRoot == null || rigView == null)
            {
                throw new InvalidOperationException(
                    "H10.3 rig root or rig view is missing.");
            }

            if (!rigView.IsRigReady)
            {
                throw new InvalidOperationException(
                    "H10.3 rig view has unresolved part references.");
            }

            var baseRenderer = visual.GetComponent<SpriteRenderer>();
            if (baseRenderer == null || baseRenderer.enabled
                || baseRenderer.sprite == null)
            {
                throw new InvalidOperationException(
                    "H10.3 requires the base visual renderer disabled with its "
                    + "sprite retained for the H7 contract.");
            }

            foreach (var part in Parts)
            {
                var parent = part.ChildOfTorso
                    ? rigRoot.Find("Rig_Torso")
                    : rigRoot;
                var node = parent?.Find(part.NodeName);
                var renderer = node?.GetComponent<SpriteRenderer>();
                if (node == null || renderer == null || renderer.sprite == null)
                {
                    throw new InvalidOperationException(
                        $"H10.3 rig part '{part.NodeName}' is missing its sprite.");
                }

                if (renderer.sortingOrder != part.SortingOrder)
                {
                    throw new InvalidOperationException(
                        $"H10.3 rig part '{part.NodeName}' has sorting order "
                        + $"{renderer.sortingOrder}, expected {part.SortingOrder}.");
                }
            }

            var duplicates = rigRoot.parent.GetComponentsInChildren<Transform>(true);
            var rigCount = 0;
            foreach (var candidate in duplicates)
            {
                if (candidate.name == H103BodyRigView.RigRootName)
                {
                    rigCount++;
                }
            }

            if (rigCount != 1)
            {
                throw new InvalidOperationException(
                    $"H10.3 expects exactly one rig root, found {rigCount}.");
            }

            foreach (var enemy in UnityEngine.Object.FindObjectsByType<MordeluzController>(
                FindObjectsSortMode.None))
            {
                if (enemy.GetComponent<H4CombatFeedback>() == null)
                {
                    throw new InvalidOperationException(
                        $"H10.3 enemy '{enemy.name}' lost its combat feedback.");
                }
            }

            Debug.Log($"H10.3 body animation validation passed in {scene.name}: "
                + $"rig parts={Parts.Length}, impact delay="
                + $"{ProjectConstants.PlayerAttackWindupSeconds + ProjectConstants.PlayerAttackStrikeSeconds:0.00}s.");
        }

        private static RigPart FindPart(string nodeName)
        {
            foreach (var part in Parts)
            {
                if (part.NodeName == nodeName)
                {
                    return part;
                }
            }

            throw new InvalidOperationException($"Unknown rig part '{nodeName}'.");
        }

        private static Transform BuildPart(Transform parent, RigPart part)
        {
            var node = parent.Find(part.NodeName);
            if (node == null)
            {
                node = new GameObject(part.NodeName).transform;
                node.SetParent(parent, false);
            }

            node.localPosition = new Vector3(part.LocalPosition.x,
                part.LocalPosition.y, 0f);
            node.localRotation = Quaternion.identity;
            node.localScale = Vector3.one;

            var renderer = node.GetComponent<SpriteRenderer>();
            if (!renderer)
            {
                renderer = node.gameObject.AddComponent<SpriteRenderer>();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                PartsRoot + part.SpriteFile);
            if (sprite == null)
            {
                throw new InvalidOperationException(
                    $"H10.3 could not load part sprite '{part.SpriteFile}'.");
            }

            renderer.sprite = sprite;
            renderer.sortingOrder = part.SortingOrder;
            renderer.drawMode = SpriteDrawMode.Simple;
            return node;
        }

        private static void ConfigurePartTexture(string path, Vector2 pivot)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 512;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 2048;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static T GetOrAdd<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private static void SetLayerRecursive(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            foreach (Transform child in root)
            {
                SetLayerRecursive(child, layer);
            }
        }
    }
}
