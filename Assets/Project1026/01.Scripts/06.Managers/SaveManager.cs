using System;
using System.IO;
using UnityEngine;

public class SaveManager
{
    #region Properties
    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    public bool HasSaveData => File.Exists(SavePath);
    #endregion

    public bool SaveGame()
    {
        try
        {
            SaveData saveData = Managers.GameState.GetSaveData();

            string json = JsonUtility.ToJson(saveData, true);

            File.WriteAllText(SavePath, json);

            Debug.Log($"SaveManager : 저장 완료 / {SavePath}");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager : 저장 실패 / {e.Message}");
            return false;
        }
    }

    public bool LoadGame()
    {
        if (!HasSaveData)
        {
            Debug.LogWarning("SaveManager : 저장 파일이 없습니다.");
            return false;
        }

        try
        {
            string json = File.ReadAllText(SavePath);

            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData == null)
                throw new Exception("저장 데이터가 비어 있습니다.");

            Managers.GameState.LoadSaveData(saveData);

            Debug.Log("SaveManager : 불러오기 완료");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager : 불러오기 실패 / {e.Message}");
            return false;
        }
    }

    public bool DeleteSaveData()
    {
        try
        {
            if (!HasSaveData)
                return false;

            File.Delete(SavePath);

            Debug.Log("SaveManager : 저장 파일 삭제 완료");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager : 저장 파일 삭제 실패 / {e.Message}");
            return false;
        }
    }
}
