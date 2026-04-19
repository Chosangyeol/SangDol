using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void StartGame()
    {
        StartCoroutine(StartGameRoutine());
    }


    private IEnumerator EnableServerSequence()
    {
        while (serverSelectUI.alpha < 1f)
        {
            serverSelectUI.alpha += Time.deltaTime * speed;
            touchText.alpha -= Time.deltaTime * speed;
            yield return null;
        }

        serverSelectUI.blocksRaycasts = true;
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

        serverSelectUI.blocksRaycasts = false;
        serverSelectUI.alpha = 0f;
        touchText.alpha = 1f;

    }

    private IEnumerator StartGameRoutine()
    {
        yield return SceneManager.LoadSceneAsync("Main", LoadSceneMode.Additive);

        // 2. Main 씬에 있는 SceneController를 찾습니다.
        // (싱글톤 Instance가 Awake에서 잡혔을 테니 바로 접근 가능합니다.)
        if (SceneChanger.instance != null)
        {
            // 3. SceneController에게 첫 필드 로딩을 시킵니다.
            // 이때 SceneController 내부 로직에 의해 '로딩 화면'이 즉시 켜집니다.
            SceneChanger.instance.LoadScene("DungeonTest");
        }

        // 4. 이제 필요 없어진 Title 씬은 언로드합니다.
        SceneManager.UnloadSceneAsync("Title");
    }
}
