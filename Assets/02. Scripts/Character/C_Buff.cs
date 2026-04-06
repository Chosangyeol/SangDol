using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_Buff
{
    private CharacterModel _model;

    private List<SBuff> _listBuff;
    public List<SBuff> ListBuff => _listBuff;

    public delegate void BuffHandler(ref SBuff buff);

    public event BuffHandler ActionBeforeAddBuff;
    public event Action<SBuff> ActionAfterAddBuff;
    public event BuffHandler ActionBeforeRemoveBuff;
    public event Action<SBuff> ActionAfterRemoveBuff;

    public C_Buff(CharacterModel model)
    {
        _model = model;
        _listBuff = new List<SBuff>();
        return;
    }

    public void AddBuff(SBuff buff)
    {
        ActionBeforeAddBuff?.Invoke(ref buff);
        buff.act.OnEnable();
        _listBuff.Add(buff);
        ActionAfterAddBuff?.Invoke(buff);
        return;
    }

    public bool UpdateBuff(float delta)
    {
        bool result = false;

        for (int i = _listBuff.Count - 1; i >= 0; i--)
        {
            SBuff buff = _listBuff[i];
            bool isEnd = buff.act.OnUpdate(delta);

            if (isEnd)
            {
                ActionBeforeRemoveBuff?.Invoke(ref buff);
                buff.act.OnDisable();
                _listBuff.RemoveAt(i);
                ActionAfterRemoveBuff?.Invoke(buff);
            }
        }
        return (result);
    }


}
