using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material hitFlash;
    [SerializeField] private Material deathMaterial;

    [Header("Animation")]
    [SerializeField] private EnemyModel model;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private Sprite[] moveFrames;
    [SerializeField] private Sprite[] attackFrames;
    [SerializeField, Min(0.01f)] private float moveFrameDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float attackDuration = 0.18f;

    private Sprite idleSprite;
    private bool idleFlip;
    private Vector2 previousPosition;
    private float moveTime;
    private float movementHoldTime;
    private float attackTime;

    private static readonly int FadeId = Shader.PropertyToID("_Fade");

    private Material normalMaterial;
    private MaterialPropertyBlock deathProperties;

    public bool HasDeathEffect => deathMaterial != null;

    private void Awake()
    {
        idleSprite = spriteRenderer.sprite;
        idleFlip = spriteRenderer.flipX;
        normalMaterial = spriteRenderer.sharedMaterial;
        deathProperties = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
        moveTime = 0f;
        movementHoldTime = 0f;
        attackTime = 0f;
        spriteRenderer.sprite = idleSprite;
        spriteRenderer.flipX = idleFlip;

        if (attack != null && attackFrames != null && attackFrames.Length > 0)
            attack.AttackPerformed += OnAttackPerformed;
    }

    private void LateUpdate()
    {
        if (model == null)
            return;

        Vector2 position = transform.position;
        Vector2 movement = position - previousPosition;
        previousPosition = position;

        if (!Managers.BattleState.CanUpdate || model.IsDead)
            return;

        if (attackTime > 0f)
        {
            int frame = Mathf.Min(
                Mathf.FloorToInt((1f - attackTime / attackDuration) * attackFrames.Length),
                attackFrames.Length - 1);
            spriteRenderer.sprite = attackFrames[frame];
            attackTime -= Time.deltaTime;
            return;
        }

        if (movement.sqrMagnitude > 0.000001f)
        {
            movementHoldTime = Time.fixedDeltaTime * 2f;
        }
        else
        {
            movementHoldTime -= Time.deltaTime;
        }

        if (movementHoldTime <= 0f || moveFrames == null || moveFrames.Length == 0)
        {
            moveTime = 0f;
            spriteRenderer.sprite = idleSprite;
            return;
        }

        int moveFrame = Mathf.FloorToInt(moveTime / moveFrameDuration) % moveFrames.Length;
        spriteRenderer.sprite = moveFrames[moveFrame];
        moveTime += Time.deltaTime;
    }

    private void OnAttackPerformed(Vector2 direction)
    {
        UpdateFacing(direction.x);
        moveTime = 0f;
        movementHoldTime = 0f;
        attackTime = attackDuration;
        spriteRenderer.sprite = attackFrames[0];
    }

    public void SetMovementFacing(Vector2 direction)
    {
        if (attackTime > 0f)
            return;

        const float minimumHorizontalRatio = 0.2f;
        if (Mathf.Abs(direction.x) >= direction.magnitude * minimumHorizontalRatio)
            UpdateFacing(direction.x);
    }

    private void UpdateFacing(float horizontal)
    {
        if (Mathf.Abs(horizontal) > 0.001f)
            spriteRenderer.flipX = idleFlip ^ (horizontal < 0f);
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
        if (attack != null)
            attack.AttackPerformed -= OnAttackPerformed;

        spriteRenderer.SetPropertyBlock(null);
        spriteRenderer.sharedMaterial = normalMaterial;
    }
}