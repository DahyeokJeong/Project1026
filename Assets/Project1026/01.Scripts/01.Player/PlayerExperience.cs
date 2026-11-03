using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    #region Component
    [Header("Component")]
    [SerializeField] private PlayerModel model;
    [SerializeField] private PlayerView view;
    #endregion

    private void Start()
    {
        view.Init(model);
    }

    public void AddExp(float amount)
    {
        model.AddExp(amount);

        view.SetExp(model.CurrentExp);
    }
}
