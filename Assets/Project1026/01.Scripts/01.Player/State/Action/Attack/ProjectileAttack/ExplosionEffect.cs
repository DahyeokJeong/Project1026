using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    #region Settings
    [Header("Settings")]
    [SerializeField] private Animator anim;
    [SerializeField, Min(0.01f)] private float lifeTime = 0.25f;
    #endregion

    #region Fields
    private float timer;
    #endregion

    private void OnEnable()
    {
        timer = lifeTime;

        anim.Play("Explosion", 0, 0f);
    }

    private void Update()
    {
        if (!Managers.BattleState.CanUpdate)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
            Managers.Pool.Push(gameObject);
    }
}
