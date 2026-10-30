using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    #region Properties
    public int MaxHp { get; private set; }
    public int AttackDamage { get; private set; }
    public float AttackSpeed { get; private set; }
    public float MoveSpeed { get; private set; }
    public int Defense { get; private set; }
    public float Critical { get; private set; }

    public float CurrentHp => currentHp;
    public Vector2 LastMoveDirection { get; set; }
    #endregion

    #region Fields
    private float currentHp;
    #endregion


    // 추후 변경 Manager로 값 연동 예정
    private void Awake()
    {
        if(Managers.Data.IsLoaded)
        {
            Init();
            return;
        }

        Managers.Data.DataLoadCompleted += Init;
    }

    public void Init()
    {
        if(Managers.Initialized)
            Managers.Data.DataLoadCompleted -= Init;
        MaxHp = Mathf.RoundToInt(GetStatValue("STAT001"));
        AttackDamage = Mathf.RoundToInt(GetStatValue("STAT003"));
        AttackSpeed = GetStatValue("STAT005");
        Defense = Mathf.RoundToInt(GetStatValue("STAT006"));
        MoveSpeed = GetStatValue("STAT007");
        Critical = GetStatValue("STAT008");

        currentHp = MaxHp;
        LastMoveDirection = Vector2.down;

    }

    public void ReduceHP(float damage)
    {
        currentHp -= damage;

        if(currentHp < 0f)
            currentHp = 0f;
        Debug.Log(currentHp);
    }

    private float GetStatValue(string id)
    {
        if (Managers.Data.PlayerStatDic.TryGetValue(id, out PlayerStatData stat))
            return stat.BaseValue;
        Debug.LogError($"[PlayerModel] PlayerStat을 찾을 수 없습니다. ID: {id}");
        return 0f;
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
}
