using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBuff : BuffBase
{
    protected CharacterModel model;
    public C_Enums.CharacterStat effectedStat;
    public bool isPercent;
    public float value;

    public StatBuff(CharacterModel model,BuffSO buffSO, float remainSecond, 
        C_Enums.CharacterStat effectedStat, bool isPercent, float value) : base(buffSO, remainSecond)
    {
        this.model = model;
        buffType = EBuffType.StatBuff;
        this.remainSecond = remainSecond;

        this.effectedStat = effectedStat;
        this.isPercent = isPercent;
        this.value = value;

        return;
    }

    public override void OnEnable()
    {
        model.AddStat(effectedStat, isPercent, value);
    }

    public override void OnDisable()
    {
        model.RemoveStat(effectedStat, isPercent, value * currentStack);
    }

    public override void Stack()
    {
        if (isStackable && currentStack < maxStack)
        {
            // 부모 클래스(BuffBase)의 원래 스택 로직(시간 초기화, 스택 증가 등)을 실행
            base.Stack();

            // ⭐️ 스택이 하나 쌓였으니, 1스택 분량의 스탯을 캐릭터에게 추가로 적용!
            model.AddStat(effectedStat, isPercent, value);
        }
    }

}
