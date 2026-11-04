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
    #endregion

    #region Attack
    [Header("attack & chase")]
    [SerializeField] private GameObject attackPrefab;

    [Header("Enmey Ranged Radius")]
    [SerializeField, Min(0.1f)] private float fireRaius = 0.3f;
    #endregion

    #region field
    private IState currentState;

    private EnemyGenerate generate;
    private EnemyAttackState attackState;
    private EnemyChaseState chaseState;
    private EnemyHitState hitState;
    #endregion

    #region Property
    public NavigationArea NavArea { get; private set; }
    private Transform Target => NavArea != null ? NavArea.Target : null;
    public EnemyModel Model => model;
    public EnemyAttack Attack => attack;
    public bool CanAttack => CheckAttackRange(model.AttackRange);
    public GameObject AttackPrefab => attackPrefab;
    #endregion

    public void Initialize(NavigationArea area, EnemyGenerate generate)
    {
        Model.Init();
        NavArea?.Unregister(this);
        NavArea = area;
        this.generate = generate;

        attackState = new EnemyAttackState(this, Attack);
        chaseState = new EnemyChaseState(this);
        hitState = new EnemyHitState(this);

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
            Managers.RunData.AddExp(model.RewardExp);
            generate.Despawn(this);
            return;
        }

        HandleStateChange();
        currentState.Tick();
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

        Vector2 origin = transform.position;
        Vector2 shot = (Vector2)target.position - origin;
        if (shot.sqrMagnitude > range * range)
            return false;

        if(model.AttackType == EnemyAttackType.Melee)
        {
            return Physics2D.Linecast(origin, target.position, NavArea.Obstacle).collider == null;
        }

        return Physics2D.CircleCast(origin, fireRaius, shot.normalized, shot.magnitude, NavArea.Obstacle).collider == null;
    }

    private void OnDisable()
    {
        NavArea?.Unregister(this);
        if (Managers.Initialized)
            Managers.Damage.Unregister(transform);
    }
}
