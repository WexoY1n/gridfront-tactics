using System;

namespace Gridfront.BattleCore.Pathfinding
{
    public sealed class PathFollower
    {
        public PathFollower(int entityId, GridPath path, int speedMilliPerTick)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (speedMilliPerTick <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(speedMilliPerTick),
                    speedMilliPerTick,
                    "Speed must be positive.");
            }

            EntityId = entityId;
            Path = path;
            SpeedMilliPerTick = speedMilliPerTick;
            DistanceAlongPath = 0;
            ReachedGoal = path.TotalLengthMilli == 0;
        }

        public int EntityId { get; }

        public GridPath Path { get; }

        public int SpeedMilliPerTick { get; }

        public int DistanceAlongPath { get; private set; }

        public int RouteProgress => Path.RouteProgress(DistanceAlongPath);

        public bool ReachedGoal { get; private set; }

        public void Step()
        {
            if (ReachedGoal)
            {
                return;
            }

            var next = DistanceAlongPath + SpeedMilliPerTick;
            if (next >= Path.TotalLengthMilli)
            {
                DistanceAlongPath = Path.TotalLengthMilli;
                ReachedGoal = true;
                return;
            }

            DistanceAlongPath = next;
        }

        public void GetPositionMilli(out int xMilli, out int yMilli)
        {
            Path.GetPositionMilli(DistanceAlongPath, out xMilli, out yMilli);
        }
    }
}
