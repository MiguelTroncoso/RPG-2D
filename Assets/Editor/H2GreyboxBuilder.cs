using System;
using System.Collections.Generic;
using System.Linq;
using Lumbre.Game.Client;
using Lumbre.Game.Client.World;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Lumbre.Game.Editor
{
    public static class H2GreyboxBuilder
    {
        private const string GreyboxRootPath = "Assets/Game/Greybox";
        private const string MeshPath = GreyboxRootPath + "/Meshes";
        private const string MaterialPath = GreyboxRootPath + "/Materials";

        public static void Build()
        {
            EnsureFolder("Assets/Game");
            EnsureFolder(GreyboxRootPath);
            EnsureFolder(MeshPath);
            EnsureFolder(MaterialPath);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("H2_Greybox");
            root.AddComponent<GameSceneMarker>().Marker = "H2Greybox";
            root.AddComponent<H2GreyboxRuntime>();

            BuildZone(root.transform, WorldZoneId.Plaza, "Zone_Plaza", new Color(0.25f, 0.52f, 0.72f, 1f), 0f);
            BuildZone(root.transform, WorldZoneId.Trail, "Zone_Sendero", new Color(0.56f, 0.42f, 0.24f, 1f), 0.01f);
            BuildZone(root.transform, WorldZoneId.Cave, "Zone_Cueva", new Color(0.16f, 0.18f, 0.25f, 1f), 0.02f);
            BuildCollisions(root.transform);
            BuildNavigationPreview(root.transform);
            BuildCamera();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ProjectConstants.BootstrapScenePath, true),
                new EditorBuildSettingsScene(ProjectConstants.VerticalSliceScenePath, true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.SaveScene(scene, ProjectConstants.VerticalSliceScenePath);
        }

        private static void BuildZone(
            Transform parent,
            WorldZoneId zone,
            string objectName,
            Color color,
            float z)
        {
            var zoneObject = new GameObject(objectName);
            zoneObject.transform.SetParent(parent, false);

            var mesh = CreateZoneMesh(zone, objectName, z);
            var meshFilter = zoneObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = zoneObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = CreateMaterial(objectName + "Material", color);
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        private static Mesh CreateZoneMesh(WorldZoneId zone, string objectName, float z)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var grid = GreyboxLayout.CreateWalkabilityGrid();

            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var position = new GridPosition(x, y);
                    if (GreyboxLayout.GetZone(position) != zone)
                    {
                        continue;
                    }

                    AddDiamond(vertices, triangles, position, z);
                }
            }

            var path = MeshPath + "/" + objectName + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
            {
                mesh = new Mesh { name = objectName + "Mesh" };
                AssetDatabase.CreateAsset(mesh, path);
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            return mesh;
        }

        private static void AddDiamond(
            List<Vector3> vertices,
            List<int> triangles,
            GridPosition position,
            float z)
        {
            var center = GreyboxWorldCoordinates.ToWorld(position, z);
            var halfWidth = ProjectConstants.IsometricTileWidth * 0.5f;
            var halfHeight = ProjectConstants.IsometricTileHeight * 0.5f;
            var start = vertices.Count;

            vertices.Add(center + new Vector3(0f, halfHeight, 0f));
            vertices.Add(center + new Vector3(halfWidth, 0f, 0f));
            vertices.Add(center + new Vector3(0f, -halfHeight, 0f));
            vertices.Add(center + new Vector3(-halfWidth, 0f, 0f));

            triangles.Add(start);
            triangles.Add(start + 1);
            triangles.Add(start + 2);
            triangles.Add(start);
            triangles.Add(start + 2);
            triangles.Add(start + 3);
        }

        private static Material CreateMaterial(string materialName, Color color)
        {
            var path = MaterialPath + "/" + materialName + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default");
                material = new Material(shader) { name = materialName };
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void BuildCollisions(Transform parent)
        {
            var collisionRoot = new GameObject("H2_Collisions");
            collisionRoot.transform.SetParent(parent, false);

            var bounds = GetWorldBounds();
            const float wallThickness = 0.25f;
            AddBoxCollider(collisionRoot.transform, "WorldBoundaryLeft",
                new Vector2(bounds.min.x - wallThickness * 0.5f, bounds.center.y),
                new Vector2(wallThickness, bounds.size.y));
            AddBoxCollider(collisionRoot.transform, "WorldBoundaryRight",
                new Vector2(bounds.max.x + wallThickness * 0.5f, bounds.center.y),
                new Vector2(wallThickness, bounds.size.y));
            AddBoxCollider(collisionRoot.transform, "WorldBoundaryBottom",
                new Vector2(bounds.center.x, bounds.min.y - wallThickness * 0.5f),
                new Vector2(bounds.size.x, wallThickness));
            AddBoxCollider(collisionRoot.transform, "WorldBoundaryTop",
                new Vector2(bounds.center.x, bounds.max.y + wallThickness * 0.5f),
                new Vector2(bounds.size.x, wallThickness));

            foreach (var obstacle in GreyboxLayout.GetObstacles())
            {
                var obstacleObject = new GameObject("BlockedCell_" + obstacle);
                obstacleObject.transform.SetParent(collisionRoot.transform, false);
                obstacleObject.transform.position = GreyboxWorldCoordinates.ToWorld(obstacle);
                var collider = obstacleObject.AddComponent<PolygonCollider2D>();
                var halfWidth = ProjectConstants.IsometricTileWidth * 0.5f;
                var halfHeight = ProjectConstants.IsometricTileHeight * 0.5f;
                collider.points = new[]
                {
                    new Vector2(0f, halfHeight),
                    new Vector2(halfWidth, 0f),
                    new Vector2(0f, -halfHeight),
                    new Vector2(-halfWidth, 0f)
                };
            }
        }

        private static void AddBoxCollider(Transform parent, string objectName, Vector2 center, Vector2 size)
        {
            var wall = new GameObject(objectName);
            wall.transform.SetParent(parent, false);
            wall.transform.position = center;
            var collider = wall.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static Bounds GetWorldBounds()
        {
            var corners = new[]
            {
                GreyboxWorldCoordinates.ToWorld(new GridPosition(0, 0)),
                GreyboxWorldCoordinates.ToWorld(new GridPosition(ProjectConstants.GreyboxGridWidth - 1, 0)),
                GreyboxWorldCoordinates.ToWorld(new GridPosition(0, ProjectConstants.GreyboxGridHeight - 1)),
                GreyboxWorldCoordinates.ToWorld(new GridPosition(
                    ProjectConstants.GreyboxGridWidth - 1,
                    ProjectConstants.GreyboxGridHeight - 1))
            };

            var minX = corners.Min(point => point.x) - ProjectConstants.IsometricTileWidth * 0.5f;
            var maxX = corners.Max(point => point.x) + ProjectConstants.IsometricTileWidth * 0.5f;
            var minY = corners.Min(point => point.y) - ProjectConstants.IsometricTileHeight * 0.5f;
            var maxY = corners.Max(point => point.y) + ProjectConstants.IsometricTileHeight * 0.5f;
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            bounds.SetMinMax(new Vector3(minX, minY, 0f), new Vector3(maxX, maxY, 0f));
            return bounds;
        }

        private static void BuildNavigationPreview(Transform parent)
        {
            var grid = GreyboxLayout.CreateWalkabilityGrid();
            var start = new GridPosition(ProjectConstants.NavigationStartX, ProjectConstants.NavigationStartY);
            var goal = new GridPosition(ProjectConstants.NavigationGoalX, ProjectConstants.NavigationGoalY);
            var path = GridPathfinder.FindPath(grid, start, goal, ProjectConstants.NavigationNodeBudget);
            if (path.Count == 0)
            {
                throw new InvalidOperationException("H2 greybox route could not connect the plaza and cave.");
            }

            var navigationObject = new GameObject("H2_NavigationPreview");
            navigationObject.transform.SetParent(parent, false);
            var line = navigationObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.sharedMaterial = CreateMaterial("NavigationPreviewMaterial", new Color(1f, 0.85f, 0.2f, 1f));
            line.startWidth = 0.08f;
            line.endWidth = 0.08f;
            line.positionCount = path.Count;
            for (var index = 0; index < path.Count; index++)
            {
                line.SetPosition(index, GreyboxWorldCoordinates.ToWorld(path[index], -0.05f));
            }
        }

        private static void BuildCamera()
        {
            var center = GreyboxWorldCoordinates.ToWorld(new GridPosition(
                ProjectConstants.GreyboxGridWidth / 2,
                ProjectConstants.GreyboxGridHeight / 2));
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(center.x, center.y, -20f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 9f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.055f, 0.09f, 1f);
            AddComponentByTypeName(cameraObject, "Unity.Cinemachine.CinemachineBrain");

            var virtualCameraObject = new GameObject("CM_OfficialCamera");
            virtualCameraObject.transform.position = new Vector3(center.x, center.y, -20f);
            AddComponentByTypeName(virtualCameraObject, "Unity.Cinemachine.CinemachineCamera");
        }

        private static Component AddComponentByTypeName(GameObject gameObject, string fullTypeName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullTypeName, false))
                .FirstOrDefault(candidate => candidate != null);

            if (type == null)
            {
                throw new InvalidOperationException(
                    $"Required package type '{fullTypeName}' was not found. Ensure Cinemachine is resolved.");
            }

            return gameObject.AddComponent(type);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = path.Substring(0, path.LastIndexOf('/'));
            var folder = path.Substring(path.LastIndexOf('/') + 1);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
