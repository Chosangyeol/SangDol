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
    public string warpName;
    public Transform targetPos;
    public Transform playerRespawn;

    [Header("연결된 섹터")]
    public SectorController nextSector;

    [Header("보스전 전용")]
    public bool isBossRoom = false;

    [Header("체크포인트")]

    [Header("연출 설정")]
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
    public bool isEnterStart = true;

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

        dungeonUI.SetActive(false);

        if (isEnterStart)
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
        WarpData data = warpDatas[index];
        dungeonStepIndex = index;

        _model.PlayerController.StopMove();

        _model.SetControlable(false);

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
        _model.SetControlable(true);

        if (data.nextSector != null)
        {
            data.nextSector.ActivateSector();
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
        _model.SetControlable(false);

        yield return new WaitForSeconds(3f);

        _model.transform.position = warpDatas[dungeonStepIndex].playerRespawn.position;

        _model.Revive();

        SectorController current = warpDatas[dungeonStepIndex].nextSector;

        if (current != null)
        {
            foreach (var condition in current.conditions)
            {
                if (condition is EnemySector es) es.ResetCondition();
                else if (condition is MiddleBossSector mbs) mbs.ResetCondition();
                else if (condition is FinalBossSector fbs) fbs.ResetCondition();
            }

        }
        yield return new WaitForSeconds(3f);

        _model.SetControlable(true);
    }
    #endregion

    #region 던전 UI

    public void UpdateDungeonUI()
    {
        dungeonUI.SetActive(true);

        SectorController nowSector = allSectors[currentSector];
        ISectorCondition currentCondition = nowSector.conditions[nowSector.currentConditionIndex];

        sectorName.text = nowSector.sectorName;

        sectorGoal.text = currentCondition.GetProgressString();
    }

    #endregion


}
