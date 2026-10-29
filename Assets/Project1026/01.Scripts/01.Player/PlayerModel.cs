using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    //임시 스크립트아블 연결
    [SerializeField] private PlayerData data;

    public int MaxHp { get; set; }
    public float AttackDamage { get; set; }
    public float AttackSpeed { get; set; }
    public float MoveSpeed { get; set; }
    public float Critical { get; set; }
    public Vector2 LastMoveDirection { get; set; }
    private float currentHp;

    // 추후 변경 Manager로 값 연동 예정
    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        MaxHp = data.MaxHp;
        AttackDamage = data.AttackPower;
        AttackSpeed = data.AttackSpeed;
        MoveSpeed = data.MoveSpeed;
        Critical = data.Critical;
        currentHp = MaxHp;
        LastMoveDirection = Vector2.down;
    }

    public void ReduceHP(float damage)
    {
        currentHp -= damage;

        if(currentHp < 0f)
            currentHp = 0f;
        Debug.Log(currentHp);
    }

    private void OnEnable()
    {
        Managers.Damage.Register(transform, ReduceHP);
    }

    private void OnDisable()
    {
        if (Managers.Initialized)
            Managers.Damage.Unregister(transform);
    }
}
