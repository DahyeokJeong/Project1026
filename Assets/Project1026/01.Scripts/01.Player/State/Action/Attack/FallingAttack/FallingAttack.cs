using System.Collections.Generic;
using UnityEngine;

public class FallingAttack : ITickAttack
{
    #region Compononts
    private readonly PlayerModel model;
    private readonly FallingAttackConfigSO config;
    private readonly FallingAttackRuntimeData data;
    #endregion

    #region Settings
    private const float StrikeRange = 5f;
    private readonly float attackInterval;
    private readonly ContactFilter2D enemyFilter;
    #endregion

    #region Fields
    private readonly MapArea mapArea;
    private readonly List<Collider2D> targetHits = new();
    private readonly HashSet<Transform> uniqueTargets = new();
    private readonly List<Vector2> targetPositions = new();
    private float attackTimer;
    #endregion

    public FallingAttack(PlayerModel model, FallingAttackConfigSO config, 
        FallingAttackRuntimeData data, float interval)
    {
        this.model = model;
        this.config = config;
        this.data = data;

        attackInterval = Mathf.Max(0.01f, interval);
        attackTimer = attackInterval;

        mapArea = Object.FindAnyObjectByType<MapArea>();

        ContactFilter2D filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));
        enemyFilter = filter;
    }

    public void Tick(float deltaTime)
    {
        float speed = config.UsePlayerStat ? Mathf.Max(0f, model.AttackSpeed) : 1f;
        attackTimer -= deltaTime * speed / model.AutoAttackCooldownMultiplier;

        if (attackTimer > 0f)
            return;

        Fall();
        attackTimer = attackInterval;
    }

    private void Fall()
    {
        if (mapArea == null || !mapArea.IsReady)
            return;

        Vector2 center = model.transform.position;
        CollectTargetPositions(center);

        for(int i = 0; i < data.FallingCount; ++i)
        {
            int targetIndex = -1;
            Vector2 pos;
            if (targetPositions.Count > 0)
            {
                targetIndex = Random.Range(0, targetPositions.Count);
                pos = targetPositions[targetIndex];
            }
            else if (!TryGetRandomPosition(center, out pos))
                continue;

            GameObject go = Managers.Pool.Pop(config.Prefab, pos, Quaternion.identity);

            if(go == null)
                continue;

            if(!go.TryGetComponent(out FallingBase falling))
            {
                Managers.Pool.Push(go);
                continue;
            }

            float damage = model.AttackDamage;
            if (falling is LightningFalling lightning)
                lightning.SetSizeBonus(data.SizeBonus);

            falling.Initialize(damage, model.Critical, data.DamageBonus);

            if (targetIndex >= 0)
                targetPositions.RemoveAt(targetIndex);
        }

    }

    private void CollectTargetPositions(Vector2 center)
    {
        targetPositions.Clear();
        uniqueTargets.Clear();
        Physics2D.OverlapCircle(center, StrikeRange, enemyFilter, targetHits);

        foreach (Collider2D hit in targetHits)
        {
            if (!Managers.Damage.TryGetTarget(hit.transform, out Transform target) ||
                !target.gameObject.activeInHierarchy)
                continue;

            Vector2 position = target.position;
            if ((position - center).sqrMagnitude <= StrikeRange * StrikeRange && uniqueTargets.Add(target))
                targetPositions.Add(position);
        }
    }

    private bool TryGetRandomPosition(Vector2 center, out Vector2 pos)
    {
        pos = Vector2.zero;

        const float placementClearance = 0.2f;

        for (int i = 0; i < 20; ++i)
        {
            Vector2 candidate = center + Random.insideUnitCircle * StrikeRange;

            if (!mapArea.CanPlace(candidate, placementClearance))
                continue;

            pos = candidate;
            return true;
        }

        return false;
    }
}
