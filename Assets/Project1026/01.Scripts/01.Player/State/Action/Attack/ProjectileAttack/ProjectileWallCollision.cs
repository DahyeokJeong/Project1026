using System.Collections.Generic;
using UnityEngine;

public class ProjectileWallCollision : MonoBehaviour
{
    #region Settings
    [SerializeField] private LayerMask wallLayer;
    [SerializeField, Min(0.01f)] private float wallRadius = 0.1f;
    [SerializeField, Min(0.001f)] private float wallOffset = 0.02f;
    [SerializeField, Min(1)] private int maxCollisionsPerFrame = 3;
    #endregion

    #region Fields
    private readonly List<RaycastHit2D> hits = new();
    private readonly List<Collider2D> walls = new();
    private ContactFilter2D wallFilter;
    #endregion

    private void Awake()
    {
        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(wallLayer);
        wallFilter.useTriggers = false;
    }

    public bool TryMove(ref Vector2 pos, ref Vector2 dir, float dis, ref int remainingBounces)
    {
        dir.Normalize();
        float remainingDis = Mathf.Max(0f, dis);

        if (!TryResolveOverlap(ref pos))
            return false;

        for (int i = 0; i < maxCollisionsPerFrame; ++i)
        {
            if (remainingDis <= 0f)
                return true;

            if (!TryFindWall(pos, dir, remainingDis, out RaycastHit2D hit))
            {
                pos += dir * remainingDis;
                return true;
            }

            if (!TryBounce(hit, ref pos, ref dir, ref remainingDis, ref remainingBounces))
                return false;
        }

        return true;
    }

    private bool TryFindWall(Vector2 pos, Vector2 dir, float dis, out RaycastHit2D nearestHit)
    {
        Physics2D.CircleCast(pos, wallRadius, dir, wallFilter, hits, dis);

        nearestHit = default;
        float nearestDis = float.PositiveInfinity;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.distance >= nearestDis)
                continue;

            nearestHit = hit;
            nearestDis = hit.distance;
        }

        return nearestHit.collider != null;
    }

    private bool TryBounce(RaycastHit2D hit, ref Vector2 pos, ref Vector2 dir, ref float remainingDis, ref int remainingBounces)
    {
        if (hit.distance <= 0f)
            return false;

        pos += dir * Mathf.Max(0f, hit.distance - wallOffset);

        if (remainingBounces <= 0)
            return false;

        remainingBounces--;
        remainingDis = Mathf.Max(0f, remainingDis - hit.distance);
        dir = Vector2.Reflect(dir, hit.normal).normalized;
        pos += hit.normal * wallOffset;

        return TryResolveOverlap(ref pos);
    }

    private bool TryResolveOverlap(ref Vector2 pos)
    {
        for (int i = 0; i < 8; ++i)
        {
            if (Physics2D.OverlapCircle(pos, wallRadius, wallFilter, walls) == 0)
                return true;

            bool moved = false;

            foreach (Collider2D wall in walls)
            {
                if (wall == null)
                    continue;

                Vector2 push = GetSeparation(wall, pos);

                if (push.sqrMagnitude <= 0.000001f)
                    continue;

                pos += push;
                moved = true;
            }

            if (!moved)
                return false;
        }

        return Physics2D.OverlapCircle(pos, wallRadius, wallFilter, walls) == 0;
    }

    private Vector2 GetSeparation(Collider2D wall, Vector2 pos)
    {
        if (wall is BoxCollider2D box)
            return GetBoxSeparation(box, pos);

        Vector2 diff = pos - wall.ClosestPoint(pos);
        float dis = diff.magnitude;

        if (dis <= 0.0001f)
            return Vector2.zero;

        float penetration = Mathf.Max(0f, wallRadius + wallOffset - dis);
        return diff.normalized * penetration;
    }

    private Vector2 GetBoxSeparation(BoxCollider2D box, Vector2 pos)
    {
        Transform wallTransform = box.transform;
        Vector2 localPos = wallTransform.InverseTransformPoint(pos);
        Vector2 halfSize = box.size * 0.5f;
        Vector2 min = box.offset - halfSize;
        Vector2 max = box.offset + halfSize;

        Vector2 closest = new Vector2(Mathf.Clamp(localPos.x, min.x, max.x), Mathf.Clamp(localPos.y, min.y, max.y));
        Vector2 diff = localPos - closest;

        float radius = wallRadius + wallOffset;
        Vector3 scale = wallTransform.lossyScale;
        float scaleX = Mathf.Abs(scale.x);
        float scaleY = Mathf.Abs(scale.y);

        if (scaleX <= 0.0001f || scaleY <= 0.0001f)
            return Vector2.zero;

        if (diff.sqrMagnitude > 0.000001f)
        {
            Vector2 worldDiff = wallTransform.TransformVector(diff);
            float dis = worldDiff.magnitude;

            if (dis >= radius)
                return Vector2.zero;

            return worldDiff.normalized * (radius - dis);
        }

        float left = (localPos.x - min.x) * scaleX;
        float right = (max.x - localPos.x) * scaleX;
        float bottom = (localPos.y - min.y) * scaleY;
        float top = (max.y - localPos.y) * scaleY;
        float minDis = Mathf.Min(left, right, bottom, top);

        Vector2 normal;

        if (minDis == left)
            normal = Vector2.left;
        else if (minDis == right)
            normal = Vector2.right;
        else if (minDis == bottom)
            normal = Vector2.down;
        else
            normal = Vector2.up;

        Vector2 worldNormal = wallTransform.TransformVector(normal);
        return worldNormal.normalized * (minDis + radius);
    }
}