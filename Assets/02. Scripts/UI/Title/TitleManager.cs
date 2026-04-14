using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    public static TitleManager instance;

    [Header("타이틀 UI")]
    public CanvasGroup serverSelectUI;
    public CanvasGroup touchText;
    public float speed = 1f;

    private void Awake()
    {
        instance = this;
        touchText.alpha = 1f;
        serverSelectUI.alpha = 0f;
    }

    public void EnableServerSelect()
    {
        StartCoroutine(EnableServerSequence());
    }

    public void DisableServerSelect()
    {
        StartCoroutine(DisableServerSequence());
    }

    private IEnumerator EnableServerSequence()
    {
        while (serverSelectUI.alpha < 1f)
        {
            serverSelectUI.alpha += Time.deltaTime * speed;
            touchText.alpha -= Time.deltaTime * speed;
            yield return null;
        }

        serverSelectUI.alpha = 1f;
        touchText.alpha = 0f;
    }

    private IEnumerator DisableServerSequence()
    {
        while (serverSelectUI.alpha > 0f)
        {
            serverSelectUI.alpha -= Time.deltaTime * speed;
            touchText.alpha += Time.deltaTime * speed;
            yield return null;
        }

        serverSelectUI.alpha = 0f;
        touchText.alpha = 1f;

    }
}
