using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SaveNPCDialogue : MonoBehaviour
{
    [SerializeField] private RectTransform canvas;
    [SerializeField] private SaveNPCController saveController;
    [SerializeField] private TMP_FontAsset koreanFont;

    private GameObject dialogue;

    public bool IsOpen => dialogue != null && dialogue.activeInHierarchy;

    private void Awake()
    {
        if (canvas == null || saveController == null || koreanFont == null)
        {
            Debug.LogError("[SaveNPCDialogue] 캔버스, 저장 컨트롤러, 글꼴 연결을 확인하세요.", this);
            enabled = false;
            return;
        }

        BuildDialogue();
    }

    public void OpenDialogue()
    {
        if (isActiveAndEnabled && dialogue != null)
            dialogue.SetActive(true);
    }

    private void OnDisable()
    {
        if (dialogue != null)
            CloseDialogue();
    }

    private void BuildDialogue()
    {
        RectTransform shade = AddRegion(canvas, "Save NPC Dialogue", new Vector2(0, 0), new Vector2(1, 1));
        shade.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        dialogue = shade.gameObject;
        dialogue.SetActive(false);
        RectTransform panel = AddRegion(shade, "Dialogue Window", new Vector2(0.20f, 0.24f), new Vector2(0.80f, 0.76f));
        panel.gameObject.AddComponent<Image>().color = new Color(0.16f, 0.16f, 0.16f);
        AddLabel(panel, "저장 NPC", 0.72f, 0.94f, 28);
        AddLabel(panel, "안녕하세요. 게임 기록을 어떻게 할까요?", 0.46f, 0.72f, 21);

        AddAction(panel, "저장", new Vector2(0.06f, 0.26f), new Vector2(0.33f, 0.43f), saveController.OnClickSave);
        AddAction(panel, "불러오기", new Vector2(0.365f, 0.26f), new Vector2(0.635f, 0.43f), saveController.OnClickLoad);
        AddAction(panel, "삭제", new Vector2(0.67f, 0.26f), new Vector2(0.94f, 0.43f), saveController.OnClickDelete);
        AddAction(panel, "닫기", new Vector2(0.38f, 0.07f), new Vector2(0.62f, 0.22f), CloseDialogue);
    }

    private void CloseDialogue()
    {
        EventSystem events = EventSystem.current;
        if (events != null && events.currentSelectedGameObject != null &&
            events.currentSelectedGameObject.transform.IsChildOf(dialogue.transform))
        {
            events.SetSelectedGameObject(null);
        }

        dialogue.SetActive(false);
    }

    private void AddAction(Transform parent, string title, Vector2 min, Vector2 max, UnityAction action)
    {
        RectTransform rect = AddRegion(parent, title, min, max);
        Image background = rect.gameObject.AddComponent<Image>();
        background.color = new Color(0.32f, 0.32f, 0.32f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.onClick.AddListener(action);
        AddLabel(rect, title, 0f, 1f, 20);
    }

    private void AddLabel(Transform parent, string text, float bottom, float top, int fontSize)
    {
        RectTransform rect = AddRegion(parent, text, new Vector2(0.04f, bottom), new Vector2(0.96f, top));
        ConfigureLabel(rect.gameObject.AddComponent<TextMeshProUGUI>(), text, fontSize);
    }

    private void ConfigureLabel(TextMeshProUGUI label, string text, int fontSize)
    {
        label.font = koreanFont;
        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.enableAutoSizing = true;
        label.fontSizeMin = 10;
        label.fontSizeMax = fontSize;
        label.raycastTarget = false;
    }

    private static RectTransform AddRegion(Transform parent, string name, Vector2 min, Vector2 max)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        SetAnchors(rect, min, max);
        return rect;
    }

    private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
