using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class DungeonCompletionReturnToTitle : MonoBehaviour
{
    [Header("던전 완료 연결")]
    [Tooltip("비워두면 이 오브젝트와 같은 씬의 DungeonManager를 자동 연결합니다.")]
    [SerializeField] private DungeonManager dungeonManager;

    [Header("개발 진도 종료 화면")]
    [Tooltip("게임 시작 화면의 씬 이름입니다. Build Settings에 등록되어 있어야 합니다.")]
    [SerializeField] private string titleSceneName = "Title";
    [Tooltip("보스 사망 및 섹터 연출이 끝난 뒤 대기하는 실제 시간(초)입니다.")]
    [SerializeField, Min(0f)] private float returnDelay = 5f;

    private DungeonManager subscribedDungeon;
    private CharacterModel lockedPlayer;
    private Coroutine returnRoutine;
    private bool departureClaimed;
    private bool sceneLoadStarted;

    private void OnEnable() => BindDungeon();
    private void Start() => BindDungeon();

    private void BindDungeon()
    {
        if (subscribedDungeon != null) return;
        var source = dungeonManager;
        if (source == null && gameObject.scene.IsValid())
        {
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                source = root.GetComponentInChildren<DungeonManager>(true);
                if (source != null) break;
            }
        }
        if (source == null) return;
        subscribedDungeon = source;
        source.CompletionRequested += TryHandleCompletion;
    }

    private bool TryHandleCompletion(DungeonManager source)
    {
        if (!isActiveAndEnabled || source != subscribedDungeon) return false;
        if (departureClaimed) return true;
        if (string.IsNullOrWhiteSpace(titleSceneName) || !Application.CanStreamedLevelBeLoaded(titleSceneName))
        {
            Debug.LogWarning("[DungeonCompletionReturnToTitle] 시작 화면 씬을 로드할 수 없습니다. 기존 던전 복귀를 사용합니다.", this);
            return false;
        }

        departureClaimed = true;
        var player = FindFirstObjectByType<CharacterModel>();
        if (player != null && !player.isDie && !player.IsExternalControlLocked)
        {
            lockedPlayer = player;
            player.ControlDisable();
        }
        returnRoutine = StartCoroutine(ReturnToTitle(titleSceneName));
        return true;
    }

    private IEnumerator ReturnToTitle(string sceneName)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, returnDelay));
        sceneLoadStarted = true;
        returnRoutine = null;
        // Once departure commits, moving/removing this optional object must not
        // interrupt cleanup between destroying the pool and loading the menu.
        subscribedDungeon.StartCoroutine(LoadTitleScene(sceneName));
    }

    private static IEnumerator LoadTitleScene(string sceneName)
    {
        Time.timeScale = 1f;

        // Main is still alive while pooled boss/UI cleanup callbacks run.
        if (PoolManager.Instance != null) Destroy(PoolManager.Instance.gameObject);
        yield return null;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }

    private void OnDisable()
    {
        var source = subscribedDungeon;
        if (source != null) source.CompletionRequested -= TryHandleCompletion;
        subscribedDungeon = null;
        if (returnRoutine != null) StopCoroutine(returnRoutine);
        returnRoutine = null;

        if (departureClaimed && !sceneLoadStarted)
        {
            if (lockedPlayer != null && !lockedPlayer.isDie) lockedPlayer.ControlEnable();
            if (source != null) source.ResumeDefaultDeparture();
        }
        lockedPlayer = null;
        departureClaimed = false;
    }
}
