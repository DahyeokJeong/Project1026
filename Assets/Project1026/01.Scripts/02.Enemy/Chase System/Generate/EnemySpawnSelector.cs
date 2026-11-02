using System.Collections.Generic;
using UnityEngine;

public static class EnemySpawnSelector
{
    public static EnemyController Select(
        IReadOnlyList<StageSpawnData.EnemyEntry> entries)
    {
        float totalWeight = 0f;

        foreach (var entry in entries)
        {
            if (entry == null || entry.Prefab == null || entry.Weight <= 0f)
                continue;

            totalWeight += entry.Weight;
        }

        if (totalWeight <= 0f)
            return null;

        float random = Random.value * totalWeight;
        EnemyController lastCandidate = null;

        foreach (var entry in entries)
        {
            if (entry == null || entry.Prefab == null || entry.Weight <= 0f)
                continue;

            lastCandidate = entry.Prefab;

            if (random < entry.Weight)
                return entry.Prefab;

            random -= entry.Weight;
        }

        // Random.value가 상한값인 1을 반환한 경우 처리
        return lastCandidate;
    }
}