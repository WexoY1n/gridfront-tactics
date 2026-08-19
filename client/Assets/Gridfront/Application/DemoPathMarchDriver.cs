using System.Collections.Generic;
using Gridfront.BattleCore.Pathfinding;
using UnityEngine;

namespace Gridfront.Client.Application
{
    /// <summary>
    /// Drives the pure C# march at 20 ticks/second. View reads this; it does not consume Unity positions.
    /// </summary>
    public sealed class DemoPathMarchDriver : MonoBehaviour
    {
        public const int TicksPerSecond = 20;
        public const int EnemyCount = 20;
        public const int SpawnEveryTicks = 8;
        public const int SpeedMilliPerTick = 200;

        private readonly List<PathFollower> _followers = new List<PathFollower>();
        private float _tickCarry;

        public DemoPathMarchBoard Board { get; private set; }

        public IReadOnlyList<PathFollower> Followers => _followers;

        public int Tick { get; private set; }

        public bool PathDebugVisible { get; set; } = true;

        private void Awake()
        {
            Board = DemoPathMarchBoard.Create();
        }

        private void Update()
        {
            _tickCarry += Time.unscaledDeltaTime;
            var stepSeconds = 1f / TicksPerSecond;
            while (_tickCarry >= stepSeconds)
            {
                _tickCarry -= stepSeconds;
                StepOnce();
            }
        }

        private void StepOnce()
        {
            if (_followers.Count < EnemyCount && Tick % SpawnEveryTicks == 0)
            {
                var id = _followers.Count + 1;
                _followers.Add(new PathFollower(id, Board.Path, SpeedMilliPerTick));
            }

            for (var i = 0; i < _followers.Count; i++)
            {
                _followers[i].Step();
            }

            Tick += 1;
        }
    }
}
