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
    Radius,
    AutoAttackSpeed
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
        if (auto == null || data == null || data.Value <= 0)
            return false;

        return (target, stat) switch
        {
            (AugmentStatTarget.Attack, AugmentStatType.Damage) =>
                auto.TryGetAttackData(data.AttackConfigKey, out AttackRuntimeData attackData)
                && attackData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.ProjectileCount) =>
                auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData projectileData)
                && projectileData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.PierceCount) =>
                auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData piercingData)
                && piercingData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.Radius) =>
                auto.TryGetAttackData(data.AttackConfigKey, out AuraAttackRuntimeData auraData)
                && auraData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.AutoAttackSpeed) =>
                auto.TryGetAttackData(data.AttackConfigKey, out AttackRuntimeData speedData)
                && speedData.IsOwned,

            _ => false
        };
    }

    public override bool TryApply(PlayerAutoAttack auto, AugmentData data)
    {
        if (!CanApply(auto, data))
            return false;

        return (target, stat) switch
        {
            (AugmentStatTarget.Attack, AugmentStatType.Damage) =>
                auto.TryGetAttackData(data.AttackConfigKey, out AttackRuntimeData attackData)
                && attackData.AddDamage(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.ProjectileCount) =>
                auto.AddProjectileCount(data.AttackConfigKey, data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.PierceCount) =>
                auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData piercingData)
                && piercingData.AddPierceCount(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.Radius) =>
                auto.TryGetAttackData(data.AttackConfigKey, out AuraAttackRuntimeData auraData)
                && auraData.AddRadius(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.AutoAttackSpeed) =>
                auto.TryGetAttackData(data.AttackConfigKey, out AttackRuntimeData speedData)
                && speedData.AddAutoAttackSpeed(data.Value),

            _ => false
        };
    }
}