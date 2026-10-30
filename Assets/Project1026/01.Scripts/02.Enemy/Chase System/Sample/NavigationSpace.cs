using UnityEngine;
using UnityEngine.UIElements;

public class NavigationSpace
{
    private readonly NavigationGrid grid;
    private readonly float agentRadius;
    private readonly float padding;
    private readonly LayerMask obstacleLayer;

    public NavigationSpace 
        (
            NavigationGrid grid,
            float agendtRadius,
            float padding,
            LayerMask obsLayer
        )
    {
        this.grid = grid;
        this.agentRadius = agendtRadius;
        this.padding = padding;
        this.obstacleLayer = obsLayer;
    }

    public bool IsValidPos(Vector2 pos)
    {
        if (grid == null)
            return false;
        if (!grid.TryWorldToCell(pos, out Vector2Int cell))
            return false;
        if (!grid.IsWalkable(cell))
            return false;

        Collider2D obs = Physics2D.OverlapCircle(
            pos,
            agentRadius + padding,
            obstacleLayer
            );

        if (obs != null)
            return false;

        return true;
    }
}
