using System;
using UnityEngine;

public class PoolRegistry : MonoBehaviour
{
    [Serializable]
    public class PoolEntry
    {
        public PoolType type;
        public GameObject prefab;
    }

    #region Components
    [Header("Pool Prefabs")]
    [SerializeField] private PoolEntry[] entries;
    #endregion

    private void Awake()
    {
        foreach (PoolEntry entry in entries)
        {
            Managers.Pool.Register(entry.type, entry.prefab);
        }
    }
}