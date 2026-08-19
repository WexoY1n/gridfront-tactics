using System;
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
        private Transform _gridRoot;
        private Transform _dotRoot;
        private LineRenderer _pathLine;
        private Material _lineMaterial;

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
            BuildGrid();
            BuildPathLine();
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null)
            {
                Destroy(_lineMaterial);
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
            if (!driver.PathDebugVisible)
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

            for (var y = 0; y < map.Height; y++)
            {
                for (var x = 0; x < map.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = "Tile_" + x + "_" + y;
                    tile.transform.SetParent(_gridRoot, false);
                    tile.transform.position = GridWorld.CellCenter(pos);
                    tile.transform.localScale = new Vector3(0.92f, 0.08f, 0.92f);
                    var renderer = tile.GetComponent<MeshRenderer>();
                    renderer.material.color = map.IsWalkable(pos)
                        ? new Color(0.22f, 0.35f, 0.28f)
                        : new Color(0.18f, 0.12f, 0.12f);
                }
            }
        }

        private void BuildPathLine()
        {
            var lineObject = new GameObject("PathLine");
            lineObject.transform.SetParent(transform, false);
            _pathLine = lineObject.AddComponent<LineRenderer>();
            var shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                throw new InvalidOperationException("Sprites/Default shader is missing; cannot draw path debug.");
            }

            _lineMaterial = new Material(shader);

            _pathLine.material = _lineMaterial;
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
            }

            var followers = driver.Followers;
            while (_dots.Count < followers.Count)
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "Enemy_" + (_dots.Count + 1);
                sphere.transform.SetParent(_dotRoot, false);
                sphere.transform.localScale = Vector3.one * 0.35f;
                var renderer = sphere.GetComponent<MeshRenderer>();
                renderer.material.color = new Color(0.95f, 0.35f, 0.2f);
                var collider = sphere.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                _dots.Add(sphere.transform);
            }

            for (var i = 0; i < followers.Count; i++)
            {
                followers[i].GetPositionMilli(out var xMilli, out var yMilli);
                _dots[i].position = GridWorld.FromMilli(xMilli, yMilli, 0.28f);
            }
        }
    }
}
