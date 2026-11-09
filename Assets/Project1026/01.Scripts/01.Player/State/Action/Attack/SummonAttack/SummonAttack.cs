using UnityEngine;

public class SummonAttack : IActivatableAttack, ITickAttack
{
    private readonly Transform firePoint;
    private readonly PlayerModel model;
    private readonly SummonAttackConfigSO config;
    private readonly AttackRuntimeData data;
    private readonly ProjectileAttackRuntimeData wispData;
    private readonly float interval;

    private OrbitSummon activeWisp;
    private float summonTimer;

    public SummonAttack(
        Transform firePoint,
        PlayerModel model,
        SummonAttackConfigSO config,
        AttackRuntimeData data,
        float interval)
    {
        this.firePoint = firePoint;
        this.model = model;
        this.config = config;
        this.data = data;
        this.interval = Mathf.Max(0.01f, interval);

        wispData = data as ProjectileAttackRuntimeData;
        summonTimer = this.interval;
    }

    public void Activate()
    {
        if (model == null || !model.gameObject.activeInHierarchy)
            return;

        Vector2 direction = model.LastMoveDirection.normalized;
        if (direction == Vector2.zero)
            direction = Vector2.down;

        Transform center = wispData != null && firePoint != null
            ? firePoint
            : model.transform;

        Vector3 position = center.position + (Vector3)direction;
        GameObject instance = Managers.Pool.Pop(config.Prefab, position, Quaternion.identity);

        if (instance == null)
            return;

        if (!instance.TryGetComponent(out SummonBase summon))
        {
            Managers.Pool.Push(instance);
            return;
        }

        summon.Initialize(model, data, interval);

        if (summon is OrbitSummon wisp && wispData != null)
        {
            wisp.SetOrbit(center, direction, wispData);

            if (activeWisp != null && activeWisp != wisp)
                activeWisp.Despawn(wispData);

            activeWisp = wisp;
        }

        summonTimer = interval;
    }

    public void Tick(float deltaTime)
    {
        if (wispData == null)
            return;

        summonTimer = Mathf.Max(0f, summonTimer - deltaTime);
        if (summonTimer <= 0f)
            Activate();
    }
}