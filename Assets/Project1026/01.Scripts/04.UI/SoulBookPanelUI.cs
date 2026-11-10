using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class SoulBookPanelUI : MonoBehaviour
{
    private const string LobbyScene = "01.LobbyScene";
    public const string PrefabAddress = "SoulBookPanel";
    private static readonly Color Ink = Color.white;
    private static readonly Color Surface = new(0.16f, 0.16f, 0.16f);
    private static readonly Color ButtonColor = new(0.30f, 0.30f, 0.30f);
    private static readonly Color SelectedColor = new(0.48f, 0.48f, 0.48f);

    [SerializeField] private TMP_FontAsset koreanFont;

    private GameObject overlay;
    public bool IsOpen => overlay != null && overlay.activeInHierarchy;
    private TextMeshProUGUI balance;
    private TextMeshProUGUI selectedNode;
    private GameObject confirmation;
    private TextMeshProUGUI confirmationText;
    private SoulBookNode? pendingNode;
    private readonly List<(SoulBookNode Id, string Title, Button Button, TextMeshProUGUI Label)> nodes = new();
    private readonly RectTransform[] branches = new RectTransform[3];
    private readonly Button[] branchTabs = new Button[3];
    private GameStateManager gameState;
    private AsyncOperationHandle<GameObject> prefabHandle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHook()
    {
        SceneManager.sceneLoaded -= InstallInLobby;
        SceneManager.sceneLoaded += InstallInLobby;
    }


    private static async void InstallInLobby(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != LobbyScene)
            return;

        AsyncOperationHandle<GameObject> handle =
            Addressables.LoadAssetAsync<GameObject>(PrefabAddress);

        try
        {
            GameObject prefab = await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded || prefab == null ||
                !prefab.TryGetComponent(out SoulBookPanelUI panelPrefab))
            {
                Debug.LogError($"[SoulBook] 도술서 UI 프리팹 로드 실패 ({PrefabAddress}): {handle.OperationException}");
                return;
            }

            if (!scene.IsValid() || !scene.isLoaded)
                return;

            SoulBookPanelUI panel = Instantiate(panelPrefab);
            panel.prefabHandle = handle;
            handle = default;
            SceneManager.MoveGameObjectToScene(panel.gameObject, scene);
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"[SoulBook] 프리팹 로드 실패: {exception.Message}");
        }
        finally
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
    }


    private void Update()
    {
        if (SceneManager.GetActiveScene() != gameObject.scene)
        {
            if (overlay != null && overlay.activeSelf)
                SetOpen(false);

            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.kKey.wasPressedThisFrame)
            SetOpen(overlay == null || !overlay.activeSelf);
        else if (keyboard.escapeKey.wasPressedThisFrame && overlay != null && overlay.activeSelf)
        {
            if (pendingNode.HasValue)
                CloseConfirmation();
            else
                SetOpen(false);
        }
    }

    private void OnDisable()
    {
        if (overlay != null && overlay.activeSelf)
            SetOpen(false);
    }

    private void OnDestroy()
    {
        if (prefabHandle.IsValid())
            Addressables.Release(prefabHandle);
    }

    private void SetOpen(bool open)
    {
        if (open)
        {
            if (overlay == null)
                BuildPanel();

            gameState = Managers.GameState;
            gameState.ProgressChanged += RefreshProgress;
            RefreshProgress();
        }
        else if (gameState != null)
        {
            CloseConfirmation();
            gameState.ProgressChanged -= RefreshProgress;
            gameState = null;
        }

        overlay.SetActive(open);
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    private void RefreshProgress()
    {
        balance.text = $"영혼의 정수   {gameState.SoulEssence:N0}";
        foreach (var node in nodes)
        {
            int level = gameState.GetSoulBookLevel(node.Id);
            string status = !gameState.IsSoulBookNodeImplemented(node.Id) ? "준비 중" :
                !gameState.IsSoulBookNodeUnlocked(node.Id) ? "잠김" :
                $"{level}/{GameStateManager.SoulBookMaxLevel}";
            node.Label.text = $"{node.Title}\n{status}";
            node.Button.interactable = gameState.IsSoulBookNodeImplemented(node.Id) &&
                gameState.IsSoulBookNodeUnlocked(node.Id) && level < GameStateManager.SoulBookMaxLevel;
        }
    }

    private void BuildPanel()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        RectTransform shade = AddRegion(transform, "Overlay", 0f, 0f, 1f, 1f);
        shade.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
        overlay = shade.gameObject;
        RectTransform panel = AddRegion(shade, "Soul Book", 0.07f, 0.10f, 0.93f, 0.90f);
        panel.gameObject.AddComponent<Image>().color = Surface;

        AddText(panel, "도술서", 0.035f, 0.84f, 0.25f, 0.96f, 32, Ink);
        balance = AddText(panel, "", 0.55f, 0.85f, 0.83f, 0.95f, 24, Ink);
        AddButton(panel, "닫기", 0.84f, 0.85f, 0.97f, 0.95f, () => SetOpen(false));

        branchTabs[0] = AddButton(panel, "공격", 0.035f, 0.73f, 0.215f, 0.82f, () => ShowBranch(0));
        branchTabs[1] = AddButton(panel, "방어", 0.23f, 0.73f, 0.41f, 0.82f, () => ShowBranch(1));
        branchTabs[2] = AddButton(panel, "유틸리티", 0.425f, 0.73f, 0.605f, 0.82f, () => ShowBranch(2));
        AddText(panel, "도술 (액티브)", 0.64f, 0.73f, 0.965f, 0.82f, 22, Ink);

        branches[0] = AddBranch(panel, "공격",
            (SoulBookNode.AttackDamage, "공격력 증가"), (SoulBookNode.CriticalChance, "치명타 확률 증가"),
            (SoulBookNode.AttackSpeed, "공격속도 증가"), (SoulBookNode.AutoAttackCooldown, "자동 공격 쿨다운 감소"),
            (SoulBookNode.ActiveSkillCooldown, "액티브 스킬 쿨다운 감소"));
        RectTransform defense = AddRegion(panel, "방어", 0.035f, 0.15f, 0.605f, 0.71f);
        AddText(defense, "기획 중", 0.05f, 0.38f, 0.95f, 0.62f, 22, Ink);
        branches[1] = defense;
        branches[2] = AddBranch(panel, "유틸리티",
            (SoulBookNode.HealthRegen, "체력 재생량 증가"), (SoulBookNode.AugmentRerolls, "증강 리롤 횟수 증가"),
            (SoulBookNode.MoveSpeed, "이동속도 증가"), (SoulBookNode.ExperienceGain, "경험치 획득량 증가"),
            (SoulBookNode.SoulEssenceGain, "영혼의 정수 획득량 증가"));

        AddNode(panel, (SoulBookNode.Clone, "분신술"), 0.64f, 0.51f, 0.965f, 0.63f);
        AddNode(panel, (SoulBookNode.AreaAttack, "단발성 광역 공격"), 0.64f, 0.35f, 0.965f, 0.47f);
        selectedNode = AddText(panel, "", 0.035f, 0.035f, 0.965f, 0.13f, 21, Ink);
        ShowBranch(0);
        BuildConfirmation(shade);
    }

    private void BuildConfirmation(Transform parent)
    {
        RectTransform shade = AddRegion(parent, "Confirmation", 0f, 0f, 1f, 1f);
        shade.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        RectTransform dialog = AddRegion(shade, "Dialog", 0.29f, 0.32f, 0.71f, 0.68f);
        dialog.gameObject.AddComponent<Image>().color = Surface;
        confirmationText = AddText(dialog, "", 0.05f, 0.38f, 0.95f, 0.94f, 24, Ink);
        AddButton(dialog, "예", 0.12f, 0.10f, 0.47f, 0.32f, ConfirmUpgrade);
        AddButton(dialog, "아니오", 0.53f, 0.10f, 0.88f, 0.32f, CloseConfirmation);
        confirmation = shade.gameObject;
        confirmation.SetActive(false);
    }

    private void OpenConfirmation(SoulBookNode id, string title)
    {
        if (pendingNode.HasValue)
            return;

        if (!gameState.CanUpgradeSoulBookNode(id))
        {
            selectedNode.text = "영혼의 정수가 부족합니다.";
            return;
        }

        int level = gameState.GetSoulBookLevel(id);
        pendingNode = id;
        confirmationText.text = $"{title}  {level} → {level + 1}\n영혼의 정수 {GameStateManager.SoulBookLevelCost}개를 사용하시겠습니까?";
        confirmation.SetActive(true);
    }

    private void ConfirmUpgrade()
    {
        if (!pendingNode.HasValue)
            return;

        SoulBookNode id = pendingNode.Value;
        CloseConfirmation();
        selectedNode.text = gameState.TryUpgradeSoulBookNode(id) ? "강화했습니다." : "강화할 수 없습니다.";
    }

    private void CloseConfirmation()
    {
        pendingNode = null;
        confirmation.SetActive(false);
    }

    private void ShowBranch(int index)
    {
        for (int i = 0; i < branches.Length; i++)
        {
            bool selected = i == index;
            branches[i].gameObject.SetActive(selected);
            branchTabs[i].image.color = selected ? SelectedColor : ButtonColor;
        }

        selectedNode.text = "노드를 선택하세요";
    }

    private RectTransform AddBranch(Transform panel, string title,
        (SoulBookNode, string) root, (SoulBookNode, string) secondLeft, (SoulBookNode, string) secondRight,
        (SoulBookNode, string) thirdLeft, (SoulBookNode, string) thirdRight)
    {
        RectTransform branch = AddRegion(panel, title, 0.035f, 0.15f, 0.605f, 0.71f);
        AddConnections(branch, 0.50f, 0.66f, 0.56f);
        AddConnections(branch, 0.265f, 0.38f, 0.28f);
        AddNode(branch, root, 0.26f, 0.66f, 0.74f, 0.84f);
        AddNode(branch, secondLeft, 0.05f, 0.38f, 0.48f, 0.56f);
        AddNode(branch, secondRight, 0.52f, 0.38f, 0.95f, 0.56f);
        AddNode(branch, thirdLeft, 0.05f, 0.10f, 0.48f, 0.28f);
        AddNode(branch, thirdRight, 0.52f, 0.10f, 0.95f, 0.28f);
        return branch;
    }

    private static void AddConnections(Transform branch, float source, float top, float bottom)
    {
        const float left = 0.265f;
        const float right = 0.735f;
        const float width = 0.004f;
        float middle = (top + bottom) / 2f;
        AddLine(source - width, middle, source + width, top);
        AddLine(left, middle - width, right, middle + width);
        AddLine(left - width, bottom, left + width, middle);
        AddLine(right - width, bottom, right + width, middle);

        void AddLine(float xMin, float yMin, float xMax, float yMax)
        {
            Image line = AddRegion(branch, "Connection", xMin, yMin, xMax, yMax).gameObject.AddComponent<Image>();
            line.color = SelectedColor;
            line.raycastTarget = false;
        }
    }

    private void AddNode(Transform parent, (SoulBookNode Id, string Title) node, float left, float bottom,
        float right, float top)
    {
        Button button = AddButton(parent, node.Title, left, bottom, right, top,
            () => OpenConfirmation(node.Id, node.Title));
        nodes.Add((node.Id, node.Title, button, button.GetComponentInChildren<TextMeshProUGUI>()));
    }

    private Button AddButton(Transform parent, string title, float left, float bottom,
        float right, float top, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform region = AddRegion(parent, title, left, bottom, right, top);
        Image background = region.gameObject.AddComponent<Image>();
        background.color = ButtonColor;
        Button button = region.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.onClick.AddListener(onClick);
        AddText(region, title, 0.05f, 0.05f, 0.95f, 0.95f, 20, Ink);
        return button;
    }

    private TextMeshProUGUI AddText(Transform parent, string value, float left, float bottom,
        float right, float top, int fontSize, Color color)
    {
        RectTransform region = AddRegion(parent, value, left, bottom, right, top);
        TextMeshProUGUI label = region.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = koreanFont;
        label.text = value;
        label.alignment = TextAlignmentOptions.Center;
        label.color = color;
        label.enableAutoSizing = true;
        label.fontSizeMin = 10;
        label.fontSizeMax = fontSize;
        label.raycastTarget = false;
        return label;
    }

    private static RectTransform AddRegion(Transform parent, string name, float left, float bottom,
        float right, float top)
    {
        RectTransform region = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        region.SetParent(parent, false);
        region.anchorMin = new Vector2(left, bottom);
        region.anchorMax = new Vector2(right, top);
        region.offsetMin = Vector2.zero;
        region.offsetMax = Vector2.zero;
        return region;
    }
}
