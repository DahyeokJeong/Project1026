using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "StageSpawnData",
    menuName = "Data/Stage Spawn")]
public class StageSpawnData : ScriptableObject
{
    [Serializable]
    public class EnemyEntry
    {
        [SerializeField] private EnemyController prefab;
        [SerializeField, Min(0f)] private float weight = 1f;

        public EnemyController Prefab => prefab;
        public float Weight => weight;
    }

    #region Stage
    [Header("Stage")]
    [SerializeField, Min(1)] private int stageId = 1;
    #endregion

    #region Enemies
    [Header("Enemies")]
    [SerializeField] private List<EnemyEntry> enemies = new();
    #endregion

    #region Spawn Settings
    [Header("Spawn Settings")]
    [SerializeField, Min(1)] private int maxCount = 200;
    [SerializeField, Min(1)] private int spawnCount = 5;
    [SerializeField, Min(0.01f)] private float spawnInterval = 2f;
    [SerializeField] private bool spawnImmediately;
    #endregion

    #region Properties
    public int StageId => stageId;
    public IReadOnlyList<EnemyEntry> Enemies => enemies;

    public int MaxCount => maxCount;
    public int SpawnCount => spawnCount;
    public float SpawnInterval => spawnInterval;
    public bool SpawnImmediately => spawnImmediately;
    #endregion
}