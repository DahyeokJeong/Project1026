using UnityEngine;

public static class LocalizationHelper
{
    public static LocalizationData Get(string key)
    {
        if(!Managers.Data.IsLoaded)
            return null;

        if(Managers.Data.LocalizationDic.TryGetValue(key, out LocalizationData data))
            return data;

        return null;
    }

    public static string GetName(string key)
    {
        LocalizationData data = Get(key);

        return data != null ? data.NameKO : key;
    }

    public static string GetDescription(string key)
    {
        LocalizationData data = Get(key);

        return data != null ? data.DescriptionKO : string.Empty;
    }
}
