using System.Collections.Generic;
using UnityEngine;

public class PlayerAutoAttack : MonoBehaviour
{
    #region Components
    [Header("Component")]
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private Transform firePoint;
    #endregion

    #region Attack Settings
    [Header("Attack Settings")]
    [SerializeField] private float attackAInterval = 1f;
    [SerializeField] private float attackBInterval = 2f;
    [SerializeField] private float spreadAngle = 20f;
    #endregion

    #region Fields
    private readonly List<IAttack> attacks = new();
    #endregion

    private void Start()
    {
        attacks.Add(new ProjectileAttack(
            firePoint,
            movement,
            attackAInterval
        ));

        attacks.Add(new SpreadAttack(
            firePoint,
            movement,
            attackBInterval,
            spreadAngle
        ));
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