using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class WindKnifeParticleTrail : MonoBehaviour
{
    [SerializeField] private Vector2 emitterOffset = new(-0.55f, 0f);
    [SerializeField, Min(0f)] private float particlesPerUnit = 8f;

    private ParticleSystem trail;

    private void Awake()
    {
        SpriteRenderer blade = GetComponent<SpriteRenderer>();
        var emitter = new GameObject("Wind Knife Particles");
        emitter.SetActive(false);
        emitter.transform.SetParent(transform, false);
        emitter.transform.localPosition = emitterOffset;

        trail = emitter.AddComponent<ParticleSystem>();

        ParticleSystem.MainModule main = trail.main;
        main.playOnAwake = false;
        main.loop = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.24f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.12f, 0.45f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.045f, 0.095f);
        main.startColor = new Color(0.47f, 1f, 0.82f, 0.8f);
        main.maxParticles = 20;

        ParticleSystem.EmissionModule emission = trail.emission;
        emission.rateOverTime = 0f;
        emission.rateOverDistance = particlesPerUnit;

        ParticleSystem.ShapeModule shape = trail.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.13f;

        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = trail.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(fade);

        var renderer = emitter.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = blade.sharedMaterial;
        renderer.sortingLayerID = blade.sortingLayerID;
        renderer.sortingOrder = blade.sortingOrder - 2;
        emitter.SetActive(true);
    }

    private void Update()
    {
        if (Managers.BattleState.CanUpdate)
        {
            if (!trail.isPlaying)
                trail.Play(false);
        }
        else if (trail.isPlaying)
        {
            trail.Pause(false);
        }
    }

    private void OnDisable()
    {
        trail.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
