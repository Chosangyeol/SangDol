using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElderGolem_Pattern1Data
{
    public GameObject warning1;
    public GameObject warning2;
    public GameObject stoneSpear ;
}

[System.Serializable]
public class ElderGolem_Pattern2Data
{

}

[System.Serializable]
public class ElderGolem_Pattern3Data
{
    public GameObject centerAoe;
    public GameObject lightOrb;
    public GameObject warning;
}

public class ElderGolem_Normal
{

}

public class ElderGolem : BossModel
{
    [Header("일반 패턴 공용 변수")]
    public Transform center;

    [Header("각 패턴 변수")]
    public ElderGolem_Pattern1Data Pattern1Data;
    public ElderGolem_Pattern2Data Pattern2Data;
    public ElderGolem_Pattern3Data Pattern3Data;



    protected override void Start()
    {
        base.Start();

        center = GameObject.FindGameObjectWithTag("BossSpawnPos").transform;
        bossSpawnPoint = GameObject.FindGameObjectWithTag("BossSpawnPos").transform;

        normalPatterns.Add(new ElderGolem_Pattern1(Pattern1Data.warning1, Pattern1Data.warning2, Pattern1Data.stoneSpear));
        normalPatterns.Add(new ElderGolem_Pattern2());
        normalPatterns.Add(new ElderGolem_Pattern3(Pattern3Data.centerAoe,Pattern3Data.lightOrb, Pattern3Data.warning));
    }

}


