using TMPro;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    #region Component
    [SerializeField] private Animator animator;
    [SerializeField] private RectTransform currentExp;
    [SerializeField] private TMP_Text expTxt;
    [SerializeField] private TMP_Text levelTxt;
    #endregion

    #region Fields
    private PlayerAnimationView animationView = new();
    private PlayerExpView expView = new();
    #endregion

    public void InitAnimation()
    {
        animationView.Init(animator);
    }

    public void InitExp(float maxExp)
    {
        expView.Init(currentExp, expTxt, levelTxt, maxExp);
    }

    public void SetBattleUIVisible(bool visible)
    {
        if (currentExp != null)
            currentExp.gameObject.SetActive(visible);

        if (expTxt != null)
            expTxt.gameObject.SetActive(visible);

        if (levelTxt != null)
            levelTxt.gameObject.SetActive(visible);
    }

    public void SetMoveAnimation(Vector2 moveInput)
    {
        animationView.SetMoveAnimation(moveInput);
    }

    public void SetIdleAnimation()
    {
        animationView.SetIdleAnimation();
    }

    public void SetExp(float currentExp, int currentLevel)
    {
        expView.SetExp(currentExp, currentLevel);
    }
}
