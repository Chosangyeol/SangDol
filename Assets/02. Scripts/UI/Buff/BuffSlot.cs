using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BuffSlot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image remainOverlay;
    [SerializeField] private TMP_Text remainTimeText;
    [SerializeField] private TMP_Text stackText;

    private Canvas rootCanvas;

    private bool isEnable = false;

    private BuffBase currentBuff;

    private BuffTooltip tooltip;

    private void Update()
    {
        if (currentBuff == null)
            return;

        UpdateBuffTimer();
    }

    public void Init(SBuff buff, BuffTooltip tooltip)
    {
        currentBuff = buff.act;
        this.tooltip = tooltip;

        if (currentBuff != null)
            iconImage.sprite = currentBuff.buffSO.buffIcon;
    }

    private void UpdateBuffTimer()
    {
        if (currentBuff.isInfinite)
        {
            remainOverlay.fillAmount = 0f;
            remainTimeText.text = "";
            if (currentBuff.currentStack > 1 && currentBuff.isStackable)
                stackText.text = "x" + currentBuff.currentStack;
            else
                stackText.text = "";
            return;
        }

        if (currentBuff.remainSecond > 0)
        {
            remainOverlay.fillAmount = 1 - currentBuff.remainSecond / currentBuff.duration;
            remainTimeText.text = currentBuff.remainSecond.ToString("F1") + "초";
            if (currentBuff.currentStack > 1 && currentBuff.isStackable)
                stackText.text = "x" + currentBuff.currentStack;
            else
                stackText.text = "";
        }
        else
        {
            remainOverlay.fillAmount = 1f;
            remainTimeText.text = "";
        }
    }

    #region 버프 툴팁 표시
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentBuff == null) return;
        if (tooltip == null) return;

        tooltip.ToggleTooltip(true, this.GetComponent<RectTransform>(),currentBuff);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentBuff == null) return;
        if (tooltip == null) return;

        tooltip.ToggleTooltip(false);
    }

    #endregion
}
