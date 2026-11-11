using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack_Aura", menuName = "Game/Attack/Aura")]
public class AuraAttackConfigSO : AttackConfigBaseSO
{
    [SerializeField] private GameObject prefab;

    public GameObject Prefab => prefab;

    public override AttackRuntimeData CreateRuntimeData(bool initiallyOwned)
    {
        return new AuraAttackRuntimeData(initiallyOwned);
    }

    public override IAttack CreateAttack(Transform firePoint, PlayerModel model, AttackRuntimeData data, float interval)
    {
        if (prefab == null || firePoint == null || model == null)
        {
            Debug.LogError($"[AuraAttackConfigSO] 필수 참조가 비어 있습니다: {name}", this);
            return null;
        }

        if (!prefab.TryGetComponent(out AuraBase _))
        {
            Debug.LogError($"[AuraAttackConfigSO] 프리팹에 AuraBase 컴포넌트가 필요합니다: {name}", this);
            return null;
        }

        if (data is not AuraAttackRuntimeData auraData)
        {
            Debug.LogError($"[AuraAttackConfigSO] 런타임 데이터 타입이 일치하지 않습니다: {ConfigKey}", this);
            return null;
        }

        return new AuraAttack(firePoint, model, this, auraData, interval);
    }
}