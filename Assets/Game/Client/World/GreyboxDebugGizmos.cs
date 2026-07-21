using System.Collections.Generic;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Navigation;
using UnityEngine;

namespace Lumbre.Game.Client.World
{
    [ExecuteAlways]
    public sealed class GreyboxDebugGizmos : MonoBehaviour
    {
        [SerializeField] private bool drawGrid = true;
        [SerializeField] private bool drawObstacles = true;
        [SerializeField] private bool drawBfsRoute = true;
        [SerializeField] private bool drawRespawn = true;

        private void OnDrawGizmos()
        {
            var grid = GreyboxLayout.CreateWalkabilityGrid();

            if (drawGrid)
            {
                DrawGrid(grid);
            }

            if (drawObstacles)
            {
                DrawObstacles();
            }

            if (drawBfsRoute)
            {
                DrawRoute(grid);
            }

            if (drawRespawn)
            {
                DrawRespawn();
            }
        }

        private static void DrawGrid(WalkabilityGrid grid)
        {
            Gizmos.color = new Color(0.65f, 0.8f, 1f, 0.18f);
            var halfWidth = ProjectConstants.IsometricTileWidth * 0.5f;
            var halfHeight = ProjectConstants.IsometricTileHeight * 0.5f;

            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var center = GreyboxWorldCoordinates.ToWorld(new GridPosition(x, y), -0.1f);
                    var north = center + new Vector3(0f, halfHeight, 0f);
                    var east = center + new Vector3(halfWidth, 0f, 0f);
                    var south = center + new Vector3(0f, -halfHeight, 0f);
                    var west = center + new Vector3(-halfWidth, 0f, 0f);
                    Gizmos.DrawLine(north, east);
                    Gizmos.DrawLine(east, south);
                    Gizmos.DrawLine(south, west);
                    Gizmos.DrawLine(west, north);
                }
            }
        }

        private static void DrawObstacles()
        {
            Gizmos.color = new Color(1f, 0.25f, 0.2f, 0.65f);
            foreach (var obstacle in GreyboxLayout.GetObstacles())
            {
                var center = GreyboxWorldCoordinates.ToWorld(obstacle, -0.15f);
                var halfWidth = ProjectConstants.IsometricTileWidth * 0.5f;
                var halfHeight = ProjectConstants.IsometricTileHeight * 0.5f;
                var points = new[]
                {
                    center + new Vector3(0f, halfHeight, 0f),
                    center + new Vector3(halfWidth, 0f, 0f),
                    center + new Vector3(0f, -halfHeight, 0f),
                    center + new Vector3(-halfWidth, 0f, 0f)
                };

                for (var index = 0; index < points.Length; index++)
                {
                    Gizmos.DrawLine(points[index], points[(index + 1) % points.Length]);
                }
            }
        }

        private static void DrawRoute(WalkabilityGrid grid)
        {
            var start = new GridPosition(ProjectConstants.NavigationStartX, ProjectConstants.NavigationStartY);
            var goal = new GridPosition(ProjectConstants.NavigationGoalX, ProjectConstants.NavigationGoalY);
            var route = GridPathfinder.FindPath(grid, start, goal, ProjectConstants.NavigationNodeBudget);
            if (route.Count == 0)
            {
                return;
            }

            Gizmos.color = new Color(1f, 0.85f, 0.15f, 0.95f);
            for (var index = 0; index < route.Count; index++)
            {
                var position = GreyboxWorldCoordinates.ToWorld(route[index], -0.2f);
                Gizmos.DrawSphere(position, 0.08f);
                if (index + 1 < route.Count)
                {
                    Gizmos.DrawLine(position,
                        GreyboxWorldCoordinates.ToWorld(route[index + 1], -0.2f));
                }
            }
        }

        private static void DrawRespawn()
        {
            var respawn = new GridPosition(ProjectConstants.NavigationStartX, ProjectConstants.NavigationStartY);
            var position = GreyboxWorldCoordinates.ToWorld(respawn, -0.25f);
            Gizmos.color = new Color(0.2f, 1f, 0.85f, 1f);
            Gizmos.DrawWireSphere(position, 0.2f);
            Gizmos.DrawLine(position + Vector3.left * 0.3f, position + Vector3.right * 0.3f);
            Gizmos.DrawLine(position + Vector3.down * 0.3f, position + Vector3.up * 0.3f);
        }
    }
}
