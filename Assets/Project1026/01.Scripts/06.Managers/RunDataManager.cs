using System;
using System.Collections.Generic;
using UnityEngine;

public class RunDataManager
{
    #region Player
    private PlayerModel model;
    private PlayerView view;
    #endregion

    #region Events
    public event Action<int> LevelUpOccurred;
    #endregion

    #region Attack
    private readonly Dictionary<string, AttackRuntimeData> attackData = new();

    public AttackRuntimeData GetOrCreateAttackData(AttackConfigBaseSO config, bool initiallyOwned)
    {
        if (config == null || string.IsNullOrWhiteSpace(config.ConfigKey))
        {
            Debug.LogError("[RunDataManager] ���� ���� �Ǵ� �ĺ� Ű�� �����ϴ�.");
            return null;
        }

        string key = config.ConfigKey;
        if (attackData.TryGetValue(key, out AttackRuntimeData data))
            return data;

        data = config.CreateRuntimeData(initiallyOwned);
        if (data == null)
        {
            Debug.LogError($"[RunDataManager] ���� ��Ÿ�� ������ ������ �����߽��ϴ�: {key}");
            return null;
        }

        attackData.Add(key, data);
        return data;
    }
    #endregion

    #region Player Stat
    public int PlayerAttackDamageBonus { get; private set; }
    public int PlayerMaxHpBonus { get; private set; }
    public int PlayerHealthRegenBonus { get; private set; }
    public int PlayerDefenseBonus { get; private set; }

    public float PlayerAttackSpeedMultiplier { get; private set; } = 1f;
    public float PlayerMoveSpeedMultiplier { get; private set; } = 1f;
    public float PlayerCriticalBonus { get; private set; }
    public float PlayerAutoAttackCooldownMultiplier { get; private set; } = 1f;

    public bool AddPlayerStat(PlayerAugmentStat stat, int amount)
    {
        if (amount <= 0)
            return false;

        switch (stat)
        {
            case PlayerAugmentStat.MaxHp:
                PlayerMaxHpBonus += amount;
                RefreshPlayerHp(model);
                break;
            case PlayerAugmentStat.HealthRegen:
                PlayerHealthRegenBonus += amount;
                break;
            case PlayerAugmentStat.AttackDamage:
                PlayerAttackDamageBonus += amount;
                break;
            case PlayerAugmentStat.AttackSpeed:
                PlayerAttackSpeedMultiplier *= 1f + amount / 100f;
                break;
            case PlayerAugmentStat.Defense:
                PlayerDefenseBonus += amount;
                break;
            case PlayerAugmentStat.MoveSpeed:
                PlayerMoveSpeedMultiplier *= 1f + amount / 100f;
                break;
            case PlayerAugmentStat.Critical:
                PlayerCriticalBonus += amount;
                break;
            case PlayerAugmentStat.AutoAttackCooldown:
                if (amount >= 100)
                    return false;

                PlayerAutoAttackCooldownMultiplier *= 1f - amount / 100f;
                break;
            default:
                return false;
        }

        return true;
    }

    private void ResetPlayerStats()
    {
        PlayerAttackDamageBonus = 0;
        PlayerMaxHpBonus = 0;
        PlayerHealthRegenBonus = 0;
        PlayerDefenseBonus = 0;

        PlayerAttackSpeedMultiplier = 1f;
        PlayerMoveSpeedMultiplier = 1f;
        PlayerCriticalBonus = 0f;
        PlayerAutoAttackCooldownMultiplier = 1f;
    }
    #endregion

    #region Fields
    private readonly Dictionary<string, int> acquiredAugmentCounts = new();
    private readonly HashSet<string> acquiredAttackKeys = new();
    #endregion

    #region Properties
    public float CurrentExp { get; set; }
    public int Level { get; set; } = 1;
    private float requiredExpIncrease;
    private float queuedRequiredExpIncrease;

    public float GetRequiredExp(float baseExp) => baseExp + requiredExpIncrease;

    public void QueueRequiredExpIncrease(float increase)
    {
        queuedRequiredExpIncrease = increase;
        if (CurrentExp <= 0f)
            ApplyQueuedRequiredExpIncrease();
    }

    public void ApplyQueuedRequiredExpIncrease()
    {
        requiredExpIncrease = queuedRequiredExpIncrease;
    }

    public IReadOnlyDictionary<string, int> AcquiredAugmentCounts => acquiredAugmentCounts;
    public IReadOnlyCollection<string> AcquiredAttackKeys => acquiredAttackKeys;
    #endregion

    public void SetPlayer(PlayerModel model, PlayerView view)
    {
        this.model = model;
        this.view = view;

        RefreshPlayerHp(model);
        view.InitExp();
        view.SetExp(CurrentExp, Level, GetRequiredExp(model.MaxExp));
    }

    public void RefreshPlayerHp(PlayerModel player)
    {
        if (player != null && player == model)
            view?.SetHp(player.CurrentHp, player.MaxHp);
    }

    public void AddExp(float amount)
    {
        if (model == null)
            return;

        int levelUpCount = model.CalculateExp(this, amount);

        view?.SetExp(CurrentExp, Level, GetRequiredExp(model.MaxExp));

        if (levelUpCount > 0)
            OnLevelUp(levelUpCount);
    }

    private void OnLevelUp(int levelUpCount)
    {
        if (levelUpCount <= 0)
            return;

        LevelUpOccurred?.Invoke(levelUpCount);
    }

    public void RecordAugment(string augmentId)
    {
        acquiredAugmentCounts.TryGetValue(augmentId, out int count);
        acquiredAugmentCounts[augmentId] = count + 1;
    }

    public void RecordAcquiredAttack(string attackConfigKey)
    {
        acquiredAttackKeys.Add(attackConfigKey);
    }

    public void Reset()
    {
        CurrentExp = 0f;
        requiredExpIncrease = queuedRequiredExpIncrease = 0f;
        Level = 1;

        ResetPlayerStats();
        attackData.Clear();
        acquiredAugmentCounts.Clear();
        acquiredAttackKeys.Clear();

        LevelUpOccurred = null;
        model = null;
        view = null;
    }
}