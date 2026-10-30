using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager
{
    #region Events
    public event Action SceneLoadReady;
    #endregion

    #region Fields
    private string targetSceneName;
    private AsyncOperation sceneOperation;
    private Coroutine loadCoroutine;
    #endregion

    #region Properties
    public string TargetSceneName => targetSceneName;
    public float Progress => sceneOperation != null
        ? Mathf.Clamp01(sceneOperation.progress / 0.9f)
        : 0f;
    #endregion

    public void LoadGame(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        targetSceneName = sceneName;

        SceneManager.LoadScene("LoadingScene");
    }

    public void LoadTargetSceneAsync(MonoBehaviour runner)
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("GameSceneManager : 목적지 씬이 없습니다.");
            return;
        }

        if (loadCoroutine != null)
            return;

        loadCoroutine = runner.StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        sceneOperation = SceneManager.LoadSceneAsync(targetSceneName);
        sceneOperation.allowSceneActivation = false;

        while (sceneOperation.progress < 0.9f)
            yield return null;

        SceneLoadReady?.Invoke();
    }

    public void ActivateTargetScene()
    {
        if (sceneOperation == null)
            return;

        if (sceneOperation.progress < 0.9f)
            return;

        sceneOperation.allowSceneActivation = true;
        loadCoroutine = null;
    }
}
