using System.Collections.Generic;
using UnityEngine;

public abstract class SummonBase : MonoBehaviour
{
    #region Settings
    [Header("Attack Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0f)] private float baseDamage = 1f;
    [SerializeField, Min(0.01f)] private float attackRadius = 0.5f;
    #endregion

    #region Fields
    private readonly List<Collider2D> hitBuffer = new(32);
    private readonly HashSet<Transform> damagedTargets = new();
    private float attackInterval;
    private float attackTimer;
    private bool initialized;
    #endregion

    #region Properties
    protected PlayerModel Owner { get; private set; }
    protected AttackRuntimeData RuntimeData { get; private set; }
    protected ContactFilter2D EnemyFilter { get; private set; }
    #endregion

    protected virtual void Awake()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;
        EnemyFilter = filter;
    }

    protected virtual void OnEnable()
    {
        initialized = false;
    }

    public void Initialize(PlayerModel owner, AttackRuntimeData data, float interval)
    {
        Owner = owner;
        RuntimeData = data;
        attackInterval = Mathf.Max(0.01f, interval);
        attackTimer = 0f;
        damagedTargets.Clear();

        OnSummonInitialized();
        initialized = true;
    }

    protected virtual void Update()
    {
        if (!initialized)
            return;

        if (Owner == null || !Owner.gameObject.activeInHierarchy)
        {
            Release();
            return;
        }

        if (!Managers.BattleState.CanUpdate)
            return;

        Move(Time.deltaTime);
        attackTimer = Mathf.Max(0f, attackTimer - Time.deltaTime);

        if (attackTimer <= 0f && TryAttack())
            attackTimer = attackInterval;
    }

    protected abstract void OnSummonInitialized();
    protected abstract void Move(float deltaTime);

    private bool TryAttack()
    {
        Physics2D.OverlapCircle(transform.position, attackRadius, EnemyFilter, hitBuffer);
        damagedTargets.Clear();

        foreach (Collider2D hit in hitBuffer)
        {
            if (hit == null)
                continue;

            if (!Managers.Damage.TryGetTarget(hit.transform, out Transform target))
                continue;

            if (!target.gameObject.activeInHierarchy || !damagedTargets.Add(target))
                continue;

            float damage = baseDamage + Owner.AttackDamage + RuntimeData.DamageBonus;
            if (Random.value < Mathf.Clamp(Owner.Critical, 0f, 100f) / 100f)
                damage *= 1.5f;

            Managers.Damage.ApplyDamage(target, damage);
        }

        return damagedTargets.Count > 0;
    }

    protected void Release()
    {
        initialized = false;
        Managers.Pool.Push(gameObject);
    }

    protected virtual void OnDisable()
    {
        initialized = false;
        Owner = null;
        RuntimeData = null;
        hitBuffer.Clear();
        damagedTargets.Clear();
    }
}