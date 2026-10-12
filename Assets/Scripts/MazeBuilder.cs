using System.Collections.Generic;
using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
    [Header("Префабы модулей (можно оставить пустыми)")]
    [SerializeField] private GameObject floorPrefab;
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject columnPrefab;

    private const float Cell = 3f;
    private const float WallHeight = 3f;
    private const float WallThickness = 0.4f;
    private const float FloorThickness = 0.2f;

    // # стена, . проход, S старт игрока
    private static readonly string[] Map =
    {
        "##############",
        "#S...........#",
        "#..###.###.#.#",
        "#.####.#.#.#.#",
        "#.........##.#",
        "#.##.#.##.##.#",
        "#.##....#....#",
        "#.##.#....##.#",
        "#..#.##.#.##.#",
        "#.##......##.#",
        "#.#.###.##.#.#",
        "#.#.###.##.#.#",
        "#............#",
        "##############",
    };

    private void Awake()
    {
        if (transform.childCount == 0) Build();
    }

    [ContextMenu("Build Maze")]
    public void Build()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        int rows = Map.Length;
        var corners = new HashSet<Vector2Int>();
        float h = Cell / 2f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < Map[r].Length; c++)
            {
                if (IsWall(r, c)) continue;

                float cx = c * Cell + h;
                float cz = (rows - 1 - r) * Cell + h;

                Spawn(floorPrefab, "Floor_Tile",
                    new Vector3(cx, -FloorThickness / 2f, cz),
                    new Vector3(Cell, FloorThickness, Cell), 0f);

                bool n = IsWall(r - 1, c);
                bool s = IsWall(r + 1, c);
                bool e = IsWall(r, c + 1);
                bool w = IsWall(r, c - 1);

                Vector3 wallScale = new Vector3(Cell, WallHeight, WallThickness);
                float y = WallHeight / 2f;

                if (n) Spawn(wallPrefab, "Wall_Straight", new Vector3(cx, y, cz + h), wallScale, 0f);
                if (s) Spawn(wallPrefab, "Wall_Straight", new Vector3(cx, y, cz - h), wallScale, 0f);
                if (e) Spawn(wallPrefab, "Wall_Straight", new Vector3(cx + h, y, cz), wallScale, 90f);
                if (w) Spawn(wallPrefab, "Wall_Straight", new Vector3(cx - h, y, cz), wallScale, 90f);

                TryCorner(corners, cx + h, cz + h, n || e || IsWall(r - 1, c + 1));
                TryCorner(corners, cx - h, cz + h, n || w || IsWall(r - 1, c - 1));
                TryCorner(corners, cx + h, cz - h, s || e || IsWall(r + 1, c + 1));
                TryCorner(corners, cx - h, cz - h, s || w || IsWall(r + 1, c - 1));
            }
        }
    }

    private bool IsWall(int r, int c)
    {
        if (r < 0 || r >= Map.Length || c < 0 || c >= Map[r].Length) return true;
        return Map[r][c] == '#';
    }

    private void TryCorner(HashSet<Vector2Int> used, float x, float z, bool needed)
    {
        if (!needed) return;
        var key = new Vector2Int(Mathf.RoundToInt(x * 10f), Mathf.RoundToInt(z * 10f));
        if (!used.Add(key)) return;

        Spawn(columnPrefab, "Wall_Corner",
            new Vector3(x, WallHeight / 2f, z),
            new Vector3(WallThickness, WallHeight, WallThickness), 0f);
    }

    private void Spawn(GameObject prefab, string objName, Vector3 pos, Vector3 scale, float rotY)
    {
        GameObject go = prefab != null ? Instantiate(prefab) : GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = objName;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = Quaternion.Euler(0f, rotY, 0f);
        if (prefab == null) go.transform.localScale = scale;
    }
}