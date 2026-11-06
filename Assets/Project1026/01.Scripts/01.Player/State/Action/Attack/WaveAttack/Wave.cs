using System.Collections.Generic;
using UnityEngine;

public class Wave : WaveBase
{
    #region Settings
    [Header("Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private SpriteRenderer waveSprite;
    [SerializeField] private Vector2 maxRadius = new Vector2(4f, 2f);
    #endregion

    #region Fields
    private readonly List<Collider2D> hitBuffer = new(64);
    private readonly HashSet<Transform> hitTargets = new();
    private ContactFilter2D contactFilter;
    private Vector2 spriteSize;
    private Color initialColor;
    #endregion

    private void Awake()
    {
        if (waveSprite == null || waveSprite.sprite == null)
        {
            enabled = false;
            return;
        }

        spriteSize = waveSprite.sprite.bounds.size;
        initialColor = waveSprite.color;
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayer);
        contactFilter.useTriggers = true;
    }


    protected override void OnWaveInitialized()
    {
        hitTargets.Clear();
        hitBuffer.Clear();
        waveSprite.color = initialColor;
        waveSprite.transform.localScale = Vector3.zero;
    }

    protected override void UpdateWave(float progress)
    {
        Vector2 radius = new Vector2(Mathf.Max(0.01f, maxRadius.x), Mathf.Max(0.01f, maxRadius.y)) * progress;
        RefreshVisual(radius, progress);
        CheckHit(radius);
    }

    private void RefreshVisual(Vector2 radius, float progress)
    {
        float width = radius.x * 2f;
        float height = radius.y * 2f;
        waveSprite.transform.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, 1f);

        Color color = initialColor;
        color.a *= 1f - progress;
        waveSprite.color = color;
    }

    private void CheckHit(Vector2 radius)
    {
        if (radius.x <= 0f || radius.y <= 0f)
            return;

        Vector2 center = transform.position;
        float searchRadius = Mathf.Max(radius.x, radius.y);
        Physics2D.OverlapCircle(center, searchRadius, contactFilter, hitBuffer);

        foreach (Collider2D hit in hitBuffer)
        {
            if (hit == null)
                continue;

            if (!Managers.Damage.TryGetTarget(hit.transform, out Transform target))
                continue;

            if (!target.gameObject.activeInHierarchy)
                continue;

            Vector2 offset = (Vector2)hit.ClosestPoint(center) - center;
            float x = offset.x / radius.x;
            float y = offset.y / radius.y;

            if (x * x + y * y > 1f)
                continue;

            if (!hitTargets.Add(target))
                continue;

            Managers.Damage.ApplyDamage(target, Damage);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        hitTargets.Clear();
        hitBuffer.Clear();

        if (waveSprite != null)
            waveSprite.color = initialColor;
    }
}
