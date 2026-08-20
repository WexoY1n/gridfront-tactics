using System;
using System.Collections.Generic;

namespace Gridfront.BattleCore.Pathfinding
{
    /// <summary>
    /// Orthogonal A* with stable tie-breaks. Neighbor order: +X, -X, +Y, -Y.
    /// Grid index is <c>y * width + x</c>.
    /// </summary>
    public static class AStarPathfinder
    {
        private static readonly GridPos[] NeighborOffsets =
        {
            new GridPos(1, 0),
            new GridPos(-1, 0),
            new GridPos(0, 1),
            new GridPos(0, -1)
        };

        public const int OrthogonalCost = 10;

        public static PathSearchResult Find(GridMap map, GridPos start, GridPos goal)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            var startIndex = map.ToIndex(start);
            var goalIndex = map.ToIndex(goal);

            if (!map.IsWalkable(start) || !map.IsWalkable(goal))
            {
                return PathSearchResult.NotFound;
            }

            if (startIndex == goalIndex)
            {
                return new PathSearchResult(true, new[] { start }, 0);
            }

            var nodes = new Node[map.CellCount];
            var open = new List<int>(32);

            nodes[startIndex] = new Node(start, startIndex, 0, Heuristic(start, goal), -1);
            open.Add(startIndex);

            while (open.Count > 0)
            {
                var currentIndex = PopBest(open, nodes);
                var current = nodes[currentIndex];
                current.Closed = true;

                if (currentIndex == goalIndex)
                {
                    return Reconstruct(nodes, start, currentIndex, current.G);
                }

                for (var i = 0; i < NeighborOffsets.Length; i++)
                {
                    var offset = NeighborOffsets[i];
                    var nextPos = new GridPos(current.Pos.X + offset.X, current.Pos.Y + offset.Y);
                    if (!map.InBounds(nextPos) || !map.IsWalkable(nextPos))
                    {
                        continue;
                    }

                    var nextIndex = map.ToIndex(nextPos);
                    var candidateG = current.G + OrthogonalCost;
                    var existing = nodes[nextIndex];

                    if (existing != null && (existing.Closed || existing.G <= candidateG))
                    {
                        continue;
                    }

                    nodes[nextIndex] = new Node(nextPos, nextIndex, candidateG, Heuristic(nextPos, goal), currentIndex);

                    if (existing == null)
                    {
                        open.Add(nextIndex);
                    }
                }
            }

            return PathSearchResult.NotFound;
        }

        public static PathSearchResult FindViaWaypoints(GridMap map, IReadOnlyList<GridPos> waypoints)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            if (waypoints == null)
            {
                throw new ArgumentNullException(nameof(waypoints));
            }

            if (waypoints.Count < 2)
            {
                throw new ArgumentException("A route requires at least a start and a goal.", nameof(waypoints));
            }

            var combined = new List<GridPos>();
            var totalCost = 0;

            for (var i = 0; i < waypoints.Count - 1; i++)
            {
                var segment = Find(map, waypoints[i], waypoints[i + 1]);
                if (!segment.Found)
                {
                    return PathSearchResult.NotFound;
                }

                var startNode = i == 0 ? 0 : 1;
                for (var n = startNode; n < segment.Nodes.Count; n++)
                {
                    combined.Add(segment.Nodes[n]);
                }

                totalCost += segment.Cost;
            }

            return new PathSearchResult(true, combined, totalCost);
        }

        private static int Heuristic(GridPos from, GridPos goal)
        {
            return from.Manhattan(goal) * OrthogonalCost;
        }

        private static int PopBest(List<int> open, Node[] nodes)
        {
            var bestSlot = 0;
            var best = nodes[open[0]];

            for (var i = 1; i < open.Count; i++)
            {
                var candidate = nodes[open[i]];
                if (ComparePriority(candidate, best) < 0)
                {
                    bestSlot = i;
                    best = candidate;
                }
            }

            var index = open[bestSlot];
            var last = open.Count - 1;
            open[bestSlot] = open[last];
            open.RemoveAt(last);
            return index;
        }

        private static int ComparePriority(Node left, Node right)
        {
            var f = left.F.CompareTo(right.F);
            if (f != 0)
            {
                return f;
            }

            var h = left.H.CompareTo(right.H);
            if (h != 0)
            {
                return h;
            }

            return left.Index.CompareTo(right.Index);
        }

        private static PathSearchResult Reconstruct(Node[] nodes, GridPos start, int goalIndex, int cost)
        {
            var stack = new Stack<GridPos>();
            var cursor = nodes[goalIndex];
            if (cursor == null)
            {
                throw new InvalidOperationException("Goal was expanded but the node record is missing.");
            }

            while (true)
            {
                stack.Push(cursor.Pos);
                if (cursor.ParentIndex < 0)
                {
                    break;
                }

                cursor = nodes[cursor.ParentIndex];
                if (cursor == null)
                {
                    throw new InvalidOperationException("Path reconstruction hit a missing parent.");
                }
            }

            if (stack.Peek() != start)
            {
                throw new InvalidOperationException("Path reconstruction did not reach the start cell.");
            }

            return new PathSearchResult(true, stack.ToArray(), cost);
        }

        private sealed class Node
        {
            public Node(GridPos pos, int index, int g, int h, int parentIndex)
            {
                Pos = pos;
                Index = index;
                G = g;
                H = h;
                ParentIndex = parentIndex;
            }

            public GridPos Pos { get; }

            public int Index { get; }

            public int G { get; }

            public int H { get; }

            public int F => G + H;

            public int ParentIndex { get; }

            public bool Closed { get; set; }
        }
    }
}
