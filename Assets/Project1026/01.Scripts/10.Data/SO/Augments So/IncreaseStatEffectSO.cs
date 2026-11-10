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
    AutoAttackSpeed,
    FallingCount,
    ProjectileSpeed,
    ProjectileSize,
    ExplosionRadius,
    WaveRadius,
    FallingSize,
    SummonDuration,
    SummonCount
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

            (AugmentStatTarget.Attack, AugmentStatType.FallingCount) =>
                auto.TryGetAttackData(data.AttackConfigKey, out FallingAttackRuntimeData fallingData)
                && fallingData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.ProjectileSpeed) =>
                auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData projectileSpeedData)
                && projectileSpeedData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.ProjectileSize) =>
                auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData sizeData)
                && sizeData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.ExplosionRadius) =>
                auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData explosionData)
                && explosionData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.WaveRadius) =>
                auto.TryGetAttackData(data.AttackConfigKey, out WaveAttackRuntimeData waveData)
                && waveData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.SummonCount) =>
                auto.TryGetAttackData(data.AttackConfigKey, out IndependentSummonRuntimeData countData)
                && countData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.SummonDuration) =>
                auto.TryGetAttackData(data.AttackConfigKey, out IndependentSummonRuntimeData durationData)
                && durationData.IsOwned,

            (AugmentStatTarget.Attack, AugmentStatType.FallingSize) =>
                auto.TryGetAttackData(data.AttackConfigKey, out FallingAttackRuntimeData fallingSizeData)
                && fallingSizeData.IsOwned,

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

            (AugmentStatTarget.Attack, AugmentStatType.FallingCount) =>
                auto.TryGetAttackData(data.AttackConfigKey, out FallingAttackRuntimeData fallingData)
                && fallingData.AddFallingCount(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.ProjectileSpeed) =>
                auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData projectileSpeedData)
                && projectileSpeedData.AddProjectileSpeed(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.ProjectileSize) =>
                auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData sizeData)
                && sizeData.AddProjectileSize(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.ExplosionRadius) =>
                auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData explosionData)
                && explosionData.AddExplosionRadius(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.WaveRadius) =>
                auto.TryGetAttackData(data.AttackConfigKey, out WaveAttackRuntimeData waveData)
                && waveData.AddRadius(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.SummonCount) =>
                auto.TryGetAttackData(data.AttackConfigKey, out IndependentSummonRuntimeData countData)
                && countData.AddSummonCount(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.SummonDuration) =>
                auto.TryGetAttackData(data.AttackConfigKey, out IndependentSummonRuntimeData durationData)
                && durationData.AddDuration(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.FallingSize) =>
                auto.TryGetAttackData(data.AttackConfigKey, out FallingAttackRuntimeData fallingSizeData)
                && fallingSizeData.AddSize(data.Value),

            _ => false
        };
    }
}