using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Components
    [Header("Component")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerModel model;
    #endregion

    #region Dash
    private Vector2 dashDirection;
    private float dashTime;
    private float dashSpeed;
    private float cooldown;

    public bool IsDashing => dashTime > 0f;
    public bool CanDash => !IsDashing && cooldown <= 0f;
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

    public void TickCooldown(float deltatime)
    {
        cooldown = Mathf.Max(0f, cooldown - deltatime);
    }

    public bool TryStartDash(Vector2 inputDir)
    {
        if (!CanDash)
            return false;

        dashDirection = inputDir.sqrMagnitude > 0f ? inputDir.normalized : model.LastMoveDirection;

        LastMoveDirection = dashDirection;
        model.LastMoveDirection = dashDirection;

        float duration = Mathf.Max(0.01f, model.DashDuration);

        dashSpeed = Mathf.Max(0.01f, model.DashDistance) / duration;
        dashTime = duration;
        cooldown = Mathf.Max(0f, model.DashCooldown);

        return true;
    }

    public void TickDash(float deltaTime)
    {
        if (!IsDashing)
            return;

        float stepTime = Mathf.Min(deltaTime, dashTime);

        Vector2 nextPos = rb.position + dashDirection * dashSpeed * stepTime;

        rb.MovePosition(nextPos);

        dashTime = Mathf.Max(0f, dashTime - stepTime);
    }    

    public void CancelDash()
    {
        dashTime = 0f;
    }
}
