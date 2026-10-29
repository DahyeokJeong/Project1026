using UnityEngine;
public class NormalProjectile : ProjectileBase
{
    #region Component
    [Header("Component")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallLayer;
    #endregion

    #region Projectile Settings
    [Header("Projectile Settings")]
    [SerializeField] private float hitRadius = 0.3f;
    [SerializeField] private float wallRadius = 0.1f;
    #endregion

    #region Fields
    private readonly Collider2D[] hitBuffer = new Collider2D[32];
    private ContactFilter2D contactFilter;
    #endregion

    private void Awake()
    {
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(enemyLayer);
        contactFilter.useTriggers = true;
    }

    protected override void Update()
    {
        base.Update();

        if (!gameObject.activeInHierarchy)
            return;

        CheckHit();
    }

    protected override void Move()
    {
        Vector2 pos = transform.position;
        Vector2 direction = MoveDirection;

        float distance = MoveSpeed * Time.deltaTime;

        RaycastHit2D hit = Physics2D.CircleCast(
            pos,
            wallRadius,
            direction,
            distance,
            wallLayer
        );

        if (hit.collider != null)
        {
            float moveDistance = Mathf.Max(0f, hit.distance - 0.01f);

            pos += direction * moveDistance;

            Vector2 reflectedDirection = Vector2.Reflect(
                direction,
                hit.normal
            );

            SetDirection(reflectedDirection);
        }
        else
        {
            pos += direction * distance;
        }

        transform.position = new Vector3(
            pos.x,
            pos.y,
            transform.position.z
        );
    }

    private void CheckHit()
    {
        int count = Physics2D.OverlapCircle(
            transform.position,
            hitRadius,
            contactFilter,
            hitBuffer
        );

        if (count <= 0)
            return;

        for(int i = 0; i < count; ++i)
        {
            Collider2D hit = hitBuffer[i];

            Managers.Damage.ApplyDamage(hit.transform, Damage);
        }
        // Hit 시스템 구현 시 실제 데미지 처리 연결

        Release();
    }
}
