using System.Collections.Generic;
using Lumbre.Game.Domain.Constants;

namespace Lumbre.Game.Domain.Navigation
{
    public static class GreyboxLayout
    {
        public static WalkabilityGrid CreateWalkabilityGrid()
        {
            var grid = new WalkabilityGrid(
                ProjectConstants.GreyboxGridWidth,
                ProjectConstants.GreyboxGridHeight);

            for (var y = 0; y < grid.Height; y++)
            {
                for (var x = 0; x < grid.Width; x++)
                {
                    var position = new GridPosition(x, y);
                    grid.SetWalkable(position, GetZone(position) != WorldZoneId.None);
                }
            }

            foreach (var obstacle in GetObstacles())
            {
                grid.SetWalkable(obstacle, false);
            }

            return grid;
        }

        public static WorldZoneId GetZone(GridPosition position)
        {
            if (IsInside(position, ProjectConstants.CaveMinX, ProjectConstants.CaveMaxX,
                ProjectConstants.CaveMinY, ProjectConstants.CaveMaxY))
            {
                return WorldZoneId.Cave;
            }

            if (IsInside(position, ProjectConstants.PlazaMinX, ProjectConstants.PlazaMaxX,
                ProjectConstants.PlazaMinY, ProjectConstants.PlazaMaxY))
            {
                return WorldZoneId.Plaza;
            }

            if (IsInside(position, ProjectConstants.TrailMinX, ProjectConstants.TrailMaxX,
                ProjectConstants.TrailMinY, ProjectConstants.TrailMaxY))
            {
                return WorldZoneId.Trail;
            }

            return WorldZoneId.None;
        }

        public static IEnumerable<GridPosition> GetObstacles()
        {
            for (var y = ProjectConstants.PlazaObstacleMinY; y <= ProjectConstants.PlazaObstacleMaxY; y++)
            {
                for (var x = ProjectConstants.PlazaObstacleMinX; x <= ProjectConstants.PlazaObstacleMaxX; x++)
                {
                    yield return new GridPosition(x, y);
                }
            }

            for (var y = ProjectConstants.CaveObstacleMinY; y <= ProjectConstants.CaveObstacleMaxY; y++)
            {
                for (var x = ProjectConstants.CaveObstacleMinX; x <= ProjectConstants.CaveObstacleMaxX; x++)
                {
                    yield return new GridPosition(x, y);
                }
            }
        }

        private static bool IsInside(GridPosition position, int minX, int maxX, int minY, int maxY)
        {
            return position.X >= minX && position.X <= maxX && position.Y >= minY && position.Y <= maxY;
        }
    }
}
