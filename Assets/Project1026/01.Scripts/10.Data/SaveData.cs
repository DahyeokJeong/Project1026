using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public List<int> clearedStages = new();
    public List<string> completedEvents = new();
}
