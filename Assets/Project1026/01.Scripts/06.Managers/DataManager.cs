using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DataManager
{
    public enum InitState
    {
        None,
        Loading,
        Completed,
        Failed
    }

    #region Events
    public event Action DataLoadCompleted;
    public event Action DataLoadFailed;
    #endregion

    #region Properties
    public InitState State { get; private set; } = InitState.None;
    public bool IsLoaded => State == InitState.Completed;

    public Dictionary<string, PlayerStatData> PlayerStatDic { get; private set; } = new();
    public Dictionary<string, AugmentData> AugmentDic { get; private set; } = new();
    public Dictionary<string, LocalizationData> LocalizationDic { get; private set; } = new();
    #endregion

    #region Fields
    private readonly List<AsyncOperationHandle> handles = new();
    private readonly List<Action<int>> loadTasks = new();
    private Dictionary<string, AugmentEffectSO> augmentEffects = new();

    private int completedCount;
    private int loadVersion;
    #endregion

    public void Init()
    {
        if (State == InitState.Loading || State == InitState.Completed)
            return;

        Clear();

        State = InitState.Loading;

        int version = loadVersion;

        RegisterLoadTasks();

        if (loadTasks.Count == 0)
        {
            CompleteLoad();
            return;
        }

        foreach (Action<int> task in loadTasks)
        {
            if (State != InitState.Loading)
                break;

            task.Invoke(version);
        }
    }

    private void RegisterLoadTasks()
    {
        loadTasks.Clear();

        loadTasks.Add(version => LoadJson<PlayerStatLoader>("PlayerStat", SetPlayerStats, version));

        loadTasks.Add(version => LoadJson<AugmentLoader>("Augment", SetAugments, version));

        loadTasks.Add(version => LoadJson<LocalizationLoader>("Localization_KO", SetLocalizations, version));

        loadTasks.Add(LoadAugmentEffects);
    }

    private void LoadAugmentEffects(int version)
    {
        const string label = "AugmentEffects";
        var handle = Addressables.LoadAssetsAsync<AugmentEffectSO>(label, null);
        handles.Add(handle);

        handle.Completed += operation =>
        {
            if (version != loadVersion || State != InitState.Loading)
                return;

            if (operation.Status != AsyncOperationStatus.Succeeded ||
                operation.Result == null || operation.Result.Count == 0)
            {
                FailLoad(label);
                return;
            }

            Dictionary<string, AugmentEffectSO> effects = new();

            foreach (AugmentEffectSO effect in operation.Result)
            {
                if (effect == null || string.IsNullOrWhiteSpace(effect.ConfigKey))
                {
                    Debug.LogError("증강 효과 SO 또는 ConfigKey가 비어 있습니다.");
                    FailLoad(label);
                    return;
                }

                if (!effects.TryAdd(effect.ConfigKey, effect))
                {
                    Debug.LogError($"중복된 증강 효과 ConfigKey: {effect.ConfigKey}");
                    FailLoad(label);
                    return;
                }
            }

            augmentEffects = effects;
            completedCount++;

            if (completedCount == loadTasks.Count)
                CompleteLoad();
        };
    }

    private void LoadJson<T>(
        string address,
        Action<T> onLoaded,
        int version)
    {
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(address);

        handles.Add(handle);

        handle.Completed += operation =>
        {
            if (version != loadVersion ||
                State != InitState.Loading)
                return;

            if (operation.Status != AsyncOperationStatus.Succeeded ||
                operation.Result == null)
            {
                FailLoad(address);
                return;
            }

            try
            {
                T loader = JsonUtility.FromJson<T>(
                    operation.Result.text);

                if (loader == null)
                    throw new Exception("JSON 역직렬화 결과가 null입니다.");

                onLoaded?.Invoke(loader);
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 처리 실패 / Address: {address} / 오류: {e.Message}");

                FailLoad(address);
                return;
            }

            completedCount++;

            if (completedCount == loadTasks.Count)
                CompleteLoad();
        };
    }

    private void CompleteLoad()
    {
        if (State != InitState.Loading)
            return;

        State = InitState.Completed;

        Debug.Log("DataManager 초기화 완료");

        DataLoadCompleted?.Invoke();
    }

    private void SetPlayerStats(PlayerStatLoader loader)
    {
        Dictionary<string, PlayerStatData> dictionary = new();

        foreach (PlayerStatData data in loader.PlayerStats)
        {
            if (!dictionary.TryAdd(data.Id, data))
                throw new Exception($"중복된 PlayerStat ID: {data.Id}");
        }

        PlayerStatDic = dictionary;
    }

    private void SetAugments(AugmentLoader loader)
    {
        Dictionary<string, AugmentData> dictionary = new();

        foreach (AugmentData data in loader.Augments)
        {
            if (!dictionary.TryAdd(data.Id, data))
                throw new Exception($"중복된 Augment ID: {data.Id}");
        }

        AugmentDic = dictionary;
    }

    private void SetLocalizations(LocalizationLoader loader)
    {
        Dictionary<string, LocalizationData> dictionary = new();

        foreach (LocalizationData data in loader.Localizations)
        {
            if (!dictionary.TryAdd(data.Key, data))
                throw new Exception($"중복된 Localization Key: {data.Key}");
        }

        LocalizationDic = dictionary;
    }

    public bool TryGetAugmentEffect(string configKey, out AugmentEffectSO effect)
    {
        effect = null;

        if (!IsLoaded || string.IsNullOrWhiteSpace(configKey))
            return false;

        return augmentEffects.TryGetValue(configKey, out effect);
    }

    private void FailLoad(string address)
    {
        if (State != InitState.Loading)
            return;

        State = InitState.Failed;

        PlayerStatDic = new();
        AugmentDic = new();
        augmentEffects.Clear();
        LocalizationDic = new();

        Debug.LogError($"DataManager 초기화 실패 / Address: {address}");

        DataLoadFailed?.Invoke();
    }

    public void Clear()
    {
        loadVersion++;

        foreach (AsyncOperationHandle handle in handles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        handles.Clear();
        loadTasks.Clear();

        PlayerStatDic = new();
        AugmentDic = new();
        augmentEffects.Clear();
        LocalizationDic = new();

        completedCount = 0;
        State = InitState.None;
    }
}
