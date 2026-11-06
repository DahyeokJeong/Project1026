using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack_Wave", menuName = "Game/Attack/Wave")]
public class WaveAttackConfigSO : AttackConfigBaseSO
{
    #region Settings
    [Header("Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private bool usePlayerStat;
    #endregion

    #region Properties
    public GameObject Prefab => prefab;
    public bool UsePlayerStat => usePlayerStat;
    #endregion

    public override IAttack CreateAttack(Transform firePoint, PlayerModel model, AttackRuntimeData data, float interval)
    {
        if (prefab == null || model == null || data == null)
            return null;

        if(!prefab.TryGetComponent(out WaveBase _))
            return null;

        return new WaveAttack(firePoint, model, this, data, interval);
    }

    public override AttackRuntimeData CreateRuntimeData(bool initiallyOwned)
    {
        return new AttackRuntimeData(initiallyOwned);
    }
}
