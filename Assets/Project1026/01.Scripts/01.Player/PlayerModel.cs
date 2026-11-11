using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    private const float AttackDamagePerLevel = 0.05f;
    private const float AttackSpeedPerLevel = 0.05f;
    private const float CriticalChancePerLevel = 2f;
    private const float AutoAttackCooldownPerLevel = 0.03f;
    private const int HealthRegenPerLevel = 1;
    private const float MoveSpeedPerLevel = 0.05f;
    private const float ExperienceGainPerLevel = 0.05f;

    #region Health Fields
    private float currentHp;

    private int baseMaxHp;
    private int baseHealthRegen;
    private int baseDefense;
    #endregion

    #region Attack Fields
    private float baseAttackSpeed;
    private float baseCritical;
    #endregion

    #region Movement Fields
    private float baseMoveSpeed;
    #endregion

    #region Initialization
    private bool initialized;
    #endregion

    #region Dash Settings
    [Header("Dash Setting")]
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashDuration = 0.5f;
    [SerializeField] private float dashCooldown = 3f;
    #endregion

    #region Properties
    public int MaxHp =>
        baseMaxHp + Managers.RunData.PlayerMaxHpBonus;

    public float CurrentHp => currentHp;

    public int HealthRegen => baseHealthRegen + Managers.RunData.PlayerHealthRegenBonus +
        SoulBookLevel(SoulBookNode.HealthRegen) * HealthRegenPerLevel;

    public int Defense =>  baseDefense + Managers.RunData.PlayerDefenseBonus;

    public bool IsDead => initialized && currentHp <= 0f;
    public int BaseAttackDamage { get; private set; }

    public int AttackDamage => BaseAttackDamage + Managers.RunData.PlayerAttackDamageBonus +
        Mathf.RoundToInt(BaseAttackDamage * SoulBookLevel(SoulBookNode.AttackDamage) * AttackDamagePerLevel);

    public float AttackSpeed => baseAttackSpeed * Managers.RunData.PlayerAttackSpeedMultiplier *
        (1f + SoulBookLevel(SoulBookNode.AttackSpeed) * AttackSpeedPerLevel);

    public float Critical => Mathf.Clamp(baseCritical + Managers.RunData.PlayerCriticalBonus +
        SoulBookLevel(SoulBookNode.CriticalChance) * CriticalChancePerLevel, 0f, 100f);

    public float AutoAttackCooldownMultiplier => Mathf.Max(0.01f, Managers.RunData.PlayerAutoAttackCooldownMultiplier *
        (1f - SoulBookLevel(SoulBookNode.AutoAttackCooldown) * AutoAttackCooldownPerLevel));
    public float MoveSpeed => baseMoveSpeed * Managers.RunData.PlayerMoveSpeedMultiplier *
        (1f + SoulBookLevel(SoulBookNode.MoveSpeed) * MoveSpeedPerLevel);

    public Vector2 LastMoveDirection { get; set; }
    public float DashDistance => dashDistance;
    public float DashDuration => dashDuration;
    public float DashCooldown => dashCooldown;
    public float MaxExp { get; private set; }
    public float CurrentExp => Managers.RunData.CurrentExp;
    public int Level => Managers.RunData.Level;
    #endregion

    #region Lifecycle
    private void Awake()
    {
        TryInitialize();
    }

    private void OnEnable()
    {
        Managers.Damage.Register(transform, ReduceHP);

        TryInitialize();
    }

    private void Update()
    {
        if (!initialized || !Managers.BattleState.CanUpdate || IsDead)
            return;

        Heal(HealthRegen * Time.deltaTime);
    }

    private void OnDisable()
    {
        if (!Managers.Initialized)
            return;

        Managers.Damage.Unregister(transform);
        Managers.Data.DataLoadCompleted -= Init;
    }
    #endregion
    private void TryInitialize()
    {
        if (initialized)
            return;

        if (Managers.Data.IsLoaded)
        {
            Init();
            return;
        }

        Managers.Data.DataLoadCompleted -= Init;
        Managers.Data.DataLoadCompleted += Init;
    }

    public void Init()
    {
        if (!Managers.Data.IsLoaded)
            return;

        Managers.Data.DataLoadCompleted -= Init;

        if (initialized)
            return;

        baseMaxHp = Mathf.RoundToInt(GetStatValue("STAT001"));
        baseHealthRegen = Mathf.RoundToInt(GetStatValue("STAT002"));

        BaseAttackDamage = Mathf.RoundToInt(GetStatValue("STAT003"));
        baseAttackSpeed = GetStatValue("STAT005");
        baseDefense = Mathf.RoundToInt(GetStatValue("STAT006"));
        baseMoveSpeed = GetStatValue("STAT007");
        baseCritical = GetStatValue("STAT008");

        MaxExp = GetStatValue("STAT009");

        currentHp = MaxHp;
        LastMoveDirection = Vector2.down;

        initialized = true;
        Managers.RunData.RefreshPlayerHp(this);
    }

    private float GetStatValue(string id)
    {
        if (Managers.Data.PlayerStatDic.TryGetValue(
            id,
            out PlayerStatData stat))
        {
            return stat.BaseValue;
        }

        Debug.LogError(
            $"[PlayerModel] PlayerStat을 찾을 수 없습니다. ID: {id}",
            this);

        return 0f;
    }
    public void ReduceHP(float damage)
    {
        if (!initialized || IsDead || damage <= 0f)
            return;

        float finalDamage = Mathf.Max(0f, damage - Defense);
        currentHp = Mathf.Max(0f, currentHp - finalDamage);
        Managers.RunData.RefreshPlayerHp(this);

        Debug.Log($"[PlayerModel] 현재 체력: {currentHp}", this);

        if (IsDead)
        {
            // 실제 사망 처리는 기존 게임 흐름에 맞춰 연결
        }
    }

    public void Heal(float amount)
    {
        if (!initialized || IsDead || amount <= 0f || currentHp == MaxHp)
            return;

        currentHp = Mathf.Min(MaxHp, currentHp + amount);
        Managers.RunData.RefreshPlayerHp(this);
    }
    private static int SoulBookLevel(SoulBookNode node) => Managers.GameState.GetSoulBookLevel(node);

    public int CalculateExp(RunDataManager runData, float amount)
    {
        if (!initialized ||
            runData == null ||
            amount <= 0f ||
            MaxExp <= 0f)
        {
            return 0;
        }

        int levelUpCount = 0;

        runData.CurrentExp += amount * (1f + SoulBookLevel(SoulBookNode.ExperienceGain) * ExperienceGainPerLevel);

        while (runData.CurrentExp >= MaxExp)
        {
            runData.CurrentExp -= MaxExp;
            runData.Level++;

            levelUpCount++;
        }

        return levelUpCount;
    }
}