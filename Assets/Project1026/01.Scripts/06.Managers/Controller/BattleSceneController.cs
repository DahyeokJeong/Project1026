using System.Collections.Generic;
using UnityEngine;

public class BattleSceneController : MonoBehaviour
{
    #region References
    [Header("References")]
    [SerializeField] private EnemyGenerate enemyGenerate;
    [SerializeField] private AugmentPanelUI augment;
    #endregion

    #region Data
    [Header("Stage Data")]
    [SerializeField] private List<StageSpawnData> stageSpawnDatas = new();
    #endregion

    #region fields
    private IStageClearCondition condition;
    private StageSpawnData currentStageData;
    private int currentWave;
    private float waveTimeRemaining;
    private bool allWavesEnded;
    #endregion

    #region Properties
    public StageSpawnData CurrentStageData => currentStageData;
    public EnemyGenerate EnemyGenerate => enemyGenerate;
    public int CurrentWave => currentWave;
    public float StageTimeRemaining =>
    currentStageData == null || allWavesEnded ? 0f : Mathf.Max(0f, waveTimeRemaining) + (currentStageData.WaveCount - currentWave) * currentStageData.WaveDuration;
    #endregion

    private void Start()
    {
        Managers.BattleState.Reset();

        if (enemyGenerate == null || !Managers.Data.IsLoaded)
            return;

        int stageId = Managers.GameState.SelectedStage;

        if (!TryGetStageData(stageId, out StageSpawnData data))
            return;

        currentStageData = data;
        condition = new DefeatAllEnemiesCondition(enemyGenerate);
        StartNextWave();
    }

    private void Update()
    {
        if (condition == null)
            return;

        if (allWavesEnded)
        {
            if (condition.IsSatisfied())
            {
                condition = null;
                OnBattleCleared();
            }

            return;
        }

        if (!Managers.BattleState.CanUpdate || !enemyGenerate.IsSpawning)
            return;

        waveTimeRemaining -= Time.deltaTime;

        bool currentWaveDefeated =
            enemyGenerate.HasSpawnedAll &&
            enemyGenerate.CurrentWaveActiveEnemyCount == 0;

        if (waveTimeRemaining > 0f && !currentWaveDefeated)
            return;

        EndCurrentWave();
    }

    private void EndCurrentWave()
    {
        enemyGenerate.StopSpawning();

        if (currentWave < currentStageData.WaveCount)
        {
            StartNextWave();
            return;
        }

        allWavesEnded = true;
    }

    private void StartNextWave()
    {
        enemyGenerate.StartSpawning(currentStageData);

        if(!enemyGenerate.IsSpawning)
        {
            condition = null;
            return;
        }

        currentWave++;
        waveTimeRemaining = currentStageData.WaveDuration;
    }

    private void OnBattleCleared()
    {
        int stageId = Managers.GameState.SelectedStage;

        Managers.GameState.CompleteBattleStage(stageId);

        ReturnToLobby();
    }

    private void ReturnToLobby()
    {
        Managers.Scene.LoadGame("01.LobbyScene");
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
    private void OnLevelUp(int levelUpCount)
    {
        if (levelUpCount <= 0 || augment == null)
            return;

        Managers.BattleState.SetState(BattleState.AugmentSelecting);

        augment.Show(levelUpCount);
    }

    private void OnEnable()
    {
        Managers.RunData.LevelUpOccurred += OnLevelUp;
    }
    private void OnDisable()
    {
        if (!Managers.Initialized)
            return;

        Managers.RunData.LevelUpOccurred -= OnLevelUp;
    }
}
