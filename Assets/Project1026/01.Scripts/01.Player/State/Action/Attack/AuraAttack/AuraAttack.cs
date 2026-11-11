using UnityEngine;

public class AuraAttack : IActivatableAttack, ITickAttack
{
    private readonly Transform firePoint;
    private readonly PlayerModel model;
    private readonly AuraAttackConfigSO config;
    private readonly AuraAttackRuntimeData data;
    private readonly float interval;
    private float remainingTime;

    public AuraAttack(Transform firePoint ,PlayerModel model, AuraAttackConfigSO config, AuraAttackRuntimeData data, float interval)
    {
        this.firePoint = firePoint;
        this.model = model;
        this.config = config;
        this.data = data;
        this.interval = Mathf.Max(0.01f, interval);
    }

    public void Activate()
    {
        remainingTime = interval;
        if (model == null || !model.gameObject.activeInHierarchy || firePoint == null)
            return;

        GameObject go = Managers.Pool.Pop(config.Prefab, firePoint.position, Quaternion.identity);

        if (go == null)
            return;

        if (!go.TryGetComponent(out AuraBase aura))
        {
            Managers.Pool.Push(go);
            return;
        }

        aura.SetAttack(model, firePoint, data);
    }

    public void Tick(float deltaTime)
    {
        remainingTime -= deltaTime / model.AugmentAutoAttackCooldownMultiplier;
        if (remainingTime <= 0f)
            Activate();
    }
}
