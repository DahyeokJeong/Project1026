using System.Collections.Generic;
using UnityEngine;

public class Wave : WaveBase
{
    #region Settings
    [Header("Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private SpriteRenderer waveSprite;
    [SerializeField] private Vector2 maxRadius = new Vector2(4f, 2f);

    [Header("Sprite Animation")]
    [SerializeField] private Sprite[] waveFrames;
    #endregion

    #region Fields
    private readonly List<Collider2D> hitBuffer = new(64);
    private readonly HashSet<Transform> hitTargets = new();

    private ContactFilter2D contactFilter;
    private Vector2 spriteSize;
    #endregion

    private void Awake()
    {
        if (waveSprite == null || waveSprite.sprite == null)
        {
            enabled = false;
            return;
        }

        spriteSize = waveSprite.sprite.bounds.size;

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayer);
        contactFilter.useTriggers = true;
    }

    protected override void OnWaveInitialized()
    {
        hitTargets.Clear();
        hitBuffer.Clear();

        UpdateSpriteFrame(0f);
        waveSprite.transform.localScale = Vector3.zero;
    }

    protected override void UpdateWave(float progress)
    {
        Vector2 radius = new Vector2(
            Mathf.Max(0.01f, maxRadius.x),
            Mathf.Max(0.01f, maxRadius.y)) * progress;

        UpdateSpriteFrame(progress);
        RefreshVisual(radius);
        CheckHit(radius);
    }

    private void UpdateSpriteFrame(float progress)
    {
        if (waveFrames == null || waveFrames.Length == 0)
            return;

        int frameIndex = Mathf.Min(
            Mathf.FloorToInt(progress * waveFrames.Length),
            waveFrames.Length - 1);

        Sprite frame = waveFrames[frameIndex];
        if (frame == null || waveSprite.sprite == frame)
            return;

        waveSprite.sprite = frame;
        spriteSize = frame.bounds.size;
    }

    private void RefreshVisual(Vector2 radius)
    {
        float width = radius.x * 2f;
        float height = radius.y * 2f;

        waveSprite.transform.localScale = new Vector3(
            width / spriteSize.x,
            height / spriteSize.y,
            1f);
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
    }
}