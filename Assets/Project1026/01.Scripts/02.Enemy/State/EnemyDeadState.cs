using UnityEngine;

public class EnemyDeadState : IState
{
    private const float FlashDuration = 0.06f;
    private readonly EnemyController enemy;
    private float elapsed;

    public EnemyDeadState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        elapsed = 0f;
        enemy.View.SetHitFlash(true);
    }

    public void Tick()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= FlashDuration)
            enemy.CompleteDeath();
    }

    public void Exit()
    {
    }

    public void FixedTick()
    {
    }
}
