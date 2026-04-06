using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkillSlot : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("스킬 슬룻 설정")]
    public C_Enums.SkillSlot skillSlot;

    // ⭐️ 추가: 이 슬롯을 쿨타임에만 보여줄 것인가? (스페이스바 슬롯만 true로 체크)
    public bool showOnlyOnCooldown = false;

    [Header("UI 세팅")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image coolOverlay;
    [SerializeField] private TMP_Text slotKey;
    [SerializeField] private TMP_Text coolTimeText;
    [SerializeField] private bool cantChange = false;

    private Canvas rootCanvas;
    private Image dragIcon;
    private RectTransform dragIconRect;
    private bool droppedOnSlot;


    private C_SkillSystem _skillSystem;
    private CanvasGroup _canvasGroup; // ⭐️ 추가: 투명도 조절용 컴포넌트

    private SkillBase currentSkill => _skillSystem?.GetSkillToSlot(skillSlot);
    public SkillBase CurrentSkill => currentSkill;

    private void Update()
    {
        UpdateSkillCool();
    }

    public void Init(C_SkillSystem skillSystem)
    {
        _skillSystem = skillSystem;

        // ⭐️ 추가: CanvasGroup이 없다면 자동으로 붙여줍니다.
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (slotKey != null)
            slotKey.text = skillSlot.ToString();

        rootCanvas = GetComponentInParent<Canvas>();
        Refresh();
    }

    public void Refresh()
    {
        SkillBase skill = CurrentSkill;

        if (skill != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = skill.skillData.skillIcon;

            if (skill.SkillLevel <= 0)
            {
                _skillSystem.ClearSkillSlot(skillSlot);
                iconImage.enabled = false;
                iconImage.sprite = null;
            }
        }
        else
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }
    }

    public void UpdateSkillCool()
    {
        if (_skillSystem == null) return;

        SkillBase skill = CurrentSkill;

        // 스킬이 아예 장착되지 않은 경우
        if (skill == null)
        {
            if (showOnlyOnCooldown && _canvasGroup != null)
                _canvasGroup.alpha = 0f; // 숨기기
            return;
        }

        if (!skill.canUse)
        {
            // ⭐️ 쿨타임 중: 슬롯 전체를 보이게 만듦 (투명도 1)
            if (showOnlyOnCooldown && _canvasGroup != null)
                _canvasGroup.alpha = 1f;

            if (!coolTimeText.IsActive())
                coolTimeText.gameObject.SetActive(true);

            coolTimeText.text = Mathf.Ceil(skill.nowCoolTime).ToString() + "s";
            float fillAmount = skill.nowCoolTime / skill.coolTime;
            coolOverlay.fillAmount = fillAmount;
        }
        else
        {
            // ⭐️ 사용 가능: 슬롯 전체를 숨김 (투명도 0)
            if (showOnlyOnCooldown && _canvasGroup != null)
                _canvasGroup.alpha = 0f;

            coolTimeText.gameObject.SetActive(false);
            coolOverlay.fillAmount = 0f;
        }
    }



    #region 클릭
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

    #region 드래그
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (cantChange) return;

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
        if (cantChange) return;

        if (dragIconRect == null) return;

        dragIconRect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (cantChange) return;

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
        if (cantChange) return;

        SkillSlot fromSlot = eventData.pointerDrag?.GetComponent<SkillSlot>();
        if (fromSlot != null && fromSlot != this)
        {
            _skillSystem.Swap(fromSlot.skillSlot, this.skillSlot);
            Refresh();
            fromSlot.Refresh();
            return;
        }

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
