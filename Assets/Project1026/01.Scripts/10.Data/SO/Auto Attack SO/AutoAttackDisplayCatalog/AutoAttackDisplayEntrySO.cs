using UnityEngine;

[CreateAssetMenu(
    fileName = "SO_AutoAttackDisplay",
    menuName = "Data/Auto Attack Display Entry")]
public class AutoAttackDisplayEntrySO : ScriptableObject
{
    public AttackConfigBaseSO config;
    public string displayName;

    [TextArea(2, 4)] public string summary;
    [TextArea(3, 8)] public string behavior;
    [TextArea(2, 6)] public string baseStats;
}