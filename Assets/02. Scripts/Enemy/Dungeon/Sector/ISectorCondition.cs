using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISectorCondition
{
    string SectorGoal { get; }
    string GetProgressString();

    /// <summary>
    /// 해당 섹터의 클리어 조건 충족 여부
    /// </summary>
    bool IsSatisfied {  get; }

    /// <summary>
    /// 해당 섹터 진입시 초기화 로직
    /// </summary>
    void OnConditionStart();
}
