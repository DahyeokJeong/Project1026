using UnityEngine;

[CreateAssetMenu(
    fileName = "SO_Augment_FireballHoming",
    menuName = "Game/Augment/Enable Fireball Homing")]
public class EnableFireballHomingEffectSO : AugmentEffectSO
{
    #region Settings
    private const string FireballAttackKey = "SO_Attack_Explosion";
    #endregion

    public override bool CanApply(PlayerAutoAttack auto, AugmentData data)
    {
        if (auto == null || data == null)
            return false;

        return data.AttackConfigKey == FireballAttackKey
            && auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData runtimeData)
            && runtimeData.IsOwned
            && !runtimeData.HomingEnabled;
    }

    public override bool TryApply(PlayerAutoAttack auto, AugmentData data)
    {
        if (!CanApply(auto, data))
            return false;

        return auto.TryGetAttackData(data.AttackConfigKey, out ProjectileAttackRuntimeData runtimeData)
            && runtimeData.TryEnableHoming();
    }
}