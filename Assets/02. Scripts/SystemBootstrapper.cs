using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemBootstrapper
{
    // 게임이 처음 켜질 때 '이벤트 구독'만 해둡니다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Initialize()
    {
        // 씬이 로드될 때마다 아래 OnSceneLoaded 함수를 실행하라고 예약합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 첫 번째 씬(현재 씬)에 대해서도 한 번 체크 실행
        CheckAndSpawn(SceneManager.GetActiveScene().name);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckAndSpawn(scene.name);
    }

    private static void CheckAndSpawn(string sceneName)
    {
        // 타이틀이면 생성을 안 함
        if (sceneName == "Title") return;

        Debug.Log($"현재 씬 [{sceneName}]: 시스템 자동 생성 체크 시작");

        // 1. UIManager 확인 및 생성
        if (UIManager.Instance == null)
        {
            GameObject uiPrefab = Resources.Load<GameObject>("UIManager");
            if (uiPrefab != null) Object.Instantiate(uiPrefab);
        }

        // 2. Player 및 CamContainer 확인 및 생성
        if (Object.FindAnyObjectByType<CharacterModel>() == null)
        {
            GameObject playerPrefab = Resources.Load<GameObject>("Player");
            GameObject camPrefab = Resources.Load<GameObject>("CamContainer");

            if (playerPrefab != null)
            {
                // 스폰 위치 확인
                GameObject spawnPos = GameObject.FindGameObjectWithTag("PlayerRevivePos");
                Vector3 pos = spawnPos != null ? spawnPos.transform.position : Vector3.zero;
                Quaternion rot = spawnPos != null ? spawnPos.transform.rotation : Quaternion.identity;

                // 플레이어 생성
                GameObject pInst = Object.Instantiate(playerPrefab, pos, rot);

                // 카메라 세트가 씬에 없다면 함께 생성
                if (GameObject.FindGameObjectWithTag("CamContainer") == null && camPrefab != null)
                {
                    GameObject cInst = Object.Instantiate(camPrefab);

                    // 플레이어 스크립트에 카메라 연결 (필요한 경우)
                    var model = pInst.GetComponent<CharacterModel>();
                    model.camContainer = cInst;
                }
            }
        }
    }
}