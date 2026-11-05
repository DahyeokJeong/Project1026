using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dashAction;

    // Input Values
    public Vector2 MoveInput => moveAction != null && moveAction.action != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
    public bool DashPressed => dashAction != null && dashAction.action != null && dashAction.action.WasPressedThisFrame();

    private void OnEnable()
    {
        moveAction.action.Enable();
        dashAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        dashAction.action.Disable();
    }
}
