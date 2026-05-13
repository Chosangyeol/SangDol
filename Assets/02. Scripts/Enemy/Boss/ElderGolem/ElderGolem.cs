using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElderGolem_Pattern1Data
{
    public GameObject prefab;
}

[System.Serializable]
public class ElderGolem_Pattern2Data
{

}

[System.Serializable]
public class ElderGolem_Pattern3Data
{

}

public class ElderGolem_Normal
{

}

public class ElderGolem : BossModel
{
    [Header("각 패턴 변수")]
    public ElderGolem_Pattern1Data Pattern1Data;
    public ElderGolem_Pattern2Data Pattern2Data;
    public ElderGolem_Pattern3Data Pattern3Data;

    protected override void Start()
    {
        base.Start();


    }
}


