using System;

public sealed class ProjectileAttackRuntimeData : AttackRuntimeData
{
    #region Properties
    public int ProjectileCount { get; private set; }
    #endregion

    public ProjectileAttackRuntimeData(
        bool initiallyOwned,
        int initialProjectileCount)
        : base(initiallyOwned)
    {
        ProjectileCount = Math.Max(1, initialProjectileCount);
    }

    public bool TryAcquire(int initialProjectileCount)
    {
        if (initialProjectileCount <= 0 || !base.TryAcquire())
            return false;

        ProjectileCount = initialProjectileCount;
        return true;
    }

    public bool AddProjectileCount(int amount)
    {
        if (!IsOwned || amount <= 0)
            return false;

        ProjectileCount += amount;
        return true;
    }
}