using System.Collections.Generic;
using UnityEngine;

public class PiercingProjectile : ProjectileBase
{
    #region Settings
    [SerializeField] private LayerMask enemyLayer;

    [Header("Piercing Settings")]
    [SerializeField] private float hitRadius = 0.2f;
    [SerializeField] private int maxPierceCount = 3;
    #endregion

    #region Fields
    private readonly HashSet<Collider2D> hitTargets = new();
    private readonly Collider2D[] hitBuffer = new Collider2D[32];

    private ContactFilter2D contactFilter;
    private int pierceCount;
    #endregion

    private void Awake()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayer);
        contactFilter.useTriggers = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        pierceCount = 0;
        hitTargets.Clear();
    }

    protected override void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        base.Update();

        if (!gameObject.activeInHierarchy)
            return;

        CheckHit();
    }

    private void CheckHit()
    {
        int count = Physics2D.OverlapCircle(transform.position, hitRadius, contactFilter, hitBuffer);

        for (int i = 0; i < count; ++i)
        {
            Collider2D target = hitBuffer[i];

            if (!hitTargets.Add(target))
                continue;

            Managers.Damage.ApplyDamage(target.transform, Damage);
            pierceCount++;

            if (pierceCount >= maxPierceCount)
            {
                Release();
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}