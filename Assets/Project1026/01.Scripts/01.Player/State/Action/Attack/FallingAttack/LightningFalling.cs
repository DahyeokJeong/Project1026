using System.Collections.Generic;
using UnityEngine;

public class LightningFalling : FallingBase
{
    #region Settings
    [Header("Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0.01f)] private float hitRadius = 1f;
    #endregion

    #region Fields
    private readonly Collider2D[] hitBuffer = new Collider2D[64];
    private readonly HashSet<Transform> hitTargets = new();

    private ContactFilter2D contactFilter;
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
        hitTargets.Clear();
    }

    protected override void OnLanded()
    {
        hitTargets.Clear();

        int count = Physics2D.OverlapCircle(transform.position, hitRadius, contactFilter, hitBuffer);

        for(int i = 0; i < count; ++i)
        {
            Collider2D hit = hitBuffer[i];

            if(hit == null)
                continue;

            if(!Managers.Damage.TryGetTarget(hit.transform, out Transform target))
                continue;

            if(!target.gameObject.activeInHierarchy)
                continue;

            if(!hitTargets.Add(target))
                continue;

            Managers.Damage.ApplyDamage(target, Damage);
        }
    }


}
