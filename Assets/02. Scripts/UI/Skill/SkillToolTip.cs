using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillToolTip : MonoBehaviour
{
    RectTransform rectTransform;

    public Vector2 offset = new Vector2(10f,30f);

    [Header("스킬 툴팁 UI")]
    [SerializeField] private TMP_Text _skillName;
    [SerializeField] private Image _skillIcon;
    [SerializeField] private TMP_Text _skillCool;
    [SerializeField] private TMP_Text _skillLevel;
    [SerializeField] private TMP_Text _skillType;
    [SerializeField] private TMP_Text _skillDamage;
    [SerializeField] private TMP_Text _skillDesc;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ToggleSkillTooltip(bool onoff, RectTransform owner = null, SkillBase skill = null)
    {
        if (!onoff)
        {
            this.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);
        canvasGroup.alpha = 0f;
        StartCoroutine(TooltipCo(owner, skill));
    }

    private IEnumerator TooltipCo(RectTransform owner = null, SkillBase skill = null)
    {


        Vector2 slotPos = owner.position;

        float pivotX = slotPos.x > Screen.width / 2f ? 1f : 0f;
        float pivotY = slotPos.y > Screen.height / 2f ? 1f : 0f;

        rectTransform.pivot = new Vector2(pivotX, pivotY);

        float offsetX = pivotX == 1f ? -offset.x : offset.x;
        float offsetY = pivotY == 1f ? -offset.y : offset.y;

        rectTransform.position = new Vector2(slotPos.x + offsetX, slotPos.y + offsetY);

        UpdateTooltip(skill);

        yield return null;

        canvasGroup.alpha = 1f;
    }

    public void UpdateTooltip(SkillBase skill)
    {
        if (skill == null) return;

        _skillName.text = skill.skillData.skillName;
        _skillIcon.sprite = skill.skillData.skillIcon;
        _skillLevel.text = $"{skill.SkillLevel}레벨";
        _skillCool.text = $"기본 쿨타임: {skill.skillData.skillCool}초";
        if (!skill.skillData.isChargeSkill)
        {
            _skillType.text = "타입: 일반";
        }
        else
        {
            if (skill.skillData.isHoldingSkill)
            {
                _skillType.text = "타입: 홀딩";
            }
            else
            {
                _skillType.text = "타입: 차징";
            }
        }

        if (skill.SkillLevel == 0)
        {
            _skillDamage.text = $"데미지: {skill.skillData.damageMultipliers[0] * 100f}%";
        }
        else
        {
            _skillDamage.text = $"데미지: {skill.GetCurrentDamageMultiplier() * 100f}%";
        }
        _skillDesc.text = skill.skillData.skillDesc;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
}
