using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StigmaSlotUI : MonoBehaviour,
    IPointerClickHandler
{
    [Header("노드 기본 정보")]
    public int stigmaLevel;
    public bool isTypeA;
    public EStigmaType stigamType;

    [Header("시각 연출 컴포넌트")]
    public Image nodeBImage;
    public Image nodeIconImage;
    public Image nodeLockedImage;
    public GameObject nodeGlow;
    public Image connectedLine;

    [Header("색상 세팅")]
    public Color normalColor = Color.gray;
    public Color activeColor = Color.cyan;

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
        }
        else
        {
            if (nodeLockedImage != null) nodeLockedImage.enabled = false;
        }

        if (isActived)
        {
            nodeBImage.color = activeColor;
            if (nodeGlow != null) nodeGlow.SetActive(true);
            if (connectedLine != null) connectedLine.color = activeColor;
        }
        else
        {
            nodeBImage.color = normalColor;
            if (nodeGlow != null) nodeGlow.SetActive(false);
            if (connectedLine != null) connectedLine.color = normalColor;
        }
    }
}
