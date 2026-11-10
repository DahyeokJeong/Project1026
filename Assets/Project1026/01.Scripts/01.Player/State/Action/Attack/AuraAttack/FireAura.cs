using UnityEngine;

public class FireAura : AuraBase
{
    #region LayerMask
    [SerializeField] private LayerMask enemyLayer;
    #endregion

    #region Aura Settings
    [Header("Aura Settings")]
    [SerializeField] private CapsuleCollider2D hitArea;
    [SerializeField] private Vector2 positionOffset;
    [SerializeField, Min(0.01f)] private float duration = 6f;
    #endregion

    #region Fields
    private readonly Collider2D[] hitBuffer = new Collider2D[32];

    private Vector3 initialScale;
    private ContactFilter2D contactFilter;
    private float sizeMultiplier = 1f;
    private float tickTimer;
    private float remainingDuration;
    #endregion

    private void Awake()
    {
        initialScale = transform.localScale;

        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayer);
        contactFilter.useTriggers = true;
    }

    protected override void OnAttackInitialized()
    {
        tickTimer = TickInterval;
        remainingDuration = duration;
        RefreshSize();
    }

    protected override void Update()
    {
        if (Owner == null || !Owner.gameObject.activeInHierarchy || !Managers.BattleState.CanUpdate)
            return;

        RefreshSize();
        base.Update();

        float elapsed = Mathf.Min(Time.deltaTime, remainingDuration);
        remainingDuration -= elapsed;
        tickTimer -= elapsed;

        while (tickTimer <= 0f)
        {
            CheckHit();
            tickTimer += TickInterval;
        }

        if (remainingDuration <= 0f)
            Managers.Pool.Push(gameObject);
    }

    private void RefreshSize()
    {
        sizeMultiplier = 1f + RuntimeData.RadiusBonus;

        transform.localScale = new Vector3(
            initialScale.x * sizeMultiplier,
            initialScale.y * sizeMultiplier,
            initialScale.z);
    }

    protected override void Chase()
    {
        base.Chase();

        if (hitArea == null)
            return;

        Vector3 scaledOffset = transform.TransformVector(new Vector3(hitArea.offset.x, hitArea.offset.y, 0f));

        Vector3 sizeCorrection = scaledOffset * (1f / sizeMultiplier - 1f);

        transform.position += sizeCorrection  + new Vector3(positionOffset.x, positionOffset.y, 0f);
    }

    private void CheckHit()
    {
        if (hitArea == null || !hitArea.isActiveAndEnabled)
            return;
        int count = hitArea.Overlap(contactFilter, hitBuffer);

        if (count == 0)
            return;

        for(int i = 0; i < count; ++i)
        {
            Collider2D hit = hitBuffer[i];

            if (hit != null)
                Managers.Damage.ApplyDamage(hit.transform, CalculateDamage());
        }
    }
}
