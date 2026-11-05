using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
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

    #region Fields
    private readonly Dictionary<string, AttackEntry> attacks = new();

    private DataManager dataManager;
    private bool attacksInitialized;
    #endregion

    private void Awake()
    {
        if (model == null)
        {
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        if (!enabled)
            return;

        dataManager = Managers.Data;

        if(dataManager.IsLoaded)
            InitializeAttacks();
        else
            dataManager.DataLoadCompleted += InitializeAttacks;
    }

    private void InitializeAttacks()
    {
        dataManager.DataLoadCompleted -= InitializeAttacks;

        if (attacksInitialized)
            return;

        attacksInitialized = true;

        foreach(AttackConfigBaseSO config in dataManager.AttackConfigs.Values)
        {
            AttackRuntimeData data = Managers.RunData.GetOrCreateAttackData(config, config.InitiallyOwned);

            if(data == null)
                continue;

            IAttack attack = config.CreateAttack(firePoint, model, data, config.Interval);

            if (attack == null)
                continue;

            attacks.Add(config.ConfigKey, new AttackEntry(data, attack));
        }

        foreach(AttackEntry entry in attacks.Values)
        {
            if(entry.Data.IsOwned && entry.Attack is IActivatableAttack attack)
                attack.Activate();
        }
    }

    private void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        foreach (AttackEntry entry in attacks.Values)
        {
            if (entry.Data.IsOwned && entry.Attack is ITickAttack attack)
                attack.Tick(Time.deltaTime);
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

    public bool TryAcquireAttack(string configKey, int value = 1)
    {
        if (string.IsNullOrWhiteSpace(configKey) || !attacks.TryGetValue(configKey, out AttackEntry entry) 
            || !entry.Data.TryAcquire(value))
            return false;

        if(entry.Attack is IActivatableAttack attack)
            attack.Activate();

        return true;
    }

    public bool AddProjectileCount(string configKey, int value)
    {
        return TryGetAttackData( configKey, out ProjectileAttackRuntimeData data) && data.AddProjectileCount(value);
    }

    public int GetProjectileCount(string configKey)
    {
        return TryGetAttackData( configKey, out ProjectileAttackRuntimeData data) ? data.ProjectileCount : 0;
    }

    private void OnDestroy()
    {
        if (dataManager != null)
            dataManager.DataLoadCompleted -= InitializeAttacks;
    }
}