using UnityEngine;

public abstract class AugmentEffectSO : ScriptableObject
{
    #region Settings
    [SerializeField] private string configKey;
    [SerializeField] private Sprite icon;
    #endregion

    #region Properties
    public string ConfigKey => configKey;
    public Sprite Icon => icon;
    #endregion

    public abstract bool CanApply(PlayerAutoAttack auto, AugmentData data);

    public abstract bool TryApply(PlayerAutoAttack auto, AugmentData data);
}