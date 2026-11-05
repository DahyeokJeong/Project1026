using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    #region Health
    private float currentHp;
    #endregion

    #region Dash Setting
    [Header("Dash Setting")]
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 3f;
    #endregion

    #region Health
    public int MaxHp { get; private set; }
    public float CurrentHp => currentHp;
    public int Defense { get; private set; }
    #endregion

    #region Attack
    public int BaseAttackDamage { get; private set; }
    public int AttackDamage =>
        BaseAttackDamage + Managers.RunData.PlayerAttackDamageBonus;

    public float AttackSpeed { get; private set; }
    public float Critical { get; private set; }
    #endregion

    #region Movement
    public float MoveSpeed { get; private set; }
    public Vector2 LastMoveDirection { get; set; }
    #endregion

    #region Dash
    public float DashDistance => dashDistance;
    public float DashDuration => dashDuration;
    public float DashCooldown => dashCooldown;
    #endregion

    #region Experience
    public float MaxExp { get; private set; }
    public float CurrentExp => Managers.RunData.CurrentExp;
    public int Level => Managers.RunData.Level;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        if (Managers.Data.IsLoaded)
        {
            Init();
            return;
        }

        Managers.Data.DataLoadCompleted += Init;
    }

    private void OnEnable()
    {
        Managers.Damage.Register(transform, ReduceHP);
    }

    private void OnDisable()
    {
        if (!Managers.Initialized)
            return;

        Managers.Damage.Unregister(transform);
        Managers.Data.DataLoadCompleted -= Init;
    }
    #endregion

    public void Init()
    {
        if (Managers.Initialized)
            Managers.Data.DataLoadCompleted -= Init;

        MaxHp = Mathf.RoundToInt(GetStatValue("STAT001"));
        BaseAttackDamage = Mathf.RoundToInt(GetStatValue("STAT003"));
        AttackSpeed = GetStatValue("STAT005");
        Defense = Mathf.RoundToInt(GetStatValue("STAT006"));
        MoveSpeed = GetStatValue("STAT007");
        Critical = GetStatValue("STAT008");
        MaxExp = GetStatValue("STAT009");

        currentHp = MaxHp;
        LastMoveDirection = Vector2.down;
    }


    private float GetStatValue(string id)
    {
        if (Managers.Data.PlayerStatDic.TryGetValue(id, out PlayerStatData stat))
            return stat.BaseValue;
        Debug.LogError($"[PlayerModel] PlayerStat을 찾을 수 없습니다. ID: {id}");
        return 0f;
    }

    public void ReduceHP(float damage)
    {
        currentHp -= damage;

        if(currentHp < 0f)
            currentHp = 0f;
        Debug.Log(currentHp);
    }

    public int CalculateExp(RunDataManager runData, float amount)
    {
        int levelUpCount = 0;

        runData.CurrentExp += amount;

        while (runData.CurrentExp >= MaxExp)
        {
            runData.CurrentExp -= MaxExp;
            runData.Level++;

            levelUpCount++;
        }

        return levelUpCount;
    }
}
