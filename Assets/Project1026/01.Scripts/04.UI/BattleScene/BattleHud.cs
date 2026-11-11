using System.Collections;
using TMPro;
using UnityEngine;

public class BattleHud : MonoBehaviour
{
    [SerializeField] private BattleSceneController battleController;
    [SerializeField] private TMP_Text killCountText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text waveText;

    private int killCount;
    private int seconds;
    private StageSpawnData stageData;

    private void Awake()
    {
        EnemyController.EnemyDead += OnEnemyDead;
    }

    private IEnumerator Start()
    {
        yield return null;

        stageData = battleController.CurrentStageData;
        if (stageData == null)
            yield break;

        RefreshText();
    }

    private void LateUpdate()
    {
        UpdateTimerText();
        UpdateWaveText();
    }

    private void UpdateTimerText()
    {
        seconds = Mathf.CeilToInt(Mathf.Max(0f, battleController.StageTimeRemaining));

        timerText.SetText("{0}:{1}{2}", seconds / 60, seconds % 60 / 10, seconds % 10);
    }

    private void UpdateWaveText()
    {
        StageSpawnData stageData = battleController.CurrentStageData;
        if (stageData == null || battleController.CurrentWave == 0)
            return;

        waveText.SetText("¿þÀÌºê {0} / {1}", battleController.CurrentWave, stageData.WaveCount);
    }

    private void OnEnemyDead(EnemyGenerate generate)
    {
        killCount++;
        RefreshText();
    }

    private void RefreshText()
    {
        int killTarget = stageData.WaveCount * stageData.MaxCount;
        killCountText.text = $"{killCount} / {killTarget}";
    }

    private void OnDestroy()
    {
        EnemyController.EnemyDead -= OnEnemyDead;
    }

    
}
