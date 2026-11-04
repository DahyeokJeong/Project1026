
using System.Collections.Generic;
using UnityEngine;

public class PiercingProjectile : ProjectileBase
{
    #region Component
    [Header("Component")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallLayer;
    #endregion

    #region Projectile Settings
    [Header("Piercing Settings")]
    [SerializeField] private float hitRadius = 0.2f;
    [SerializeField] private int maxPierceCount = 3;
    [SerializeField] private float wallRadius = 0.1f;
    [SerializeField] private float wallOffset = 0.02f;
    [SerializeField] private int maxBouncePerFrame = 3;
    #endregion

    #region Fields
    private int pierceCount;

    private readonly HashSet<Collider2D> hitTargets = new();
    private readonly Collider2D[] hitBuffer = new Collider2D[32];
    private readonly Collider2D[] wallBuffer = new Collider2D[16];

    private ContactFilter2D contactFilter;
    private ContactFilter2D wallFilter;
    #endregion

    private void Awake()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayer);
        contactFilter.useTriggers = true;

        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(wallLayer);
        wallFilter.useTriggers = false;
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

    protected override void Move()
    {
        Vector2 position = transform.position;
        Vector2 direction = MoveDirection.normalized;
        float remainingDistance = MoveSpeed * Time.deltaTime;

        ResolveWallOverlap(ref position, ref direction);

        for (int i = 0; i < maxBouncePerFrame && remainingDistance > 0f; i++)
        {
            RaycastHit2D hit = Physics2D.CircleCast(
                position,
                wallRadius,
                direction,
                remainingDistance,
                wallLayer
            );

            if (hit.collider == null)
            {
                position += direction * remainingDistance;
                remainingDistance = 0f;
                break;
            }

            if (hit.distance <= 0f)
            {
                if (!ResolveWallOverlap(ref position, ref direction))
                    break;

                continue;
            }

            float moveDistance = Mathf.Max(
                0f,
                hit.distance - wallOffset
            );

            position += direction * moveDistance;

            remainingDistance -= hit.distance;

            direction = Vector2.Reflect(
                direction,
                hit.normal
            ).normalized;

            position += hit.normal * wallOffset;

            ResolveWallOverlap(ref position, ref direction);
        }

        SetDirection(direction);

        transform.position = new Vector3(
            position.x,
            position.y,
            transform.position.z
        );
    }

    private bool ResolveWallOverlap(
        ref Vector2 position,
        ref Vector2 direction)
    {

        for (int iteration = 0; iteration < 8; iteration++)
        {
            int count = Physics2D.OverlapCircle(
                position,
                wallRadius,
                wallFilter,
                wallBuffer
            );

            if (count == 0)
                return true;

            bool moved = false;

            for (int i = 0; i < count; i++)
            {
                Collider2D wall = wallBuffer[i];

                if (wall == null)
                    continue;

                if (wall is not BoxCollider2D box)
                    continue;

                Vector2 push = GetBoxSeparation(
                    box,
                    position,
                    wallRadius + wallOffset
                );

                if (push.sqrMagnitude <= 0.000001f)
                    continue;

                position += push;

                Vector2 normal = push.normalized;

                if (Vector2.Dot(direction, normal) < 0f)
                {
                    direction = Vector2.Reflect(
                        direction,
                        normal
                    ).normalized;
                }

                moved = true;
            }

            if (!moved)
                return false;
        }

        return Physics2D.OverlapCircle(
            position,
            wallRadius,
            wallLayer
        ) == null;
    }

    private Vector2 GetBoxSeparation(
        BoxCollider2D box,
        Vector2 worldPosition,
        float radius)
    {
        Transform wallTransform = box.transform;

        Vector2 localPosition =
            wallTransform.InverseTransformPoint(worldPosition);

        Vector2 center = box.offset;
        Vector2 halfSize = box.size * 0.5f;

        Vector2 min = center - halfSize;
        Vector2 max = center + halfSize;

        Vector2 closest = new Vector2(
            Mathf.Clamp(localPosition.x, min.x, max.x),
            Mathf.Clamp(localPosition.y, min.y, max.y)
        );

        Vector2 difference = localPosition - closest;

        Vector3 scale = wallTransform.lossyScale;

        float scaleX = Mathf.Abs(scale.x);
        float scaleY = Mathf.Abs(scale.y);

        if (scaleX <= 0.0001f || scaleY <= 0.0001f)
            return Vector2.zero;

        if (difference.sqrMagnitude > 0.000001f)
        {
            Vector2 worldDifference =
                wallTransform.TransformVector(difference);

            float distance = worldDifference.magnitude;

            if (distance >= radius)
                return Vector2.zero;

            return worldDifference.normalized *
                (radius - distance);
        }

        float left = (localPosition.x - min.x) * scaleX;
        float right = (max.x - localPosition.x) * scaleX;
        float bottom = (localPosition.y - min.y) * scaleY;
        float top = (max.y - localPosition.y) * scaleY;

        float minDistance = Mathf.Min(
            left,
            right,
            bottom,
            top
        );

        Vector2 localNormal;
        float pushDistance = minDistance + radius;

        if (minDistance == left)
            localNormal = Vector2.left;
        else if (minDistance == right)
            localNormal = Vector2.right;
        else if (minDistance == bottom)
            localNormal = Vector2.down;
        else
            localNormal = Vector2.up;

        Vector2 worldNormal =
            wallTransform.TransformDirection(localNormal);

        return worldNormal.normalized * pushDistance;
    }

    private void CheckHit()
    {
        int count = Physics2D.OverlapCircle(
            transform.position,
            hitRadius,
            contactFilter,
            hitBuffer
        );

        for (int i = 0; i < count; i++)
        {
            Collider2D target = hitBuffer[i];

            if (!hitTargets.Add(target))
                continue;

            Managers.Damage.ApplyDamage(
                target.transform,
                Damage
            );

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
        Gizmos.DrawWireSphere(
            transform.position,
            hitRadius
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            wallRadius
        );
    }
}
