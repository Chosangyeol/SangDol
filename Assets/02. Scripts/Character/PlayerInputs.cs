using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;

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
    private readonly Queue<SkillInput> skillInputs = new Queue<SkillInput>();
    private struct SkillInput
    {
        public C_Enums.SkillSlot slot;
        public Vector2 position;
        public bool released;
    }

    private void Awake()
    {
        if (model == null) model = GetComponent<CharacterModel>();
    }

    private void Update()
    {
        if (model == null || model.isDie)
        {
            skillInputs.Clear();
            return;
        }

        bool isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (model != null && model.PlayerInput != null)
        {
            while (skillInputs.Count > 0)
            {
                SkillInput input = skillInputs.Dequeue();
                if (input.released)
                    model.PlayerInput.OnSkillKeyUp(input.slot, input.position);
                else if (!isPointerOverUI)
                    model.PlayerInput.OnSkillKeyDown(input.slot, input.position);
            }
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

            if (isMoveHeld && !isPointerOverUI)
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
        isMoveHeld = false;
        isAttackHeld = false;
        skillInputs.Clear();
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
        skillInputs.Enqueue(new SkillInput { slot = GetSkillSlotFromInput(ctx), position = GetPointerScreenPos() });
    }

    // 스킬 버튼에서 손을 뗐을 때 (차징 종료 및 발사)
    private void OnSkillSlotCanceled(InputAction.CallbackContext ctx)
    {
        skillInputs.Enqueue(new SkillInput { slot = GetSkillSlotFromInput(ctx), position = GetPointerScreenPos(), released = true });
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
