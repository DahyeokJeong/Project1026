using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack_", menuName = "Game/Attack Config")]
public class AttackConfigSO : AttackConfigBaseSO
{
    #region Attack Settings
    [Header("Projectile Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private ProjectilePattern pattern;
    [SerializeField] private bool usePlayerStat;
    [SerializeField, Min(1)] private int initialProjectileCount = 1;
    #endregion

    #region Properties
    public GameObject Prefab => prefab;
    public bool UsePlayerStat => usePlayerStat;
    public ProjectilePattern Pattern => pattern;
    #endregion

    public override AttackRuntimeData CreateRuntimeData(bool initiallyOwned)
    {
        return new ProjectileAttackRuntimeData(initiallyOwned, initialProjectileCount);
    }

    public override IAttack CreateAttack(Transform firePoint, PlayerModel model, AttackRuntimeData data, float interval)
    {
        if (prefab == null || firePoint == null || model == null)
        {
            Debug.LogError($"[AttackConfigSO] 투사체 공격의 필수 참조가 없습니다: {name}", this);
            return null;
        }

        if (data is not ProjectileAttackRuntimeData projectileData)
        {
            Debug.LogError($"[AttackConfigSO] 런타임 데이터 타입이 맞지 않습니다: {ConfigKey}", this);
            return null;
        }

        return new ProjectileAttack(firePoint, model, this, interval, projectileData);
    }
}