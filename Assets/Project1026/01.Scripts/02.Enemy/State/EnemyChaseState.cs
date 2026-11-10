using UnityEngine;

public class EnemyChaseState : IState
{
    private EnemyController enemy;

    private Vector2 dir;
    private Vector2 approachOffset;
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
        approachOffset = Vector2.zero;
        movement = Vector3.zero;
    }

    public void FixedTick()
    {
        Vector3 displacement = movement * enemy.Model.MoveSpeed * Time.deltaTime;
        enemy.transform.position += displacement;
        enemy.View.SetMovementFacing(displacement);
    }

    public void Tick()
    {
        dir = enemy.NavArea == null ? Vector2.zero : enemy.NavArea.GetDirection(enemy, ref approachOffset);
        movement = new Vector3(dir.x, dir.y, 0f);
    }
}
