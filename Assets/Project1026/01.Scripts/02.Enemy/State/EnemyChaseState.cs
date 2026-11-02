using UnityEngine;

public class EnemyChaseState : IState
{
    private EnemyController enemy;

    private Vector2 dir;
    private Vector3 movement;
    public EnemyChaseState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
    }

    public void Exit()
    {
        dir = Vector2.zero;
    }

    public void FixedTick()
    {
        enemy.transform.position += movement * enemy.Model.MoveSpeed * Time.deltaTime;
    }

    public void Tick()
    {
        dir = enemy.NavArea == null ? Vector2.zero : enemy.NavArea.GetDirection(enemy.transform.position);
        movement = new Vector3(dir.x, dir.y, 0f);
    }
}
