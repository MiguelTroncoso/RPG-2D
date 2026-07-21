using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Navigation;
using UnityEngine;

namespace Lumbre.Game.Client.World
{
    public static class GreyboxWorldCoordinates
    {
        public static Vector3 ToWorld(GridPosition position, float z = 0f)
        {
            return new Vector3(
                (position.X - position.Y) * ProjectConstants.IsometricTileWidth * 0.5f,
                (position.X + position.Y) * ProjectConstants.IsometricTileHeight * 0.5f,
                z);
        }
    }
}
