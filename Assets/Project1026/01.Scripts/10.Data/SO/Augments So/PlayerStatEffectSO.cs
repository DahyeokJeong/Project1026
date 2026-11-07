using UnityEngine;

public enum PlayerAugmentStat
{
    MaxHp = 0,
    HealthRegen = 1,
    AttackDamage = 2,
    AttackSpeed = 3,
    Defense = 4,
    MoveSpeed = 5,
    Critical = 6,
    AutoAttackCooldown = 7
}

[CreateAssetMenu(fileName = "SO_Augment_PlayerStat", menuName = "Game/Augment/Player Stat")]
public class PlayerStatEffectSO : AugmentEffectSO
{
    [SerializeField] private PlayerAugmentStat stat;

    public override bool CanApply(PlayerAutoAttack auto, AugmentData data)
    {
        if (data == null ||
            data.Value <= 0 ||
            !string.IsNullOrWhiteSpace(data.AttackConfigKey))
        {
            return false;
        }

        switch (stat)
        {
            case PlayerAugmentStat.MaxHp:
            case PlayerAugmentStat.HealthRegen:
            case PlayerAugmentStat.AttackDamage:
            case PlayerAugmentStat.AttackSpeed:
            case PlayerAugmentStat.Defense:
            case PlayerAugmentStat.MoveSpeed:
            case PlayerAugmentStat.Critical:
                return true;

            case PlayerAugmentStat.AutoAttackCooldown:
                return data.Value < 100;

            default:
                return false;
        }
    }

    public override bool TryApply(PlayerAutoAttack auto, AugmentData data)
    {
        return CanApply(auto, data) &&
               Managers.RunData.AddPlayerStat(stat, data.Value);
    }
}