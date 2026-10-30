using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    [SerializeField] private PlayerAutoAttack autoAttack;
    [SerializeField] private PlayerAugment playerAugment;

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            Debug.Log(
                $"적용 전 Normal: " +
                $"{autoAttack.GetProjectileCount("SO_Attack_Normal")}");

            playerAugment.Apply("AUG001");

            Debug.Log(
                $"적용 후 Normal: " +
                $"{autoAttack.GetProjectileCount("SO_Attack_Normal")}");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            Debug.Log(
                $"적용 전 Piercing: " +
                $"{autoAttack.GetProjectileCount("SO_Attack_Piercing")}");

            playerAugment.Apply("AUG002");

            Debug.Log(
                $"적용 후 Piercing: " +
                $"{autoAttack.GetProjectileCount("SO_Attack_Piercing")}");
        }
    }
}