using System.Collections.Generic;
using UnityEngine;

public class EnemySeparation
{
    private readonly MapArea mapArea;
    private readonly NavigationGrid grid;

    private readonly float agentRadius;
    private readonly float padding;

    private readonly Collider2D[] neighbors = new Collider2D[64];
    private readonly RaycastHit2D[] approachHits = new RaycastHit2D[64];
    private ContactFilter2D filter;

    public EnemySeparation(MapArea mapArea, NavigationGrid grid, float agentRadius, float padding, LayerMask enemyLayer)
    {
        this.mapArea = mapArea;
        this.grid = grid;
        this.agentRadius = agentRadius;
        this.padding = padding;

        filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = false;
    }

    public bool IsApproachClear(EnemyController enemy, Vector2 direction,
        float distance, IReadOnlyDictionary<EnemyController, Vector2> enemies)
    {
        Bounds footprint = GetFootprint(enemy);
        int count = Physics2D.BoxCast(footprint.center, footprint.size, 0f, direction,
            filter, approachHits, distance);
        if (count == approachHits.Length)
            return false;

        for (int i = 0; i < count; i++)
        {
            EnemyController other = approachHits[i].collider.GetComponentInParent<EnemyController>();
            if (other == null || other == enemy || !enemies.ContainsKey(other))
                continue;

            Vector2 offset = (Vector2)(GetFootprint(other).center - footprint.center);
            if (approachHits[i].fraction > 0f || Vector2.Dot(offset, direction) > 0f)
                return false;
        }

        return true;
    }

    public Vector2 Steer(EnemyController enemy, Vector2 direction,
        float lookAhead, float strength, IReadOnlyDictionary<EnemyController, Vector2> enemies,
        out bool pathCrowded)
    {
        pathCrowded = false;
        if (enemies.Count < 2)
            return direction;

        Bounds footprint = GetFootprint(enemy);
        Vector2 castSize = (Vector2)footprint.size + Vector2.one * (lookAhead * 2f);
        Vector2 side = new Vector2(-direction.y, direction.x);
        float pressure = 0f;
        float strongestBlock = 0f;
        int count = Physics2D.OverlapBox(footprint.center, castSize, 0f, filter, neighbors);
        for (int i = 0; i < count; i++)
        {
            EnemyController other = neighbors[i].GetComponentInParent<EnemyController>();
            if (other == null || other == enemy || !enemies.ContainsKey(other))
                continue;

            Bounds otherFootprint = GetFootprint(other);
            Vector2 offset = (Vector2)(otherFootprint.center - footprint.center);
            Vector2 extents = (Vector2)(footprint.extents + otherFootprint.extents);
            float width = Mathf.Abs(side.x) * extents.x + Mathf.Abs(side.y) * extents.y;
            float range = Mathf.Abs(direction.x) * extents.x + Mathf.Abs(direction.y) * extents.y + lookAhead;
            float forward = Vector2.Dot(offset, direction);
            float lateral = Vector2.Dot(offset, side);
            if (forward <= 0f || forward >= range || Mathf.Abs(lateral) >= width)
                continue;

            float weight = (1f - forward / range) * (1f - Mathf.Abs(lateral) / width);
            pressure += weight * lateral / width;
            strongestBlock = Mathf.Max(strongestBlock, weight);
        }

        if (strongestBlock <= 0f)
            return direction;

        float turn = Mathf.Abs(pressure) > 0.01f
            ? -Mathf.Sign(pressure)
            : ((enemy.GetEntityId().GetHashCode() & 1) == 0 ? 1f : -1f);

        pathCrowded = !IsApproachClear(enemy, direction, lookAhead, enemies);
        return (direction + side * (turn * strongestBlock * strength)).normalized;
    }

    public void Resolve(IReadOnlyDictionary<EnemyController, Vector2> enemies)
    {
        if (enemies.Count < 2 || agentRadius <= 0f)
            return;

        Physics2D.SyncTransforms();

        foreach (EnemyController enemy in enemies.Keys)
        {
            if (enemy == null || enemy.IsHoldingAttackPosition)
                continue;

            Bounds footprint = GetFootprint(enemy);
            Vector2 correction = Vector2.zero;

            int count = Physics2D.OverlapBox(footprint.center, footprint.size, 0f, filter, neighbors);

            for (int i = 0; i < count; i++)
            {
                EnemyController other = neighbors[i].GetComponentInParent<EnemyController>();

                if (other == null ||
                    other == enemy ||
                    !enemies.ContainsKey(other))
                    continue;

                Bounds otherFootprint = GetFootprint(other);
                Vector2 offset = (Vector2)(footprint.center - otherFootprint.center);
                Vector2 extents = (Vector2)(footprint.extents + otherFootprint.extents);
                float overlapX = extents.x - Mathf.Abs(offset.x);
                float overlapY = extents.y - Mathf.Abs(offset.y);
                if (overlapX <= 0f || overlapY <= 0f)
                    continue;

                if (offset == Vector2.zero)
                    offset = GetOverlapDirection(enemy, other, enemies);

                Vector2 push = overlapX < overlapY
                    ? Vector2.right * (Mathf.Sign(offset.x) * overlapX)
                    : Vector2.up * (Mathf.Sign(offset.y) * overlapY);
                correction += push * (other.IsHoldingAttackPosition ? 1f : 0.5f);
            }

            if (correction.magnitude <= 0.001f)
                continue;

            Push(enemy.transform, correction);
            Physics2D.SyncTransforms();
        }
    }

    private Bounds GetFootprint(EnemyController enemy)
    {
        Collider2D collider = enemy.MovementCollider;
        if (collider != null && collider.enabled && !collider.isTrigger)
            return collider.bounds;

        return new Bounds(enemy.transform.position, Vector3.one * (agentRadius * 2f));
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