using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [SerializeField] private PlayerData data;

    public int MaxHp { get; set; }
    public int Hp { get; set; }
    public int AttackDamage { get; set; }
    public float AttackSpeed { get; set; }
    public float MoveSpeed { get; set; }
    public float Exp { get; set; }
    public Vector2 LastMoveDirection { get; set; }

    private void Awake()
    {
        MaxHp = data.Hp;
        Hp = MaxHp;
        AttackDamage = data.AttackDamage;
        AttackSpeed = data.AttackSpeed;
        MoveSpeed = data.MoveSpeed;
        Exp = 0f;
        LastMoveDirection = Vector2.down;
    }

}
