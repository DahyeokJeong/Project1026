using System;

public sealed class ProjectileAttackRuntimeData : AttackRuntimeData
{
    #region Properties
    public int ProjectileCount { get; private set; }
    public int ProjectileSpeedBonusPercent { get; private set; }
    public int ProjectileSizeBonusPercent { get; private set; }
    public int ExplosionRadiusBonusPercent { get; private set; }
    public int PierceBonus { get; private set; }
    public int BounceCount { get; private set; }
    public bool HomingEnabled { get; private set; }
    #endregion

    public ProjectileAttackRuntimeData(bool initiallyOwned,int initialProjectileCount) : base(initiallyOwned)
    {
        ProjectileCount = Math.Max(1, initialProjectileCount);
    }

    #region TryAugment
    public override bool TryAcquire(int initialProjectileCount)
    {
        if (initialProjectileCount <= 0 || !base.TryAcquire())
            return false;

        ProjectileCount = initialProjectileCount;
        return true;
    }

    public bool TryEnableHoming()
    {
        if (!IsOwned || HomingEnabled)
            return false;

        HomingEnabled = true;
        return true;
    }
    #endregion

    #region AddAugment
    public bool AddProjectileCount(int amount)
    {
        if (!IsOwned || amount <= 0)
            return false;

        ProjectileCount += amount;
        return true;
    }

    public bool AddProjectileSpeed(int amount)
    {
        if (!IsOwned || amount <= 0)
            return false;

        ProjectileSpeedBonusPercent += amount;
        return true;
    }

    public bool AddProjectileSize(int amount)
    {
        if (!IsOwned || amount <= 0)
            return false;

        ProjectileSizeBonusPercent += amount;
        return true;
    }

    public bool AddPierceCount(int amount)
    {
        if (!IsOwned || amount <= 0)
            return false;

        PierceBonus += amount;
        return true;
    }

    public bool AddExplosionRadius(int amount)
    {
        if (!IsOwned || amount <= 0)
            return false;

        ExplosionRadiusBonusPercent += amount;
        return true;
    }

    public bool AddBounceCount(int amount)
    {
        if (!IsOwned || amount <= 0)
            return false;

        BounceCount += amount;
        return true;
    }
    #endregion
}