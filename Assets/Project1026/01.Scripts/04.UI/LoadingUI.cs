using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    #region Components
    [Header("Component")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private Image background;

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

        switch (sceneName)
        {
            case "01.LobbyScene":
                selectedSprite = villageBackground;
                break;

            case "03.BattleScene":
                int stage = Managers.GameState.SelectedStage;

                switch (stage)
                {
                    case 1:
                        selectedSprite = stage1Background;
                        break;

                    case 2:
                        selectedSprite = stage2Background;
                        break;

                    case 3:
                        selectedSprite = stage3Background;
                        break;
                }
                break;
        }

        if (selectedSprite != null)
            background.sprite = selectedSprite;
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
