using System.Collections.Generic;
using UnityEngine;


public class ExplosionProjectile : ProjectileBase
{
    #region Settings
    [Header("Settings")]
    [SerializeField] private LayerMask enemtLayer;
    [SerializeField] private float hitRadius = 0.3f;
    [SerializeField] private Vector2 explosionRadius = new Vector2(3f, 1.5f);
    [SerializeField] private GameObject explosionPrefab;
    #endregion

    #region Fields
    private readonly List<Collider2D> hits = new();
    private readonly HashSet<Transform> damagedTargets = new();
    private float explosionRadiusMultiplier = 1f;
    private ContactFilter2D contactFilter;
    #endregion

    private void Awake()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemtLayer);
        contactFilter.useTriggers = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        damagedTargets.Clear();
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
        Physics2D.OverlapCircle(transform.position, hitRadius, contactFilter, hits);

        foreach(Collider2D hit in hits)
        {
            if(hit == null)
                continue;

            if (!Managers.Damage.TryGetTarget(hit.transform, out Transform target))
                continue;

            if(!target.gameObject.activeInHierarchy)
                continue;

            Managers.Pool.Pop(explosionPrefab, transform.position, Quaternion.identity);
            Explode();
            return;
        }
    }

    private void Explode()
    {
        Vector2 center = transform.position;
        Vector2 radius = explosionRadius * explosionRadiusMultiplier;
        float searchRadius = Mathf.Max(radius.x, radius.y);

        Physics2D.OverlapCircle(center, searchRadius, contactFilter, hits);

        damagedTargets.Clear();

        foreach(Collider2D hit in hits)
        {
            if(hit == null)
                continue;

            if (!Managers.Damage.TryGetTarget(hit.transform, out Transform target))
                continue;

            if(!target.gameObject.activeInHierarchy)
                continue;

            Vector2 offset = (Vector2)hit.ClosestPoint(center) - center;
            float x = offset.x / explosionRadius.x;
            float y = offset.y / explosionRadius.y;

            if (x * x + y * y > 1f)
                continue;

            if(!damagedTargets.Add(target))
                continue;

            Managers.Damage.ApplyDamage(target, Damage);
        }

        Release();
    }

    public void SetExplosionRadiusBonus(int bonusPercent)
    {
        explosionRadiusMultiplier = 1f + bonusPercent * 0.01f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hitRadius);

        Gizmos.color = Color.yellow;

        const int segments = 64;
        Vector3 center = transform.position;

        for (int i = 0; i < segments; i++)
        {
            float a1 = i * Mathf.PI * 2f / segments;
            float a2 = (i + 1) * Mathf.PI * 2f / segments;

            Vector3 p1 = center + new Vector3(
                Mathf.Cos(a1) * explosionRadius.x,
                Mathf.Sin(a1) * explosionRadius.y, 0f);

            Vector3 p2 = center + new Vector3(
                Mathf.Cos(a2) * explosionRadius.x,
                Mathf.Sin(a2) * explosionRadius.y, 0f);

            Gizmos.DrawLine(p1, p2);
        }
    }
}
