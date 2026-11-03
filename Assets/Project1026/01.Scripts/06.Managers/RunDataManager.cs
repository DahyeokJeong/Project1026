using UnityEngine;

public class RunDataManager
{
    #region Player
    private PlayerModel model;
    private PlayerView view;
    #endregion

    #region Properties
    public float CurrentExp { get; set; }
    public int Level { get; set; } = 1;
    #endregion

    public void SetPlayer(PlayerModel model, PlayerView view)
    {
        this.model = model;
        this.view = view;

        view.InitExp(model.MaxExp);
        view.SetExp(CurrentExp, Level);
    }

    public void AddExp(float amount)
    {
        if (model == null)
            return;

        int levelUpCount = model.CalculateExp(this, amount);

        view?.SetExp(CurrentExp, Level);

        if (levelUpCount > 0)
            OnLevelUp(levelUpCount);
    }

    private void OnLevelUp(int levelUpCount)
    {
        // 추후 증강 시스템 연결
    }

    public void Reset()
    {
        CurrentExp = 0f;
        Level = 1;

        model = null;
        view = null;
    }
}