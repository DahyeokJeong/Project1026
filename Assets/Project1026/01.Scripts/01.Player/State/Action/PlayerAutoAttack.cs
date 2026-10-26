using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    #region Components
    [Header("Components")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private GameObject attackPrefab;
    #endregion

    #region Attack Settings
    [SerializeField] private float attackInterval = 1f;
    #endregion

    #region Fields
    private float attackTimer;
    #endregion

    private void OnEnable()
    {
        attackTimer = attackInterval;
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        Fire();

        attackTimer = Mathf.Max(0.01f, attackInterval);
    }

    private void Fire()
    {
        GameObject attackObject = Managers.Pool.Pop(
            attackPrefab,
            transform.position,
            Quaternion.identity
        );

        if (attackObject == null)
            return;

        if (attackObject.TryGetComponent(out AttackA attack))
        {
            attack.SetDirection(movement.LastMoveDirection);
        }
    }
}
