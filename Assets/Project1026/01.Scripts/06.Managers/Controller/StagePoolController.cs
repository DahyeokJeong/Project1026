using UnityEngine;


[DefaultExecutionOrder(-1000)]
public class StagePoolController : MonoBehaviour
{
    private void Awake()
    {
        Managers.Pool.SetRoot(transform);
    }

    private void OnDisable()
    {
        if (!Managers.Initialized)
            return;

        Managers.Pool.ClearStagePools(transform);
    }
}
