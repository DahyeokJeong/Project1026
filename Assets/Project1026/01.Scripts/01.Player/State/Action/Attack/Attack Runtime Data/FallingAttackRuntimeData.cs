using UnityEngine;

public sealed class FallingAttackRuntimeData : AttackRuntimeData
{
    #region Properties
    public int FallingCount { get; private set; }
    #endregion


    public FallingAttackRuntimeData(bool initiallyOwned, int initialCount) : base(initiallyOwned)
    {
        FallingCount = Mathf.Max(1, initialCount);
    }

    public override bool TryAcquire(int initialCount)
    {
        if(initialCount <= 0 || !base.TryAcquire())
            return false;

        FallingCount = initialCount;
        return true;
    }

    public bool AddFallingCount(int amount)
    {
        if (!IsOwned || amount <= 0)
            return false;

        FallingCount += amount;
        return true;
    }
}
