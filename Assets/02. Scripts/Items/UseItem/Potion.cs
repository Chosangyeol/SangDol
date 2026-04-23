using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : UseItemBase
{
    public Potion(UseItemSO itemBaseSO, int currentStack) : base(itemBaseSO, currentStack)
    {
       
    }

    public override ItemBase Clone(int stack)
    {
        return new Potion(this.itemBaseSO, stack);
    }

    public override bool UseItem(CharacterModel owner)
    {
        if (owner == null) return false;



        return false;
    }
}
