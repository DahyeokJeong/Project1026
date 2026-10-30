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
        #endregion

        #region Properties
        public AttackConfigSO Config => config;
        public float Interval => interval;
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
    #endregion

    private void Start()
    {
        attacks.Clear();

        foreach (AttackSlot slot in attackSlots)
        {
            if(slot == null || slot.Config == null)
                continue;

            if(slot.Config.Prefab == null)
            {
                Debug.LogWarning($"[PlayerAutoAttack] {slot.Config.name}에 프리팹이 없습니다.");
                continue;
            }

            attacks.Add(new ProjectileAttack(
                firePoint,
                model,
                slot.Config,
                slot.Interval));
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
}
