using UnityEngine;

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

    public PoolableMono[] skillEffects;
    
    public abstract SkillBase SkillInit(CharacterModel model);
}