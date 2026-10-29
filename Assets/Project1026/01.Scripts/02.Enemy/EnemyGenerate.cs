using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerate : MonoBehaviour
{
    #region Regerences
    [Header("References")]
    [SerializeField] private NavigationArea area;
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
    [SerializeField, Min(0.01f)] private float enemyRadius = 0.5f;
    [SerializeField, Min(0f)] private float edgePadding = 0.15f;
    [SerializeField, Min(1)] private int positionAttempts = 20;
    #endregion

    #region 
    private readonly List<EnemyController> activeEnemies = new();
    private readonly Dictionary<GameObject, EnemyController> enemyCache = new();
    private readonly List<GameObject> destroyedKeys = new();
    private float timer;
    private int totalSpawned;
    #endregion

    private void Start()
    {
        if (area == null || area.Grid == null || meleePrefab == null || rangedPrefab == null)
            return;

        timer = Mathf.Max(0.01f, spawnInterval);

        if (spawnImmediately)
            SpawnWave();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer > 0f)
            return;

        timer = Mathf.Max(0.01f, spawnInterval);
        SpawnWave();
    }

    private void SpawnWave()
    {
        if (area == null || area.Grid == null)
            return;

        activeEnemies.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
        ClearDestroyedCache();

        int available = Mathf.Max(0, maxCount - totalSpawned);
        int count = Mathf.Min(spawnCount, available);

        for (int i = 0; i < count; i++)
        {
            if (!TryGetSpawnPosition(out Vector2 position))
                continue;

            EnemyController prefab = Random.value < rangedRatio ? rangedPrefab : meleePrefab;

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
    private bool TryGetSpawnPosition(out Vector2 position)
    {
        position = Vector2.zero;

        NavigationGrid grid = area.Grid;
        float inset = enemyRadius + edgePadding;

        Vector2 min = grid.Origin + Vector2.one * inset;

        Vector2 max =grid.Origin + new Vector2(grid.Width, grid.Height) * grid.CellSize - Vector2.one * inset;

        float width = max.x - min.x;
        float height = max.y - min.y;

        if (width <= 0f || height <= 0f)
            return false;

        float perimeter = (width + height) * 2f;

        for (int i = 0; i < positionAttempts; i++)
        {
            float distance = Random.Range(0f, perimeter);
            Vector2 candidate;

            if (distance < width) 
            {
                candidate = new Vector2(min.x + distance, min.y);
            }
            else if (distance < width + height)
            {
                candidate = new Vector2(max.x, min.y + distance - width);
            }
            else if (distance < width * 2f + height)
            {
                candidate = new Vector2(max.x - (distance - width - height), max.y
                );
            }
            else
            {
                candidate = new Vector2(min.x, max.y - (distance - width * 2f - height));
            }

            if (!grid.TryWorldToCell(candidate, out Vector2Int cell))
                continue;

            if (!grid.IsWalkable(cell))
                continue;

            if (Physics2D.OverlapCircle( candidate, enemyRadius + edgePadding, area.Obstacle) != null)
                continue;

            position = candidate;
            return true;
        }

        /*      3번째
           ← ← ← ← ←
           ↑         ↑
        4번째       2번째
           ↓         ↑
           → → → → →
               1번째
        */
        return false;
    }
}