using UnityEngine;

[CreateAssetMenu(fileName = "PD")]
public class PlayerData : ScriptableObject
{
    [SerializeField] private int hp;
    [SerializeField] private int attackDamage;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float moveSpeed;

    public int Hp => hp;
    public int AttackDamage => attackDamage;
    public float AttackSpeed => attackSpeed;
    public float MoveSpeed => moveSpeed;
}
