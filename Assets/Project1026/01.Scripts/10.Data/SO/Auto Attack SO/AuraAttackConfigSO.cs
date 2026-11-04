using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack_Aura", menuName = "Game/Attack/Aura")]
public class AuraAttackConfigSO : AttackConfigBaseSO
{
    [SerializeField, Min(0f)] private float radius = 2f;
    [SerializeField] private LayerMask enemyLayer;

    public override IAttack CreateAttack(Transform firePoint, PlayerModel model, AttackRuntimeData data, float interval)
    {
        return new AuraAttack(model, data, radius, interval, enemyLayer);
    }

    public override AttackRuntimeData CreateRuntimeData(bool initiallyOwned)
    {
        return new AttackRuntimeData(initiallyOwned);
    }
}
