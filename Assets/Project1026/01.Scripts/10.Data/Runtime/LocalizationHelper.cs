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

    public static string GetDescription(string key, int augmentLevel = 1)
    {
        LocalizationData data = Get(key);

        string description = data?.DescriptionKO ?? string.Empty;
        if (description.Contains("{bonusValue}") && AugmentHelper.Get(key) is AugmentData augment)
            return description.Replace("{bonusValue}", (augment.Value * augmentLevel).ToString());

        return description;
    }
}
