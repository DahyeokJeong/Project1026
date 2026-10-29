using UnityEngine;

public class Managers : MonoBehaviour
{
    #region Instance
    private static Managers s_instance;

    private static Managers Instance
    {
        get
        {
            Init();
            return s_instance;
        }
    }
    #endregion

    #region Core
    private PoolManager pool = new PoolManager();
    private DamageManager damage = new DamageManager();
    #endregion

    #region Properties
    public static bool Initialized => s_instance != null;
    public static PoolManager Pool => Instance.pool;
    public static DamageManager Damage => Instance.damage;
    #endregion

    private void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        s_instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (s_instance != this)
            return;

        pool.Clear();
        damage.Clear();
        s_instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    public static void Initialize()
    {
        Init();
    }

    private static void Init()
    {
        if (s_instance != null)
            return;

        GameObject go = GameObject.Find("@Managers");

        if (go == null)
            go = new GameObject("@Managers");

        s_instance = go.GetComponent<Managers>();

        if (s_instance == null)
            s_instance = go.AddComponent<Managers>();

        DontDestroyOnLoad(go);
    }
}