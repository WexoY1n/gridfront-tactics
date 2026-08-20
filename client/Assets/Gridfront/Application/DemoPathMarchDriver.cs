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
        public const float StepSeconds = 1f / TicksPerSecond;

        private readonly List<PathFollower> _followers = new List<PathFollower>();
        private float _tickCarry;
        private bool _clockStarted;

        public DemoPathMarchBoard Board { get; private set; }

        public IReadOnlyList<PathFollower> Followers => _followers;

        public int Tick { get; private set; }

        public float TickRemainder => _tickCarry;

        public bool PathDebugVisible { get; set; } = true;

        private void Awake()
        {
            Board = DemoPathMarchBoard.Create();
        }

        private void Start()
        {
            SpawnNext();
        }

        private void Update()
        {
            if (!_clockStarted)
            {
                _clockStarted = true;
                _tickCarry = 0f;
                return;
            }

            _tickCarry += Time.unscaledDeltaTime;
            if (_tickCarry >= StepSeconds)
            {
                _tickCarry -= StepSeconds;
                StepOnce();
                if (_tickCarry >= StepSeconds)
                {
                    _tickCarry = 0f;
                }
            }
        }

        private void StepOnce()
        {
            for (var i = 0; i < _followers.Count; i++)
            {
                _followers[i].Step();
            }

            Tick += 1;

            if (_followers.Count < EnemyCount && Tick % SpawnEveryTicks == 0)
            {
                SpawnNext();
            }
        }

        private void SpawnNext()
        {
            var id = _followers.Count + 1;
            _followers.Add(new PathFollower(id, Board.Path, SpeedMilliPerTick));
        }
    }
}
