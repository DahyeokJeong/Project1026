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
    [SerializeField] private float baseDamage = 1f;
    #endregion

    #region Fields
    private float fallTimer;
    private float damage;
    private bool isFalling;
    #endregion

    #region Properties
    protected float Damage => damage;
    #endregion

    protected virtual void OnEnable()
    {
        fallTimer = 0f;
        isFalling = false;

        if (visual != null)
            visual.localPosition = Vector3.up * fallHeight;
    }

    public void Initialize(float attackDamage, float critical)
    {
        damage = baseDamage + attackDamage;

        if (Random.value < Mathf.Clamp(critical, 0f, 100f) / 100f)
            damage *= 1.5f;

        fallTimer = 0f;
        isFalling = true;

        if (visual != null)
            visual.localPosition = Vector3.up * fallHeight;
    }

    protected virtual void Update()
    {
        if (!Managers.BattleState.CanUpdate || !isFalling || visual == null)
            return;

        fallTimer += Time.deltaTime;

        float ratio = Mathf.Clamp01(fallTimer / fallDuration);

        visual.localPosition = Vector3.Lerp(Vector3.up * fallHeight, Vector3.zero, ratio);

        if (ratio < 1f)
            return;

        isFalling |= false;

        OnLanded();
        Release();
    }

    protected abstract void OnLanded();

    protected void Release()
    {
        Managers.Pool.Push(gameObject);
    }
}
