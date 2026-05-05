using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerInputs : MonoBehaviour
{
    [SerializeField] private CharacterModel model;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference interactActon;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference skillSlotAction;
    [SerializeField] private InputActionReference useItemSlotAction;
    [SerializeField] private InputActionReference uiAction;
    [SerializeField] private InputActionReference pointerPos;

    private bool isAttackHeld = false;
    private bool isMoveHeld = false;

    private void Awake()
    {
        if (model == null) model = GetComponent<CharacterModel>();
    }

    private void Update()
    {
        if (model.isDie) return;

        bool isPointerOverUI = EventSystem.current.IsPointerOverGameObject();

        if (model != null && model.PlayerInput != null)
        {
            // 🌟 수정된 공격 로직 🌟
            // UI 위가 아니거나, 혹은 마우스를 뗐을 때(!isAttackHeld)
            if (!isPointerOverUI || !isAttackHeld)
            {
                // 누를 때(true)는 canAttack이 true일 때만 전달!
                if (isAttackHeld && model.canAttack)
                {
                    model.PlayerInput.OnAttackClick(true, GetPointerScreenPos());
                }
                // 뗄 때(false)는 canAttack 무시하고 무조건 전달!
                else if (!isAttackHeld)
                {
                    model.PlayerInput.OnAttackClick(false, GetPointerScreenPos());
                }
            }

            // 🌟 수정된 이동 로직 (기존의 model.canAttack 조건에서 분리) 🌟
            if (isMoveHeld && !isPointerOverUI && model.canMove)
            {
                model.PlayerInput.OnMoveClick(GetPointerScreenPos());
            }
        }
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        interactActon.action.Enable();
        attackAction.action.Enable();
        skillSlotAction.action.Enable();
        useItemSlotAction.action.Enable();
        uiAction.action.Enable();
        pointerPos.action.Enable();

        moveAction.action.started += OnMoveStarted;
        moveAction.action.canceled += OnMoveCanceled;


        interactActon.action.performed += OnInteract;

        attackAction.action.started += OnAttackStarted;
        attackAction.action.canceled += OnAttackCanceled;


        skillSlotAction.action.started += OnSkillSlotStarted;
        skillSlotAction.action.canceled += OnSkillSlotCanceled;

        useItemSlotAction.action.performed += OnUseItemSlot;
        uiAction.action.performed += OnUIInput;
    }

    private void OnDisable()
    {
        moveAction.action.started -= OnMoveStarted;
        moveAction.action.canceled -= OnMoveCanceled;

        interactActon.action.performed -= OnInteract;
        attackAction.action.started -= OnAttackStarted;
        attackAction.action.canceled -= OnAttackCanceled;
        skillSlotAction.action.started -= OnSkillSlotStarted;
        skillSlotAction.action.canceled -= OnSkillSlotCanceled;
        useItemSlotAction.action.performed -= OnUseItemSlot;
        uiAction.action.performed -= OnUIInput;
    }

    private Vector2 GetPointerScreenPos()
        => pointerPos.action.ReadValue<Vector2>();

    private void OnMoveStarted(InputAction.CallbackContext ctx) => isMoveHeld = true;
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => isMoveHeld = false;

    private void OnInteract(InputAction.CallbackContext ctx)
        => model.PlayerInput.OnInteract();

    private void OnAttackStarted(InputAction.CallbackContext ctx) => isAttackHeld = true;
    private void OnAttackCanceled(InputAction.CallbackContext ctx) => isAttackHeld = false;

    private void OnSkillSlotStarted(InputAction.CallbackContext ctx)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return; // UI 위면 무시

        C_Enums.SkillSlot slot = GetSkillSlotFromInput(ctx);
        // OnSkillInput 대신 명확하게 '누름'을 전달합니다 (모델쪽 함수 이름도 맞춰서 변경 필요)
        model.PlayerInput.OnSkillKeyDown(slot, GetPointerScreenPos());
    }

    // 스킬 버튼에서 손을 뗐을 때 (차징 종료 및 발사)
    private void OnSkillSlotCanceled(InputAction.CallbackContext ctx)
    {
        C_Enums.SkillSlot slot = GetSkillSlotFromInput(ctx);
        // 손을 뗐다는 신호를 전달합니다.
        model.PlayerInput.OnSkillKeyUp(slot, GetPointerScreenPos());
    }

    private void OnUseItemSlot(InputAction.CallbackContext ctx)
    {
        C_Enums.UseSlot slot = GetUseSlotFromInput(ctx);
        model.PlayerInput.OnUseItemInput(slot);
    }
    private void OnUIInput(InputAction.CallbackContext ctx)
    {
        if (ctx.control is KeyControl key)
        {
            if (key.keyCode == Key.Escape) model.PlayerInput.OnUIInput(C_Enums.UIList.Option);
            if (key.keyCode == Key.I) model.PlayerInput.OnUIInput(C_Enums.UIList.Inventory);
            if (key.keyCode == Key.K) model.PlayerInput.OnUIInput(C_Enums.UIList.SkillTree);
            if (key.keyCode == Key.L) model.PlayerInput.OnUIInput(C_Enums.UIList.Quest);
            if (key.keyCode == Key.P) model.PlayerInput.OnUIInput(C_Enums.UIList.Status);
        }
    }

    private C_Enums.SkillSlot GetSkillSlotFromInput(InputAction.CallbackContext ctx)
    {
        if (ctx.control is KeyControl key)
        {
            if (key.keyCode == Key.Z) return C_Enums.SkillSlot.Z;
            if (key.keyCode == Key.Q) return C_Enums.SkillSlot.Q;
            if (key.keyCode == Key.W) return C_Enums.SkillSlot.W;
            if (key.keyCode == Key.E) return C_Enums.SkillSlot.E;
            if (key.keyCode == Key.R) return C_Enums.SkillSlot.R;
            if (key.keyCode == Key.Space) return C_Enums.SkillSlot.Space;
            if (key.keyCode == Key.V) return C_Enums.SkillSlot.V;
            //if (key.keyCode == Key.A) return C_Enums.SkillSlot.A;
            //if (key.keyCode == Key.S) return C_Enums.SkillSlot.S;
            //if (key.keyCode == Key.D) return C_Enums.SkillSlot.D;
            //if (key.keyCode == Key.F) return C_Enums.SkillSlot.F;
        }

        return C_Enums.SkillSlot.Q;
    }

    private C_Enums.UseSlot GetUseSlotFromInput(InputAction.CallbackContext ctx)
    {
        if (ctx.control is KeyControl key)
        {
            if (key.keyCode == Key.Digit1) return C_Enums.UseSlot.Slot_1;
            if (key.keyCode == Key.Digit2) return C_Enums.UseSlot.Slot_2;
            if (key.keyCode == Key.Digit3) return C_Enums.UseSlot.Slot_3;
            if (key.keyCode == Key.Digit4) return C_Enums.UseSlot.Slot_4;
        }
        return C_Enums.UseSlot.Slot_1;
    }
}
