using UnityEngine;

public class PlayerDashState : IState
{
    PlayerController player;
    private float elapsed;

    public PlayerDashState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        elapsed = 0f;
        player.View.SetIdleAnimation();
        UpdateAnimation();
    }

    public void Exit()
    {
        player.Movement.CancelDash();
        player.View.StopDashAnimation();
    }

    public void FixedTick()
    {
        player.Movement.TickDash(Time.fixedDeltaTime);
        elapsed += Time.fixedDeltaTime;
    }

    public void Tick()
    {
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        float duration = Mathf.Max(0.01f, player.Model.DashDuration);
        player.View.SetDashAnimation(player.Movement.LastMoveDirection, elapsed / duration);
    }
}
