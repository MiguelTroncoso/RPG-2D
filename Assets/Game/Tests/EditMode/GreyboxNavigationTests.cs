using System.Linq;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Navigation;
using NUnit.Framework;

namespace Lumbre.Game.Tests
{
    public sealed class GreyboxNavigationTests
    {
        [Test]
        public void GreyboxRouteConnectsPlazaToCaveAroundObstacles()
        {
            var grid = GreyboxLayout.CreateWalkabilityGrid();
            var start = new GridPosition(ProjectConstants.NavigationStartX, ProjectConstants.NavigationStartY);
            var goal = new GridPosition(ProjectConstants.NavigationGoalX, ProjectConstants.NavigationGoalY);
            var path = GridPathfinder.FindPath(grid, start, goal, ProjectConstants.NavigationNodeBudget);

            Assert.IsNotEmpty(path);
            Assert.AreEqual(start, path.First());
            Assert.AreEqual(goal, path.Last());
            Assert.IsTrue(path.All(grid.IsWalkable));
            Assert.IsFalse(grid.IsWalkable(new GridPosition(
                ProjectConstants.PlazaObstacleMinX,
                ProjectConstants.PlazaObstacleMinY)));
        }

        [Test]
        public void GreyboxUsesOnlyTheThreeApprovedZones()
        {
            var grid = GreyboxLayout.CreateWalkabilityGrid();
            var zones = Enumerable.Range(0, grid.Width)
                .SelectMany(x => Enumerable.Range(0, grid.Height).Select(y => GreyboxLayout.GetZone(new GridPosition(x, y))))
                .Where(zone => zone != WorldZoneId.None)
                .Distinct()
                .ToArray();

            CollectionAssert.AreEquivalent(
                new[] { WorldZoneId.Plaza, WorldZoneId.Trail, WorldZoneId.Cave },
                zones);
        }

        [Test]
        public void GreyboxRouteSupportsReturningFromCaveToPlaza()
        {
            var grid = GreyboxLayout.CreateWalkabilityGrid();
            var start = new GridPosition(ProjectConstants.NavigationStartX, ProjectConstants.NavigationStartY);
            var goal = new GridPosition(ProjectConstants.NavigationGoalX, ProjectConstants.NavigationGoalY);
            var path = GridPathfinder.FindPath(grid, goal, start, ProjectConstants.NavigationNodeBudget);

            Assert.IsNotEmpty(path);
            Assert.AreEqual(goal, path.First());
            Assert.AreEqual(start, path.Last());
            Assert.IsTrue(path.All(grid.IsWalkable));
        }
    }
}
