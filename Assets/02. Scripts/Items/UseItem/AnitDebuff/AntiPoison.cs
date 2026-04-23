using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiPoison : UseItemBase
{
    public AntiPoison(UseItemSO itemBaseSO, int currentStack) : base(itemBaseSO, currentStack)
    {

    }

    public override ItemBase Clone(int stack)
    {
        return new AntiPoison(this.itemBaseSO, stack);
    }

    public override bool UseItem(CharacterModel owner)
    {
        if (owner == null) return false;

        if (owner.Buff.RemoveBuff(EBuffType.Poison))
            return true;

        return false;
    }
}
