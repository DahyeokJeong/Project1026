using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileWallCollision))]
public class WispProjectile : ProjectileBase
{
    #region Settings
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0.01f)] private float hitRadius = 0.3f;
    [SerializeField] private GameObject impactPrefab;
    #endregion

    #region Fields
    private readonly List<Collider2D> hitBuffer = new(32);
    private ContactFilter2D enemyFilter;
    #endregion

    private void Awake()
    {
        enemyFilter = new ContactFilter2D();
        enemyFilter.SetLayerMask(enemyLayer);
        enemyFilter.useTriggers = true;
    }

    protected override void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        base.Update();

        if (!gameObject.activeInHierarchy)
            return;

        Transform target = FindHitTarget(out Vector2 impactPosition);
        if (target == null)
            return;

        Managers.Damage.ApplyDamage(target, Damage);
        if (impactPrefab != null)
            Managers.Pool.Pop(impactPrefab, impactPosition, Quaternion.identity);

        Release();
    }

    private Transform FindHitTarget(out Vector2 impactPosition)
    {
        Vector2 position = transform.position;
        impactPosition = position;
        Physics2D.OverlapCircle(position, hitRadius, enemyFilter, hitBuffer);

        Transform nearest = null;
        float nearestDistance = float.PositiveInfinity;

        foreach (Collider2D hit in hitBuffer)
        {
            if (hit == null || !Managers.Damage.TryGetTarget(hit.transform, out Transform candidate))
                continue;

            if (!candidate.gameObject.activeInHierarchy)
                continue;

            Vector2 closestPoint = hit.ClosestPoint(position);
            float distance = (closestPoint - position).sqrMagnitude;
            if (distance >= nearestDistance)
                continue;

            nearest = candidate;
            nearestDistance = distance;
            impactPosition = closestPoint;
        }

        return nearest;
    }

    private void OnDisable()
    {
        hitBuffer.Clear();
    }
}
