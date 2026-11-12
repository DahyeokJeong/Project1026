using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class WindKnifeAfterimage : MonoBehaviour
{
    [SerializeField] private Sprite[] streakSprites;
    [SerializeField, Min(0.01f)] private float streakLifetime = 0.19f;
    [SerializeField, Min(0.01f)] private float repeatInterval = 0.26f;
    [SerializeField, Min(0f)] private float stagger = 0.055f;
    [SerializeField, Range(0f, 1f)] private float speedRatio = 0.65f;

    private sealed class Streak
    {
        public SpriteRenderer Renderer;
        public float TimeUntilEmission;
        public float Age;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Velocity;
    }

    private SpriteRenderer blade;
    private Streak[] streaks;
    private Vector3 previousPosition;

    private void Awake()
    {
        blade = GetComponent<SpriteRenderer>();
        streaks = new Streak[streakSprites.Length];

        for (int i = 0; i < streaks.Length; i++)
        {
            var renderer = new GameObject("Wind Knife Streak", typeof(SpriteRenderer))
                .GetComponent<SpriteRenderer>();
            renderer.transform.SetParent(transform, false);
            renderer.sprite = streakSprites[i];
            renderer.sharedMaterial = blade.sharedMaterial;
            renderer.sortingLayerID = blade.sortingLayerID;
            renderer.sortingOrder = blade.sortingOrder - 1;
            renderer.enabled = false;
            streaks[i] = new Streak { Renderer = renderer };
        }
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
        for (int i = 0; i < streaks.Length; i++)
        {
            streaks[i].TimeUntilEmission = i * stagger;
            streaks[i].Renderer.enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f)
            return;

        Vector3 position = transform.position;
        float bladeSpeed = (position - previousPosition).magnitude / deltaTime;
        previousPosition = position;

        foreach (Streak streak in streaks)
        {
            streak.TimeUntilEmission -= deltaTime;
            if (streak.TimeUntilEmission <= 0f && bladeSpeed > 0f)
            {
                streak.TimeUntilEmission = Mathf.Max(repeatInterval, streakLifetime);
                streak.Age = 0f;
                streak.Position = position;
                streak.Rotation = transform.rotation;
                streak.Velocity = transform.right * (bladeSpeed * speedRatio);
                streak.Renderer.enabled = true;
            }
            else if (streak.Renderer.enabled)
            {
                streak.Age += deltaTime;
                streak.Position += streak.Velocity * deltaTime;
            }

            if (!streak.Renderer.enabled)
                continue;

            float progress = Mathf.Clamp01(streak.Age / streakLifetime);
            Color color = blade.color;
            color.a *= (1f - progress) * 0.9f;
            streak.Renderer.color = color;
            streak.Renderer.transform.SetPositionAndRotation(streak.Position, streak.Rotation);
            streak.Renderer.enabled = progress < 1f;
        }
    }
}
