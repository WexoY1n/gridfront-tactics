using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Tests
{
    public sealed class RouteProgressTests
    {
        [Fact]
        public void ProgressIsStrictlyMonotonicAcrossACorner()
        {
            var path = new GridPath(
                new[]
                {
                    new GridPos(0, 0),
                    new GridPos(1, 0),
                    new GridPos(1, 1)
                },
                cost: 20);

            Assert.Equal(2000, path.TotalLengthMilli);

            var previous = -1;
            for (var distance = 0; distance <= path.TotalLengthMilli; distance++)
            {
                var progress = path.RouteProgress(distance);
                Assert.Equal(distance, progress);
                Assert.True(progress > previous);
                previous = progress;
            }
        }

        [Fact]
        public void PositionFollowsOrthogonalSegments()
        {
            var path = new GridPath(
                new[]
                {
                    new GridPos(0, 0),
                    new GridPos(1, 0),
                    new GridPos(1, 1)
                },
                cost: 20);

            path.GetPositionMilli(0, out var x0, out var y0);
            Assert.Equal(0, x0);
            Assert.Equal(0, y0);

            path.GetPositionMilli(400, out var x1, out var y1);
            Assert.Equal(400, x1);
            Assert.Equal(0, y1);

            path.GetPositionMilli(1000, out var x2, out var y2);
            Assert.Equal(1000, x2);
            Assert.Equal(0, y2);

            path.GetPositionMilli(1500, out var x3, out var y3);
            Assert.Equal(1000, x3);
            Assert.Equal(500, y3);

            path.GetPositionMilli(2000, out var x4, out var y4);
            Assert.Equal(1000, x4);
            Assert.Equal(1000, y4);
        }

        [Fact]
        public void DistancePastThePath_Throws()
        {
            var path = new GridPath(new[] { new GridPos(0, 0), new GridPos(1, 0) }, cost: 10);

            Assert.Throws<ArgumentOutOfRangeException>(() => path.RouteProgress(1001));
        }
    }
}
