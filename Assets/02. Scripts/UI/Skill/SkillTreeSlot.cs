using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkillTreeSlot : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("UI ���� ���")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private GameObject highlight;
    [SerializeField] private TMP_Text skillName;
    [SerializeField] private TMP_Text skillLevel;
    [SerializeField] private Button levelUpButton;
    [SerializeField] private Button levelDownButton;

    private Canvas rootCanvas;
    private Image dragIcon;
    private RectTransform dragIconRect;

    private SkillToolTip _skillTooltip;

    private bool droppedOnSlot;

    private C_SkillSystem _skillSystem;

    private SkillBase currentSkill;
    public SkillBase CurrentSkill => currentSkill;

    public void Init(C_SkillSystem skillSystem, SkillBase skill, SkillToolTip skillToolTip)
    {
        _skillSystem = skillSystem;
        currentSkill = skill;
        _skillTooltip = skillToolTip;

        rootCanvas = GetComponentInParent<Canvas>();
        
        levelUpButton.onClick.AddListener((() => _skillSystem.LevelUpSkill(currentSkill)));
        levelDownButton.onClick.AddListener((() => _skillSystem.LevelDownSkill(currentSkill)));
        Refresh();
    }

    public void Refresh()
    {
        skillIcon.sprite = currentSkill.skillData.skillIcon;
        skillName.text = currentSkill.skillData.skillName;
        skillLevel.text = currentSkill.SkillLevel.ToString();
    }

    #region 드래그 드랍
    public void OnBeginDrag(PointerEventData eventData)
    {
        SkillBase skill = currentSkill;

        if (skill == null) return;
        if (skill.SkillLevel <= 0) return;
        
        droppedOnSlot = false;

        dragIcon = new GameObject("DragIcon").AddComponent<Image>();
        dragIcon.transform.SetParent(rootCanvas.transform, false);
        dragIcon.raycastTarget = false;
        dragIcon.sprite = skill.skillData.skillIcon;

        dragIconRect = dragIcon.rectTransform;
        dragIconRect.sizeDelta = skillIcon.rectTransform.sizeDelta;
        dragIconRect.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconRect == null) return;

        dragIconRect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            Destroy(dragIcon.gameObject);

        dragIcon = null;
        dragIconRect = null;
        Refresh();
    }
    #endregion

    #region 스킬 툴팁 출력
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentSkill == null) return;
        if (_skillTooltip == null) return;

        _skillTooltip.ToggleSkillTooltip(true, skillIcon.GetComponent<RectTransform>(), currentSkill);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentSkill == null) return;
        if (_skillTooltip == null) return;

        _skillTooltip.ToggleSkillTooltip(false);
    }
    #endregion
}
