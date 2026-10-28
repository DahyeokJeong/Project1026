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
    #endregion

    #region Properties
    protected Vector2 MoveDirection => moveDirection;
    protected float MoveSpeed => moveSpeed;
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
    }

    protected void Release()
    {
        Managers.Pool.Push(gameObject);
    }
}
