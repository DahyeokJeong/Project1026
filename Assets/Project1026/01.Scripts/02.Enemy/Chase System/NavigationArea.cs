using UnityEngine;
using System.Collections.Generic;

public class NavigationArea : MonoBehaviour
{
    #region Grid
    [Header("Grid")]
    [SerializeField, Min(0.05f)] private float cellSize = 0.5f;
    [SerializeField] private MapArea mapArea;
    #endregion

    #region Obstacle
    [Header("Obstacle")]
    [SerializeField, Min(0.01f)] private float agentRadius = 0.5f;
    [SerializeField, Min(0f)] private float padding = 0.15f;
    [SerializeField, Min(0f)] private float enemyGap = 0.1f;
    #endregion
    #region Target
    [Header("Target")]
    [SerializeField] private Transform target;
    #endregion
    #region Layer
    [Header("LayerMask")]
    [SerializeField] private LayerMask enemyLayer;
    #endregion
    //몬스터 등록
    private Dictionary<EnemyController, Vector2> enemies = new();
    private EnemySeparation separation;


    #region Property
    public float AgentRadius => agentRadius;
    public float Padding => padding;
    public Transform Target => target;
    public LayerMask Obstacle => mapArea.Obs;
    public NavigationGrid Grid { get; private set; }
    #endregion

    private FlowField flowField;
    private Vector2Int? lastTargetCell;
    public bool IsReady => mapArea != null && mapArea.IsReady && Grid != null && flowField != null;

    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        RefreshFlowField();
    }

    private void LateUpdate()
    {
        if (!IsReady)
            return;
        
        separation.Resolve(enemies);
    }

    public void Initialize()
    {
        if (mapArea == null || !mapArea.IsReady)
            return;

        Bounds bounds = mapArea.worldBounds;

        Grid = new NavigationGrid(bounds.center, bounds.size, cellSize);
        flowField = new FlowField(Grid);
        separation = new EnemySeparation(mapArea, Grid, agentRadius, padding, enemyGap, enemyLayer);

        RefreshObstacles();
    }

    public void RefreshObstacles()
    {
        if (!IsReady)
            return;

        Physics2D.SyncTransforms();
        float clearance = agentRadius + padding;

        for(int x = 0; x < Grid.Width; ++x)
        {
            for(int y = 0; y < Grid.Height; ++y)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Vector2 center = Grid.GetCellCenter(cell);

                Grid.SetWalkable(cell, mapArea.CanPlace(center, clearance));
            }
        }
        RefreshFlowField(true);
    }

    public void Register(EnemyController enemy)
    {
        if (enemy != null && !enemies.ContainsKey(enemy))
            enemies.Add(enemy, Vector2.zero);
    }

    public void Unregister(EnemyController enemy)
    {
        enemies.Remove(enemy);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        RefreshFlowField(true);
    }

    public Vector2 GetDirection(Vector2 pos)
    {
        return flowField == null ? Vector2.zero : flowField.GetDirection(pos);
    }

    private void RefreshFlowField(bool force = false)
    {
        if (Grid == null || flowField == null)
            return;

        Vector2Int? targetCell = null;

        if (target != null && Grid.TryWorldToCell(target.position, out Vector2Int cell))
        {
            targetCell = cell;
        }

        if (!force && targetCell == lastTargetCell)
            return;

        flowField.Build(targetCell ?? new Vector2Int(-1, -1));
        lastTargetCell = targetCell;
    }

    private void OnDrawGizmos()
    {
        if (Grid == null)
            return;

        Vector3 size = new Vector3(Grid.CellSize, Grid.CellSize, 0f);

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Vector2 center = Grid.GetCellCenter(cell);

                Gizmos.color = Grid.IsWalkable(cell)
                    ? Color.green
                    : Color.red;

                Gizmos.DrawWireCube(
                    new Vector3(center.x, center.y, mapArea.worldBounds.center.z),
                    size);
            }
        }
    }

}