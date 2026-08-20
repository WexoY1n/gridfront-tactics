using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Tests
{
    public sealed class PathFollowerTests
    {
        [Fact]
        public void StopsAtGoal_WithoutLeavingThePath()
        {
            var path = GridPath.FromResult(
                new PathSearchResult(
                    true,
                    new[] { new GridPos(0, 0), new GridPos(1, 0), new GridPos(2, 0) },
                    20));
            var follower = new PathFollower(entityId: 1, path, speedMilliPerTick: 700);

            follower.Step();
            Assert.False(follower.ReachedGoal);
            Assert.Equal(700, follower.DistanceAlongPath);

            follower.Step();
            Assert.False(follower.ReachedGoal);
            Assert.Equal(1400, follower.DistanceAlongPath);

            follower.Step();
            Assert.True(follower.ReachedGoal);
            Assert.Equal(2000, follower.DistanceAlongPath);

            follower.GetPositionMilli(out var x, out var y);
            Assert.Equal(2000, x);
            Assert.Equal(0, y);

            follower.Step();
            Assert.Equal(2000, follower.DistanceAlongPath);
        }

        [Fact]
        public void TwentyEnemies_FollowCachedPath_ToGoal()
        {
            var map = AsciiMaps.Load(
                @"
S....
.###.
.###.
G....",
                out var start,
                out var goal);
            var cache = new PathCache(map);
            var search = cache.GetOrFind(start, goal, MovementType.Ground);
            var path = GridPath.FromResult(search);

            var followers = new PathFollower[20];
            for (var i = 0; i < followers.Length; i++)
            {
                followers[i] = new PathFollower(entityId: i + 1, path, speedMilliPerTick: 250);
            }

            var ticks = 0;
            while (ticks < 10_000)
            {
                var allDone = true;
                for (var i = 0; i < followers.Length; i++)
                {
                    followers[i].Step();
                    allDone &= followers[i].ReachedGoal;
                }

                ticks++;
                if (allDone)
                {
                    break;
                }
            }

            Assert.True(ticks < 10_000);
            for (var i = 0; i < followers.Length; i++)
            {
                Assert.True(followers[i].ReachedGoal);
                Assert.Equal(path.TotalLengthMilli, followers[i].RouteProgress);
                followers[i].GetPositionMilli(out var x, out var y);
                Assert.Equal(goal.X * GridMap.TileUnits, x);
                Assert.Equal(goal.Y * GridMap.TileUnits, y);
            }
        }
    }
}
