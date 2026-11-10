using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LobbyInteractionPrompt
{
    private static readonly Color Wood = new(0.19f, 0.10f, 0.07f);
    private static readonly Color Lacquer = new(0.68f, 0.30f, 0.18f);
    private static readonly Color Ivory = new(0.98f, 0.89f, 0.72f);
    private readonly RectTransform canvas;
    private readonly Camera worldCamera;
    private readonly RectTransform badge;
    private readonly TextMeshProUGUI actionLabel;
    private Transform target;
    private Renderer targetRenderer;

    public LobbyInteractionPrompt(RectTransform canvas, Camera worldCamera, TMP_FontAsset font)
    {
        this.canvas = canvas;
        this.worldCamera = worldCamera;

        badge = AddImage(canvas, "F Interaction Prompt", Lacquer, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        badge.anchorMin = badge.anchorMax = canvas.pivot;
        badge.pivot = Vector2.zero;
        badge.sizeDelta = new Vector2(108, 32);

        RectTransform face = AddImage(badge, "Wood", Wood, Vector2.zero, Vector2.one,
            new Vector2(2, 2), new Vector2(-2, -2));
        RectTransform key = AddImage(face, "Keycap", Ivory, Vector2.zero, Vector2.zero,
            new Vector2(5, 4), new Vector2(27, 24));
        AddText(key, font, "F", Wood, 16, 0, 0);
        actionLabel = AddText(face, font, "", Ivory, 17, 32, 9);
        AddImage(face, "Seal", Lacquer, new Vector2(1, 0), new Vector2(1, 0),
            new Vector2(-8, 5), new Vector2(-4, 9));
        Hide();
    }

    public void SetTarget(Transform nextTarget, string action)
    {
        if (target == nextTarget)
            return;

        target = nextTarget;
        targetRenderer = target.GetComponent<Renderer>();
        actionLabel.text = action;
        badge.sizeDelta = new Vector2(Mathf.Max(84f, Mathf.Ceil(actionLabel.preferredWidth) + 48f), 32);
    }

    public void UpdatePosition()
    {
        if (target == null)
            return;

        Vector3 anchor = target.position;
        if (targetRenderer != null)
            anchor.y = targetRenderer.bounds.max.y;

        Vector3 screen = worldCamera.WorldToScreenPoint(anchor);
        if (screen.z <= 0f || !worldCamera.pixelRect.Contains((Vector2)screen) ||
            !RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screen, null, out Vector2 position))
        {
            badge.gameObject.SetActive(false);
            return;
        }

        Rect bounds = canvas.rect;
        float x = Mathf.Clamp(position.x + 7f, bounds.xMin, bounds.xMax - badge.rect.width);
        float y = Mathf.Clamp(position.y + 8f, bounds.yMin, bounds.yMax - badge.rect.height);
        badge.anchoredPosition = new Vector2(x, y);
        badge.gameObject.SetActive(true);
    }

    public void Hide()
    {
        target = null;
        badge.gameObject.SetActive(false);
    }

    private static RectTransform AddImage(Transform parent, string name, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rect = AddRegion(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private static TextMeshProUGUI AddText(Transform parent, TMP_FontAsset font, string text, Color color,
        int size, float left, float right)
    {
        RectTransform rect = AddRegion(parent, "Text", Vector2.zero, Vector2.one,
            new Vector2(left, 0), new Vector2(-right, 0));
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.fontSize = size;
        label.text = text;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform AddRegion(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        return rect;
    }
}
