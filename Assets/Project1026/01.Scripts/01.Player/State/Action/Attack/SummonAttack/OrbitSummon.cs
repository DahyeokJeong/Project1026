using System.Collections.Generic;
using UnityEngine;

public class OrbitSummon : SummonBase
{
    #region Settings
    [Header("Orbit Settings")]
    [SerializeField] private float angularSpeed = 180f;
    [SerializeField, Min(0.01f)] private float orbitRadius = 1f;
    [SerializeField, Min(0.01f)] private float lifeTime = 5f;

    [Header("Projectile Settings")]
    [SerializeField] private WispProjectile projectilePrefab;

    [Header("Sprite Animation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField, Min(1f)] private float framesPerSecond = 8f;
    #endregion

    #region Constants
    private const float DetectionRadius = 3f;
    private const float BaseFireInterval = 1.2f;
    #endregion

    #region Fields
    private readonly List<Collider2D> searchBuffer = new(32);

    private Transform orbitCenter;
    private ProjectileAttackRuntimeData projectileData;
    private float remainingLifetime;
    private float fireCooldown;
    private float angle;
    #endregion

    protected override void OnSummonInitialized(float interval)
    {
        remainingLifetime = Mathf.Max(0.01f, lifeTime);
        fireCooldown = 0f;
        UpdateIdleSprite(0f);
    }

    public void SetOrbit(
        Transform center,
        Vector2 direction,
        ProjectileAttackRuntimeData data)
    {
        orbitCenter = center;
        projectileData = data;
        angle = Mathf.Atan2(direction.y, direction.x);
    }

    public void Despawn(ProjectileAttackRuntimeData data)
    {
        if (projectileData == data)
            Release();
    }

    private void Update()
    {
        if (!CanUpdateSummon())
            return;

        float deltaTime = Time.deltaTime;
        remainingLifetime -= deltaTime;

        if (remainingLifetime <= 0f || orbitCenter == null || projectileData == null)
        {
            Release();
            return;
        }

        UpdateIdleSprite(Mathf.Max(0.01f, lifeTime) - remainingLifetime);

        float attackSpeed = 1f + projectileData.AutoAttackSpeedBonusPercent / 100f;
        fireCooldown = Mathf.Max(0f, fireCooldown - deltaTime * attackSpeed);

        Move(deltaTime);
        if (fireCooldown > 0f)
            return;

        Transform target = FindTarget();
        if (target != null && TryFireVolley(target))
            fireCooldown = BaseFireInterval;
    }

    private void UpdateIdleSprite(float elapsedTime)
    {
        if (spriteRenderer == null || idleFrames == null || idleFrames.Length == 0)
            return;

        int frameIndex = Mathf.FloorToInt(elapsedTime * framesPerSecond)
            % idleFrames.Length;

        Sprite frame = idleFrames[frameIndex];
        if (frame != null && spriteRenderer.sprite != frame)
            spriteRenderer.sprite = frame;
    }

    private void Move(float deltaTime)
    {
        Vector2 offset = transform.position - orbitCenter.position;
        if (offset.sqrMagnitude > 0.000001f)
            angle = Mathf.Atan2(offset.y, offset.x);

        angle = Mathf.Repeat(
            angle + angularSpeed * Mathf.Deg2Rad * deltaTime,
            Mathf.PI * 2f);

        Vector3 orbitOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        transform.position = orbitCenter.position + orbitOffset * orbitRadius;
    }

    private Transform FindTarget()
    {
        Vector2 position = transform.position;
        Physics2D.OverlapCircle(position, DetectionRadius, EnemyFilter, searchBuffer);

        Transform nearest = null;
        float nearestDistance = DetectionRadius * DetectionRadius;

        foreach (Collider2D hit in searchBuffer)
        {
            if (hit == null || !Managers.Damage.TryGetTarget(hit.transform, out Transform candidate))
                continue;

            if (!candidate.gameObject.activeInHierarchy)
                continue;

            float distance = ((Vector2)candidate.position - position).sqrMagnitude;
            if (distance > nearestDistance)
                continue;

            nearest = candidate;
            nearestDistance = distance;
        }

        return nearest;
    }

    private bool TryFireVolley(Transform target)
    {
        if (projectilePrefab == null)
            return false;

        Vector2 direction = target.position - transform.position;
        if (direction == Vector2.zero)
            direction = Owner.LastMoveDirection;

        if (direction == Vector2.zero)
            direction = Vector2.down;

        float damage = CalculateDamage();
        bool fired = false;
        int count = projectileData.ProjectileCount;
        Vector2 side = Vector2.Perpendicular(direction.normalized);
        const float spacing = 0.4f;

        for (int i = 0; i < count; i++)
        {
            float offset = (i - (count - 1) * 0.5f) * spacing;
            Vector2 spawnPosition = (Vector2)transform.position + side * offset;
            GameObject instance = Managers.Pool.Pop(
                projectilePrefab.gameObject, spawnPosition, Quaternion.identity);

            if (instance == null)
                break;

            WispProjectile projectile = instance.GetComponent<WispProjectile>();
            projectile.SetDirection(direction);
            projectile.SetDamage(damage, 0f);
            fired = true;
        }

        return fired;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        orbitCenter = null;
        projectileData = null;
        searchBuffer.Clear();
    }
}
