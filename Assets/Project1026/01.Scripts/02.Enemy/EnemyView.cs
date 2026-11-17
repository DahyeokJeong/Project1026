using UnityEngine;

public class EnemyView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material hitFlash;
    [SerializeField] private Material deathMaterial;
    [SerializeField] private EliteSpriteAtlas eliteSpriteAtlas;

    [Header("Animation")]
    [SerializeField] private EnemyModel model;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField, Min(0.01f)] private float idleFrameDuration = 0.12f;
    [SerializeField] private Sprite[] moveFrames;
    [SerializeField] private Sprite[] attackFrames;
    [SerializeField] private Sprite[] deadFrames;
    [SerializeField, Min(0.01f)] private float deadFrameDuration = 0.06f;
    [SerializeField, Min(0.01f)] private float moveFrameDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float attackDuration = 0.18f;

    private Sprite idleSprite;
    private bool idleFlip;
    private Vector2 previousPosition;
    private float idleTime;
    private float moveTime;
    private float movementHoldTime;
    private float attackTime;
    private float deadTime;
    private bool playingDeath;

    public bool IsDeathAnimationComplete { get; private set; }

    private static readonly int FadeId = Shader.PropertyToID("_Fade");

    private Material normalMaterial;
    private Sprite currentFrame;
    private bool isElite;
    private MaterialPropertyBlock deathProperties;

    public bool HasDeathEffect => deathMaterial != null;

    private void Awake()
    {
        idleSprite = idleFrames != null && idleFrames.Length > 0 ? idleFrames[0] : spriteRenderer.sprite;
        idleFlip = spriteRenderer.flipX;
        normalMaterial = spriteRenderer.sharedMaterial;
        deathProperties = new MaterialPropertyBlock();
    }

    private void OnEnable()
    {
        previousPosition = transform.position;
        idleTime = 0f;
        moveTime = 0f;
        movementHoldTime = 0f;
        attackTime = 0f;
        deadTime = 0f;
        playingDeath = false;
        IsDeathAnimationComplete = false;
        SetFrame(idleSprite);
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

        if (!Managers.BattleState.CanUpdate)
            return;

        if (playingDeath)
        {
            deadTime += Time.deltaTime;
            int frame = Mathf.FloorToInt(deadTime / Mathf.Max(0.01f, deadFrameDuration));
            SetFrame(deadFrames[Mathf.Min(frame, deadFrames.Length - 1)]);
            IsDeathAnimationComplete = frame >= deadFrames.Length;
            playingDeath = !IsDeathAnimationComplete;
            return;
        }

        if (model.IsDead)
            return;

        if (attackTime > 0f)
        {
            int frame = Mathf.Min(
                Mathf.FloorToInt((1f - attackTime / Mathf.Max(0.01f, attackDuration)) * attackFrames.Length),
                attackFrames.Length - 1);
            SetFrame(attackFrames[frame]);
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
            if (idleFrames != null && idleFrames.Length > 0)
            {
                int idleFrame = Mathf.FloorToInt(idleTime / Mathf.Max(0.01f, idleFrameDuration)) % idleFrames.Length;
                SetFrame(idleFrames[idleFrame]);
                idleTime += Time.deltaTime;
            }
            else
            {
                SetFrame(idleSprite);
            }
            return;
        }

        idleTime = 0f;
        int moveFrame = Mathf.FloorToInt(moveTime / Mathf.Max(0.01f, moveFrameDuration)) % moveFrames.Length;
        SetFrame(moveFrames[moveFrame]);
        moveTime += Time.deltaTime;
    }

    private void OnAttackPerformed(Vector2 direction)
    {
        if (model != null && model.IsDead)
            return;

        UpdateFacing(direction.x);
        moveTime = 0f;
        movementHoldTime = 0f;
        idleTime = 0f;
        attackTime = Mathf.Max(0.01f, attackDuration);
        SetFrame(attackFrames[0]);
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

    public void SetElite(bool elite)
    {
        isElite = elite;
        ApplyFrame();
    }

    private void SetFrame(Sprite frame)
    {
        if (currentFrame == frame)
            return;

        currentFrame = frame;
        ApplyFrame();
    }

    private void ApplyFrame()
    {
        spriteRenderer.sprite = isElite && eliteSpriteAtlas != null
            ? eliteSpriteAtlas.GetOutlined(currentFrame)
            : currentFrame;
    }

    public void SetHitFlash(bool active)
    {
        spriteRenderer.sharedMaterial = active ? hitFlash : normalMaterial;
    }

    public void PlayDeathAnimation()
    {
        SetHitFlash(false);
        attackTime = 0f;
        deadTime = 0f;
        playingDeath = deadFrames != null && deadFrames.Length > 0;
        IsDeathAnimationComplete = !playingDeath;
        if (playingDeath)
            SetFrame(deadFrames[0]);
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

        isElite = false;
        currentFrame = null;
        spriteRenderer.sprite = idleSprite;
        spriteRenderer.SetPropertyBlock(null);
        spriteRenderer.sharedMaterial = normalMaterial;
    }
}
