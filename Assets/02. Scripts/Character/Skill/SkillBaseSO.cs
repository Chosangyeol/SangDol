using UnityEngine;

public enum EAttackShape
{
    Sphere,
    Box
}

public abstract class SkillBaseSO : ScriptableObject
{
    [Header("스킬 세팅")]
    public int skillID;
    public string skillName;
    public string skillDesc;
    public Sprite skillIcon;
    public int requireLevel;
    public int maxLevel;
    public float skillCool;
    public bool hasDamage = true;
    public float[] damageMultipliers;
    public bool canMoveSkill = false;

    [Header("차징 스킬 전용 세팅")]
    public bool isChargeSkill = false;
    public bool isHoldingSkill = false;
    public float maxChargeTime = 2.0f;
    [Tooltip("차지 단계별 데미지 비율")]
    public float[] chargeDamageMultipliers;
    [Tooltip("차지 단계 기준 시간")]
    public float[] chargeStageTimes;
    [Tooltip("차징 단계별 타격/이펙트 크기 배율 (예: 1.0, 1.5, 2.0)")]
    public float[] chargeScaleMultipliers;

    [Header("퍼펙트 존(Perfect Zone) 세팅")]
    public bool hasPerfectZone = false;           // 이 스킬에 퍼펙트 존이 있는가?
    public float perfectZoneStart = 1.0f;         // 퍼펙트 존 시작 타이밍 (예: 1.0초)
    public float perfectZoneEnd = 1.2f;           // 퍼펙트 존 종료 타이밍 (예: 1.2초)

    [Header("스킬 타격 공통 세팅")]
    public EAttackShape attackShape = EAttackShape.Sphere;
    public float forwardOffset = 1.5f;
    public float knockDownPower = 4f;
    public float idenGain = 1.0f;

    [Header("원형 전용")]
    public float attackRadius = 2f;

    [Header("사각형 전용")]
    public Vector3 boxSize = new Vector3(2f, 2f, 4f);

    public PoolableMono[] skillEffects;
    
    public abstract SkillBase SkillInit(CharacterModel model);
}