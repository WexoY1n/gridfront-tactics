using System;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Tests
{
    internal static class AsciiMaps
    {
        public static GridMap Load(string map, out GridPos start, out GridPos goal)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            var rawLines = map.Replace("\r\n", "\n").Trim().Split('\n');
            var height = rawLines.Length;
            if (height == 0)
            {
                throw new ArgumentException("Map is empty.", nameof(map));
            }

            var width = rawLines[0].Length;
            var walkable = new bool[width * height];
            start = default;
            goal = default;
            var hasStart = false;
            var hasGoal = false;

            for (var row = 0; row < height; row++)
            {
                var line = rawLines[row];
                if (line.Length != width)
                {
                    throw new ArgumentException("ASCII map rows must have equal length.", nameof(map));
                }

                var y = height - 1 - row;
                for (var x = 0; x < width; x++)
                {
                    var cell = line[x];
                    var index = (y * width) + x;
                    switch (cell)
                    {
                        case '.':
                            walkable[index] = true;
                            break;
                        case 'S':
                            walkable[index] = true;
                            start = new GridPos(x, y);
                            hasStart = true;
                            break;
                        case 'G':
                            walkable[index] = true;
                            goal = new GridPos(x, y);
                            hasGoal = true;
                            break;
                        case '#':
                            walkable[index] = false;
                            break;
                        default:
                            throw new ArgumentException($"Unexpected map character '{cell}'.", nameof(map));
                    }
                }
            }

            if (!hasStart)
            {
                throw new ArgumentException("Map must contain S.", nameof(map));
            }

            if (!hasGoal)
            {
                throw new ArgumentException("Map must contain G.", nameof(map));
            }

            return new GridMap(width, height, walkable);
        }
    }
}
