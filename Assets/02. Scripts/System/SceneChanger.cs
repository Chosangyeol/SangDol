using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger instance;


    [Header("로딩 UI")]
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private UnityEngine.UI.Slider progressBar;

    [Header("플레이어")]
    public GameObject playerObject;
    public NavMeshAgent playerAgent;

    private string _currentScene = "";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        loadingCanvas.SetActive(false);
    }

    private void Start()
    {
        // 현재 로드된 씬들 중 Main이나 Title이 아닌 것을 찾아 _currentScene으로 설정
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name != "Main" && s.name != "Title")
            {
                _currentScene = s.name;
                break;
            }
        }
    }

    public void ChageScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void LoadScene(string sceneName, string spawnPointName = "Spawn_Default")
    {
        StartCoroutine(LoadSequence(sceneName, spawnPointName));
    }

    private IEnumerator LoadSequence(string nextScene, string spawnPointName)
    {
        if (playerObject != null)
        {
            // 콜라이더를 꺼서 이전 좌표의 포탈 충돌을 방지합니다.
            if (playerObject.TryGetComponent<Collider>(out var col)) col.enabled = false;
            if (playerAgent != null) playerAgent.enabled = false;
        }

        loadingCanvas.SetActive(true);

        // 3. 이전 씬 언로드
        if (!string.IsNullOrEmpty(_currentScene))
        {
            Scene sceneToUnload = SceneManager.GetSceneByName(_currentScene);
            if (sceneToUnload.IsValid() && sceneToUnload.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(_currentScene);
            }
        }

        // 4. 다음 씬 비동기 로드
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (progressBar != null) progressBar.value = op.progress;
            yield return null;
        }

        // 6. 씬 활성화 및 대기
        op.allowSceneActivation = true;
        yield return new WaitUntil(() => op.isDone);

        _currentScene = nextScene;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_currentScene));

        // 8. 플레이어 위치 세팅 (중요!)
        MovePlayerToSpawnPoint(spawnPointName);

        // 시네머신 워프 처리를 위해 한 프레임 대기하거나 수동 갱신 호출 필요
        // CinemachineBrain이 새 좌표를 인식할 시간을 줍니다.
        yield return new WaitForEndOfFrame();

        // 9. 물리 판정 재활성화 (좌표 이동이 완전히 끝난 후)
        if (playerObject != null)
        {
            if (playerObject.TryGetComponent<Collider>(out var col)) col.enabled = true;
            if (playerAgent != null) playerAgent.enabled = true;
        }

        RefreshCameraStack();

        DynamicGI.UpdateEnvironment();
        RenderSettings.skybox = RenderSettings.skybox;
        yield return new WaitForSeconds(0.5f);
        loadingCanvas.SetActive(false);
    }

    private void MovePlayerToSpawnPoint(string pointName)
    {
        SpawnPoint[] points = FindObjectsOfType<SpawnPoint>();
        SpawnPoint targetPoint = System.Array.Find(points, p => p.pointName == pointName);

        if (targetPoint != null && playerObject != null)
        {
            // 에이전트가 있으면 Warp 사용, 없으면 transform.position 사용
            if (playerAgent != null)
            {
                playerAgent.Warp(targetPoint.transform.position);
                playerObject.transform.rotation = targetPoint.transform.rotation;
            }
            else
            {
                playerObject.transform.position = targetPoint.transform.position;
                playerObject.transform.rotation = targetPoint.transform.rotation;
            }
        }
    }

    private void RefreshCameraStack()
    {
        // 1. Main 씬에 있는 Base Camera를 찾습니다. (직접 참조 변수가 있다면 그것 사용)
        Camera baseCam = Camera.main;
        if (baseCam == null) return;

        var cameraData = baseCam.GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Clear(); // 기존 스택 초기화 (안전용)

        // 2. 현재 씬들에 있는 모든 Overlay 카메라를 찾아 스택에 추가합니다.
        // (보통 맵 씬에 UI 전용 카메라를 Overlay 모드로 두었을 때)
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (var cam in allCameras)
        {
            var data = cam.GetUniversalAdditionalCameraData();
            if (data.renderType == CameraRenderType.Overlay)
            {
                cameraData.cameraStack.Add(cam);
                Debug.Log($"[CameraStack] {cam.name} 추가됨");
            }
        }
    }
}
