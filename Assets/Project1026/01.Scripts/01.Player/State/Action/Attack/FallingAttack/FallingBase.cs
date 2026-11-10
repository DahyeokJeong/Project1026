using UnityEngine;

public abstract class FallingBase : MonoBehaviour
{
    #region Components
    [Header("Components")]
    [SerializeField] private Transform visual;
    #endregion

    #region Settings
    [Header("Settings")]
    [SerializeField, Min(0.01f)] private float fallDuration = 0.6f;
    [SerializeField, Min(0f)] private float fallHeight = 4f;
    [SerializeField] private float damageMultiplier = 1f;
    #endregion

    #region Fields
    private enum FallState
    {
        Inactive,
        Falling,
        Landed
    }

    private FallState state;
    private float elapsedTime;
    private float damage;
    #endregion

    #region Properties
    protected float Damage => damage;
    protected virtual float LandingDuration => 0f;
    #endregion

    protected virtual void OnEnable()
    {
        state = FallState.Inactive;
        elapsedTime = 0f;

        if (visual != null)
            visual.localPosition = Vector3.up * fallHeight;
    }

    public void Initialize(float attackDamage, float critical, float damageBonus = 0f)
    {
        damage = attackDamage * damageMultiplier + damageBonus;

        if (Random.value < Mathf.Clamp(critical, 0f, 100f) / 100f)
            damage *= 1.5f;

        elapsedTime = 0f;
        state = FallState.Falling;

        if (visual != null)
            visual.localPosition = Vector3.up * fallHeight;

        OnFallProgress(0f);
    }

    protected virtual void Update()
    {
        if (state == FallState.Inactive || !Managers.BattleState.CanUpdate)
            return;

        elapsedTime += Time.deltaTime;

        if (state == FallState.Landed)
        {
            float duration = LandingDuration;
            if (elapsedTime >= duration)
            {
                Release();
                return;
            }

            OnLandingProgress(elapsedTime / duration);
            return;
        }

        float progress = Mathf.Clamp01(elapsedTime / Mathf.Max(0.01f, fallDuration));

        if (visual != null)
            visual.localPosition = Vector3.Lerp(Vector3.up * fallHeight, Vector3.zero, progress);

        if (progress < 1f)
        {
            OnFallProgress(progress);
            return;
        }

        state = FallState.Landed;
        elapsedTime = 0f;

        OnLandingProgress(0f);
        OnLanded();

        if (state == FallState.Landed && LandingDuration <= 0f)
            Release();
    }

    protected virtual void OnFallProgress(float progress)
    {
    }

    protected virtual void OnLandingProgress(float progress)
    {
    }

    protected abstract void OnLanded();

    protected void Release()
    {
        state = FallState.Inactive;
        Managers.Pool.Push(gameObject);
    }
}