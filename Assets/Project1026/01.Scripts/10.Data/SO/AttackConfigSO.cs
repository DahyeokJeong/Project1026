using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack_", menuName = "Game/Attack Config")]
public class AttackConfigSO : ScriptableObject
{
    #region Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private ProjectilePattern pattern;
    #endregion

    #region Properties
    public GameObject Prefab => prefab;
    public ProjectilePattern Pattern => pattern;
    #endregion
}
