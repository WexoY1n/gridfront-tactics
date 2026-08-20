using Gridfront.BattleCore.Combat;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Tests
{
    public sealed class OperatorTargetingTests
    {
        private readonly DefaultOperatorTargetPolicy _policy = new DefaultOperatorTargetPolicy();
        private readonly GridPos _origin = new GridPos(0, 0);
        private readonly IReadOnlyList<GridPos> _range = new[]
        {
            new GridPos(1, 0),
            new GridPos(2, 0),
            new GridPos(3, 0)
        };

        [Fact]
        public void PrefersHigherRouteProgress()
        {
            var farther = Enemy(id: 1, progress: 100, cell: new GridPos(1, 0), spawn: 0);
            var closerToGoal = Enemy(id: 2, progress: 500, cell: new GridPos(2, 0), spawn: 1);

            var chosen = _policy.Select(Query(null, farther, closerToGoal));

            Assert.Equal(2, chosen);
        }

        [Fact]
        public void TieBreaksByDistanceThenSpawnThenId_AndIsStable()
        {
            var a = Enemy(id: 8, progress: 10, cell: new GridPos(2, 0), spawn: 1);
            var b = Enemy(id: 3, progress: 10, cell: new GridPos(2, 0), spawn: 1);

            var first = _policy.Select(Query(null, a, b));
            var second = _policy.Select(Query(null, b, a));

            Assert.Equal(3, first);
            Assert.Equal(first, second);
        }

        [Fact]
        public void FiltersFlyingStealthedDeadAndUntargetable()
        {
            var flying = Enemy(id: 1, progress: 999, cell: new GridPos(1, 0), spawn: 0, flying: true);
            var stealthed = Enemy(id: 2, progress: 999, cell: new GridPos(1, 0), spawn: 1, stealthed: true);
            var dead = Enemy(id: 3, progress: 999, cell: new GridPos(1, 0), spawn: 2, alive: false);
            var hidden = Enemy(id: 4, progress: 999, cell: new GridPos(1, 0), spawn: 3, targetable: false);
            var legal = Enemy(id: 5, progress: 1, cell: new GridPos(3, 0), spawn: 4);

            var chosen = _policy.Select(Query(null, flying, stealthed, dead, hidden, legal));

            Assert.Equal(5, chosen);
        }

        [Fact]
        public void KeepsLockWhileCurrentTargetRemainsLegal()
        {
            var locked = Enemy(id: 1, progress: 10, cell: new GridPos(1, 0), spawn: 0);
            var better = Enemy(id: 2, progress: 900, cell: new GridPos(2, 0), spawn: 1);

            var chosen = _policy.Select(Query(currentTargetId: 1, locked, better));

            Assert.Equal(1, chosen);
        }

        [Fact]
        public void ReacquiresWhenLockLeavesRange()
        {
            var lockedOutOfRange = Enemy(id: 1, progress: 900, cell: new GridPos(9, 9), spawn: 0);
            var other = Enemy(id: 2, progress: 10, cell: new GridPos(1, 0), spawn: 1);

            var chosen = _policy.Select(Query(currentTargetId: 1, lockedOutOfRange, other));

            Assert.Equal(2, chosen);
        }

        private TargetQuery Query(int? currentTargetId, params TargetCandidate[] candidates)
        {
            return new TargetQuery(
                _origin,
                _range,
                candidates,
                currentTargetId,
                canTargetAir: false,
                canTargetStealthed: false);
        }

        private static TargetCandidate Enemy(
            int id,
            int progress,
            GridPos cell,
            int spawn,
            bool alive = true,
            bool flying = false,
            bool stealthed = false,
            bool targetable = true)
        {
            return new TargetCandidate(
                id,
                spawn,
                cell,
                progress,
                tauntLevel: 0,
                alive,
                flying,
                stealthed,
                targetable);
        }
    }
}
