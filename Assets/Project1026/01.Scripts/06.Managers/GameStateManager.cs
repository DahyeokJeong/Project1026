using System;
using System.Collections.Generic;

public class GameStateManager
{
    #region Events
    public event Action ProgressChanged;
    #endregion

    #region Fields
    private readonly HashSet<int> clearedStages = new();
    private readonly HashSet<string> completedEvents = new();
    #endregion

    #region Properties
    public int SelectedStage { get; private set; }
    #endregion

    public bool IsStageCleared(int stageId)
    {
        return clearedStages.Contains(stageId);
    }

    public bool IsStageUnlocked(int stageId)
    {
        if (stageId < 1 || stageId > 3)
            return false;

        if (stageId == 1)
            return true;

        return IsStageCleared(stageId - 1);
    }

    public bool SelectStage(int stageId)
    {
        if (!IsStageUnlocked(stageId))
            return false;

        SelectedStage = stageId;
        return true;
    }

    public void ClearStage(int stageId)
    {
        if (stageId < 1 || stageId > 3)
            return;

        if (!clearedStages.Add(stageId))
            return;

        ProgressChanged?.Invoke();
    }

    public bool IsEventCompleted(string eventId)
    {
        return completedEvents.Contains(eventId);
    }

    public void CompleteEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
            return;

        if (!completedEvents.Add(eventId))
            return;

        ProgressChanged?.Invoke();
    }

    public SaveData GetSaveData()
    {
        return new SaveData
        {
            clearedStages = new List<int>(clearedStages),
            completedEvents = new List<string>(completedEvents)
        };
    }

    public void LoadSaveData(SaveData saveData)
    {
        if (saveData == null)
            return;

        clearedStages.Clear();
        completedEvents.Clear();

        if(saveData.clearedStages != null)
            clearedStages.UnionWith(saveData.clearedStages);

        if(saveData.completedEvents != null)
            completedEvents.UnionWith(saveData.completedEvents);

        SelectedStage = 0;

        ProgressChanged?.Invoke();
    }

    public void Clear()
    {
        clearedStages.Clear();
        completedEvents.Clear();
        SelectedStage = 0;
    }
}
