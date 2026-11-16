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

    private int selectedStage = 1;
    private bool isLeaving;

    private void Awake()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int id = i + 1;
            stageButtons[i].onClick.AddListener(() => OnClickStage(id));
        }

        if (carousel != null)
        {
            var cards = new RectTransform[stageButtons.Length];
            for (int i = 0; i < stageButtons.Length; i++)
                cards[i] = (RectTransform)stageButtons[i].transform.parent;

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
        if (isLeaving || stageId < 1 || stageId > stageButtons.Length ||
            !Managers.GameState.IsStageUnlocked(stageId))
            return;

        if (carousel != null)
        {
            carousel.Focus(stageId - 1);
            return;
        }

        selectedStage = stageId;
        RefreshButtons();
    }

    private void OnCenteredCardChanged(int? cardIndex)
    {
        if (cardIndex.HasValue)
            selectedStage = cardIndex.Value + 1;

        RefreshButtons();
    }

    private bool CanEnterSelectedStage()
    {
        return !isLeaving &&
            selectedStage >= 1 &&
            selectedStage <= stageButtons.Length &&
            (carousel == null || carousel.CenteredCardIndex == selectedStage - 1) &&
            Managers.GameState.IsStageUnlocked(selectedStage);
    }

    private void RefreshButtons()
    {
        bool hasCenteredSelection = carousel == null ||
            carousel.CenteredCardIndex == selectedStage - 1;

        for (int i = 0; i < stageButtons.Length; i++)
        {
            int id = i + 1;
            bool unlocked = Managers.GameState.IsStageUnlocked(id);

            stageButtons[i].interactable = unlocked && !isLeaving;
            selectionFrames[i].SetActive(hasCenteredSelection && id == selectedStage && unlocked);
            lockBadges[i].SetActive(!unlocked);
            stageStatus[i].text = !unlocked ? $"스테이지 {id - 1} 클리어 후 개방" :
                Managers.GameState.IsStageCleared(id) ? "클리어 완료" : "도전 가능";
        }

        if (selectedStage >= 1 && selectedStage <= stageButtons.Length)
        {
            selectedTitle.text = $"스테이지 {selectedStage}";
            string description = selectedStage <= StageDescriptions.Length
                ? StageDescriptions[selectedStage - 1]
                : selectedTitle.text;

            stageDescription.text = description + (Managers.GameState.IsStageUnlocked(selectedStage)
                ? "\n준비가 되면 입장하기를 눌러주세요."
                : $"\n스테이지 {selectedStage - 1} 클리어 후 개방됩니다.");
        }

        enterButton.interactable = CanEnterSelectedStage();
        backButton.interactable = !isLeaving;
    }

    public void EnterSelectedStage()
    {
        if (!CanEnterSelectedStage() || !Managers.GameState.SelectStage(selectedStage))
            return;

        isLeaving = true;
        RefreshButtons();
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