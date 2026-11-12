using TMPro;
using UnityEngine;

public class AutoAttackDescriptionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text descriptionText;

    public void Show(string title, string summary, string behavior, string baseStats)
    {
        descriptionText.text =
            $"<size=32><b>{title}</b></size>\n" +
            $"{summary}\n\n" +
            $"<b>동작 설명</b>\n{behavior}\n\n" +
            $"<b>기본 성능</b>\n{baseStats}";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}