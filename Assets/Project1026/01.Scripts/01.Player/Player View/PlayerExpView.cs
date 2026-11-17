using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerExpView
{
    private RectTransform currentExp;
    private Image fillImage;
    private TMP_Text levelTxt;
    private float displayedProgress;
    private float targetProgress;
    private float fillSmoothTime;
    private float fillVelocity;
    private bool hasTarget;

    public void Init(RectTransform currentExp, TMP_Text levelTxt, float smoothTime)
    {
        this.currentExp = currentExp;
        fillImage = currentExp != null ? currentExp.GetComponent<Image>() : null;
        this.levelTxt = levelTxt;

        fillSmoothTime = smoothTime;
        fillVelocity = 0f;
        displayedProgress = 0f;
        targetProgress = 0f;
        hasTarget = false;
        RefreshDisplay();
    }

    public void SetExp(float current, int level, float maxExp)
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

        if (levelTxt != null)
            levelTxt.text = $"Lv.{completedLevels + 1}";
    }
}
