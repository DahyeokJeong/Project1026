using System;
using System.Collections.Generic;

public class GameStateManager
{
    public const int SoulBookLevelCost = 10;
    public const int SoulBookMaxLevel = 5;
    private const int SoulEssenceBonusPercentPerLevel = 5;

    #region Events
    public event Action ProgressChanged;
    #endregion

    #region Fields
#if UNITY_EDITOR
    private const int StartingSoulEssence = 100; //디버깅 영혼의 정수 생성
#else
    private const int StartingSoulEssence = 0;
#endif

    private readonly HashSet<int> clearedStages = new();
    private readonly HashSet<string> completedEvents = new();
    private readonly int[] soulBookLevels = new int[(int)SoulBookNode.AreaAttack + 1];
    #endregion

    #region Properties
    public int SelectedStage { get; private set; }
    public int SoulEssence { get; private set; } = StartingSoulEssence;
    #endregion

    public bool IsStageCleared(int stageId)
    {
        return clearedStages.Contains(stageId);
    }

    public bool IsStageUnlocked(int stageId)
    {
        if (stageId < 1 || stageId > 3)
            return false;

        if (stageId == 1)
            return true;

        return IsStageCleared(stageId - 1);
    }

    public bool SelectStage(int stageId)
    {
        if (!IsStageUnlocked(stageId))
            return false;

        SelectedStage = stageId;
        return true;
    }

    public void ClearStage(int stageId)
    {
        RecordStageClear(stageId, 0);
    }

    public void CompleteBattleStage(int stageId)
    {
        const int soulEssencePerStage = 10;
        int bonusPercent = GetSoulBookLevel(SoulBookNode.SoulEssenceGain) * SoulEssenceBonusPercentPerLevel;
        int reward = (stageId * soulEssencePerStage * (100 + bonusPercent) + 50) / 100;
        RecordStageClear(stageId, reward);
    }

    public int GetSoulBookLevel(SoulBookNode node)
    {
        return (uint)node < (uint)soulBookLevels.Length ? soulBookLevels[(int)node] : 0;
    }

    public bool IsSoulBookNodeImplemented(SoulBookNode node)
    {
        return (uint)node < (uint)soulBookLevels.Length &&
            node != SoulBookNode.ActiveSkillCooldown && node != SoulBookNode.Clone && node != SoulBookNode.AreaAttack;
    }

    public bool IsSoulBookNodeUnlocked(SoulBookNode node)
    {
        return node switch
        {
            SoulBookNode.CriticalChance or SoulBookNode.AttackSpeed => GetSoulBookLevel(SoulBookNode.AttackDamage) > 0,
            SoulBookNode.AutoAttackCooldown or SoulBookNode.ActiveSkillCooldown => GetSoulBookLevel(SoulBookNode.CriticalChance) > 0,
            SoulBookNode.AugmentRerolls or SoulBookNode.MoveSpeed => GetSoulBookLevel(SoulBookNode.HealthRegen) > 0,
            SoulBookNode.ExperienceGain or SoulBookNode.SoulEssenceGain => GetSoulBookLevel(SoulBookNode.AugmentRerolls) > 0,
            _ => true
        };
    }

    public bool CanUpgradeSoulBookNode(SoulBookNode node)
    {
        return IsSoulBookNodeImplemented(node) &&
            IsSoulBookNodeUnlocked(node) &&
            GetSoulBookLevel(node) < SoulBookMaxLevel &&
            SoulEssence >= SoulBookLevelCost;
    }

    public bool TryUpgradeSoulBookNode(SoulBookNode node)
    {
        if (!CanUpgradeSoulBookNode(node))
            return false;

        SoulEssence -= SoulBookLevelCost;
        soulBookLevels[(int)node]++;
        ProgressChanged?.Invoke();
        return true;
    }

    private void RecordStageClear(int stageId, int soulEssenceReward)
    {
        if (stageId < 1 || stageId > 3)
            return;

        bool firstClear = clearedStages.Add(stageId);
        if (!firstClear && soulEssenceReward == 0)
            return;

        SoulEssence += soulEssenceReward;
        ProgressChanged?.Invoke();
    }

    public bool IsEventCompleted(string eventId)
    {
        return completedEvents.Contains(eventId);
    }

    public void CompleteEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
            return;

        if (!completedEvents.Add(eventId))
            return;

        ProgressChanged?.Invoke();
    }

    public SaveData GetSaveData()
    {
        return new SaveData
        {
            clearedStages = new List<int>(clearedStages),
            completedEvents = new List<string>(completedEvents),
            soulEssence = SoulEssence,
            soulBookLevels = new List<int>(soulBookLevels)
        };
    }

    public void LoadSaveData(SaveData saveData)
    {
        if (saveData == null)
            return;

        clearedStages.Clear();
        completedEvents.Clear();

        if(saveData.clearedStages != null)
            clearedStages.UnionWith(saveData.clearedStages);

        if(saveData.completedEvents != null)
            completedEvents.UnionWith(saveData.completedEvents);

        SoulEssence = saveData.soulEssence;
        Array.Clear(soulBookLevels, 0, soulBookLevels.Length);
        if (saveData.soulBookLevels != null)
        {
            int count = Math.Min(soulBookLevels.Length, saveData.soulBookLevels.Count);
            for (int i = 0; i < count; i++)
                soulBookLevels[i] = Math.Clamp(saveData.soulBookLevels[i], 0, SoulBookMaxLevel);
        }

        SelectedStage = 0;

        ProgressChanged?.Invoke();
    }

    public void Clear()
    {
        clearedStages.Clear();
        completedEvents.Clear();
        SoulEssence = StartingSoulEssence;
        Array.Clear(soulBookLevels, 0, soulBookLevels.Length);
        SelectedStage = 0;
    }

    public void StartNewGame()
    {
        Clear();
        ProgressChanged?.Invoke();
    }
}
