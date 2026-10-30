using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Components
    [Header("Component")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerModel model;
    #endregion

    #region Properties
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;
    #endregion

    public void Move(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude <= 0f)
            return;

        Vector2 moveDir = moveInput.normalized;
        model.LastMoveDirection = moveDir;

        Vector2 nextPos =
            rb.position + moveDir * model.MoveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPos);
    }
}
