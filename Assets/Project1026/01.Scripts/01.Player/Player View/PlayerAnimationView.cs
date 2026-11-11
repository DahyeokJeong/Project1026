using UnityEngine;

public class PlayerAnimationView
{
    private Animator animator;
    private SpriteRenderer bodyRenderer;
    private Vector3 dashEffectOrigin;
    private SpriteRenderer dashEffect;
    private Sprite[] dashFrames;

    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");

    public void Init(Animator animator, SpriteRenderer bodyRenderer, SpriteRenderer dashEffect, Sprite[] dashFrames)
    {
        this.animator = animator;
        this.bodyRenderer = bodyRenderer;
        this.dashEffect = dashEffect;
        this.dashFrames = dashFrames;

        if (dashEffect != null)
            dashEffectOrigin = dashEffect.transform.localPosition;
        StopDashAnimation();
    }

    public void SetDashAnimation(Vector2 direction, float progress)
    {
        UpdateFacing(direction.x);

        if (dashEffect == null || dashFrames == null || dashFrames.Length == 0)
            return;

        int frame = Mathf.Min(
            Mathf.FloorToInt(Mathf.Clamp01(progress) * dashFrames.Length),
            dashFrames.Length - 1);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        const float trailOffset = 0.35f;
        Vector3 origin = dashEffect.transform.parent.TransformPoint(dashEffectOrigin);

        dashEffect.transform.position = origin - (Vector3)(direction.normalized * trailOffset);
        dashEffect.sprite = dashFrames[frame];
        dashEffect.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        dashEffect.enabled = true;
    }

    public void StopDashAnimation()
    {
        if (dashEffect != null)
            dashEffect.enabled = false;
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