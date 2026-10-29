using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] private EnemyModel model;

    [Header("Layer")]
    [SerializeField] private LayerMask playerLayer;

    [Header("Ranged Attack")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject rangedPrefab;

    private Vector2 dir;
    private float angle;

    public void Execute(Transform target)
    {
        dir = target.position - firePoint.position;
        angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg - 90f;
        switch (model.AttackType)
        {
            case EnemyAttackType.Melee:
                MeleeAttack(target);
                    break;
            case EnemyAttackType.Ranged:
                RangedAttack(target);
                break;
        }
    }

    private void MeleeAttack(Transform target)
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, model.AttackRange, playerLayer);
        if (hit == null)
            return;

        Managers.Damage.ApplyDamage(hit.transform, model.AttackDamage);
    }

    private void RangedAttack(Transform target)
    {

        if (dir.sqrMagnitude <= 0f)
            return;

        GameObject go = Managers.Pool.Pop(rangedPrefab, firePoint.position, Quaternion.Euler(0f, 0f, angle));

        if (go == null)
            return;

        if(go.TryGetComponent(out RangeAttack attack))
            attack.Set(dir, model.AttackDamage);
        else
        {
            Managers.Pool.Push(go);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (model == null || !Application.isPlaying)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, model.AttackRange);
    }
}
