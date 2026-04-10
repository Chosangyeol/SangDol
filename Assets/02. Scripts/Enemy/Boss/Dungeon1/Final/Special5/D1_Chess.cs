using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum EChessPhase
{
    Pawn,
    Knight,
    Bishop,
    Queen
}

public class D1_Chess : MonoBehaviour
{
    [Header("체스판 생성")]
    public GameObject tilePrefab;
    public float tileSize = 1f;
    public Material lightMat;
    public Material darkMat;
    public D1_ChessTile[,] tiles = new D1_ChessTile[8, 8];

    [Header("카메라 및 플레이어")]
    public Camera mainCam;
    public Camera chessCam;
    public GameObject playerPawn;
    public LayerMask tileLayer;

    [Header("타겟 기물 세팅")]
    private EChessPhase currentPhase;
    public GameObject[] bossPiecePrefabs; // 0:나이트, 1:비숍, 2:룩, 3:킹 (인스펙터에서 프리팹 할당)
    private GameObject currentBossPiece;  // 현재 맵에 소환된 보스 기물
    private D1_ChessTile targetTile;

    private D1_ChessTile currentTile;
    private List<D1_ChessTile> validMoves = new List<D1_ChessTile>();
    private bool isMoving = false;
    private bool isGimmickActive = false;

    private void Update()
    {
        // 테스트용 단축키 (엔터 누르면 기믹 시작)
        if (Input.GetKeyDown(KeyCode.Return) && !isGimmickActive)
        {
            StartGimmick();
        }

        // ⭐️ 마우스 클릭 감지 (기믹 중 & 이동 중이 아닐 때만 작동)
        if (isGimmickActive && !isMoving && Input.GetMouseButtonDown(0))
        {
            DetectTileClick();
        }
    }

    public void StartGimmick()
    {
        isGimmickActive = true;
        currentPhase = EChessPhase.Pawn; // 폰으로 시작

        GenerateChessBoard(transform.position);

        mainCam.gameObject.SetActive(false);
        chessCam.gameObject.SetActive(true);

        // 플레이어 시작 위치 [3, 0]
        currentTile = tiles[3, 0];
        playerPawn.transform.position = currentTile.transform.position;

        // ⭐️ 첫 번째 타겟(나이트) 소환
        SpawnTarget(EChessPhase.Pawn);

        ShowValidMoves();
    }

    private void SpawnTarget(EChessPhase phase)
    {
        int targetIndex = (int)phase;
        if (targetIndex >= bossPiecePrefabs.Length) return;

        // 플레이어와 너무 가깝지 않은 랜덤한 타일 찾기
        int rx, ry;
        do
        {
            rx = Random.Range(0, 8);
            ry = Random.Range(3, 8); // 위쪽 절반 구역에 스폰되도록 (0~2는 피함)
        }
        while (rx == currentTile.gridX && ry == currentTile.gridY);

        targetTile = tiles[rx, ry];

        // 해당 위치에 보스 기물 프리팹 생성
        currentBossPiece = Instantiate(bossPiecePrefabs[targetIndex], targetTile.transform.position, Quaternion.identity);
    }

    private void GenerateChessBoard(Vector3 centerPosition)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // 중앙 정렬을 위해 3.5f를 빼서 위치 계산
                Vector3 spawnPos = centerPosition + new Vector3((x - 3.5f) * tileSize, 0f, (y - 3.5f) * tileSize);

                // 바닥에 눕혀서(X:90) 생성
                GameObject tileObj = Instantiate(tilePrefab, spawnPos, Quaternion.Euler(90f, 0f, 0f), transform);
                tileObj.name = $"Tile_{x}_{y}";

                // 체크무늬 색상 적용
                bool isLightSquare = (x + y) % 2 == 0;
                tileObj.GetComponent<Renderer>().material = isLightSquare ? lightMat : darkMat;

                // 스크립트 추가 및 배열에 등록
                D1_ChessTile tileComponent = tileObj.AddComponent<D1_ChessTile>();
                tileComponent.gridX = x;
                tileComponent.gridY = y;

