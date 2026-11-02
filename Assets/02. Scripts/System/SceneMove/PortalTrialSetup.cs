using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

internal static class PortalTrialSetup
{
    private sealed class PotionGrantMarker { }
    private static readonly ConditionalWeakTable<CharacterModel, PotionGrantMarker> potionGrantMarkers = new();

    public static void Apply(CharacterModel player, int targetLevel)
    {
        if (player == null || player.Stat == null) return;

        player.Stat.RaiseToLevel(Mathf.Max(1, targetLevel));
        player.Heal(player.Stat.Stat.maxHp.FinalValue);
        GrantTrialPotions(player);

        if (UIManager.Instance == null) return;
        UIManager.Instance.InitGameUIs();

        if (UIManager.Instance.skillTreeUI != null &&
            !UIManager.Instance.skillTreeUI.ApplyPortalLoadout())
        {
            Debug.LogWarning("[PortalTrialSetup] QWER 포탈 스킬 자동 등록을 완료하지 못했습니다.");
        }
    }

    private static void GrantTrialPotions(CharacterModel player)
    {
        if (player == null || player.Inventory == null) return;

        UseItemSO potion = Resources.Load<UseItemSO>("ItemSOs/UseItems/20001_hpPotion_Basic");
        if (potion == null)
        {
            Debug.LogWarning("[PortalTrialSetup] 체력 포션 데이터를 찾지 못했습니다.");
            return;
        }

        if (!potionGrantMarkers.TryGetValue(player, out _))
        {
            var rewards = new List<KeyValuePair<ItemBaseSO, int>>
            {
                new KeyValuePair<ItemBaseSO, int>(potion, 10)
            };

            if (!player.Inventory.TryAddRewards(rewards,
                    () => potionGrantMarkers.Add(player, new PotionGrantMarker())))
            {
                Debug.LogWarning("[PortalTrialSetup] 인벤토리 공간이 부족해 체력 포션 10개를 지급하지 못했습니다.");
                return;
            }
        }

        int potionIndex = -1;
        for (int i = 0; i < player.Inventory.Items.Count; i++)
        {
            ItemBase item = player.Inventory.Items[i];
            if (item?.itemBaseSO == null || item.itemBaseSO.itemID != potion.itemID) continue;
            potionIndex = i;
            break;
        }

        if (potionIndex < 0)
        {
            Debug.LogWarning("[PortalTrialSetup] 인벤토리에 체력 포션을 추가하지 못했습니다.");
            return;
        }

        C_Enums.UseSlot slot = C_Enums.UseSlot.Slot_1;
        player.Inventory.useSlots[slot] = potionIndex;
        if (UIManager.Instance != null && UIManager.Instance.mainUI != null)
            UIManager.Instance.mainUI.RegisterUseItemSlot(slot, potionIndex);
    }
}
