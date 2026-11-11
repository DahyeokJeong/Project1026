using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] private BattleSceneController battleController;
    [SerializeField] private TMP_Text killCountText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private Image roundProgressFill;
    [SerializeField] private RectTransform waveTrack;
    [SerializeField] private TMP_Text waveLabelTemplate;

    private static readonly Color ActiveWaveColor = new(1f, 0.81f, 0.35f);
    private static readonly Color InactiveWaveColor = new(0.92f, 0.78f, 0.62f);

    private readonly List<TMP_Text> waveLabels = new();
    private readonly List<RectTransform> waveDividers = new();
    private int displayedRound;
    private int displayedWave;
    private int displayedKills = -1;
    private int displayedSpawns = -1;

    private void LateUpdate()
    {
        if (battleController == null || battleController.CurrentRoundData == null)
            return;

        int round = battleController.CurrentRound;
        if (displayedRound != round)
        {
            displayedRound = round;
            displayedWave = 0;
            roundText.SetText("{0}라운드", round);
            LayoutWaves(battleController.CurrentRoundData.Waves);
        }

        int wave = battleController.CurrentWave;
        if (displayedWave != wave)
        {
            displayedWave = wave;
            for (int index = 0; index < waveLabels.Count; index++)
                waveLabels[index].color = index + 1 == wave ? ActiveWaveColor : InactiveWaveColor;
        }

        roundProgressFill.fillAmount = battleController.RoundProgress;

        EnemyGenerate spawner = battleController.EnemyGenerate;
        int kills = spawner.RoundKillCount;
        int spawns = spawner.RoundSpawnedCount;
        if (displayedKills == kills && displayedSpawns == spawns)
            return;

        displayedKills = kills;
        displayedSpawns = spawns;
        killCountText.SetText("{0} / {1}", kills, spawns);
    }

    private void LayoutWaves(IReadOnlyList<WaveDataSO> waves)
    {
        float totalDuration = 0f;
        foreach (WaveDataSO wave in waves)
            totalDuration += wave.Duration;

        while (waveLabels.Count < waves.Count)
            waveLabels.Add(Instantiate(waveLabelTemplate, waveTrack));

        while (waveDividers.Count < waves.Count - 1)
            waveDividers.Add(CreateDivider());

        float elapsed = 0f;
        for (int index = 0; index < waveLabels.Count; index++)
        {
            TMP_Text label = waveLabels[index];
            bool visible = index < waves.Count;
            label.gameObject.SetActive(visible);
            if (!visible)
                continue;

            float start = elapsed / totalDuration;
            elapsed += waves[index].Duration;
            float end = elapsed / totalDuration;
            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(start, 1f);
            rect.anchorMax = new Vector2(end, 1f);
            rect.anchoredPosition = new Vector2(0f, 16f);
            rect.sizeDelta = new Vector2(0f, 28f);
            label.SetText("웨이브 {0}", index + 1);

            if (index < waves.Count - 1)
            {
                RectTransform divider = waveDividers[index];
                divider.anchorMin = new Vector2(end, 0f);
                divider.anchorMax = new Vector2(end, 1f);
                divider.anchoredPosition = Vector2.zero;
                divider.sizeDelta = new Vector2(3f, 6f);
            }
        }

        for (int index = 0; index < waveDividers.Count; index++)
            waveDividers[index].gameObject.SetActive(index < waves.Count - 1);
    }

    private RectTransform CreateDivider()
    {
        var divider = new GameObject("Wave Divider", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)divider.transform;
        rect.SetParent(waveTrack, false);
        Image image = divider.GetComponent<Image>();
        image.color = ActiveWaveColor;
        image.raycastTarget = false;
        return rect;
    }
}
