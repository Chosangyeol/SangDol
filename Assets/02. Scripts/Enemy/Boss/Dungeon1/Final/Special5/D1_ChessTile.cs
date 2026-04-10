using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D1_ChessTile : MonoBehaviour
{
    public int gridX;
    public int gridY;

    public bool isHighlighted = false;

    private Renderer rend;
    private Color originColor;


    private void Awake()
    {
        rend = GetComponent<Renderer>();
        originColor = rend.material.color;
    }

    public void SetHighlight(bool on)
    {
        isHighlighted = on;
        rend.material.color = on ? Color.green : originColor;
    }
}