                tiles[x, y] = tileComponent;
            }
        }
    }

    private void DetectTileClick()
    {
        Ray ray = chessCam.ScreenPointToRay(Input.mousePosition);

        // 100f 거리까지 tileLayer(타일)만 맞는지 검사
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, tileLayer))
        {
            D1_ChessTile clickedTile = hit.collider.GetComponent<D1_ChessTile>();
            if (clickedTile != null)
            {
                ProcessTileClick(clickedTile);
            }
        }
    }

    private void ProcessTileClick(D1_ChessTile clickedTile)
    {
        if (validMoves.Contains(clickedTile))
        {
            // 정답 경로: 해당 타일로 부드럽게 이동
            StartCoroutine(MovePieceRoutine(clickedTile));
        }
        else
        {
            // 오답 경로: 즉시 패턴 실패 처리
            Debug.Log("🚨 초록색 칸이 아닌 곳을 눌렀습니다! 패턴 실패!");
            EndGimmick(false);
        }
    }

    private IEnumerator MovePieceRoutine(D1_ChessTile targetTile)
    {
        isMoving = true;
        ClearHighlights(); // 이동 시작 시 불빛 끄기

        Vector3 targetPos = targetTile.transform.position;
        targetPos.y = playerPawn.transform.position.y; // Y축 높이는 그대로 유지

        // 목표 지점까지 부드럽게 이동
        while (Vector3.Distance(playerPawn.transform.position, targetPos) > 0.05f)
        {
            playerPawn.transform.position = Vector3.MoveTowards(
                playerPawn.transform.position, targetPos, 5 * Time.deltaTime);
            yield return null;
        }

        // 오차 보정 및 이동 완료
        playerPawn.transform.position = targetPos;
        currentTile = targetTile;
        isMoving = false;

        CheckPromotion(targetTile);
    }

    private void CheckPromotion(D1_ChessTile clickedTile)
    {
        // 1. 방금 도착한 타일이 타겟 몬스터가 있는 타일인가?
        if (clickedTile == targetTile)
        {
            Debug.Log("🎉 타겟 처치 성공! 다음 기물로 승격합니다.");

            // 기존 몬스터 파괴 (파티클 등 연출 추가 가능)
            Destroy(currentBossPiece);

            // 2. 단계 상승
            switch (currentPhase)
            {
                case EChessPhase.Pawn:
                    currentPhase = EChessPhase.Knight;
                    SpawnTarget(currentPhase); // 비숍 소환
                    // TODO: 플레이어 외형을 나이트로 변경하는 연출 추가
                    break;
                case EChessPhase.Knight:
                    currentPhase = EChessPhase.Bishop;
                    SpawnTarget(currentPhase); // 룩 소환
                    break;
                case EChessPhase.Bishop:
                    currentPhase = EChessPhase.Queen;
                    SpawnTarget(currentPhase); // 킹 소환
                    break;
                case EChessPhase.Queen:
                    // 퀸으로 킹을 잡았다면 기믹 최종 성공!
                    EndGimmick(true);
                    return;
            }
        }

        // 타겟을 잡았든, 빈 칸으로 이동했든 다음 턴 진행
        ShowValidMoves();
    }

    public void ShowValidMoves()
    {
        ClearHighlights();

        int cx = currentTile.gridX;
        int cy = currentTile.gridY;

        switch (currentPhase)
        {
            case EChessPhase.Pawn:
                // 폰: 앞으로 1칸 이동, 대각선 앞으로 1칸 (퍼즐의 편의성을 위해 공격/이동 통합 허용)
                CheckAndAddTile(cx, cy + 1);
                CheckAndAddTile(cx - 1, cy + 1);
                CheckAndAddTile(cx + 1, cy + 1);
                break;

            case EChessPhase.Knight:
                // 나이트: L자 이동 (총 8방향)
                int[] kx = { 1, 1, 2, 2, -1, -1, -2, -2 };
                int[] ky = { 2, -2, 1, -1, 2, -2, 1, -1 };
                for (int i = 0; i < 8; i++) CheckAndAddTile(cx + kx[i], cy + ky[i]);
                break;

            case EChessPhase.Bishop:
                // 비숍: 대각선 무한 이동
                AddLineMoves(cx, cy, 1, 1);
                AddLineMoves(cx, cy, 1, -1);
                AddLineMoves(cx, cy, -1, 1);
                AddLineMoves(cx, cy, -1, -1);
                break;

            case EChessPhase.Queen:
                // 퀸: 상하좌우 + 대각선 무한 이동
                AddLineMoves(cx, cy, 1, 1); AddLineMoves(cx, cy, 1, -1);
                AddLineMoves(cx, cy, -1, 1); AddLineMoves(cx, cy, -1, -1);
                AddLineMoves(cx, cy, 1, 0); AddLineMoves(cx, cy, -1, 0);
                AddLineMoves(cx, cy, 0, 1); AddLineMoves(cx, cy, 0, -1);
                break;
        }

        // 계산된 유효 타일들을 초록색으로 켬
        foreach (var tile in validMoves)
        {
            tile.SetHighlight(true);
        }
    }

    // 체스판 범위(0~7) 안인지 확인하고 리스트에 추가하는 헬퍼 함수
    private void CheckAndAddTile(int x, int y)
    {
        if (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            validMoves.Add(tiles[x, y]);
        }
    }

    // 비숍, 퀸처럼 한 방향으로 끝까지 이동할 수 있는 경로를 계산하는 헬퍼 함수
    private void AddLineMoves(int startX, int startY, int dirX, int dirY)
    {
        int x = startX + dirX;
        int y = startY + dirY;

        while (x >= 0 && x < 8 && y >= 0 && y < 8)
        {
            validMoves.Add(tiles[x, y]);
            x += dirX;
            y += dirY;
        }
    }

    private void ClearHighlights()
    {
        foreach (var tile in validMoves) tile.SetHighlight(false);
        validMoves.Clear();
    }

    public void EndGimmick(bool isSuccess)
    {
        isGimmickActive = false;

        mainCam.gameObject.SetActive(true);
        chessCam.gameObject.SetActive(false);

        if (isSuccess) Debug.Log("🎉 패턴 파훼 성공!");
        else Debug.Log("💀 즉사기 발동!");
    }

}
