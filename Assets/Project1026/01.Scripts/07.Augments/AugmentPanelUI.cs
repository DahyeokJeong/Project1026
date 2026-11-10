using System;
using System.Collections.Generic;
using UnityEngine;

public class AugmentPanelUI : MonoBehaviour
{
    #region References
    [Header("Regerences")]
    [SerializeField] private GameObject panel;
    [SerializeField] private PlayerAugment playerAugment;
    [SerializeField] private AugmentCardUI[] cards;
    #endregion

    #region Fields
    private readonly List<AugmentData> currentChoices = new();
    private const int BonusRerollsPerLevel = 1;
    private readonly int[] rerollsRemaining = new int[3];
    private int choiceCount;
    #endregion

    public void Show(int count)
    {
        if (count < 0)
            return;

        choiceCount += count;

        Debug.Log(choiceCount);
        if (panel == null || panel.activeSelf)
            return;

        if (playerAugment == null || cards == null || cards.Length != 3)
        {
            Debug.LogError("[AugmentPanelUI] PlayerAugment와 카드 3개를 연결하세요.", this);
            return;
        }

        if (!playerAugment.TryGetChoice(cards.Length, out List<AugmentData> choices))
        {
            return;
        }

        currentChoices.Clear();
        currentChoices.AddRange(choices);

        int rerollsPerCard = 1 + Managers.GameState.GetSoulBookLevel(SoulBookNode.AugmentRerolls) * BonusRerollsPerLevel;
        for(int i = 0; i < cards.Length; ++i)
        {
            rerollsRemaining[i] = rerollsPerCard;

            cards[i].SetData(currentChoices[i], OnCardSelected, OnCardReroll);

            cards[i].SetRerollInteractable(true);
        }

        --choiceCount;
        panel.SetActive(true);
    }

    private void OnCardSelected(string augmentId)
    {
        if (!currentChoices.Exists(data => data.Id == augmentId))
            return;

        if (!playerAugment.TryApply(augmentId))
        {
            Debug.LogWarning($"[AugmentPanelUI] 증강 적용 실패: {augmentId}", this);
            return;
        }

        Debug.Log($"[AugmentPanelUI] 증강 적용 성공: {augmentId}", this);

        panel.SetActive(false);
        if (choiceCount > 0)
        {
            Show(0);
            return;
        }

        Managers.BattleState.SetState(BattleState.Playing);
    }

    private void OnCardReroll(string augmentId)
    {
        Debug.Log(
        $"[AugmentPanelUI] 리롤 처리 시작: {augmentId}",
        this);

        int index = currentChoices.FindIndex(data => data.Id == augmentId);

        if (index < 0 || rerollsRemaining[index] <= 0)
            return;

        HashSet<string> excludedIds = new();

        foreach(AugmentData data in currentChoices)
            excludedIds.Add(data.Id);

        List<AugmentData> candidates = new();

        foreach(AugmentData data in Managers.Data.AugmentDic.Values)
        {
            if (data == null || excludedIds.Contains(data.Id))
                continue;

            candidates.Add(data);
        }

        Debug.Log($"[AugmentPanelUI] 리롤 요청" + $" / 현재 카드: {augmentId}" + $" / 제외 후 후보: {candidates.Count}",
    this);

        if (!AugmentSelector.TrySelect(candidates, playerAugment.CanApply, 1, out List<AugmentData> choices))
        {
            Debug.LogWarning("[AugmentPanelUI] 리롤 가능한 증강이 없습니다.", this);
            return;
        }

        AugmentData newData = choices[0];

        currentChoices[index] = newData;

        cards[index].SetData(newData, OnCardSelected, OnCardReroll);
        rerollsRemaining[index]--;
        cards[index].SetRerollInteractable(rerollsRemaining[index] > 0);
        Debug.Log($"[AugmentPanelUI] 카드 {index + 1} 리롤 완료: {augmentId} → {newData.Id}", this);
    }
}
