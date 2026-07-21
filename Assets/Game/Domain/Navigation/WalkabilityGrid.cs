using System.Collections.Generic;

namespace Lumbre.Game.Domain.Navigation
{
    public sealed class WalkabilityGrid
    {
        private readonly bool[,] _walkable;

        public WalkabilityGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _walkable = new bool[width, height];
        }

        public int Width { get; }
        public int Height { get; }

        public bool Contains(GridPosition position)
        {
            return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
        }

        public bool IsWalkable(GridPosition position)
        {
            return Contains(position) && _walkable[position.X, position.Y];
        }

        public void SetWalkable(GridPosition position, bool walkable)
        {
            if (Contains(position))
            {
                _walkable[position.X, position.Y] = walkable;
            }
        }

        public IEnumerable<GridPosition> GetNeighbors(GridPosition position)
        {
            var candidates = new[]
            {
                new GridPosition(position.X + 1, position.Y),
                new GridPosition(position.X, position.Y + 1),
                new GridPosition(position.X, position.Y - 1),
                new GridPosition(position.X - 1, position.Y)
            };

            foreach (var candidate in candidates)
            {
                if (IsWalkable(candidate))
                {
                    yield return candidate;
                }
            }
        }
    }
}
