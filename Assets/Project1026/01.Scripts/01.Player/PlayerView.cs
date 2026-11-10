using TMPro;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    #region Component
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer dashEffect;
    [SerializeField] private Sprite[] dashFrames;
    [SerializeField] private RectTransform currentExp;
    [SerializeField] private TMP_Text expTxt;
    [SerializeField] private TMP_Text levelTxt;
    #endregion

    #region field
    private PlayerAnimationView animationView = new();
    private PlayerExpView expView = new();
    #endregion

    public void InitAnimation()
    {
        animationView.Init(animator, bodyRenderer, dashEffect, dashFrames);
    }

    public void InitExp(float maxExp)
    {
        expView.Init(currentExp, expTxt,levelTxt, maxExp);
    }
    public void SetMoveAnimation(Vector2 moveInput)
    {
        animationView.SetMoveAnimation(moveInput);
    }

    public void SetIdleAnimation()
    {
        animationView.SetIdleAnimation();
    }

    public void SetDashAnimation(Vector2 direction, float progress)
    {
        animationView.SetDashAnimation(direction, progress);
    }

    public void StopDashAnimation()
    {
        animationView.StopDashAnimation();
    }

    private void OnDisable()
    {
        StopDashAnimation();
    }

    public void SetExp(float currentExp, int currentLevel)
    {
        expView.SetExp(currentExp, currentLevel);
    }
}
