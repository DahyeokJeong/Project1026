using TMPro;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    #region Component
    [SerializeField] private Animator animator;
    [SerializeField] private RectTransform currentExp;
    [SerializeField] private TMP_Text expTxt;
    #endregion

    #region field
    private PlayerAnimationView animationView = new();
    private PlayerExpView expView = new();
    #endregion

    public void Init(PlayerModel model)
    {
        animationView.Init(animator);
        expView.Init(currentExp, expTxt, model.MaxExp);
    }
    public void SetMoveAnimation(Vector2 moveInput)
    {
        animationView.SetMoveAnimation(moveInput);
    }

    public void SetIdleAnimation()
    {
        animationView.SetIdleAnimation();
    }

    public void SetExp(float currentExp)
    {
        expView.SetExp(currentExp);
    }
}
