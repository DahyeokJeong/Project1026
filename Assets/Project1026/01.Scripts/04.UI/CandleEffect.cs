using UnityEngine;
using UnityEngine.UI;

public sealed class CandleEffect : MonoBehaviour
{
    #region References
    [SerializeField] private Image flame;
    [SerializeField] private Image glow;
    [SerializeField] private Sprite[] flameFrames;
    #endregion

    #region Settings
    [SerializeField, Min(1f)] private float framesPerSecond = 10f;
    [SerializeField, Min(0.01f)] private float flickerSpeed = 2.2f;
    [SerializeField, Range(0f, 8f)] private float swayDegrees = 2.5f;
    [SerializeField, Range(0f, 0.2f)] private float flameScaleVariation = 0.045f;
    [SerializeField, Range(0f, 0.3f)] private float flameBrightnessVariation = 0.09f;
    [SerializeField, Range(0f, 0.5f)] private float glowAlphaVariation = 0.24f;
    [SerializeField, Range(0f, 0.3f)] private float glowScaleVariation = 0.085f;
    [SerializeField, Min(0.01f)] private float glowResponse = 4f;
    #endregion

    private Color flameColor;
    private Color glowColor;
    private float elapsed;
    private float frameProgress;
    private float glowIntensity;
    private int frameIndex;

    private void Awake()
    {
        flameColor = flame.color;
        glowColor = glow.color;
    }

    private void OnEnable()
    {
        elapsed = 0f;
        frameProgress = 0f;
        frameIndex = 0;
        glowIntensity = Sample(13.7f, 0f);
        flame.sprite = flameFrames[0];
        ApplyVisuals(glowIntensity, Sample(37.2f, 0f));
    }

    private void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        elapsed += deltaTime;

        float sampleTime = elapsed * flickerSpeed;
        float intensity = Sample(13.7f, sampleTime);
        float sway = Sample(37.2f, sampleTime * 0.85f);

        frameProgress += deltaTime * framesPerSecond * (1f + 0.25f * sway);
        if (frameProgress >= 1f)
        {
            int steps = Mathf.FloorToInt(frameProgress);
            frameProgress -= steps;
            frameIndex = (frameIndex + steps) % flameFrames.Length;
            flame.sprite = flameFrames[frameIndex];
        }

        glowIntensity = Mathf.Lerp(glowIntensity, intensity, 1f - Mathf.Exp(-glowResponse * deltaTime));
        ApplyVisuals(intensity, sway);
    }

    private void ApplyVisuals(float intensity, float sway)
    {
        RectTransform flameRect = flame.rectTransform;
        flameRect.localRotation = Quaternion.Euler(0f, 0f, sway * swayDegrees);
        flameRect.localScale = new Vector3(
            1f + (0.65f * intensity + 0.35f * sway) * flameScaleVariation,
            1f + intensity * flameScaleVariation,
            1f);

        Color currentFlameColor = flameColor;
        currentFlameColor.a *= 1f - (1f - intensity) * 0.5f * flameBrightnessVariation;
        flame.color = currentFlameColor;

        Color currentGlowColor = glowColor;
        currentGlowColor.a *= 1f + glowIntensity * glowAlphaVariation;
        glow.color = currentGlowColor;

        float scale = 1f + glowIntensity * glowScaleVariation;
        glow.rectTransform.localScale = new Vector3(scale, scale, 1f);
    }

    private static float Sample(float channel, float time)
    {
        return Mathf.Clamp01(Mathf.PerlinNoise(channel, time)) * 2f - 1f;
    }
}
