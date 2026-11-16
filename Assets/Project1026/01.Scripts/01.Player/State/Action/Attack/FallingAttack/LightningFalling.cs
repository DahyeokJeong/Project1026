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
    [SerializeField] private float[] frameDurations;
    [SerializeField] private bool useGroundImpactVisual = true;
    [SerializeField] private ImpactParticleBurst impactParticles;
    #endregion

    #region Fields
    private readonly Collider2D[] hitBuffer = new Collider2D[64];
    private readonly HashSet<Transform> hitTargets = new();
    private ContactFilter2D contactFilter;
    private Vector3 initialVisualScale;
    private float sizeMultiplier = 1f;
    private float timeToLand;
    private float landingDuration;
    private GroundImpactVisual groundImpactVisual;

    private const int LandingFrame = 4;
    private const float GroundAnchor = 0.1f;
    #endregion

    protected override bool MoveVisualDuringFall => false;
    protected override float TimeToLand => timeToLand;
    protected override float LandingDuration => landingDuration;
    private void Awake()
    {
        if (visualRenderer != null)
            initialVisualScale = visualRenderer.transform.localScale;

        for (int i = 0; i < frameDurations.Length; i++)
        {
            if (i < LandingFrame)
                timeToLand += frameDurations[i];
            else
                landingDuration += frameDurations[i];
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
        PlayFrames(progress * TimeToLand, 0, LandingFrame);
        ShowGroundImpact(new Color(0.55f, 0.75f, 1f, Mathf.Lerp(0.12f, 0.35f, progress)));
    }

    protected override void OnLandingProgress(float progress)
    {
        PlayFrames(progress * LandingDuration, LandingFrame, fallFrames.Length);
        ShowGroundImpact(new Color(0.85f, 0.93f, 1f, 1f - progress));
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

    private void PlayFrames(float elapsed, int first, int end)
    {
        int index = first;
        while (index < end - 1 && elapsed >= frameDurations[index])
        {
            elapsed -= frameDurations[index];
            index++;
        }

        SetFrame(index);
    }

    private void SetFrame(int index)
    {
        Sprite frame = fallFrames[index];
        visualRenderer.sprite = frame;

        Vector2 groundPixel = Vector2.Scale(frame.rect.size, new Vector2(0.5f, GroundAnchor));
        Vector2 offset = (groundPixel - frame.pivot) / frame.pixelsPerUnit;
        Transform visual = visualRenderer.transform;
        visual.position = transform.position - visual.TransformVector(offset);
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
