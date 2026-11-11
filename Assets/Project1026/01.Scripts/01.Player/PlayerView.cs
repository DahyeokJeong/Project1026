using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    #region Component
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField] private SpriteRenderer dashEffect;
    [SerializeField] private Sprite[] dashFrames;
    [SerializeField] private Image currentHp;
    [SerializeField] private RectTransform currentExp;
    [SerializeField] private TMP_Text expTxt;
    [SerializeField] private TMP_Text levelTxt;
    [SerializeField, Min(0.01f)] private float expFillSmoothTime = 0.18f;
    #endregion

    #region Fields
    private PlayerAnimationView animationView = new();
    private PlayerExpView expView = new();
    #endregion

    public void InitAnimation()
    {
        animationView.Init(animator, bodyRenderer, dashEffect, dashFrames);
    }

    public void InitExp(float maxExp)
    {
        expView.Init(currentExp, expTxt, levelTxt, maxExp, expFillSmoothTime);
    }

    private void Update()
    {
        expView.Tick(Time.unscaledDeltaTime);
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

    public void SetHp(float current, float maximum)
    {
        if (currentHp != null)
            currentHp.fillAmount = maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f;
    }

    public void SetExp(float currentExp, int currentLevel)
    {
        expView.SetExp(currentExp, currentLevel);
    }
}
