using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FlowField
{
    private static Vector2Int[] offset =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right,
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1)
    };

    private NavigationGrid grid;
    private int[,] distances;
    private Vector2Int[,] nextCell;
    private Queue<Vector2Int> queue = new();

    public FlowField(NavigationGrid grid)
    {
        this.grid = grid;

        distances = new int[grid.Width, grid.Height];
        nextCell = new Vector2Int[grid.Width, grid.Height];
    }

    public void Build(Vector2Int target)
    {
        queue.Clear();

        for(int x = 0; x < grid.Width; ++x)
        {
            for(int y = 0; y < grid.Height; ++y)
            {
                distances[x, y] = -1;
            }
        }

        if (!grid.IsWalkable(target))
            return;

        distances[target.x, target.y] = 0;
        nextCell[target.x, target.y] = target;
        queue.Enqueue(target);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach(Vector2Int offset in offset)
            {
                Vector2Int neighbor = current + offset;

                if (!grid.IsWalkable(neighbor))
                    continue;
                if (distances[neighbor.x, neighbor.y] != -1)
                    continue;

                if(offset.x != 0 && offset.y != 0)
                {

                    Vector2Int hor = current + new Vector2Int(offset.x, 0);
                    Vector2Int ver = current + new Vector2Int(0, offset.y);

                    if (!grid.IsWalkable(hor) || !grid.IsWalkable(ver))
                        continue;
                }

                distances[neighbor.x, neighbor.y] = distances[current.x, current.y] + 1;

                nextCell[neighbor.x, neighbor.y] = current;
                queue.Enqueue(neighbor);
            }
        }
    }

    public Vector2 GetDirection(Vector2 pos)
    {
        if (!grid.TryWorldToCell(pos, out Vector2Int cell))
            return Vector2.zero;
        if (distances[cell.x, cell.y] < 0)
            return Vector2.zero;

        Vector2Int next = nextCell[cell.x, cell.y];
        Vector2 destination = grid.GetCellCenter(next);
         
        return (destination - pos).normalized;
    }
}
