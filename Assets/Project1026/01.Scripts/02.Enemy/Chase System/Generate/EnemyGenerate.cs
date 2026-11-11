using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerate : MonoBehaviour
{
    #region References
    [Header("References")]
    [SerializeField] private NavigationArea area;
    [SerializeField] private MapArea mapArea;
    #endregion

    #region Spawn Position
    [Header("Spawn Position")]
    [SerializeField, Min(1)] private int positionAttempts = 20;
    [SerializeField, Min(0f)] private float spawnClearance;
    #endregion

    #region Fields
    private readonly List<EnemyController> activeEnemies = new();
    private readonly HashSet<EnemyController> currentWaveEnemies = new();
    private readonly Dictionary<GameObject, EnemyController> enemyCache = new();
    private readonly List<GameObject> destroyedKeys = new();

    private EnemySpawnPosition spawnPosition;
    private StageSpawnData spawnData;

    private float timer;
    private int totalSpawned;
    private bool isSpawning;
    private bool IsSpawnAreaReady => mapArea != null && mapArea.IsReady && area != null && area.IsReady;
    #endregion

    #region Properties
    public bool IsSpawning => isSpawning;
    public bool HasSpawnedAll => spawnData != null && totalSpawned >= spawnData.MaxCount;
    public int ActiveEnemyCount => activeEnemies.Count;
    public int CurrentWaveActiveEnemyCount => currentWaveEnemies.Count;
    #endregion

    // Start()에서 자동으로 시작하지 않음.
    // BattleSceneController가 데이터를 전달한 후 시작.

    public void StartSpawning(StageSpawnData data)
    {
        if (!CanStartSpawning(data))
            return;

        activeEnemies.RemoveAll(IsInactiveEnemy);

        ClearDestroyedCache();

        spawnData = data;
        spawnPosition = new EnemySpawnPosition(mapArea);

        currentWaveEnemies.Clear();
        totalSpawned = 0;
        timer = spawnData.SpawnInterval;
        isSpawning = true;

        if (spawnData.SpawnImmediately)
            SpawnBatch();
    }

    private bool CanStartSpawning(StageSpawnData data)
    {
        if (isSpawning || data == null)
            return false;

        if (!IsSpawnAreaReady)
            return false;

        if (data.MaxCount <= 0 || data.SpawnCount <= 0 || data.SpawnInterval <= 0f)
            return false;

        foreach (var entry in data.Enemies)
        {
            if (entry != null && entry.Prefab != null && entry.Weight > 0f)
                return true;
        }

        return false;
    }

    private void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        if (!isSpawning || HasSpawnedAll)
            return;

        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        timer = Mathf.Max(0.01f, spawnData.SpawnInterval);
        SpawnBatch();
    }

    private void SpawnBatch()
    {
        if (!IsSpawnAreaReady)
            return;

        activeEnemies.RemoveAll(IsInactiveEnemy);
        currentWaveEnemies.RemoveWhere(IsInactiveEnemy);

        ClearDestroyedCache();

        int available = Mathf.Max(0,spawnData.MaxCount - totalSpawned);

        int count = Mathf.Min(spawnData.SpawnCount, available);

        for (int i = 0; i < count; i++)
        {
            if (!spawnPosition.TryGetPosition(spawnClearance, positionAttempts, out Vector2 position))
                continue;

            EnemyController prefab = EnemySpawnSelector.Select(spawnData.Enemies);

            if (prefab == null)
                continue;

            SpawnEnemy(prefab, position);
        }
    }

    private void SpawnEnemy(EnemyController prefab,Vector2 position)
    {
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
        currentWaveEnemies.Add(enemy);
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
        currentWaveEnemies.Remove(enemy);
        return true;
    }

    private void ClearDestroyedCache()
    {
        destroyedKeys.Clear();

        foreach (var entry in enemyCache)
        {
            if (entry.Key == null || entry.Value == null)
                destroyedKeys.Add(entry.Key);
        }

        foreach (GameObject key in destroyedKeys)
            enemyCache.Remove(key);
    }

    private static bool IsInactiveEnemy(EnemyController enemy)
    {
        return enemy == null || !enemy.gameObject.activeInHierarchy;
    }

    private void OnDisable()
    {
        StopSpawning();
    }
}