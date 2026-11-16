using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDataSO", menuName = "Data/Stage/Stage")]
public class StageDataSO : ScriptableObject
{
    [SerializeField, Min(1)] private int stageId = 1;
    [SerializeField] private List<RoundDataSO> rounds = new();

    [SerializeField] private GwaegangWhiteTiger bossPrefab;
    public GwaegangWhiteTiger BossPrefab => bossPrefab;

    public int StageId => stageId;
    public IReadOnlyList<RoundDataSO> Rounds => rounds;
}
