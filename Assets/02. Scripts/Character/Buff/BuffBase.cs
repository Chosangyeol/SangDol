using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EBuffType
{
    StatBuff,
    Dot,
    Hot,
    Stun,
    Panic
}

public abstract class BuffBase
{
    public BuffSO buffSO;
    public float duration;
    public float remainSecond;
    public bool isStackable;
    public int maxStack;
    public int currentStack;
    public bool isActive;

    public EBuffType buffType;
    public string Desc { get; protected set; }

    public BuffBase(BuffSO buffSO, float remainSecond)
    {
        this.buffSO = buffSO;
        this.duration = remainSecond;
        this.remainSecond = remainSecond;
        this.isStackable = this.buffSO.isStackable;
        this.maxStack = this.buffSO.maxStack;
        this.currentStack = 1;
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

    public virtual void Stack()
    {
        if (isStackable && currentStack < maxStack)
        {
            currentStack++;
            remainSecond = duration;
        }
    }
}
