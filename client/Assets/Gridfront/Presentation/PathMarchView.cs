using System.Collections.Generic;
using Gridfront.BattleCore.Pathfinding;
using Gridfront.Client.Application;
using UnityEngine;

namespace Gridfront.Client.Presentation
{
    /// <summary>
    /// Reads <see cref="DemoPathMarchDriver"/> and draws tiles, dots, and F2 path debug.
    /// Does not write Unity floats back into Battle.Core.
    /// </summary>
    public sealed class PathMarchView : MonoBehaviour
    {
        [SerializeField]
        private DemoPathMarchDriver driver;

        private readonly List<Transform> _dots = new List<Transform>();
        private readonly List<Vector3> _from = new List<Vector3>();
        private readonly List<Vector3> _to = new List<Vector3>();
        private ViewPalette _palette;
        private Transform _gridRoot;
        private Transform _dotRoot;
        private LineRenderer _pathLine;
        private Material _enemyMaterial;
        private int _syncedTick = -1;
        private int _syncedCountSnapshot;

        private void Awake()
        {
            if (driver == null)
            {
                driver = FindAnyObjectByType<DemoPathMarchDriver>();
            }

            if (driver == null)
            {
                throw new MissingComponentException("PathMarchView requires a DemoPathMarchDriver in the scene.");
            }
        }

        private void Start()
        {
            _palette = new ViewPalette();
            BuildGrid();
            BuildPathLine();
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
            if (Input.GetKeyDown(KeyCode.F2))
            {
                driver.PathDebugVisible = !driver.PathDebugVisible;
            }
        }

        private void LateUpdate()
        {
            SyncDots();
            _pathLine.enabled = driver.PathDebugVisible;
        }

        private void OnGUI()
        {
            if (!driver.PathDebugVisible || driver.Board == null)
            {
                return;
            }

            var followerCount = driver.Followers.Count;
            var progress = followerCount == 0 ? 0 : driver.Followers[0].RouteProgress;
            var reached = 0;
            for (var i = 0; i < followerCount; i++)
            {
                if (driver.Followers[i].ReachedGoal)
                {
                    reached++;
                }
            }

            GUI.Box(new Rect(12, 12, 420, 88), string.Empty);
            GUI.Label(
                new Rect(20, 20, 400, 72),
                "F2 path debug\nTick " + driver.Tick
                + "  enemies " + followerCount + "/" + DemoPathMarchDriver.EnemyCount
                + "  reached " + reached
                + "\nLead routeProgress " + progress
                + " / " + driver.Board.Path.TotalLengthMilli);
        }

        private void BuildGrid()
        {
            _gridRoot = new GameObject("Grid").transform;
            _gridRoot.SetParent(transform, false);
            var map = driver.Board.Map;
            var walkable = _palette.Lit(new Color(0.22f, 0.35f, 0.28f));
            var blocked = _palette.Lit(new Color(0.18f, 0.12f, 0.12f));
            var scale = new Vector3(0.92f, 0.08f, 0.92f);

            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    ViewPalette.Primitive(
                        PrimitiveType.Cube,
                        _gridRoot,
                        "Tile_" + x + "_" + y,
                        GridWorld.CellCenter(pos),
                        scale,
                        map.IsWalkable(pos) ? walkable : blocked);
                }
            }
        }

        private void BuildPathLine()
        {
            var lineObject = new GameObject("PathLine");
            lineObject.transform.SetParent(transform, false);
            _pathLine = lineObject.AddComponent<LineRenderer>();
            _pathLine.sharedMaterial = _palette.Unlit(new Color(1f, 0.85f, 0.2f, 1f));
            _pathLine.startColor = new Color(1f, 0.85f, 0.2f, 1f);
            _pathLine.endColor = new Color(1f, 0.45f, 0.1f, 1f);
            _pathLine.startWidth = 0.08f;
            _pathLine.endWidth = 0.08f;
            _pathLine.useWorldSpace = true;
            _pathLine.positionCount = driver.Board.Path.Nodes.Count;

            var nodes = driver.Board.Path.Nodes;
            for (var i = 0; i < nodes.Count; i++)
            {
                var center = GridWorld.CellCenter(nodes[i]);
                _pathLine.SetPosition(i, new Vector3(center.x, 0.2f, center.z));
            }
        }

        private void SyncDots()
        {
            if (_dotRoot == null)
            {
                _dotRoot = new GameObject("Dots").transform;
                _dotRoot.SetParent(transform, false);
                _enemyMaterial = _palette.Lit(new Color(0.95f, 0.35f, 0.2f));
            }

            var followers = driver.Followers;
            while (_dots.Count < followers.Count)
            {
                var sphere = ViewPalette.Primitive(
                    PrimitiveType.Sphere,
                    _dotRoot,
                    "Enemy_" + (_dots.Count + 1),
                    Vector3.zero,
                    Vector3.one * 0.35f,
                    _enemyMaterial);
                _dots.Add(sphere.transform);
                _from.Add(Vector3.zero);
                _to.Add(Vector3.zero);
            }

            if (driver.Tick != _syncedTick)
            {
                for (var i = 0; i < followers.Count; i++)
                {
                    var current = WorldPos(followers[i]);
                    if (i >= _to.Count)
                    {
                        continue;
                    }

                    _from[i] = _syncedTick < 0 || i >= _syncedCountSnapshot ? current : _to[i];
                    _to[i] = current;
                }

                _syncedTick = driver.Tick;
                _syncedCountSnapshot = followers.Count;
            }

            var t = Mathf.Clamp01(driver.TickRemainder / DemoPathMarchDriver.StepSeconds);
            for (var i = 0; i < followers.Count; i++)
            {
                _dots[i].position = Vector3.Lerp(_from[i], _to[i], t);
            }
        }

        private static Vector3 WorldPos(PathFollower follower)
        {
            follower.GetPositionMilli(out var xMilli, out var yMilli);
            return GridWorld.FromMilli(xMilli, yMilli, 0.28f);
        }
    }
}
