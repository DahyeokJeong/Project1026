using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    [Serializable]
    public class AttackSlot
    {
        #region Settings
        [SerializeField] private AttackConfigBaseSO config;
        [SerializeField] private bool initiallyOwned;
        [SerializeField, Min(0.01f)] private float interval = 1f;
        #endregion

        #region Properties
        public AttackConfigBaseSO Config => config;
        public bool InitiallyOwned => initiallyOwned;
        public float Interval => interval;
        #endregion
    }

    private sealed class AttackEntry
    {
        public AttackRuntimeData Data { get; }
        public IAttack Attack { get; }

        public AttackEntry(AttackRuntimeData data, IAttack attack)
        {
            Data = data;
            Attack = attack;
        }
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
    private readonly Dictionary<string, AttackEntry> attacks = new();
    #endregion

    private void Awake()
    {
        if (model == null)
        {
            Debug.LogError("[PlayerAutoAttack] PlayerModel이 연결되지 않았습니다.", this);

            enabled = false;
            return;
        }

        foreach (AttackSlot slot in attackSlots)
        {
            if (slot == null || slot.Config == null)
            {
                Debug.LogError("[PlayerAutoAttack] 공격 슬롯의 Config가 없습니다.", this);
                continue;
            }

            AttackConfigBaseSO config = slot.Config;
            string key = config.ConfigKey;

            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogError($"[PlayerAutoAttack] 공격 키가 없습니다: {config.name}", this);
                continue;
            }

            if (attacks.ContainsKey(key))
            {
                Debug.LogError($"[PlayerAutoAttack] 중복된 공격 키입니다: {key}", this);
                continue;
            }

            AttackRuntimeData data = Managers.RunData.GetOrCreateAttackData(config, slot.InitiallyOwned);

            if (data == null)
                continue;

            IAttack attack = config.CreateAttack(firePoint, model, data, slot.Interval);

            if (attack == null)
                continue;

            attacks.Add(key, new AttackEntry(data, attack));
        }
    }

    private void Update()
    {
        foreach (AttackEntry entry in attacks.Values)
        {
            if (entry.Data.IsOwned)
                entry.Attack.Tick(Time.deltaTime);
        }
    }

    public bool HasAttack(string configKey)
    {
        return !string.IsNullOrWhiteSpace(configKey) && attacks.ContainsKey(configKey);
    }

    public bool TryGetAttackData<T>(string configKey, out T data) where T : AttackRuntimeData
    {
        data = null;

        if (string.IsNullOrWhiteSpace(configKey) || !attacks.TryGetValue(configKey, out AttackEntry entry))
            return false;

        data = entry.Data as T;
        return data != null;
    }

    public bool IsOwned(string configKey)
    {
        return TryGetAttackData(configKey, out AttackRuntimeData data) && data.IsOwned;
    }

    public bool TryAcquireAttack(string configKey)
    {
        return TryGetAttackData(configKey, out AttackRuntimeData data) && data.TryAcquire();
    }

    public bool AddProjectileCount(string configKey, int value)
    {
        return TryGetAttackData( configKey, out ProjectileAttackRuntimeData data) && data.AddProjectileCount(value);
    }

    public int GetProjectileCount(string configKey)
    {
        return TryGetAttackData( configKey, out ProjectileAttackRuntimeData data) ? data.ProjectileCount : 0;
    }
}