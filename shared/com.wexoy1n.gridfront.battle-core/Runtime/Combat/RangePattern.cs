using System;
using System.Collections.Generic;
using Gridfront.BattleCore.Domain;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Combat
{
    public static class RangePattern
    {
        public static GridPos Rotate(GridPos northRelative, Facing facing)
        {
            switch (facing)
            {
                case Facing.North:
                    return northRelative;
                case Facing.East:
                    return new GridPos(northRelative.Y, -northRelative.X);
                case Facing.South:
                    return new GridPos(-northRelative.X, -northRelative.Y);
                case Facing.West:
                    return new GridPos(-northRelative.Y, northRelative.X);
                default:
                    throw new ArgumentOutOfRangeException(nameof(facing), facing, "Unknown facing.");
            }
        }

        public static IReadOnlyList<GridPos> Resolve(
            GridPos origin,
            Facing facing,
            IReadOnlyList<GridPos> northRelativeCells,
            StageMap map)
        {
            if (northRelativeCells == null)
            {
                throw new ArgumentNullException(nameof(northRelativeCells));
            }

            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            var cells = new List<GridPos>(northRelativeCells.Count);
            for (var i = 0; i < northRelativeCells.Count; i++)
            {
                var offset = Rotate(northRelativeCells[i], facing);
                var cell = new GridPos(origin.X + offset.X, origin.Y + offset.Y);
                if (map.InBounds(cell))
                {
                    cells.Add(cell);
                }
            }

            return cells;
        }
    }
}
