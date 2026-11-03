using TMPro;
using UnityEngine;

public class PlayerExpView
{
    private RectTransform currentExp;
    private TMP_Text expTxt;
    private TMP_Text levelTxt;
    private float maxExp;

    public void Init(RectTransform currentExp, TMP_Text expTxt, TMP_Text levelTxt, float amount)
    {
        this.currentExp = currentExp;
        this.expTxt = expTxt;
        this.levelTxt = levelTxt;
        maxExp = amount;

        SetExp(0f, 1);
    }

    public void SetExp(float current, int level)
    {
        float ratio = Mathf.Clamp01(current / maxExp);

        currentExp.anchorMax = new Vector2(ratio, 1f);

        expTxt.text = $"{current:F1} / {maxExp}";
        levelTxt.text = $"Lv.{level}";
    }
}