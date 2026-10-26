using UnityEngine;

public class SpreadAttack : IAttack
{
    #region Components
    private readonly Transform firePoint;
    private readonly PlayerMovement movement;
    #endregion

    #region Attack Settings
    private readonly float attackInterval;
    private readonly float spreadAngle;
    #endregion

    #region Fields
    private float attackTimer;
    #endregion

    public SpreadAttack(
        Transform firePoint,
        PlayerMovement movement,
        float attackInterval,
        float spreadAngle)
    {
        this.firePoint = firePoint;
        this.movement = movement;
        this.attackInterval = Mathf.Max(0.01f, attackInterval);
        this.spreadAngle = spreadAngle;

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
        Vector2 baseDirection = movement.LastMoveDirection;

        for (int i = -1; i <= 1; i++)
        {
            Vector2 direction =
                Quaternion.Euler(0f, 0f, i * spreadAngle) * baseDirection;

            GameObject go = Managers.Pool.Pop(
                PoolType.AttackB,
                firePoint.position,
                Quaternion.identity
            );

            if (go != null && go.TryGetComponent(out AttackB attack))
                attack.SetDirection(direction);
        }
    }
}