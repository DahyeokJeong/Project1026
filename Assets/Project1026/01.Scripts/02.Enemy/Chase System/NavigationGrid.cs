using UnityEngine;

public class NavigationGrid
{
    private bool[,] walkable;

    public Vector2 Origin { get; }
    public float CellSize { get; }
    public int Width { get; }
    public int Height { get; }

    public NavigationGrid(Vector2 center, Vector2 size, float cellsize)
    {
        CellSize = cellsize;
        Width = Mathf.CeilToInt(size.x / cellsize);
        Height = Mathf.CeilToInt(size.y / cellsize);

        Vector2 gridSize = new Vector2(Width, Height) * cellsize;
        Origin = center - gridSize * 0.5f;

        walkable = new bool[Width, Height];
    }

    public bool Contains(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < Width && cell.y >= 0 && cell.y < Height;
    }

    public bool IsWalkable(Vector2Int cell)
    {
        return Contains(cell) && walkable[cell.x, cell.y];
    }

    public void SetWalkable(Vector2Int cell, bool value)
    {
        walkable[cell.x, cell.y] = value;
    }

    public Vector2 GetCellCenter(Vector2Int cell)
    {
        return Origin + new Vector2(cell.x + 0.5f, cell.y + 0.5f) * CellSize;
    }

    public bool TryWorldToCell(Vector2 pos, out Vector2Int cell)
    {
        Vector2 local = (pos - Origin) / CellSize;

        cell = new Vector2Int(Mathf.FloorToInt(local.x), Mathf.FloorToInt(local.y));
        return Contains(cell);
    }
}
