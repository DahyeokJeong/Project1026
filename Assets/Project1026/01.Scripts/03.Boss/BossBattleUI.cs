using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BossBattleUI : MonoBehaviour
{
    private GwaegangWhiteTiger boss;
    private GameObject root, healthPanel;
    private Image fill;
    private TMP_Text hp;
    private TMP_FontAsset font;

    public void Bind(GwaegangWhiteTiger target)
    {
        boss = target;
        var existing = FindAnyObjectByType<TMP_Text>();
        font = existing != null ? existing.font : TMP_Settings.defaultFontAsset;
        root = new GameObject("Boss battle HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        root.GetComponent<Canvas>().sortingOrder = 200;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        healthPanel = Panel("Boss health", root.transform, new Vector2(0.25f, 0.86f), new Vector2(0.75f, 0.96f), new Color(0.06f, 0.04f, 0.04f, 0.92f));
        Label("괴강백호", healthPanel.transform, new Vector2(0, 0.55f), Vector2.one, 28);
        var track = Panel("Health track", healthPanel.transform, new Vector2(0.04f, 0.18f), new Vector2(0.96f, 0.47f), new Color(0.16f, 0.1f, 0.1f));
        fill = Panel("Health fill", track.transform, Vector2.zero, Vector2.one, new Color(0.75f, 0.12f, 0.12f)).GetComponent<Image>();
        hp = Label("", track.transform, Vector2.zero, Vector2.one, 22);
    }

    private void LateUpdate()
    {
        if (boss == null || fill == null) return;
        var rect = fill.rectTransform;
        rect.anchorMax = new Vector2(Mathf.Clamp01(boss.CurrentHp / boss.MaxHp), 1f);
        hp.SetText("{0} / {1}", Mathf.CeilToInt(boss.CurrentHp), boss.MaxHp);
    }

    public void ShowReward(int amount, Action returnToLobby)
    {
        healthPanel.SetActive(false);
        var shade = Panel("Clear rewards", root.transform, Vector2.zero, Vector2.one, new Color(0, 0, 0, 0.8f));
        Label("괴강백호 처치\n스테이지 클리어", shade.transform, new Vector2(0.2f, 0.57f), new Vector2(0.8f, 0.8f), 48);
        Label($"영혼의 정수 +{amount}", shade.transform, new Vector2(0.2f, 0.43f), new Vector2(0.8f, 0.55f), 32);
        var button = Panel("Return to lobby", shade.transform, new Vector2(0.38f, 0.27f), new Vector2(0.62f, 0.36f), new Color(0.34f, 0.2f, 0.1f));
        var click = button.AddComponent<Button>();
        click.targetGraphic = button.GetComponent<Image>();
        click.onClick.AddListener(() => { click.interactable = false; returnToLobby(); });
        Label("마을로 돌아가기", button.transform, Vector2.zero, Vector2.one, 28);
    }

    private static GameObject Panel(string name, Transform parent, Vector2 min, Vector2 max, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>(), min, max);
        go.GetComponent<Image>().color = color;
        return go;
    }

    private TMP_Text Label(string text, Transform parent, Vector2 min, Vector2 max, int size)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>(), min, max);
        var label = go.GetComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.9f, 0.7f);
        label.raycastTarget = false;
        return label;
    }

    private static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
        if (root == null) return;
        if (Application.isPlaying) Destroy(root);
        else DestroyImmediate(root);
    }
}
