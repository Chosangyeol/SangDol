using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class C_SpecialStat
{
    private CharacterModel _model;

    public int _totalPoint
    {
        get
        {
            return _model.Stat.Stat.statPoint;
        }
        
    }

    public int _usedPoint { get; private set; }

    public int _remainPoint
    {
        get
        {
            return _totalPoint - _usedPoint;
        }
    }

    public  Dictionary<C_Enums.SpecialStat, int> _currentStatState { get; private set;  }

    public event Action OnChangeSpecialStat;

    public C_SpecialStat(CharacterModel model)
    {
        _model = model;
        _currentStatState = new Dictionary<C_Enums.SpecialStat, int>();

        foreach(C_Enums.SpecialStat stat in System.Enum.GetValues(typeof(C_Enums.SpecialStat)))
        {
            _currentStatState[stat] = 0;
        }
        return;
    }

    public bool TryInvestPont(C_Enums.SpecialStat stat)
    {
        if (_remainPoint <= 0) return false;

        _usedPoint++;
        _currentStatState[stat]++;

        ApplySpecialStat(stat);
        return true;
    }

    public void ApplySpecialStat(C_Enums.SpecialStat stat)
    {
        switch (stat)
        {
            // 공격력 증가
            case C_Enums.SpecialStat.S1:
                _model.AddStat(C_Enums.CharacterStat.AttackDamage, true, 0.03f);
                break;
            // 이동 / 공격속도 증가
            case C_Enums.SpecialStat.S2:
                _model.AddStat(C_Enums.CharacterStat.MoveSpeed, true, 0.1f);
                _model.AddStat(C_Enums.CharacterStat.AttackSpeed, true, 0.05f);
                break;
            // 무력화 피해량 증가
            case C_Enums.SpecialStat.S3:
                _model.AddStat(C_Enums.CharacterStat.DownPower, true, 0.05f);
                break;
            // 치명타 확률 / 치명타 피해 증가
            case C_Enums.SpecialStat.S4:
                _model.AddStat(C_Enums.CharacterStat.CriticalChance, false, 0.02f);
                _model.AddStat(C_Enums.CharacterStat.CriticalDamage, false, 0.05f);
                break;
            case C_Enums.SpecialStat.S5:
                _model.AddStat(C_Enums.CharacterStat.MaxHp, false, 0.05f);
                _model.AddStat(C_Enums.CharacterStat.DamageTakeMultiplier, false, 0.03f);
                break;
        }
    }

    public void ResetSpecialStat()
    {
        if (_usedPoint <= 0) return;

        _usedPoint = 0;

        _model.RemoveStat(C_Enums.CharacterStat.AttackDamage, false, 0.03f * _currentStatState[C_Enums.SpecialStat.S1]);

        _model.RemoveStat(C_Enums.CharacterStat.MoveSpeed, true, 0.1f * _currentStatState[C_Enums.SpecialStat.S2]);
        _model.RemoveStat(C_Enums.CharacterStat.AttackSpeed, true, 0.05f * _currentStatState[C_Enums.SpecialStat.S2]);
    }
}
