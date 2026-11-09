using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material hitFlash;
    [SerializeField] private Material deathMaterial;

    private static readonly int FadeId = Shader.PropertyToID("_Fade");

    private Material normalMaterial;
    private MaterialPropertyBlock deathProperties;

    public bool HasDeathEffect => deathMaterial != null;

    private void Awake()
    {
        normalMaterial = spriteRenderer.sharedMaterial;
        deathProperties = new MaterialPropertyBlock();
    }

    public void SetHitFlash(bool active)
    {
        spriteRenderer.sharedMaterial = active ? hitFlash : normalMaterial;
    }

    public void BeginDeathEffect()
    {
        spriteRenderer.sharedMaterial = deathMaterial;
        SetDeathFade(0f);
    }

    public void SetDeathFade(float progress)
    {
        Bounds bounds = spriteRenderer.localBounds;
        float fade = Mathf.Lerp(bounds.max.y, bounds.min.y - 1f, progress);

        deathProperties.SetFloat(FadeId, fade);
        spriteRenderer.SetPropertyBlock(deathProperties);
    }

    private void OnDisable()
    {
        spriteRenderer.SetPropertyBlock(null);
        spriteRenderer.sharedMaterial = normalMaterial;
    }
}