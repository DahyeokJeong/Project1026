using UnityEngine;

public class ExpPickup : MonoBehaviour, IMagnetPickup
{
    [SerializeField, Min(0.1f)] private float pullSpeed = 8f;

    private float rewardExp;
    private Transform target;

    public static void Spawn(GameObject prefab, Vector3 position, float amount)
    {
        GameObject pickup = Managers.Pool.Pop(prefab, position, Quaternion.identity);
        if (pickup == null)
            return;

        if (pickup.TryGetComponent(out ExpPickup expPickup))
            expPickup.Initialize(amount);
        else
            Managers.Pool.Push(pickup);
    }

    public void Initialize(float amount)
    {
        rewardExp = amount;
        target = null;
    }

    public bool TryBeginPull(Transform pullTarget)
    {
        if (target != null || pullTarget == null)
            return false;

        target = pullTarget;
        return true;
    }

    private void Update()
    {
        if (target == null || !Managers.BattleState.CanUpdate)
            return;

        Vector3 destination = target.position;
        destination.z = transform.position.z;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            pullSpeed * Time.deltaTime);

        if (transform.position == destination && Managers.Pool.Push(gameObject))
            Managers.RunData.AddExp(rewardExp);
    }
}