using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    #region Components
    [Header("Component")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerModel model;
    #endregion

    #region Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private float piercingInterval = 1f;
    [SerializeField] private float normalInterval = 2f;
    #endregion

    #region Fields
    private readonly List<IAttack> attacks = new();
    #endregion

    private void Start()
    {
        attacks.Add(new ProjectileAttack(
            firePoint,
            movement,
            PoolType.Piercing,
            model.AttackDamage,
            piercingInterval));

        attacks.Add(new ProjectileAttack(
            firePoint,
            movement,
            PoolType.Normal,
            model.AttackDamage,
            normalInterval));
    }

    private void Update()
    {
        foreach (IAttack attack in attacks)
            attack.Tick(Time.deltaTime);
    }

    public void AddAttack(IAttack attack)
    {
        if (attack == null)
            return;

        attacks.Add(attack);
    }
}
