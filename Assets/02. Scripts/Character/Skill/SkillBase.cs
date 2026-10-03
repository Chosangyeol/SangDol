using Unity.VisualScripting.FullSerializer.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillBase
{
    protected CharacterModel _model;
    public SkillBaseSO skillData;

    private int skillLevel;
    public int SkillLevel => skillLevel;

    public float coolTime;
    public float finalCoolTime;
    public float nowCoolTime;
    public bool isSelected;
    public bool canUse;

    public bool isCharging = false;
    public bool isPerfectCharge = false;
    public float currentChargeTime = 0f;
    public float currentHoldTime = 0f;

    protected PlayerAttackContainer _attackContainer;
    private bool managedExecution;
    private bool executionCancelled;
    private readonly List<Coroutine> executionRoutines = new List<Coroutine>();
    private readonly Dictionary<PoolableMono, Vector3> executionEffects = new Dictionary<PoolableMono, Vector3>();

    protected bool CanContinueExecution => !executionCancelled && !_model.isDie &&
        !_model.Buff.isStun && !_model.IsExternalControlLocked;

    protected void BeginExecution()
    {
        StopExecutionResources();
        managedExecution = true;
        executionCancelled = false;
        _model.SkillSystem.BeginSkillAction(this);
    }

    protected void StartExecutionRoutine(IEnumerator routine)
    {
        executionRoutines.Add(_model.StartCoroutine(routine));
    }

    protected PoolableMono TakeExecutionEffect(PoolableMono prefab)
    {
        if (!CanContinueExecution || prefab == null || PoolManager.Instance == null) return null;
        PoolableMono effect = PoolManager.Instance.Pop(prefab.name);
        if (effect != null) executionEffects[effect] = effect.transform.localScale;
        return effect;
    }

    protected PoolableMono GetExecutionEffectPrefab(int index)
        => skillData.skillEffects != null && index < skillData.skillEffects.Length ? skillData.skillEffects[index] : null;

    protected void ReturnExecutionEffect(PoolableMono effect)
    {
        if (effect == null || !executionEffects.TryGetValue(effect, out Vector3 scale)) return;
        executionEffects.Remove(effect); // Release ownership before returning; delayed cleanup becomes a no-op.
        effect.transform.SetParent(null);
        effect.transform.localScale = scale;
        if (PoolManager.Instance != null) PoolManager.Instance.Push(effect);
        else effect.gameObject.SetActive(false);
    }

    private void StopExecutionResources()
    {
        foreach (Coroutine routine in executionRoutines)
            if (routine != null && _model != null) _model.StopCoroutine(routine);
        executionRoutines.Clear();
        foreach (PoolableMono effect in new List<PoolableMono>(executionEffects.Keys))
            ReturnExecutionEffect(effect);
        executionEffects.Clear();
    }

    internal void InterruptExecution()
    {
        if (!managedExecution || executionCancelled) return;
        executionCancelled = true;
        bool wasCharging = isCharging;
        isCharging = false;
        isPerfectCharge = false;
        currentChargeTime = 0f;
        currentHoldTime = 0f;
        StopExecutionResources();
        if (_attackContainer != null && _attackContainer.currentSkill == this)
            _attackContainer.currentSkill = null;
        if (wasCharging) GameEvent.OnGaugeUpdate?.Invoke(false, "", 0f, -1f, -1f);
        if (_model.Anim != null)
        {
            string[] triggers = GetType().Name == "Skill_1" ? new[] { "Skill1" } :
                GetType().Name == "Skill_3" ? new[] { "Skill3_Charge", "Skill3_ChargeEnd" } :
                new[] { "Skill4_Spin", "Skill4_Perfect", "Skill4_End" };
            foreach (string trigger in triggers) _model.Anim.ResetTrigger(trigger);
        }
    }

    internal bool OwnsAnimationEvent(AnimationEvent animationEvent)
    {
        if (!managedExecution || executionCancelled || !CanContinueExecution || animationEvent == null) return false;
        AnimationClip clip = animationEvent.animatorClipInfo.clip;
        if (clip == null) return false;
        string name = clip.name.Replace("(Clone)", "");
        return name == GetType().Name || name.StartsWith(GetType().Name + "_");
    }

    public SkillBase(CharacterModel model, SkillBaseSO skillData)
    {
        _model = model;
        this.skillData = skillData;

        if (skillData.maxLevel == 1)
            skillLevel = 1;
        else
            skillLevel = 0;

        _attackContainer = _model.GetComponent<PlayerAttackContainer>();

        coolTime = skillData.skillCool;
        finalCoolTime = coolTime;
        isSelected = false;
        canUse = true;
        return;
    }

    public virtual bool UseSkill(Vector3 targetPos)
    {
        if (canUse)
        {
            if (_attackContainer != null)
            {
                _attackContainer.currentSkill = this;
            }

            finalCoolTime = coolTime * _model.Stat.Stat.cooldownReduction.FinalValue;
            nowCoolTime = finalCoolTime;
            canUse = false;

            _model.PlayerController.StopMove();
            _model.PlayerController.InterruptAttackForSkill();
            _model.PlayerController.FaceTo(targetPos);

            currentHoldTime = 0f;

            return true;
        }
        return false;
    }

    public virtual void ReleaseSkill(Vector3 targetPos) { }

    public float GetCurrentDamageMultiplier()
    {
        if (skillData == null) return 1f;

        // 예시: SO에 레벨별 데미지 배열(float[])이 있다고 가정
        // 인덱스는 0부터 시작하므로 Level - 1을 해줍니다.
        int levelIndex = Mathf.Clamp(SkillLevel - 1, 0, skillData.damageMultipliers.Length - 1);

        return skillData.damageMultipliers[levelIndex];
    }

    public float GetChargeMultiplier()
    {
        // 차징 스킬이 아니거나 데이터가 없으면 배율은 1배(그대로)
        if (!skillData.isChargeSkill || skillData.chargeStageTimes == null) return 1f;

        float multiplier = 1f;
        for (int i = 0; i < skillData.chargeStageTimes.Length; i++)
        {
            if (currentChargeTime >= skillData.chargeStageTimes[i])
            {
                multiplier = skillData.chargeDamageMultipliers[i];
            }
        }
        return multiplier;
    }

    public float GetCurrentScaleMultiplier()
    {
        // 차징 스킬이 아니거나 데이터가 세팅되지 않았으면 기본 1배
        if (!skillData.isChargeSkill || skillData.chargeStageTimes == null || skillData.chargeScaleMultipliers == null)
            return 1f;

        float multiplier = 1f; // 기본 1배 시작

        // 내가 모은 시간이 어느 단계(Stage)까지 도달했는지 검사
        for (int i = 0; i < skillData.chargeStageTimes.Length; i++)
        {
            if (currentChargeTime >= skillData.chargeStageTimes[i])
            {
                // 배열 길이를 넘어가는 에러 방지용 안전장치
                if (i < skillData.chargeScaleMultipliers.Length)
                {
                    multiplier = skillData.chargeScaleMultipliers[i];
                }
            }
        }
        return multiplier;
    }

    public void CheckPerfectCharge()
    {
        isPerfectCharge = false; // 초기화

        if (skillData == null || !skillData.hasPerfectZone) return;

        // 현재 모은 시간이 시작과 끝 시간 사이에 정확히 들어왔다면 성공!
        if (currentChargeTime >= skillData.perfectZoneStart &&
            currentChargeTime <= skillData.perfectZoneEnd)
        {
            isPerfectCharge = true;
        }
    }

    public virtual void UpdateSkill(float deltaTime)
    {
        if (managedExecution && !executionCancelled && !CanContinueExecution)
        {
            _model.SkillSystem.InterruptActiveSkill(!_model.isDie && !_model.IsExternalControlLocked);
            InterruptExecution();
        }
        if (isCharging)
        {
            // 1. 차징 시간 계산
            currentChargeTime += deltaTime;
            if (currentChargeTime >= skillData.maxChargeTime)
            {
                currentChargeTime = skillData.maxChargeTime;
                currentHoldTime += deltaTime;

                if (skillData.hasPerfectZone)
                {
                    Vector3 forceTarget = _model.transform.position + _model.transform.forward;

                    ReleaseSkill(forceTarget);
                    return;
                }

                // [수정됨] 1초를 초과하면 마우스 위치 계산 없이 그냥 정면으로 강제 발사!
                if (currentHoldTime >= 1.0f)
                {
                    // 캐릭터의 현재 위치 + 캐릭터가 바라보는 앞쪽(forward)
                    Vector3 forceTarget = _model.transform.position + _model.transform.forward;

                    ReleaseSkill(forceTarget);

                    isCharging = false;
                    currentHoldTime = 0f;
                    GameEvent.OnGaugeUpdate?.Invoke(false, "", 0f, -1f, -1f);
                }
            }

            // 2. 차징 중 마우스 방향으로 천천히 회전하기 (강제 발사가 안 되었을 때만 유지)
            if (isCharging && _model.mainCam != null && UnityEngine.InputSystem.Mouse.current != null)
            {
                Vector2 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                Ray ray = _model.mainCam.ScreenPointToRay(mouseScreenPos);
                Vector3 lookTarget = Vector3.zero;
                bool hasTarget = false;

                if (Physics.Raycast(ray, out RaycastHit hit, 200f, _model.groundLayer))
                {
                    lookTarget = hit.point;
                    hasTarget = true;
                }
                else
                {
                    Plane virtualGroundPlane = new Plane(Vector3.up, new Vector3(0, _model.transform.position.y, 0));
                    if (virtualGroundPlane.Raycast(ray, out float enterDistance))
                    {
                        lookTarget = ray.GetPoint(enterDistance);
                        hasTarget = true;
                    }
                }

                if (hasTarget)
                {
                    Vector3 dir = lookTarget - _model.transform.position;
                    dir.y = 0;

                    if (dir.sqrMagnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(dir);
                        float rotationSpeed = 1f; // 회전 속도
                        _model.transform.rotation = Quaternion.Slerp(_model.transform.rotation, targetRotation, deltaTime * rotationSpeed);
                    }
                }
            }

            // 3. UI 업데이트 로직
            if (isCharging)
            {
                float progress = currentChargeTime / skillData.maxChargeTime;

                // 2. 퍼펙트 존 비율 계산 (기본값은 -1f 로 설정해서 UI 숨기기)
                float pStartRatio = -1f;
                float pEndRatio = -1f;

                if (skillData.hasPerfectZone && skillData.maxChargeTime > 0f)
                {
                    // 시작 시간과 끝 시간을 전체 시간으로 나눠서 0.0 ~ 1.0 사이의 비율(%)로 만듭니다.
                    pStartRatio = skillData.perfectZoneStart / skillData.maxChargeTime;
                    pEndRatio = skillData.perfectZoneEnd / skillData.maxChargeTime;
                }

                // 3. 수정된 이벤트로 데이터 발사!
                GameEvent.OnGaugeUpdate?.Invoke(true, $"{currentChargeTime:F1}초 / {skillData.maxChargeTime:F1}초", progress, pStartRatio, pEndRatio);
            }
        }

        // 쿨타임 로직
        if (!canUse)
        {
            nowCoolTime -= deltaTime;
            if (nowCoolTime <= 0f)
            {
                nowCoolTime = 0f;
                canUse = true;
            }
        }
    }
  

    public virtual void ResetSkillCool()
    {
        nowCoolTime = 0f;
        canUse = true;
    }

    public virtual void LevelUpSkill()
    {
        if (skillLevel >= skillData.maxLevel) return;

        skillLevel += 1;
        Debug.Log("스킬 레벨 업");
    }

    public virtual void LevelDownSkill()
    {
        if (skillLevel <= 0) return;
        
        skillLevel -= 1;
        Debug.Log("스킬 레벨 다운");
    }
}
