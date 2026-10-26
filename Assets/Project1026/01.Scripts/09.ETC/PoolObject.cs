
using UnityEngine;

public class PooledObject : MonoBehaviour
{
    #region Properties
    internal Pool Owner { get; private set; }
    public bool IsSpawned { get; private set; }
    #endregion

    internal void Init(Pool owner)
    {
        Owner = owner;
        IsSpawned = false;
    }

    internal void MarkSpawned()
    {
        IsSpawned = true;
    }

    internal void MarkReleased()
    {
        IsSpawned = false;
    }
}
