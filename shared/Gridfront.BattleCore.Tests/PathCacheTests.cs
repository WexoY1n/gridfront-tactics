using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Tests
{
    public sealed class PathCacheTests
    {
        [Fact]
        public void CachesSearchByStartGoalMovementAndNavVersion()
        {
            var map = AsciiMaps.Load(
                @"
S.#
..G",
                out var start,
                out var goal);
            var cache = new PathCache(map);

            var first = cache.GetOrFind(start, goal, MovementType.Ground);
            var second = cache.GetOrFind(start, goal, MovementType.Ground);

            Assert.True(first.Found);
            Assert.Same(first.Nodes, second.Nodes);
            Assert.Equal(1, cache.Count);

            var reverse = cache.GetOrFind(goal, start, MovementType.Ground);
            Assert.True(reverse.Found);
            Assert.Equal(2, cache.Count);
        }

        [Fact]
        public void CachesNotFoundResults()
        {
            var map = AsciiMaps.Load(
                @"
S.#
###
..G",
                out var start,
                out var goal);
            var cache = new PathCache(map);

            var first = cache.GetOrFind(start, goal, MovementType.Ground);
            var second = cache.GetOrFind(start, goal, MovementType.Ground);

            Assert.False(first.Found);
            Assert.False(second.Found);
            Assert.Equal(1, cache.Count);
        }
    }
}
