using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealBuff : BuffBase
{
    protected CharacterModel model;
    public bool isPercent;
    public float value;
    public float interval = 1f;

    private float timer = 1f;

    public HealBuff(CharacterModel model, BuffSO buffSO, float remainSecond
        , bool isPercent, float value, float inverval) : base(buffSO, remainSecond)
    {
        this.model = model;
        this.isPercent = isPercent;
        this.value = value;
        this.interval = inverval;

        return;
    }

    public override void OnEnable()
    {
        
    }

    public override bool OnUpdate(float delta)
    {
        if (isActive)
        {
            timer += delta;
            if (timer >= interval)
            {
                ApplyHeal();
                timer -= interval;
            }
        }

        return base.OnUpdate(delta);
    }

    private void ApplyHeal()
    {
        if (isPercent)
            model.Heal(model.Stat.Stat.maxHp.FinalValue * value);
        else
            model.Heal(value);
    }

    public override void OnDisable()
    {
        
    }
}
