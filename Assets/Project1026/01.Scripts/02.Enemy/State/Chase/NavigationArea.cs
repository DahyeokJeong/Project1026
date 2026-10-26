using UnityEngine;
using System.Collections.Generic;

public class NavigationArea : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Vector2 worldSize = new Vector2(25, 10);
    [SerializeField, Min(0.05f)] private float cellSize = 0.5f;

    [Header("Obstacle")]
    [SerializeField, Min(0.01f)] private float agentRadius = 0.5f;
    [SerializeField, Min(0f)] private float padding = 0.15f;
    [SerializeField, Min(0f)] private float enemyGap = 0.1f;

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("LayerMask")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask enemyLayer;

    //몬스터 등록
    private Dictionary<EnemyController, Vector2> enemies = new();
    private Collider2D[] neighbors = new Collider2D[64];

    public Transform Target => target;
    public LayerMask Obstacle => obstacleLayer;
    public NavigationGrid Grid { get; private set; }
    private FlowField flowField;
    private Vector2Int? lastTargetCell;

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

        if (enemies.Count < 2 || agentRadius <= 0f)
            return;

        Physics2D.SyncTransforms();

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = false;

        float dia = agentRadius * 2f + enemyGap;

        foreach(EnemyController enemy in enemies.Keys)
        {
            if(enemy == null)
                continue;

            Vector2 pos = enemy.transform.position;
            Vector2 cor = Vector2.zero;

            int count = Physics2D.OverlapCircle(pos, dia, filter, neighbors);

            for(int i = 0; i < count; ++i)
            {
                EnemyController other = neighbors[i].GetComponentInParent<EnemyController>();

                if (other == null || other == enemy || other.Area != this)
                    continue;

                Vector2 offset = pos - (Vector2)other.transform.position;

                float dis = offset.magnitude;

                if (dis >= dia)
                    continue;

                Vector2 dir;

                if(dis > 0.001f)
                {
                    dir = offset / dis;
                }
                else 
                {
                    if (!enemies.ContainsKey(other))
                        continue;

                    dir = Vector2.right;

                    foreach (EnemyController register in enemies.Keys)
                    {
                        if (register == enemy)
                        {
                            dir = Vector2.left;
                            break;
                        }

                        if (register == other)
                            break;
                    }
                }
                cor += dir * (dia - dis) * 0.5f;
            }
            if(cor.magnitude > 0.001f)
            {
                Push(enemy.transform, cor);
                Physics2D.SyncTransforms();
            }
        }
    }

    private void Push(Transform enemy, Vector2 cor)
    {
        if (Grid == null)
            return;

        Vector2 pos = enemy.position;

        Vector2 min = Grid.Origin + Vector2.one * agentRadius;

        Vector2 max = Grid.Origin + new Vector2(Grid.Width, Grid.Height) * Grid.CellSize - Vector2.one * agentRadius;

        if (min.x > max.x || min.y > max.y)
            return;

        Vector2 next = pos + cor;

        next.x = Mathf.Clamp(next.x, min.x, max.x);
        next.y = Mathf.Clamp(next.y, min.y, max.y);

        cor = next - pos;

        float dis = cor.magnitude;

        if (dis < 0.0001f)
            return;

        Vector2 dir = cor / dis;

        RaycastHit2D hit = Physics2D.CircleCast(enemy.position, agentRadius, dir, dis, obstacleLayer);

        float allowDis = hit.collider == null ? dis : Mathf.Max(0f, hit.distance - 0.01f);

        Vector2 des= pos + dir * allowDis;

        if (!Grid.TryWorldToCell(des, out Vector2Int cell))
            return;

        if (!Grid.IsWalkable(cell))
            return;

        if (Physics2D.OverlapCircle(des, agentRadius + padding, obstacleLayer ) != null)
            return;

        enemy.position += (Vector3)(des - pos);
    }

    public void Initialize()
    {
        Grid = new NavigationGrid(transform.position, worldSize, cellSize);

        flowField = new FlowField(Grid);
        RefreshObstacles();
    }

    public void RefreshObstacles()
    {
        if (Grid == null)
            return;

        Physics2D.SyncTransforms();

        for(int x = 0; x < Grid.Width; ++x)
        {
            for(int y = 0; y < Grid.Height; ++y)
            {
                Vector2Int cel = new Vector2Int(x, y);
                Vector2 center = Grid.GetCellCenter(cel);

                Collider2D obstacle = Physics2D.OverlapCircle(center, agentRadius + padding, obstacleLayer);

                Grid.SetWalkable(cel, obstacle == null);
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

    private void OnDrawGizmosSelected()
    {
        if(Grid == null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(worldSize.x, worldSize.y, 0.1f));
            return;
        }

        for (int x = 0; x < Grid.Width; ++x)
        {
            for (int y = 0; y < Grid.Height; ++y)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Vector2 center = Grid.GetCellCenter(cell);

                Gizmos.color = Grid.IsWalkable(cell) ? Color.green : Color.red;
                Gizmos.DrawWireCube(new Vector3(center.x, center.y, transform.position.z), new Vector3(Grid.CellSize, Grid.CellSize, 0.1f) * 0.9f);
            }
        }
    }
}
