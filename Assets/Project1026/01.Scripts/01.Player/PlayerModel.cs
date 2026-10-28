using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    [SerializeField] private PlayerData data;

    public int MaxHp { get; private set; }
    public int Hp { get; set; }
    public int AttackDamage { get; private set; }
    public float AttackSpeed { get; private set; }
    public float MoveSpeed { get; private set; }
    public float Exp { get; set; }

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        MaxHp = data.Hp;
        Hp = MaxHp;
        AttackDamage = data.AttackDamage;
        AttackSpeed = data.AttackSpeed;
        MoveSpeed = data.MoveSpeed;
        Exp = 0f;
    }

    public void AddExp(float addExp)
    {
        Exp += addExp;
        Debug.Log(addExp);
    }
}
