public sealed class AuraAttackRuntimeData : AttackRuntimeData
{
    public float RadiusBonus { get; private set; }
    public AuraAttackRuntimeData(bool initiallyOwned) : base(initiallyOwned)
    {
    }

    public bool AddRadius(float percentage)
    {
        if (!IsOwned || percentage <= 0f)
            return false;

        RadiusBonus += percentage * 0.01f;
        return true;
    }
}
