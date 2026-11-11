using UnityEngine;

public class PlayerAnimationView
{
    private Animator animator;
    private SpriteRenderer bodyRenderer;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");

    public void Init(Animator animator, SpriteRenderer bodyRenderer)
    {
        this.animator = animator;
        this.bodyRenderer = bodyRenderer;
    }

    public void SetMoveAnimation(Vector2 moveInput)
    {
        animator.SetBool(IsMoving, moveInput.sqrMagnitude > 0f);

        if (moveInput.sqrMagnitude <= 0f)
            return;

        UpdateFacing(moveInput.x);
        animator.SetFloat(MoveX, moveInput.x);
        animator.SetFloat(MoveY, moveInput.y);
    }

    private void UpdateFacing(float horizontal)
    {
        if (bodyRenderer != null && Mathf.Abs(horizontal) > 0.01f)
            bodyRenderer.flipX = horizontal < 0f;
    }

    public void SetIdleAnimation()
    {
        animator.SetBool(IsMoving, false);
    }
}