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

        GameObject effect = Instantiate(bishopEffect, transform.position + Vector3.up * 2, Quaternion.identity);

        while (isBishopActive)
        {
            yield return null;
        }

        Destroy(effect);

    }
}
