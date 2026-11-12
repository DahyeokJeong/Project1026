using UnityEngine;

public sealed class ImpactParticleBurst : MonoBehaviour
{
    [SerializeField] private SpriteRenderer referenceRenderer;
    [SerializeField] private Material particleMaterial;
    [SerializeField, Range(0f, 1f)] private float spawnRadiusFraction = 0.65f;
    [SerializeField, Min(1)] private int particleCount = 16;
    [SerializeField] private Color lightColor = Color.white;
    [SerializeField] private Color accentColor = Color.cyan;
    [SerializeField] private Vector2 lifetime = new(0.08f, 0.24f);
    [SerializeField] private Vector2 particleSize = new(0.055f, 0.14f);
    [SerializeField, Min(0f)] private float spreadSpeed = 3f;

    private ParticleSystem particles;

    private void Awake()
    {
        var emitter = new GameObject("Impact Particles");
        emitter.SetActive(false);
        emitter.transform.SetParent(transform, false);
        particles = emitter.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = particles.main;
        main.playOnAwake = false;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = particleCount;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = false;
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = false;

        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fade);

        var renderer = emitter.GetComponent<ParticleSystemRenderer>();
        if (particleMaterial != null)
            renderer.sharedMaterial = particleMaterial;

        if (referenceRenderer != null)
        {
            renderer.sortingLayerID = referenceRenderer.sortingLayerID;
            renderer.sortingOrder = referenceRenderer.sortingOrder + 2;
        }

        emitter.SetActive(true);
    }

    public void Play(Vector2 radius)
    {
        if (!isActiveAndEnabled)
            return;

        particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.Play(false);

        for (int i = 0; i < particleCount; i++)
        {
            float angle = i * (2f * Mathf.PI / particleCount);
            float variation = Mathf.Repeat(i * 0.618f, 1f);
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 radial = Vector2.Scale(direction, radius);
            Vector2 startOffset = radial * Mathf.Lerp(spawnRadiusFraction * 0.8f, spawnRadiusFraction, variation);
            Vector2 velocity = radial * (spreadSpeed * Mathf.Lerp(0.55f, 1f, variation));
            var particle = new ParticleSystem.EmitParams
            {
                position = transform.position + (Vector3)startOffset,
                velocity = velocity,
                startLifetime = Mathf.Lerp(lifetime.x, lifetime.y, variation),
                startSize = Mathf.Lerp(particleSize.x, particleSize.y, variation),
                startColor = Color.Lerp(lightColor, accentColor, variation)
            };
            particles.Emit(particle, 1);
        }
    }

    private void Update()
    {
        if (!Managers.BattleState.CanUpdate && particles.isPlaying)
            particles.Pause(false);
        else if (Managers.BattleState.CanUpdate && particles.isPaused)
            particles.Play(false);
    }

    private void OnDisable()
    {
        if (particles != null)
            particles.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
