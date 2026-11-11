using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack_Summon", menuName = "Game/Attack/Summon")]
public class SummonAttackConfigSO : AttackConfigBaseSO
{
    [SerializeField] private GameObject prefab;

    public GameObject Prefab => prefab;

    public override AttackRuntimeData CreateRuntimeData(bool initiallyOwned)
    {
        if (prefab != null && prefab.TryGetComponent(out OrbitSummon _))
            return new ProjectileAttackRuntimeData(initiallyOwned, 1);

        if (prefab != null && prefab.TryGetComponent(out IndependentSummon _))
            return new IndependentSummonRuntimeData(initiallyOwned);

        return new AttackRuntimeData(initiallyOwned);
    }

    public override IAttack CreateAttack(
        Transform firePoint,
        PlayerModel model,
        AttackRuntimeData data,
        float interval)
    {
        if (prefab == null || model == null || data == null)
            return null;

        if (!prefab.TryGetComponent(out SummonBase _))
            return null;

        return new SummonAttack(firePoint, model, this, data, interval);
    }
}