using System.Collections.Generic;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Combat
{
    public sealed class DefaultOperatorTargetPolicy : ITargetPolicy
    {
        public int? Select(in TargetQuery query)
        {
            var eligible = new List<TargetCandidate>();
            for (var i = 0; i < query.Candidates.Count; i++)
            {
                var candidate = query.Candidates[i];
                if (IsEligible(candidate, query))
                {
                    eligible.Add(candidate);
                }
            }

            if (eligible.Count == 0)
            {
                return null;
            }

            if (query.CurrentTargetId.HasValue)
            {
                var currentId = query.CurrentTargetId.Value;
                for (var i = 0; i < eligible.Count; i++)
                {
                    if (eligible[i].EntityId == currentId)
                    {
                        return currentId;
                    }
                }
            }

            var origin = query.Origin;
            eligible.Sort((left, right) => Compare(left, right, origin));
            return eligible[0].EntityId;
        }

        private static bool IsEligible(in TargetCandidate candidate, in TargetQuery query)
        {
            if (!candidate.Alive || !candidate.Targetable)
            {
                return false;
            }

            if (candidate.Flying && !query.CanTargetAir)
            {
                return false;
            }

            if (candidate.Stealthed && !query.CanTargetStealthed)
            {
                return false;
            }

            var inRange = false;
            for (var i = 0; i < query.RangeCells.Count; i++)
            {
                if (query.RangeCells[i] == candidate.Cell)
                {
                    inRange = true;
                    break;
                }
            }

            return inRange;
        }

        private static int Compare(TargetCandidate left, TargetCandidate right, GridPos origin)
        {
            var taunt = right.TauntLevel.CompareTo(left.TauntLevel);
            if (taunt != 0)
            {
                return taunt;
            }

            var progress = right.RouteProgress.CompareTo(left.RouteProgress);
            if (progress != 0)
            {
                return progress;
            }

            var dist = DistanceSquared(origin, left.Cell).CompareTo(DistanceSquared(origin, right.Cell));
            if (dist != 0)
            {
                return dist;
            }

            var spawn = left.SpawnSequence.CompareTo(right.SpawnSequence);
            if (spawn != 0)
            {
                return spawn;
            }

            return left.EntityId.CompareTo(right.EntityId);
        }

        private static int DistanceSquared(GridPos origin, GridPos cell)
        {
            var dx = cell.X - origin.X;
            var dy = cell.Y - origin.Y;
            return (dx * dx) + (dy * dy);
        }
    }
}
