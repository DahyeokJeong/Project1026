public class PlayerIdleState : IState
{
    private PlayerController controller;

    public PlayerIdleState(PlayerController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        //controller.View.SetIdleAnimation();
    }

    public void Tick()
    {
    }

    public void FixedTick()
    {
    }

    public void Exit()
    {
    }
}
