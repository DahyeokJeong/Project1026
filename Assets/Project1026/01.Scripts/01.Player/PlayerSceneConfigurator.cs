using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class PlayerSceneConfigurator : MonoBehaviour
{
    private const string LobbyScene = "01.LobbyScene";
    private const string StageSelectScene = "02.StageSelectScene";

    #region Component
    [Header("Battle Components")]
    [SerializeField] private PlayerAutoAttack autoAttack;
    [SerializeField] private PlayerAugment augment;

    [Header("Battle UI")]
    [SerializeField] private GameObject[] battleUIObjects;

    [Header("Lobby Interaction")]
    [SerializeField, Min(0f)] private float interactionRadius = 1.5f;
    [SerializeField] private RectTransform interactionCanvas;
    [SerializeField] private Camera interactionCamera;
    [SerializeField] private TMP_FontAsset interactionFont;

    private SaveNPCDialogue saveNpc;
    private Transform portal;
    private LobbyInteractionPrompt interactionPrompt;
    private SoulBookPanelUI soulBook;
    private bool leavingLobby;
    #endregion

    #region Properties
    public bool IsBattleScene =>
        SceneManager.GetActiveScene().name == "03.BattleScene";
    #endregion

    private void Awake()
    {
        Configure();
    }

    private void Start()
    {
        if (gameObject.scene.name != LobbyScene)
            return;

        saveNpc = FindAnyObjectByType<SaveNPCDialogue>();
        portal = GameObject.Find("Portal")?.transform;
        if (interactionCanvas != null && interactionCamera != null && interactionFont != null)
            interactionPrompt = new LobbyInteractionPrompt(interactionCanvas, interactionCamera, interactionFont);
    }

    private void Update()
    {
        if (leavingLobby || gameObject.scene.name != LobbyScene ||
            SceneManager.GetActiveScene() != gameObject.scene ||
            (saveNpc != null && saveNpc.IsOpen))
        {
            interactionPrompt?.Hide();
            return;
        }

        float rangeSquared = interactionRadius * interactionRadius;
        float npcDistance = saveNpc != null && saveNpc.isActiveAndEnabled && saveNpc.gameObject.scene == gameObject.scene
            ? SqrDistanceTo(saveNpc.transform) : float.PositiveInfinity;
        float portalDistance = portal != null && portal.gameObject.activeInHierarchy && portal.gameObject.scene == gameObject.scene
            ? SqrDistanceTo(portal) : float.PositiveInfinity;

        bool npcIsClosest = npcDistance <= portalDistance;
        if (Mathf.Min(npcDistance, portalDistance) > rangeSquared)
        {
            interactionPrompt?.Hide();
            return;
        }

        if (soulBook == null)
            soulBook = FindAnyObjectByType<SoulBookPanelUI>();

        if (soulBook != null && soulBook.IsOpen)
        {
            interactionPrompt?.Hide();
            return;
        }

        if (Keyboard.current?.fKey.wasPressedThisFrame != true)
        {
            Transform target = npcIsClosest ? saveNpc.transform : portal;
            interactionPrompt?.SetTarget(target, npcIsClosest ? "대화" : "스테이지 선택");
            return;
        }

        interactionPrompt?.Hide();
        if (npcIsClosest)
        {
            saveNpc.OpenDialogue();
        }
        else
        {
            leavingLobby = true;
            Managers.Scene.LoadGame(StageSelectScene);
        }
    }

    private void LateUpdate() => interactionPrompt?.UpdatePosition();

    private void OnDisable() => interactionPrompt?.Hide();

    private float SqrDistanceTo(Transform target)
    {
        return ((Vector2)(target.position - transform.position)).sqrMagnitude;
    }

    private void Configure()
    {
        bool isBattle = IsBattleScene;

        if (autoAttack != null)
            autoAttack.enabled = isBattle;

        if (augment != null)
            augment.enabled = isBattle;

        foreach (GameObject ui in battleUIObjects)
        {
            if (ui != null)
                ui.SetActive(isBattle);
        }
    }
}
