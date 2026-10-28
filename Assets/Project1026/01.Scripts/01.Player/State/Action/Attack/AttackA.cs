using UnityEngine;

public class AttackA : MonoBehaviour
{
    #region Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float hitRadius = 0.3f; //피격 판정 둘레
    #endregion

    #region Fields
    private Vector2 moveDirection;
    private float timer;
    #endregion

    private void OnEnable()
    {
        timer = lifeTime;
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);

        if (CheckHit()) // 피격되면 아래 시간이 안가고 반환처리
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
            Managers.Pool.Push(gameObject);
    }

    private bool CheckHit() // 피격판정
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, hitRadius, LayerMask.GetMask("Enemy"));

        if (hit == null)
            return false;

        if (!hit.TryGetComponent(out EnemyController enemy))
            return false;

        //enemy.TakeDamage();
        Managers.Pool.Push(gameObject);
        return true;
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }

}
