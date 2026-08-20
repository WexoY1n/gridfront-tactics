using System;
using System.Collections.Generic;

namespace Gridfront.BattleCore.Pathfinding
{
    public sealed class PathCache
    {
        private readonly GridMap _map;
        private readonly Dictionary<PathCacheKey, PathSearchResult> _results =
            new Dictionary<PathCacheKey, PathSearchResult>();

        public PathCache(GridMap map)
        {
            _map = map ?? throw new ArgumentNullException(nameof(map));
        }

        public int Count => _results.Count;

        public PathSearchResult GetOrFind(GridPos start, GridPos goal, MovementType movementType)
        {
            if (movementType != MovementType.Ground)
            {
                throw new NotSupportedException("Only ground movement is implemented.");
            }

            var key = new PathCacheKey(start, goal, movementType, _map.NavVersion);
            if (_results.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var result = AStarPathfinder.Find(_map, start, goal);
            _results.Add(key, result);
            return result;
        }
    }

    internal readonly struct PathCacheKey : IEquatable<PathCacheKey>
    {
        public PathCacheKey(GridPos start, GridPos goal, MovementType movementType, int navVersion)
        {
            Start = start;
            Goal = goal;
            MovementType = movementType;
            NavVersion = navVersion;
        }

        public GridPos Start { get; }

        public GridPos Goal { get; }

        public MovementType MovementType { get; }

        public int NavVersion { get; }

        public bool Equals(PathCacheKey other)
        {
            return Start == other.Start
                && Goal == other.Goal
                && MovementType == other.MovementType
                && NavVersion == other.NavVersion;
        }

        public override bool Equals(object obj)
        {
            return obj is PathCacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Start.GetHashCode();
                hash = (hash * 397) ^ Goal.GetHashCode();
                hash = (hash * 397) ^ (int)MovementType;
                hash = (hash * 397) ^ NavVersion;
                return hash;
            }
        }
    }
}
