using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class BombSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform plane;
    public GameObject wallPrefab;
    public GameObject bombPrefab;
    public GameObject enemyPrefab;
    public NavMeshSurface navMeshSurface;

    [Header("Maze Settings")]
    public int gridWidth = 21;
    public int gridHeight = 21;
    public int numberOfBombs = 5;
    public float wallHeight = 2f;
    public float wallThickness = 0.1f; // 🆕 Editable wall thickness

    [Header("Enemy Settings")]
    public int numberOfEnemies = 2;
    public float enemySpeed = 3.5f;
    public Vector3 enemyRotation; // 🆕 Editable rotation

    private int[,] maze;
    private List<Vector2Int> pathCells = new List<Vector2Int>();
    private float cellSize;

    private Transform wallsParent;
    private Transform bombsParent;
    private Transform enemiesParent;

    void Start()
    {
        if (gridWidth % 2 == 0) gridWidth++;
        if (gridHeight % 2 == 0) gridHeight++;

        wallsParent = new GameObject("Walls").transform;
        bombsParent = new GameObject("Bombs").transform;
        enemiesParent = new GameObject("Enemies").transform;

        SetupCellSize();
        GenerateMaze();
        BuildMaze();
        navMeshSurface.BuildNavMesh(); // Rebuild NavMesh after maze is built
        SpawnBombs();
        SpawnPlayerAtCenter();
        SpawnEnemies();
    }

    void SetupCellSize()
    {
        Renderer renderer = plane.GetComponent<Renderer>();
        Vector3 planeSize = renderer.bounds.size;
        cellSize = Mathf.Min(planeSize.x / gridWidth, planeSize.z / gridHeight);
    }

    void GenerateMaze()
    {
        maze = new int[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                maze[x, y] = 0;

        pathCells.Clear();
        RecursiveBacktrack(1, 1);
    }

    void RecursiveBacktrack(int x, int y)
    {
        maze[x, y] = 1;
        pathCells.Add(new Vector2Int(x, y));

        int[] dirs = { 0, 1, 2, 3 };
        Shuffle(dirs);

        foreach (int dir in dirs)
        {
            int dx = 0, dy = 0;
            switch (dir)
            {
                case 0: dy = -2; break;
                case 1: dx = 2; break;
                case 2: dy = 2; break;
                case 3: dx = -2; break;
            }

            int nx = x + dx;
            int ny = y + dy;

            if (nx > 0 && ny > 0 && nx < gridWidth - 1 && ny < gridHeight - 1 && maze[nx, ny] == 0)
            {
                maze[x + dx / 2, y + dy / 2] = 1;
                pathCells.Add(new Vector2Int(x + dx / 2, y + dy / 2));
                RecursiveBacktrack(nx, ny);
            }
        }
    }

    void BuildMaze()
    {
        Vector3 origin = plane.position - new Vector3(gridWidth / 2f * cellSize, 0, gridHeight / 2f * cellSize);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (maze[x, y] == 0) continue;

                Vector3 cellCenter = origin + new Vector3(x * cellSize, 0, y * cellSize);

                if (y + 1 >= gridHeight || maze[x, y + 1] == 0)
                {
                    Vector3 pos = cellCenter + new Vector3(0, wallHeight / 2f, cellSize / 2f);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, wallsParent);
                    wall.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);
                }
                if (y - 1 < 0 || maze[x, y - 1] == 0)
                {
                    Vector3 pos = cellCenter + new Vector3(0, wallHeight / 2f, -cellSize / 2f);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, wallsParent);
                    wall.transform.localScale = new Vector3(cellSize, wallHeight, wallThickness);
                }
                if (x + 1 >= gridWidth || maze[x + 1, y] == 0)
                {
                    Vector3 pos = cellCenter + new Vector3(cellSize / 2f, wallHeight / 2f, 0);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, wallsParent);
                    wall.transform.localScale = new Vector3(wallThickness, wallHeight, cellSize);
                }
                if (x - 1 < 0 || maze[x - 1, y] == 0)
                {
                    Vector3 pos = cellCenter + new Vector3(-cellSize / 2f, wallHeight / 2f, 0);
                    GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, wallsParent);
                    wall.transform.localScale = new Vector3(wallThickness, wallHeight, cellSize);
                }
            }
        }
    }

    void SpawnBombs()
    {
        Vector3 origin = plane.position - new Vector3(gridWidth / 2f * cellSize, 0, gridHeight / 2f * cellSize);
        List<Vector2Int> bombCandidates = new List<Vector2Int>();

        foreach (Vector2Int cell in pathCells)
        {
            if (cell.x % 2 == 1 && cell.y % 2 == 1)
                bombCandidates.Add(cell);
        }

        int bombsPlaced = 0;

        while (bombsPlaced < numberOfBombs && bombCandidates.Count > 0)
        {
            int index = Random.Range(0, bombCandidates.Count);
            Vector2Int cell = bombCandidates[index];
            bombCandidates.RemoveAt(index);

            Vector3 spawnPos = origin + new Vector3(cell.x * cellSize, 0.5f, cell.y * cellSize);
            Instantiate(bombPrefab, spawnPos, Quaternion.identity, bombsParent);
            bombsPlaced++;
        }
    }

    void SpawnPlayerAtCenter()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("No player object found with tag 'Player'");
            return;
        }

        Vector2Int centerCell = new Vector2Int(gridWidth / 2, gridHeight / 2);
        Vector2Int closest = centerCell;
        float closestDist = float.MaxValue;

        foreach (var cell in pathCells)
        {
            float dist = Vector2Int.Distance(centerCell, cell);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = cell;
            }
        }

        Vector3 origin = plane.position - new Vector3(gridWidth / 2f * cellSize, 0, gridHeight / 2f * cellSize);
        Vector3 spawnPos = origin + new Vector3(closest.x * cellSize, player.transform.position.y, closest.y * cellSize);
        player.transform.position = spawnPos;
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy Prefab not assigned!");
            return;
        }

        Vector3 origin = plane.position - new Vector3(gridWidth / 2f * cellSize, 0, gridHeight / 2f * cellSize);
        List<Vector2Int> candidates = new List<Vector2Int>(pathCells);

        for (int i = 0; i < numberOfEnemies && candidates.Count > 0; i++)
        {
            int index = Random.Range(0, candidates.Count);
            Vector2Int cell = candidates[index];
            candidates.RemoveAt(index);

            Vector3 spawnPos = origin + new Vector3(cell.x * cellSize, 0.1f, cell.y * cellSize);
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.Euler(enemyRotation), enemiesParent);

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
                agent.speed = enemySpeed;
        }
    }

    void Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rand = Random.Range(i, array.Length);
            (array[i], array[rand]) = (array[rand], array[i]);
        }
    }
}
