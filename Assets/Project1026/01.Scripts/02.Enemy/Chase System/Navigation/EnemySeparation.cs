using System.Collections.Generic;
using UnityEngine;

public class EnemySeparation
{
    private readonly MapArea mapArea;
    private readonly NavigationGrid grid;

    private readonly float agentRadius;
    private readonly float padding;
    private readonly float enemyGap;

    private readonly Collider2D[] neighbors = new Collider2D[64];
    private ContactFilter2D filter;

    public EnemySeparation(MapArea mapArea, NavigationGrid grid, float agentRadius, float padding, float enemyGap, LayerMask enemyLayer)
    {
        this.mapArea = mapArea;
        this.grid = grid;
        this.agentRadius = agentRadius;
        this.padding = padding;
        this.enemyGap = enemyGap;

        filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = false;
    }

    public void Resolve(IReadOnlyDictionary<EnemyController, Vector2> enemies)
    {
        if (enemies.Count < 2 || agentRadius <= 0f)
            return;

        Physics2D.SyncTransforms();

        float minimumDistance = agentRadius * 2f + enemyGap;

        foreach (EnemyController enemy in enemies.Keys)
        {
            if (enemy == null)
                continue;

            Vector2 position = enemy.transform.position;
            Vector2 correction = Vector2.zero;

            int count = Physics2D.OverlapCircle(position, minimumDistance, filter, neighbors);

            for (int i = 0; i < count; i++)
            {
                EnemyController other = neighbors[i].GetComponentInParent<EnemyController>();

                if (other == null ||
                    other == enemy ||
                    !enemies.ContainsKey(other))
                    continue;

                Vector2 offset = position - (Vector2)other.transform.position;

                float distance = offset.magnitude;

                if (distance >= minimumDistance)
                    continue;

                Vector2 direction;

                if (distance > 0.001f)
                {
                    direction = offset / distance;
                }
                else
                {
                    direction = GetOverlapDirection(enemy, other, enemies);
                }

                correction += direction * (minimumDistance - distance) * 0.5f;
            }

            if (correction.magnitude <= 0.001f)
                continue;

            Push(enemy.transform, correction);
            Physics2D.SyncTransforms();
        }
    }

    private Vector2 GetOverlapDirection(EnemyController enemy, EnemyController other, IReadOnlyDictionary<EnemyController, Vector2> enemies)
    {
        foreach (EnemyController registered in enemies.Keys)
        {
            if (registered == enemy)
                return Vector2.left;

            if (registered == other)
                return Vector2.right;
        }

        return Vector2.right;
    }

    private void Push(Transform enemy, Vector2 correction)
    {
        if (!mapArea.TryGetInserBounds(agentRadius, out Vector2 min, out Vector2 max))
            return;

        Vector2 position = enemy.position;
        Vector2 next = position + correction;

        next.x = Mathf.Clamp(next.x, min.x, max.x);
        next.y = Mathf.Clamp(next.y, min.y, max.y);

        correction = next - position;

        float distance = correction.magnitude;

        if (distance < 0.0001f)
            return;

        Vector2 direction = correction / distance;

        RaycastHit2D hit = Physics2D.CircleCast(position, agentRadius, direction, distance, mapArea.Obs);

        float allowedDistance = hit.collider == null ? distance : Mathf.Max(0f, hit.distance - 0.01f);

        Vector2 destination = position + direction * allowedDistance;

        if (!mapArea.CanPlace(destination, agentRadius + padding))
            return;

        if (!grid.TryWorldToCell(destination, out Vector2Int cell))
            return;

        if (!grid.IsWalkable(cell))
            return;

        enemy.position += (Vector3)(destination - position);
    }
}