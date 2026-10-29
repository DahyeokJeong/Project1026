using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class PlayerStatLoader : IDataLoader
{
    public List<PlayerStatData> PlayerStats = new();

    public IList GetDataList()
    {
        return PlayerStats;
    }
}
