using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
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
    [SerializeField] private Transform _playerRevivePos;

    [Header("던전 구성")]
    public string dungeonName;
    public List<SectorController> allSectors;
    public int currentSector = 0;
    public bool isEnterStart = true;
    [SerializeField] PoolingListSO fieldEnemyListSO;
    public AudioClip dungeonBGM;
    public string outSpawnPointName;
    public Sprite dungeonOutLoadingImage;

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
        if (PoolManager.Instance != null && fieldEnemyListSO != null)
        {
            PoolManager.Instance.LoadStagePools(fieldEnemyListSO);
        }

        dungeonStepIndex = 0;
        currentSector = 0;
        _model = GameObject.FindObjectOfType<CharacterModel>();

        dungeonUI.SetActive(false);

        if (isEnterStart)
            StartCoroutine(StartDungeon());
    }

    // 🌟 신규 추가: 어떤 경로로든 섹터가 '활성화'되면 매니저의 인덱스를 강제로 동기화합니다.
    // 이 함수 덕분에 SectorController나 WarpData가 제멋대로 다음 섹터를 켜도 인덱스가 절대 꼬이지 않습니다.
    public void RegisterActiveSector(SectorController activeSector)
    {
        int index = allSectors.IndexOf(activeSector);
        if (index != -1)
        {
            currentSector = index;
            Debug.Log($"<color=green>[DungeonManager] 현재 활성 섹터 동기화 완료: Index [{currentSector}] - {activeSector.sectorName}</color>");
        }
    }

    public void OnSectorCleared(SectorController sector)
    {
        int clearedIndex = allSectors.IndexOf(sector);

        // 던전의 마지막 섹터를 클리어했는지 검사
        if (clearedIndex >= allSectors.Count - 1)
        {
            OnDungeonComplete();
        }
        else
        {
            // 💡 팁: 이제 개별 섹터가 체인 형식으로 다음 섹터를 직접 켜주므로, 
            // 여기서는 안전하게 인덱스 백업 및 UI 최신화만 보조합니다.
            if (currentSector == clearedIndex)
            {
                currentSector = clearedIndex + 1;
            }
            UpdateDungeonUI();
        }
    }

    IEnumerator StartDungeon()
    {
        yield return new WaitForSeconds(2f);

        if (allSectors.Count > 0 && allSectors[0] != null)
        {
            allSectors[0].ActivateSector();
            if (dungeonBGM != null)
            {
                AudioManager.instance.PlayBGM(dungeonBGM);
            }
        }
    }

    private void OnDungeonComplete()
    {
        Debug.Log("★ 던전의 모든 위협을 제거했습니다! ★");
        // 결과창 UI 출력, 클리어 포탈 생성 등 기획 추가 구간


        StartCoroutine(DungeonOut());
    }

    IEnumerator DungeonOut()
    {
        yield return new WaitForSeconds(5f);
        SceneChanger.instance.LoadScene("Map1-Forest", outSpawnPointName, dungeonOutLoadingImage);
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

        if (warpDatas[index].playerRespawn != null)
            _playerRevivePos = warpDatas[index].playerRespawn;

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
            warpDatas[index].hasPlayed = true;
        }

        _model.transform.position = warpDatas[index].targetPos.position;
        _model.cams[0].PreviousStateIsValid = false;
        _model.SetControlable(true);

        if (data.nextSector != null)
        {
            // 🌟 여기서 섹터가 켜지면서 RegisterActiveSector를 타고 currentSector가 안전하게 매핑됩니다.
            data.nextSector.ActivateSector();
        }
    }

    public void ReplacePlayer()
    {
        StartCoroutine(ReplaceSequence());
    }

    private IEnumerator ReplaceSequence()
    {
        _model.SetControlable(false);
        GameEvent.OnBossStateChange?.Invoke(null); // 보스 체력바 끄기

        yield return new WaitForSeconds(3f);

        AudioManager.instance.PlayBGM(dungeonBGM);

        if (_playerRevivePos != null)
        {
            _model.transform.position = _playerRevivePos.position;
        }
        else
        {
            Debug.LogWarning("저장된 부활 위치가 없습니다. 제자리에서 부활합니다.");
        }

        _model.Revive();

        if (allSectors != null && currentSector < allSectors.Count)
        {
            Debug.Log($"기존 섹터 [{allSectors[currentSector].sectorName}] 초기화 시작");
            SectorController current = allSectors[currentSector];

            current.ResetSector();

            bool needsWarp = warpDatas.Exists(w => w.nextSector == current && w.isBossRoom);
            if (!needsWarp)
            {
                current.ActivateSector();
            }

            UpdateDungeonUI();
        }

        yield return new WaitForSeconds(3f);
    }
    #endregion

    #region 던전 UI
    public void UpdateDungeonUI()
    {
        if (allSectors == null || allSectors.Count <= currentSector || allSectors[currentSector] == null) return;

        dungeonUI.SetActive(true);

        SectorController nowSector = allSectors[currentSector];

        // 🌟 예외 방어막: 섹터가 클리어되어 조건 인덱스가 넘쳤을 때의 OutOfBounds 에러를 방지합니다.
        int conditionIdx = Mathf.Clamp(nowSector.currentConditionIndex, 0, nowSector.conditions.Count - 1);

        if (nowSector.conditions.Count > 0 && nowSector.conditions[conditionIdx] != null)
        {
            ISectorCondition currentCondition = nowSector.conditions[conditionIdx];
            sectorName.text = nowSector.sectorName;
            sectorGoal.text = currentCondition.GetProgressString();
        }
    }
    #endregion
}