using UnityEngine;

[CreateAssetMenu(fileName = "Skill_1", menuName = "Skills/Skill_1SO")]
public class Skill_1SO : SkillBaseSO
{
    public override SkillBase SkillInit(CharacterModel model)
    {
        return new Skill_1(model, this);
    }
}
