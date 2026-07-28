using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunDeBuff : BuffBase
{
    protected CharacterModel model;
     
    public StunDeBuff(CharacterModel model, BuffSO buffSO, float remainSecond) : base(buffSO, remainSecond)
    {
        this.model = model;
        buffType = EBuffType.Stun;
        this.remainSecond = remainSecond;
        
        return;
    }

    public override void OnEnable()
    {
        model.StunEnable();
    }

    public override void OnDisable()
    {
        model.StunDisable();
    }
}
