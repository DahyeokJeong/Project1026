using UnityEngine;

[CreateAssetMenu(
    fileName = "SO_Augment_AcquireProjectile",
    menuName = "Game/Augment/Acquire Projectile Attack")]
public class AcquireProjectileAttackEffectSO : AugmentEffectSO
{
    public override bool CanApply(
        PlayerAutoAttack auto,
        AugmentData data)
    {
        return data.Value > 0
            && auto.TryGetAttackData(
                data.AttackConfigKey,
                out ProjectileAttackRuntimeData runtimeData)
            && !runtimeData.IsOwned;
    }

    public override bool TryApply(
        PlayerAutoAttack auto,
        AugmentData data)
    {
        return auto.TryGetAttackData(
                data.AttackConfigKey,
                out ProjectileAttackRuntimeData runtimeData)
            && runtimeData.TryAcquire(data.Value);
    }
}