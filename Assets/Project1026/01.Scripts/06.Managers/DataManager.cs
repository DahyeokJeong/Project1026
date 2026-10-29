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

    public event Action DataLoadCompleted;
    public event Action DataLoadFailed;

    public InitState State { get; private set; } = InitState.None;
    public bool IsLoaded => State == InitState.Completed;

    #region Properties
    public Dictionary<string, PlayerStatData> PlayerStatDic { get; private set; } = new();
    public Dictionary<string, AugmentData> AugmentDic { get; private set; } = new();
    public Dictionary<string, LocalizationData> LocalizationDic { get; private set; } = new();
    #endregion

    private readonly List<AsyncOperationHandle<TextAsset>> handles = new();

    private int completedCount;
    private int loadVersion;

    public void Init()
    {
        if (State == InitState.Loading || State == InitState.Completed)
            return;

        Clear();

        State = InitState.Loading;
        completedCount = 0;

        int version = loadVersion;

        // 향후 새로운 데이터 추가될 때 자동 탐색 구조로 개선할 필요
        LoadJson<PlayerStatLoader>("PlayerStat", loader => SetPlayerStats(loader), version);

        LoadJson<AugmentLoader>("Augment", loader => SetAugments(loader), version);

        LoadJson<LocalizationLoader>("Localization_KO", loader => SetLocalizations(loader), version);
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
            if (version != loadVersion || State != InitState.Loading)
                return;

            if (operation.Status != AsyncOperationStatus.Succeeded ||
                operation.Result == null)
            {
                FailLoad(address);
                return;
            }

            try
            {
                T loader = JsonUtility.FromJson<T>(operation.Result.text);

                if (loader == null)
                    throw new Exception("JSON 역직렬화 결과가 null입니다.");

                onLoaded?.Invoke(loader);

                completedCount++;

                // 하드코딩 부분
                if (completedCount == 3)
                {
                    State = InitState.Completed;
                    Debug.Log("DataManager 초기화 완료");
                    DataLoadCompleted?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"데이터 처리 실패 / Address: {address} / 오류: {e.Message}");

                FailLoad(address);
            }
        };
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

    private void FailLoad(string address)
    {
        if (State != InitState.Loading)
            return;

        State = InitState.Failed;

        Debug.LogError($"DataManager 초기화 실패 / Address: {address}");

        DataLoadFailed?.Invoke();
    }

    public void Clear()
    {
        loadVersion++;

        foreach (AsyncOperationHandle<TextAsset> handle in handles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        handles.Clear();

        PlayerStatDic.Clear();
        AugmentDic.Clear();
        LocalizationDic.Clear();

        completedCount = 0;
        State = InitState.None;
    }
}
