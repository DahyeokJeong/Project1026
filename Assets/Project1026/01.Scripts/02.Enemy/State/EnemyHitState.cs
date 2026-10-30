using UnityEngine;

public class EnemyHitState : IState
{
    private EnemyController enemy;
    private float hitDuration = 0.2f;
    private float timer;

    public EnemyHitState(EnemyController enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        timer = hitDuration;
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
        if (timer <= 0f)
            enemy.Model.IsHit = false;
    }
}
