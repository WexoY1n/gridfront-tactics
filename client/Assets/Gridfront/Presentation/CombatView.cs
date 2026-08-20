using System.Collections.Generic;
using Gridfront.BattleCore.Combat;
using Gridfront.BattleCore.Domain;
using Gridfront.BattleCore.Pathfinding;
using Gridfront.Client.Application;
using UnityEngine;

namespace Gridfront.Client.Presentation
{
    /// <summary>
    /// Reads <see cref="DemoCombatDriver"/> and draws pads, range, lock, and hit.
    /// Does not write Unity floats back into Battle.Core.
    /// </summary>
    public sealed class CombatView : MonoBehaviour
    {
        private const int HitFlashTicks = 3;

        [SerializeField]
        private DemoCombatDriver driver;

        private readonly List<Transform> _dots = new List<Transform>();
        private readonly List<MeshRenderer> _dotRenderers = new List<MeshRenderer>();
        private readonly List<Vector3> _from = new List<Vector3>();
        private readonly List<Vector3> _to = new List<Vector3>();
        private ViewPalette _palette;
        private Material _enemyMaterial;
        private Material _lockedMaterial;
        private Material _hitMaterial;
        private Material _deadMaterial;
        private Transform _dotRoot;
        private LineRenderer _lockLine;
        private int _syncedTick = -1;
        private int _syncedCountSnapshot;

        private void Awake()
        {
            if (driver == null)
            {
                driver = FindAnyObjectByType<DemoCombatDriver>();
            }

            if (driver == null)
            {
                throw new MissingComponentException("CombatView requires a DemoCombatDriver in the scene.");
            }
        }

        private void Start()
        {
            _palette = new ViewPalette();
            BuildBoard();
        }

        private void OnDestroy()
        {
            if (_palette != null)
            {
                _palette.Dispose();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                driver.TargetingDebugVisible = !driver.TargetingDebugVisible;
            }
        }

        private void LateUpdate()
        {
            SyncDots();
            SyncLockLine();
        }

        private void OnGUI()
        {
            if (!driver.TargetingDebugVisible || driver.Board == null)
            {
                return;
            }

            var locked = driver.LockedTargetId;
            var lockedHp = "-";
            if (locked.HasValue)
            {
                var enemy = driver.FindEnemy(locked.Value);
                if (enemy != null)
                {
                    lockedHp = enemy.Hp + "/" + enemy.MaxHp;
                }
            }

            GUI.Box(new Rect(12, 12, 460, 128), string.Empty);
            GUI.Label(
                new Rect(20, 20, 440, 112),
                "F3 targeting debug\nTick " + driver.Tick
                + "  phase " + driver.Phase
                + " +" + driver.ElapsedInPhase
                + "  hits " + driver.HitCount
                + "\nDeploy " + driver.Board.DeployCell
                + " " + driver.Board.Facing
                + "  cost " + driver.RemainingCost
                + "  wrong-tile sample " + driver.SampleWrongTileReject
                + "\nLock " + (locked.HasValue ? locked.Value.ToString() : "-")
                + "  hp " + lockedHp
                + "  last hit dmg " + driver.LastHitDamage
                + " @ tick " + driver.LastHitTick);
        }

        private void BuildBoard()
        {
            var root = new GameObject("Board").transform;
            root.SetParent(transform, false);
            var stage = driver.Board.Stage;
            var nav = driver.Board.Nav;
            var walkable = _palette.Lit(new Color(0.20f, 0.32f, 0.28f));
            var blocked = _palette.Lit(new Color(0.16f, 0.11f, 0.11f));
            var melee = _palette.Lit(new Color(0.28f, 0.42f, 0.62f));
            var high = _palette.Lit(new Color(0.42f, 0.32f, 0.55f));
            var spawn = _palette.Lit(new Color(0.38f, 0.28f, 0.18f));
            var goal = _palette.Lit(new Color(0.22f, 0.40f, 0.38f));
            var tileScale = new Vector3(0.92f, 0.08f, 0.92f);

            for (var y = 0; y < stage.Height; y++)
            {
                for (var x = 0; x < stage.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    var tile = stage.TileAt(pos);
                    var material = MaterialFor(tile, nav.IsWalkable(pos), walkable, blocked, melee, high, spawn, goal);
                    ViewPalette.Primitive(
                        PrimitiveType.Cube,
                        root,
                        "Tile_" + x + "_" + y,
                        GridWorld.CellCenter(pos),
                        tileScale,
                        material);
                }
            }

            var rangeMat = _palette.Unlit(new Color(0.35f, 0.85f, 1f, 0.35f));
            var rangeRoot = new GameObject("Range").transform;
            rangeRoot.SetParent(transform, false);
            var rangeCells = driver.RangeCells;
            for (var i = 0; i < rangeCells.Count; i++)
            {
                var center = GridWorld.CellCenter(rangeCells[i]);
                ViewPalette.Primitive(
                    PrimitiveType.Cube,
                    rangeRoot,
                    "Range_" + i,
                    new Vector3(center.x, 0.12f, center.z),
                    new Vector3(0.7f, 0.02f, 0.7f),
                    rangeMat);
            }

            var operatorGo = ViewPalette.Primitive(
                PrimitiveType.Cylinder,
                transform,
                "Operator",
                GridWorld.CellCenter(driver.Board.DeployCell) + new Vector3(0f, 0.45f, 0f),
                new Vector3(0.45f, 0.4f, 0.45f),
                _palette.Lit(new Color(0.55f, 0.78f, 0.95f)));
            operatorGo.transform.rotation = Quaternion.Euler(0f, Yaw(driver.Board.Facing), 0f);

            var facingMark = ViewPalette.Primitive(
                PrimitiveType.Cube,
                operatorGo.transform,
                "Facing",
                operatorGo.transform.position,
                new Vector3(0.12f, 0.12f, 0.35f),
                _palette.Lit(new Color(1f, 0.9f, 0.35f)));
            facingMark.transform.localPosition = new Vector3(0f, 0.55f, 0.55f);

            var lockObject = new GameObject("LockLine");
            lockObject.transform.SetParent(transform, false);
            _lockLine = lockObject.AddComponent<LineRenderer>();
            _lockLine.sharedMaterial = _palette.Unlit(new Color(1f, 0.85f, 0.2f, 0.9f));
            _lockLine.startColor = new Color(1f, 0.85f, 0.2f, 0.9f);
            _lockLine.endColor = new Color(1f, 0.45f, 0.15f, 0.9f);
            _lockLine.startWidth = 0.05f;
            _lockLine.endWidth = 0.05f;
            _lockLine.useWorldSpace = true;
            _lockLine.positionCount = 2;
            _lockLine.enabled = false;
        }

