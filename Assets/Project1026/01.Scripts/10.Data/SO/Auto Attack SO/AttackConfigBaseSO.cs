using UnityEngine;

public abstract class AttackConfigBaseSO : ScriptableObject
{
    #region Settings
    [SerializeField] private string configKey;
    #endregion

    #region Properties
    public string ConfigKey => configKey;
    #endregion

    public abstract AttackRuntimeData CreateRuntimeData(bool initiallyOwned);

    public abstract IAttack CreateAttack(Transform firePoint, PlayerModel model, AttackRuntimeData data, float interval);
}