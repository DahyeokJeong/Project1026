using System.Collections.Generic;
using UnityEngine;

public class PlayerAugment : MonoBehaviour
{
    #region References
    [SerializeField] private PlayerAutoAttack auto;
    #endregion

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

    public void Apply(string augmentId)
    {
        TryApply(augmentId);
    }

    private bool TryResolve(string augmentId, out AugmentData data, out AugmentEffectSO effect)
    {
        data = null;
        effect = null;

        if (auto == null || !Managers.Data.IsLoaded || string.IsNullOrWhiteSpace(augmentId))
            return false;

        data = AugmentHelper.Get(augmentId);

        return data != null &&
               Managers.Data.TryGetAugmentEffect(data.ConfigKey, out effect);
    }

    public bool TryGetChoice(int count, out List<AugmentData> choices)
    {
        choices = new List<AugmentData>();

        if (auto == null || !Managers.Data.IsLoaded)
            return false;

        return AugmentSelector.TrySelect(Managers.Data.AugmentDic.Values, CanApply, count, out choices);
    }

}