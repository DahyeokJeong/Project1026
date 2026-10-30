using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    #region Components
    [Header("Component")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text progressText;
    #endregion

    private void Update()
    {
        float progress = Managers.Loading.Progress;

        loadingSlider.value = progress;
        progressText.text = $"{Mathf.RoundToInt(progress * 100f)}%";
    }
}
