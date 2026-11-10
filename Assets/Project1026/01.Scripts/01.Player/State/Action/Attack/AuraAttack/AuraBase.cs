using UnityEngine;

public abstract class AuraBase : MonoBehaviour
{
    #region Aura Settings
    [Header("Aura Settings")]
    [SerializeField] private float damageMultiplier = 1f;
    #endregion

    #region Fields
    private Transform target;
    #endregion

    #region Properties
    protected PlayerModel Owner {  get; private set; }
    protected AuraAttackRuntimeData RuntimeData { get; private set; }
    protected float TickInterval { get; private set; }
    #endregion

    protected virtual void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy || !Managers.BattleState.CanUpdate)
            return;

        Chase();
    }

    protected virtual void Chase()
    {
        transform.position = target.position;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        if(target != null)
            transform.position = target.position;
    }

    public void SetAttack(PlayerModel owner,Transform fierPoint ,AuraAttackRuntimeData data, float interval)
    {
        Owner = owner;
        RuntimeData = data;
        TickInterval = Mathf.Max(0.01f, interval);

        SetTarget(fierPoint);
        OnAttackInitialized();
    }

    protected abstract void OnAttackInitialized();

    protected float CalculateDamage()
    {
        float damage = Owner.AttackDamage * damageMultiplier + RuntimeData.DamageBonus;
        float critical = Mathf.Clamp(Owner.Critical, 0f, 100f) / 100f;

        if (Random.value < critical)
            damage *= 1.5f;

        return damage;
    }
}
