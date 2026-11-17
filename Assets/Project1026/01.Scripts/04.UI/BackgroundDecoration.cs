using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class BackgroundDecoration : MonoBehaviour
{
    [Serializable]
    private struct Track
    {
        public Vector2 start;
        public Vector2 end;
        public Vector2 size;
        public float duration;
        public float phase;
        public float rotation;
        public float rotationTravel;
    }

    [Serializable]
    private sealed class Effect
    {
        public Sprite sprite;
        public Color tint;
        public bool flicker;
        public Track[] tracks;
    }

    [SerializeField] private int initialEffect;
    [SerializeField] private Effect[] effects;

    private Image[] images;
    private Effect currentEffect;
    private int currentIndex = -1;
    private float elapsed;

    private void Awake()
    {
        if (initialEffect >= 0)
            Show(initialEffect);
    }

    public void Show(int index)
    {
        if (index == currentIndex)
            return;

        if (images != null)
        {
            foreach (Image image in images)
            {
                image.gameObject.SetActive(false);
                Destroy(image.gameObject);
            }
        }

        images = null;
        currentEffect = null;
        currentIndex = -1;

        if (effects == null || index < 0 || index >= effects.Length || effects[index].sprite == null)
            return;

        currentIndex = index;
        currentEffect = effects[index];
        elapsed = 0f;
        images = new Image[currentEffect.tracks.Length];

        for (int i = 0; i < images.Length; i++)
        {
            GameObject decoration = new GameObject("Decoration", typeof(RectTransform), typeof(Image));
            decoration.layer = gameObject.layer;
            decoration.transform.SetParent(transform, false);

            Image image = decoration.GetComponent<Image>();
            image.sprite = currentEffect.sprite;
            image.raycastTarget = false;
            images[i] = image;
        }

        UpdateTracks();
    }

    private void Update()
    {
        if (currentEffect == null)
            return;

        elapsed += Time.unscaledDeltaTime;
        UpdateTracks();
    }

    private void UpdateTracks()
    {
        for (int i = 0; i < images.Length; i++)
        {
            Track track = currentEffect.tracks[i];
            float progress = elapsed / Mathf.Max(0.1f, track.duration) + track.phase;
            float cycle = Mathf.Repeat(progress, 1f);
            Color color = currentEffect.tint;

            if (currentEffect.flicker)
            {
                float pulse = 0.76f + 0.16f * Mathf.Sin(progress * 2f * Mathf.PI)
                    + 0.08f * Mathf.Sin(progress * 6f * Mathf.PI + 0.7f);
                color.a *= pulse;
                cycle = 0f;
            }
            else
            {
                float fadeIn = Mathf.SmoothStep(0f, 1f, cycle / 0.2f);
                float fadeOut = Mathf.SmoothStep(0f, 1f, (1f - cycle) / 0.25f);
                color.a *= fadeIn * fadeOut;
            }

            Image image = images[i];
            RectTransform rect = image.rectTransform;
            // The layer stretches with the BG so positions and sizes follow the original artwork.
            Vector2 center = Vector2.Lerp(track.start, track.end, cycle);
            rect.anchorMin = center - track.size * 0.5f;
            rect.anchorMax = center + track.size * 0.5f;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.Euler(0f, 0f, track.rotation + track.rotationTravel * cycle);
            image.color = color;
        }
    }
}
