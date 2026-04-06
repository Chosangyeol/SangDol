using UnityEngine;
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
        if (model != null && model.PlayerInput != null)
        {
            model.PlayerInput.OnAttackClick(isAttackHeld, GetPointerScreenPos());

            if (isMoveHeld)
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


        skillSlotAction.action.performed += OnSkillSlot;
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
        skillSlotAction.action.performed -= OnSkillSlot;
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

    private void OnSkillSlot(InputAction.CallbackContext ctx)
    {
        C_Enums.SkillSlot slot = GetSkillSlotFromInput(ctx);
        model.PlayerInput.OnSkillInput(slot, GetPointerScreenPos());
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
