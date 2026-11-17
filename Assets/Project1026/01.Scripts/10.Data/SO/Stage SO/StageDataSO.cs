using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDataSO", menuName = "Data/Stage/Stage")]
public class StageDataSO : ScriptableObject
{
    [SerializeField, Min(1)] private int stageId = 1;
    [SerializeField] private List<RoundDataSO> rounds = new();

    [SerializeField] private GwaegangWhiteTiger bossPrefab;
    [Header("Endless")]
    [SerializeField, Min(0)] private float endlessExpPerCycle = 30f;
    [SerializeField, Min(0)] private float endlessHpPerCycle = 0.15f;
    [SerializeField, Min(0)] private float endlessDamagePerCycle = 0.025f;
    [SerializeField, Min(0)] private int endlessBatchPerCycle = 1;
    [SerializeField, Min(0)] private float endlessEliteChancePerCycle = 0.02f;

    public GwaegangWhiteTiger BossPrefab => bossPrefab;
    public float EndlessExpPerCycle => endlessExpPerCycle;
    public float EndlessHpPerCycle => endlessHpPerCycle;
    public float EndlessDamagePerCycle => endlessDamagePerCycle;
    public int EndlessBatchPerCycle => endlessBatchPerCycle;
    public float EndlessEliteChancePerCycle => endlessEliteChancePerCycle;

    public int StageId => stageId;
    public IReadOnlyList<RoundDataSO> Rounds => rounds;
}
