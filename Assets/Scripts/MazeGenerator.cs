using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public int width = 21;  // must be odd
    public int height = 21; // must be odd
    public float cellSize = 1f;

    public GameObject wallPrefab;
    public GameObject bombPrefab;
    public int numberOfBombs = 5;

    private int[,] maze;

    void Start()
    {
        GenerateMaze();
        BuildMaze();
        SpawnBombs();
    }

    void GenerateMaze()
    {
        maze = new int[width, height];

        // Fill maze with walls
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                maze[x, y] = 0;

        // Start DFS from (1, 1)
        RecursiveBacktrack(1, 1);
    }

    void RecursiveBacktrack(int x, int y)
    {
        maze[x, y] = 1;

        int[] dirs = { 0, 1, 2, 3 }; // N, E, S, W
        Shuffle(dirs);

        foreach (int dir in dirs)
        {
            int dx = 0, dy = 0;

            switch (dir)
            {
                case 0: dy = -2; break; // North
                case 1: dx = 2; break;  // East
                case 2: dy = 2; break;  // South
                case 3: dx = -2; break; // West
            }

            int nx = x + dx;
            int ny = y + dy;

            if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1 && maze[nx, ny] == 0)
            {
                maze[x + dx / 2, y + dy / 2] = 1; // break wall between
                RecursiveBacktrack(nx, ny);
            }
        }
    }

    void BuildMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 0)
                {
                    Vector3 pos = new Vector3(x * cellSize, 0.5f, y * cellSize);
                    Instantiate(wallPrefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }

    void SpawnBombs()
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (maze[x, y] == 1) validPositions.Add(new Vector2Int(x, y));
            }
        }

        for (int i = 0; i < numberOfBombs && validPositions.Count > 0; i++)
        {
            int index = Random.Range(0, validPositions.Count);
            Vector2Int pos = validPositions[index];
            validPositions.RemoveAt(index);

            Vector3 worldPos = new Vector3(pos.x * cellSize, 0.5f, pos.y * cellSize);
            Instantiate(bombPrefab, worldPos, Quaternion.identity, transform);
        }
    }

    void Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rnd = Random.Range(i, array.Length);
            int temp = array[rnd];
            array[rnd] = array[i];
            array[i] = temp;
        }
    }
}
