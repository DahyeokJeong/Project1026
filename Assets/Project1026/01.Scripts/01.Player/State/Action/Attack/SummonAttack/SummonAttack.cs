using UnityEngine;

public class SummonAttack : IActivatableAttack
{
    #region Fields
    private readonly PlayerModel model;
    private readonly SummonAttackConfigSO config;
    private readonly AttackRuntimeData data;
    private readonly float interval;
    #endregion

    public SummonAttack(PlayerModel model, SummonAttackConfigSO config, AttackRuntimeData data, float interval)
    {
        this.model = model;
        this.config = config;
        this.data = data;
        this.interval = Mathf.Max(0.01f, interval);
    }

    public void Activate()
    {
        if (model == null || !model.gameObject.activeInHierarchy)
            return;

        Vector2 direction = model.LastMoveDirection.normalized;
        if (direction.sqrMagnitude <= 0f)
            direction = Vector2.down;

        Vector3 position = model.transform.position + new Vector3(direction.x, direction.y, 0f);
        GameObject go = Managers.Pool.Pop(config.Prefab, position, Quaternion.identity);
        if (go == null)
            return;

        if (!go.TryGetComponent(out SummonBase summon))
        {
            Managers.Pool.Push(go);
            return;
        }

        summon.Initialize(model, data, interval);
    }
}