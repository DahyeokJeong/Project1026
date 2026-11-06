using UnityEngine;

public class FallingAttack : ITickAttack
{
    #region Compononts
    private readonly PlayerModel model;
    private readonly FallingAttackConfigSO config;
    private readonly FallingAttackRuntimeData data;
    #endregion

    #region Settings
    private readonly float attackInterval;
    #endregion

    #region Fields
    private readonly MapArea mapArea;
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
    }

    public void Tick(float deltaTime)
    {
        float speed = config.UsePlayerStat ? Mathf.Max(0f, model.AttackSpeed) : 1f;
        attackTimer -= deltaTime * speed;

        if (attackTimer > 0f)
            return;

        Fall();
    }

    private void Fall()
    {
        if (mapArea == null || !mapArea.IsReady)
            return;

        for(int i = 0; i < data.FallingCount; ++i)
        {
            if(!TryGetRandomPosition(out Vector2 pos))
                continue;

            GameObject go = Managers.Pool.Pop(config.Prefab, pos, Quaternion.identity);

            if(go == null)
                continue;

            if(!go.TryGetComponent(out FallingBase falling))
            {
                Managers.Pool.Push(go);
                continue;
            }

            float damage = model.AttackDamage + data.DamageBonus;
            falling.Initialize(damage, model.Critical);
        }

    }

    private bool TryGetRandomPosition(out Vector2 pos)
    {
        pos = Vector2.zero;

        Bounds bounds = mapArea.worldBounds;
        const float margin = 0.5f;

        if(bounds.size.x <= margin * 2f || bounds.size.y <= margin * 2f)
            return false;

        for(int i = 0; i < 20; ++i)
        {
            Vector2 candidate = new Vector2(
                Random.Range(bounds.min.x + margin, bounds.max.x - margin),
                Random.Range(bounds.min.y + margin, bounds.max.y - margin)
                );

            if(!mapArea.CanPlace(candidate, 0.2f))
                continue;

            pos = candidate;
            return true;
        }
        return false;
    }
}
