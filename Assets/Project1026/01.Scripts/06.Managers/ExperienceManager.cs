using UnityEngine;

public class ExperienceManager
{
    private PlayerModel model;

    public void SetPlayer(PlayerModel model)
    {
        this.model = model;
    }

    public void AddExp(float amount)
    {
        model?.AddExp(amount);
    }
}
