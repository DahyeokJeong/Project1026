using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StageSelectController : MonoBehaviour
{
    [SerializeField] private Button[] stageButtons;
    [SerializeField] private GameObject[] selectionFrames;
    [SerializeField] private GameObject[] lockBadges;
    [SerializeField] private TextMeshProUGUI[] stageStatus;
    [SerializeField] private TextMeshProUGUI selectedTitle;
    [SerializeField] private TextMeshProUGUI stageDescription;
    [SerializeField] private Button enterButton;
    [SerializeField] private Button backButton;
    [SerializeField] private StageCardCarousel carousel;

    private static readonly string[] StageDescriptions =
    {
        "황혼이 내려앉은 마을",
        "불길에 휩싸인 들판",
        "푸른 정기가 흐르는 숲"
    };

    [SerializeField] private UnityEngine.UI.Button endlessButton;
    [SerializeField] private GameObject endlessSelectionFrame;
    [SerializeField] private GameObject endlessLockBadge;
    [SerializeField] private TextMeshProUGUI endlessStatus;
    [SerializeField] private TextMeshProUGUI endlessRecord;

    private const string EndlessName = "무한 모드";
    private const string EndlessUnlockHint = "스테이지 3 클리어 후 개방";

    private int selectedCardIndex;
    private bool IsEndlessCardSelected => selectedCardIndex == stageButtons.Length;
    private int SelectedStageId => selectedCardIndex + 1;
    private bool isLeaving;
    private ColorBlock endlessUnlockedColors;
    private ColorBlock endlessLockedColors;
    private void Awake()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int id = i + 1;
            stageButtons[i].onClick.AddListener(() => OnClickStage(id));
        }

        endlessUnlockedColors = endlessButton.colors;
        endlessLockedColors = endlessUnlockedColors;
        // Apply the disabled tint in every selectable state so the locked card still accepts clicks.
        Color lockedTint = endlessUnlockedColors.disabledColor;
        endlessLockedColors.normalColor = lockedTint;
        endlessLockedColors.highlightedColor = lockedTint;
        endlessLockedColors.pressedColor = lockedTint;
        endlessLockedColors.selectedColor = lockedTint;
        endlessButton.onClick.AddListener(() => FocusCard(stageButtons.Length));

        if (carousel != null)
        {
            var cards = new RectTransform[stageButtons.Length + 1];
            for (int i = 0; i < stageButtons.Length; i++)
                cards[i] = (RectTransform)stageButtons[i].transform.parent;

            cards[stageButtons.Length] = (RectTransform)endlessButton.transform.parent;
            carousel.SetCards(cards);
        }

        enterButton.onClick.AddListener(EnterSelectedStage);
        backButton.onClick.AddListener(ReturnToLobby);
    }

    private void OnEnable()
    {
        if (carousel != null)
        {
            carousel.CenteredCardChanged += OnCenteredCardChanged;
            OnCenteredCardChanged(carousel.CenteredCardIndex);
        }
        else
        {
            RefreshButtons();
        }

        Managers.GameState.ProgressChanged += RefreshButtons;
    }

    private void OnDisable()
    {
        if (carousel != null)
            carousel.CenteredCardChanged -= OnCenteredCardChanged;

        Managers.GameState.ProgressChanged -= RefreshButtons;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ReturnToLobby();
    }

    public void OnClickStage(int stageId)
    {
        if (stageId >= 1 && stageId <= stageButtons.Length && Managers.GameState.IsStageUnlocked(stageId))
            FocusCard(stageId - 1);
    }

    private void FocusCard(int cardIndex)
    {
        if (isLeaving)
            return;

        if (carousel != null)
        {
            carousel.Focus(cardIndex);
            return;
        }

        selectedCardIndex = cardIndex;
        RefreshButtons();
    }

    private void OnCenteredCardChanged(int? cardIndex)
    {
        if (cardIndex.HasValue)
            selectedCardIndex = cardIndex.Value;

        RefreshButtons();
    }

    private bool HasCenteredSelection => carousel == null || carousel.CenteredCardIndex == selectedCardIndex;

    private bool CanEnterSelectedStage()
    {
        return !isLeaving && HasCenteredSelection &&
            (IsEndlessCardSelected ? Managers.GameState.IsEndlessUnlocked :
                Managers.GameState.IsStageUnlocked(SelectedStageId));
    }

    private void RefreshButtons()
    {
        GameStateManager state = Managers.GameState;
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int id = i + 1;
            bool unlocked = state.IsStageUnlocked(id);

            stageButtons[i].interactable = unlocked && !isLeaving;
            selectionFrames[i].SetActive(HasCenteredSelection && i == selectedCardIndex && unlocked);
            lockBadges[i].SetActive(!unlocked);
            stageStatus[i].text = !unlocked ? $"스테이지 {id - 1} 클리어 후 개방" :
                state.IsStageCleared(id) ? "클리어 완료" : "도전 가능";
        }

        bool endlessUnlocked = state.IsEndlessUnlocked;
        endlessButton.interactable = !isLeaving;
        endlessButton.colors = endlessUnlocked ? endlessUnlockedColors : endlessLockedColors;
        endlessSelectionFrame.SetActive(HasCenteredSelection && IsEndlessCardSelected && endlessUnlocked);
        endlessLockBadge.SetActive(!endlessUnlocked);
        endlessStatus.gameObject.SetActive(!endlessUnlocked);
        endlessStatus.text = EndlessUnlockHint;
        endlessRecord.gameObject.SetActive(endlessUnlocked);
        int seconds = Mathf.FloorToInt(state.EndlessBestSurvivalTime);
        endlessRecord.text = state.EndlessHighestWave > 0 || state.EndlessBestSurvivalTime > 0f
            ? $"최고 {seconds / 60:00}:{seconds % 60:00} · {state.EndlessHighestWave}파"
            : "최고 기록 없음";

        if (IsEndlessCardSelected)
        {
            selectedTitle.text = EndlessName;
            stageDescription.text = "끝없이 몰려오는 요괴에게 도전하고 생존 기록을 세우세요." +
                (endlessUnlocked ? "" : $"\n{EndlessUnlockHint}");
        }
        else
        {
            int id = SelectedStageId;
            selectedTitle.text = $"스테이지 {id}";
            string description = id <= StageDescriptions.Length ? StageDescriptions[id - 1] : selectedTitle.text;
            stageDescription.text = state.IsStageUnlocked(id)
                ? description
                : $"{description}\n스테이지 {id - 1} 클리어 후 개방";
        }

        enterButton.interactable = CanEnterSelectedStage();
        backButton.interactable = !isLeaving;
    }

    public void EnterSelectedStage()
    {
        if (!CanEnterSelectedStage())
            return;

        GameStateManager state = Managers.GameState;
        if (!(IsEndlessCardSelected ? state.SelectEndless() : state.SelectStage(SelectedStageId)))
            return;

        isLeaving = true;
        RefreshButtons();
        Managers.RunData.Reset();
        Managers.Scene.LoadGame("03.BattleScene");
    }

    public void ReturnToLobby()
    {
        if (isLeaving)
            return;

        isLeaving = true;
        RefreshButtons();
        Managers.Scene.LoadGame("01.LobbyScene");
    }
}
