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
    [Header("Nav AI")]
    [SerializeField] private NavigationArea area;
    public NavigationArea Area => area;

    [Header("attack & chase")]
    [SerializeField, Min(0f)] private float attackRange = 1f;
    [SerializeField, Min(0f)] private float chaseMargin = 0.3f;

    [Header("Enemy Type")]
    [SerializeField] private EnemyAttackType type;

    [Header("Enmey Ranged Radius")]
    [SerializeField, Min(0.1f)] private float fireRaius = 0.3f;
    public EnemyAttackType Type => type;
    private Transform Target => Area != null ? area.Target : null;
    public bool CanAttack => CheckAttackRange(attackRange);
    public bool CanKeepAttack => CheckAttackRange(attackRange + chaseMargin);

    private IState currentState;
    private IState prevState;

    private EnemyAttackState attackState;
    private EnemyChaseState chaseState;

    private void Awake()
    {
        attackState = new EnemyAttackState(this);
        chaseState = new EnemyChaseState(this);
    }

    void Start()
    {
        ChangeState(chaseState);
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

        prevState = currentState;
        currentState = newState;

        currentState?.Enter();
    }

    private void HandleStateChange()
    {
        if(CanAttack)
        {
            ChangeState(attackState);
        }
        else if(!CanKeepAttack)
        {
            ChangeState(chaseState);
        }
    }

    public void Initialize(NavigationArea area)
    {
        this.area?.Unregister(this);

        this.area = area;

        if(isActiveAndEnabled)
            this.area?.Register(this);
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
            return Physics2D.Linecast(origin, target.position, Area.Obstacle).collider == null;
        }

        return Physics2D.CircleCast(origin, fireRaius, shot.normalized, shot.magnitude, Area.Obstacle).collider == null;
    }

    private void OnEnable()
    {
        area?.Register(this);
    }

    private void OnDisable()
    {
        area?.Unregister(this);
    }
}
