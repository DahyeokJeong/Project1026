using TMPro;
using UnityEngine;

public class PlayerExpView
{
    private RectTransform currentExp;
    private TMP_Text expTxt;
    private float maxExp;

    public void Init(RectTransform currentExp, TMP_Text expTxt, float amount)
    {
        this.currentExp = currentExp;
        this.expTxt = expTxt;
        maxExp = amount;

        SetExp(0);
    }

    public void SetExp(float current)
    {
        float ratio = Mathf.Clamp01(current / maxExp);

        currentExp.anchorMax = new Vector2(ratio, 1f);

        expTxt.text = $"{current:F1} / {maxExp}";
    }
}