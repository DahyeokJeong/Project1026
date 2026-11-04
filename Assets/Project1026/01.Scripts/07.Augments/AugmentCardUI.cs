using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AugmentCardUI : MonoBehaviour
{
    #region References
    [Header("UI")]
    [SerializeField] private Button cardButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descripText;
    #endregion

    #region Fields
    private AugmentData currentData;
    private Action<String> onSelected;
    private Action<string> onReroll;
    #endregion

    private void Awake()
    {
        Debug.Log($"[AugmentCardUI] Awake ½ÇÇà: {name}", this);

        if (cardButton == null || rerollButton == null)
            return;

        cardButton.onClick.AddListener(OnCardClicked);
        rerollButton.onClick.AddListener(OnRerollClicked);

    }

    private void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;
    }

    private void OnDestroy()
    {
        cardButton.onClick.RemoveListener(OnCardClicked);
        rerollButton.onClick.RemoveListener(OnRerollClicked);
    }

    public void SetData(AugmentData data, Action<string> onSelected, Action<string> onReroll)
    {
        currentData = data;

        this.onSelected = onSelected;
        this.onReroll = onReroll;

        nameText.text = LocalizationHelper.GetName(data.Id);
        descripText.text = LocalizationHelper.GetDescription(data.Id);
    }

    private void OnCardClicked()
    {
        if (currentData == null)
            return;
        onSelected?.Invoke(currentData.Id);
    }

    private void OnRerollClicked()
    {

        if (currentData == null)
            return;

        onReroll?.Invoke(currentData.Id);
    }

    public void SetRerollInteractable(bool interactable)
    {
        rerollButton.interactable = interactable;
    }
}
