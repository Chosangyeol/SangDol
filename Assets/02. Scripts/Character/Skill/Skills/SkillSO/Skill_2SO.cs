using UnityEngine;

[CreateAssetMenu(fileName = "Skill_2", menuName = "Skills/Skill_2SO")]
public class Skill_2SO : SkillBaseSO
{
    public override SkillBase SkillInit(CharacterModel model)
    {
        return new Skill_2(model, this);
    }
}
