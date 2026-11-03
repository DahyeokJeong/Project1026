using UnityEngine;

public abstract class AugmentEffectSO : ScriptableObject
{
    #region Settings
    [SerializeField] private string configKey;
    #endregion

    #region Properties
    public string ConfigKey => configKey;
    #endregion

    public abstract bool CanApply(PlayerAutoAttack auto, AugmentData data);

    public abstract bool TryApply(PlayerAutoAttack auto, AugmentData data);
}