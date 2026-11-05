using UnityEngine;

public class AuraAttack : IActivatableAttack
{
    private readonly Transform firePoint;
    private readonly PlayerModel model;
    private readonly AuraAttackConfigSO config;
    private readonly AuraAttackRuntimeData data;
    private readonly float interval;

    public AuraAttack(Transform firePoint ,PlayerModel model, AuraAttackConfigSO config, AuraAttackRuntimeData data, float interval)
    {
        this.firePoint = firePoint;
        this.model = model;
        this.config = config;
        this.data = data;
        this.interval = interval;
    }

    public void Activate()
    {
        GameObject go = Managers.Pool.Pop(config.Prefab, firePoint.position, Quaternion.identity);

        if (go == null)
            return;

        if (!go.TryGetComponent(out AuraBase aura))
        {
            Managers.Pool.Push(go);
            return;
        }

        aura.SetAttack(model, firePoint, data, interval);
    }

}
