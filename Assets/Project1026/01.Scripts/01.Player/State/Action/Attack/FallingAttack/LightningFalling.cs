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
    [SerializeField] private SpriteRenderer[] afterimageBranches = System.Array.Empty<SpriteRenderer>();
    [SerializeField, Min(0.01f)] private float afterimageDuration = 0.26f;
    [SerializeField] private bool useGroundImpactVisual = true;
    [SerializeField] private ImpactParticleBurst impactParticles;
    #endregion

    #region Fields
    private readonly Collider2D[] hitBuffer = new Collider2D[64];
    private readonly HashSet<Transform> hitTargets = new();
    private ContactFilter2D contactFilter;
    private Vector3 initialVisualScale;
    private Vector3[] afterimageStartPositions;
    private float sizeMultiplier = 1f;
    private GroundImpactVisual groundImpactVisual;
    #endregion

    protected override bool MoveVisualDuringFall => false;
    protected override float TimeToLand => StrikeFrameDuration * 2f;
    protected override float LandingDuration => StrikeFrameDuration + Mathf.Max(0.01f, afterimageDuration);

    private float StrikeFrameDuration => FallDuration / 3f;

    private void Awake()
    {
        if (visualRenderer != null)
            initialVisualScale = visualRenderer.transform.localScale;

        afterimageStartPositions = new Vector3[afterimageBranches.Length];
        for (int i = 0; i < afterimageBranches.Length; i++)
        {
            if (afterimageBranches[i] != null)
                afterimageStartPositions[i] = afterimageBranches[i].transform.localPosition;
        }

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
        if (visualRenderer != null)
            visualRenderer.enabled = true;

        foreach (SpriteRenderer branch in afterimageBranches)
        {
            if (branch != null)
                branch.enabled = false;
        }

        groundImpactVisual?.Hide();
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
        if (visualRenderer != null && fallFrames != null && fallFrames.Length > 1 && fallFrames[1] != null)
        {
            Sprite fullStrike = fallFrames[1];
            Transform visual = visualRenderer.transform;
            float strikeHeight = fullStrike.rect.height / fullStrike.pixelsPerUnit;
            Vector3 top = transform.position + visual.TransformVector(Vector3.up * strikeHeight);
            visual.position = top - GetStrikeOffset(1f);
        }

        ShowGroundImpact(new Color(0.55f, 0.75f, 1f, Mathf.Lerp(0.12f, 0.35f, progress)));
    }

    protected override void OnLandingProgress(float progress)
    {
        float elapsed = progress * LandingDuration;
        ShowGroundImpact(new Color(0.85f, 0.93f, 1f, 1f - progress));

        if (visualRenderer == null)
            return;

        if (elapsed < StrikeFrameDuration)
        {
            SetFrame(2);
            visualRenderer.transform.position = transform.position - GetStrikeOffset(0.13f);
            return;
        }

        ShowAfterimage(elapsed - StrikeFrameDuration);
    }

    private void ShowAfterimage(float elapsed)
    {
        float progress = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, afterimageDuration));
        visualRenderer.enabled = false;
        visualRenderer.transform.position = transform.position;

        const float spreadDistance = 0.42f;
        float remaining = 1f - progress;
        float distance = spreadDistance * (1f - remaining * remaining * remaining);

        for (int i = 0; i < afterimageBranches.Length; i++)
        {
            SpriteRenderer branch = afterimageBranches[i];
            if (branch == null)
                continue;

            Vector3 startPosition = afterimageStartPositions[i];
            Vector3 centerOffset = branch.sprite != null ? branch.sprite.bounds.center : Vector3.zero;
            Vector3 center = startPosition + centerOffset;
            Vector3 direction = new Vector3(Mathf.Sign(center.x), Mathf.Sign(center.y), 0f).normalized;
            Vector3 position = startPosition + direction * distance;

            float flashInterval = 0.045f + 0.008f * i;
            float cycle = (elapsed + 0.009f * i) / flashInterval;
            float flash = 1f - Mathf.Clamp01(Mathf.Repeat(cycle, 1f) * 4f);
            Color color = visualRenderer.color;
            color.a *= remaining * Mathf.Lerp(0.48f, 1f, flash);

            branch.transform.localPosition = position;
            branch.color = color;
            branch.enabled = true;
        }
    }

    private Vector3 GetStrikeOffset(float normalizedHeight)
    {
        Sprite frame = visualRenderer.sprite;
        if (frame == null)
            return Vector3.zero;

        Vector2 strikePixel = Vector2.Scale(frame.rect.size, new Vector2(0.5f, normalizedHeight));
        Vector2 offset = (strikePixel - frame.pivot) / frame.pixelsPerUnit;
        return visualRenderer.transform.TransformVector(offset);
    }

    private void ShowGroundImpact(Color color)
    {
        if (!useGroundImpactVisual || visualRenderer == null)
        {
            groundImpactVisual?.Hide();
            return;
        }

        groundImpactVisual ??= new GroundImpactVisual(transform, visualRenderer);
        groundImpactVisual.Show(Vector2.one * (hitRadius * sizeMultiplier), color);
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
        impactParticles?.Play(Vector2.one * (hitRadius * sizeMultiplier));

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
