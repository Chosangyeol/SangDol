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
        _isDead = true;
        Agent.enabled = false;

        // 1. UI 및 이벤트 버스 신호 전달 (체력바 끄기, 킬 카운트)
        GameEvent.OnBossStateChange?.Invoke(null);
        GameEvent.OnMonsterKill?.Invoke(statSO.enemyID);

        // 2. 사망 애니메이션 트리거 작동
        if (Anim != null)
        {
            Anim.SetTrigger("Die");
        }

        // 3. 사망 도중 밀리거나 움직이지 않도록 내비메시 에이전트 전면 정지
        if (Agent != null && Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
            Agent.ResetPath();
        }

        // 4. 보스를 처치한 플레이어에게 보상(경험치, 골드) 지급
        if (source != null && source.TryGetComponent<CharacterModel>(out CharacterModel character))
        {
            character.Stat.GainExp(statSO.expAmount);
            character.Stat.GainGold(statSO.goldAmount);
            // 💡 팁: 추후 골렘 전용 아이템 드랍 테이블 루프가 필요하다면 여기에 작성하시면 됩니다.
        }

        // 5. 플레이 중인 모든 패턴 기믹 및 장판 오브젝트 일제 청소
        ForceStopCurrentAction();

        // 6. [선택지 A] 코루틴 방식을 쓸 경우 활성화 (기본 상태)
        StartCoroutine(ExecuteDeathSequence());
    }

    /// <summary>
    /// [방법 A] 코루틴을 사용하여 지정된 초(Duration)만큼 대기한 후 풀로 반환하는 루틴
    /// </summary>
    private IEnumerator ExecuteDeathSequence()
    {
        yield return new WaitForSeconds(3f);

        // 최종적으로 오브젝트 풀에 반환
        PoolManager.Instance.Push(this);
    }

}


