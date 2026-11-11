using UnityEngine;

public class EnemyDeadState : IState
{
    private float duration = 0.3f;
    private float flashDuration = 0.06f;
    private float elapsed;
    private EnemyController enemy;

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
        bool wasFlashing = elapsed < flashDuration;
        elapsed += Time.deltaTime;

        if (elapsed < flashDuration)
            return;

        if (wasFlashing)
            enemy.View.BeginDeathEffect();

        enemy.View.SetDeathFade((elapsed - flashDuration) / (duration - flashDuration));

        if (elapsed >= duration)
            enemy.CompleteDeath();
    }

    public void Exit()
    {
    }

    public void FixedTick()
    {
    }
}