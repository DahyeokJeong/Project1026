using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dashAction;

    // Input Values
    public Vector2 MoveInput { get; private set; }
    public bool DashPressed => dashAction.action.WasPressedThisFrame();

    private void OnEnable()
    {
        moveAction.action.Enable();
        dashAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        dashAction.action.Disable();

        MoveInput = Vector2.zero;
    }

    private void Update()
    {
        MoveInput = moveAction.action.ReadValue<Vector2>();
    }
}
