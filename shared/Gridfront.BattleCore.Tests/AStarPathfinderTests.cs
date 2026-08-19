using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Tests
{
    public sealed class AStarPathfinderTests
    {
        [Fact]
        public void FindsUniqueShortestPath()
        {
            var map = AsciiMaps.Load(
                @"
S#
.#
.#
G#",
                out var start,
                out var goal);

            var result = AStarPathfinder.Find(map, start, goal);

            Assert.True(result.Found);
            Assert.Equal(
                new[]
                {
                    new GridPos(0, 3),
                    new GridPos(0, 2),
                    new GridPos(0, 1),
                    new GridPos(0, 0)
                },
                result.Nodes);
            Assert.Equal(30, result.Cost);
        }

        [Fact]
        public void EqualCostPaths_AreStableAndMatchLockedRoute()
        {
            var map = AsciiMaps.Load(
                @"
S..
...
..G",
                out var start,
                out var goal);

            var first = AStarPathfinder.Find(map, start, goal);
            var second = AStarPathfinder.Find(map, start, goal);

            Assert.True(first.Found);
            Assert.Equal(first.Nodes, second.Nodes);
            Assert.Equal(first.Cost, second.Cost);
            Assert.Equal(40, first.Cost);
            Assert.Equal(
                new[]
                {
                    new GridPos(0, 2),
                    new GridPos(0, 1),
                    new GridPos(0, 0),
                    new GridPos(1, 0),
                    new GridPos(2, 0)
                },
                first.Nodes);
        }

        [Fact]
        public void DisconnectedMap_ReturnsNotFound()
        {
            var map = AsciiMaps.Load(
                @"
S.#
###
..G",
                out var start,
                out var goal);

            var result = AStarPathfinder.Find(map, start, goal);

            Assert.False(result.Found);
            Assert.Empty(result.Nodes);
            Assert.Equal(0, result.Cost);
        }

        [Fact]
        public void UnwalkableStart_ReturnsNotFound()
        {
            var walkable = new[] { false, true };
            var map = new GridMap(2, 1, walkable);

            var result = AStarPathfinder.Find(map, new GridPos(0, 0), new GridPos(1, 0));

            Assert.False(result.Found);
        }

        [Fact]
        public void StartEqualsGoal_ReturnsSingleNode()
        {
            var start = new GridPos(0, 0);
            var map = new GridMap(1, 1, new[] { true });

            var result = AStarPathfinder.Find(map, start, start);

            Assert.True(result.Found);
            Assert.Equal(new[] { start }, result.Nodes);
            Assert.Equal(0, result.Cost);
        }

        [Fact]
        public void OutOfBounds_Throws()
        {
            var map = new GridMap(1, 1, new[] { true });

            Assert.Throws<ArgumentOutOfRangeException>(
                () => AStarPathfinder.Find(map, new GridPos(0, 0), new GridPos(1, 0)));
        }

        [Fact]
        public void Waypoints_StitchSegments_AndFailLoudlyIfAnySegmentIsBlocked()
        {
            var map = AsciiMaps.Load(
                @"
S.#G
..#.
....",
                out var start,
                out var goal);
            var waypoint = new GridPos(0, 0);

            var routed = AStarPathfinder.FindViaWaypoints(
                map,
                new[] { start, waypoint, goal });

            Assert.True(routed.Found);
            Assert.Equal(start, routed.Nodes[0]);
            Assert.Contains(waypoint, routed.Nodes);
            Assert.Equal(goal, routed.Nodes[routed.Nodes.Count - 1]);

            var blocked = AsciiMaps.Load(
                @"
S.#G
###.
....",
                out var blockedStart,
                out var blockedGoal);

            var failed = AStarPathfinder.FindViaWaypoints(
                blocked,
                new[] { blockedStart, new GridPos(1, 0), blockedGoal });

            Assert.False(failed.Found);
            Assert.Empty(failed.Nodes);
        }
    }
}
