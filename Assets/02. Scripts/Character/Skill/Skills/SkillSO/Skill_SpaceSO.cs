using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Space", menuName = "Skills/Skill_SpaceSO")]

public class Skill_SpaceSO : SkillBaseSO
{
    public override SkillBase SkillInit(CharacterModel model)
    {
        return new Skill_Space(model, this);
    }
}
