using UnityEngine;
using System.Collections.Generic;

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

    public bool AddPlayerAttackDamage(int amount)
    {
        if (amount <= 0)
            return false;

        PlayerAttackDamageBonus += amount;
        return true;
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
        // 추후 증강 시스템 연결
    }

    public void Reset()
    {
        CurrentExp = 0f;
        Level = 1;

        PlayerAttackDamageBonus = 0;
        attackData.Clear();

        model = null;
        view = null;
    }
}