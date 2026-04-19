using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpObject : InteractableObject
{
    [Header("목적지")]
    public Transform targetPos;
    public AnimationCurve jumpCurve;

    protected override void Start()
    {
        base.Start();
        Init($"G - 이동");
    }

    public override bool Interact(Transform target)
    {
        if (!base.Interact(target)) return false;

        UpdateUIState();

        CharacterModel model = target.GetComponent<CharacterModel>();

        model.Navmesh.enabled = false;

        model.canAttack = false;
        model.canMove = false;
        model.canSkill = false;
        model.canUse = false;

        model.transform.LookAt(targetPos);

        StartCoroutine(JumpSequence(model));

        return true;
    }

    IEnumerator JumpSequence(CharacterModel model)
    { 
        Vector3 startPos = model.transform.position;
        Vector3 endPos = targetPos.position;

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime;

            float t = time / 1f;

            float curveT = jumpCurve.Evaluate(t);

            // 3. 직선 위치 계산 (Base Position)
            Vector3 basePos = Vector3.Lerp(startPos, endPos, curveT);

            // ⭐️ 4. 포물선 높이 계산 (Sine 함수 사용)
            // t가 0일때 0, 0.5(중간)일때 최고점(1 * Height), 1일때 다시 0이 됩니다.
            float arc = Mathf.Sin(t * Mathf.PI) * 25f;

            // 5. 직선 위치에 포물선 높이(Y축)를 더해서 최종 위치 적용!
            model.transform.position = basePos + new Vector3(0f, arc, 0f);

            yield return null;
        }

        model.EndJump();
    }
}
