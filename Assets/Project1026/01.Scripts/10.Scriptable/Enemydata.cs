using UnityEngine;

[CreateAssetMenu(fileName = "ED")]
public class Enemydata : ScriptableObject
{
    [SerializeField] private float maxHp;
    [SerializeField] private float attackDamage;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private int defense;
    [SerializeField] private float attackRange;
    [SerializeField] private EnemyAttackType attackType;

    public float MaxHp => maxHp;
    public float AttackDamage => attackDamage;
    public float AttackSpeed => attackSpeed;
    public float MoveSpeed => moveSpeed;
    public int Defense => defense;
    public float AttackRange => attackRange;
    public EnemyAttackType AttackType => attackType;
}
