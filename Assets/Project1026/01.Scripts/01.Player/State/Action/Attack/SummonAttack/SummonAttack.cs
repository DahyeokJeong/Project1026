using UnityEngine;

public class SummonAttack : IActivatableAttack, ITickAttack
{
    private readonly Transform firePoint;
    private readonly PlayerModel model;
    private readonly SummonAttackConfigSO config;
    private readonly AttackRuntimeData data;
    private readonly ProjectileAttackRuntimeData wispData;
    private readonly IndependentSummonRuntimeData independentData;
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
        independentData = data as IndependentSummonRuntimeData;
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

        Vector2 side = new Vector2(-direction.y, direction.x);
        int count = independentData?.SummonCount ?? 1;
        const float spacing = 0.5f;

        for (int i = 0; i < count; i++)
        {
            float offset = (i - (count - 1) * 0.5f) * spacing;
            Vector3 position = center.position + (Vector3)(direction + side * offset);
            SpawnSummon(position, center, direction);
        }

        summonTimer = interval;
    }

    private void SpawnSummon(Vector3 position, Transform center, Vector2 direction)
    {
        GameObject instance = Managers.Pool.Pop(config.Prefab, position, Quaternion.identity);

        if (instance == null)
            return;

        if (!instance.TryGetComponent(out SummonBase summon))
        {
            Managers.Pool.Push(instance);
            return;
        }

        summon.Initialize(model, data, interval);

        if (summon is IndependentSummon independent && independentData != null)
            independent.SetDurationBonus(independentData.DurationBonus);

        if (summon is OrbitSummon wisp && wispData != null)
        {
            wisp.SetOrbit(center, direction, wispData);

            if (activeWisp != null && activeWisp != wisp)
                activeWisp.Despawn(wispData);

            activeWisp = wisp;
        }
    }

    public void Tick(float deltaTime)
    {
        if (wispData == null && independentData == null)
            return;

        summonTimer = Mathf.Max(0f, summonTimer - deltaTime / model.AugmentAutoAttackCooldownMultiplier);
        if (summonTimer <= 0f)
            Activate();
    }
}