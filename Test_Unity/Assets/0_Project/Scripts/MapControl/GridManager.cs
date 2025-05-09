using UnityEngine;
using System.Collections.Generic;
using _0_Project.Scripts.MapControl;
using Sirenix.OdinInspector;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")] 
    [SerializeField] int width = 10;
    [SerializeField] int height = 10;
    //--------------------------------//
    [Header("Start and Goal Positions")] 
    [SerializeField] Vector2Int startPosition;
    [SerializeField] Vector2Int endPosition;
    //--------------------------------//
    [Header("Visual Settings")]
    [SerializeField] Cell cellPrefab;
    [SerializeField] Color wallColor = Color.gray;
    [SerializeField] Color pathColor = Color.yellow;
    [SerializeField] Color startColor = Color.green;
    [SerializeField] Color endColor = Color.red;
    [SerializeField] Color emptyColor = Color.white;
    [SerializeField] Camera mainCamera;
    //--------------------------------//
    private Node[,] grid;
    private Cell[,] cellObjects;
    private List<Node> mainPath;
    private List<Node> aStarPath;

    [Button]
    public void GenerateMap()
    {
        ClearExistingMap();
        CreateGrid();
        GenerateMainPath();
        GenerateRandomWalls();
        aStarPath = FindAStarPath();
        DrawPath();
        SetColorMap();
        CenterCamera();
        Debug.Log($"Map generated  A* more {mainPath.Count - aStarPath.Count} points ");
    }

    void ClearExistingMap()
    {
        if (cellObjects == null) return;
        foreach (var obj in cellObjects)
            if (obj != null)
                DestroyImmediate(obj);
    }

    void CreateGrid()
    {
        grid = new Node[width, height];
        cellObjects = new Cell[width, height];
        Vector3 offset = new Vector3(-(width * 0.5f), -(height * 0.5f), 0);

        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            grid[x, y] = new Node(x, y);
            Cell cell = Instantiate(cellPrefab, new Vector3(x, y, 0) + offset, Quaternion.identity, transform);
            cell.SpriteRenderer.color = emptyColor;
            cellObjects[x, y] = cell;
        }
    }

    void GenerateMainPath()
    {
        mainPath = new List<Node>();
        int x = startPosition.x;
        int y = startPosition.y;
        mainPath.Add(grid[x, y]);

        int maxSteps = width * height * 3;
        for (int steps = 0; (x != endPosition.x || y != endPosition.y) && steps < maxSteps; steps++)
        {
            bool moveToGoal = Random.value < 0.7f; //random 70% path short
            List<Vector2Int> candidateMoves = new List<Vector2Int>();
            if (moveToGoal)
            {
                if (x != endPosition.x) candidateMoves.Add(new Vector2Int(x + ((endPosition.x > x) ? 1 : -1), y));
                if (y != endPosition.y) candidateMoves.Add(new Vector2Int(x, y + ((endPosition.y > y) ? 1 : -1)));
            }
            else
            {
                if (x + 1 < width) candidateMoves.Add(new Vector2Int(x + 1, y));
                if (x - 1 >= 0) candidateMoves.Add(new Vector2Int(x - 1, y));
                if (y + 1 < height) candidateMoves.Add(new Vector2Int(x, y + 1));
                if (y - 1 >= 0) candidateMoves.Add(new Vector2Int(x, y - 1));
            }
            candidateMoves.RemoveAll(pos => mainPath.Exists(n => n.x == pos.x && n.y == pos.y));
            if (candidateMoves.Count == 0)
            {
                if (x != endPosition.x) x += (endPosition.x > x) ? 1 : -1;
                else if (y != endPosition.y) y += (endPosition.y > y) ? 1 : -1;
            }
            else
            {
                var next = candidateMoves[Random.Range(0, candidateMoves.Count)];
                x = next.x;
                y = next.y;
            }

            mainPath.Add(grid[x, y]);
        }

        OutputPathCoordinates();
    }

    void GenerateRandomWalls()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            Node node = grid[x, y];
            if (node.x == startPosition.x && node.y == startPosition.y) continue;
            if (node.x == endPosition.x && node.y == endPosition.y) continue;
            if (mainPath.Contains(node)) continue;
            node.isWall = Random.value < 0.3f; //random wall;
        }
    }

    void SetColorMap()
    {
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
        {
            Node node = grid[x, y];
            var sr = cellObjects[x, y].GetComponent<SpriteRenderer>();

            if (node.x == startPosition.x && node.y == startPosition.y)
                sr.color = startColor;
            else if (node.x == endPosition.x && node.y == endPosition.y)
                sr.color = endColor;
            else if (node.isWall)
                sr.color = wallColor;
            else if (mainPath != null && aStarPath != null && mainPath.Contains(node) && aStarPath.Contains(node))
                sr.color = Color.blue;
            else if (mainPath != null && mainPath.Contains(node))
                sr.color = pathColor;
            else if (aStarPath != null && aStarPath.Contains(node))
                sr.color = Color.cyan;
            else
                sr.color = emptyColor;
        }
    }

    void DrawPath()
    {
        for (int index = 0; index < mainPath.Count; index++)
        {
            var node = mainPath[index];
            if ((node.x == startPosition.x && node.y == startPosition.y) ||
                (node.x == endPosition.x && node.y == endPosition.y))
                continue;

            cellObjects[node.x, node.y].SpriteRenderer.color = pathColor;
        }
    }

    void CenterCamera()
    {
        float orthoSize = Mathf.Max(width, height);
        mainCamera.orthographicSize = orthoSize;
    }

    void OutputPathCoordinates()
    {
        Debug.Log("-------------------------Normal path :-----------------------------------");
        for (int i = 0; i < mainPath.Count; i++)
        {
            Node node = mainPath[i];
            Debug.Log($"index {i}: ({node.x}, {node.y})");
        }
    }

    //AStar
    [Button]
    public void FindAndDebugAStarPath()
    {
        Debug.Log("-------------------------A* Shortest path:-------------------------------");
        for (int i = 0; i < aStarPath.Count; i++)
        {
            Node node = aStarPath[i];
            Debug.Log($"index {i}: ({node.x}, {node.y})");
            if (!(node.x == startPosition.x && node.y == startPosition.y) &&
                !(node.x == endPosition.x && node.y == endPosition.y))
            {
                cellObjects[node.x, node.y].SpriteRenderer.color = Color.cyan;
            }
        }
    }

    /// <summary>
    /// A*
    /// </summary>
    List<Node> FindAStarPath()
    {
        Node start = grid[startPosition.x, startPosition.y];
        Node goal = grid[endPosition.x, endPosition.y];

        var openSet = new List<Node>();
        var cameFrom = new Dictionary<Node, Node>();
        var gScore = new Dictionary<Node, int>();
        var fScore = new Dictionary<Node, int>();

        for (int index0 = 0; index0 < grid.GetLength(0); index0++)
        for (int index1 = 0; index1 < grid.GetLength(1); index1++)
        {
            var node = grid[index0, index1];
            gScore[node] = int.MaxValue;
            fScore[node] = int.MaxValue;
        }

        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        openSet.Add(start);

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int index = 0; index < openSet.Count; index++)
            {
                var node = openSet[index];
                if (fScore[node] < fScore[current])
                    current = node;
            }

            if (current == goal)
            {
                List<Node> path = new List<Node>();
                while (cameFrom.ContainsKey(current))
                {
                    path.Add(current);
                    current = cameFrom[current];
                }

                path.Add(start);
                path.Reverse();
                return path;
            }

            openSet.Remove(current);

            var list = GetNeighbors(current);
            for (int index = 0; index < list.Count; index++)
            {
                var neighbor = list[index];
                if (neighbor.isWall) continue;
                int tentativeG = gScore[current] + 1;
                if (tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        FindAndDebugAStarPath();
        return null;
    }

    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        int[,] dirs = { { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, 0 } };
        for (int i = 0; i < 4; i++)
        {
            int nx = node.x + dirs[i, 0];
            int ny = node.y + dirs[i, 1];
            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                neighbors.Add(grid[nx, ny]);
            }
        }

        return neighbors;
    }

    int Heuristic(Node a, Node b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}

public class Node
{
    public int x, y;
    public bool isWall;

    public Node(int x, int y)
    {
        this.x = x;
        this.y = y;
        isWall = false;
    }
}