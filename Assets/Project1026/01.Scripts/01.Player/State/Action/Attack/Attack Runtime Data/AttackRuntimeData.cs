public class AttackRuntimeData
{
    #region Properties
    public bool IsOwned { get; private set; }
    public int DamageBonus { get; private set; }
    #endregion

    public AttackRuntimeData(bool initiallyOwned)
    {
        IsOwned = initiallyOwned;
    }

    public bool TryAcquire()
    {
        if (IsOwned)
            return false;

        IsOwned = true;
        return true;
    }

    public virtual bool TryAcquire(int value)
    {
        return TryAcquire();
    }

    public bool AddDamage(int amount)
    {
        if (!IsOwned || amount <= 0)
            return false;

        DamageBonus += amount;
        return true;
    }
}