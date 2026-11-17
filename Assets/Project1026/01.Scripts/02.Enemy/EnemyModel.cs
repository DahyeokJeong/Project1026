using UnityEngine;

public class EnemyModel : MonoBehaviour
{
    [SerializeField] private Enemydata data;

    public float MaxHp { get; set; }
    private float CurrentHp;

    public float AttackDamage { get; set; }
    public float Defense { get; set; }
    public float AttackInerval { get; set; }
    public float AttackRange { get; set; }

    public float MoveSpeed { get; set; }

    public EnemyAttackType AttackType { get; set; }

    public float RewardExp { get; private set; }

    public bool IsHit { get; set; }
    public bool IsDead => CurrentHp <= 0f;

    private void Awake()
    {
        Init();
    }

    public void Init(float hpMultiplier = 1f, float damageMultiplier = 1f, float expMultiplier = 1f)
    {
        MaxHp = data.MaxHp * hpMultiplier;
        CurrentHp = MaxHp;
        AttackDamage = data.AttackDamage * damageMultiplier;
        AttackRange = data.AttackRange;
        Defense = data.Defense;
        AttackInerval = data.AttackSpeed;
        MoveSpeed = data.MoveSpeed;
        AttackType = data.AttackType;
        RewardExp = data.RewardExp * expMultiplier;
        IsHit = false;
    }

    public void RecudeHp(float damage)
    {
        if (IsHit || damage <= 0f)
            return;

        CurrentHp = Mathf.Max(0f, CurrentHp - damage);
        IsHit = true;
    }

    private void OnEnable()
    {
        Managers.Damage.Register(transform, RecudeHp);
    }
}
