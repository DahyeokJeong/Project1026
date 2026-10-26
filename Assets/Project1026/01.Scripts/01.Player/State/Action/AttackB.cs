using UnityEngine;

public class AttackB : MonoBehaviour
{
    #region Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float lifeTime = 2f;
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

        timer -= Time.deltaTime;

        if (timer <= 0f)
            Managers.Pool.Push(gameObject);
    }

    public void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;
    }
}
