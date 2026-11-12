using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInfoPanelUI : MonoBehaviour
{
    #region Player Stat
    [SerializeField] private PlayerModel player;

    [Header("Current Stats")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text attackDamageText;
    [SerializeField] private TMP_Text attackSpeedText;
    [SerializeField] private TMP_Text defenseText;
    [SerializeField] private TMP_Text moveSpeedText;
    [SerializeField] private TMP_Text criticalText;
    [SerializeField] private TMP_Text healthRegenText;
    [SerializeField] private TMP_Text cooldownReductionText;
    #endregion

    #region Augment Bonus
    [Header("Acquired Augments")]
    [SerializeField] private Transform augmentContent;
    [SerializeField] private CharacterInfoRowUI augmentRowTemplate;

    private readonly List<CharacterInfoRowUI> spawnedAugmentRows = new();
    #endregion

    #region Auto Attack Acquire
    

    [Header("Acquired Auto Attacks")]
    [SerializeField] private Transform autoAttackContent;
    [SerializeField] private CharacterInfoRowUI autoAttackRowTemplate;
    [SerializeField] private AutoAttackDescriptionUI autoAttackDescription;
    [SerializeField] private AutoAttackDisplayEntrySO[] attackDisplayEntries;

    private readonly List<CharacterInfoRowUI> spawnedAttackRows = new();
    #endregion

    private void OnEnable()
    {
        RefreshStat();
        RefreshAugments();
        RefreshAutoAttacks();
    }

    private void RefreshStat()
    {
        if (player == null)
            return;

        levelText.text = player.Level.ToString();
        healthText.text = $"{Mathf.CeilToInt(player.CurrentHp)} / {player.MaxHp}";
        attackDamageText.text = player.AttackDamage.ToString();
        attackSpeedText.text = player.AttackSpeed.ToString("0.00");
        defenseText.text = player.Defense.ToString();
        moveSpeedText.text = player.MoveSpeed.ToString("0.00");
        criticalText.text = $"{player.Critical:0.#}%";
        healthRegenText.text = $"{player.HealthRegen} / √ ";

        float reductionPercent = (1f - player.AutoAttackCooldownMultiplier) * 100f;
        cooldownReductionText.text = $"{reductionPercent:0.#}%";
    }

    private void RefreshAugments()
    {
        foreach (CharacterInfoRowUI row in spawnedAugmentRows)
        {
            row.gameObject.SetActive(false);
            Destroy(row.gameObject);
        }
        spawnedAugmentRows.Clear();

        foreach (var entry in Managers.RunData.AcquiredAugmentCounts)
        {
            AugmentData data = AugmentHelper.Get(entry.Key);
            if (data == null)
                continue;

            Sprite icon = Managers.Data.TryGetAugmentEffect(data.ConfigKey, out var effect)
                ? effect.Icon
                : null;

            CharacterInfoRowUI row = Instantiate(augmentRowTemplate, augmentContent);
            row.BindAugment(
                LocalizationHelper.GetName(data.Id),
                LocalizationHelper.GetDescription(data.Id),
                icon,
                entry.Value);

            row.gameObject.SetActive(true);
            spawnedAugmentRows.Add(row);
        }
    }

    private void RefreshAutoAttacks()
    {
        if (autoAttackContent == null || autoAttackRowTemplate == null ||
            attackDisplayEntries == null)
            return;

        foreach (CharacterInfoRowUI row in spawnedAttackRows)
        {
            row.gameObject.SetActive(false);
            Destroy(row.gameObject);
        }
        spawnedAttackRows.Clear();

        foreach (string attackKey in Managers.RunData.AcquiredAttackKeys)
        {
            foreach (AutoAttackDisplayEntrySO entry in attackDisplayEntries)
            {
                if (entry == null || entry.config == null || entry.config.ConfigKey != attackKey)
                    continue;

                CharacterInfoRowUI row =
                    Instantiate(autoAttackRowTemplate, autoAttackContent);

                row.BindAutoAttack(entry.displayName, entry.config.Icon);

                Button button = row.GetComponent<Button>();
                button.onClick.AddListener(() => autoAttackDescription.Show(
                    entry.displayName,
                    entry.summary,
                    entry.behavior,
                    entry.baseStats));

                row.gameObject.SetActive(true);
                spawnedAttackRows.Add(row);
                break;
            }
        }
    }
}