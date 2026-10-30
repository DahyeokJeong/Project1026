using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

internal class Pool
{
    #region Fields
    private GameObject prefab;
    private ObjectPool<GameObject> pool;
    private Transform root;
    #endregion

    #region Properties
    public bool IsDisposed { get; private set; }
    #endregion

    public Pool(GameObject prefab, Transform parent)
    {
        this.prefab = prefab;

        GameObject go = new GameObject($"@{prefab.name}Pool");
        root = go.transform;
        root.SetParent(parent);

        pool = new ObjectPool<GameObject>(
            createFunc: OnCreate,
            actionOnGet: null,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroyObject,
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 50
        );
    }

    private GameObject OnCreate()
    {
        GameObject go = Object.Instantiate(prefab, root);
        go.name = prefab.name;
        go.SetActive(false);

        PooledObject pooledObject = go.GetComponent<PooledObject>();

        if (pooledObject == null)
            pooledObject = go.AddComponent<PooledObject>();

        pooledObject.Init(this);

        return go;
    }

    private void OnRelease(GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(root);
    }

    private void OnDestroyObject(GameObject go)
    {
        Object.Destroy(go);
    }

    public GameObject Pop(Vector3 position, Quaternion rotation)
    {
        if (IsDisposed)
            return null;

        GameObject go = pool.Get();

        go.transform.SetPositionAndRotation(position, rotation);

        PooledObject pooledObject = go.GetComponent<PooledObject>();
        pooledObject.MarkSpawned();

        go.SetActive(true);

        return go;
    }

    public void Push(GameObject go)
    {
        if (IsDisposed)
            return;

        pool.Release(go);
    }

    public void Clear()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;

        pool.Clear();

        if (root != null)
            Object.Destroy(root.gameObject);
    }
}

public class PoolManager
{
    #region Fields
    private Dictionary<GameObject, Pool> pools = new();
    private readonly Dictionary<PoolType, GameObject> registeredPrefabs = new();
    private Transform root;
    #endregion

    #region Properties
    private Transform Root
    {
        get
        {
            if (root == null)
            {
                GameObject go = new GameObject("@Pool");
                root = go.transform;
                Object.DontDestroyOnLoad(go);
            }

            return root;
        }
    }
    #endregion

    public GameObject Pop(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation)
    {
        if (prefab == null)
            return null;

        if (!pools.TryGetValue(prefab, out Pool pool))
        {
            pool = new Pool(prefab, Root);
            pools.Add(prefab, pool);
        }

        return pool.Pop(position, rotation);
    }

    public bool Push(GameObject go)
    {
        if (go == null)
            return false;

        if (!go.TryGetComponent(out PooledObject pooledObject))
            return false;

        if (!pooledObject.IsSpawned || pooledObject.Owner == null)
            return false;

        if (pooledObject.Owner.IsDisposed)
            return false;

        pooledObject.MarkReleased();
        pooledObject.Owner.Push(go);

        return true;
    }

    public void Clear()
    {
        foreach (Pool pool in pools.Values)
            pool.Clear();

        pools.Clear();
        registeredPrefabs.Clear();

        if (root != null)
        {
            Object.Destroy(root.gameObject);
            root = null;
        }
    }

    public void Register(PoolType type, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"[PoolManager] {type} 프리팹이 없습니다.");
            return;
        }

        registeredPrefabs[type] = prefab;
    }

    public GameObject Pop(
        PoolType type,
        Vector3 position,
        Quaternion rotation)
    {
        if (!registeredPrefabs.TryGetValue(type, out GameObject prefab))
        {
            Debug.LogError($"[PoolManager] {type} 프리팹이 등록되지 않았습니다.");
            return null;
        }

        return Pop(prefab, position, rotation);
    }
}
