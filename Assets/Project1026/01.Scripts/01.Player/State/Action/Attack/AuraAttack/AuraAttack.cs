using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AuraAttack : IAttack
{
    private readonly PlayerModel model;
    private readonly AttackRuntimeData data;
    private readonly float radius;
    private readonly float interval;
    private readonly ContactFilter2D enemyFilter;

    private readonly List<Collider2D> hits = new();
    private readonly HashSet<EnemyModel> damagedEnemies = new();

    private float timer;

    public AuraAttack(PlayerModel model, AttackRuntimeData data, float radius, float intervbal, LayerMask enemyLayer)
    {
        this.model = model;
        this.data = data;
        this.radius = radius;
        this.interval = Mathf.Max(0.01f, interval);

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(enemyLayer);
        filter.useTriggers = true;
        enemyFilter = filter;

        timer = this.interval;
    }

    public void Tick(float deltaTime)
    {
        timer -= deltaTime;

        if (timer > 0f)
            return;

        timer = interval;
        DamageEnemies();
    }

    private void DamageEnemies()
    {
        damagedEnemies.Clear();
        Physics2D.OverlapCircle(model.transform.position, radius, enemyFilter, hits);

        float damage = model.AttackDamage + data.DamageBonus;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            EnemyModel enemy = hit.GetComponentInParent<EnemyModel>();

            if (enemy != null && damagedEnemies.Add(enemy))
                Managers.Damage.ApplyDamage(enemy.transform, damage);
        }
    }
}
