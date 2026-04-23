using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PoisonDebuff : BuffBase
{
    protected CharacterModel model;

    public bool isPercent;
    public float damage;

    private float timer = 1f;

    public PoisonDebuff(CharacterModel model, BuffSO buffSO, float remainSecond
        , bool isPercent, float damage) : base(buffSO,remainSecond)
    {
        this.model = model;
        buffType = EBuffType.Poison;
        this.remainSecond = remainSecond;
        this.isPercent = isPercent;
        this.damage = damage;

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
            if (timer >= 1.0f)
            {
                ApplyDamage();
                timer -= 1.0f;
            }
        }


        return base.OnUpdate(delta);
    }

    private void ApplyDamage()
    {
        if (isPercent)
            model.Damaged(damage, true);
        else
            model.Damaged(damage, false);
    }

    public override void OnDisable()
    {
        
    }
}
