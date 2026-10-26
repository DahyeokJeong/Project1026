using UnityEngine;

public class EnemyAttackState : IState
{
    private EnemyController enemy;
    public EnemyAttackState(EnemyController enemy)
    {
        this.enemy = enemy;
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
        if (!enemy.CanAttack)
            return;

        switch (enemy.Type)
        {
            case EnemyAttackType.Melee:
                break;
            case EnemyAttackType.Ranged:
                break;
        }

    }
}
