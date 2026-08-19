using System;

namespace Gridfront.BattleCore.Pathfinding
{
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }

        public int Y { get; }

        public int Manhattan(GridPos other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        public bool Equals(GridPos other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPos other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return "(" + X + "," + Y + ")";
        }

        public static bool operator ==(GridPos left, GridPos right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridPos left, GridPos right)
        {
            return !left.Equals(right);
        }
    }
}
