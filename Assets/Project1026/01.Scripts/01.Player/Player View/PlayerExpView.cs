using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExpView
{
    private RectTransform currentExp;
    private Image fillImage;
    private TMP_Text expTxt;
    private TMP_Text levelTxt;
    private float maxExp;
    private float displayedProgress;
    private float targetProgress;
    private float fillSmoothTime;
    private float fillVelocity;
    private bool hasTarget;

    public void Init(RectTransform currentExp, TMP_Text expTxt, TMP_Text levelTxt, float amount,
        float smoothTime)
    {
        this.currentExp = currentExp;
        fillImage = currentExp != null ? currentExp.GetComponent<Image>() : null;
        this.expTxt = expTxt;
        this.levelTxt = levelTxt;
        maxExp = amount;

        fillSmoothTime = smoothTime;
        fillVelocity = 0f;
        displayedProgress = 0f;
        targetProgress = 0f;
        hasTarget = false;
        RefreshDisplay();
    }

    public void SetExp(float current, int level)
    {
        float ratio = maxExp > 0f ? Mathf.Clamp01(current / maxExp) : 0f;
        targetProgress = level - 1 + ratio;

        if (!hasTarget || targetProgress < displayedProgress)
        {
            hasTarget = true;
            displayedProgress = targetProgress;
            fillVelocity = 0f;
            RefreshDisplay();
        }
    }

    public void Tick(float deltaTime)
    {
        if (displayedProgress == targetProgress || deltaTime <= 0f)
            return;

        displayedProgress = Mathf.SmoothDamp(displayedProgress, targetProgress, ref fillVelocity,
            fillSmoothTime, Mathf.Infinity, deltaTime);

        if (Mathf.Abs(targetProgress - displayedProgress) < 0.0001f)
        {
            displayedProgress = targetProgress;
            fillVelocity = 0f;
        }

        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        int completedLevels = Mathf.FloorToInt(displayedProgress);
        float ratio = displayedProgress - completedLevels;

        if (fillImage != null && fillImage.type == Image.Type.Filled)
            fillImage.fillAmount = ratio;
        else if (currentExp != null)
            currentExp.anchorMax = new Vector2(ratio, 1f);

        if (expTxt != null)
            expTxt.text = $"{ratio * maxExp:F1} / {maxExp}";
        if (levelTxt != null)
            levelTxt.text = $"Lv.{completedLevels + 1}";
    }
}
