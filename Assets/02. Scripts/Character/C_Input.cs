using UnityEngine;
using static C_Enums;

public class C_Input
{
    private readonly CharacterModel _model;
    private readonly C_Controller _controller;

    public C_Input(CharacterModel model,C_Controller controller)
    {
        _model = model;
        _controller = controller;
        return;
    }

    public void OnMoveClick(Vector2 screenPos)
    {
        if (!GetMouseInput(screenPos, out var point))
            return;

        _controller.RequestMove(point);
    }

    public void OnAttackClick(bool isHeld, Vector2 screenPos)
    {
        Vector3 point = Vector3.zero;

        if (isHeld)
        {
            if (!GetMouseInput(screenPos, out point)) return;
        }

        _controller.RequestBasicAttack(isHeld,point);
    }

    public void OnInteract()
    {
        _controller.RequestInteract();
    }


    public void OnSkillInput(C_Enums.SkillSlot skillSlot, Vector2 screenPos)
    {
        if (!GetMouseInput(screenPos, out var point))
            return;

        _controller.RequsetSkill(skillSlot, point);
    }

    public void OnUseItemInput(C_Enums.UseSlot useSlot)
    {
        _controller.RequestUseItem(useSlot);
    }
    public void OnUIInput(C_Enums.UIList ui)
    {
        _controller.RequestUI(ui);
    }


    public bool GetMouseInput(Vector2 screenPos, out Vector3 point)
    {
        point = default;
        if (_model == null || _model.mainCam == null)
            return false;

        Ray ray = _model.mainCam.ScreenPointToRay(screenPos);

        // 1. 먼저 실제 땅(groundLayer)이나 적중할 만한 오브젝트에 맞는지 검사 (기존 방식 유지)
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, _model.groundLayer))
        {
            point = hit.point;
            return true;
        }

        // 2. 만약 허공을 클릭했다면? 가상의 평면(Plane)을 만들어 교차점을 찾음
        // 캐릭터의 Y축 높이를 지나는 무한한 평면 생성 (보통 높이 0 또는 캐릭터 발밑)
        float characterY = _model.transform.position.y;
        Plane virtualGroundPlane = new Plane(Vector3.up, new Vector3(0, characterY, 0));

        // 광선이 이 가상 평면과 만나는 거리를 계산
        if (virtualGroundPlane.Raycast(ray, out float enterDistance))
        {
            // 거리를 바탕으로 교차점(포인트) 획득
            point = ray.GetPoint(enterDistance);
            return true;
        }

        return false; // 카메라가 하늘을 보고 있어서 바닥을 아예 안 쏠 때만 false
    }
}
