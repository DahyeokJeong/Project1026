using UnityEngine;

public enum AugmentStatTarget
{
    Attack,
    Player
}

public enum AugmentStatType
{
    ProjectileCount,
    Damage
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
        if (data == null || data.Value <= 0)
            return false;

        return (target, stat) switch
        {
            // 플레이어 공격력 증가
            (AugmentStatTarget.Player, AugmentStatType.Damage) =>
                string.IsNullOrWhiteSpace(data.AttackConfigKey),

            // 보유 공격의 추가 피해 증가
            (AugmentStatTarget.Attack, AugmentStatType.Damage) => auto != null && auto.TryGetAttackData(data.AttackConfigKey, out AttackRuntimeData attackData)&& attackData.IsOwned,

            // 보유 투사체 공격의 발사 수 증가
            (AugmentStatTarget.Attack, AugmentStatType.ProjectileCount) => auto != null && auto.TryGetAttackData(data.AttackConfigKey,out ProjectileAttackRuntimeData projectileData) && projectileData.IsOwned,

            _ => false
        };
    }

    public override bool TryApply(PlayerAutoAttack auto, AugmentData data)
    {
        if (!CanApply(auto, data))
            return false;

        return (target, stat) switch
        {
            (AugmentStatTarget.Player, AugmentStatType.Damage) =>
                Managers.RunData.AddPlayerAttackDamage(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.Damage) => auto.TryGetAttackData(data.AttackConfigKey, out AttackRuntimeData attackData) && attackData.AddDamage(data.Value),

            (AugmentStatTarget.Attack, AugmentStatType.ProjectileCount) => auto.AddProjectileCount(data.AttackConfigKey, data.Value), _ => false
        };
    }
}