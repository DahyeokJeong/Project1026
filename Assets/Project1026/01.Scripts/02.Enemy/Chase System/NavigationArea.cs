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

    private CapsuleCollider2D targetCollider;
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

        CacheTargetCollider();
        RefreshObstacles();
    }

    public void RefreshObstacles()
    {
        if (!IsReady)
            return;

        Physics2D.SyncTransforms();

        for(int x = 0; x < Grid.Width; ++x)
        {
            for(int y = 0; y < Grid.Height; ++y)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Vector2 center = Grid.GetCellCenter(cell);

                Grid.SetWalkable(cell, CanPlaceNavigationAgent(center));
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
        CacheTargetCollider();
        RefreshObstacles();
    }

    public Vector2 GetDirection(EnemyController enemy, ref Vector2? flankDestination)
    {
        Vector2 position = enemy.transform.position;
        Vector2 direction = GetPathDirection(position);
        if (direction == Vector2.zero || avoidanceLookAhead <= 0f || avoidanceStrength <= 0f)
        {
            flankDestination = null;
            return direction;
        }

        Vector2 avoided = separation.Steer(enemy, direction, avoidanceLookAhead,
            avoidanceStrength, enemies, out bool pathCrowded);
        if (!pathCrowded || IsLocalStepClear(enemy, position, direction))
        {
            flankDestination = null;
            return direction;
        }

        if (flankDestination.HasValue &&
            TryGetApproachDirection(enemy, position, flankDestination.Value, out Vector2 flankDirection))
        {
            return flankDirection;
        }

        flankDestination = null;
        if (TryFindFlank(enemy, position, avoided, direction, out Vector2 destination, out flankDirection))
        {
            flankDestination = destination;
            return flankDirection;
        }

        if (avoided != direction && IsLocalStepClear(enemy, position, avoided))
            return avoided;

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


    private bool IsLocalStepClear(EnemyController enemy, Vector2 position, Vector2 direction)
    {
        return IsLocalStepClear(enemy, position, direction, avoidanceLookAhead);
    }

    private bool IsLocalStepClear(EnemyController enemy, Vector2 position, Vector2 direction, float distance)
    {
        if (direction == Vector2.zero)
            return false;

        return IsObstaclePathClear(position, position + direction * distance) &&
            separation.IsApproachClear(enemy, direction, distance, enemies);
    }

    private bool TryFindFlank(EnemyController enemy, Vector2 position, Vector2 avoided,
        Vector2 direction, out Vector2 destination, out Vector2 flankDirection)
    {
        Vector2 radial = (position - (Vector2)target.position).normalized;
        float approachRadius = Mathf.Max(agentRadius, enemy.Model.AttackRange - padding);
        Vector2 side = new Vector2(-direction.y, direction.x);
        float preferredSide = Vector2.Dot(avoided, side) >= 0f ? -1f : 1f;

        for (int step = 1; step <= 2; step++)
        {
            float angle = flankAngle * step * preferredSide;
            destination = GetFlankDestination(position, radial, angle, approachRadius);
            if (TryGetApproachDirection(enemy, position, destination, out flankDirection))
                return true;

            destination = GetFlankDestination(position, radial, -angle, approachRadius);
            if (TryGetApproachDirection(enemy, position, destination, out flankDirection))
                return true;
        }

        // When the inner approaches are blocked, advance around the outside of the crowd.
        float outerRadius = Vector2.Distance(position, target.position);
        float outerAngle = flankAngle * 0.5f * preferredSide;
        destination = GetFlankDestination(position, radial, outerAngle, outerRadius);
        if (TryGetApproachDirection(enemy, position, destination, out flankDirection))
            return true;

        destination = GetFlankDestination(position, radial, -outerAngle, outerRadius);
        if (TryGetApproachDirection(enemy, position, destination, out flankDirection))
            return true;

        destination = Vector2.zero;
        flankDirection = Vector2.zero;
        return false;
    }

    private Vector2 GetFlankDestination(Vector2 position, Vector2 radial, float angle, float radius)
    {
        Vector2 offset = (Vector2)(Quaternion.Euler(0f, 0f, angle) * radial) * radius;
        return Vector2.MoveTowards(position, (Vector2)target.position + offset, avoidanceLookAhead);
    }

    private bool TryGetApproachDirection(EnemyController enemy, Vector2 position, Vector2 destination,
        out Vector2 direction)
    {
        Vector2 delta = destination - position;
        float distance = delta.magnitude;
        direction = delta.normalized;
        return distance > Mathf.Min(Grid.CellSize, avoidanceLookAhead) * 0.5f &&
            IsObstaclePathClear(position, destination) &&
            separation.IsApproachClear(enemy, direction, distance, enemies);
    }

    private Vector2 GetPathDirection(Vector2 pos)
    {
        if (!IsReady || target == null)
            return Vector2.zero;

        Vector2 destination = target.position;
        Vector2 toTarget = destination - pos;

        if (toTarget == Vector2.zero)
            return Vector2.zero;

        if (IsObstaclePathClear(pos, destination))
            return toTarget.normalized;

        return flowField.GetDirection(pos);
    }

    private void CacheTargetCollider()
    {
        targetCollider = null;
        if (target != null && target.TryGetComponent(out CapsuleCollider2D capsule) && !capsule.isTrigger)
            targetCollider = capsule;
    }

    private bool CanPlaceNavigationAgent(Vector2 position)
    {
        if (targetCollider == null)
            return mapArea.CanPlace(position, agentRadius + padding);

        if (!mapArea.TryGetInserBounds(0f, out Vector2 min, out Vector2 max) ||
            position.x < min.x || position.x > max.x || position.y < min.y || position.y > max.y)
            return false;

        Vector2 offset = targetCollider.transform.TransformVector(targetCollider.offset);
        return Physics2D.OverlapCapsule(position + offset, GetNavigationCapsuleSize(),
            targetCollider.direction, targetCollider.transform.eulerAngles.z, Obstacle) == null;
    }

    private bool IsObstaclePathClear(Vector2 start, Vector2 destination)
    {
        if (!CanPlaceNavigationAgent(destination))
            return false;

        Vector2 delta = destination - start;
        float distance = delta.magnitude;
        if (distance <= Mathf.Epsilon)
            return true;

        Vector2 direction = delta / distance;
        if (targetCollider == null)
            return Physics2D.CircleCast(start, agentRadius + padding, direction, distance, Obstacle).collider == null;

        Vector2 offset = targetCollider.transform.TransformVector(targetCollider.offset);
        return Physics2D.CapsuleCast(start + offset, GetNavigationCapsuleSize(), targetCollider.direction,
            targetCollider.transform.eulerAngles.z, direction, distance, Obstacle).collider == null;
    }

    private Vector2 GetNavigationCapsuleSize()
    {
        Vector3 scale = targetCollider.transform.lossyScale;
        return Vector2.Scale(targetCollider.size, new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
    }

    private Vector2Int? FindApproachCell(Vector2Int blockedCell, Vector2 destination)
    {
        const int searchRadius = 3;
        Vector2Int? nearest = null;
        Vector2Int? nearestWithClearPath = null;
        float nearestDistance = float.PositiveInfinity;
        float nearestClearDistance = float.PositiveInfinity;
        bool canStandAtDestination = CanPlaceNavigationAgent(destination);

        for (int y = -searchRadius; y <= searchRadius; y++)
        {
            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                Vector2Int candidate = blockedCell + new Vector2Int(x, y);
                if (!Grid.IsWalkable(candidate))
                    continue;

                Vector2 center = Grid.GetCellCenter(candidate);
                float distance = (center - destination).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }

                if (canStandAtDestination && distance < nearestClearDistance &&
                    IsObstaclePathClear(center, destination))
                {
                    nearestWithClearPath = candidate;
                    nearestClearDistance = distance;
                }
            }
        }

        // A player touching an obstacle can fail the final cast while nearby cells remain reachable.
        return nearestWithClearPath ?? nearest;
    }


    private void RefreshFlowField(bool force = false)
    {
        if (Grid == null || flowField == null)
            return;

        Vector2Int? targetCell = null;

        if (target != null && Grid.TryWorldToCell(target.position, out Vector2Int cell))
        {
            targetCell = Grid.IsWalkable(cell) ? cell : FindApproachCell(cell, target.position);
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