using System.Collections.Generic;
using UnityEngine;

public class IndependentSummon : SummonBase
{
    #region Settings
    [Header("Movement Settings")]
    [SerializeField, Min(0f)] private float moveSpeed = 3f;
    [SerializeField, Min(0.01f)] private float searchRadius = 6f;
    [SerializeField, Min(0f)] private float stopDistance = 0.2f;
    [SerializeField, Min(0.01f)] private float searchInterval = 0.2f;
    #endregion

    #region Fields
    private readonly List<Collider2D> searchBuffer = new(32);
    private Transform target;
    private float searchTimer;
    #endregion

    protected override void OnSummonInitialized()
    {
        target = null;
        searchTimer = 0f;
        searchBuffer.Clear();
    }

    protected override void Move(float deltaTime)
    {
        if (!IsValidTarget())
            target = null;

        searchTimer -= deltaTime;
        if (searchTimer <= 0f)
        {
            SelectTarget();
            searchTimer = Mathf.Max(0.01f, searchInterval);
        }

        if (target == null)
            return;

        Vector2 position = transform.position;
        Vector2 offset = (Vector2)target.position - position;
        float distance = offset.magnitude;
        float moveDistance = Mathf.Min(moveSpeed * deltaTime, Mathf.Max(0f, distance - stopDistance));

        if (moveDistance <= 0f)
            return;

        position += offset.normalized * moveDistance;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    private bool IsValidTarget()
    {
        return target != null && target.gameObject.activeInHierarchy && Managers.Damage.IsRegistered(target);
    }

    private void SelectTarget()
    {
        Physics2D.OverlapCircle(transform.position, searchRadius, EnemyFilter, searchBuffer);

        target = null;
        float nearestDistance = searchRadius * searchRadius;

        foreach (Collider2D hit in searchBuffer)
        {
            if (hit == null)
                continue;

            if (!Managers.Damage.TryGetTarget(hit.transform, out Transform candidate))
                continue;

            if (!candidate.gameObject.activeInHierarchy)
                continue;

            float distance = (candidate.position - transform.position).sqrMagnitude;
            if (distance > nearestDistance)
                continue;

            nearestDistance = distance;
            target = candidate;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        target = null;
        searchBuffer.Clear();
    }
}