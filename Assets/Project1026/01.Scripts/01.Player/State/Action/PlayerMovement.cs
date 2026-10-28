using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Components
    [Header("Component")]
    [SerializeField] private Rigidbody2D rb;
    #endregion

    #region Move Settings
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 3f;
    #endregion

    #region Properties
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;
    #endregion

    public void Move(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude <= 0f)
            return;

        Vector2 moveDir = moveInput.normalized;
        LastMoveDirection = moveDir;

        Vector2 nextPos =
            rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(nextPos);
    }
}
