using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SBuff
{
    public GameObject Source { get; private set; }
    public GameObject Target { get; private set; }
    public BuffBase act;

    public SBuff(GameObject source, GameObject target, BuffBase act)
    {
        Source = source;
        Target = target;
        this.act = act;
        return ;
    }
}
