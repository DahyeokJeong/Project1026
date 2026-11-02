using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerate : MonoBehaviour
{
    #region Regerences
    [Header("References")]
    [SerializeField] private NavigationArea area;
    [SerializeField] private MapArea mapArea;

    #endregion

    #region Prefab
    [Header("Prefab")]
    [SerializeField] private EnemyController meleePrefab;
    [SerializeField] private EnemyController rangedPrefab;
    #endregion

    #region Setting
    [Header("Spawn Settings")]
    [SerializeField, Min(1)] private int maxCount = 200;
    [SerializeField, Min(1)] private int spawnCount = 5;
    [SerializeField, Min(0.01f)] private float spawnInterval = 2f;
    [SerializeField] private bool spawnImmediately;
    #endregion

    #region Ratio
    [Header("Spawn Ratio")]
    [SerializeField, Range(0f, 1f)] private float rangedRatio = 0.3f;
    #endregion

    #region Spawn
    [Header("Spawn Position")]
    [SerializeField, Min(1)] private int positionAttempts = 20;
    [SerializeField, Min(0f)] private float spawnClearance;
    #endregion

    #region  fields
    private readonly List<EnemyController> activeEnemies = new();
    private readonly Dictionary<GameObject, EnemyController> enemyCache = new();
    private readonly List<GameObject> destroyedKeys = new();
    private EnemySpawnPosition spawnPosition;
    private float timer;
    private int totalSpawned;
    private bool isSpawning;
    #endregion

    private void Start()
    {
        StartSpawning();
    }

    private void Update()
    {
        if (!isSpawning)
            return;

        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        timer = Mathf.Max(0.01f, spawnInterval);
        SpawnWave();
    }

    public void StartSpawning()
    {
        if (isSpawning)
            return;

        if (mapArea == null || !mapArea.IsReady || area == null || !area.IsReady || meleePrefab == null || rangedPrefab == null)
            return;

        activeEnemies.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);

        if (activeEnemies.Count > 0)
            return;
        ClearDestroyedCache();

        spawnPosition = new EnemySpawnPosition(mapArea);

        totalSpawned = 0;
        timer = Mathf.Max(0.01f, spawnInterval);
        isSpawning = true;

        if (spawnImmediately)
            SpawnWave();
    }

    private void SpawnWave()
    {
        if (mapArea == null || !mapArea.IsReady || area == null || !area.IsReady)
            return;

        activeEnemies.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
        ClearDestroyedCache();

        int available = Mathf.Max(0, maxCount - totalSpawned);
        int count = Mathf.Min(spawnCount, available);

        for (int i = 0; i < count; i++)
        {
            if (!spawnPosition.TryGetPosition(
                spawnClearance,
                positionAttempts,
                out Vector2 position))
                continue;

            EnemyController prefab = EnemySpawnSelector.Select(meleePrefab, rangedPrefab, rangedRatio);

            SpawnEnemy(prefab, position);
        }
    }

    private void SpawnEnemy(EnemyController prefab, Vector2 position)
    {
        if (prefab == null)
            return;

        GameObject obj = Managers.Pool.Pop(prefab.gameObject, new Vector3(position.x, position.y, transform.position.z), Quaternion.identity);

        if (obj == null)
            return;


        if (!enemyCache.TryGetValue(obj, out EnemyController enemy) || enemy == null)
        {
            enemy = obj.GetComponent<EnemyController>();

            if (enemy == null)
            {
                Managers.Pool.Push(obj);
                return;
            }

            enemyCache[obj] = enemy;
        }

        enemy.Initialize(area, this);
        activeEnemies.Add(enemy);
        totalSpawned++;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public bool Despawn(EnemyController enemy)
    {
        if (enemy == null || !activeEnemies.Contains(enemy))
            return false;

        if (!Managers.Pool.Push(enemy.gameObject))
            return false;

        activeEnemies.Remove(enemy);
        return true;
    }

    private void ClearDestroyedCache()
    {
        destroyedKeys.Clear();

        foreach (KeyValuePair<GameObject, EnemyController> entry in enemyCache)
        {
            if (entry.Key == null || entry.Value == null)
                destroyedKeys.Add(entry.Key);
        }

        foreach (GameObject key in destroyedKeys)
            enemyCache.Remove(key);
    }

    private void OnDisable()
    {
        StopSpawning();
    }
}