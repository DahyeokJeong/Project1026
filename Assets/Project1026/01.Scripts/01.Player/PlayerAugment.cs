using System.Collections.Generic;
using UnityEngine;

public class PlayerAugment : MonoBehaviour
{
    #region References
    [SerializeField] private PlayerAutoAttack auto;
    [SerializeField] private List<AugmentEffectSO> effectConfigs = new();
    #endregion

    #region Fields
    private Dictionary<string, AugmentEffectSO> effects;
    #endregion

    private void Awake()
    {
        if (auto == null || effectConfigs.Count == 0)
        {
            Debug.LogError("[PlayerAugment] Auto와 효과 목록을 연결하세요.", this);
            return;
        }

        Dictionary<string, AugmentEffectSO> dictionary = new();

        foreach (AugmentEffectSO effect in effectConfigs)
        {
            if (effect == null || string.IsNullOrWhiteSpace(effect.ConfigKey))
            {
                Debug.LogError("[PlayerAugment] 효과 SO 또는 ConfigKey가 없습니다.", this);
                return;
            }

            if (!dictionary.TryAdd(effect.ConfigKey, effect))
            {
                Debug.LogError($"[PlayerAugment] 중복된 ConfigKey: {effect.ConfigKey}", this);
                return;
            }
        }

        effects = dictionary;
    }

    public bool CanApply(string augmentId)
    {
        return TryResolve(augmentId, out AugmentData data, out AugmentEffectSO effect) && effect.CanApply(auto, data);
    }

    public bool TryApply(string augmentId)
    {
        if (!TryResolve(augmentId, out AugmentData data, out AugmentEffectSO effect))
        {
            Debug.LogWarning($"[PlayerAugment] 증강 데이터 또는 효과 연결을 확인하세요: {augmentId}", this);
            return false;
        }

        return effect.CanApply(auto, data) && effect.TryApply(auto, data);
    }

    // 기존 Apply 호출을 유지하기 위한 진입점
    public void Apply(string augmentId)
    {
        TryApply(augmentId);
    }

    private bool TryResolve(string augmentId, out AugmentData data, out AugmentEffectSO effect)
    {
        data = null;
        effect = null;

        if (effects == null || !Managers.Data.IsLoaded || string.IsNullOrWhiteSpace(augmentId))
            return false;

        data = AugmentHelper.Get(augmentId);

        if (data == null || string.IsNullOrWhiteSpace(data.ConfigKey))
            return false;

        return effects.TryGetValue(data.ConfigKey, out effect);
    }

    public bool TryGetChoice(int count, out List<AugmentData> choices)
    {
        choices = new List<AugmentData>();

        if (effects == null || !Managers.Data.IsLoaded)
            return false;

        return AugmentSelector.TrySelect(Managers.Data.AugmentDic.Values, CanApply, count, out choices);
    }

}