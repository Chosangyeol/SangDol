using UnityEngine;
using UnityEngine.SceneManagement;

public class SystemBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        string activeSceneName = SceneManager.GetActiveScene().name;

        // 2. Title 씬에서 시작한 거라면 아무것도 하지 않습니다.
        // (TitleManager가 나중에 직접 Main을 부를 것이기 때문)
        if (activeSceneName == "Title")
        {
            return;
        }

        // 3. Title이 아닌 맵(Field, Dungeon 등)에서 단독 실행했을 때만 Main을 붙여줍니다.
        if (!SceneManager.GetSceneByName("Main").isLoaded)
        {
            // [참고] 에디터 환경에서 개발 편의를 위한 것이므로 보통 로딩 없이 즉시 부릅니다.
            SceneManager.LoadScene("Main", LoadSceneMode.Additive);
        }
    }
}