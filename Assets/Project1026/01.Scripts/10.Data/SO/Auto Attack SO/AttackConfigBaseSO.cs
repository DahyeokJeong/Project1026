using UnityEngine;

public abstract class AttackConfigBaseSO : ScriptableObject
{
    #region Settings
    [SerializeField] private string configKey;
    [SerializeField, Min(0.01f)] private float interval;
    [SerializeField] private bool initiallyOwned;
    #endregion

    #region Properties
    public string ConfigKey => configKey;
    public float Interval => interval;
    public bool InitiallyOwned => initiallyOwned;
    #endregion

    public abstract AttackRuntimeData CreateRuntimeData(bool initiallyOwned);

    public abstract IAttack CreateAttack(Transform firePoint, PlayerModel model, AttackRuntimeData data, float interval);

}