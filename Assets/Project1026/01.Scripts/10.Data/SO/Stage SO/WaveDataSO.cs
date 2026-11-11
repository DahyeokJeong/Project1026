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

    public float Duration => duration;
    public IReadOnlyList<EnemyEntry> Enemies => enemies;
    public int SpawnBatchSize => spawnBatchSize;
    public float SpawnInterval => spawnInterval;
    public bool SpawnImmediately => spawnImmediately;
}
