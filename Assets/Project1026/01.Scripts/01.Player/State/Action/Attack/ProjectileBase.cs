using UnityEngine;
public abstract class ProjectileBase : MonoBehaviour
{
    #region Projectile Settings
    [Header("Projectile Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float lifeTime = 3f;
    #endregion

    #region Fields
    private Vector2 moveDirection;
    private float timer;
    private int damage;
    #endregion

    #region Properties
    protected Vector2 MoveDirection => moveDirection;
    protected float MoveSpeed => moveSpeed;
    protected int Damage => damage;
    #endregion

    protected virtual void OnEnable()
    {
        timer = lifeTime;
    }

    protected virtual void Update()
    {
        Move();

        timer -= Time.deltaTime;

        if (timer <= 0f)
            Release();
    }

    protected virtual void Move()
    {
        transform.position +=
            (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
        UpdateRotation();
    }

    public void SetDamage(int attackDamage)
    {
        damage = attackDamage;
    }

    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(
            moveDirection.y,
            moveDirection.x
        ) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angle
        );
    }

    protected void Release()
    {
        Managers.Pool.Push(gameObject);
    }
}
