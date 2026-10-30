using UnityEngine;

public static class AugmentHelper
{
    public static AugmentData Get(string id)
    {
        if (!Managers.Data.IsLoaded)
        {
            Debug.LogError($"[Augment] DataManager 로딩이 완료되지 않았습니다. Id: {id}");
            return null;
        }

        if (Managers.Data.AugmentDic.TryGetValue(id, out AugmentData data))
            return data;

        Debug.LogError(
            $"[Augment] 데이터를 찾을 수 없습니다. Id: {id}");

        return null;
    }
}