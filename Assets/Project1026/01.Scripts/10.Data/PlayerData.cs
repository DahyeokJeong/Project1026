using UnityEngine;

[CreateAssetMenu(fileName = "PD")]
public class PlayerData : ScriptableObject
{
    [SerializeField] private int maxHp;
    [SerializeField] private int maxExp;
    [SerializeField] private int attackPower;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private int defense;
    [SerializeField] private float critical;

    public int MaxHp { get { return maxHp; } }
    public int MaxExp { get { return maxExp; } }
    public int AttackPower { get { return attackPower; } }
    public float AttackSpeed { get { return attackSpeed; } }
    public float MoveSpeed { get { return moveSpeed; } }
    public int Defense { get { return defense; } }
    public float Critical { get { return critical; } }
}
