using System.Collections.Generic;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Navigation;
using UnityEngine;

namespace Lumbre.Game.Client.World
{
    public sealed class H2GreyboxRuntime : MonoBehaviour
    {
        public IReadOnlyList<GridPosition> NavigationPreviewPath { get; private set; }

        private void Awake()
        {
            var grid = GreyboxLayout.CreateWalkabilityGrid();
            var start = new GridPosition(ProjectConstants.NavigationStartX, ProjectConstants.NavigationStartY);
            var goal = new GridPosition(ProjectConstants.NavigationGoalX, ProjectConstants.NavigationGoalY);
            NavigationPreviewPath = GridPathfinder.FindPath(grid, start, goal, ProjectConstants.NavigationNodeBudget);

            if (NavigationPreviewPath.Count == 0)
            {
                Debug.LogError("H2 greybox navigation preview could not find a route from the plaza to the cave.", this);
            }
        }
    }
}
