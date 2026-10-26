using UnityEngine;

public class EnemyAttackState : IState
{
    private EnemyController enemy;
    private float timer;
    public EnemyAttackState(EnemyController enemy)
    {
        this.enemy = enemy;
        timer = enemy.AttackTimer;
    }

    public void Enter()
    {
    }

    public void Exit()
    {

    }

    public void FixedTick()
    {
    }

    public void Tick()
    {
        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        Vector2 dir = enemy.NavArea.Target.position - enemy.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        switch (enemy.Type)
        {
            case EnemyAttackType.Melee:
                break;
            case EnemyAttackType.Ranged:
                if (dir.sqrMagnitude > 0f)
                {
                    GameObject attackObject = Managers.Pool.Pop(
                        enemy.AttackPrefab, 
                        enemy.transform.position,
                        Quaternion.Euler(0f, 0f, angle)
                        );
                    if (attackObject == null)
                        return;
                    if(attackObject.TryGetComponent(out RangeAttack attack))
                    {
                        attack.SetDirection(dir);
                    }
                }
                break;
        }

        timer = Mathf.Max(0f, enemy.AttackTimer);
    }

    public void ResetTimer()
    {
        timer = Mathf.Max(0f, enemy.AttackTimer);
    }
}
