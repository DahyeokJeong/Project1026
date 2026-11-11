public sealed class IndependentSummonRuntimeData : AttackRuntimeData
{
    public int SummonCount { get; private set; } = 1;
    public float DurationBonus { get; private set; }

    public IndependentSummonRuntimeData(bool initiallyOwned) : base(initiallyOwned)
    {
    }

    public bool AddSummonCount(int amount)
    {
        if (!IsOwned || amount <= 0)
            return false;

        SummonCount += amount;
        return true;
    }

    public bool AddDuration(int percentage)
    {
        if (!IsOwned || percentage <= 0)
            return false;

        DurationBonus += percentage * 0.01f;
        return true;
    }
}
