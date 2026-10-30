using UnityEngine;

public class PlayerAugment : MonoBehaviour
{
    [SerializeField] private PlayerAutoAttack auto;

    public void Apply(string augmentId)
    {
        AugmentData data = AugmentHelper.Get(augmentId);
        if (data == null)
            return;
        Apply(data);
    }

    private void Apply(AugmentData data)
    {
        switch(data.ConfigKey)
        {
            case "SO_Augment_NormalCount":
            case "SO_Augment_PiercingCount":
                auto.AddProjectileCount(data.AttackConfigKey, data.Value);
                break;
            default:
                Debug.LogWarning(
                $"[PlayerAugment] 처리되지 않은 ConfigKey: {data.ConfigKey}");
                break;
        }
    }
}
