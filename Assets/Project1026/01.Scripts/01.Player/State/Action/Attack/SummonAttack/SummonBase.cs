using System.Collections.Generic;
using UnityEngine;

public abstract class SummonBase : MonoBehaviour
{
    #region Settings
    [Header("Attack Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0f)] private float baseDamage = 1f;
    #endregion

    #region Fields
    private bool initialized;
    #endregion

    #region Properties
    protected PlayerModel Owner { get; private set; }
    protected ContactFilter2D EnemyFilter { get; private set; }
    #endregion

    protected virtual void Awake()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;
        EnemyFilter = filter;
    }

    protected bool CanUpdateSummon()
    {
        if (!initialized)
            return false;

        if(Owner == null || !Owner.gameObject.activeInHierarchy)
        {
            Release();
            return false;
        }

        return Managers.BattleState.CanUpdate;
    }

    protected float CalculateDamage()
    {
        return Owner.AttackDamage * baseDamage;
    }

    public void Initialize(PlayerModel owner, AttackRuntimeData data, float interval)
    {
        Owner = owner;
        OnSummonInitialized(interval);
        initialized = true;
    }

    protected abstract void OnSummonInitialized(float interval);

    protected void Release()
    {
        initialized = false;
        Managers.Pool.Push(gameObject);
    }

    protected virtual void OnDisable()
    {
        initialized = false;
        Owner = null;
    }
}