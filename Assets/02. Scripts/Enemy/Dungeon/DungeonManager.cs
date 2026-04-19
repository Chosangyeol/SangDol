using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

[System.Serializable]
public class WarpData
{
    public Transform targetPos;
    public bool isBossRoom = false;
    public EnemySector bossSector;
    public Transform playerRespawn;
    public bool hasVideo;
    public VideoClip clip;
    public bool hasAudio;
    public C_Enums.BGM_List bgm;
    public bool hasPlayed;
}

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager instance;

    [Header("던전 내부 계산 변수들")]
    [SerializeField] private int dungeonStepIndex = 0;
    [SerializeField] private List<WarpData> warpDatas = new List<WarpData>();
    private CharacterModel _model;

    [Header("던전 구성")]
    public string dungeonName;
    public List<SectorController> allSectors;
    public int currentSector = 0;

    [Header("던전 UI")]
    public GameObject dungeonUI;
    public TMP_Text sectorName;
    public TMP_Text sectorGoal;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);  
    }

    private void Start()
    {
        dungeonStepIndex = 0;
        currentSector = 0;
        _model = GameObject.FindObjectOfType<CharacterModel>();

        StartCoroutine(StartDungeon());
    }

    public void OnSectorCleared(SectorController sector)
    {
        int clearedIndex = allSectors.IndexOf(sector);

        if (clearedIndex >= allSectors.Count-1)
        {
            OnDungeonComplete();
        }
        else
        {
            currentSector = clearedIndex + 1;
        }
    }

    IEnumerator StartDungeon()
    {
        yield return new WaitForSeconds(2f);

        allSectors[0].ActivateSector();
    }
    private void OnDungeonComplete()
    {
        Debug.Log("★ 던전의 모든 위협을 제거했습니다! ★");
        // 결과창 UI 출력, 포탈 생성 등
    }

    #region 워프
    public void WarpPlayer(int index)
    {
        StartCoroutine(WarpSequence(index));
    }

    private IEnumerator WarpSequence(int index)
    {
        dungeonStepIndex = index;

        _model.PlayerController.StopMove();
        _model.canMove = false;
        _model.canAttack = false;
        _model.canUse = false;
        _model.canSkill = false;
        _model.Navmesh.enabled = false;

        GameEvent.OnBossRoomEnterCount?.Invoke(true, 0f);

        yield return new WaitForSeconds(2f);

        GameEvent.OnBossRoomEnterCount?.Invoke(false, 0f);

        yield return new WaitForSeconds(1f);

        if (warpDatas[index].hasAudio)
            AudioManager.instance.PlayBGM(warpDatas[index].bgm);

        if (warpDatas[index].hasVideo && !warpDatas[index].hasPlayed)
        {
            VideoPlayManager.instance.PlayVideo(warpDatas[index].clip);


            yield return new WaitUntil(() => !VideoPlayManager.instance.isPlaying);

            // 3. 다음번에 워프할 때는 다시 재생되지 않도록 true로 바꿔줍니다.
            warpDatas[index].hasPlayed = true;
        }

        _model.transform.position = warpDatas[index].targetPos.position;
        _model.cams[0].PreviousStateIsValid = false;

        // 2. 조작 해제
        _model.Navmesh.enabled = true;
        _model.canAttack = true;
        _model.canUse = true;
        _model.canMove = true;
        _model.canSkill = true;


        // 3. 보스방이라면 EnemySector 실행
        if (warpDatas[index].isBossRoom && warpDatas[index].bossSector != null)
        {
            // 보스 섹터 시작 (EnemySector 내의 SpawnRoutine이 실행됨)
            warpDatas[index].bossSector.OnConditionStart();

            // [선택 사항] 보스가 죽을 때까지 기다렸다가 던전 클리어 처리를 하고 싶다면:
            StartCoroutine(MonitorBossClear(warpDatas[index].bossSector));
        }
    }

    private IEnumerator MonitorBossClear(EnemySector bossSector)
    {
        yield return new WaitUntil(() => bossSector.IsSatisfied);

        Debug.Log("★ 보스 처치 완료! 던전 최종 클리어 ★");
        // 여기에 결과창 UI나 보상 연출 추가
    }

    public void ReplacePlayer()
    {
        StartCoroutine(ReplaceSequence());
    }

    private IEnumerator ReplaceSequence()
    {
        yield return new WaitForSeconds(3f);

        _model.Navmesh.enabled = false;
        _model.canMove = false;
        _model.canUse = false;

        _model.transform.position = warpDatas[dungeonStepIndex].playerRespawn.position;

        _model.Revive();

        yield return new WaitForSeconds(3f);

        _model.canMove = true;
    }
    #endregion

    #region 던전 UI

    public void UpdateDungeonUI()
    {
        SectorController nowSector = allSectors[currentSector];
        ISectorCondition currentCondition = nowSector.conditions[nowSector.currentConditionIndex];

        sectorName.text = nowSector.sectorName;

        if (nowSector.conditions[nowSector.currentConditionIndex] is EnemySector enemyS)
        {
            sectorGoal.text = currentCondition.GetProgressString();
            return;
        }

        sectorGoal.text = nowSector.conditions[nowSector.currentConditionIndex].SectorGoal;
    }

    #endregion


}
