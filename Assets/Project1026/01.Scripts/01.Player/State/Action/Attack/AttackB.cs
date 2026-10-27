using UnityEngine;

public class AttackB : MonoBehaviour
{
    #region Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private float hitRadius = 0.3f; // 피격판정 둘레
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

        if (CheckHit()) // 피격되면 아래 시간 안가고 반환처리 합니다.
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
            Managers.Pool.Push(gameObject);
    }

    private bool CheckHit() // 피격판정되는 메서드입니다
    {
        //오버랩사용했습니다. 콜라이더의 IsTrigger는 몹이나 투사체에 리지드바디가 있어야 감지가 되서 사용했습니다.
        Collider2D hit = Physics2D.OverlapCircle(transform.position, hitRadius, LayerMask.GetMask("Enemy"));

        if (hit == null)
            return false;
        
        if(!hit.TryGetComponent(out EnemyController enemy))
            return false;

        enemy.TakeDamage();
        Managers.Pool.Push(gameObject);
        return true;
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }
}