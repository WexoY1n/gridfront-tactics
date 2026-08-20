using System;
using System.Collections.Generic;

namespace Gridfront.BattleCore.Pathfinding
{
    public sealed class GridPath
    {
        public GridPath(IReadOnlyList<GridPos> nodes, int cost)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (nodes.Count == 0)
            {
                throw new ArgumentException("A path must contain at least one node.", nameof(nodes));
            }

            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cost), cost, "Cost must be non-negative.");
            }

            for (var i = 1; i < nodes.Count; i++)
            {
                if (nodes[i].Manhattan(nodes[i - 1]) != 1)
                {
                    throw new ArgumentException(
                        "Path nodes must be orthogonal neighbors.",
                        nameof(nodes));
                }
            }

            Nodes = nodes;
            Cost = cost;
            TotalLengthMilli = (nodes.Count - 1) * GridMap.TileUnits;
        }

        public IReadOnlyList<GridPos> Nodes { get; }

        public int Cost { get; }

        public int TotalLengthMilli { get; }

        public static GridPath FromResult(PathSearchResult result)
        {
            if (!result.Found)
            {
                throw new InvalidOperationException("Cannot build a GridPath from a failed search.");
            }

            return new GridPath(result.Nodes, result.Cost);
        }

        public int RouteProgress(int distanceAlongPath)
        {
            if (distanceAlongPath < 0 || distanceAlongPath > TotalLengthMilli)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(distanceAlongPath),
                    distanceAlongPath,
                    "Distance must stay on the path.");
            }

            return distanceAlongPath;
        }

        public void GetPositionMilli(int distanceAlongPath, out int xMilli, out int yMilli)
        {
            var travelled = RouteProgress(distanceAlongPath);
            if (TotalLengthMilli == 0)
            {
                xMilli = Nodes[0].X * GridMap.TileUnits;
                yMilli = Nodes[0].Y * GridMap.TileUnits;
                return;
            }

            var segment = travelled / GridMap.TileUnits;
            if (segment >= Nodes.Count - 1)
            {
                var last = Nodes[Nodes.Count - 1];
                xMilli = last.X * GridMap.TileUnits;
                yMilli = last.Y * GridMap.TileUnits;
                return;
            }

            var remaining = travelled - (segment * GridMap.TileUnits);
            var from = Nodes[segment];
            var to = Nodes[segment + 1];
            xMilli = (from.X * GridMap.TileUnits) + ((to.X - from.X) * remaining);
            yMilli = (from.Y * GridMap.TileUnits) + ((to.Y - from.Y) * remaining);
        }
    }
}
