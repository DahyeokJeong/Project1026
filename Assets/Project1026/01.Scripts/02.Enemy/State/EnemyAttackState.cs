using UnityEngine;

public class EnemyAttackState : IState
{
    private EnemyController enemy;
    private EnemyAttack attack;
    Transform target;
    private float timer = 2f;
    public EnemyAttackState(EnemyController enemy, EnemyAttack attack)
    {
        this.enemy = enemy;
        this.attack = attack;
        target = enemy.NavArea.Target;
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


        attack.Execute(target);
        ResetTimer();
    }

    public void ResetTimer()
    {
        timer = Mathf.Max(0f, enemy.Model.AttackInerval);
    }
}
