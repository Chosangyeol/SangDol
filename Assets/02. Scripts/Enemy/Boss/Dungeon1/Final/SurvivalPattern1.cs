using System.Collections;
using UnityEngine;

public class SurvivalPattern1 : MonoBehaviour
{
    private BossModel _boss;

    public GameObject parent;

    [Header("패턴 설정")]
    public SpriteRenderer centerSymbol; // 가운데 나타날 문양의 SpriteRenderer
    public Sprite[] suitSprites;        // 4가지 문양 스프라이트 (0:스페이드, 1:하트, 2:다이아, 3:클로버)
    public GameObject[] safeZones;       // 4개의 카드(안전지대) 위치 (위 스프라이트 배열과 순서를 맞춰주세요)
    public Animator test;

    private Transform player;            // 플레이어의 위치

    [Header("타이머 및 설정 값")]
    public float fadeDuration = 1.0f;   // 서서히 나타나고 사라지는 시간 (초)
    public float patternDuration = 3.0f;// 문양이 완전히 보이고 유지되는 시간 (이 시간 안에 이동해야 함)
    public float safeRadius = 1.5f;     // 안전지대 인정 범위 (거리)

    private int currentPatternIndex = -1;

    void Start()
    {
        // 시작할 때 가운데 문양을 투명하게 만듭니다.
        Color c = centerSymbol.color;
        c.a = 0f;
        centerSymbol.color = c;
    }

    public void Init(BossModel boss)
    {
        _boss = boss;
        player = boss.Target.transform;
    }

    // 애니메이션이 종료될 때 이 함수를 호출해주세요 (Animation Event 등 활용)
    public void StartPattern()
    {
        StartCoroutine(ExecutePatternRoutine());
    }

    private IEnumerator ExecutePatternRoutine()
    {
        // 1. 랜덤 문양 선택
        currentPatternIndex = Random.Range(0, suitSprites.Length);
        centerSymbol.sprite = suitSprites[currentPatternIndex];

        // 2. 서서히 나타나기 (Fade In)
        yield return StartCoroutine(FadeRoutine(0f, 1f, fadeDuration));

        // 3. 플레이어가 이동할 수 있도록 대기
        yield return new WaitForSeconds(patternDuration);

        yield return new WaitForSeconds(1.5f);

        // 4. 생존 여부 판정 (문양이 사라지기 직전이나 시작할 때 판정)
        CheckSurvival();

        // 5. 서서히 사라지기 (Fade Out)
        yield return StartCoroutine(FadeRoutine(1f, 0f, fadeDuration));
    }

    // 투명도를 조절하는 코루틴
    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;
        Color color = centerSymbol.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            centerSymbol.color = color;
            yield return null; // 다음 프레임까지 대기
        }

        color.a = endAlpha;
        centerSymbol.color = color;
    }

    // 플레이어가 올바른 카드 위에 있는지 확인하는 함수
    private void CheckSurvival()
    {
        if (currentPatternIndex == -1) return;

        Collider[] target = Physics.OverlapBox(safeZones[currentPatternIndex].transform.position, new Vector3(3f, 1f, 4.5f), Quaternion.identity, LayerMask.GetMask("Player"));


        if (target.Length > 0)
        {
            Debug.Log("생존 성공! 정답 카드 위에 있습니다.");
            // 생존 처리 (이펙트 재생 등)
        }
        else
        {
            Debug.Log("생존 실패... 데미지를 입거나 사망합니다.");
            player.GetComponent<CharacterModel>().Damaged(2.0f, true);
        }

        StartCoroutine(EndSpecial());
    }

    IEnumerator EndSpecial()
    {
        float timer = 0f;
        yield return new WaitForSeconds(1.0f);

        while (timer < 1f)
        {
            timer += Time.deltaTime;
            parent.transform.position += transform.up  * -1 * Time.deltaTime;
            yield return null;
        }

        if (_boss is D1_FinalBoss d1Boss)
            d1Boss.EndSpecialPattern();

        yield return new WaitForSeconds(0.5f);

        Destroy(parent);
    }
}