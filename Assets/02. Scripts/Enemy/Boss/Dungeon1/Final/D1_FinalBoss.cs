using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_FinalBoss : BossModel
{
    [Header("일반 패턴 공용 변수")]
    public Transform center;
    

    [Header("일반 패턴1 변수")]
    public GameObject normal1;
    public int boxCount = 3;
    public float spawnRadius = 30f;
    public BuffSO stunDebuffSO;
    public float stunDuration = 1.5f;

    [Header("일반 패턴2 변수")]
    public GameObject normal2;
    public BuffSO slowDebuffSO;
    public int knifeCount = 20;
    public float n2DamagePercent = 0.1f;
    public float slowPercent = 0.2f;
    public float slowDuration = 5f;

    [Header("일반 패턴3 변수")]
    public GameObject normal3Swing;
    public GameObject normal3Warning1;
    public GameObject normal3Warning2;
    public AnimationCurve jumpCurve;
    public float n3damagePercent = 0.2f;

    [Header("일반 패턴4 변수")]
    public float n4DamagePercent = 0.2f;
    public GameObject normal4Warning1;
    public GameObject normal4Warning2;
    public float n4damagePercent = 0.15f;


    [Header("일반 패턴5 변수")]
    public GameObject hat;
    public float n5DamagePercent;
    public GameObject normal5Bullet;
    public GameObject normal5Warning;


    protected override void Start()
    {
        base.Start();

        normalPatterns.Add(new D1_Final_Normal1(normal1, _groundLayer, boxCount, spawnRadius, center, stunDebuffSO, stunDuration));
        normalPatterns.Add(new D1_Final_Normal2(normal2, slowDebuffSO, 20, n2DamagePercent, slowPercent,slowDuration,center));
        normalPatterns.Add(new D1_Final_Normal3(normal3Swing,n3damagePercent, jumpCurve, normal3Warning1,normal3Warning2));
        normalPatterns.Add(new D1_Final_Normal4(n4DamagePercent,normal4Warning1, normal4Warning2));
        normalPatterns.Add(new D1_Final_Normal5(n5DamagePercent, normal5Bullet, normal5Warning, hat));
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
