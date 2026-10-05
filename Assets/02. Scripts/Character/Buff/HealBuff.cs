using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealBuff : BuffBase
{
    protected CharacterModel model;
    public bool isPercent;
    public float value;
    public float interval = 1f;

    private float timer;
    private int completedTicks;

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
            float step = Mathf.Max(.001f, interval);
            int tickLimit = isInfinite ? int.MaxValue : Mathf.Max(1, Mathf.CeilToInt(duration / step));
            float elapsed = Mathf.Max(0f, delta);
            if (!isInfinite) elapsed = Mathf.Min(elapsed, Mathf.Max(0f, remainSecond));
            timer += elapsed;
            while (timer >= step && completedTicks < tickLimit)
            {
                ApplyHeal();
                completedTicks++;
                timer -= step;
            }
            // A finite potion also pays its final partial interval once.
            if (!isInfinite && delta >= remainSecond && timer > 0f && completedTicks < tickLimit)
            { ApplyHeal(); completedTicks++; timer = 0f; }
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
