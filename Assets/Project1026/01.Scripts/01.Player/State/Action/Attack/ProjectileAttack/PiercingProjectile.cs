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
    private readonly HashSet<Transform> hitTargets = new();
    private readonly Collider2D[] hitBuffer = new Collider2D[32];

    private Vector3 initialScale;
    private float sizeMultiplier = 1f;

    private ContactFilter2D contactFilter;
    #endregion

    private void Awake()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayer);
        contactFilter.useTriggers = true;

        initialScale = transform.localScale;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        hitTargets.Clear();
        SetProjectileSizeBonus(0);
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
        int count = Physics2D.OverlapCircle(transform.position, hitRadius * sizeMultiplier, contactFilter, hitBuffer);

        for (int i = 0; i < count; ++i)
        {
            Collider2D hit = hitBuffer[i];
            if (hit == null || !Managers.Damage.TryGetTarget(hit.transform, out Transform target))
                continue;

            if (!target.gameObject.activeInHierarchy || !hitTargets.Add(target))
                continue;

            Managers.Damage.ApplyDamage(target, Damage);

            if (maxPierceCount > 0 && hitTargets.Count >= maxPierceCount)
            {
                Release();
                return;
            }
        }
    }
    public void SetProjectileSizeBonus(int bonusPercent)
    {
        sizeMultiplier = 1f + bonusPercent * 0.01f;
        transform.localScale = initialScale * sizeMultiplier;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius * sizeMultiplier);
    }
}
