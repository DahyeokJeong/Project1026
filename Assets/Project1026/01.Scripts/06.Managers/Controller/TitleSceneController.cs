using UnityEngine;

public class TitleSceneController : MonoBehaviour
{
    public void OnClickGameStart()
    {
        Managers.Scene.LoadGame("01.LobbyScene");
    }
}
