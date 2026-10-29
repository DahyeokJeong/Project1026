using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class LocalizationLoader : IDataLoader
{
    public List<LocalizationData> Localizations = new();

    public IList GetDataList()
    {
        return Localizations;
    }
}
