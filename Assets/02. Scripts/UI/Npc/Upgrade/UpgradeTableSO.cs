using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    /// <summary>
    /// 타겟 강화 단계 ( 예 : 1 => 1단계로 강화 )
    /// </summary>
    public int targetLevel;
    /// <summary>
    /// 성공 확률 ( 100 이하 )
    /// </summary>
    public float successRate;
    /// <summary>
    /// 실패 시 파괴 여부
    /// </summary>
    public bool breakable;
    /// <summary>
    /// 강화 요구 골드
    /// </summary>
    public int requireGold;
    /// <summary>
    /// 강화 요구 재료 리스트
    /// </summary>
    public string[] requireMaterialID;
    /// <summary>
    /// 강화 요구 재료 갯수
    /// </summary>
    public int[] requiredMaterialAmount;
}

[CreateAssetMenu(fileName = "New UpgradeTableSO", menuName = "System/ItemUpgradeTable")]
public class UpgradeTableSO : ScriptableObject
{
    [Header("강화 테이블 설정")]
    public List<UpgradeData> data;

    public UpgradeData GetUpgradeData(int currentLevel)
    {
        if (currentLevel >= data.Count)
        {
            return null;
        }
        return data[currentLevel];
    }
}
