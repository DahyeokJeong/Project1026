using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : ProjectileBase
{
    #region Settings
    [Header("Homing Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0f)] private float searchRadius = 5f;
    [SerializeField, Min(0f)] private float turnSpeed = 360f;
    [SerializeField, Min(0.01f)] private float hitRadius = 0.3f;
    #endregion

    #region Fields
    private readonly List<Collider2D> hits = new();
    private ContactFilter2D contactFilter;
    private Transform target;
    private bool targetSelected;
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

        target = null;
        targetSelected = false;
    }

    protected override void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        base.Update();

        CheckHit();
    }

    protected override void Move()
    {
        if(!targetSelected)
        {
            SelectTarget();
            targetSelected = true;
        }

        UpdateDirection();
        base.Move();
    }

    private void UpdateDirection()
    {
        if (target == null)
            return;

        if(!target.gameObject.activeInHierarchy || !Managers.Damage.IsRegistered(target))
        {
            target = null;
            return;
        }

        Vector2 dir = target.position - transform.position;

        if(dir.sqrMagnitude <= 0f)
            return;

        float currentAngle = Mathf.Atan2(MoveDirection.y, MoveDirection.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * Time.deltaTime);
        float radians = angle * Mathf.Deg2Rad;

        SetDirection(new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)));
    }

    private void SelectTarget()
    {
        Physics2D.OverlapCircle(transform.position, searchRadius, contactFilter, hits);

        float nearestDis = searchRadius * searchRadius;

        foreach(Collider2D hit in hits)
        {
            if(hit == null)
                continue;
            if(!Managers.Damage.TryGetTarget(hit.transform, out Transform candidate))
                continue;
            if(!candidate.gameObject.activeInHierarchy)
                continue;
            float dis = (candidate.position - transform.position).sqrMagnitude;

            if (dis > nearestDis)
                continue;

            nearestDis = dis;

            target = candidate;
        }
    }

    private void CheckHit()
    {
        Physics2D.OverlapCircle(transform.position, hitRadius, contactFilter, hits);

        foreach(Collider2D hit in hits)
        {
            if(hit == null)
                continue;
            if(!Managers.Damage.TryGetTarget(hit.transform, out Transform hitTarget))
                continue;

            if(!hitTarget.gameObject.activeInHierarchy)
                continue;

            Managers.Damage.ApplyDamage(hitTarget, Damage);
            Release();
            return;
        }
    }
}
