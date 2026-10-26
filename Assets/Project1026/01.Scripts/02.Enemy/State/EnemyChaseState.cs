using UnityEngine;

public class EnemyChaseState : IState
{
    private EnemyController enemy;

    float movespeed;
    private Vector2 dir;

    public EnemyChaseState(EnemyController enemy)
    {
        this.enemy = enemy;
        movespeed = 2f;
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
    }

    public void Tick()
    {
        dir = enemy.NavArea == null ? Vector2.zero : enemy.NavArea.GetDirection(enemy.transform.position);

        Vector3 movement = new Vector3(dir.x, dir.y, 0f);

        enemy.transform.position += movement * movespeed * Time.deltaTime;
    }
}
