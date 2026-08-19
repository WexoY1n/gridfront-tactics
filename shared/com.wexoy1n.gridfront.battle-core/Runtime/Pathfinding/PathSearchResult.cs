using System;
using System.Collections.Generic;

namespace Gridfront.BattleCore.Pathfinding
{
    public readonly struct PathSearchResult
    {
        private static readonly IReadOnlyList<GridPos> EmptyNodes = Array.Empty<GridPos>();

        public static PathSearchResult NotFound { get; } = new PathSearchResult(false, EmptyNodes, 0);

        public PathSearchResult(bool found, IReadOnlyList<GridPos> nodes, int cost)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (found && nodes.Count == 0)
            {
                throw new ArgumentException("A found path must contain at least one node.", nameof(nodes));
            }

            if (!found && nodes.Count != 0)
            {
                throw new ArgumentException("A failed search must not include path nodes.", nameof(nodes));
            }

            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cost), cost, "Cost must be non-negative.");
            }

            Found = found;
            Nodes = nodes;
            Cost = cost;
        }

        public bool Found { get; }

        public IReadOnlyList<GridPos> Nodes { get; }

        public int Cost { get; }
    }
}
