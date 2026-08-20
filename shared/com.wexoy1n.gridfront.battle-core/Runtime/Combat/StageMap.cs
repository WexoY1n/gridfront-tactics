using System;
using Gridfront.BattleCore.Domain;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Combat
{
    public sealed class StageMap
    {
        private readonly TileType[] _tiles;

        public StageMap(int width, int height, TileType[] tiles)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be positive.");
            }

            if (tiles == null)
            {
                throw new ArgumentNullException(nameof(tiles));
            }

            if ((long)width * height != tiles.Length)
            {
                throw new ArgumentException("Tile buffer length must equal width * height.", nameof(tiles));
            }

            Width = width;
            Height = height;
            _tiles = new TileType[tiles.Length];
            Array.Copy(tiles, _tiles, tiles.Length);
        }

        public int Width { get; }

        public int Height { get; }

        public bool InBounds(GridPos pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;
        }

        public TileType TileAt(GridPos pos)
        {
            if (!InBounds(pos))
            {
                throw new ArgumentOutOfRangeException(nameof(pos), pos, "Position is outside the map.");
            }

            return _tiles[(pos.Y * Width) + pos.X];
        }
    }
}
