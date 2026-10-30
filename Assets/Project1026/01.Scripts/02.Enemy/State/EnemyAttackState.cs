using UnityEngine;

public class EnemyAttackState : IState
{
    private EnemyController enemy;
    private EnemyAttack attack;
    private float timer;
    public EnemyAttackState(EnemyController enemy, EnemyAttack attack)
    {
        this.enemy = enemy;
        this.attack = attack;

        ResetTimer();
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

        Transform target = enemy.NavArea.Target;

        attack.Execute(target);
        ResetTimer();
    }

    public void ResetTimer()
    {
        timer = Mathf.Max(0f, enemy.Model.AttackInerval);
    }
}
