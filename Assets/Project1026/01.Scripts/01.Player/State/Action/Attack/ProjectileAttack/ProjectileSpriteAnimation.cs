using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class ProjectileSpriteAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(0.01f)] private float frameDuration = 0.06f;

    private SpriteRenderer spriteRenderer;
    private float elapsed;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        elapsed = 0f;
        if (frames != null && frames.Length > 0)
            spriteRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null || frames.Length < 2 || !Managers.BattleState.CanUpdate)
            return;

        float duration = Mathf.Max(0.01f, frameDuration);
        elapsed = Mathf.Repeat(elapsed + Time.deltaTime, duration * frames.Length);
        int index = Mathf.Min(Mathf.FloorToInt(elapsed / duration), frames.Length - 1);
        Sprite frame = frames[index];
        if (spriteRenderer.sprite != frame)
            spriteRenderer.sprite = frame;
    }
}
