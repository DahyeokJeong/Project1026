using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Component
    [Header("Component")]
    [SerializeField] private PlayerModel model;
    [SerializeField] private PlayerView view;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerMovement movement;
    #endregion

    #region State
    // States
    private IState currentState;
    private IState prevState;
    
    private PlayerIdleState idleState;
    private PlayerMoveState moveState;
    private PlayerDashState dashState;
    #endregion

    #region Properties
    // Properties
    public PlayerInputHandler InputHandler => inputHandler;
    public PlayerMovement Movement => movement;
    public PlayerModel Model => model;
    public PlayerView View => view;
    #endregion

    private void Awake()
    {
        idleState = new PlayerIdleState(this);
        moveState = new PlayerMoveState(this);
        dashState = new PlayerDashState(this);

        view.InitAnimation();
        Managers.RunData.SetPlayer(model, view);
    }

    private void Start()
    {
        Managers.RunData.SetPlayer(model, view);
        ChangeState(idleState);
    }

    private void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        movement.TickCooldown(Time.deltaTime);

        HandleStateChange();
        currentState?.Tick();
    }

    private void FixedUpdate()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        currentState?.FixedTick();
    }

    private void HandleStateChange()
    {
        if (currentState == dashState && movement.IsDashing)
            return;

        if (inputHandler.DashPressed && movement.TryStartDash(inputHandler.MoveInput))
        {
            ChangeState(dashState);
            return;
        }

        if(inputHandler.MoveInput.sqrMagnitude > 0f)
            ChangeState(moveState);
        else
        {
            ChangeState(idleState);
        }
    }

    public void ChangeState(IState newState)
    {
        if (currentState == newState)
            return;

        currentState?.Exit();

        prevState = currentState;
        currentState = newState;

        currentState?.Enter();
    }

    private void OnDisable()
    {
        movement?.CancelDash();
    }
}
