using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text effectText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Image iconImage;

    public void BindAugment(string title, string description, Sprite icon, int count)
    {
        BindAutoAttack(title, icon);
        effectText.text = description;
        countText.text = $"¡¿{count}";
    }

    public void BindAutoAttack(string title, Sprite icon)
    {
        nameText.text = title;
        iconImage.sprite = icon;
        iconImage.enabled = icon != null;
    }
}