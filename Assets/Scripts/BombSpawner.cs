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
    [SerializeField, Tooltip("How many bombs will spawn inside the maze")]
    private int numberOfBombs = 5;
    public float wallHeight = 3f;
    [SerializeField, Tooltip("How many walls will be spawned across the maze (affects density)")]
    private int numberOfWalls = 200;

    [Header("Player Settings")]
    [SerializeField, Tooltip("How high above the plane the player should spawn to avoid clipping")]
    private float playerSpawnHeightOffset = 1.5f;

    [Header("Enemy Settings")]
    public int numberOfEnemies = 2;
    public float enemySpeed = 3.5f;
    public Vector3 enemyRotation;

    [Header("Finish Settings")]
    [SerializeField, Tooltip("Prefab for the finish portal (a small circle on the plane)")]
    private GameObject finishPortalPrefab;
    [SerializeField, Tooltip("UI object that appears when the player wins")]
    private GameObject winUI;
    [SerializeField, Tooltip("Height offset so portal doesn't clip into the ground")]
    private float portalSpawnHeight = 0.5f;
    [SerializeField, Tooltip("How many finish portals can spawn")]
    private int numberOfFinishPortals = 1;

    private int[,] maze;
    private List<Vector2Int> pathCells = new();
    private float cellSize;
    private Transform wallsParent;
    private Transform bombsParent;
    private Transform enemiesParent;
    private Transform portalsParent;

    private readonly float[] thicknessMultipliers = { 10f, 30f, 50f };

    void Start()
    {
        if (gridWidth % 2 == 0) gridWidth++;
        if (gridHeight % 2 == 0) gridHeight++;

        wallsParent = new GameObject("Walls").transform;
        bombsParent = new GameObject("Bombs").transform;
        enemiesParent = new GameObject("Enemies").transform;
        portalsParent = new GameObject("FinishPortals").transform;

        SetupCellSize();
        GenerateMaze();
        BuildMaze();
        navMeshSurface.BuildNavMesh();
        SpawnBombs();
        SpawnPlayerAtCenter();
        SpawnEnemies();
        SpawnFinishPortals();
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
        float halfHeight = wallHeight / 2f;
        List<Vector2Int> allCells = new();

        for (int x = 0; x < gridWidth; x++)
            for (int y = 0; y < gridHeight; y++)
                if (maze[x, y] == 0) allCells.Add(new Vector2Int(x, y));

        int wallCount = Mathf.Min(numberOfWalls, allCells.Count);
        int attempts = 0;

        for (int i = 0; i < wallCount && attempts < allCells.Count * 2; i++)
        {
            attempts++;
            Vector2Int cell = allCells[Random.Range(0, allCells.Count)];
            Vector3 pos = origin + new Vector3(cell.x * cellSize, halfHeight, cell.y * cellSize);

            float randomThickness = thicknessMultipliers[Random.Range(0, thicknessMultipliers.Length)];

            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity, wallsParent);
            wall.transform.localScale = new Vector3(randomThickness, wallHeight, randomThickness);
            wall.tag = "Wall";

            bool overlapped = false;
            Collider[] hits = Physics.OverlapBox(wall.transform.position, wall.transform.localScale / 2f);
            foreach (Collider hit in hits)
            {
                if (hit.gameObject != wall && hit.CompareTag("Wall"))
                {
                    overlapped = true;
                    break;
                }
            }

            if (overlapped)
                Destroy(wall);
        }
    }

    void SpawnBombs()
    {
        Vector3 origin = plane.position - new Vector3(gridWidth / 2f * cellSize, 0, gridHeight / 2f * cellSize);
        List<Vector2Int> bombCandidates = new();
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
        if (player == null) return;

        Vector2Int centerCell = new(gridWidth / 2, gridHeight / 2);
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
        Vector3 spawnPos = origin + new Vector3(closest.x * cellSize, playerSpawnHeightOffset, closest.y * cellSize);
        player.transform.position = spawnPos;
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null) return;
        Vector3 origin = plane.position - new Vector3(gridWidth / 2f * cellSize, 0, gridHeight / 2f * cellSize);
        List<Vector2Int> candidates = new(pathCells);
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

    void SpawnFinishPortals()
    {
        if (finishPortalPrefab == null)
        {
            Debug.LogWarning("Finish Portal Prefab not assigned!");
            return;
        }

        Vector3 origin = plane.position - new Vector3(gridWidth / 2f * cellSize, 0, gridHeight / 2f * cellSize);
        int portalsPlaced = 0, safetyCounter = 0;

        while (portalsPlaced < numberOfFinishPortals && safetyCounter < 5000)
        {
            safetyCounter++;
            Vector2Int cell = pathCells[Random.Range(0, pathCells.Count)];
            Vector3 spawnPos = origin + new Vector3(cell.x * cellSize, portalSpawnHeight, cell.y * cellSize);

            Collider[] hits = Physics.OverlapBox(spawnPos, Vector3.one * (cellSize * 0.4f));
            bool blocked = false;
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("Wall"))
                {
                    blocked = true;
                    break;
                }
            }
            if (blocked) continue;

            GameObject portal = Instantiate(finishPortalPrefab, spawnPos, Quaternion.identity, portalsParent);
            var portalScript = portal.GetComponent<FinishPortal>();
            if (portalScript != null) portalScript.AssignWinUI(winUI);
            portalsPlaced++;
        }

        if (portalsPlaced == 0)
            Debug.LogWarning("⚠ No valid location found for Finish Portal!");
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
