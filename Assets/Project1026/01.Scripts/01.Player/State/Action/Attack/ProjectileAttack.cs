using UnityEngine;
public class ProjectileAttack : IAttack
{
    #region Components
    private readonly Transform firePoint;
    private readonly PlayerModel model;
    private readonly AttackConfigSO config;
    #endregion

    #region Attack Settings
    private readonly float attackInterval;
    #endregion

    #region Fields
    private float attackTimer;
    #endregion

    public ProjectileAttack(
        Transform firePoint,
        PlayerModel model,
        AttackConfigSO config,
        float attackInterval)
    {
        this.firePoint = firePoint;
        this.model = model;
        this.config = config;
        this.attackInterval = Mathf.Max(0.01f, attackInterval);

        attackTimer = this.attackInterval;
    }

    public void Tick(float deltaTime)
    {
        attackTimer -= deltaTime * Mathf.Max(0f, model.AttackSpeed);

        if (attackTimer > 0f)
            return;

        Fire();
        attackTimer = attackInterval;
    }

    private void Fire()
    {
        GameObject go = Managers.Pool.Pop(config.Prefab, firePoint.position, Quaternion.identity);

        if (go != null &&
            go.TryGetComponent(out ProjectileBase projectile))
        {
            projectile.SetDirection(model.LastMoveDirection);
            projectile.SetDamage(model.AttackDamage);
        }
    }
}
