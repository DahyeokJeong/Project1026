
using UnityEngine;

public class NormalProjectile : ProjectileBase
{
    #region Component
    [Header("Component")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallLayer;
    #endregion

    #region Projectile Settings
    [Header("Projectile Settings")]
    [SerializeField] private float hitRadius = 0.3f;
    [SerializeField] private float wallRadius = 0.1f;
    [SerializeField] private float wallOffset = 0.02f;
    [SerializeField] private int maxBouncePerFrame = 3;
    #endregion

    #region Fields
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

        if (!ResolveWallOverlap(ref position, ref direction))
        {
            SetDirection(direction);
            SetPosition(position);
            return;
        }

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

            if (!ResolveWallOverlap(ref position, ref direction))
                break;
        }

        SetDirection(direction);
        SetPosition(position);
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

                Vector2 closestPoint = wall.ClosestPoint(position);
                Vector2 push = position - closestPoint;

                if (push.sqrMagnitude < 0.000001f)
                {
                    push = GetBoundsEscape(wall, position);
                }
                else
                {
                    float distance = push.magnitude;
                    float penetration = wallRadius + wallOffset - distance;

                    if (penetration <= 0f)
                        continue;

                    push = push.normalized * penetration;
                }

                if (push.sqrMagnitude < 0.000001f)
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

    private Vector2 GetBoundsEscape(
        Collider2D wall,
        Vector2 position)
    {
        Bounds bounds = wall.bounds;
        float padding = wallRadius + wallOffset;

        float left = position.x - bounds.min.x;
        float right = bounds.max.x - position.x;
        float bottom = position.y - bounds.min.y;
        float top = bounds.max.y - position.y;

        float minDistance = Mathf.Min(left, right, bottom, top);

        if (minDistance == left)
            return Vector2.left * (left + padding);

        if (minDistance == right)
            return Vector2.right * (right + padding);

        if (minDistance == bottom)
            return Vector2.down * (bottom + padding);

        return Vector2.up * (top + padding);
    }

    private void SetPosition(Vector2 position)
    {
        transform.position = new Vector3(
            position.x,
            position.y,
            transform.position.z
        );
    }

    private void CheckHit()
    {
        int count = Physics2D.OverlapCircle(
            transform.position,
            hitRadius,
            contactFilter,
            hitBuffer
        );

        if (count <= 0)
            return;

        for (int i = 0; i < count; ++i)
        {
            Collider2D hit = hitBuffer[i];

            Managers.Damage.ApplyDamage(
                hit.transform,
                Damage
            );
        }

        Release();
    }
}
