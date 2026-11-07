using UnityEngine;
using System.Collections.Generic;
using System;

public class RunDataManager
{
    #region Player
    private PlayerModel model;
    private PlayerView view;
    #endregion

    #region Properties
    public float CurrentExp { get; set; }
    public int Level { get; set; } = 1;
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
            Debug.LogError("[RunDataManager] 공격 설정 또는 식별 키가 없습니다.");
            return null;
        }

        string key = config.ConfigKey;

        if (attackData.TryGetValue(key, out AttackRuntimeData data))
            return data;

        data = config.CreateRuntimeData(initiallyOwned);

        if (data == null)
        {
            Debug.LogError($"[RunDataManager] 공격 런타임 데이터 생성에 실패했습니다: {key}");
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

    public bool AddPlayerAttackDamage(int amount)
    {
        return AddPlayerStat(PlayerAugmentStat.AttackDamage, amount);
    }

    public bool AddPlayerStat(PlayerAugmentStat stat, int amount)
    {
        if(amount <= 0)
            return false;

        switch (stat)
        {
            case PlayerAugmentStat.MaxHp:
                PlayerMaxHpBonus += amount;
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

    public void SetPlayer(PlayerModel model, PlayerView view)
    {
        this.model = model;
        this.view = view;

        view.InitExp(model.MaxExp);
        view.SetExp(CurrentExp, Level);
    }

    public void AddExp(float amount)
    {
        if (model == null)
            return;

        int levelUpCount = model.CalculateExp(this, amount);

        view?.SetExp(CurrentExp, Level);

        if (levelUpCount > 0)
            OnLevelUp(levelUpCount);
    }

    private void OnLevelUp(int levelUpCount)
    {
        if (levelUpCount <= 0)
            return;

        LevelUpOccurred?.Invoke(levelUpCount);
    }

    public void Reset()
    {
        CurrentExp = 0f;
        Level = 1;

        ResetPlayerStats();
        attackData.Clear();

        LevelUpOccurred = null;
        model = null;
        view = null;
    }
}