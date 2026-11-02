using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class BuffTooltip : MonoBehaviour
{
    RectTransform rectTransform;
    private RectTransform currentOwner;

    public Vector2 offset = new Vector2(0f, 0f);

    [SerializeField] TMP_Text _buffName;
    [SerializeField] Image _buffIcon;
    [SerializeField] TMP_Text _buffDesc;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void ToggleTooltip(bool onOff, RectTransform owner = null, BuffBase buff = null)
    {
        currentOwner = onOff ? owner : null;
        this.gameObject.SetActive(onOff);

        if (!onOff) return;

        Vector2 slotPos = owner.position;

        float pivotX = slotPos.x > Screen.width / 2f ? 1f : 0f;
        float pivotY = slotPos.y > Screen.height / 2f ? 1f : 0f;

        rectTransform.pivot = new Vector2(pivotX, pivotY);

        float offsetX = pivotX == 1f ? -offset.x : offset.x;
        float offsetY = pivotY == 1f ? -offset.y : offset.y;

        rectTransform.position = new Vector2(slotPos.x + offsetX, slotPos.y + offsetY);

        UpdateTooltip(buff);
    }

    internal void HideFor(RectTransform owner)
    {
        if (owner == null || currentOwner != owner) return;
        ToggleTooltip(false);
    }

    public void UpdateTooltip(BuffBase buff)
    {
        if (buff == null) return;

        _buffIcon.sprite = buff.buffSO.buffIcon;
        _buffName.text = buff.buffSO.buffName;
        _buffDesc.text = buff.buffSO.buffDesc;

    }
}
