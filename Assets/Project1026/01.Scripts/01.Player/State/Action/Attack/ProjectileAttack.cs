using UnityEngine;
public class ProjectileAttack : IAttack
{
    #region Components
    private readonly Transform firePoint;
    private readonly PlayerMovement movement;
    #endregion

    #region Attack Settings
    private readonly PoolType poolType;
    private readonly float attackInterval;
    #endregion

    #region Fields
    private float attackTimer;
    #endregion

    public ProjectileAttack(
        Transform firePoint,
        PlayerMovement movement,
        PoolType poolType,
        float attackInterval)
    {
        this.firePoint = firePoint;
        this.movement = movement;
        this.poolType = poolType;
        this.attackInterval = Mathf.Max(0.01f, attackInterval);

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
        GameObject go = Managers.Pool.Pop(
            poolType,
            firePoint.position,
            Quaternion.identity
        );

        if (go != null &&
            go.TryGetComponent(out ProjectileBase projectile))
        {
            projectile.SetDirection(movement.LastMoveDirection);
        }
    }
}
