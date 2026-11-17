using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    #region Components
    [Header("Component")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private Image background;
    [SerializeField] private BackgroundDecoration backgroundDecoration;

    [Header("Background")]
    [SerializeField] private Sprite villageBackground;
    [SerializeField] private Sprite stage1Background;
    [SerializeField] private Sprite stage2Background;
    [SerializeField] private Sprite stage3Background;
    #endregion

    #region Fields
    private bool isCompleted;
    #endregion

    private void Start()
    {
        loadingSlider.minValue = 0f;
        loadingSlider.maxValue = 1f;

        SetBackground();
        SetProgress(0f);
    }

    private void Update()
    {
        if (isCompleted)
            return;

        SetProgress(Managers.Loading.Progress);
    }

    private void SetBackground()
    {
        if (background == null)
            return;

        string sceneName = Managers.Scene.TargetSceneName;
        Sprite selectedSprite = null;
        int decorationIndex = -1;

        switch (sceneName)
        {
            case "01.LobbyScene":
                selectedSprite = villageBackground;
                decorationIndex = 0;
                break;

            case "03.BattleScene":
                // Endless currently shares the Stage 3 loading artwork.
                int stage = Managers.GameState.IsEndlessSelected ? 3 : Managers.GameState.SelectedStage;

                switch (stage)
                {
                    case 1:
                        selectedSprite = stage1Background;
                        decorationIndex = 1;
                        break;

                    case 2:
                        selectedSprite = stage2Background;
                        decorationIndex = 2;
                        break;

                    case 3:
                        selectedSprite = stage3Background;
                        decorationIndex = 3;
                        break;
                }
                break;
        }

        if (selectedSprite != null)
            background.sprite = selectedSprite;

        backgroundDecoration?.Show(selectedSprite != null ? decorationIndex : -1);
    }

    private void SetProgress(float progress)
    {
        if (loadingSlider != null)
            loadingSlider.value = Mathf.Clamp01(progress);
    }

    public void CompleteLoading()
    {
        isCompleted = true;
        SetProgress(1f);
    }
}
