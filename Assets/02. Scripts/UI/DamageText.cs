using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public TextMeshPro textMesh; // World Space 텍스트

    [Header("애니메이션 설정")]
    public float floatSpeed = 2f; // 떠오르는 속도
    public float duration = 1.0f; // 유지 시간

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damage, bool isCritical, bool isPlayer =false)
    {
        textMesh.text = damage.ToString();

        // 크리티컬 여부에 따라 색상과 크기 변경
        if (isCritical)
        {
            textMesh.color = Color.yellow;
            textMesh.fontSize = 12f; // 크리티컬 폰트 크게
            // 여기에 '반짝!' 하는 DOTween 펀치 스케일을 넣으면 더 좋습니다.
        }
        else
        {
            textMesh.color = Color.white;
            textMesh.fontSize = 8f; // 일반 폰트
        }

        if (isPlayer)
        {
            textMesh.color = Color.red;
            textMesh.fontSize = 10f;
        }

        StartCoroutine(FloatingRoutine());
    }

    private void Update()
    {
        // ⭐️ 빌보드 효과: 텍스트가 항상 카메라를 정면으로 바라보게 만듭니다. (안 하면 글씨가 뒤집히거나 안 보임)
        transform.rotation = mainCamera.transform.rotation;
    }

    private IEnumerator FloatingRoutine()
    {
        float timer = 0f;
        Color startColor = textMesh.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // 1. 위로 떠오르기
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // 2. 투명하게 페이드아웃 (마지막 절반 시간 동안만 투명해지도록)
            if (timer > duration / 2f)
            {
                float fadeProgress = (timer - (duration / 2f)) / (duration / 2f);
                startColor.a = Mathf.Lerp(1f, 0f, fadeProgress);
                textMesh.color = startColor;
            }

            yield return null;
        }

        // 연출이 끝나면 매니저에게 나를 다시 창고(풀)로 돌려보내달라고 요청
        DamageTextManager.Instance.ReturnToPool(this.gameObject);
    }
}
