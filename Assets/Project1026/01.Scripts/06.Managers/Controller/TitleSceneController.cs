using UnityEngine;

public class TitleSceneController : MonoBehaviour
{
    public void OnClickStart()
    {
        Managers.Scene.LoadGame("99.JIHO");
    }
}
