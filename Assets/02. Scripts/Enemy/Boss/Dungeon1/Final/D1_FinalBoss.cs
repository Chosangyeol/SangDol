using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class D1_Final_Normal1Data
{
    public GameObject prefab;
    public LayerMask groundLayer;
    public int boxCount = 3;
    public float spawnRadius = 30f;
    public BuffSO stunDebuffSO;
    public float stunDuration = 1.5f;
}

[System.Serializable]
public class D1_Final_Normal2Data
{
    public GameObject prefab;
    public BuffSO slowDebuffSO;
    public int knifeCount = 20;
    public float damagePercent = 0.1f; 
    public float slowPercent = 0.2f;
    public float slowDuration = 5f;
}

[System.Serializable]
public class D1_Final_Normal3Data
{
    public GameObject swingPrefab;
    public GameObject warning1;
    public GameObject warning2;
    public GameObject effect1;
    public GameObject effect2;
    public AnimationCurve jumpCurve;
    public float damagePercent = 0.2f;
}

[System.Serializable]
public class D1_Final_Normal4Data
{
    public float damagePercent = 0.2f;
    public GameObject warning1;
    public GameObject warning2;
    public GameObject effect1;
    public GameObject effect2;
}

[System.Serializable]
public class D1_Final_Normal5Data
{
    public GameObject hat;
    public GameObject bulletPrefab;
    public GameObject warningPrefab;
    public float damagePercent;
}

[System.Serializable]
public class D1_Final_Special1Data
{
    public GameObject warning1;
    public GameObject warning2;
    public GameObject effect1;
    public GameObject effect2;
    public float damagePercent;
}

[System.Serializable]
public class D1_Final_Special2Data
{
    public GameObject prefab;
}

[System.Serializable]
public class D1_Final_Special3Data
{
    [Tooltip("D1_MiddleBoss 컴포넌트를 포함한 중간 보스 프리팹")]
    public GameObject prefab;
    public VideoClip cutsceneClip;
    [Tooltip("컷씬과 전투에서 함께 사용하는 씬의 중간 보스. 비어 있으면 prefab을 생성합니다.")]
    public D1_MiddleBoss sceneMiddleBoss;
    [Tooltip("중앙 이동과 동영상 종료 후 재생할 Cinemachine Timeline")]
    public PlayableDirector cutsceneDirector;
    public Transform waitingArea;
    public Transform middleBossSpawnPoint;
    public Transform returnPoint;
}

[System.Serializable]
public class D1_Final_Special4Data
{
    public GameObject yabawiPrefab;
    public int shuffleCount = 5;
    public float swapDuration = 1.5f;
    public GameObject crownPrefab;
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public BuffSO panicBuffSO;
    public BuffSO stunBuffSO;
    public GameObject crownSpawnPos;
    public GameObject swingPrefab;
    public GameObject ballPrefab;
}

[System.Serializable]
public class D1_Final_Special5Data
{
    public D1_Chess prefab;
    public GameObject cutSceneObj;
}

public class D1_FinalBoss : BossModel
{
    [Header("일반 패턴 공용 변수")]
    public Transform center;
    public Transform playerStartPos;


    [Header("각 일반 패턴 변수")]
    public D1_Final_Normal1Data pattern1;
    public D1_Final_Normal2Data pattern2;
    public D1_Final_Normal3Data pattern3;
    public D1_Final_Normal4Data pattern4;
    public D1_Final_Normal5Data pattern5;

    [Header("각 특수 패턴 변수")]
    public D1_Final_Special1Data Special1;
    public D1_Final_Special2Data Special2;
    public D1_Final_Special3Data Special3;
    public D1_Final_Special4Data Special4;
    public D1_Final_Special5Data Special5;

    public SkinnedMeshRenderer[] bossMeshs;

    protected override void Start()
    {
        base.Start();

        center = GameObject.FindGameObjectWithTag("BossSpawnPos").transform;
        playerStartPos = GameObject.FindGameObjectWithTag("PlayerStart").transform;
        bossSpawnPoint = GameObject.FindGameObjectWithTag("BossSpawnPos").transform;

        // Temporary in-scene anchors for exercising Special3 before the production arena is authored.
        if (Special3 != null)
        {
            if (Special3.waitingArea == null)
                Special3.waitingArea = GameObject.Find("TEST_Special3_WaitingArea")?.transform;
            if (Special3.middleBossSpawnPoint == null)
                Special3.middleBossSpawnPoint = GameObject.Find("TEST_Special3_MiddleBossSpawn")?.transform;
            if (Special3.returnPoint == null)
                Special3.returnPoint = GameObject.Find("TEST_Special3_ReturnPoint")?.transform;
        }

        pattern5.hat = GameObject.FindGameObjectWithTag("D1_Final_N5");

        Special5.prefab = GameObject.FindGameObjectWithTag("D1_Final_S5").GetComponent<D1_Chess>();
        
        GameObject parent = GameObject.Find("Root");
        Special5.cutSceneObj = parent.transform.Find("D1_Special5CutScene").gameObject;

        Special5.cutSceneObj.SetActive(false);

        bossMeshs = GetComponentsInChildren<SkinnedMeshRenderer>();

        normalPatterns.Add(new D1_Final_Normal1(pattern1, center));
        normalPatterns.Add(new D1_Final_Normal2(pattern2, center));
        normalPatterns.Add(new D1_Final_Normal3(pattern3));
        normalPatterns.Add(new D1_Final_Normal4(pattern4));
        normalPatterns.Add(new D1_Final_Normal5(pattern5));

    }

