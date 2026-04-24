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

    public bool isStun = false;
    public bool isImmunity = false;
    public bool isInvincibility = false;
    public bool isPanic = false;

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

        // 1. 이미 적용 중인 버프 중에서 똑같은 BuffSO를 가진 버프가 있는지 찾습니다.
        int existingIndex = _listBuff.FindIndex(x => x.act.buffSO == buff.act.buffSO);

        if (existingIndex != -1) // 같은 버프를 찾았다면!
        {
            SBuff existingBuff = _listBuff[existingIndex];

            // 2. 그 버프가 스택 가능한(isStackable) 버프인지 확인합니다.
            if (existingBuff.act.isStackable)
            {
                // 기존 버프의 스택을 쌓고(시간 갱신 포함) 함수를 종료합니다. 
                // (새로 리스트에 추가하지 않음!)
                existingBuff.act.Stack();

                // 스택이 쌓였음을 UI 등에 알리기 위해 이벤트 호출
                ActionAfterAddBuff?.Invoke(existingBuff);
                return;
            }
        }

        // --- 여기부터는 [처음 걸리는 버프] 거나 [스택 불가능해서 따로 적용해야 하는 버프]일 때의 로직입니다 ---

        if (buff.act.buffSO.isBuff)
        {
            buff.act.OnEnable();
            _listBuff.Add(buff);
        }
        else // 디버프일 경우
        {
            if (!_model.Buff.isImmunity) // 면역이 아닐 때만 적용
            {
                if (existingIndex != -1)
                {
                    _listBuff[existingIndex].act.OnDisable();
                    _listBuff.RemoveAt(existingIndex);
                }

                buff.act.OnEnable();
                _listBuff.Add(buff);
            }
            else
            {
                // 면역이라서 디버프가 튕겨나갔다면, 추가 적용(ActionAfterAddBuff)도 안 하는 것이 맞습니다.
                return;
            }
        }

        ActionAfterAddBuff?.Invoke(buff);
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

    public void RemoveAllBuff()
    {
        for (int i = 0; i < _listBuff.Count; i++)
        {
            _listBuff[i].act.OnDisable();
            
        }
        _listBuff.Clear();
    }

    public bool RemoveBuff(BuffSO targetSO)
    {
        // FindIndex를 사용하면 for문을 돌리는 것보다 코드가 훨씬 짧고 직관적이게 됩니다.
        int index = _listBuff.FindIndex(x => x.act.buffSO == targetSO);

        if (index != -1) // 일치하는 BuffSO를 찾았다면!
        {
            SBuff buffToRemove = _listBuff[index];

            // 완벽하게 작성하신 생명주기(이벤트 -> 종료 -> 삭제 -> 이벤트)를 그대로 적용합니다.
            ActionBeforeRemoveBuff?.Invoke(ref buffToRemove);

            buffToRemove.act.OnDisable();
            _listBuff.RemoveAt(index);

            ActionAfterRemoveBuff?.Invoke(buffToRemove);

            return true;
        }

        // 지우려는 버프가 리스트에 없으면 false 반환
        return false;
    }

    public bool RemoveBuff(EBuffType targetType)
    {
        for (int i = 0; i < _listBuff.Count;i++)
        {
            SBuff buff = _listBuff[i];

            if (buff.act.buffType == targetType)
            {
                ActionBeforeRemoveBuff?.Invoke(ref buff);
                buff.act.OnDisable();
                _listBuff.RemoveAt(i);

                ActionAfterRemoveBuff?.Invoke(buff);
                return true;
            }
        }
        return false;
    }

    private bool IsDebuff(EBuffType type)
    {
        // 기획에 따라 해로운 효과에 해당하는 타입을 모두 나열해줍니다.
        return type == EBuffType.Dot || type == EBuffType.Stun || type == EBuffType.Panic || type == EBuffType.Poison;
    }

    #region 상태이상 부여

    public void StunEnable()
    {
        isStun = true;
    }

    public void StunDisable()
    {
        isStun = false;
    }

    public void ImmunityEnable()
    {
        isImmunity = true;
    }

    public void ImmunityDisable()
    {
        isImmunity = false;
    }

    public void InvincibilityEnable()
    {
        isInvincibility = true;
    }

    public void InvincibilityDisable()
    {
        isInvincibility = false;
    }



    public void PanicEnable()
    {
        isPanic = true;
    }

    public void PanicDisable()
    {
        isPanic = false;
    }

    #endregion
}
