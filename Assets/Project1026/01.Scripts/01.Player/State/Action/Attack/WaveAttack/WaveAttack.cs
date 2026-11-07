using UnityEngine;

public class WaveAttack : ITickAttack
{
    #region Settings
    private readonly Transform firePoint;
    private readonly PlayerModel model;
    private readonly WaveAttackConfigSO config;
    private readonly AttackRuntimeData data;
    private readonly float interval;
    private float attackTimer;
    #endregion

    public WaveAttack(Transform firePoint, PlayerModel model, WaveAttackConfigSO config, AttackRuntimeData data, float interval)
    {
        this.firePoint = firePoint;
        this.model = model;
        this.config = config;
        this.data = data;
        this.interval = Mathf.Max(0, interval); ;
        attackTimer = this.interval;
    }

    public void Tick(float deltaTime)
    {
        if (model == null || !model.gameObject.activeInHierarchy)
            return;

        float speed = config.UsePlayerStat ? Mathf.Max(0f, model.AttackSpeed) : 1f;
        attackTimer -= deltaTime * speed / model.AutoAttackCooldownMultiplier;

        if (attackTimer > 0f)
            return;

        EmitWave();
        attackTimer = interval;
    }

    private void EmitWave()
    {
        GameObject go = Managers.Pool.Pop(config.Prefab, firePoint.position, Quaternion.identity);

        if (go == null)
            return;

        if(!go.TryGetComponent(out WaveBase wave))
        {
            Managers.Pool.Push(go);
            return;
        }

        wave.Initialize(model.AttackDamage + data.DamageBonus, model.Critical);
    }
}
