using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveDataSO", menuName = "Data/Stage/Wave")]
public class WaveDataSO : ScriptableObject
{
    [System.Serializable]
    public class EnemyEntry
    {
        [SerializeField] private EnemyController prefab;
        [SerializeField, Min(0f)] private float weight = 1f;

        public EnemyController Prefab => prefab;
        public float Weight => weight;
    }

    [SerializeField, Min(0.01f)] private float duration = 180f;
    [SerializeField] private List<EnemyEntry> enemies = new();

    [SerializeField, Min(1)] private int spawnBatchSize = 20;
    [SerializeField, Min(0.01f)] private float spawnInterval = 10f;
    [SerializeField] private bool spawnImmediately = true;

    [Header("Difficulty")]
    [SerializeField] private GwaegangWhiteTiger bossPrefab;
    [SerializeField, Min(1)] private int activeEnemyLimit = 80;
    [SerializeField, Min(0.01f)] private float hpMultiplier = 1f;
    [SerializeField, Min(0)] private float expMultiplier = 1f;
    [SerializeField, Range(0f, 1f)] private float eliteChance;
    [SerializeField, Range(0f, 1f)] private float eliteChanceLimit = 0.35f;
    [SerializeField, Min(1)] private float eliteHpMultiplier = 1.65f;
    [SerializeField, Min(1)] private float eliteExpMultiplier = 1.25f;

    public GwaegangWhiteTiger BossPrefab => bossPrefab;
    public int ActiveEnemyLimit => activeEnemyLimit;
    public float HpMultiplier => hpMultiplier;
    public float ExpMultiplier => expMultiplier;
    public float EliteChance => eliteChance;
    public float EliteChanceLimit => eliteChanceLimit;
    public float EliteHpMultiplier => eliteHpMultiplier;
    public float EliteExpMultiplier => eliteExpMultiplier;

    public float Duration => duration;
    public IReadOnlyList<EnemyEntry> Enemies => enemies;
    public int SpawnBatchSize => spawnBatchSize;
    public float SpawnInterval => spawnInterval;
    public bool SpawnImmediately => spawnImmediately;
}
