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

    public bool isHighlighted = false;

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
}
