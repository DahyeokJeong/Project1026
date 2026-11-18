using System.Collections;
using UnityEngine;

public sealed class StagePageIndicator : MonoBehaviour
{
    private const float DotWidth = 11f;
    private const float SelectedWidth = 30f;
    private const float Spacing = 12f;
    private const float TransitionDuration = 0.18f;
    private static readonly Color SelectedColor = new Color32(224, 173, 100, 255);
    private static readonly Color DotColor = new Color32(184, 163, 139, 255);

    [SerializeField] private StageCardCarousel carousel;
    [SerializeField] private UnityEngine.UI.Image[] pages;

    private Coroutine transition;

    private void OnEnable()
    {
        carousel.CenteredCardChanged += OnCenteredCardChanged;
        Render(carousel.CenteredCardIndex ?? 0);
    }

    private void OnDisable()
    {
        carousel.CenteredCardChanged -= OnCenteredCardChanged;
        StopTransition();
    }

    private void OnCenteredCardChanged(int? index)
    {
        // Keep the last centered page highlighted while the carousel is moving.
        if (!index.HasValue)
            return;

        StopTransition();
        transition = StartCoroutine(Animate(index.Value));
    }

    private void StopTransition()
    {
        if (transition == null)
            return;

        StopCoroutine(transition);
        transition = null;
    }

    private IEnumerator Animate(int index)
    {
        var widths = new float[pages.Length];
        var colors = new Color[pages.Length];
        for (int i = 0; i < pages.Length; i++)
        {
            widths[i] = pages[i].rectTransform.sizeDelta.x;
            colors[i] = pages[i].color;
        }

        float elapsed = 0f;
        while (elapsed < TransitionDuration)
        {
            yield return null;
            elapsed += Time.unscaledDeltaTime;
            Render(index, Mathf.SmoothStep(0f, 1f, elapsed / TransitionDuration), widths, colors);
        }

        transition = null;
    }

    private void Render(int index, float progress = 1f, float[] widths = null, Color[] colors = null)
    {
        float left = -(SelectedWidth + (pages.Length - 1) * (DotWidth + Spacing)) * 0.5f;
        for (int i = 0; i < pages.Length; i++)
        {
            var page = pages[i];
            bool selected = i == index;
            float width = selected ? SelectedWidth : DotWidth;
            Color color = selected ? SelectedColor : DotColor;
            if (widths != null)
            {
                width = Mathf.Lerp(widths[i], width, progress);
                color = Color.Lerp(colors[i], color, progress);
            }

            RectTransform rect = page.rectTransform;
            rect.sizeDelta = new Vector2(width, DotWidth);
            rect.anchoredPosition = new Vector2(left + width * 0.5f, 0f);
            page.color = color;
            left += width + Spacing;
        }
    }
}
