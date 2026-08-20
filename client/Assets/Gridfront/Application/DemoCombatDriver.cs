using System;
using System.Collections.Generic;
using Gridfront.BattleCore.Combat;
using Gridfront.BattleCore.Domain;
using Gridfront.BattleCore.Pathfinding;
using UnityEngine;

namespace Gridfront.Client.Application
{
    /// <summary>
    /// Drives deploy, targeting, and attack at 20 ticks/second. View only reads this.
    /// </summary>
    public sealed class DemoCombatDriver : MonoBehaviour
    {
        public const int TicksPerSecond = 20;
        public const int EnemyCount = 8;
        public const int SpawnEveryTicks = 8;
        public const int SpeedMilliPerTick = 100;
        public const int StartingCost = 25;
        public const int DeployCost = 10;
        public const int OperatorAttack = 12;
        public const int EnemyDefense = 5;
        public const int MinDamage = 1;
        public const int EnemyMaxHp = 14;
        public const float StepSeconds = 1f / TicksPerSecond;

        private readonly List<DemoCombatEnemy> _enemies = new List<DemoCombatEnemy>();
        private readonly List<TargetCandidate> _candidates = new List<TargetCandidate>();
        private readonly HashSet<GridPos> _occupied = new HashSet<GridPos>();
        private readonly DefaultOperatorTargetPolicy _policy = new DefaultOperatorTargetPolicy();
        private AttackTimeline _timeline;
        private int? _holdLock;
        private float _tickCarry;
        private bool _clockStarted;

        public DemoCombatBoard Board { get; private set; }

        public IReadOnlyList<DemoCombatEnemy> Enemies => _enemies;

        public IReadOnlyList<GridPos> RangeCells => Board.RangeCells;

        public int Tick { get; private set; }

        public float TickRemainder => _tickCarry;

        public int RemainingCost { get; private set; }

        public DeployReject SampleWrongTileReject { get; private set; }

        public AttackPhase Phase => _timeline.Phase;

        public int ElapsedInPhase => _timeline.ElapsedInPhase;

        public int? LockedTargetId => _timeline.LockedTargetId ?? _holdLock;

        public int HitCount { get; private set; }

        public int LastHitTick { get; private set; } = -1;

        public int LastHitEntityId { get; private set; } = -1;

        public int LastHitDamage { get; private set; }

        public bool TargetingDebugVisible { get; set; } = true;

        private void Awake()
        {
            Board = DemoCombatBoard.Create();
            _timeline = new AttackTimeline(windupTicks: 3, recoveryTicks: 3);
            DeployOperator();
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

        public DemoCombatEnemy FindEnemy(int entityId)
        {
            for (var i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].EntityId == entityId)
                {
                    return _enemies[i];
                }
            }

            return null;
        }

        private void DeployOperator()
        {
            RemainingCost = StartingCost;
            var legal = new DeployRequest(Board.DeployCell, Board.Facing, OperatorSlot.Melee, DeployCost);
            var accepted = DeployRules.Evaluate(Board.Stage, _occupied, RemainingCost, legal);
            if (!accepted.Accepted)
            {
                throw new InvalidOperationException("Demo melee deploy was rejected: " + accepted.Reject);
            }

            RemainingCost = accepted.RemainingCost;
            _occupied.Add(Board.DeployCell);

            var wrongSlot = new DeployRequest(Board.DeployCell, Board.Facing, OperatorSlot.HighGround, DeployCost);
            var occupiedNow = new HashSet<GridPos>(_occupied);
            occupiedNow.Remove(Board.DeployCell);
            SampleWrongTileReject = DeployRules.Evaluate(Board.Stage, occupiedNow, RemainingCost, wrongSlot).Reject;
            if (SampleWrongTileReject != DeployReject.WrongTile)
            {
                throw new InvalidOperationException(
                    "High-ground on a melee pad must reject as WrongTile, got " + SampleWrongTileReject);
            }
        }

