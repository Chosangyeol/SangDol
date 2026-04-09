using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SDamageInfo
{
    public float damage; // 데미지
    public GameObject source;
    public float knockDownPower; // 무력화 피해
    public bool isCounterable; // 카운터 여부
    public bool isCritical; // 치명타 여부
    public bool isHeadattack;
    public bool isBackattack;
}
