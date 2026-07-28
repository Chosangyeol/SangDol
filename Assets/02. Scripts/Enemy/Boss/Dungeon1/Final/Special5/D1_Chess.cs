using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class D1_Chess : MonoBehaviour
{
    [Header("체스판 생성")]
    public Transform tilesParent;
    public D1_ChessTile[,] tiles = new D1_ChessTile[8, 8]; // 생성된 타일 저장용 배열
    public Transform kingSpawnPos;
    public Transform startPos;
    public Transform returnPos;

    [Header("오브젝트")]
    public GameObject rookPrefab;
    public GameObject kingBoss;
    public CinemachineDollyCart chessDollyCart;

    public bool isDoing = false;
    public bool isClear = false;

    private GameObject[] rookList = new GameObject[8];
    private int currentRow = 0;
    private bool isRaidActive = false;
    private bool isKingVulnerable = false;

    private D1_FinalBoss _boss;

    [Header("카메라 및 플레이어")]
    public LayerMask tileLayer;

    private void Awake()
    {
        D1_ChessTile[] allTiles = tilesParent.GetComponentsInChildren<D1_ChessTile>();

        for (int i = 0; i < allTiles.Length; i++)
        {
            int x = i % 8;
            int y = i / 8;

            tiles[x, y] = allTiles[i];
            tiles[x, y].gridX = x;
            tiles[x, y].gridY = y;

        }

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (y == 0)
                    tiles[x, y].OpenPath(false); // 0번 줄은 열어둠
                else
                    tiles[x, y].OpenPath(true);  // 나머지는 꽉 막음
            }
        }
    }

    private void Update()
    {

    }

    public void StartCheckmate(D1_FinalBoss boss)
    {
        _boss = boss;
        isDoing = true;
        StartCoroutine(RaidSequenceRoutine());
    }

    private IEnumerator Timer(float time)
    {
        yield return new WaitForSeconds(time);

        if (!isClear)
            _boss.Target.Damaged(2.0f, true);
    }

    public void ReturnBackStage()
    {
        StartCoroutine(ReturnStage());
    }

    private IEnumerator ReturnStage()
    {
        _boss.Target.ChangeCam(0, false);

        yield return new WaitForSeconds(2f);

        _boss.Target.Navmesh.enabled = false;
        _boss.Target.transform.position = returnPos.position;
        _boss.Target.Navmesh.enabled = true;

        yield return new WaitForSeconds(1f);

        _boss.EndSpecialPattern();
    }

    private IEnumerator RaidSequenceRoutine()
    {
        isRaidActive = true;
        currentRow = 1;

        // 킹 소환

        GameObject king = Instantiate(kingBoss, kingSpawnPos);
        king.GetComponent<D1_King>().SetImmunity(true);
        king.GetComponent<D1_King>().Init(this);

        while (king.transform.position.y > 1)
        {
            king.transform.position += Vector3.down * 40 * Time.deltaTime;
            yield return null;
        }
        

        yield return new WaitForSeconds(1f);

        _boss.Target.ChangeCam(1, false);

        yield return new WaitForSeconds(1f);


        // 룩 돌진 시작
        for (int row = 1; row < 6; row++)
        {
            // 길 열고
            OpenRow(row);

            yield return new WaitForSeconds(1f);

            // 룩 소환
            SpawnRook();
            // 돌진 대기

            yield return new WaitForSeconds(3f);

            if (row == 1)
                StartCoroutine(BishopAttack(0));
            StartCoroutine(BishopAttack(row));
        }

        yield return StartCoroutine(CounterRook());

        king.GetComponent<D1_King>().SetImmunity(false);

        yield return new WaitForSeconds(2f);

        _boss.Target.ChangeCam(0, false);

        yield return new WaitForSeconds(3f);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                tiles[x,y].UnActiveBishop();
            }
        }

        StartCoroutine(PawnAttack());
    }

    private void SpawnRook()
    {
        int safe = Random.Range(1, 7);

        for (int i = 0; i < 8;i++)
        {
            if (i == safe) continue;

            GameObject rook = Instantiate(rookPrefab, tiles[i, 7].gameObject.transform.position + Vector3.up * 30,Quaternion.Euler(new Vector3(0,180,0)));
            rookList[i] = rook;
            rook.GetComponent<D1_Rook>().Init(false);
        }
    }

    // 다음 줄 오픈
    private void OpenRow(int row)
    {
        for (int x = 0; x < 8; x++)
            tiles[x,row].OpenPath(false);
    }

    IEnumerator BishopAttack(int row)
    {
        yield return new WaitForSeconds(1f);

        for (int x = 0; x < 8; x++)
            tiles[x, row].ActiveBishopAttack();
    }

    // 마지막 줄에서 카운터 가능한 룩 생성
    IEnumerator CounterRook()
    {
        OpenRow(6);

        int safe = Random.Range(1, 7);

        for (int i = 0; i < 8; i++)
        {
            if (i == safe)
            {
                GameObject safeRook = Instantiate(rookPrefab, tiles[i, 7].gameObject.transform.position + Vector3.up * 30, Quaternion.Euler(new Vector3(0, 180, 0)));
                safeRook.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                rookList[i] = safeRook;
                safeRook.GetComponent<D1_Rook>().Init(true,true);

                continue;
            }

            GameObject rook = Instantiate(rookPrefab, tiles[i, 7].gameObject.transform.position + Vector3.up * 30, Quaternion.Euler(new Vector3(0, 180, 0)));
            rookList[i] = rook;
            rook.GetComponent<D1_Rook>().Init(false,true);

        }

        yield return new WaitForSeconds(1f);
        OpenRow(7);

        yield return new WaitForSeconds(2f);

        StartCoroutine(BishopAttack(6));
    }

    // 룩 카운터 이후 폰 공격 시작
    IEnumerator PawnAttack()
    {
        while (isDoing)
        {
            List<D1_ChessTile> allTiles = GetTiles();

            for (int i = 0; i < allTiles.Count;i++)
            {
                allTiles[i].SetHighlight(true);
            }

            yield return new WaitForSeconds(1.5f);

            for (int i = 0; i < allTiles.Count;i++)
            {
                allTiles[i].SetHighlight(false);
            }

            
            for (int i = 0; i < allTiles.Count; i++)
            {
                allTiles[i].SpawnPawnAndAttack();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    public List<D1_ChessTile> GetTiles()
    {
        List<D1_ChessTile> allTiles = new List<D1_ChessTile>();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (tiles[x, y] != null)
                    allTiles.Add(tiles[x, y]);
            }
        }

        for (int i = 0; i < allTiles.Count; i++)
        {
            int rand = Random.Range(i, allTiles.Count);
            D1_ChessTile temp = allTiles[i];
            allTiles[i] = allTiles[rand];
            allTiles[rand] = temp;
        }

        List<D1_ChessTile> targetTiles = allTiles.GetRange(0, 30);

        return targetTiles;
    }

    public void ResetChess()
    {
        // 1. 모든 코루틴 정지 (돌진 대기, 폰 소환 등)
        StopAllCoroutines();

        isDoing = false;
        isClear = false;
        isRaidActive = false;

        // 2. 소환되었던 룩(Rook) 전부 삭제
        for (int i = 0; i < rookList.Length; i++)
        {
            if (rookList[i] != null)
            {
                Destroy(rookList[i]);
                rookList[i] = null;
            }
        }

        // 3. 소환되었던 킹(King) 삭제 (kingSpawnPos 아래에 있는 모든 자식 삭제)
        foreach (Transform child in kingSpawnPos)
        {
            Destroy(child.gameObject);
        }

        // 4. 모든 타일 초기화 (길 막기, 이펙트 끄기)
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (tiles[x, y] != null)
                {
                    tiles[x, y].ResetTile();

                    // 첫 줄 빼고 다 막기 (Awake 때와 동일하게 세팅)
                    if (y == 0)
                        tiles[x, y].OpenPath(false);
                    else
                        tiles[x, y].OpenPath(true);
                }
            }
        }

        if (chessDollyCart != null)
        {
            chessDollyCart.m_Position = 0f;
        }

        // 5. 컷씬 애니메이션 초기화 로직
        // D1_FinalBoss 스크립트에서 넘겨받은 컷씬 오브젝트가 있다면 시간을 0으로 돌립니다.
        if (_boss != null && _boss.Special5.cutSceneObj != null)
        {
            UnityEngine.Playables.PlayableDirector director = _boss.Special5.cutSceneObj.GetComponent<UnityEngine.Playables.PlayableDirector>();
            if (director != null)
            {
                director.Stop();
                director.time = 0; // 타임라인 재생 시간을 처음으로 되돌림
                director.Evaluate(); // 되돌린 시간을 즉시 씬에 적용하여 오브젝트들 위치를 원복시킴
            }
            _boss.Special5.cutSceneObj.SetActive(false);
        }

        Debug.Log("체스 미니게임 상태 초기화 완료");
    }
}
