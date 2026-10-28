using UnityEngine;

public class EnemyHitState : IState
{
    private EnemyController enemy;
    private int damage;

    public EnemyHitState(EnemyController enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        enemy.IsHit = false;
    }

    public void Exit()
    {
    }

    public void FixedTick()
    {
    }

    public void Tick()
    {
    }

    public void GetDamage(int damage)
    {
        this.damage = damage;
    }
}
