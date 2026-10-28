using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region Component
    [Header("Component")]
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerMovement movement;
    #endregion

    #region State
    // States
    private IState currentState;
    private IState prevState;
    

    private PlayerIdleState idleState;
    private PlayerMoveState moveState;
    #endregion

    #region Properties
    // Properties
    public PlayerInputHandler InputHandler => inputHandler;
    public PlayerMovement Movement => movement;
    #endregion

    private void Awake()
    {
        idleState = new PlayerIdleState(this);
        moveState = new PlayerMoveState(this);
    }

    private void Start()
    {
        ChangeState(idleState);
    }

    private void Update()
    {
        HandleStateChange();
        currentState?.Tick();
    }

    private void FixedUpdate()
    {
        currentState?.FixedTick();
    }

    private void HandleStateChange()
    {
        bool isMoving = inputHandler.MoveInput.sqrMagnitude > 0f;

        if (currentState == idleState && isMoving)
        {
            ChangeState(moveState);
        }
        else if (currentState == moveState && !isMoving)
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
}
