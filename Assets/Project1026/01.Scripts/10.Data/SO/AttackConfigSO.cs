using UnityEngine;

[CreateAssetMenu(fileName = "SO_Attack_", menuName = "Game/Attack Config")]
public class AttackConfigSO : ScriptableObject
{
    #region Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private GameObject prefab;
    #endregion

    #region Properties
    public GameObject Prefab => prefab;
    #endregion
}
