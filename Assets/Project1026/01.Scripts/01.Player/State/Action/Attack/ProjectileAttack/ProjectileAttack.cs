using System.Collections.Generic;
using UnityEngine;
public class ProjectileAttack : ITickAttack
{
    #region Components
    private readonly Transform firePoint;
    private readonly PlayerModel model;
    private readonly AttackConfigSO config;
    #endregion

    #region Attack Settings
    private const float TargetSearchRadius = 5f;
    private readonly float attackInterval;
    private readonly ProjectileAttackRuntimeData data;
    private readonly ContactFilter2D enemyFilter;
    #endregion

    #region Fields
    private readonly List<Collider2D> targetHits = new();
    private float attackTimer;
    #endregion

    public ProjectileAttack(Transform firePoint, PlayerModel model, AttackConfigSO config, float attackInterval, ProjectileAttackRuntimeData data)
    {
        this.firePoint = firePoint;
        this.model = model;
        this.config = config;
        this.attackInterval = Mathf.Max(0.01f, attackInterval);
        this.data = data;

        ContactFilter2D filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        enemyFilter = filter;

        attackTimer = this.attackInterval;
    }

    public void Tick(float deltaTime)
    {
        float speed = config.UsePlayerStat ? Mathf.Max(0f, model.AttackSpeed) : 1f;
        attackTimer -= deltaTime * speed / model.AutoAttackCooldownMultiplier;

        if (attackTimer > 0f)
            return;

        Fire();
        attackTimer = attackInterval;
    }

    private void Fire()
    {
        int count = data.ProjectileCount;

        if (count <= 0)
            return;

        Vector2 direction = GetLaunchDirection();
        switch(config.Pattern)
        {
            case ProjectilePattern.Parallel:
                FireParallel(count, direction);
                break;
            case ProjectilePattern.Spread:
                FireSpread(count, direction);
                break;
            case ProjectilePattern.Homing:
                FireHoming(count, direction);
                break;
        }
    }

    private Vector2 GetLaunchDirection()
    {
        Vector2 playerPosition = model.transform.position;
        Vector2 launchPosition = firePoint.position;
        Physics2D.OverlapCircle(playerPosition, TargetSearchRadius, enemyFilter, targetHits);

        float nearestDistance = TargetSearchRadius * TargetSearchRadius;
        Vector2 direction = model.LastMoveDirection.normalized;

        foreach (Collider2D hit in targetHits)
        {
            if (!Managers.Damage.TryGetTarget(hit.transform, out Transform target) ||
                !target.gameObject.activeInHierarchy)
                continue;

            Vector2 targetPosition = target.position;
            float distance = (targetPosition - playerPosition).sqrMagnitude;
            Vector2 aim = targetPosition - launchPosition;
            if (distance > nearestDistance || aim == Vector2.zero)
                continue;

            nearestDistance = distance;
            direction = aim.normalized;
        }

        return direction;
    }

    private void SpawnProjectile(Vector2 position, Vector2 direction)
    {
        GameObject go = Managers.Pool.Pop(config.Prefab, position, Quaternion.identity);

        if (go != null && go.TryGetComponent(out ProjectileBase projectile))
        {
            float attackDamage = model.AttackDamage;

            projectile.SetDirection(direction);
            projectile.SetDamage(attackDamage, model.Critical, data.DamageBonus);
            projectile.SetBounceCount(data.BounceCount);
            if (go.TryGetComponent(out ProjectileHoming homing))
                homing.SetHomingEnabled(data.HomingEnabled);
            if (projectile is PiercingProjectile piercingProjectile)
                piercingProjectile.SetProjectileSizeBonus(data.ProjectileSizeBonusPercent);
            if (projectile is ExplosionProjectile explosionProjectile)
                explosionProjectile.SetExplosionRadiusBonus(data.ExplosionRadiusBonusPercent);
        }
    }

    private void FireParallel(int count, Vector2 direction)
    {
        float spacing = 0.4f;
        float startOffset = -(count - 1) * spacing * 0.5f;

        Vector2 side = new Vector2(-direction.y, direction.x);

        for (int i = 0; i < count; i++)
        {
            float offset = startOffset + i * spacing;
            Vector2 spawnPosition =
                (Vector2)firePoint.position + side * offset;

            SpawnProjectile(spawnPosition, direction);
        }
    }

    private void FireSpread(int count, Vector2 direction)
    {
        float angleSpacing = 15f;
        float startAngle = -(count - 1) * angleSpacing * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + i * angleSpacing;
            Vector2 spreadDirection = Quaternion.Euler(0f, 0f, angle) * direction;

            SpawnProjectile(firePoint.position, spreadDirection);
        }
    }

    private void FireHoming(int count, Vector2 direction)
    {
        for(int i = 0; i < count; ++i)
        {
            SpawnProjectile(firePoint.position, direction);
        }
    }
}
