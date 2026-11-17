using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public List<int> clearedStages = new();
    public List<string> completedEvents = new();
    // Keep legacy per-stage records when rewriting older saves.
    public List<EndlessRecord> endlessRecords = new();
    public float endlessBestSurvivalTime;
    public int endlessHighestWave;
    public int soulEssence;
    public List<int> soulBookLevels = new();
}

[Serializable]
public struct EndlessRecord
{
    public int stageId;
    public float survivalSeconds;
    public int wave;
}
