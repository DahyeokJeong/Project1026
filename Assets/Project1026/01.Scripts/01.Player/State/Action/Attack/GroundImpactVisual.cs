using UnityEngine;

public sealed class GroundImpactVisual
{
    private const int TextureSize = 128;
    private static Sprite ringSprite;
    private readonly SpriteRenderer renderer;

    public GroundImpactVisual(Transform parent, SpriteRenderer source)
    {
        GameObject ring = new GameObject("Ground Impact Ring");
        ring.transform.SetParent(parent, false);
        renderer = ring.AddComponent<SpriteRenderer>();
        renderer.sprite = GetRingSprite();
        renderer.sortingLayerID = source.sortingLayerID;
        renderer.sortingOrder = source.sortingOrder + 1;
        Hide();
    }

    public void Show(Vector2 radius, Color color)
    {
        Vector3 parentScale = renderer.transform.parent.lossyScale;
        renderer.transform.localScale = new Vector3(
            radius.x * 2f / Mathf.Abs(parentScale.x),
            radius.y * 2f / Mathf.Abs(parentScale.y), 1f);
        renderer.color = color;
        renderer.enabled = true;
    }

    public void Hide()
    {
        renderer.enabled = false;
    }

    private static Sprite GetRingSprite()
    {
        if (ringSprite != null)
            return ringSprite;

        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        Color32[] pixels = new Color32[TextureSize * TextureSize];

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                Vector2 point = new Vector2(x + 0.5f, y + 0.5f) * (2f / TextureSize) - Vector2.one;
                float distance = Mathf.Abs(point.magnitude - 0.95f);
                float edge = Mathf.InverseLerp(0.018f, 0.045f, distance);
                byte alpha = (byte)Mathf.RoundToInt(255f * (1f - Mathf.SmoothStep(0f, 1f, edge)));
                pixels[y * TextureSize + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        ringSprite = Sprite.Create(texture, new Rect(0f, 0f, TextureSize, TextureSize),
            new Vector2(0.5f, 0.5f), TextureSize);
        return ringSprite;
    }
}