    public override void Reset()
    {
        base.Reset();
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(C_Enums.BGM_List.D1_Final_BGM2);
            AudioManager.instance.PlaySFX(C_Enums.SFX_List.D1_Final_Enter);
        }
    }

    protected override void StartSpecialPattern(BossSpecialPattern pattern)
    {
        Debug.Log($"🚨 [기믹 발동] {pattern.patternName} 시작!");

        if (pattern.patternName == "쇼타임")
            StartCoroutine(Special_ShowTime());
        else if (pattern.patternName == "운명의 점")
            StartCoroutine(Special_Aracna());
        else if (pattern.patternName == "중간 보스")
            StartCoroutine(Special_MiddleBoss());
        else if (pattern.patternName == "야바위")
            StartCoroutine(Special_Mix());
        else if (pattern.patternName == "체크메이트")
            StartCoroutine(Special_Chess());
    }

    #region 특수 패턴

    IEnumerator ReadyForSpecial()
    {
        if (Agent != null) Agent.enabled = false;

        Vector3 playerPos = Target.transform.position;

        playerPos.x = 0;
        playerPos.z = 0;

        transform.Rotate(playerPos);

        GameObject swing = GameObject.Instantiate(
            pattern3.swingPrefab,
            transform.position + (transform.forward * 3f) + new Vector3(0f, 60f, 0), // 위치 수정
            transform.rotation
            );

        patternObjects.Add(swing);

        while (swing.transform.position.y > 2)
        {
            swing.transform.Translate(Vector3.down * 30 * Time.deltaTime, Space.World);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        Vector3 startPos = transform.position;
        Vector3 targetPos = swing.transform.position + Vector3.up * 2.5f;

        float time = 0f;

        if (Anim != null) Anim.SetTrigger("Jump");

        while (time < 1.5f)
        {
            time += Time.deltaTime;

            // 1. 진행도 (0.0 ~ 1.0)
            float t = time / 1.5f;

            // 2. 앞으로 나아가는 '속도' 조절 (이징)
            float curveT = pattern3.jumpCurve.Evaluate(t);

            // 3. 직선 위치 계산 (Base Position)
            Vector3 basePos = Vector3.Lerp(startPos, targetPos, curveT);

            // ⭐️ 4. 포물선 높이 계산 (Sine 함수 사용)
            // t가 0일때 0, 0.5(중간)일때 최고점(1 * Height), 1일때 다시 0이 됩니다.
            float arc = Mathf.Sin(t * Mathf.PI) * 2.5f;

            // 5. 직선 위치에 포물선 높이(Y축)를 더해서 최종 위치 적용!
            transform.position = basePos + new Vector3(0f, arc, 0f);

            yield return null;
        }

        transform.position = targetPos;

        yield return new WaitForSeconds(0.5f);

        while (transform.position.y < 20)
        {
            transform.Translate(Vector3.up * 60 * Time.deltaTime, Space.World);
            swing.transform.Translate(Vector3.up * 60 * Time.deltaTime, Space.World);
            yield return null;
        }

        foreach (SkinnedMeshRenderer mesh in bossMeshs)
        {
            mesh.enabled = false;
        }

        Destroy(swing);

        GameEvent.OnBossStateChange?.Invoke(null);
    }

    public void EndSpecialPattern()
    {
        StartCoroutine(EndSpecial());
    }

    IEnumerator EndSpecial(bool completePattern = true)
    {
        foreach (SkinnedMeshRenderer mesh in bossMeshs)
        {
            mesh.enabled = true;
        }

        GameEvent.OnBossStateChange?.Invoke(this);

        GameObject swing = GameObject.Instantiate(
            pattern3.swingPrefab,
            center.transform.position + new Vector3(0f, 20f, 3f), // 위치 수정
            Quaternion.identity
            );

        patternObjects.Add(swing);

        transform.position = swing.transform.position + Vector3.up * 2.5f;

        Vector3 centerPos = center.transform.position;

        centerPos.x = 0;
        centerPos.z = 0;

        transform.Rotate(centerPos);

        yield return new WaitForSeconds(0.5f);

        while (swing.transform.position.y > 3)
        {
            transform.Translate(Vector3.down * 40 * Time.deltaTime, Space.World);
            swing.transform.Translate(Vector3.down * 40 * Time.deltaTime, Space.World);
            yield return null;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = center.transform.position;

        float time = 0f;

        if (Anim != null) Anim.SetTrigger("Jump");

        while (time < 1.5f)
        {
            time += Time.deltaTime;

            // 1. 진행도 (0.0 ~ 1.0)
            float t = time / 1.5f;

            // 2. 앞으로 나아가는 '속도' 조절 (이징)
            float curveT = pattern3.jumpCurve.Evaluate(t);

            // 3. 직선 위치 계산 (Base Position)
            Vector3 basePos = Vector3.Lerp(startPos, targetPos, curveT);

            // ⭐️ 4. 포물선 높이 계산 (Sine 함수 사용)
            // t가 0일때 0, 0.5(중간)일때 최고점(1 * Height), 1일때 다시 0이 됩니다.
            float arc = Mathf.Sin(t * Mathf.PI) * 2.5f;

            // 5. 직선 위치에 포물선 높이(Y축)를 더해서 최종 위치 적용!
            transform.position = basePos + new Vector3(0f, arc, 0f);

            yield return null;
        }

        transform.position = targetPos;

        yield return new WaitForSeconds(0.5f);

        if (completePattern)
        {
            if (Agent != null) Agent.enabled = true;
            isDoingSpecial = false;
        }

        while (swing.transform.position.y < 20)
        {
            swing.transform.Translate(Vector3.up * 40 * Time.deltaTime, Space.World);
            yield return null;
        }

        if (completePattern) SetImmunity(false);

        Destroy(swing);


    }

    public IEnumerator DestroyEffect(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (effect != null)
            Destroy(effect);
    }

    IEnumerator Special_ShowTime()
    {
        transform.LookAt(playerStartPos);

        yield return new WaitForSeconds(2f);

        Anim.SetTrigger("Special1");

        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y + 0.6f, transform.position.z);

        GameObject warning1 = Instantiate(Special1.warning1, spawnPos, Quaternion.identity);
        patternObjects.Add(warning1);

        yield return new WaitForSeconds(2f);   

        Destroy(warning1);
        

        yield return new WaitForSeconds(0.2f);

        GameObject effect1 = Instantiate(Special1.effect1, spawnPos, Quaternion.identity);
        patternObjects.Add(effect1);
        StartCoroutine(DestroyEffect(effect1, 0.5f));

        Collider[] targets = Physics.OverlapSphere(center.transform.position, 20f, LayerMask.GetMask("Player"));

        foreach (Collider target in targets)
        {
            Target.Damaged(Special1.damagePercent, true);
        }

        yield return new WaitForSeconds(0.2f);

        GameObject warning2 = Instantiate(Special1.warning2, spawnPos, Quaternion.identity);
        patternObjects.Add(warning2);

        yield return new WaitForSeconds(2f);

        Destroy(warning2);

        yield return new WaitForSeconds(0.2f);

        GameObject effect2 = Instantiate(Special1.effect2, spawnPos, Quaternion.identity);
        patternObjects.Add(effect2);
        StartCoroutine(DestroyEffect(effect2, 0.5f));

        targets = Physics.OverlapSphere(center.transform.position, 40f, LayerMask.GetMask("Player"));
        Collider[] safe = Physics.OverlapSphere(center.transform.position, 20f, LayerMask.GetMask("Player"));

        var finalHits = targets.Except(safe);

        foreach (Collider target in finalHits)
        {
            Target.Damaged(Special1.damagePercent, true);
        }

        yield return new WaitForSeconds(1f);

        isDoingSpecial = false;
    }

    IEnumerator Special_Aracna()
    {
        SetImmunity(true);

        yield return StartCoroutine(ReadyForSpecial());

        Vector3 spawnPos = center.position + new Vector3(0, -1.3f, 0);
        GameObject special = Instantiate(Special2.prefab, spawnPos, Quaternion.identity);
        patternObjects.Add(special);
        special.GetComponentInChildren<SurvivalPattern1>().Init(this);
        AudioManager.instance.PlaySFX(C_Enums.SFX_List.D1_Final_S2);
    }

    private D1_MiddleBoss _special3MiddleBoss;
    private bool special3UsesSceneBoss;
    private bool special3InEncounter;
    [SerializeField, Min(0f)] private float special3ReturnDelay = 3f;
    internal bool IsSpecial3Active => special3Moving || special3ControlsLocked || special3InEncounter;
    private bool special3ControlsLocked;
    private VideoPlayManager special3Video;
    private int special3VideoSession;
    private PlayableDirector special3Director;
    private bool special3DirectorWasActive;
    private DirectorWrapMode special3DirectorWrapMode;
    private bool special3Moving;
    private bool special3AgentWasEnabled;
    private AudioSource special3Bgm;
    private AudioClip special3BgmClip;
    private bool special3BgmWasMuted;
    private bool special3BgmWasPlaying;

    private void PauseSpecial3Bgm()
    {
        if (special3Bgm != null || AudioManager.instance == null) return;
        var source = AudioManager.instance.transform.Find("BGM_Player")?.GetComponent<AudioSource>();
        if (source == null) return;
        special3Bgm = source;
        special3BgmClip = source.clip;
        special3BgmWasMuted = source.mute;
        special3BgmWasPlaying = source.isPlaying;
        source.mute = true;
        if (special3BgmWasPlaying) source.Pause();
    }

    private void RestoreSpecial3Bgm()
    {
        if (special3Bgm != null)
        {
            special3Bgm.mute = special3BgmWasMuted;
            if (special3BgmWasPlaying && special3Bgm.isActiveAndEnabled && special3Bgm.clip == special3BgmClip)
                special3Bgm.UnPause();
        }
        special3Bgm = null;
        special3BgmClip = null;
        special3BgmWasPlaying = false;
    }

    private IEnumerator WaitForNormalPatternEnd()
    {
        while (currentPattern != null) yield return null;

        // Let the Animator consume the normal pattern's end trigger before checking Idle.
        if (Anim != null && Anim.runtimeAnimatorController != null) Anim.SetBool("Move", false);
        yield return null;
        int idleState = Animator.StringToHash("Base Layer.Idle");
        while (Anim != null && Anim.isActiveAndEnabled && Anim.runtimeAnimatorController != null
            && Anim.HasState(0, idleState)
            && (Anim.IsInTransition(0) || !Anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")))
            yield return null;
    }

    private bool IsSpecial3Configured()
    {
        D1_MiddleBoss middleBossPrefab = Special3 != null && Special3.sceneMiddleBoss != null
            ? Special3.sceneMiddleBoss
            : Special3 != null && Special3.prefab != null ? Special3.prefab.GetComponentInChildren<D1_MiddleBoss>(true) : null;

        return Special3 != null
            && middleBossPrefab != null
            && middleBossPrefab.statSO != null
            && Special3.waitingArea != null
            && Special3.middleBossSpawnPoint != null
            && center != null
            && playerStartPos != null
            && Target != null && !Target.isDie && !Target.IsExternalControlLocked;
    }

    private IEnumerator MoveBossToCenter()
    {
        if (pattern3 == null || pattern3.swingPrefab == null || pattern3.jumpCurve == null)
        {
            Debug.LogWarning("[D1_FinalBoss] Special3 swing presentation unavailable; moving to center directly.");
            if (Agent != null) Agent.enabled = false;
            transform.position = center.position;
            yield return null;
            yield break;
        }

        yield return ReadyForSpecial();
        yield return EndSpecial(false);
    }

    private IEnumerator PlaySpecial3Timeline()
    {
        PlayableDirector director = Special3.cutsceneDirector;
        if (director == null || director.playableAsset == null)
        {
            Debug.LogWarning("[D1_FinalBoss] Special3 Cinemachine Timeline unavailable; continuing the middle boss encounter.");
            yield break;
        }

        special3Director = director;
        special3DirectorWasActive = director.gameObject.activeSelf;
        special3DirectorWrapMode = director.extrapolationMode;
        var presentation = GetSpecial3Presentation();
        bool useFade = presentation != null && IsSpecial3Active;
        if (useFade) yield return presentation.FadeTo(1f);
        director.extrapolationMode = useFade ? DirectorWrapMode.Hold : DirectorWrapMode.None;
        GameEvent.OnUIInvisable?.Invoke();
        director.gameObject.SetActive(true);
        director.time = 0d;
        if (DungeonManager.instance != null) DungeonManager.instance.PlayCutscene(director);
        else director.Play();
        if (useFade) yield return presentation.FadeTo(0f);

        while (director != null && director.isActiveAndEnabled && director.state == PlayState.Playing
            && Target != null && !Target.isDie)
        {
            if (useFade && director.duration - director.time <= presentation.FadeDuration)
            {
                director.Pause();
                yield return presentation.FadeTo(1f);
                break;
            }
            yield return null;
        }

        if (useFade && Target != null && !Target.isDie) yield return presentation.FadeTo(1f);
        StopSpecial3Timeline();
    }

    private void StopSpecial3Timeline()
    {
        if (special3Director != null)
        {
            special3Director.Stop();
            special3Director.extrapolationMode = special3DirectorWrapMode;
            bool keepArena = isDoingSpecial && Special3 != null && Special3.sceneMiddleBoss != null && Target != null && !Target.isDie;
            special3Director.gameObject.SetActive(keepArena || special3DirectorWasActive);
            if (Target != null && !Target.isDie)
            {
                if (Target.cams != null && Target.cams.Length > 0 && Target.cams[0] != null)
                    Target.ChangeCam(0, true);
                GameEvent.OnMainUIviable?.Invoke();
            }
        }
        special3Director = null;
    }

    private void RestoreSpecial3Movement()
    {
        if (!special3Moving) return;
        if (Agent != null) Agent.enabled = false;
        if (center != null) transform.position = center.position;
        if (Agent != null)
        {
            Agent.enabled = special3AgentWasEnabled;
            if (Agent.enabled && Agent.isOnNavMesh)
            {
                Agent.Warp(transform.position);
                Agent.ResetPath();
                Agent.isStopped = true;
                Agent.velocity = Vector3.zero;
            }
        }
        special3Moving = false;
    }

    private void WarpPlayerTo(Transform destination)
    {
        if (Target == null || destination == null) return;

        Target.PlayerController?.StopMove();
        Target.SetControlable(false);
        Target.transform.SetPositionAndRotation(destination.position, destination.rotation);
        special3ControlsLocked = true;

        if (Target.Navmesh != null)
        {
            Target.Navmesh.enabled = true;
            if (!Target.Navmesh.Warp(destination.position))
                Debug.LogWarning("[D1_FinalBoss] 플레이어 위치가 NavMesh에 없어 Transform 기준으로 이동했습니다.");
            if (Target.Navmesh.isOnNavMesh)
                Target.Navmesh.ResetPath();
        }

        for (int i = 0; Target.cams != null && i < Target.cams.Length; i++)
        {
            if (Target.cams[i] != null)
                Target.cams[i].PreviousStateIsValid = false;
        }
    }

    private IEnumerator Special_MiddleBoss()
    {
        yield return WaitForNormalPatternEnd();
        while (Target != null && !Target.isDie && Target.IsExternalControlLocked) yield return null;

        if (!IsSpecial3Configured())
        {
            Debug.LogWarning("[D1_FinalBoss] Special3 설정이 부족합니다. 영상, 중간 보스 프리팹(D1_MiddleBoss), 대기 위치와 스폰 위치를 설정한 뒤 사용할 수 있습니다.");
            SetImmunity(false);
            isDoingSpecial = false;
            yield break;
        }

        SetImmunity(true);
        DisableCounter();

        special3AgentWasEnabled = Agent != null && Agent.enabled;
        special3Moving = true;
        yield return MoveBossToCenter();
        while (Target != null && !Target.isDie && Target.IsExternalControlLocked) yield return null;
        if (Target == null || Target.isDie) yield break;
        Target.PlayerController?.StopMove();
        Target.SetControlable(false);
        special3ControlsLocked = true;
        PauseSpecial3Bgm();
        special3Video = VideoPlayManager.instance;
        if (special3Video != null && special3Video.TryPlayVideo(Special3.cutsceneClip, out special3VideoSession))
        {
            if (GetSpecial3Presentation() != null) special3Video.HoldFrameForTransition(special3VideoSession);
            while (special3Video != null && special3Video.IsPlaybackActive(special3VideoSession) &&
                Target != null && !Target.isDie) yield return null;
        }
        else Debug.LogWarning("[D1_FinalBoss] Special3 cutscene unavailable; continuing the middle boss encounter.");
        var transitionPresentation = GetSpecial3Presentation();
        if (transitionPresentation != null && Target != null && !Target.isDie) yield return transitionPresentation.FadeTo(1f);
        if (special3Video != null) special3Video.CancelPlayback(special3VideoSession);
        special3Video = null; special3VideoSession = 0;

        if (Target == null || Target.isDie) yield break;

        yield return PlaySpecial3Timeline();
        if (Target == null || Target.isDie) yield break;

        GameObject spawnedMiddleBoss = null;
        special3UsesSceneBoss = Special3.sceneMiddleBoss != null;
        if (special3UsesSceneBoss)
        {
            var presentation = Special3.cutsceneDirector != null ? Special3.cutsceneDirector.GetComponentInParent<D1_FinalBoss60Presentation>() : null;
            if (presentation != null) presentation.BeginEncounter();
            _special3MiddleBoss = Special3.sceneMiddleBoss;
            _special3MiddleBoss.BeginCombat(Special3.middleBossSpawnPoint);
        }
        else if (_special3MiddleBoss == null || _special3MiddleBoss.IsDead || !_special3MiddleBoss.gameObject.activeInHierarchy)
        {
            if (_special3MiddleBoss != null && _special3MiddleBoss.IsDead && _special3MiddleBoss.gameObject.activeInHierarchy)
                Destroy(_special3MiddleBoss.gameObject);

            spawnedMiddleBoss = Instantiate(
                Special3.prefab,
                Special3.middleBossSpawnPoint.position,
                Special3.middleBossSpawnPoint.rotation);

            _special3MiddleBoss = spawnedMiddleBoss.GetComponentInChildren<D1_MiddleBoss>(true);
        }
        else
        {
            _special3MiddleBoss.ResetBossState();
        }

        if (_special3MiddleBoss == null || _special3MiddleBoss.Stat == null)
        {
            Debug.LogError("[D1_FinalBoss] Special3 프리팹에서 D1_MiddleBoss를 찾지 못했습니다.");
            if (spawnedMiddleBoss != null) Destroy(spawnedMiddleBoss);
            Target.SetControlable(true);
            special3ControlsLocked = false;
            RestoreSpecial3Movement();
            RestoreSpecial3Bgm();
            FinishSpecial3Arena();
            special3UsesSceneBoss = false;
            SetImmunity(false);
            isDoingSpecial = false;
            yield break;
        }

        _special3MiddleBoss.isInField = true;
        _special3MiddleBoss.isCombatStarted = false;
        _special3MiddleBoss.bossSpawnPoint = Special3.middleBossSpawnPoint;

        WarpPlayerTo(Special3.waitingArea);
        special3InEncounter = true;
        if (transitionPresentation != null) yield return transitionPresentation.FadeTo(0f);
        Target.SetControlable(true);
        special3ControlsLocked = false;

        GameEvent.OnBossStateChange?.Invoke(_special3MiddleBoss);
        while (Target != null && !Target.isDie && _special3MiddleBoss != null && !_special3MiddleBoss.IsDead)
        {
            Vector3 playerPosition = Target.transform.position;
            Vector3 bossPosition = _special3MiddleBoss.transform.position;
            playerPosition.y = 0f;
            bossPosition.y = 0f;

            if (!_special3MiddleBoss.isCombatStarted
                && (playerPosition - bossPosition).sqrMagnitude <= Mathf.Pow(_special3MiddleBoss.Stat.attackRange, 2f))
            {
                _special3MiddleBoss.isCombatStarted = true;
                Debug.Log("[D1_FinalBoss] 플레이어가 중간 보스 공격 범위에 진입해 전투를 시작합니다.");
            }

            yield return null;
        }

        // 플레이어 사망 시 BossModel의 기존 OnPlayerDie 초기화/부활 흐름을 그대로 둡니다.
        if (Target == null || Target.isDie) yield break;

        while (_special3MiddleBoss != null && _special3MiddleBoss.IsDead && !_special3MiddleBoss.IsDeathSequenceFinished && Target != null && !Target.isDie) yield return null;
        if (Target == null || Target.isDie) yield break;

        if (_special3MiddleBoss != null && _special3MiddleBoss.IsDead)
        {
            if (!special3UsesSceneBoss) Destroy(_special3MiddleBoss.gameObject);
            _special3MiddleBoss = null;
        }

        Transform returnPoint = Special3.returnPoint != null ? Special3.returnPoint : playerStartPos;
        WarpPlayerTo(returnPoint);
        Target.SetControlable(true);
        special3ControlsLocked = false;
        special3InEncounter = false;
        FinishSpecial3Arena();
        special3UsesSceneBoss = false;

        RestoreSpecial3Movement();
        RestoreSpecial3Bgm();
        SetImmunity(false);
        if (special3ReturnDelay > 0f) yield return new WaitForSeconds(special3ReturnDelay);
        isDoingSpecial = false;
    }

    private D1_FinalBoss60Presentation GetSpecial3Presentation()
    {
        return Special3 != null && Special3.cutsceneDirector != null
            ? Special3.cutsceneDirector.GetComponentInParent<D1_FinalBoss60Presentation>() : null;
    }

    IEnumerator Special_Mix()
    {
        SetImmunity(true);

        yield return StartCoroutine(ReadyForSpecial());

        yield return new WaitForSeconds(1f);

        GameObject yabawi = Instantiate(Special4.yabawiPrefab, center.transform.position - new Vector3(0, 0, 4),Quaternion.identity);
        patternObjects.Add(yabawi);

        AudioManager.instance.PlaySFX(C_Enums.SFX_List.D1_Final_S4);

        yabawi.GetComponent<D1_Yabawe>().StartYabawi(this);

        // 광대 생성
        StartCoroutine(SpawnCrown());
        StartCoroutine(SpawnSwing());
        StartCoroutine(SpawnBall());

    }

    private IEnumerator SpawnCrown()
    {
        GameObject crownSpawnPos = Instantiate(Special4.crownSpawnPos, center.transform.position, Quaternion.identity);
        patternObjects.Add(crownSpawnPos);

        int childCount = crownSpawnPos.transform.childCount;
        Transform[] spawnPoints = new Transform[childCount];

        for (int i = 0; i < childCount; i++)
        {
            spawnPoints[i] = crownSpawnPos.transform.GetChild(i);
        }

        // 4. 배열 무작위 섞기 (Fisher-Yates Shuffle)
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // i부터 배열 끝 사이에서 무작위 인덱스 뽑기
            int randomIndex = Random.Range(i, spawnPoints.Length);

            // 현재 자리(i)와 무작위로 뽑힌 자리(randomIndex)의 값을 서로 교환(Swap)
            Transform temp = spawnPoints[i];
            spawnPoints[i] = spawnPoints[randomIndex];
            spawnPoints[randomIndex] = temp;
        }

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            GameObject crown = Instantiate(Special4.crownPrefab, spawnPoints[i].position, Quaternion.identity);
            patternObjects.Add(crown);
            crown.GetComponent<D1_Crown>().Init(Special4.bulletPrefab, Special4.bulletSpeed, Special4.panicBuffSO, Special4.stunBuffSO, Target);
            yield return new WaitForSeconds(1f);
        }

        Destroy(crownSpawnPos);
    }

    private IEnumerator SpawnSwing()
    {
        GameObject swingSpawnPos = Instantiate(Special4.crownSpawnPos, center.transform.position, Quaternion.identity);
        patternObjects.Add(swingSpawnPos);

        // 2. 자식들을 배열 대신 List에 담기 (넣고 빼기 쉽게 하기 위함)
        int childCount = swingSpawnPos.transform.childCount;
        List<Transform> availablePoints = new List<Transform>();

        for (int i = 0; i < childCount; i++)
        {
            availablePoints.Add(swingSpawnPos.transform.GetChild(i));
        }

        // 3. 중복 없이 랜덤으로 3개 뽑기
        int pickCount = Mathf.Min(3, availablePoints.Count); // 혹시 자식이 3개 미만일 때의 에러 방지용 안전장치
        List<Transform> selectedPoints = new List<Transform>();

        for (int i = 0; i < pickCount; i++)
        {
            // 남은 위치들 중에서 무작위로 하나 선택
            int randomIndex = Random.Range(0, availablePoints.Count);

            // 선택된 위치를 최종 타겟 리스트에 추가
            selectedPoints.Add(availablePoints[randomIndex]);

            // ⭐️ [핵심] 방금 뽑은 위치를 후보 리스트에서 아예 지워버림 (중복 절대 불가)
            availablePoints.RemoveAt(randomIndex);
        }

        // 4. 뽑힌 3개의 위치에 각각 그네 생성 및 발사
        foreach (Transform spawnPoint in selectedPoints)
        {
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.y = 0;

            // [방향 설정 1] Center를 바라보는 방향 계산
            Vector3 dirToCenter = center.transform.position - spawnPos;
            dirToCenter.y = 0f; // 수평 비행을 위해 높이 차이 무시

            Quaternion lookRotation = Quaternion.LookRotation(dirToCenter);

            // [방향 설정 2] -22.5 ~ +22.5도 사이의 랜덤 오프셋
            float randomAngle = Random.Range(-15f, 15f);
            Quaternion randomOffset = Quaternion.Euler(0f, randomAngle, 0f);

            // 최종 조준 각도 = 기본 방향 * 랜덤 오프셋
            Quaternion finalRotation = lookRotation * randomOffset;

            // 5. 그네 생성
            GameObject swingObj = Instantiate(Special4.swingPrefab, spawnPos, finalRotation);
            patternObjects.Add(swingObj);

            swingObj.GetComponent<D1_Swing>().Init();

            yield return new WaitForSeconds(2.5f);
        }

        Destroy(swingSpawnPos);
    }

    private IEnumerator SpawnBall()
    {
        GameObject ballSpawnPos = Instantiate(Special4.crownSpawnPos, center.transform.position, Quaternion.identity);
        patternObjects.Add(ballSpawnPos);

        // 2. 자식들을 배열 대신 List에 담기 (넣고 빼기 쉽게 하기 위함)
        int childCount = ballSpawnPos.transform.childCount;
        List<Transform> availablePoints = new List<Transform>();

        for (int i = 0; i < childCount; i++)
        {
            availablePoints.Add(ballSpawnPos.transform.GetChild(i));
        }

        // 3. 중복 없이 랜덤으로 3개 뽑기
        int pickCount = Mathf.Min(4, availablePoints.Count); // 혹시 자식이 3개 미만일 때의 에러 방지용 안전장치
        List<Transform> selectedPoints = new List<Transform>();

        for (int i = 0; i < pickCount; i++)
        {
            // 남은 위치들 중에서 무작위로 하나 선택
            int randomIndex = Random.Range(0, availablePoints.Count);

            // 선택된 위치를 최종 타겟 리스트에 추가
            selectedPoints.Add(availablePoints[randomIndex]);

            // ⭐️ [핵심] 방금 뽑은 위치를 후보 리스트에서 아예 지워버림 (중복 절대 불가)
            availablePoints.RemoveAt(randomIndex);
        }

        // 4. 뽑힌 3개의 위치에 각각 그네 생성 및 발사
        foreach (Transform spawnPoint in selectedPoints)
        {
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.y = 0;

            // [방향 설정 1] Center를 바라보는 방향 계산
            Vector3 dirToCenter = center.transform.position - spawnPos;
            dirToCenter.y = 0f; // 수평 비행을 위해 높이 차이 무시

            Quaternion lookRotation = Quaternion.LookRotation(dirToCenter);

            // [방향 설정 2] -22.5 ~ +22.5도 사이의 랜덤 오프셋
            float randomAngle = Random.Range(-22.5f, 22.5f);
            Quaternion randomOffset = Quaternion.Euler(0f, randomAngle, 0f);

            // 최종 조준 각도 = 기본 방향 * 랜덤 오프셋
            Quaternion finalRotation = lookRotation * randomOffset;

            // 5. 그네 생성
            GameObject ball = Instantiate(Special4.ballPrefab, spawnPos, finalRotation);
            patternObjects.Add(ball);

            ball.GetComponent<D1_Ball>().Init();

            yield return new WaitForSeconds(3.5f);
        }

        Destroy(ballSpawnPos);
    }

    IEnumerator Special_Chess()
    {
        SetImmunity(true);

        yield return new WaitForSeconds(2f);

        yield return StartCoroutine(ReadyForSpecial());

        yield return new WaitForSeconds(0.5f);

        Target.canMove = false;

        yield return new WaitForSeconds(0.5f);

        GameEvent.OnUIInvisable?.Invoke();

        Special5.cutSceneObj.SetActive(true);
        PlayableDirector director = Special5.cutSceneObj.GetComponent<PlayableDirector>();
        if (DungeonManager.instance != null) DungeonManager.instance.PlayCutscene(director);
        else director.Play();

        yield return new WaitForSeconds(0.5f);

        AudioManager.instance.PlayBGM(C_Enums.BGM_List.D1_Final_BGM3);

        if (director != null)
            yield return new WaitUntil(() => director.state != PlayState.Playing);
        else
            yield return new WaitForSeconds(1f);

        Target.ChangeCam(0, true);

        Target.Navmesh.enabled = false;

        Target.transform.position = Special5.prefab.startPos.position;

        Target.Navmesh.enabled = true;

        yield return new WaitForSeconds(0.5f);


        Target.SetCanMove();
        GameEvent.OnMainUIviable?.Invoke();


        yield return new WaitForSeconds(0.5f);

        Special5.prefab.StartCheckmate(this);
    }

    protected override void OnActionsStopped()
    {
        StopSpecial3Timeline();
        if (special3InEncounter && Target != null && !Target.isDie)
            WarpPlayerTo(Special3.returnPoint != null ? Special3.returnPoint : playerStartPos);
        special3InEncounter = false;
        FinishSpecial3Arena();
        RestoreSpecial3Bgm();
        RestoreSpecial3Movement();
        if (special3Video != null) special3Video.CancelPlayback(special3VideoSession);
        special3Video = null; special3VideoSession = 0;
        if (special3ControlsLocked && Target != null && !Target.isDie) Target.SetControlable(true);
        special3ControlsLocked = false;
        // 1. 자식 클래스만의 특수한 찌꺼기 제거
        // 보스가 하늘로 올라갔다가(메시 끄기) 안 내려온 상태로 끝날 수 있으니 메시 다시 켜주기
        foreach (SkinnedMeshRenderer mesh in bossMeshs ?? new SkinnedMeshRenderer[0])
        {
            if (mesh != null) mesh.enabled = true;
        }

        // 체스 컷씬이 도중에 멈췄다면 끄기
        if (Special5 != null && Special5.cutSceneObj != null)
        {
            if (Special5.cutSceneObj.activeSelf)
            {
                var director = Special5.cutSceneObj.GetComponent<PlayableDirector>();
                if (director != null) director.Stop();
                if (Target != null && !Target.isDie)
                {
                    Target.SetCanMove();
                    if (Target.cams != null && Target.cams.Length > 0 && Target.cams[0] != null)
                        Target.ChangeCam(0, true);
                    GameEvent.OnMainUIviable?.Invoke();
                }
            }
            Special5.cutSceneObj.SetActive(false);
        }

        if (Special5 != null && Special5.prefab != null && Special5.prefab.gameObject.scene.IsValid())
        {
            Special5.prefab.ResetChess();
        }

        if (_special3MiddleBoss != null)
        {
            _special3MiddleBoss.ForceStopCurrentAction();
            if (!special3UsesSceneBoss) Destroy(_special3MiddleBoss.gameObject);
            _special3MiddleBoss = null;
        }
        special3UsesSceneBoss = false;
    }

    private void FinishSpecial3Arena()
    {
        if (Special3 == null || Special3.cutsceneDirector == null) return;
        var presentation = Special3.cutsceneDirector.GetComponentInParent<D1_FinalBoss60Presentation>();
        if (presentation != null) presentation.FinishEncounter();
    }

    public override void ResetBossState() => base.ResetBossState();

    #endregion

}
