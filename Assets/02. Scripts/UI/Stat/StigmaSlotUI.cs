using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StigmaSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("노드 기본 정보")]
    public int stigmaLevel;
    public bool isTypeA;
    public EStigmaType stigamType;
    public StigmaDataSO stigmaDataSO;

    [Header("시각 연출 컴포넌트")]
    public Image nodeBImage;
    public Image nodeIconImage;
    public Image nodeLockedImage;
    public GameObject nodeGlow;
    public Image connectedLine;

    [Header("색상 세팅")]
    public Color normalColor = Color.gray;
    public Color activeColor = Color.cyan;

    // 🌟 추가: 선택(장착)되지 않은 스티그마 아이콘을 어둡게 만들 색상 세팅
    // (인스펙터 창에서 반투명도나 명도를 더 어둡게 조절할 수 있습니다)
    public Color unselectedIconColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    private StigmaUI _managerUI;
    public bool isActived = false;

    public void Init(StigmaUI ui)
    {
        _managerUI = ui;
        UpdateVisual(false, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _managerUI.OnNodeClicked(this);
    }

    public void UpdateVisual(bool actived, bool isLocked)
    {
        isActived = actived;

        if (isLocked)
        {
            if (nodeLockedImage != null) nodeLockedImage.enabled = true;
            // 잠긴 상태일 때도 아이콘을 어둡게 처리하면 더 자연스럽습니다.
            if (nodeIconImage != null) nodeIconImage.color = unselectedIconColor;
        }
        else
        {
            if (nodeLockedImage != null) nodeLockedImage.enabled = false;
        }

        // 🌟 장착(선택) 여부에 따른 아이콘 및 비주얼 어둡기 분기 처리
        if (isActived)
        {
            nodeBImage.color = activeColor;
            if (nodeGlow != null) nodeGlow.SetActive(true);
            if (connectedLine != null) connectedLine.color = activeColor;

            // 🌟 장착된 스티그마는 아이콘을 원래 밝기(원색)로 켜줍니다.
            if (nodeIconImage != null && !isLocked) nodeIconImage.color = Color.white;
        }
        else
        {
            nodeBImage.color = normalColor;
            if (nodeGlow != null) nodeGlow.SetActive(false);
            if (connectedLine != null) connectedLine.color = normalColor;

            // 🌟 장착되지 않은 스티그마는 아이콘 색상을 세팅된 색상(어둡게)으로 변경합니다.
            if (nodeIconImage != null && !isLocked) nodeIconImage.color = unselectedIconColor;
        }
    }
}