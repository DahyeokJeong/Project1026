using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerPickupCollector : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float pickupRadius = 3f;
    [SerializeField] private LayerMask pickupLayers;
    [SerializeField, Min(0.01f)] private float scanInterval = 0.1f;

    private readonly List<Collider2D> results = new(16);
    private float timeUntilScan;

    private void OnEnable()
    {
        timeUntilScan = 0f;
    }

    private void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        timeUntilScan -= Time.deltaTime;
        if (timeUntilScan > 0f)
            return;

        timeUntilScan = scanInterval;

        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(pickupLayers);

        int count = Physics2D.OverlapCircle(
            transform.position, pickupRadius, filter, results);

        for (int i = 0; i < count; i++)
        {
            if (results[i].TryGetComponent(out IMagnetPickup pickup))
                pickup.TryBeginPull(transform);
        }
    }
}