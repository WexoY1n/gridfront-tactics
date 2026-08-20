using System;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.Client.Application
{
    /// <summary>
    /// v0.2 demo board. Not a content pipeline; the same 5x4 map the core march test uses.
    /// </summary>
    public sealed class DemoPathMarchBoard
    {
        public DemoPathMarchBoard(GridMap map, GridPath path, GridPos start, GridPos goal)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Start = start;
            Goal = goal;
        }

        public GridMap Map { get; }

        public GridPath Path { get; }

        public GridPos Start { get; }

        public GridPos Goal { get; }

        public static DemoPathMarchBoard Create()
        {
            const int width = 5;
            const int height = 4;
            var walkable = new bool[width * height];
            for (var i = 0; i < walkable.Length; i++)
            {
                walkable[i] = true;
            }

            Block(walkable, width, 1, 1);
            Block(walkable, width, 2, 1);
            Block(walkable, width, 3, 1);
            Block(walkable, width, 1, 2);
            Block(walkable, width, 2, 2);
            Block(walkable, width, 3, 2);

            var map = new GridMap(width, height, walkable);
            var start = new GridPos(0, 3);
            var goal = new GridPos(0, 0);
            var cache = new PathCache(map);
            var search = cache.GetOrFind(start, goal, MovementType.Ground);
            if (!search.Found)
            {
                throw new InvalidOperationException("Demo path march board has no walkable route from start to goal.");
            }

            return new DemoPathMarchBoard(map, GridPath.FromResult(search), start, goal);
        }

        private static void Block(bool[] walkable, int width, int x, int y)
        {
            walkable[(y * width) + x] = false;
        }
    }
}
