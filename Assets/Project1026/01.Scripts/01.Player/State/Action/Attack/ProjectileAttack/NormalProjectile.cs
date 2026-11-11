
using UnityEngine;

public class NormalProjectile : ProjectileBase
{
    #region Settings
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float hitRadius = 0.3f;
    #endregion

    #region Fields
    private readonly Collider2D[] hitBuffer = new Collider2D[32];
    private ContactFilter2D contactFilter;
    #endregion

    private void Awake()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayer);
        contactFilter.useTriggers = true;
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
        int count = Physics2D.OverlapCircle(
            transform.position,
            hitRadius,
            contactFilter,
            hitBuffer
        );

        for (int i = 0; i < count; ++i)
        {
            Collider2D hit = hitBuffer[i];
            if (hit == null || !Managers.Damage.TryGetTarget(hit.transform, out Transform target))
                continue;

            if (!target.gameObject.activeInHierarchy)
                continue;

            Managers.Damage.ApplyDamage(target, Damage);
            Release();
            return;
        }
    }
}
