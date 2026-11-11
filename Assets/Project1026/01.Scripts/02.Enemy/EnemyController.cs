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
    #region Component
    [Header("Component")]
    [SerializeField] private EnemyModel model;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private EnemyView view;
    #endregion

    #region Attack
    [Header("attack & chase")]
    [SerializeField] private GameObject attackPrefab;

    #endregion

    #region ExpPrefab
    [SerializeField] private GameObject expPrefab;
    #endregion

    #region fields
    private IState currentState;

    private EnemyGenerate generate;
    private EnemyAttackState attackState;
    private EnemyChaseState chaseState;
    private EnemyHitState hitState;
    private EnemyDeadState deadState;
    #endregion

    #region Property
    public NavigationArea NavArea { get; private set; }
    private Transform Target => NavArea != null ? NavArea.Target : null;
    public EnemyModel Model => model;
    public Collider2D MovementCollider { get; private set; }
    public EnemyView View => view;
    public EnemyAttack Attack => attack;
    public bool CanAttack => CheckAttackRange(model.AttackRange);
    public bool IsHoldingAttackPosition => attackState != null && currentState == attackState;
    public GameObject AttackPrefab => attackPrefab;
    #endregion

    #region Event
    public static event System.Action<EnemyGenerate> EnemyDead;
    #endregion

    public void Initialize(NavigationArea area, EnemyGenerate generate)
    {
        Model.Init();
        MovementCollider = GetComponent<Collider2D>();
        NavArea?.Unregister(this);
        NavArea = area;
        this.generate = generate;

        attackState = new EnemyAttackState(this, Attack);
        chaseState = new EnemyChaseState(this);
        hitState = new EnemyHitState(this);
        deadState = new EnemyDeadState(this);

        ChangeState(chaseState);
        attackState.ResetTimer();

        if (isActiveAndEnabled)
            this.NavArea?.Register(this);
    }

    void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        if (Model.IsDead)
        {
            if (deadState == null)
            {
                CompleteDeath();
                return;
            }

            ChangeState(deadState);
        }
        else
        {
            HandleStateChange();
        }

        currentState.Tick();
    }

    public void CompleteDeath()
    {
        Vector3 deathPosition = transform.position;

        if (!generate.Despawn(this))
            return;

        ExpPickup.Spawn(expPrefab, deathPosition, model.RewardExp);
        EnemyDead?.Invoke(generate);
    }

    private void FixedUpdate()
    {
        if (!Managers.BattleState.CanUpdate || Model.IsDead)
            return; 

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
        if (Model.IsDead)
            return;
        if(Model.IsHit)
        {
            if (currentState != hitState)
            {
                attackState.ResetTimer();
                ChangeState(hitState);
            }
            return;
        }

        ChangeState(CanAttack ? attackState : chaseState);
    }

    private bool CheckAttackRange(float range)
    {
        Transform target = Target;

        if(target == null)
            return false;

        Vector2 targetPosition = target.position;
        Vector2 position = transform.position;
        if ((targetPosition - position).sqrMagnitude > range * range)
            return false;

        Vector2 origin = model.AttackType == EnemyAttackType.Ranged ? attack.FirePosition : position;
        return Physics2D.Linecast(origin, targetPosition, NavArea.Obstacle).collider == null;
    }

    private void OnDisable()
    {
        NavArea?.Unregister(this);
        if (Managers.Initialized)
            Managers.Damage.Unregister(transform);
    }
}
