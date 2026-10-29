using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [SerializeField] private PlayerData data;

    public int MaxHp { get; set; }
    public int AttackDamage { get; set; }
    public float AttackSpeed { get; set; }
    public float MoveSpeed { get; set; }
    public Vector2 LastMoveDirection { get; set; }
    private float currentHp;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        MaxHp = data.MaxHp;
        currentHp = MaxHp;
        AttackDamage = data.AttackPower;
        AttackSpeed = data.AttackSpeed;
        MoveSpeed = data.MoveSpeed;
        LastMoveDirection = Vector2.down;
    }

    public void ReduceHP(float damage)
    {
        currentHp -= damage;
    }
}
