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
    [SerializeField] private bool useGroundImpactVisual = true;
    #endregion

    #region Fields
    private readonly Collider2D[] hitBuffer = new Collider2D[64];
    private readonly HashSet<Transform> hitTargets = new();
    private ContactFilter2D contactFilter;
    private Vector3 initialVisualScale;
    private float sizeMultiplier = 1f;
    private GroundImpactVisual groundImpactVisual;
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
        bool firstFrame = progress < 0.5f;
        SetFrame(firstFrame ? 0 : 1);
        if (visualRenderer != null)
            visualRenderer.transform.position -= GetStrikeOffset(firstFrame ? 0f : 0.08f);

        ShowGroundImpact(1f, new Color(0.55f, 0.75f, 1f, Mathf.Lerp(0.12f, 0.35f, progress)));
    }

    protected override void OnLandingProgress(float progress)
    {
        bool impactFrame = progress < 0.5f;
        SetFrame(impactFrame ? 2 : 3);
        ShowGroundImpact(Mathf.Lerp(0.55f, 1f, progress * 2f), new Color(0.85f, 0.93f, 1f, 1f - progress));

        if (visualRenderer == null || fallFrames == null || fallFrames.Length <= 2 || fallFrames[2] == null)
            return;

        // Use the impact frame for both landing frames so the fading frame retains its proportions.
        Transform visual = visualRenderer.transform;
        float parentScale = visual.parent != null ? Mathf.Abs(visual.parent.lossyScale.x) : 1f;
        float impactWidth = fallFrames[2].bounds.size.x * Mathf.Abs(initialVisualScale.x) * parentScale;
        if (impactWidth <= 0f)
            return;

        float scale = 2f * hitRadius * sizeMultiplier / impactWidth;
        visual.localScale = new Vector3(initialVisualScale.x * scale, initialVisualScale.y * scale, initialVisualScale.z);
        visual.position = transform.position - GetStrikeOffset(impactFrame ? 0.13f : 0.5f);
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

    private void ShowGroundImpact(float scale, Color color)
    {
        if (!useGroundImpactVisual || visualRenderer == null)
        {
            groundImpactVisual?.Hide();
            return;
        }

        groundImpactVisual ??= new GroundImpactVisual(transform, visualRenderer);
        groundImpactVisual.Show(Vector2.one * (hitRadius * sizeMultiplier * scale), color);
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
