using UnityEngine;

public class SaveNPCController : MonoBehaviour
{
    public void OnClickSave()
    {
        Managers.Save.SaveGame();
    }

    public void OnClickLoad()
    {
        Managers.Save.LoadGame();
    }

    public void OnClickDelete()
    {
        if (!Managers.Save.DeleteSaveData())
            return;

        Managers.GameState.Clear();

        Debug.Log("SaveNPCController : 저장 데이터 및 현재 진행도 초기화");
    }


    //저장 확인용 임시코드
    public void OnClickTestClear()
    {
        Managers.GameState.ClearStage(1);
        Debug.Log("테스트 : 스테이지 1 클리어");
    }
}
