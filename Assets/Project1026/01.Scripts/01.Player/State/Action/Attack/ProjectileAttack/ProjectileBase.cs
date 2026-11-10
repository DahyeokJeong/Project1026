using UnityEngine;
public abstract class ProjectileBase : MonoBehaviour
{
    #region Projectile Settings
    [Header("Projectile Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float baseDamage = 1;
    [SerializeField, Min(0f)] private float damageMultiplier = 1f;
    #endregion

    #region Fields
    private ProjectileWallCollision wallCollision;
    private Vector2 moveDirection;
    private float timer;
    private float damage;
    private int remainingBounces;
    private float projectileSpeedMultiplier = 1f;
    private ProjectileHoming homing;
    #endregion

    #region Properties
    protected Vector2 MoveDirection => moveDirection;
    protected float MoveSpeed => moveSpeed * projectileSpeedMultiplier;
    protected float Damage => damage; 
    #endregion

    protected virtual void OnEnable()
    {
        timer = lifeTime;
        remainingBounces = 0;
        projectileSpeedMultiplier = 1f;

        if (wallCollision == null)
            wallCollision = GetComponent<ProjectileWallCollision>();

        homing = GetComponent<ProjectileHoming>();
    }

    protected virtual void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        Move();

        if (!gameObject.activeInHierarchy)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
            Release();
    }

    protected virtual void Move()
    {
        Vector2 pos = transform.position;
        Vector2 dir = moveDirection;

        if (homing != null)
            dir = homing.GetDirection(pos);

        bool canContinue = wallCollision.TryMove(ref pos, ref dir, MoveSpeed * Time.deltaTime, ref remainingBounces);

        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        SetDirection(dir);

        if(!canContinue)
            Release();
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        UpdateRotation();
    }

    public void SetDamage(float attackDamage, float critical, float damageBonus = 0f)
    {
        damage = attackDamage * damageMultiplier + baseDamage + damageBonus;
        if (Random.value < Mathf.Clamp(critical, 0f, 100f) / 100f)
            damage *= 1.5f;
    }

    public void SetProjectileSpeedBonus(int bonusPercent)
    {
        projectileSpeedMultiplier = 1f + bonusPercent * 0.01f;
    }

    public void SetBounceCount(int count)
    {
        remainingBounces = Mathf.Max(0, count);
    }

    private void UpdateRotation()
    {
        float angle = Mathf.Atan2( moveDirection.y, moveDirection.x ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    protected void Release()
    {
        Managers.Pool.Push(gameObject);
    }
}
