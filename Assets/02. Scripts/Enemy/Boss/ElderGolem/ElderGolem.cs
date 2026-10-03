using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ElderGolem_Pattern1Data
{
    public GameObject warning1;
    public GameObject warning2;
    public GameObject stoneSpear;
    public GameObject explosiveEffect;
}

[System.Serializable]
public class ElderGolem_Pattern2Data
{
    public GameObject rockPrefab;
}

[System.Serializable]
public class ElderGolem_Pattern3Data
{
    public GameObject centerAoe;
    public GameObject lightOrb;
    public GameObject warning;
    public GameObject effect;
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

        normalPatterns.Add(new ElderGolem_Pattern1(Pattern1Data.warning1, Pattern1Data.warning2, Pattern1Data.stoneSpear, Pattern1Data.explosiveEffect));
        normalPatterns.Add(new ElderGolem_Pattern2(Pattern2Data.rockPrefab));
        normalPatterns.Add(new ElderGolem_Pattern3(Pattern3Data.centerAoe,Pattern3Data.lightOrb, Pattern3Data.warning, Pattern3Data.effect));
    }

    protected override void Die(GameObject source = null)
    {
        // 중복 사망 방지
        if (_isDead) return;
        base.Die(source);

        // 4. 보스를 처치한 플레이어에게 보상(경험치, 골드) 지급
        if (source != null && source.TryGetComponent<CharacterModel>(out CharacterModel character))
        {
            character.Stat.GainExp(statSO.expAmount);
            character.Stat.GainGold(statSO.goldAmount);
            // 💡 팁: 추후 골렘 전용 아이템 드랍 테이블 루프가 필요하다면 여기에 작성하시면 됩니다.
        }

    }

}


