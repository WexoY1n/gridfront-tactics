using System;
using System.Collections.Generic;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Combat
{
    public readonly struct TargetCandidate
    {
        public TargetCandidate(
            int entityId,
            int spawnSequence,
            GridPos cell,
            int routeProgress,
            int tauntLevel,
            bool alive,
            bool flying,
            bool stealthed,
            bool targetable)
        {
            if (entityId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(entityId));
            }

            if (spawnSequence < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spawnSequence));
            }

            EntityId = entityId;
            SpawnSequence = spawnSequence;
            Cell = cell;
            RouteProgress = routeProgress;
            TauntLevel = tauntLevel;
            Alive = alive;
            Flying = flying;
            Stealthed = stealthed;
            Targetable = targetable;
        }

        public int EntityId { get; }

        public int SpawnSequence { get; }

        public GridPos Cell { get; }

        public int RouteProgress { get; }

        public int TauntLevel { get; }

        public bool Alive { get; }

        public bool Flying { get; }

        public bool Stealthed { get; }

        public bool Targetable { get; }
    }

    public readonly struct TargetQuery
    {
        public TargetQuery(
            GridPos origin,
            IReadOnlyList<GridPos> rangeCells,
            IReadOnlyList<TargetCandidate> candidates,
            int? currentTargetId,
            bool canTargetAir,
            bool canTargetStealthed)
        {
            if (rangeCells == null)
            {
                throw new ArgumentNullException(nameof(rangeCells));
            }

            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            Origin = origin;
            RangeCells = rangeCells;
            Candidates = candidates;
            CurrentTargetId = currentTargetId;
            CanTargetAir = canTargetAir;
            CanTargetStealthed = canTargetStealthed;
        }

        public GridPos Origin { get; }

        public IReadOnlyList<GridPos> RangeCells { get; }

        public IReadOnlyList<TargetCandidate> Candidates { get; }

        public int? CurrentTargetId { get; }

        public bool CanTargetAir { get; }

        public bool CanTargetStealthed { get; }
    }

    public interface ITargetPolicy
    {
        int? Select(in TargetQuery query);
    }
}
