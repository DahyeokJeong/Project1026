using UnityEngine;
using UnityEngine.UI;

public class StageSelectController : MonoBehaviour
{
    #region Components
    [Header("Component")]
    [SerializeField] private Button[] stageButtons;
    #endregion

    private void OnEnable()
    {
        Managers.GameState.ProgressChanged += RefreshButtons;
        RefreshButtons();
    }

    private void OnDisable()
    {
        Managers.GameState.ProgressChanged -= RefreshButtons;
    }

    private void RefreshButtons()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int stageId = i + 1;

            stageButtons[i].interactable = Managers.GameState.IsStageUnlocked(stageId);
        }
    }

    public void OnClickStage(int stageId)
    {
        if (!Managers.GameState.SelectStage(stageId))
            return;

        Debug.Log($"Stage {stageId} ¼±ÅÃ");

        Managers.Scene.LoadGame("03.BattleScene");
    }
}
