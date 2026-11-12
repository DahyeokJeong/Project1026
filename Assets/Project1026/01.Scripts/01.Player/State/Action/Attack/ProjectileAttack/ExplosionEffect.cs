using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    #region Settings
    [Header("Settings")]
    [SerializeField, Min(0.01f)] private float lifeTime = 0.42f;
    [SerializeField] private bool useGroundImpactVisual = true;
    [SerializeField] private ImpactParticleBurst impactParticles;
    #endregion

    #region Fields
    private float timer;
    private SpriteRenderer visualRenderer;
    private Color initialColor;
    private GroundImpactVisual groundImpactVisual;
    private Vector2 impactRadius;
    #endregion

    private void Awake()
    {
        visualRenderer = GetComponent<SpriteRenderer>();
        if (visualRenderer != null)
            initialColor = visualRenderer.color;
    }

    public void SetSize(Vector2 diameter)
    {
        if (visualRenderer == null)
            return;

        Vector3 currentSize = visualRenderer.bounds.size;
        if (currentSize.x <= 0f || currentSize.y <= 0f)
            return;

        Vector3 scale = transform.localScale;
        scale.x *= diameter.x / currentSize.x;
        scale.y *= diameter.y / currentSize.y;
        transform.localScale = scale;
        impactRadius = diameter * 0.5f;
        UpdateGroundImpact(0f);
        impactParticles?.Play(impactRadius);
    }

    private void OnEnable()
    {
        timer = lifeTime;
        impactRadius = Vector2.zero;
        groundImpactVisual?.Hide();
        if (visualRenderer != null)
            visualRenderer.color = initialColor;
    }

    private void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        timer -= Time.deltaTime;
        float progress = 1f - Mathf.Clamp01(timer / lifeTime);
        if (visualRenderer != null)
        {
            Color color = initialColor;
            color.a *= 1f - Mathf.InverseLerp(0.18f / 0.42f, 0.31f / 0.42f, progress);
            visualRenderer.color = color;
        }

        UpdateGroundImpact(progress);

        if (timer <= 0f)
        {
            Managers.Pool.Push(gameObject);
        }
    }

    private void UpdateGroundImpact(float progress)
    {
        if (!useGroundImpactVisual || visualRenderer == null || impactRadius == Vector2.zero)
        {
            groundImpactVisual?.Hide();
            return;
        }

        groundImpactVisual ??= new GroundImpactVisual(transform, visualRenderer);
        Color color = Color.Lerp(new Color(1f, 0.93f, 0.68f), new Color(1f, 0.34f, 0.08f), progress);
        color.a = 1f - Mathf.InverseLerp(0.18f / 0.42f, 1f, progress);
        float expansion = Mathf.InverseLerp(0f, 0.08f / 0.42f, progress);
        groundImpactVisual.Show(impactRadius * Mathf.Lerp(0.4f, 1f, expansion), color);
    }
}
