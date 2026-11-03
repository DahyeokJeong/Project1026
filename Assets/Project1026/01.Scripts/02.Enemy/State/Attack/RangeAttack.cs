using UnityEngine;

public class RangeAttack : MonoBehaviour
{
    #region Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float lifeTime = 2f;
    #endregion

    #region Fields
    private Vector2 moveDirection;
    private float damage;
    private float timer;
    #endregion

    private void OnEnable()
    {
        timer = lifeTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
            Managers.Pool.Push(gameObject);
    }

    private void FixedUpdate()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
    }

    public void Set(Vector2 direction, float damage)
    {
        moveDirection = direction.normalized;
        this.damage = damage;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            Managers.Damage.ApplyDamage(other.transform, damage);
            Managers.Pool.Push(gameObject);
        }
    }
}
