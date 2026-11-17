using TMPro;
using UnityEngine;

public class BattleHud : MonoBehaviour
{
    [SerializeField] private BattleSceneController battleController;
    [SerializeField] private TextMeshProUGUI survivalTimeText;
    [SerializeField] private TextMeshProUGUI waveInfoText;
    [SerializeField] private TextMeshProUGUI remainingTimeText;
    [SerializeField] private UnityEngine.UI.Image progressFill;

    private int displayedSeconds = -1;
    private int displayedRemaining = -1;
    private int displayedTotalWave;
    private bool displayedOvertime;

    private void LateUpdate()
    {
        if (battleController == null || battleController.TotalWave == 0)
            return;

        int seconds = Mathf.FloorToInt(battleController.SurvivalTime);
        if (seconds != displayedSeconds)
        {
            displayedSeconds = seconds;
            survivalTimeText.text = FormatTime(seconds);
        }

        int totalWave = battleController.TotalWave;
        bool waveChanged = totalWave != displayedTotalWave;
        bool bossWave = battleController.IsSpecialWave;
        if (waveChanged)
        {
            displayedTotalWave = totalWave;
            waveInfoText.text = battleController.IsEndless
                ? $"{totalWave}웨이브 · {battleController.Cycle}사이클"
                : bossWave
                    ? $"{battleController.CurrentRound}라운드 · 보스"
                    : $"{battleController.CurrentRound}라운드 · {battleController.CurrentWave}웨이브";
        }

        bool overtime = battleController.IsOvertime;
        int remaining = Mathf.CeilToInt(battleController.WaveTimeRemaining);
        if (waveChanged || overtime != displayedOvertime || (!bossWave && remaining != displayedRemaining))
        {
            displayedOvertime = overtime;
            displayedRemaining = remaining;
            remainingTimeText.text = overtime ? "토벌 연장"
                : bossWave ? "보스 토벌 중"
                : $"다음 웨이브 {FormatTime(remaining)}";
        }

        progressFill.fillAmount = battleController.WaveProgress;
    }

    private static string FormatTime(int seconds) => $"{seconds / 60:00}:{seconds % 60:00}";
}
