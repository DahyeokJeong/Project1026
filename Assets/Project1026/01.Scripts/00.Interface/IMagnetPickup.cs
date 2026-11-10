using UnityEngine;

public interface IMagnetPickup
{
    bool TryBeginPull(Transform target);
}