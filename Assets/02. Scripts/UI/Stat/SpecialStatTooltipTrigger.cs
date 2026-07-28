using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpecialStatTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Action<C_Enums.SpecialStat> _onPointerEnterAction;
    private Action _onPointerExitAction;
    private C_Enums.SpecialStat _statType;

    // 🌟 어떤 UI 스크립트든 원하는 함수를 람다식으로 안전하게 연결할 수 있는 범용 셋업
    public void Setup(Action<C_Enums.SpecialStat> onEnter, Action onExit, C_Enums.SpecialStat stat)
    {
        _onPointerEnterAction = onEnter;
        _onPointerExitAction = onExit;
        _statType = stat;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _onPointerEnterAction?.Invoke(_statType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _onPointerExitAction?.Invoke();
    }

    private void OnDisable()
    {
        _onPointerExitAction?.Invoke();
    }
}
