using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BuffSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image remainOverlay;
    [SerializeField] private TMP_Text remainTimeText;
    [SerializeField] private TMP_Text stackText;

    private Canvas rootCanvas;

    private bool isEnable = false;

    private BuffBase currentBuff;

    private void Update()
    {
        if (currentBuff == null)
            return;

        UpdateBuffTimer();
    }

    public void Init(SBuff buff)
    {
        currentBuff = buff.act;

        if (currentBuff != null)
            iconImage.sprite = currentBuff.buffSO.buffIcon;
    }

    private void UpdateBuffTimer()
    {
        remainTimeText.text = currentBuff.remainSecond.ToString("F1") + "초";

        if (currentBuff.remainSecond > 0)
        {
            remainOverlay.fillAmount = 1 - currentBuff.remainSecond / currentBuff.duration;
            if (currentBuff.currentStack > 1)
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
}
