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

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, _model.groundLayer))
        {
            point = hit.point;
            return true;
        }
        return false;
    }
}
