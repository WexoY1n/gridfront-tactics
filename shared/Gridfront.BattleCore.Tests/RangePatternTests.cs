using Gridfront.BattleCore.Combat;
using Gridfront.BattleCore.Domain;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Tests
{
    public sealed class RangePatternTests
    {
        [Fact]
        public void RotatesForwardCellWithFacing()
        {
            var forward = new GridPos(0, 1);

            Assert.Equal(new GridPos(0, 1), RangePattern.Rotate(forward, Facing.North));
            Assert.Equal(new GridPos(1, 0), RangePattern.Rotate(forward, Facing.East));
            Assert.Equal(new GridPos(0, -1), RangePattern.Rotate(forward, Facing.South));
            Assert.Equal(new GridPos(-1, 0), RangePattern.Rotate(forward, Facing.West));
        }

        [Fact]
        public void DropsCellsOutsideTheMap()
        {
            var map = new StageMap(2, 2, new[]
            {
                TileType.Walkable, TileType.Walkable,
                TileType.Walkable, TileType.Walkable
            });
            var origin = new GridPos(0, 0);
            var northRelative = new[] { new GridPos(0, 1), new GridPos(0, 2) };

            var cells = RangePattern.Resolve(origin, Facing.North, northRelative, map);

            Assert.Equal(new[] { new GridPos(0, 1) }, cells);
        }
    }
}
