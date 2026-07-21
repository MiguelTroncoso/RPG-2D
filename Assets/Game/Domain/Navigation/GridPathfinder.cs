using System.Collections.Generic;

namespace Lumbre.Game.Domain.Navigation
{
    public static class GridPathfinder
    {
        public static IReadOnlyList<GridPosition> FindPath(
            WalkabilityGrid grid,
            GridPosition start,
            GridPosition goal,
            int nodeBudget)
        {
            if (grid == null || nodeBudget <= 0 || !grid.IsWalkable(start) || !grid.IsWalkable(goal))
            {
                return new GridPosition[0];
            }

            var frontier = new Queue<GridPosition>();
            var previous = new Dictionary<GridPosition, GridPosition>();
            frontier.Enqueue(start);
            previous[start] = start;
            var visited = 0;

            while (frontier.Count > 0 && visited < nodeBudget)
            {
                var current = frontier.Dequeue();
                visited++;

                if (current == goal)
                {
                    return ReconstructPath(previous, start, goal);
                }

                foreach (var neighbor in grid.GetNeighbors(current))
                {
                    if (previous.ContainsKey(neighbor))
                    {
                        continue;
                    }

                    previous[neighbor] = current;
                    frontier.Enqueue(neighbor);
                }
            }

            return new GridPosition[0];
        }

        private static IReadOnlyList<GridPosition> ReconstructPath(
            Dictionary<GridPosition, GridPosition> previous,
            GridPosition start,
            GridPosition goal)
        {
            var result = new List<GridPosition>();
            var current = goal;

            while (current != start)
            {
                result.Add(current);
                current = previous[current];
            }

            result.Add(start);
            result.Reverse();
            return result;
        }
    }
}
