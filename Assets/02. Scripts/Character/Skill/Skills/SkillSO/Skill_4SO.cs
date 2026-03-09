using UnityEngine;

[CreateAssetMenu(fileName = "Skill_4", menuName = "Skills/Skill_4SO")]
public class Skill_4SO : SkillBaseSO
{
    public override SkillBase SkillInit(CharacterModel model)
    {
        return new Skill_4(model, this);
    }
}
