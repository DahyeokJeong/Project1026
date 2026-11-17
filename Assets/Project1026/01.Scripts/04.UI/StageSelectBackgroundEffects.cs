using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class StageSelectBackgroundEffects : MonoBehaviour
{
    [Serializable]
    private sealed class DriftLayer
    {
        [SerializeField] private Image first;
        [SerializeField] private Image second;
        [SerializeField, Min(1f)] private float duration = 36f;
        [SerializeField, Range(-0.2f, 0.2f)] private float horizontalTravel = 0.05f;
        [SerializeField, Range(0f, 1f)] private float initialPhase = 0.5f;
        [SerializeField, Range(0f, 1f)] private float opacity = 1f;
        [SerializeField, Range(0f, 0.3f)] private float opacityVariation;

        private Vector2 firstOrigin;
        private Vector2 secondOrigin;

        public void CacheOrigins()
        {
            firstOrigin = first.rectTransform.anchoredPosition;
            secondOrigin = second.rectTransform.anchoredPosition;
        }

        public void Apply(double elapsed, float backgroundWidth)
        {
            double cycle = elapsed / duration + initialPhase;
            float phase = (float)(cycle - Math.Floor(cycle));
            float secondaryPhase = Mathf.Repeat(phase + 0.5f, 1f);
            float travel = horizontalTravel * backgroundWidth;

            float alpha = opacity;
            if (opacityVariation > 0f)
            {
                float pulse = Mathf.Sin(phase * Mathf.PI * 2f);
                alpha *= 1f - opacityVariation * (0.5f + 0.5f * pulse);
            }

            ApplyCopy(first, firstOrigin, phase, travel, alpha);
            ApplyCopy(second, secondOrigin, secondaryPhase, travel, alpha);
        }

        private static void ApplyCopy(Image image, Vector2 origin, float phase, float travel, float alpha)
        {
            image.rectTransform.anchoredPosition = origin + new Vector2((phase - 0.5f) * travel, 0f);

            // Each copy crosses the wrap fully transparent while the other is at peak opacity.
            float weight = Mathf.SmoothStep(0f, 1f, 1f - Mathf.Abs(2f * phase - 1f));
            Color color = image.color;
            color.a = alpha * weight;
            image.color = color;
        }
    }

    [SerializeField] private DriftLayer fogBack;
    [SerializeField] private DriftLayer fogFront;

    private RectTransform effectsRect;
    private double elapsed;

    private void Awake()
    {
        effectsRect = (RectTransform)transform;
        fogBack.CacheOrigins();
        fogFront.CacheOrigins();
    }

    private void OnEnable()
    {
        elapsed = 0;
        ApplyLayers();
    }

    private void LateUpdate()
    {
        elapsed += Time.unscaledDeltaTime;
        ApplyLayers();
    }

    private void ApplyLayers()
    {
        float width = effectsRect.rect.width;
        fogBack.Apply(elapsed, width);
        fogFront.Apply(elapsed, width);
    }
}
