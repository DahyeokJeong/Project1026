using System;
using UnityEngine;

public class LoadingManager
{
    #region Events
    public event Action LoadingCompleted;
    public event Action LoadingFailed;
    #endregion

    #region Fields
    private MonoBehaviour runner;
    #endregion

    #region Properties
    public bool IsLoading { get; private set; }
    public bool IsDataLoaded { get; private set; }
    public bool IsSceneLoaded { get; private set; }
    public float Progress => Managers.Scene.Progress * 0.9f;
    #endregion

    public void StartLoading(MonoBehaviour runner)
    {
        if (IsLoading)
            return;

        this.runner = runner;
        IsLoading = true;
        IsDataLoaded = false;
        IsSceneLoaded = false;

        Managers.Data.DataLoadCompleted += OnDataLoadCompleted;
        Managers.Data.DataLoadFailed += OnDataLoadFailed;
        Managers.Scene.SceneLoadReady += OnSceneLoadReady;

        if (Managers.Data.IsLoaded)
            OnDataLoadCompleted();
        else
            Managers.Data.Init();
    }

    private void OnDataLoadCompleted()
    {
        if (!IsLoading || IsDataLoaded)
            return;

        IsDataLoaded = true;
        Debug.Log("LoadingManager : 데이터 로딩 완료");

        Managers.Scene.LoadTargetSceneAsync(runner);
    }

    private void OnSceneLoadReady()
    {
        if (!IsLoading)
            return;

        IsSceneLoaded = true;
        Debug.Log("LoadingManager : 씬 로딩 준비 완료");

        CheckLoadingCompleted();
    }

    private void OnDataLoadFailed()
    {
        if (!IsLoading)
            return;

        IsLoading = false;
        Debug.LogError("LoadingManager : 데이터 로딩 실패");

        UnsubscribeEvents();
        runner = null;

        LoadingFailed?.Invoke();
    }

    private void CheckLoadingCompleted()
    {
        if (!IsDataLoaded || !IsSceneLoaded)
            return;

        IsLoading = false;
        Debug.Log("LoadingManager : 모든 로딩 완료");

        UnsubscribeEvents();
        runner = null;

        LoadingCompleted?.Invoke();
    }

    private void UnsubscribeEvents()
    {
        Managers.Data.DataLoadCompleted -= OnDataLoadCompleted;
        Managers.Data.DataLoadFailed -= OnDataLoadFailed;
        Managers.Scene.SceneLoadReady -= OnSceneLoadReady;
    }

    public void Clear()
    {
        UnsubscribeEvents();
        runner = null;

        IsLoading = false;
        IsDataLoaded = false;
        IsSceneLoaded = false;
    }
}