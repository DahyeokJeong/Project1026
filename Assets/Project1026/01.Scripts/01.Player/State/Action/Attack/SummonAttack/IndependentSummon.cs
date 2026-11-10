using System.Collections.Generic;
using UnityEngine;

public class IndependentSummon : SummonBase
{
    #region Settings
    [Header("Tornado Settings")]
    [SerializeField, Min(0f)] private float moveSpeed = 3f;
    [SerializeField, Min(0.01f)] private float attackRadius = 0.5f;
    #endregion

    #region Fields
    private readonly List<Collider2D> overlaps = new(32);
    private readonly List<RaycastHit2D> enemyHits = new(32);
    private readonly List<RaycastHit2D> wallHits = new(8);
    private readonly HashSet<Transform> damagedTargets = new();

    private ContactFilter2D wallFilter;
    private Vector2 moveDirection;
    private float remainingLifetime;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(LayerMask.GetMask("Wall"));
        wallFilter.useTriggers = false;
    }

    protected override void OnSummonInitialized(float interval)
    {
        moveDirection = Owner.LastMoveDirection.normalized;
        if (moveDirection == Vector2.zero)
            moveDirection = Vector2.down;

        damagedTargets.Clear();
        remainingLifetime = interval;
    }

    public void SetDurationBonus(float durationBonus)
    {
        remainingLifetime *= 1f + durationBonus;
    }

    private void Update()
    {
        if (!CanUpdateSummon())
            return;

        float deltaTime = Mathf.Min(Time.deltaTime, remainingLifetime);
        remainingLifetime -= deltaTime;

        Vector2 position = transform.position;
        if (Physics2D.OverlapCircle(position, attackRadius, wallFilter, overlaps) > 0)
        {
            Release();
            return;
        }

        float distance = moveSpeed * deltaTime;
        bool hitWall = LimitDistanceAtWall(position, ref distance);
        DamageEnemiesAlongPath(position, distance, hitWall);

        if (hitWall)
        {
            Release();
            return;
        }

        position += moveDirection * distance;
        transform.position = new Vector3(position.x, position.y, transform.position.z);

        if (remainingLifetime <= 0f)
            Release();
    }

    private bool LimitDistanceAtWall(Vector2 position, ref float distance)
    {
        if (distance <= 0f)
            return false;

        Physics2D.CircleCast(
            position, attackRadius, moveDirection, wallFilter, wallHits, distance);

        bool hitWall = false;
        foreach (RaycastHit2D hit in wallHits)
        {
            if (hit.collider == null || hit.distance > distance)
                continue;

            distance = hit.distance;
            hitWall = true;
        }

        return hitWall;
    }

    private void DamageEnemiesAlongPath(Vector2 position, float distance, bool hitWall)
    {
        float damage = CalculateDamage();
        Physics2D.OverlapCircle(position, attackRadius, EnemyFilter, overlaps);

        foreach (Collider2D overlap in overlaps)
            TryDamage(overlap, damage);

        if (distance <= 0f)
            return;

        Physics2D.CircleCast(
            position, attackRadius, moveDirection, EnemyFilter, enemyHits, distance);

        foreach (RaycastHit2D hit in enemyHits)
        {
            if (hitWall && hit.distance >= distance)
                continue;

            TryDamage(hit.collider, damage);
        }
    }

    private void TryDamage(Collider2D hit, float damage)
    {
        if (hit == null || !Managers.Damage.TryGetTarget(hit.transform, out Transform target))
            return;

        if (!target.gameObject.activeInHierarchy || !damagedTargets.Add(target))
            return;

        Managers.Damage.ApplyDamage(target, damage);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        overlaps.Clear();
        enemyHits.Clear();
        wallHits.Clear();
        damagedTargets.Clear();
    }
}