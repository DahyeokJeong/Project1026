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
    [SerializeField, Min(0f)] private float avoidanceLookAhead = 0.6f;
    [SerializeField, Range(0f, 1f)] private float avoidanceStrength = 0.65f;
    [SerializeField, Range(15f, 90f)] private float flankAngle = 55f;
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
        separation = new EnemySeparation(mapArea, Grid, agentRadius, padding, enemyLayer);

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

    public Vector2 GetDirection(EnemyController enemy, ref Vector2 approachOffset)
    {
        Vector2 position = enemy.transform.position;
        Vector2 direction = GetPathDirection(position);
        if (direction == Vector2.zero || avoidanceLookAhead <= 0f || avoidanceStrength <= 0f)
            return direction;

        if (approachOffset != Vector2.zero &&
            TryGetApproachDirection(enemy, position, approachOffset, out Vector2 flankDirection))
        {
            return flankDirection;
        }

        approachOffset = Vector2.zero;

        Vector2 avoided = separation.Steer(enemy, direction, avoidanceLookAhead,
            avoidanceStrength, enemies, out bool pathCrowded);
        if (pathCrowded &&
            TryFindFlank(enemy, position, avoided, direction, out approachOffset, out flankDirection))
        {
            return flankDirection;
        }

        if (pathCrowded)
        {
            if (IsLocalStepClear(enemy, position, direction))
                return direction;
            if (avoided != direction && IsLocalStepClear(enemy, position, avoided))
                return avoided;

            // A short sideways step can open a route around the front row.
            Vector2 side = Vector2.Perpendicular(direction);
            if (Vector2.Dot(avoided, side) < 0f)
                side = -side;

            float sideStepDistance = Mathf.Min(Grid.CellSize, avoidanceLookAhead);
            if (IsLocalStepClear(enemy, position, side, sideStepDistance))
                return side;
            if (IsLocalStepClear(enemy, position, -side, sideStepDistance))
                return -side;

            return Vector2.zero;
        }

        if (avoided == direction)
            return direction;

        return IsLocalStepClear(enemy, position, avoided) ? avoided : direction;
    }

    private bool IsLocalStepClear(EnemyController enemy, Vector2 position, Vector2 direction)
    {
        return IsLocalStepClear(enemy, position, direction, avoidanceLookAhead);
    }

    private bool IsLocalStepClear(EnemyController enemy, Vector2 position, Vector2 direction, float distance)
    {
        if (direction == Vector2.zero)
            return false;

        float clearance = agentRadius + padding;
        return mapArea.CanPlace(position + direction * distance, clearance) &&
            Physics2D.CircleCast(position, clearance, direction, distance, Obstacle).collider == null &&
            separation.IsApproachClear(enemy, direction, distance, enemies);
    }

    private bool TryFindFlank(EnemyController enemy, Vector2 position, Vector2 avoided,
        Vector2 direction, out Vector2 offset, out Vector2 flankDirection)
    {
        Vector2 radial = (position - (Vector2)target.position).normalized;
        float approachRadius = Mathf.Max(agentRadius, enemy.Model.AttackRange - padding);
        Vector2 side = new Vector2(-direction.y, direction.x);
        float preferredSide = Vector2.Dot(avoided, side) >= 0f ? -1f : 1f;

        for (int step = 1; step <= 2; step++)
        {
            float angle = flankAngle * step * preferredSide;
            offset = (Vector2)(Quaternion.Euler(0f, 0f, angle) * radial) * approachRadius;
            if (TryGetApproachDirection(enemy, position, offset, out flankDirection))
                return true;

            offset = (Vector2)(Quaternion.Euler(0f, 0f, -angle) * radial) * approachRadius;
            if (TryGetApproachDirection(enemy, position, offset, out flankDirection))
                return true;
        }

        // When the inner approaches are blocked, advance around the outside of the crowd.
        float outerRadius = Vector2.Distance(position, target.position);
        float outerAngle = flankAngle * 0.5f * preferredSide;
        offset = (Vector2)(Quaternion.Euler(0f, 0f, outerAngle) * radial) * outerRadius;
        if (TryGetApproachDirection(enemy, position, offset, out flankDirection))
            return true;

        offset = (Vector2)(Quaternion.Euler(0f, 0f, -outerAngle) * radial) * outerRadius;
        if (TryGetApproachDirection(enemy, position, offset, out flankDirection))
            return true;

        offset = Vector2.zero;
        flankDirection = Vector2.zero;
        return false;
    }

    private bool TryGetApproachDirection(EnemyController enemy, Vector2 position, Vector2 offset,
        out Vector2 direction)
    {
        Vector2 destination = (Vector2)target.position + offset;
        Vector2 delta = destination - position;
        float distance = delta.magnitude;
        direction = delta.normalized;
        float clearance = agentRadius + padding;

        return distance > Grid.CellSize * 0.5f &&
            mapArea.CanPlace(destination, clearance) &&
            Physics2D.CircleCast(position, clearance, direction, distance, Obstacle).collider == null &&
            separation.IsApproachClear(enemy, direction, distance, enemies);
    }

    private Vector2 GetPathDirection(Vector2 pos)
    {
        if (!IsReady || target == null)
            return Vector2.zero;

        Vector2 destination = target.position;
        Vector2 toTarget = destination - pos;
        float distance = toTarget.magnitude;

        if (distance <= Mathf.Epsilon)
            return Vector2.zero;

        float clearance = agentRadius + padding;
        Vector2 direction = toTarget / distance;

        if (mapArea.TryGetInserBounds(clearance, out Vector2 min, out Vector2 max)
            && pos.x >= min.x && pos.x <= max.x && pos.y >= min.y && pos.y <= max.y
            && destination.x >= min.x && destination.x <= max.x
            && destination.y >= min.y && destination.y <= max.y
            && Physics2D.CircleCast(pos, clearance, direction, distance, Obstacle).collider == null)
        {
            return direction;
        }

        return flowField.GetDirection(pos);
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