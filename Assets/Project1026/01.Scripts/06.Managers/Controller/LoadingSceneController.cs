using System.Collections;
using UnityEngine;

public class LoadingSceneController : MonoBehaviour
{
    private bool isLoadingCompleted;

    private void OnEnable()
    {
        Managers.Loading.LoadingCompleted += OnLoadingCompleted;
        Managers.Loading.LoadingFailed += OnLoadingFailed;
    }

    private IEnumerator Start()
    {
        Managers.Pool.Clear();
        yield return null;

        StartCoroutine(WaitForLoading());
        Managers.Loading.StartLoading(this);
    }

    private IEnumerator WaitForLoading()
    {
        while (!isLoadingCompleted)
            yield return null;

        Managers.Scene.ActivateTargetScene();
    }

    private void OnLoadingCompleted()
    {
        isLoadingCompleted = true;
        Debug.Log("LoadingSceneController : 로딩 완료");
    }

    private void OnLoadingFailed()
    {
        Debug.LogError("LoadingSceneController : 로딩 실패");
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        Managers.Loading.LoadingCompleted -= OnLoadingCompleted;
        Managers.Loading.LoadingFailed -= OnLoadingFailed;
    }
}