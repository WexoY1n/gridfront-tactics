using System;

namespace Gridfront.BattleCore.Pathfinding
{
    public sealed class GridMap
    {
        public const int TileUnits = 1000;

        private readonly bool[] _walkable;

        public GridMap(int width, int height, bool[] walkable, int navVersion = 0)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");
            }

            if (walkable == null)
            {
                throw new ArgumentNullException(nameof(walkable));
            }

            if ((long)width * height != walkable.Length)
            {
                throw new ArgumentException(
                    "Walkable buffer length must equal width * height.",
                    nameof(walkable));
            }

            Width = width;
            Height = height;
            NavVersion = navVersion;
            _walkable = new bool[walkable.Length];
            Array.Copy(walkable, _walkable, walkable.Length);
        }

        public int Width { get; }

        public int Height { get; }

        public int NavVersion { get; }

        public int CellCount => _walkable.Length;

        public bool InBounds(GridPos pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;
        }

        public int ToIndex(GridPos pos)
        {
            if (!InBounds(pos))
            {
                throw new ArgumentOutOfRangeException(nameof(pos), pos, "Position is outside the map.");
            }

            return (pos.Y * Width) + pos.X;
        }

        public bool IsWalkable(GridPos pos)
        {
            return _walkable[ToIndex(pos)];
        }
    }
}
