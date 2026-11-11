using System.Collections.Generic;
using UnityEngine;

public class LightningFalling : FallingBase
{
    #region Settings
    [Header("Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0.01f)] private float hitRadius = 1f;

    [Header("Sprite Animation")]
    [SerializeField] private SpriteRenderer visualRenderer;
    [SerializeField] private Sprite[] fallFrames;
    [SerializeField, Min(0.01f)] private float landingFrameDuration = 0.1f;
    #endregion

    #region Fields
    private readonly Collider2D[] hitBuffer = new Collider2D[64];
    private readonly HashSet<Transform> hitTargets = new();
    private ContactFilter2D contactFilter;
    private Vector3 initialVisualScale;
    private float sizeMultiplier = 1f;
    #endregion

    protected override float LandingDuration => Mathf.Max(0.01f, landingFrameDuration) * 2f;

    private void Awake()
    {
        if (visualRenderer != null)
            initialVisualScale = visualRenderer.transform.localScale;

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayer);
        contactFilter.useTriggers = true;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        hitTargets.Clear();
        SetSizeBonus(0f);
        SetFrame(0);
    }

    public void SetSizeBonus(float sizeBonus)
    {
        sizeMultiplier = 1f + sizeBonus;

        if (visualRenderer != null)
        {
            visualRenderer.transform.localScale = new Vector3(
                initialVisualScale.x * sizeMultiplier,
                initialVisualScale.y * sizeMultiplier,
                initialVisualScale.z);
        }
    }

    protected override void OnFallProgress(float progress)
    {
        SetFrame(progress < 0.5f ? 0 : 1);
    }

    protected override void OnLandingProgress(float progress)
    {
        SetFrame(progress < 0.5f ? 2 : 3);
    }

    private void SetFrame(int index)
    {
        if (visualRenderer == null || fallFrames == null || index >= fallFrames.Length)
            return;

        Sprite frame = fallFrames[index];
        if (frame != null && visualRenderer.sprite != frame)
            visualRenderer.sprite = frame;
    }

    protected override void OnLanded()
    {
        hitTargets.Clear();

        int count = Physics2D.OverlapCircle(
            transform.position, hitRadius * sizeMultiplier, contactFilter, hitBuffer);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = hitBuffer[i];

            if (hit == null)
                continue;

            if (!Managers.Damage.TryGetTarget(hit.transform, out Transform target))
                continue;

            if (!target.gameObject.activeInHierarchy)
                continue;

            if (!hitTargets.Add(target))
                continue;

            Managers.Damage.ApplyDamage(target, Damage);
        }
    }
}