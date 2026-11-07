using UnityEngine;

public enum AugmentStatTarget
{
    Attack,
    Player
}

public enum AugmentStatType
{
    PierceCount,
    ProjectileCount,
    Damage,
    Radius
}

[CreateAssetMenu(
    fileName = "SO_Augment_IncreaseStat",
    menuName = "Game/Augment/Increase Stat")]
public class IncreaseStatEffectSO : AugmentEffectSO
{
    #region Settings
    [SerializeField] private AugmentStatTarget target;
    [SerializeField] private AugmentStatType stat;
    #endregion

    public override bool CanApply(PlayerAutoAttack auto, AugmentData data)
    {
        if (data == null || data.Value <= 0)
            return false;

        return (target, stat) switch
        {
            // 보유 공격의 추가 피해 증가
            (AugmentStatTarget.Attack, AugmentStatType.Damage) => auto != null && auto.TryGetAttackData(data.AttackConfigKey, out AttackRuntimeData attackData)&& attackData.IsOwned,

            // 보유 투사체 공격의 발사 수 증가
            (AugmentStatTarget.Attack, AugmentStatType.ProjectileCount) => auto != null && auto.TryGetAttackData(data.AttackConfigKey,out ProjectileAttackRuntimeData projectileData) && projectileData.IsOwned,

            //보유 투사체 공격의 관통 횟수 증가
            (AugmentStatTarget.Attack, AugmentStatType.PierceCount) => auto != null && auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData projectileData) &&  projectileData.IsOwned,

            //보유 오라 범위 증가
            (AugmentStatTarget.Attack, AugmentStatType.Radius) => auto != null && auto.TryGetAttackData(data.AttackConfigKey, out AuraAttackRuntimeData auraData) && auraData.IsOwned,

            _ => false
        };
    }

    public override bool TryApply(PlayerAutoAttack auto, AugmentData data)
    {
        if (!CanApply(auto, data))
            return false;

        return (target, stat) switch
        {
            (AugmentStatTarget.Attack, AugmentStatType.Damage) => auto.TryGetAttackData(data.AttackConfigKey, out AttackRuntimeData attackData) && attackData.AddDamage(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.ProjectileCount) => auto.AddProjectileCount(data.AttackConfigKey, data.Value), 
            
            (AugmentStatTarget.Attack, AugmentStatType.PierceCount) => auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData attackData) && attackData.AddPierceCount(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.Radius) => auto.TryGetAttackData(data.AttackConfigKey, out AuraAttackRuntimeData auraData) && auraData.AddRadius(data.Value),

            _ => false
        };
    }
}