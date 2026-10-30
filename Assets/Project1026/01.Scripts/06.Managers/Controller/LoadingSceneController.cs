using UnityEngine;

public class LoadingSceneController : MonoBehaviour
{
    private void OnEnable()
    {
        Managers.Loading.LoadingCompleted += OnLoadingCompleted;
        Managers.Loading.LoadingFailed += OnLoadingFailed;
    }

    private void Start()
    {
        Managers.Loading.StartLoading(this);
    }

    private void OnDisable()
    {
        Managers.Loading.LoadingCompleted -= OnLoadingCompleted;
        Managers.Loading.LoadingFailed -= OnLoadingFailed;
    }

    private void OnLoadingCompleted()
    {
        Debug.Log("LoadingSceneController : 씬 활성화");

        Managers.Scene.ActivateTargetScene();
    }

    private void OnLoadingFailed()
    {
        Debug.LogError("LoadingSceneController : 로딩 실패");
    }
}
