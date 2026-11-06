using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack_Falling", menuName = "Game/Attack/Falling")]
public class FallingAttackConfigSO : AttackConfigBaseSO
{
    #region Settings
    [SerializeField] private GameObject prefab;
    [SerializeField, Min(1)] private int initialFallingCount = 1;
    [SerializeField] private bool usePlayerStat;
    #endregion

    #region Properties
    public GameObject Prefab => prefab;
    public bool UsePlayerStat => usePlayerStat;
    #endregion

    public override IAttack CreateAttack(Transform firePoint, PlayerModel model, AttackRuntimeData data, float interval)
    {
        if(prefab == null || model == null)
            return null;

        if(!prefab.TryGetComponent(out FallingBase _))
            return null;

        if(data is not FallingAttackRuntimeData fallingData)
            return null;

        return new FallingAttack(model, this, fallingData, interval);
    }

    public override AttackRuntimeData CreateRuntimeData(bool initiallyOwned)
    {
        return new FallingAttackRuntimeData(initiallyOwned, initialFallingCount);
    }
}
