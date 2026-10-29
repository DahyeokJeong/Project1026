using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Attack,
    Chase
}

public enum EnemyAttackType
{
    Melee,
    Ranged
}

public class EnemyController : MonoBehaviour
{
    [Header("Nav Ai")]
    [SerializeField] private NavigationArea navArea;
    public NavigationArea NavArea => navArea;

    [Header("attack & chase")]
    [SerializeField, Min(0f)] private float attackTimer = 2f;
    [SerializeField, Min(0f)] private float attackRange = 1f;
    [SerializeField, Min(0f)] private float chaseMargin = 0.3f;
    [SerializeField] private GameObject attackPrefab;

    [Header("Enemy Type")]
    [SerializeField] private EnemyAttackType type;

    [Header("Enmey Ranged Radius")]
    [SerializeField, Min(0.1f)] private float fireRaius = 0.3f;

    public bool IsHit { get; set; } = false;
    public float AttackTimer => attackTimer;
    public GameObject AttackPrefab => attackPrefab;
    public EnemyAttackType Type => type;
    private Transform Target => NavArea != null ? NavArea.Target : null;
    public bool CanAttack => CheckAttackRange(attackRange);
    public bool CanKeepAttack => CheckAttackRange(attackRange + chaseMargin);

    private IState currentState;

    private EnemyGenerate generate;
    private EnemyAttackState attackState;
    private EnemyChaseState chaseState;
    private EnemyHitState hitState;

    public void Initialize(NavigationArea area, EnemyGenerate generate)
    {
        attackState = new EnemyAttackState(this);
        chaseState = new EnemyChaseState(this);
        hitState = new EnemyHitState(this);

        NavArea?.Unregister(this);
        navArea = area;

        this.generate = generate;
        ChangeState(chaseState);
        attackState.ResetTimer();

        if (isActiveAndEnabled)
            this.NavArea?.Register(this);
    }

    void Update()
    {
        HandleStateChange();
        currentState.Tick();
    }

    private void FixedUpdate()
    {
        currentState.FixedTick();
    }

    public void ChangeState(IState newState)
    {
        if (currentState == newState)
            return;

        currentState?.Exit();

        currentState = newState;

        currentState?.Enter();
    }

    private void HandleStateChange()
    {
        if(IsHit)
        {
            attackState.ResetTimer();
            ChangeState(hitState);
        }
        else if(CanAttack)
        {
            ChangeState(attackState);
        }
        else if(!CanKeepAttack)
        {
            ChangeState(chaseState);
        }
    }

    private bool CheckAttackRange(float range)
    {
        Transform target = Target;

        if(target == null)
            return false;

        Vector2 origin = transform.position;
        Vector2 shot = (Vector2)target.position - origin;
        if (shot.sqrMagnitude > range * range)
            return false;

        if(type == EnemyAttackType.Melee)
        {
            return Physics2D.Linecast(origin, target.position, NavArea.Obstacle).collider == null;
        }

        return Physics2D.CircleCast(origin, fireRaius, shot.normalized, shot.magnitude, NavArea.Obstacle).collider == null;
    }

    public void TakeDamage(int damage)
    {
        IsHit = true;
        hitState.GetDamage(damage);
        generate.Despawn(this);
    }

    private void OnDisable()
    {
        NavArea?.Unregister(this);
    }
}