        private void SyncDots()
        {
            if (_dotRoot == null)
            {
                _dotRoot = new GameObject("Enemies").transform;
                _dotRoot.SetParent(transform, false);
                _enemyMaterial = _palette.Lit(new Color(0.95f, 0.38f, 0.22f));
                _lockedMaterial = _palette.Lit(new Color(1f, 0.85f, 0.2f));
                _hitMaterial = _palette.Lit(new Color(1f, 1f, 1f));
                _deadMaterial = _palette.Lit(new Color(0.25f, 0.22f, 0.22f));
            }

            var enemies = driver.Enemies;
            while (_dots.Count < enemies.Count)
            {
                var sphere = ViewPalette.Primitive(
                    PrimitiveType.Sphere,
                    _dotRoot,
                    "Enemy_" + (_dots.Count + 1),
                    Vector3.zero,
                    Vector3.one * 0.35f,
                    _enemyMaterial);
                _dots.Add(sphere.transform);
                _dotRenderers.Add(sphere.GetComponent<MeshRenderer>());
                _from.Add(Vector3.zero);
                _to.Add(Vector3.zero);
            }

            if (driver.Tick != _syncedTick)
            {
                for (var i = 0; i < enemies.Count; i++)
                {
                    var current = WorldPos(enemies[i].Follower);
                    _from[i] = _syncedTick < 0 || i >= _syncedCountSnapshot ? current : _to[i];
                    _to[i] = current;
                }

                _syncedTick = driver.Tick;
                _syncedCountSnapshot = enemies.Count;
            }

            var t = Mathf.Clamp01(driver.TickRemainder / DemoCombatDriver.StepSeconds);
            var lockedId = driver.LockedTargetId;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                _dots[i].position = Vector3.Lerp(_from[i], _to[i], t);
                _dotRenderers[i].sharedMaterial = MaterialForEnemy(enemy, lockedId);
                _dots[i].localScale = enemy.Alive ? Vector3.one * 0.35f : Vector3.one * 0.22f;
            }
        }

        private void SyncLockLine()
        {
            if (_lockLine == null)
            {
                return;
            }

            var lockedId = driver.LockedTargetId;
            if (!lockedId.HasValue)
            {
                _lockLine.enabled = false;
                return;
            }

            var enemy = driver.FindEnemy(lockedId.Value);
            if (enemy == null || !enemy.Alive)
            {
                _lockLine.enabled = false;
                return;
            }

            var origin = GridWorld.CellCenter(driver.Board.DeployCell);
            var index = lockedId.Value - 1;
            var targetPos = index >= 0 && index < _dots.Count
                ? _dots[index].position
                : WorldPos(enemy.Follower);
            _lockLine.enabled = true;
            _lockLine.SetPosition(0, new Vector3(origin.x, 0.55f, origin.z));
            _lockLine.SetPosition(1, targetPos + new Vector3(0f, 0.15f, 0f));
        }

        private Material MaterialForEnemy(DemoCombatEnemy enemy, int? lockedId)
        {
            if (!enemy.Alive)
            {
                return _deadMaterial;
            }

            if (enemy.EntityId == driver.LastHitEntityId
                && driver.Tick - driver.LastHitTick <= HitFlashTicks)
            {
                return _hitMaterial;
            }

            if (lockedId.HasValue && enemy.EntityId == lockedId.Value)
            {
                return _lockedMaterial;
            }

            return _enemyMaterial;
        }

        private static Material MaterialFor(
            TileType tile,
            bool isWalkable,
            Material walkable,
            Material blocked,
            Material melee,
            Material high,
            Material spawn,
            Material goal)
        {
            switch (tile)
            {
                case TileType.MeleePad:
                    return melee;
                case TileType.HighPad:
                    return high;
                case TileType.Spawn:
                    return spawn;
                case TileType.Goal:
                    return goal;
                case TileType.Void:
                    return blocked;
                default:
                    return isWalkable ? walkable : blocked;
            }
        }

        private static float Yaw(Facing facing)
        {
            switch (facing)
            {
                case Facing.North:
                    return 0f;
                case Facing.East:
                    return 90f;
                case Facing.South:
                    return 180f;
                case Facing.West:
                    return -90f;
                default:
                    return 0f;
            }
        }

        private static Vector3 WorldPos(PathFollower follower)
        {
            follower.GetPositionMilli(out var xMilli, out var yMilli);
            return GridWorld.FromMilli(xMilli, yMilli, 0.28f);
        }
    }
}
