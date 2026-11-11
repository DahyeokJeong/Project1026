using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;

    // Input Values
    public Vector2 MoveInput => moveAction != null && moveAction.action != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }
}
