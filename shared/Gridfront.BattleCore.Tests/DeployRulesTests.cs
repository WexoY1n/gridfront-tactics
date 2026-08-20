using Gridfront.BattleCore.Combat;
using Gridfront.BattleCore.Domain;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Tests
{
    public sealed class DeployRulesTests
    {
        [Fact]
        public void AcceptsMeleeOnMeleePad_AndDeductsCost()
        {
            var map = TwoPadMap();
            var occupied = new HashSet<GridPos>();
            var request = new DeployRequest(new GridPos(0, 0), Facing.North, OperatorSlot.Melee, cost: 10);

            var decision = DeployRules.Evaluate(map, occupied, availableCost: 15, request);

            Assert.True(decision.Accepted);
            Assert.Equal(DeployReject.None, decision.Reject);
            Assert.Equal(5, decision.RemainingCost);
        }

        [Fact]
        public void RejectsMeleeOnHighPad()
        {
            var map = TwoPadMap();
            var request = new DeployRequest(new GridPos(1, 0), Facing.East, OperatorSlot.Melee, cost: 1);

            var decision = DeployRules.Evaluate(map, new HashSet<GridPos>(), 10, request);

            Assert.False(decision.Accepted);
            Assert.Equal(DeployReject.WrongTile, decision.Reject);
            Assert.Equal(10, decision.RemainingCost);
        }

        [Fact]
        public void RejectsOccupied_OutOfBounds_AndInsufficientCost()
        {
            var map = TwoPadMap();
            var occupied = new HashSet<GridPos> { new GridPos(0, 0) };

            var occupiedDecision = DeployRules.Evaluate(
                map,
                occupied,
                10,
                new DeployRequest(new GridPos(0, 0), Facing.North, OperatorSlot.Melee, 1));
            Assert.Equal(DeployReject.Occupied, occupiedDecision.Reject);

            var oob = DeployRules.Evaluate(
                map,
                new HashSet<GridPos>(),
                10,
                new DeployRequest(new GridPos(3, 0), Facing.North, OperatorSlot.Melee, 1));
            Assert.Equal(DeployReject.OutOfBounds, oob.Reject);

            var poor = DeployRules.Evaluate(
                map,
                new HashSet<GridPos>(),
                3,
                new DeployRequest(new GridPos(0, 0), Facing.North, OperatorSlot.Melee, 4));
            Assert.Equal(DeployReject.InsufficientCost, poor.Reject);
            Assert.Equal(3, poor.RemainingCost);
        }

        private static StageMap TwoPadMap()
        {
            return new StageMap(2, 1, new[] { TileType.MeleePad, TileType.HighPad });
        }
    }
}
