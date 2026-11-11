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
    private static readonly Color Ink = new(0.12f, 0.06f, 0.03f);
    private static readonly Color Surface = new(0.94f, 0.83f, 0.65f);
    private static readonly Color ButtonColor = new(0.30f, 0.16f, 0.12f);
    private static readonly Color SelectedColor = new(0.86f, 0.59f, 0.20f);

    [SerializeField] private TMP_FontAsset koreanFont;
    [SerializeField] private Texture2D[] nodeIcons;

    private GameObject overlay;
    public bool IsOpen => overlay != null && overlay.activeInHierarchy;
    private TextMeshProUGUI balance;
    private TextMeshProUGUI selectedNode;
    private GameObject confirmation;
    private TextMeshProUGUI confirmationText;
    private TextMeshProUGUI confirmationTitle;
    private RawImage confirmationIcon;
    private GameObject confirmationFallback;
    private Button upgradeButton;
    private SoulBookNode? pendingNode;
    private readonly List<(SoulBookNode Id, string Title, Button Button, Image Face, Color FaceColor, RawImage Icon, GameObject Lock)> nodes = new();
    private readonly Dictionary<Texture2D, Texture2D> grayscaleIcons = new();
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
        foreach (Texture2D texture in grayscaleIcons.Values)
            Destroy(texture);
        if (circleSprite != null)
        {
            Destroy(circleSprite.texture);
            Destroy(circleSprite);
        }
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
            // Locked and unfinished nodes communicate their state before interaction.
            bool available = gameState.IsSoulBookNodeImplemented(node.Id) && gameState.IsSoulBookNodeUnlocked(node.Id);
            node.Button.interactable = available;
            node.Face.color = available ? node.FaceColor : new Color(.23f, .23f, .23f);
            node.Lock.SetActive(!available);
            if (node.Icon != null)
            {
                Texture2D original = nodeIcons[(int)node.Id];
                node.Icon.texture = available ? original : GrayscaleIcon(original);
                node.Icon.color = available ? Color.white : new Color(.55f, .55f, .55f);
            }
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
        RectTransform frame = AddRegion(shade, "Scroll border", .025f, .05f, .975f, .95f);
        frame.gameObject.AddComponent<Image>().color = new Color(.32f, .13f, .10f);
        RectTransform panel = AddRegion(frame, "Parchment", .012f, .02f, .988f, .98f);
        panel.gameObject.AddComponent<Image>().color = Surface;
        AddText(panel, "도술서 성장", .02f, .85f, .30f, .98f, 38, Ink);
        AddText(panel, "능력치 강화", .30f, .85f, .68f, .96f, 28, Ink);
        balance = AddText(panel, "", .70f, .89f, .94f, .98f, 20, Ink);
        AddButton(panel, "×", .95f, .90f, .985f, .97f, () => SetOpen(false));
        branches[0] = AddBranch(panel, "공격",
            (SoulBookNode.AttackDamage, "공격력"), (SoulBookNode.CriticalChance, "크리티컬 확률"),
            (SoulBookNode.AttackSpeed, "공격속도"), (SoulBookNode.AutoAttackCooldown, "자동 공격\n대기시간 감소"),
            (SoulBookNode.ActiveSkillCooldown, "액티브 스킬\n대기시간 감소"));
        branches[0].anchorMin = new Vector2(.02f, .17f);
        branches[0].anchorMax = new Vector2(.25f, .76f);
        RectTransform defense = AddRegion(panel, "Defense", .265f, .17f, .475f, .76f);
        AddNode(defense, (SoulBookNode.HealthRegen, "체력 재생량 증가"), .25f, .66f, .75f, .91f);
        AddText(defense, "최대 체력 · 방어력\n추후 개방", .05f, .32f, .95f, .55f, 21, Ink);
        branches[1] = defense;
        RectTransform utility = AddRegion(panel, "Utility", .49f, .17f, .715f, .76f);
        AddText(utility, "체력 재생 강화 후 개방", .02f, .91f, .98f, 1f, 15, Ink);
        AddConnections(utility, .5f, .82f, .57f);
        AddConnections(utility, .265f, .42f, .22f);
        AddNode(utility, (SoulBookNode.AugmentRerolls, "증강 기회 횟수"), .03f, .45f, .48f, .70f);
        AddNode(utility, (SoulBookNode.MoveSpeed, "이동속도"), .52f, .45f, .97f, .70f);
        AddNode(utility, (SoulBookNode.ExperienceGain, "경험치 획득량"), .03f, .06f, .48f, .31f);
        AddNode(utility, (SoulBookNode.SoulEssenceGain, "정수 획득량"), .52f, .06f, .97f, .31f);
        branches[2] = utility;
        Heading(panel, "공격", .04f, .23f, new Color(.52f, .12f, .12f));
        Heading(panel, "방어", .28f, .46f, new Color(.12f, .24f, .40f));
        Heading(panel, "유틸리티", .51f, .695f, new Color(.16f, .33f, .20f));
        Rule(panel, .735f, .16f, .738f, .85f);
        AddText(panel, "도술 (액티브)", .76f, .76f, .97f, .85f, 25, Ink);
        AddNode(panel, (SoulBookNode.Clone, "분신술"), .76f, .48f, .855f, .71f);
        AddNode(panel, (SoulBookNode.AreaAttack, "광역 도술"), .875f, .48f, .97f, .71f);
        AddText(panel, "추가 도술\n추후 개방", .77f, .24f, .96f, .42f, 23, Ink);
        selectedNode = AddText(panel, "노드 선택 · 강화 비용: 정수 10  |  K / ESC 닫기", .03f, .03f, .97f, .12f, 20, Ink);
        BuildConfirmation(shade);
    }

    private void BuildConfirmation(Transform parent)
    {
        RectTransform shade = AddRegion(parent, "Confirmation", 0f, 0f, 1f, 1f);
        shade.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
        RectTransform dialog = AddRegion(shade, "Dialog", .27f, .20f, .73f, .80f);
        dialog.gameObject.AddComponent<Image>().color = Surface;
        confirmationTitle = AddText(dialog, "", .06f, .82f, .94f, .96f, 28, Ink);
        RectTransform iconRegion = AddRegion(dialog, "Selected icon", .5f, .67f, .5f, .67f);
        iconRegion.sizeDelta = new Vector2(72, 72);
        confirmationIcon = iconRegion.gameObject.AddComponent<RawImage>();
        confirmationIcon.raycastTarget = false;
        confirmationFallback = AddText(iconRegion, "준비", 0, 0, 1, 1, 20, Ink).gameObject;
        confirmationText = AddText(dialog, "", .07f, .23f, .93f, .55f, 20, Ink);
        confirmationText.fontSizeMin = 16;
        upgradeButton = AddButton(dialog, "강화하기", .09f, .07f, .48f, .19f, ConfirmUpgrade);
        AddButton(dialog, "닫기 / 취소", .52f, .07f, .91f, .19f, CloseConfirmation);
        confirmation = shade.gameObject;
        confirmation.SetActive(false);
    }

    private void OpenConfirmation(SoulBookNode id, string title)
    {
        if (pendingNode.HasValue)
            return;

        int level = gameState.GetSoulBookLevel(id);
        pendingNode = id;
        confirmationTitle.text = title.Replace("\n", " ");
        int index = (int)id;
        Texture2D icon = nodeIcons != null && index < nodeIcons.Length ? nodeIcons[index] : null;
        confirmationIcon.texture = icon;
        confirmationIcon.color = icon != null ? Color.white : Color.clear;
        confirmationFallback.SetActive(icon == null);
        string status = !gameState.IsSoulBookNodeImplemented(id) ? "준비 중인 노드입니다." :
            !gameState.IsSoulBookNodeUnlocked(id) ? UnlockRequirement(id) :
            level >= GameStateManager.SoulBookMaxLevel ? "최대 단계까지 강화했습니다." :
            !gameState.CanUpgradeSoulBookNode(id) ? "영혼의 정수가 부족합니다." :
            $"{level} → {level + 1} 단계로 강화하시겠습니까?";
        confirmationText.text = $"{NodeDescription(id)}\n현재 단계  {level} / {GameStateManager.SoulBookMaxLevel}\n" +
            $"강화 비용  정수 {GameStateManager.SoulBookLevelCost}  ·  보유 {gameState.SoulEssence:N0}\n\n{status}";
        upgradeButton.interactable = gameState.CanUpgradeSoulBookNode(id);
        confirmation.SetActive(true);
    }

    private static string NodeDescription(SoulBookNode id) => id switch
    {
        SoulBookNode.AttackDamage => "단계당 기본 공격력의 5%만큼 공격력이 증가합니다.",
        SoulBookNode.CriticalChance => "단계당 크리티컬 확률이 2%p 증가합니다.",
        SoulBookNode.AttackSpeed => "단계당 공격속도가 5% 증가합니다.",
        SoulBookNode.AutoAttackCooldown => "단계당 자동 공격 대기시간이 3% 감소합니다.",
        SoulBookNode.HealthRegen => "단계당 체력 재생량이 1 증가합니다.",
        SoulBookNode.MoveSpeed => "단계당 이동속도가 5% 증가합니다.",
        SoulBookNode.ExperienceGain => "단계당 경험치 획득량이 5% 증가합니다.",
        SoulBookNode.SoulEssenceGain => "단계당 영혼의 정수 획득량이 5% 증가합니다.",
        SoulBookNode.AugmentRerolls => "단계당 증강 리롤 기회가 1회 증가합니다.",
        SoulBookNode.ActiveSkillCooldown => "액티브 스킬 대기시간을 줄이는 노드입니다.\n상세 효과는 준비 중입니다.",
        SoulBookNode.Clone => "분신을 소환하는 도술입니다. 상세 효과는 준비 중입니다.",
        SoulBookNode.AreaAttack => "주변에 광역 공격을 가하는 도술입니다.\n상세 효과는 준비 중입니다.",
        _ => "상세 효과는 준비 중입니다."
    };

    private static string UnlockRequirement(SoulBookNode id) => id switch
    {
        SoulBookNode.CriticalChance or SoulBookNode.AttackSpeed => "공격력을 1단계 강화하면 개방됩니다.",
        SoulBookNode.AutoAttackCooldown or SoulBookNode.ActiveSkillCooldown => "크리티컬 확률을 1단계 강화하면 개방됩니다.",
        SoulBookNode.AugmentRerolls or SoulBookNode.MoveSpeed => "체력 재생량을 1단계 강화하면 개방됩니다.",
        SoulBookNode.ExperienceGain or SoulBookNode.SoulEssenceGain => "증강 기회를 1단계 강화하면 개방됩니다.",
        _ => "선행 노드를 강화하면 개방됩니다."
    };

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
        RectTransform area = AddRegion(parent, node.Title, left, bottom, right, top);
        // Use canvas units, independent of each branch's anchor dimensions.
        RectTransform disc = AddRegion(area, "Node", .5f, .65f, .5f, .65f);
        disc.sizeDelta = new Vector2(64f, 64f);
        Image rim = disc.gameObject.AddComponent<Image>();
        rim.sprite = GetCircle();
        rim.color = SelectedColor;
        Image face = AddRegion(disc, "Face", .06f, .06f, .94f, .94f).gameObject.AddComponent<Image>();
        face.sprite = rim.sprite;
        face.color = node.Id <= SoulBookNode.ActiveSkillCooldown ? new Color(.5f,.12f,.12f) :
            node.Id == SoulBookNode.HealthRegen ? new Color(.12f,.24f,.4f) : new Color(.16f,.33f,.2f);
        face.raycastTarget = false;
        RawImage nodeIcon = null;
        int index = (int)node.Id;
        if (nodeIcons != null && index < nodeIcons.Length && nodeIcons[index] != null)
        {
            RawImage icon = AddRegion(disc, "Icon", .20f, .20f, .80f, .80f).gameObject.AddComponent<RawImage>();
            nodeIcon = icon;
            icon.texture = nodeIcons[index];
            icon.raycastTarget = false;
        }
        else AddText(disc, "준비", .15f,.15f,.85f,.85f,18,Color.white);
        Button button = disc.gameObject.AddComponent<Button>();
        button.targetGraphic = rim;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        button.onClick.AddListener(() => OpenConfirmation(node.Id, node.Title));
        ColorBlock colors = button.colors;
        colors.disabledColor = new Color(.42f, .42f, .42f, 1f);
        button.colors = colors;
        RectTransform lockPlate = AddRegion(disc, "Locked state", .05f, .01f, .95f, .35f);
        Image plate = lockPlate.gameObject.AddComponent<Image>();
        plate.color = new Color(.12f, .12f, .12f, .96f);
        plate.raycastTarget = false;
        AddText(lockPlate, "잠김", 0, 0, 1, 1, 14, Color.white);
        lockPlate.gameObject.SetActive(false);
        nodes.Add((node.Id, node.Title, button, face, face.color, nodeIcon, lockPlate.gameObject));
    }

    private Texture2D GrayscaleIcon(Texture2D source)
    {
        if (grayscaleIcons.TryGetValue(source, out Texture2D cached)) return cached;
        RenderTexture previous = RenderTexture.active;
        RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        Texture2D result = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        try
        {
            Graphics.Blit(source, temporary);
            RenderTexture.active = temporary;
            result.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            Color[] pixels = result.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float gray = pixels[i].grayscale;
                pixels[i] = new Color(gray, gray, gray, pixels[i].a);
            }
            result.SetPixels(pixels);
            result.Apply();
            grayscaleIcons.Add(source, result);
            return result;
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
        }
    }

    private Sprite circleSprite;
    private Sprite GetCircle()
    {
        if (circleSprite != null) return circleSprite;
        Texture2D texture = new Texture2D(128,128,TextureFormat.RGBA32,false);
        texture.wrapMode = TextureWrapMode.Clamp;
        for (int y=0;y<128;y++)
            for (int x=0;x<128;x++)
                texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(64-Vector2.Distance(new Vector2(x,y),new Vector2(63.5f,63.5f)))));
        texture.Apply();
        circleSprite = Sprite.Create(texture,new Rect(0,0,128,128),new Vector2(.5f,.5f));
        return circleSprite;
    }

    private void Heading(Transform parent, string title, float left, float right, Color color)
    {
        RectTransform strip = AddRegion(parent,title,.0f+left,.77f,right,.84f);
        strip.gameObject.AddComponent<Image>().color = color;
        AddText(strip,title,0,0,1,1,25,Color.white);
    }

    private static void Rule(Transform parent,float left,float bottom,float right,float top)
    {
        Image image = AddRegion(parent,"Rule",left,bottom,right,top).gameObject.AddComponent<Image>();
        image.color = new Color(.49f,.29f,.16f,.65f);
        image.raycastTarget = false;
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
        AddText(region, title, 0.05f, 0.05f, 0.95f, 0.95f, 20, Color.white);
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
