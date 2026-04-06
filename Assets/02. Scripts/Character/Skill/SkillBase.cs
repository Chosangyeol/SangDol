using UnityEngine;

public abstract class SkillBase
{
    protected CharacterModel _model;
    public SkillBaseSO skillData;

    private int skillLevel;
    public int SkillLevel => skillLevel;

    public float coolTime;
    public float nowCoolTime;
    public bool isSelected;
    public bool canUse;

    public SkillBase(CharacterModel model, SkillBaseSO skillData)
    {
        _model = model;
        this.skillData = skillData;

        if (skillData.maxLevel == 1)
            skillLevel = 1;
        else
            skillLevel = 0;

        this.coolTime = skillData.skillCool;
        isSelected = false;
        canUse = true;
        return;
    }

    public virtual bool UseSkill(Vector3 targetPos)
    {
        if (canUse)
        {
            nowCoolTime = coolTime;
            canUse = false;
            
            return true;
        }
        return false;
    }

    public virtual void UpdateSkill(float deltaTime)
    {
        if (canUse) return;

        nowCoolTime -= deltaTime;
        canUse = nowCoolTime <= 0f;
        return;
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
