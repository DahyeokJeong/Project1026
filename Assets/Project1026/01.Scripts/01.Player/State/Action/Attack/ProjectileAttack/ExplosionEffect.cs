using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    #region Settings
    [Header("Settings")]
    [SerializeField, Min(0.01f)] private float lifeTime = 0.25f;
    #endregion

    #region Fields
    private float timer;
    #endregion

    private void OnEnable()
    {
        timer = lifeTime;
    }

    private void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Managers.Pool.Push(gameObject);
        }
    }
}
