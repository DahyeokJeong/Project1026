using System.Collections.Generic;
using UnityEngine;

public class BattleSceneController : MonoBehaviour
{
    #region References
    [Header("References")]
    [SerializeField] private EnemyGenerate enemyGenerate;
    #endregion
    #region Data
    [Header("Stage Data")]
    [SerializeField] private List<StageSpawnData> stageSpawnDatas = new();
    #endregion
    #region fields
    private IStageClearCondition condition;
    #endregion
    private void Start()
    {
        if (enemyGenerate == null || !Managers.Data.IsLoaded)
            return;

        int stageId = Managers.GameState.SelectedStage;

        if (!TryGetStageData(stageId, out StageSpawnData data))
            return;

        condition = new DefeatAllEnemiesCondition(enemyGenerate);
        enemyGenerate.StartSpawning(data);
    }

    private void Update()
    {
        if (condition == null || !enemyGenerate.IsSpawning)
            return;

        if (!condition.IsSatisfied())
            return;

        enemyGenerate.StopSpawning();
        OnBattleCleared();
    }

    private void OnBattleCleared()
    {
        int stageId = Managers.GameState.SelectedStage;

        Managers.GameState.ClearStage(stageId);

        ReturnToStageSelect();
    }

    private void ReturnToStageSelect()
    {
        Managers.Pool.Clear();
        Managers.Scene.LoadGame("02.StageSelectScene");
    }

    private bool TryGetStageData(int stageId, out StageSpawnData selectedData)
    {
        selectedData = null;

        if (stageId <= 0)
            return false;

        foreach (StageSpawnData data in stageSpawnDatas)
        {
            if (data == null || data.StageId != stageId)
                continue;

            if (selectedData != null)
                return false;

            selectedData = data;
        }

        if (selectedData == null)
            return false;

        return true;
    }
}