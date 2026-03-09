using UnityEngine;

[CreateAssetMenu(fileName = "Skill_3", menuName = "Skills/Skill_3SO")]
public class Skill_3SO : SkillBaseSO
{
    public override SkillBase SkillInit(CharacterModel model)
    {
        return new Skill_3(model, this);
    }
}