        private void StepOnce()
        {
            for (var i = 0; i < _enemies.Count; i++)
            {
                if (_enemies[i].Alive && !_enemies[i].Follower.ReachedGoal)
                {
                    _enemies[i].Follower.Step();
                }
            }

            Tick += 1;

            if (_enemies.Count < EnemyCount && Tick % SpawnEveryTicks == 0)
            {
                SpawnNext();
            }

            ResolveCombat();
        }

        private void SpawnNext()
        {
            var id = _enemies.Count + 1;
            _enemies.Add(new DemoCombatEnemy(id, new PathFollower(id, Board.Path, SpeedMilliPerTick), EnemyMaxHp));
        }

        private void ResolveCombat()
        {
            RebuildCandidates();
            var acquired = _policy.Select(Query(_holdLock));
            var stillValid = false;
            if (_timeline.LockedTargetId.HasValue)
            {
                stillValid = _policy.Select(Query(_timeline.LockedTargetId)) == _timeline.LockedTargetId;
            }

            var incoming = _timeline.Phase == AttackPhase.Idle ? acquired : _timeline.LockedTargetId;
            var step = _timeline.Step(incoming, stillValid);
            if (_timeline.Phase != AttackPhase.Idle)
            {
                _holdLock = _timeline.LockedTargetId;
            }
            else if (acquired.HasValue)
            {
                _holdLock = acquired;
            }
            else
            {
                _holdLock = null;
            }

            if (!step.Hit)
            {
                return;
            }

            var targetId = _timeline.LockedTargetId;
            if (!targetId.HasValue)
            {
                throw new InvalidOperationException("Attack hit without a locked target.");
            }

            var enemy = FindEnemy(targetId.Value);
            if (enemy == null)
            {
                throw new InvalidOperationException("Locked target " + targetId.Value + " is missing.");
            }

            var damage = Damage.Physical(OperatorAttack, EnemyDefense, MinDamage);
            enemy.ApplyDamage(damage);
            HitCount += 1;
            LastHitTick = Tick;
            LastHitEntityId = enemy.EntityId;
            LastHitDamage = damage;
            if (!enemy.Alive)
            {
                _holdLock = null;
            }
        }

        private void RebuildCandidates()
        {
            _candidates.Clear();
            for (var i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                _candidates.Add(
                    new TargetCandidate(
                        enemy.EntityId,
                        enemy.EntityId,
                        OccupiedCell(enemy.Follower),
                        enemy.Follower.RouteProgress,
                        tauntLevel: 0,
                        enemy.Alive,
                        flying: false,
                        stealthed: false,
                        targetable: enemy.Alive && !enemy.Follower.ReachedGoal));
            }
        }

        private TargetQuery Query(int? currentTargetId)
        {
            return new TargetQuery(
                Board.DeployCell,
                Board.RangeCells,
                _candidates,
                currentTargetId,
                canTargetAir: false,
                canTargetStealthed: false);
        }

        internal static GridPos OccupiedCell(PathFollower follower)
        {
            var path = follower.Path;
            if (follower.ReachedGoal || path.TotalLengthMilli == 0)
            {
                return path.Nodes[path.Nodes.Count - 1];
            }

            var segment = follower.DistanceAlongPath / GridMap.TileUnits;
            if (segment >= path.Nodes.Count - 1)
            {
                return path.Nodes[path.Nodes.Count - 1];
            }

            return path.Nodes[segment];
        }
    }

    public sealed class DemoCombatEnemy
    {
        public DemoCombatEnemy(int entityId, PathFollower follower, int maxHp)
        {
            EntityId = entityId;
            Follower = follower ?? throw new ArgumentNullException(nameof(follower));
            MaxHp = maxHp;
            Hp = maxHp;
            Alive = true;
        }

        public int EntityId { get; }

        public PathFollower Follower { get; }

        public int MaxHp { get; }

        public int Hp { get; private set; }

        public bool Alive { get; private set; }

        public void ApplyDamage(int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage));
            }

            if (!Alive)
            {
                throw new InvalidOperationException("Cannot damage a dead enemy.");
            }

            Hp -= damage;
            if (Hp <= 0)
            {
                Hp = 0;
                Alive = false;
            }
        }
    }
}
