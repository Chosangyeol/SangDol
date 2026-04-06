using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EBuffType
{
    StatBuff,
    Dot,
    Hot,
    Stun
}

public abstract class BuffBase
{
    public BuffSO buffSO;
    public float duration;
    public float remainSecond;
    public bool isActive;

    public EBuffType buffType;
    public string Desc { get; protected set; }

    public BuffBase(BuffSO buffSO, float remainSecond)
    {
        this.buffSO = buffSO;
        this.duration = remainSecond;
        this.remainSecond = remainSecond;
        Desc = buffSO.buffDesc;
        isActive = true;
        return;
    }

    public abstract void OnEnable();

    public virtual bool OnUpdate(float delta)
    {
        if (isActive)
            remainSecond -= delta;

        return (remainSecond <= 0f);
            
    }

    public abstract void OnDisable();


}
