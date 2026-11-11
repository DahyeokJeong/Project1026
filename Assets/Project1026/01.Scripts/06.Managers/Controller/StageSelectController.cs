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

    private int selectedStage = 1;
    private bool isLeaving;

    private void Awake()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int id = i + 1;
            stageButtons[i].onClick.AddListener(() => OnClickStage(id));
        }
        enterButton.onClick.AddListener(EnterSelectedStage);
        backButton.onClick.AddListener(ReturnToLobby);
    }

    private void OnEnable()
    {
        Managers.GameState.ProgressChanged += RefreshButtons;
        RefreshButtons();
    }

    private void OnDisable()
    {
        Managers.GameState.ProgressChanged -= RefreshButtons;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ReturnToLobby();
    }

    private void RefreshButtons()
    {
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int id = i + 1;
            bool unlocked = Managers.GameState.IsStageUnlocked(id);
            stageButtons[i].interactable = unlocked && !isLeaving;
            selectionFrames[i].SetActive(id == selectedStage && unlocked);
            lockBadges[i].SetActive(!unlocked);
            stageStatus[i].text = !unlocked ? $"스테이지 {id - 1} 클리어 후 개방" :
                Managers.GameState.IsStageCleared(id) ? "클리어 완료" : "도전 가능";
        }
        selectedTitle.text = $"스테이지 {selectedStage}";
        string[] descriptions = { "황혼이 내려앉은 마을", "불길에 휩싸인 들판", "푸른 정기가 흐르는 숲" };
        stageDescription.text = descriptions[selectedStage - 1] + "\n준비가 되면 입장하기를 눌러주세요.";
        enterButton.interactable = !isLeaving && Managers.GameState.IsStageUnlocked(selectedStage);
        backButton.interactable = !isLeaving;
    }

    // Selecting a card only previews it. Entering is a separate explicit action.
    public void OnClickStage(int stageId)
    {
        if (isLeaving || !Managers.GameState.IsStageUnlocked(stageId) || stageId < 1 || stageId > stageButtons.Length)
            return;
        selectedStage = stageId;
        RefreshButtons();
    }

    public void EnterSelectedStage()
    {
        if (isLeaving || !Managers.GameState.SelectStage(selectedStage)) return;
        isLeaving = true;
        RefreshButtons();
        Managers.Scene.LoadGame("03.BattleScene");
    }

    public void ReturnToLobby()
    {
        if (isLeaving) return;
        isLeaving = true;
        RefreshButtons();
        Managers.Scene.LoadGame("01.LobbyScene");
    }
}
