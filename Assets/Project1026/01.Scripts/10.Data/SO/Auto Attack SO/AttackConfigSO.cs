using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack_", menuName = "Game/Attack Config")]
public class AttackConfigSO : AttackConfigBaseSO
{
    #region Attack Settings
    [Header("Projectile Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private bool usePlayerStat;
    [SerializeField] private bool ignoreCooldownAugment;
    [SerializeField, Min(1)] private int initialProjectileCount = 1;
    #endregion

    #region Properties
    public GameObject Prefab => prefab;
    public bool UsePlayerStat => usePlayerStat;
    public bool IgnoreCooldownAugment => ignoreCooldownAugment;
    #endregion

    public override AttackRuntimeData CreateRuntimeData(bool initiallyOwned)
    {
        return new ProjectileAttackRuntimeData(initiallyOwned, initialProjectileCount);
    }

    public override IAttack CreateAttack(Transform firePoint, PlayerModel model, AttackRuntimeData data, float interval)
    {
        if (prefab == null || firePoint == null || model == null)
        {
            Debug.LogError($"[AttackConfigSO] ����ü ������ �ʼ� ������ �����ϴ�: {name}", this);
            return null;
        }

        if (data is not ProjectileAttackRuntimeData projectileData)
        {
            Debug.LogError($"[AttackConfigSO] ��Ÿ�� ������ Ÿ���� ���� �ʽ��ϴ�: {ConfigKey}", this);
            return null;
        }

        return new ProjectileAttack(firePoint, model, this, interval, projectileData);
    }
}