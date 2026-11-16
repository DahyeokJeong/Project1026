using UnityEngine;

public class EnemyDeadState : IState
{
    private const float FlashDuration = 0.06f;
    private readonly EnemyController enemy;
    private float elapsed;
    private bool animationStarted;

    public EnemyDeadState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        elapsed = 0f;
        animationStarted = false;
        enemy.View.SetHitFlash(true);
    }

    public void Tick()
    {
        elapsed += Time.deltaTime;
        if (elapsed < FlashDuration)
            return;

        if (!animationStarted)
        {
            animationStarted = true;
            enemy.View.PlayDeathAnimation();
        }

        if (enemy.View.IsDeathAnimationComplete)
            enemy.CompleteDeath();
    }

    public void Exit()
    {
    }

    public void FixedTick()
    {
    }
}
