using UnityEngine;

public class PlayerAttackTester : MonoBehaviour
{
    #region References
    [Header("Attack")]
    [SerializeField] private PlayerAutoAttack auto;
    [SerializeField] private AttackConfigBaseSO[] inspectAttacks;
    [SerializeField] private AttackConfigBaseSO targetAttack;

    [Header("Augment")]
    [SerializeField] private PlayerAugment playerAugment;

    [Header("Choice Count")]
    [SerializeField, Min(1)] private int choiceCount = 3;
    #endregion

    #region Test Settings
    [SerializeField, Min(1)] private int projectileIncrease = 1;
    [SerializeField] private string augmentId;
    #endregion

    private void Start()
    {
        PrintState();
    }

    [ContextMenu("Attack Test/1. 상태 출력")]
    private void PrintState()
    {
        if (!CanRun())
            return;

        if (inspectAttacks == null || inspectAttacks.Length == 0)
        {
            Debug.LogWarning(
                "[AttackTest] 확인할 공격 SO를 연결하세요.",
                this);
            return;
        }

        foreach (AttackConfigBaseSO config in inspectAttacks)
        {
            if (config == null)
                continue;

            string key = config.ConfigKey;

            if (!auto.TryGetAttackData(
                key,
                out AttackRuntimeData data))
            {
                Debug.LogError(
                    $"[AttackTest] 등록되지 않은 공격: {key}",
                    this);
                continue;
            }

            string count = data is ProjectileAttackRuntimeData projectile
                ? projectile.ProjectileCount.ToString()
                : "해당 없음";

            Debug.Log(
                $"[AttackTest] 씬: {gameObject.scene.name}"
                + $" / 플레이어: {auto.GetEntityId()}"
                + $" / 공격: {key}"
                + $" / 보유: {data.IsOwned}"
                + $" / 투사체 수: {count}",
                this);
        }
    }

    [ContextMenu("Attack Test/2. 대상 공격 획득")]
    private void AcquireTarget()
    {
        if (!TryGetTargetKey(out string key))
            return;

        bool success = auto.TryAcquireAttack(key);

        Debug.Log(
            $"[AttackTest] 획득 요청: {key}"
            + $" / 성공: {success}",
            this);

        PrintState();
    }

    [ContextMenu("Attack Test/3. 대상 투사체 수 증가")]
    private void IncreaseProjectileCount()
    {
        if (!TryGetTargetKey(out string key))
            return;

        bool success =
            auto.AddProjectileCount(key, projectileIncrease);

        Debug.Log(
            $"[AttackTest] 투사체 증가 요청: {key}"
            + $" / 증가량: {projectileIncrease}"
            + $" / 성공: {success}",
            this);

        PrintState();
    }

    [ContextMenu("Augment Test/선택 ID 적용")]
    private void ApplyAugment()
    {
        if (!CanRun())
            return;

        if (!Managers.Data.IsLoaded)
        {
            Debug.LogWarning(
                "[AugmentTest] 데이터 로딩이 완료되지 않았습니다.",
                this);
            return;
        }

        if (playerAugment == null
            || string.IsNullOrWhiteSpace(augmentId))
        {
            Debug.LogError(
                "[AugmentTest] PlayerAugment와 증강 ID를 설정하세요.",
                this);
            return;
        }

        bool canApply = playerAugment.CanApply(augmentId);
        bool success = playerAugment.TryApply(augmentId);

        Debug.Log(
            $"[AugmentTest] ID: {augmentId}"
            + $" / 적용 전 선택 가능: {canApply}"
            + $" / 적용 성공: {success}",
            this);

        PrintState();
    }

    private bool CanRun()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "[AttackTest] 플레이 모드에서 실행하세요.",
                this);
            return false;
        }

        if (auto == null)
        {
            Debug.LogError(
                "[AttackTest] PlayerAutoAttack을 연결하세요.",
                this);
            return false;
        }

        return true;
    }

    private bool TryGetTargetKey(out string key)
    {
        key = null;

        if (!CanRun())
            return false;

        if (targetAttack == null)
        {
            Debug.LogError(
                "[AttackTest] 대상 공격 SO를 연결하세요.",
                this);
            return false;
        }

        key = targetAttack.ConfigKey;

        if (!auto.HasAttack(key))
        {
            Debug.LogError(
                $"[AttackTest] 대상 공격이 등록되지 않았습니다: {key}",
                this);
            return false;
        }

        return true;
    }

    [ContextMenu("Augment Test/후보 카드 출력")]
    private void PrintChoices()
    {
        if (!CanRun())
            return;

        if (playerAugment == null)
        {
            Debug.LogError(
                "[AugmentTest] PlayerAugment를 연결하세요.",
                this);
            return;
        }

        if (!playerAugment.TryGetChoice(choiceCount, out var choices))
        {
            Debug.LogWarning(
                "[AugmentTest] 데이터·효과 연결 또는 후보 개수를 확인하세요.",
                this);
            return;
        }

        for (int i = 0; i < choices.Count; i++)
        {
            AugmentData data = choices[i];

            Debug.Log(
                $"[AugmentTest] 카드 {i + 1}"
                + $" / ID: {data.Id}"
                + $" / 이름: {LocalizationHelper.GetName(data.Id)}"
                + $" / 설명: {LocalizationHelper.GetDescription(data.Id)}",
                this);
        }
    }
}