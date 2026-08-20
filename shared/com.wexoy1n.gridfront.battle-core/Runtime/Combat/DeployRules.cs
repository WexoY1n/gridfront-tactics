using System;
using System.Collections.Generic;
using Gridfront.BattleCore.Domain;
using Gridfront.BattleCore.Pathfinding;

namespace Gridfront.BattleCore.Combat
{
    public readonly struct DeployRequest
    {
        public DeployRequest(GridPos cell, Facing facing, OperatorSlot slot, int cost)
        {
            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cost), cost, "Cost must be non-negative.");
            }

            Cell = cell;
            Facing = facing;
            Slot = slot;
            Cost = cost;
        }

        public GridPos Cell { get; }

        public Facing Facing { get; }

        public OperatorSlot Slot { get; }

        public int Cost { get; }
    }

    public readonly struct DeployDecision
    {
        public DeployDecision(bool accepted, DeployReject reject, int remainingCost)
        {
            if (accepted && reject != DeployReject.None)
            {
                throw new ArgumentException("An accepted deploy cannot carry a reject reason.");
            }

            if (!accepted && reject == DeployReject.None)
            {
                throw new ArgumentException("A rejected deploy must carry a reject reason.");
            }

            if (remainingCost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(remainingCost));
            }

            Accepted = accepted;
            Reject = reject;
            RemainingCost = remainingCost;
        }

        public bool Accepted { get; }

        public DeployReject Reject { get; }

        public int RemainingCost { get; }
    }

    public static class DeployRules
    {
        public static DeployDecision Evaluate(
            StageMap map,
            ISet<GridPos> occupied,
            int availableCost,
            DeployRequest request)
        {
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            if (occupied == null)
            {
                throw new ArgumentNullException(nameof(occupied));
            }

            if (availableCost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(availableCost));
            }

            if (!map.InBounds(request.Cell))
            {
                return new DeployDecision(false, DeployReject.OutOfBounds, availableCost);
            }

            if (occupied.Contains(request.Cell))
            {
                return new DeployDecision(false, DeployReject.Occupied, availableCost);
            }

            var tile = map.TileAt(request.Cell);
            if (!SlotMatchesTile(request.Slot, tile))
            {
                return new DeployDecision(false, DeployReject.WrongTile, availableCost);
            }

            if (availableCost < request.Cost)
            {
                return new DeployDecision(false, DeployReject.InsufficientCost, availableCost);
            }

            return new DeployDecision(true, DeployReject.None, availableCost - request.Cost);
        }

        private static bool SlotMatchesTile(OperatorSlot slot, TileType tile)
        {
            switch (slot)
            {
                case OperatorSlot.Melee:
                    return tile == TileType.MeleePad;
                case OperatorSlot.HighGround:
                    return tile == TileType.HighPad;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, "Unknown operator slot.");
            }
        }
    }
}
