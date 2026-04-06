using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill_Z", menuName = "Skills/Skill_ZSO")]

public class Skill_ZSO : SkillBaseSO
{
    public BuffSO buffSO;

    public override SkillBase SkillInit(CharacterModel model)
    {

        return new Skill_Z(model, this);
    }
}
