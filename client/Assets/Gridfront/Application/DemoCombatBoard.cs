using System;
using System.Collections.Generic;
using Gridfront.BattleCore.Combat;
using Gridfront.BattleCore.Domain;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.Client.Application
{
    /// <summary>
    /// v0.3 demo board: same 5x4 march map plus deploy pads and a west-facing melee range.
    /// </summary>
    public sealed class DemoCombatBoard
    {
        public DemoCombatBoard(
            GridMap nav,
            GridPath path,
            StageMap stage,
            GridPos start,
            GridPos goal,
            GridPos deployCell,
            Facing facing,
            IReadOnlyList<GridPos> northRelativeRange)
        {
            Nav = nav ?? throw new ArgumentNullException(nameof(nav));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Stage = stage ?? throw new ArgumentNullException(nameof(stage));
            if (northRelativeRange == null)
            {
                throw new ArgumentNullException(nameof(northRelativeRange));
            }

            Start = start;
            Goal = goal;
            DeployCell = deployCell;
            Facing = facing;
            NorthRelativeRange = northRelativeRange;
            RangeCells = RangePattern.Resolve(deployCell, facing, northRelativeRange, stage);
        }

        public GridMap Nav { get; }

        public GridPath Path { get; }

        public StageMap Stage { get; }

        public GridPos Start { get; }

        public GridPos Goal { get; }

        public GridPos DeployCell { get; }

        public Facing Facing { get; }

        public IReadOnlyList<GridPos> NorthRelativeRange { get; }

        public IReadOnlyList<GridPos> RangeCells { get; }

        public static DemoCombatBoard Create()
        {
            const int width = 5;
            const int height = 4;
            var walkable = new bool[width * height];
            var tiles = new TileType[width * height];
            for (var i = 0; i < walkable.Length; i++)
            {
                walkable[i] = true;
                tiles[i] = TileType.Walkable;
            }

            Block(walkable, tiles, width, 1, 1);
            Block(walkable, tiles, width, 2, 1);
            Block(walkable, tiles, width, 3, 1);
            Block(walkable, tiles, width, 1, 2);
            Block(walkable, tiles, width, 2, 2);
            Block(walkable, tiles, width, 3, 2);

            Set(tiles, width, 0, 3, TileType.Spawn);
            Set(tiles, width, 0, 0, TileType.Goal);
            Set(tiles, width, 1, 3, TileType.MeleePad);
            Set(tiles, width, 4, 1, TileType.HighPad);

            var nav = new GridMap(width, height, walkable);
            var start = new GridPos(0, 3);
            var goal = new GridPos(0, 0);
            var cache = new PathCache(nav);
            var search = cache.GetOrFind(start, goal, MovementType.Ground);
            if (!search.Found)
            {
                throw new InvalidOperationException("Demo combat board has no walkable route from start to goal.");
            }

            var deploy = new GridPos(1, 3);
            var facing = Facing.West;
            IReadOnlyList<GridPos> northRelative = new[]
            {
                new GridPos(0, 1),
                new GridPos(-1, 1),
                new GridPos(-2, 1),
                new GridPos(-3, 1)
            };

            return new DemoCombatBoard(
                nav,
                GridPath.FromResult(search),
                new StageMap(width, height, tiles),
                start,
                goal,
                deploy,
                facing,
                northRelative);
        }

        private static void Block(bool[] walkable, TileType[] tiles, int width, int x, int y)
        {
            walkable[(y * width) + x] = false;
            tiles[(y * width) + x] = TileType.Void;
        }

        private static void Set(TileType[] tiles, int width, int x, int y, TileType tile)
        {
            tiles[(y * width) + x] = tile;
        }
    }
}
