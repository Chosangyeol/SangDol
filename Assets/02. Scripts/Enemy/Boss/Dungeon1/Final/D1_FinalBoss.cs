using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_FinalBoss : BossModel
{
    [Header("일반 패턴 변수")]
    public Transform center;
    public GameObject normal1;
    public GameObject normal2;
    public GameObject normal4Warning1;
    public GameObject normal4Warning2;

    protected override void Start()
    {
        base.Start();

        normalPatterns.Add(new D1_Final_Normal1(normal1, _groundLayer, 3, 10, center));
        normalPatterns.Add(new D1_Final_Normal2(normal2, 20));
        normalPatterns.Add(new D1_Final_Normal3());
        normalPatterns.Add(new D1_Final_Normal4(normal4Warning1, normal4Warning2));
    }

    protected override void StartSpecialPattern(BossSpecialPattern pattern)
    {
        Debug.Log($"🚨 [기믹 발동] {pattern.patternName} 시작!");

        if (pattern.patternName == "쇼타임")
            StartCoroutine(Special_ShowTime());
        else if (pattern.patternName == "운명의 점")
            StartCoroutine(Special_Aracna());
    }

    #region 특수 패턴

    IEnumerator Special_ShowTime()
    {
        yield return null;
    }

    IEnumerator Special_Aracna()
    {
        yield return null;
    }

    #endregion

    #region 일반 패턴

    

    #endregion
}
