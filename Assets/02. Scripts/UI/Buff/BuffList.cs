using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffList : MonoBehaviour
{
    [SerializeField] BuffSlot slotPrefab;
    [SerializeField] Transform slotParent;

    private CharacterModel _model;
    private C_Buff _buff;
    private List<BuffSlot> slots = new List<BuffSlot>();

    public void Init(C_Buff buff, CharacterModel model)
    {
        _model = model;
        _buff = buff;

        BindBuffEvent();
        RefreshBuffUI();
    }

    public void BindBuffEvent()
    {
        _buff.ActionAfterAddBuff += (buff) => RefreshBuffUI();
        _buff.ActionAfterRemoveBuff += (buff) => RefreshBuffUI();
    }

    public void RefreshBuffUI()
    {
        var activeBuffs = _buff.ListBuff;
        int buffCount = activeBuffs.Count;

        while (slots.Count < buffCount)
        {
            BuffSlot newSlot = Instantiate(slotPrefab, slotParent);
            slots.Add(newSlot);
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < buffCount)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Init(activeBuffs[i]);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    

}
