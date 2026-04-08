using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffList : MonoBehaviour
{
    [SerializeField] BuffSlot slotPrefab;
    [SerializeField] Transform buffSlotParent;
    [SerializeField] Transform deBuffSlotParent;

    private CharacterModel _model;
    private C_Buff _buff;

    private List<BuffSlot> buffSlots = new List<BuffSlot>();
    private List<BuffSlot> debuffSlots = new List<BuffSlot>();

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

        // 1. 전체 리스트에서 버프와 디버프를 따로 분류합니다.
        // (주의: SBuff 구조체/클래스 안에 BuffSO 변수명이 다를 수 있으니 상황에 맞게 수정하세요. 예: buffSO, buffData 등)
        var pureBuffs = activeBuffs.Where(b => b.act.buffSO.isBuff).ToList();
        var pureDebuffs = activeBuffs.Where(b => !b.act.buffSO.isBuff).ToList();

        // 2. 각각 분리된 리스트와 부모를 넘겨서 슬롯을 갱신합니다.
        UpdateSlotGroup(pureBuffs, buffSlots, buffSlotParent);
        UpdateSlotGroup(pureDebuffs, debuffSlots, deBuffSlotParent);
    }

    // ⭐️ 중복되는 생성/갱신 코드를 하나의 함수로 깔끔하게 묶어줍니다.
    private void UpdateSlotGroup(List<SBuff> targetBuffs, List<BuffSlot> targetSlots, Transform parent)
    {
        int buffCount = targetBuffs.Count;

        // 슬롯이 부족하면 새로 생성해서 부모 위치에 넣습니다.
        while (targetSlots.Count < buffCount)
        {
            BuffSlot newSlot = Instantiate(slotPrefab, parent);
            targetSlots.Add(newSlot);
        }

        // 데이터 개수만큼 슬롯을 켜고 정보를 넣어줍니다.
        for (int i = 0; i < targetSlots.Count; i++)
        {
            if (i < buffCount)
            {
                targetSlots[i].gameObject.SetActive(true);
                targetSlots[i].Init(targetBuffs[i]);
            }
            else
            {
                // 남는 슬롯은 꺼둡니다 (오브젝트 풀링)
                targetSlots[i].gameObject.SetActive(false);
            }
        }
    }
}
