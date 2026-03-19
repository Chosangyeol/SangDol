using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragBar : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler
{
    [SerializeField] RectTransform targetUI;
    [SerializeField] Canvas canvas;

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        targetUI.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
