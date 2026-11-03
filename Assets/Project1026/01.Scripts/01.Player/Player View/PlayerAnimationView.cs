using UnityEngine;

public class PlayerAnimationView
{
    private Animator animator;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");

    public void Init(Animator animator)
    {
        this.animator = animator;
    }

    public void SetMoveAnimation(Vector2 moveInput)
    {
        animator.SetBool(IsMoving, moveInput.sqrMagnitude > 0f);

        if (moveInput.sqrMagnitude <= 0f)
            return;

        animator.SetFloat(MoveX, moveInput.x);
        animator.SetFloat(MoveY, moveInput.y);
    }

    public void SetIdleAnimation()
    {
        animator.SetBool(IsMoving, false);
    }
}