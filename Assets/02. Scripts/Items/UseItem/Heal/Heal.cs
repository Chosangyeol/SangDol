using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : UseItemBase
{
    public Heal(UseItemSO itemBaseSO, int currentStack) : base(itemBaseSO, currentStack)
    {

    }

    public override ItemBase Clone(int stack)
    {
        return new Heal(this.itemBaseSO, stack);
    }

    public override bool UseItem(CharacterModel owner)
    {
        if (owner == null) return false;

        if (itemBaseSO.isImmediately)
        {
            if (itemBaseSO.isPercent)
                owner.Heal(owner.Stat.Stat.maxHp.FinalValue * itemBaseSO.effectAmount);
            else
                owner.Heal(itemBaseSO.effectAmount);

            return true;
        }
        else
        {
            HealBuff buff = new HealBuff(owner, itemBaseSO.buffSO, itemBaseSO.itemDuration,
                itemBaseSO.isPercent, itemBaseSO.effectAmount);

            SBuff sBuff = new SBuff
                (
                    owner.gameObject,
                    owner.gameObject,
                    buff
                );

            owner.Buff.AddBuff(sBuff);
        }
        return false;
    }
}
