using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageManager
{
    #region Fields
    private readonly Dictionary<Transform, Action<float>> receivers = new();
    #endregion

    public void Register(Transform target, Action<float> receiver)
    {
        if (target == null || receiver == null)
            return;

        receivers[target] = receiver;
    }

    public void Unregister(Transform target)
    {
        if(target == null)
            return;
        receivers.Remove(target);
    }

    public bool IsRegistered(Transform target)
    {
        return target != null && receivers.ContainsKey(target);
    }

    public bool TryGetTarget(Transform hit, out Transform target)
    {
        target = null;
        Transform current = hit;

        while (current != null)
        {
            if(receivers.ContainsKey(current))
            {
                target = current;
                return true;
            }

            current = current.parent;
        }
        return false;
    }

    public void ApplyDamage(Transform target, float damage)
    {
        if (target == null || damage <= 0f)
            return;

        Transform current = target;

        while(current != null)
        {
            if(receivers.TryGetValue(current, out var receiver))
            {
                receiver.Invoke(damage);
                return;
            }

            current = current.parent;
        }
    }

    public void Clear()
    {
        receivers.Clear();
    }
}
