using UnityEngine;

public static class EnemySpawnSelector
{
    public static EnemyController Select(EnemyController meleePrefab, EnemyController rangedPrefab, float rangedRatio)
    {
        return Random.value < rangedRatio ? rangedPrefab : meleePrefab;
    }
}
