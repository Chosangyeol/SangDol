using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Rendering.Universal;

public class SkillSlot : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [Header("스킬 슬룻 설정")]
    public C_Enums.SkillSlot skillSlot;

    [Header("UI 세팅")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text slotKey;

    private Canvas rootCanvas;
    private Image dragIcon;
    private RectTransform dragIconRect;

    private bool droppedOnSlot;

    private C_SkillSystem _skillSystem;

    private SkillBase currentSkill =>
        _skillSystem.GetSkillToSlot(skillSlot);

    public SkillBase CurrentSkill => currentSkill;

    public void Init(C_SkillSystem skillSystem)
    {
        _skillSystem = skillSystem;
        slotKey.text = skillSlot.ToString();

        rootCanvas = GetComponentInParent<Canvas>();
        Refresh();
    }

    public void Refresh()
    {
        SkillBase skill = CurrentSkill;
        
        iconImage.enabled = true;

        if (skill != null)
            iconImage.sprite = skill.skillData.skillIcon;

        if (skill != null && skill.SkillLevel <= 0)
        {
            _skillSystem.ClearSkillSlot(skillSlot);
            iconImage.sprite = null;
        }
    }


    #region Ŭ��
    public void OnPointerClick(PointerEventData eventData)
    {
        SkillBase skill = CurrentSkill;

        if (skill == null) return;

        if (eventData.clickCount == 2)
        {
            // ��ųƮ�� ����
            Debug.Log("��ųƮ�� ����");
        }

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            _skillSystem.ClearSkillSlot(skillSlot);
        }
    }
    #endregion

    #region �巡��& ���
    public void OnBeginDrag(PointerEventData eventData)
    {
        SkillBase skill = CurrentSkill;

        if (skill == null) return;

        droppedOnSlot = false;

        dragIcon = new GameObject("DragIcon").AddComponent<Image>();
        dragIcon.transform.SetParent(rootCanvas.transform, false);
        dragIcon.raycastTarget = false;
        dragIcon.sprite = skill.skillData.skillIcon;

        dragIconRect = dragIcon.rectTransform;
        dragIconRect.sizeDelta = iconImage.rectTransform.sizeDelta;
        dragIconRect.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconRect == null) return;

        dragIconRect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!droppedOnSlot)
            _skillSystem.ClearSkillSlot(skillSlot);

        if (dragIcon != null)
            Destroy(dragIcon.gameObject);

        dragIcon = null;
        dragIconRect = null;
        Refresh();
    }

    public void OnDrop(PointerEventData eventData)
    {
        SkillSlot fromSlot = eventData.pointerDrag?.GetComponent<SkillSlot>();
        if (fromSlot != null && fromSlot != this)
        {
            _skillSystem.Swap(fromSlot.skillSlot, this.skillSlot);
            Refresh();
            fromSlot.Refresh();
            return;
        }

        // 2) ��ųƮ�� -> ���� ���(���)
        SkillTreeSlot fromTree = eventData.pointerDrag?.GetComponent<SkillTreeSlot>();
        if (fromTree != null)
        {
            SkillBase skill = fromTree.CurrentSkill;
            if (skill == null) return;

            _skillSystem.RegisterSkillToSlot(this.skillSlot, skill);
            Refresh();
            return;
        }
    }
    #endregion
}
