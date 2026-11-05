using UnityEngine;

public class PlayerDashState : IState
{
    PlayerController player;

    public PlayerDashState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
    }

    public void Exit()
    {
        player.Movement.CancelDash();
    }

    public void FixedTick()
    {
        player.Movement.TickDash(Time.fixedDeltaTime);
    }

    public void Tick()
    {
    }
}
