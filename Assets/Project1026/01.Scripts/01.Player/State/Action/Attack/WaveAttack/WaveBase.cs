using UnityEngine;

public abstract class WaveBase : MonoBehaviour
{
    #region Settings
    [Header("Settings")]
    [SerializeField, Min(0f)] private float baseDamage = 1f;
    [SerializeField, Min(0.01f)] private float expandDuration = 0.8f;
    #endregion

    #region Fields
    private float elapes;
    private bool initialized;
    #endregion

    #region Properties
    protected float Damage { get; private set; }
    #endregion

    protected virtual void OnEnable()
    {
        initialized = false;
        elapes = 0f;
        Damage = 0f;
    }

    protected virtual void Update()
    {
        if (!initialized || !Managers.BattleState.CanUpdate)
            return;

        float duration = Mathf.Max(0.01f, expandDuration);
        elapes = Mathf.Min(elapes + Time.deltaTime, duration);

        UpdateWave(elapes / duration);

        if (initialized && elapes >= duration)
            Release();
    }

    public void Initialize(float attackDamage, float critical)
    {
        Damage = baseDamage + attackDamage;

        if (Random.value < Mathf.Clamp(critical, 0f, 100f) / 100f)
            Damage *= 1.5f;

        elapes = 0f;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        OnWaveInitialized();
        initialized = true;
    }

    protected abstract void OnWaveInitialized();
    protected abstract void UpdateWave(float progress);

    protected void Release()
    {
        initialized = false;
        Managers.Pool.Push(gameObject);
    }

    protected virtual void OnDisable()
    {
        initialized = false;
    }
}
