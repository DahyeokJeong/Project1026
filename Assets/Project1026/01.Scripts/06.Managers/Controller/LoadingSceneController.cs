using System.Collections;
using UnityEngine;

public class LoadingSceneController : MonoBehaviour
{
    #region Settings
    [Header("Loading Settings")]
    [SerializeField] private float minimumLoadingTime = 3f;
    #endregion

    #region Fields
    private bool isLoadingCompleted;
    private float elapsedTime;
    #endregion

    private void OnEnable()
    {
        Managers.Loading.LoadingCompleted += OnLoadingCompleted;
        Managers.Loading.LoadingFailed += OnLoadingFailed;
    }

    private void Start()
    {
        Managers.Loading.StartLoading(this);
        StartCoroutine(WaitForLoading());
    }

    private IEnumerator WaitForLoading()
    {
        while (!isLoadingCompleted || elapsedTime < minimumLoadingTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

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
