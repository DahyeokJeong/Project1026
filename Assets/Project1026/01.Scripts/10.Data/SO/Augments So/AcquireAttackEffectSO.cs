using UnityEngine;

[CreateAssetMenu(
    fileName = "SO_Augment_Acquire",
    menuName = "Game/Augment/Acquire Attack")]
public class AcquireAttackEffectSO : AugmentEffectSO
{
    public override bool CanApply(PlayerAutoAttack auto, AugmentData data)
    {
        return data.Value > 0 && auto.TryGetAttackData(data.AttackConfigKey,
            out AttackRuntimeData runtimeData)
            && !runtimeData.IsOwned;
    }

    public override bool TryApply(PlayerAutoAttack auto, AugmentData data)
    {
        return auto.TryAcquireAttack(data.AttackConfigKey, data.Value);
    }
}