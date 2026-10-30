using System;
using UnityEngine;
public class ProjectileAttack : IAttack
{
    #region Components
    private readonly Transform firePoint;
    private readonly PlayerModel model;
    private readonly AttackConfigSO config;
    #endregion

    #region Attack Settings
    private const float spacing = 0.4f;
    private readonly float attackInterval;
    private readonly Func<int> projectileCount;
    #endregion

    #region Fields
    private float attackTimer;
    #endregion

    public ProjectileAttack(
        Transform firePoint,
        PlayerModel model,
        AttackConfigSO config,
        float attackInterval,
        Func<int> projectileCount)
    {
        this.firePoint = firePoint;
        this.model = model;
        this.config = config;
        this.attackInterval = Mathf.Max(0.01f, attackInterval);
        this.projectileCount = projectileCount;

        attackTimer = this.attackInterval;
    }

    public void Tick(float deltaTime)
    {
        attackTimer -= deltaTime;

        if (attackTimer > 0f)
            return;

        Fire();
        attackTimer = attackInterval;
    }

    private void Fire()
    {
        int count = projectileCount();

        if (count <= 0)
            return;

        switch(config.Pattern)
        {
            case ProjectilePattern.Parallel:
                FireParallel(count);
                break;
            case ProjectilePattern.Spread:
                FireSpread(count);
                break;
        }
    }
    private void FireParallel(int count)
    {
        float spacing = 0.4f;
        float startOffset = -(count - 1) * spacing * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float offset = startOffset + i * spacing;
            Vector2 spawnPosition = (Vector2)firePoint.position + Vector2.up * offset;

            SpawnProjectile(spawnPosition, model.LastMoveDirection);
        }
    }

    private void FireSpread(int count)
    {
        float spacing = 0.4f;
        float startOffset = -(count - 1) * spacing * 0.5f;

        for (int i = 0; i < count; i++)
        {
            float offset = startOffset + i * spacing;
            Vector2 spawnPosition = (Vector2)firePoint.position + Vector2.up * offset;

            SpawnProjectile(spawnPosition, model.LastMoveDirection);
        }
    }

    private void SpawnProjectile(Vector2 position, Vector2 direction)
    {
        GameObject go = Managers.Pool.Pop(config.Prefab, position, Quaternion.identity);

        if (go != null && go.TryGetComponent(out ProjectileBase projectile))
        {
            projectile.SetDirection(direction);
            projectile.SetDamage(model.AttackDamage, model.Critical);
        }
    }

}
