using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    [Serializable]
    public class AttackSlot
    {
        #region Attack Settings
        [SerializeField] private AttackConfigSO config;
        [SerializeField, Min(0.01f)] private float interval = 1f;
        [SerializeField, Min(0)] private int projectileCount = 1;
        #endregion

        #region Properties
        public AttackConfigSO Config => config;
        public float Interval => interval;
        public int ProjectileCount => projectileCount;
        #endregion
    }

    #region Components
    [Header("Component")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerModel model;
    #endregion

    #region Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private List<AttackSlot> attackSlots = new();
    #endregion

    #region Fields
    private readonly List<IAttack> attacks = new();
    private readonly Dictionary<string, int> projectileCounts = new();
    #endregion

    private void Start()
    {
        attacks.Clear();
        projectileCounts.Clear();

        foreach (AttackSlot slot in attackSlots)
        {
            if(slot == null || slot.Config == null)
                continue;

            if(slot.Config.Prefab == null)
            {
                Debug.LogWarning($"[PlayerAutoAttack] {slot.Config.name}에 프리팹이 없습니다.");
                continue;
            }

            projectileCounts[slot.Config.name] = slot.ProjectileCount;

            attacks.Add(new ProjectileAttack(
                firePoint,
                model,
                slot.Config,
                slot.Interval,
                GetProjectileCount(slot.Config.name)));
        }
    }

    private void Update()
    {
        foreach (IAttack attack in attacks)
            attack.Tick(Time.deltaTime * Mathf.Max(0f, model.AttackSpeed));
    }

    public void AddAttack(IAttack attack)
    {
        if (attack == null)
            return;

        attacks.Add(attack);
    }

    public void AddProjectileCount(string configKey, int value)
    {
        if (!projectileCounts.ContainsKey(configKey))
            return;

        projectileCounts[configKey] = value;
    }

    private int GetProjectileCount(string configKey)
    {
        if(projectileCounts.TryGetValue(configKey, out int count))
            return count;
        return 0;
    }
}
