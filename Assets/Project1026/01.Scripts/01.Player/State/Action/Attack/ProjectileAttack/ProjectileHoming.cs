using System.Collections.Generic;
using UnityEngine;

public class ProjectileHoming : MonoBehaviour
{
    #region Settings
    [Header("Homing Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0f)] private float searchRadius = 5f;
    [SerializeField, Min(0f)] private float turnSpeed = 360f;
    #endregion

    #region Fields
    private readonly List<Collider2D> hits = new();

    private ContactFilter2D contactFilter;
    private Transform target;
    private bool targetSelected;
    private bool homingEnabled;
    #endregion

    private void Awake()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayer);
        contactFilter.useTriggers = true;
    }

    private void OnEnable()
    {
        target = null;
        targetSelected = false;
        homingEnabled = false;
    }

    public void SetHomingEnabled(bool enabled)
    {
        homingEnabled = enabled;
    }

    public Vector2 GetDirection(Vector2 currentDirection)
    {
        if (!homingEnabled)
            return currentDirection;

        if (!targetSelected)
        {
            SelectTarget();
            targetSelected = true;
        }

        if (target == null)
            return currentDirection;

        if (!target.gameObject.activeInHierarchy || !Managers.Damage.IsRegistered(target))
        {
            target = null;
            return currentDirection;
        }

        Vector2 direction = target.position - transform.position;

        if (direction.sqrMagnitude <= 0f)
            return currentDirection;

        float currentAngle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, turnSpeed * Time.deltaTime);
        float radians = angle * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private void SelectTarget()
    {
        Physics2D.OverlapCircle(transform.position, searchRadius, contactFilter, hits);

        float nearestDistance = searchRadius * searchRadius;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (!Managers.Damage.TryGetTarget(hit.transform, out Transform candidate))
                continue;

            if (!candidate.gameObject.activeInHierarchy)
                continue;

            float distance = (candidate.position - transform.position).sqrMagnitude;

            if (distance > nearestDistance)
                continue;

            nearestDistance = distance;
            target = candidate;
        }
    }
}