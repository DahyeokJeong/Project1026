using UnityEngine;

public class EnemyHitState : IState
{
    private EnemyController enemy;
    private float hitDuration = 0.2f;
    private float flashDuration = 0.1f;
    private float timer;

    public EnemyHitState(EnemyController enemy)
    {
        this.enemy = enemy;
    }
    public void Enter()
    {
        timer = hitDuration;
        enemy.View.SetHitFlash(true);
    }

    public void Exit()
    {
        enemy.View.SetHitFlash(false);
    }

    public void FixedTick()
    {
    }

    public void Tick()
    {
        bool wasFlashing = timer > flashDuration;

        timer -= Time.deltaTime;

        if (wasFlashing && timer <= flashDuration)
            enemy.View.SetHitFlash(false);

        if (timer <= 0f)
            enemy.Model.IsHit = false;
    }
}
