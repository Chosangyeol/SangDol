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
        // 1. 플레이어 조작 금지
        // CharacterModel.instance.canMove = false; 

        // 2. 로딩 UI 켜기
        loadingCanvas.SetActive(true);

        // 3. 이전 씬 언로드 (타이틀에서 처음 넘어가는 게 아니라면)
        if (!string.IsNullOrEmpty(_currentScene))
        {
            yield return SceneManager.UnloadSceneAsync(_currentScene);
            // [중요] 이전 맵에서 쓴 몬스터 풀링 클리어
            // PoolManager.Instance.ClearPool(); 
        }

        // 4. 다음 씬 비동기 로드 (Additive 모드!)
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Additive);
        op.allowSceneActivation = false; // 90%에서 멈춰둠

        while (op.progress < 0.9f)
        {
            if (progressBar != null) progressBar.value = op.progress;
            yield return null;
        }

        // 5. [로아 핵심] 로딩 중에 몬스터 미리 풀링하기 (Pre-Pooling)
        // 이 시점에 프리팹들을 미리 Instantiate 해서 메모리에 올려둡니다.
        // yield return StartCoroutine(PrePoolingRoutine(nextScene));

        // 6. 씬 활성화
        op.allowSceneActivation = true;
        yield return new WaitUntil(() => op.isDone);    
        // 7. 활성화된 씬을 ActiveScene으로 설정 (내비메쉬, 라이팅 등을 위해)

        _currentScene = nextScene;

        // 8. 플레이어 위치 세팅 및 로딩 UI 끄기
        // DungeonManager.instance.OnSceneLoaded();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(_currentScene));

        MovePlayerToSpawnPoint(spawnPointName);
        RefreshCameraStack();

        DynamicGI.UpdateEnvironment();
        RenderSettings.skybox = RenderSettings.skybox;
        yield return new WaitForSeconds(0.5f); // 연출용 대기
        loadingCanvas.SetActive(false);

        // CharacterModel.instance.canMove = true;
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
