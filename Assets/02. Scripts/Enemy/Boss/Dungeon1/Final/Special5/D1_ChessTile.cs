using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class D1_ChessTile : MonoBehaviour
{
    public int gridX;
    public int gridY;

    public NavMeshObstacle obstacle;

    public GameObject warning;
    public GameObject pawnPrefab;
    public GameObject bishopEffect;

    public bool isHighlighted = false;
    public bool isBishopActive = false;

    private Renderer rend;
    private Color originColor;


    private void Awake()
    {
        obstacle = GetComponent<NavMeshObstacle>();
        rend = GetComponent<Renderer>();
        originColor = rend.material.color;
        SetHighlight(false);
        obstacle.enabled = false;
    }

    public void SetHighlight(bool on)
    {
        isHighlighted = on;
        warning.SetActive(on);
    }

    public void OpenPath(bool isBlocked)
    {
        if (obstacle != null)
        {
            obstacle.enabled = isBlocked;
        }
    }

    public void SpawnPawnAndAttack()
    {
        StartCoroutine(SpawnPawn());
    }

    private IEnumerator SpawnPawn()
    {
        GameObject pawn = Instantiate(pawnPrefab, (transform.position + Vector3.up * 20),Quaternion.identity);
        
        while (pawn.transform.position.y > transform.position.y)
        {
            pawn.transform.position += Vector3.down * 50 * Time.deltaTime;
            yield return null;
        }

        Collider[] target = Physics.OverlapBox(transform.position, new Vector3(3.5f, 2f, 3.5f),Quaternion.identity,LayerMask.GetMask("Player"));
    
        foreach (Collider collider in target)
        {
            CharacterModel model = collider.GetComponent<CharacterModel>();

            if (model != null)
                model.Damaged(0.3f, true);
        }

        Destroy(pawn);
    }

    public void ActiveBishopAttack()
    {
        isBishopActive = true;
        StartCoroutine(BishopAttack());
    }

    public void UnActiveBishop()
    {
        isBishopActive = false;
    }

    private IEnumerator BishopAttack()
    {

        GameObject effect = Instantiate(bishopEffect, transform.position + Vector3.up * 0.5f, Quaternion.identity);

        while (isBishopActive)
        {
            yield return null;
        }

        Destroy(effect);

    }

    public void ResetTile()
    {
        // 1. 코루틴 정지 (떨어지던 폰, 켜져 있던 비숍 이펙트 중지)
        StopAllCoroutines();

        // 2. 이 타일에서 생성된 모든 자식 이펙트 삭제 (비숍 이펙트, 폰 등)
        foreach (Transform child in transform)
        {
            // 경고판(warning)은 원래 있던 거니까 지우면 안 됩니다.
            if (child.gameObject != warning)
            {
                Destroy(child.gameObject);
            }
        }

        // 3. 변수 초기화
        isBishopActive = false;
        SetHighlight(false);
    }
}
