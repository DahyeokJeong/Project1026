using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveDataSO", menuName = "Data/Stage/Wave")]
public class WaveDataSO : ScriptableObject
{
    [SerializeField, Min(0.01f)] private float duration = 180f;
    [SerializeField] private List<StageSpawnData.EnemyEntry> enemies = new();

    [SerializeField, Min(1)] private int spawnBatchSize = 20;
    [SerializeField, Min(0.01f)] private float spawnInterval = 10f;
    [SerializeField] private bool spawnImmediately = true;

    public float Duration => duration;
    public IReadOnlyList<StageSpawnData.EnemyEntry> Enemies => enemies;
    public int SpawnBatchSize => spawnBatchSize;
    public float SpawnInterval => spawnInterval;
    public bool SpawnImmediately => spawnImmediately;
}
