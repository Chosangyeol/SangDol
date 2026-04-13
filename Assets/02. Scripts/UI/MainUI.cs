using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    [Header("스킬 슬룻")]
    public List<SkillSlot> skillSlots;
    public SkillSlot spaceSlot;

    [Header("소모품 슬롯")]
    public List<UseItemSlot> useItemSlots;
    public ItemTooltip tooltip;

    private C_SkillSystem _skillSystem;
    private C_Inventory _inventory;
    private CharacterModel _model;

    [Header("Player UI")]
    [SerializeField] private Slider slPlayerHp;
    [SerializeField] private TMP_Text tmpPlayerHp;
    [SerializeField] private TMP_Text tmpPlayerLevel;
    [SerializeField] private Slider slPlayerExp;
    [SerializeField] private Image idenImage;

    [Header("Boss UI")]
    public bool isBossFight = false;
    public GameObject bossUI;
    public TMP_Text bossNameText;
    public Slider bossHpSlider;
    public Image bossHpBack;
    public TMP_Text bossHpText;
    public TMP_Text bossHpGradeText;
    public Slider bossKnockDownSlider;
    public Image bossKnockDownBack;



    public void Init(C_SkillSystem skillSystem,C_Inventory inventory, CharacterModel model)
    {
        _skillSystem = skillSystem;
        _inventory = inventory;
        _model = model;

        BindEvents();

        foreach (var slot in skillSlots)
        {
            slot.Init(skillSystem);
        }

        spaceSlot.Init(skillSystem);

        foreach (var slot in useItemSlots)
        {
            slot.Init(inventory, tooltip);
        }

        RefreshAll();

        bossUI.SetActive(false);
    }

    private void BindEvents()
    {
        _skillSystem.OnSkillDataChanged += RefreshAll;
        _inventory.OnInventoryUpdated += RefreshAll;
        GameEvent.OnStatChange += UpdatePlayerUI;
        GameEvent.OnBossStateChange += UpdateBossUI;
    }

    private void RefreshAll()
    {
        foreach (var slot in skillSlots)
            slot.Refresh();

        foreach (var slot in useItemSlots)
            slot.Refresh();
    }

    public void UpdatePlayerUI(CharacterStat stat)
    {
        slPlayerHp.value = stat.curHp / stat.maxHp.GetValue();

        if (stat.currentExp != 0)
            slPlayerExp.value = stat.maxExp / stat.currentExp;
        else slPlayerExp.value = 0;

        tmpPlayerLevel.text = $"Lv.{stat.currentLevel}";
        tmpPlayerHp.text = $"{stat.curHp} | {stat.maxHp.GetValue()}";

        idenImage.fillAmount = stat.idenCurrent / stat.idenMax;
    }

    public void UpdateBossUI(BossModel boss)
    {
        if (boss == null) return;

        if (!isBossFight)
        {
            bossUI.SetActive(true);
        }

        bossNameText.text = boss.statSO.enemyName;
        bossHpSlider.value = boss.Stat.curHp / boss.Stat.maxHp;
        bossHpText.text = $"{boss.Stat.curHp} | {boss.Stat.maxHp}";
        bossHpGradeText.text = $"{(boss.Stat.curHp / boss.Stat.maxHp) * 100:0}%";
        bossKnockDownSlider.value = boss.Stat.curDown / boss.Stat.maxDown;

        if (boss.isImmunity)
            bossHpBack.color = Color.white;
        else
            bossHpBack.color = Color.red;

        if (boss.isKnockDown)
            bossKnockDownBack.color = Color.gray;
        else
            bossKnockDownBack.color = Color.blue;
    }

    public void Toggle(bool onlyFalse = false)
    {
        if (onlyFalse)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(!gameObject.activeSelf);
    }

}
