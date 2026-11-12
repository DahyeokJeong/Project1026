using System.Collections.Generic;
using UnityEngine;

public class BattleSceneController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyGenerate enemyGenerate;
    [SerializeField] private AugmentPanelUI augment;
    [SerializeField] private PlayerModel player;

    [Header("Stage Data")]
    [SerializeField] private List<StageDataSO> stageSpawnDatas = new();

    private StageDataSO currentStageData;
    private int roundIndex;
    private int waveIndex;
    private float waveTimeRemaining;
    private float roundDuration;
    private float remainingLaterWavesDuration;
    private bool battleRunning;
    private bool waitingForRoundKills;

    public RoundDataSO CurrentRoundData => currentStageData == null ? null : currentStageData.Rounds[roundIndex];

    public EnemyGenerate EnemyGenerate => enemyGenerate;
    public int CurrentRound => currentStageData == null ? 0 : roundIndex + 1;
    public int CurrentWave => currentStageData == null ? 0 : waveIndex + 1;
    public int RoundWaveCount => currentStageData == null ? 0 : CurrentRoundData.Waves.Count;
    public float RoundTimeRemaining { get; private set; }
    public float RoundProgress => roundDuration > 0f
        ? Mathf.Clamp01(1f - RoundTimeRemaining / roundDuration)
        : 0f;

    private void Start()
    {
        Managers.BattleState.Reset();

        if (enemyGenerate == null || !Managers.Data.IsLoaded)
            return;

        int stageId = Managers.GameState.SelectedStage;
        if (player == null || !TryGetStageData(stageId, out StageDataSO data))
        {
            Debug.LogError($"Battle setup is missing a player or a unique stage asset for stage {stageId}.", this);
            return;
        }

        if (!HasValidRounds(data))
        {
            Debug.LogError($"Stage {stageId} must have rounds with valid waves and durations.", data);
            return;
        }

        currentStageData = data;
        battleRunning = true;
        StartRound();
    }

    private void LateUpdate()
    {
        if (!battleRunning)
            return;

        if (player == null || player.IsDead)
        {
            EndBattle(false);
            return;
        }

        if (!Managers.BattleState.CanUpdate)
            return;

        if (waitingForRoundKills)
        {
            TryCompleteRound();
            return;
        }

        waveTimeRemaining = Mathf.Max(0f, waveTimeRemaining - Time.deltaTime);
        RoundTimeRemaining = Mathf.Max(0f, remainingLaterWavesDuration + waveTimeRemaining);
        if (waveTimeRemaining <= 0f)
            EndCurrentWave();
    }

    private void StartRound()
    {
        waitingForRoundKills = false;
        waveIndex = 0;
        roundDuration = 0f;
        foreach (WaveDataSO wave in CurrentRoundData.Waves)
            roundDuration += wave.Duration;

        RoundTimeRemaining = roundDuration;
        enemyGenerate.BeginRound();
        StartWave();
    }

    private void StartWave()
    {
        WaveDataSO wave = CurrentRoundData.Waves[waveIndex];
        waveTimeRemaining = wave.Duration;
        remainingLaterWavesDuration = Mathf.Max(0f, RoundTimeRemaining - wave.Duration);
        enemyGenerate.StartSpawning(wave);

        if (!enemyGenerate.IsSpawning)
        {
            Debug.LogError($"Could not start stage {currentStageData.StageId}, round {CurrentRound}, wave {CurrentWave}.", wave);
            EndBattle(false);
        }
    }

    private void EndCurrentWave()
    {
        enemyGenerate.StopSpawning();

        if (waveIndex + 1 < RoundWaveCount)
        {
            waveIndex++;
            StartWave();
            return;
        }

        waitingForRoundKills = true;
        TryCompleteRound();
    }

    private void TryCompleteRound()
    {
        if (enemyGenerate.RoundKillCount < enemyGenerate.RoundSpawnedCount)
            return;

        if (roundIndex + 1 < currentStageData.Rounds.Count)
        {
            roundIndex++;
            StartRound();
            return;
        }

        EndBattle(true);
    }

    public void End()
    {
        EndBattle(false);
    }

    private void EndBattle(bool cleared)
    {
        battleRunning = false;
        enemyGenerate.StopSpawning();
        Managers.BattleState.SetState(cleared ? BattleState.Cleared : BattleState.Failed);

        if (cleared)
        {
            Managers.GameState.CompleteBattleStage(currentStageData.StageId);
            // 클리어시 셀렉트 화면으로 이동 or 유지
        }

        Managers.RunData.Reset();
        Managers.Scene.LoadGame("01.LobbyScene");
    }

    private bool TryGetStageData(int stageId, out StageDataSO selectedData)
    {
        selectedData = null;
        if (stageId <= 0)
            return false;

        foreach (StageDataSO data in stageSpawnDatas)
        {
            if (data == null || data.StageId != stageId)
                continue;

            if (selectedData != null)
                return false;

            selectedData = data;
        }

        return selectedData != null;
    }

    private static bool HasValidRounds(StageDataSO stage)
    {
        if (stage.Rounds.Count == 0)
            return false;

        foreach (RoundDataSO round in stage.Rounds)
        {
            if (round == null || round.Waves.Count == 0)
                return false;

            foreach (WaveDataSO wave in round.Waves)
            {
                if (wave == null || wave.Duration <= 0f)
                    return false;
            }
        }

        return true;
    }

    private void OnLevelUp(int levelUpCount)
    {
        if (!battleRunning || levelUpCount <= 0 || augment == null)
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
        if (Managers.Initialized)
            Managers.RunData.LevelUpOccurred -= OnLevelUp;
    }
}
