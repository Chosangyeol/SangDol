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
    public SkillToolTip skillToolTip;

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
    public Image panicUI;

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

    [Header("알람 텍스트")]
    public TMP_Text bossRoomEnterCount;

    [Header("상호작용 / 게이지 UI")]
    public GameObject gaugeObject;
    public Slider gaugeFill;
    public TMP_Text gaugeTitle;
    public RectTransform perfectZoneRect;
    public Image perfectZoneImage;
    public GameObject imagePopUpUI;
    public Image popUpImage;

    public void Init(C_SkillSystem skillSystem,C_Inventory inventory, CharacterModel model)
    {
        _skillSystem = skillSystem;
        _inventory = inventory;
        _model = model;

        BindEvents();

        foreach (var slot in skillSlots)
        {
            slot.Init(skillSystem, skillToolTip);
        }

        spaceSlot.Init(skillSystem, skillToolTip);

        foreach (var slot in useItemSlots)
        {
            slot.Init(inventory, tooltip);
        }

        RefreshAll();
        SetPopUpImage(false);

        bossUI.SetActive(false);
        gaugeObject.SetActive(false);
        bossRoomEnterCount.gameObject.SetActive(false);
    }

    private void BindEvents()
    {
        _skillSystem.OnSkillDataChanged += RefreshAll;
        _inventory.OnInventoryUpdated += RefreshAll;
        GameEvent.OnStatChange += UpdatePlayerUI;
        GameEvent.OnBossStateChange += UpdateBossUI;
        GameEvent.OnBossRoomEnterCount += BossCountdownAlarm;
        GameEvent.OnPlayerPanic += TogglePanicUI;
        GameEvent.OnGaugeUpdate += SetGaugeUI;
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
            slPlayerExp.value =  stat.currentExp / stat.maxExp;
        else slPlayerExp.value = 0;

        tmpPlayerLevel.text = $"Lv.{stat.currentLevel}";
        tmpPlayerHp.text = $"{stat.curHp} | {stat.maxHp.GetValue()}";

        idenImage.fillAmount = stat.idenCurrent / stat.idenMax;
    }

    public void UpdateBossUI(BossModel boss)
    {
        if (boss == null)
        {
            bossUI.SetActive(false);
            return;
        }

        bossUI.SetActive(true);

        bossNameText.text = boss.statSO.enemyName;
        bossHpSlider.value = (float)boss.Stat.curHp / boss.Stat.maxHp;
        bossHpText.text = $"{boss.Stat.curHp} | {boss.Stat.maxHp}";
        bossHpGradeText.text = $"{((float)boss.Stat.curHp / boss.Stat.maxHp) * 100:0}%";
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

    public void TogglePanicUI(bool isPanic)
    {
        panicUI.gameObject.SetActive(isPanic);
    }

    public void BossCountdownAlarm(bool isAlarm,float time)
    {
        if (isAlarm)
        {
            bossRoomEnterCount.gameObject.SetActive(true);
            bossRoomEnterCount.text =
                $"보스룸 입장 중 입니다. {time:F0}초 뒤에 입장합니다.";
        }
        else
        {
            bossRoomEnterCount.gameObject.SetActive(false);
        }
    }

    public void SetGaugeUI(bool active, string title = "", float progress = 0f, float pZoneStartRatio = -1f, float pZoneEndRatio = -1f)
    {
        if (gaugeObject == null) return;

        if (gaugeObject.activeSelf != active)
            gaugeObject.SetActive(active);

        if (active)
        {
            if (gaugeTitle != null) gaugeTitle.text = title;
            if (gaugeFill != null) gaugeFill.value = progress;

            // [추가] 퍼펙트 존 그리기 로직
            if (perfectZoneRect != null && perfectZoneImage != null)
            {
                if (pZoneStartRatio >= 0f && pZoneEndRatio >= 0f)
                {
                    perfectZoneImage.enabled = true; // 이미지 켜기

                    perfectZoneRect.anchorMin = new Vector2(pZoneStartRatio, 0f);
                    perfectZoneRect.anchorMax = new Vector2(pZoneEndRatio, 1f);

                    perfectZoneRect.offsetMin = Vector2.zero;
                    perfectZoneRect.offsetMax = Vector2.zero;
                }
                else
                {
                    perfectZoneImage.enabled = false;
                }
            }
        }
    }

    public void SetPopUpImage(bool active, Sprite image = null)
    {
        if (imagePopUpUI == null || popUpImage == null) return;
        if (imagePopUpUI.activeSelf != active)
            imagePopUpUI.SetActive(active);

        if (active && image != null)
        {
            popUpImage.sprite = image;
        }
    }

    public void ClosePopUpImage()
    {
        if (imagePopUpUI != null)
        {
            imagePopUpUI.SetActive(false);
            popUpImage.sprite = null;

        }
    }
}
