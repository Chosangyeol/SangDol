using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public sealed class ReproducedUiRig : IDisposable
{
    public readonly UIManager Manager;
    public readonly GameObject Panel;
    private readonly GameObject root, optionHost, inventoryHost, tooltipHost;
    private readonly UIManager previous = UIManager.Instance;
    public ReproducedUiRig()
    {
        root = new GameObject("ReproducedUIRoot"); root.SetActive(false);
        Manager = root.AddComponent<UIManager>();
        typeof(UIManager).GetProperty("Instance").SetValue(null, Manager);
        optionHost = new GameObject("OptionComponentHost");
        var option = optionHost.AddComponent<OptionUI>(); option.enabled = false;
        Panel = new GameObject("ActualOptionPanel"); Panel.transform.SetParent(optionHost.transform); Panel.SetActive(false);
        option.optionUI = Panel; Manager.optionUI = option;
        inventoryHost = new GameObject("InventoryPanel"); inventoryHost.SetActive(false);
        Manager.inventoryUI = inventoryHost.AddComponent<InventoryUI>();
        tooltipHost = new GameObject("TestTooltip"); tooltipHost.SetActive(false);
        var tooltip = tooltipHost.AddComponent<ItemTooltip>();
        InventoryTransferRig.Set(Manager.inventoryUI, "tooltip", tooltip);
    }
    public List<C_Enums.UIList> Stack => (List<C_Enums.UIList>)typeof(UIManager).GetField("openedUIList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Manager);
    public void Escape() => typeof(UIManager).GetMethod("CloseTopUI", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Manager, null);
    public void Dispose()
    {
        UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(optionHost);
        UnityEngine.Object.DestroyImmediate(inventoryHost); UnityEngine.Object.DestroyImmediate(tooltipHost);
        typeof(UIManager).GetProperty("Instance").SetValue(null, previous);
    }
}

public sealed class ReproducedCraftRig : IDisposable
{
    public readonly QuestRewardRig Data;
    public readonly NpcCraftManager Craft;
    public CraftRecipe Recipe;
    private readonly GameObject go;
    private readonly NpcCraftManager previous = NpcCraftManager.instance;
    public ReproducedCraftRig(int capacity)
    {
        Data = new QuestRewardRig(capacity);
        go = new GameObject("ReproducedCraft"); go.SetActive(false); Craft = go.AddComponent<NpcCraftManager>();
        NpcCraftManager.instance = Craft;
        InventoryTransferRig.Set(Craft, "_model", Data.Model);
        Data.Model.Stat.Stat.gold = 100;
        Select(1, "10008");
    }
    public void Select(int amount, string result)
    {
        Recipe = new CraftRecipe { requireGoldAmount = 20, materialItemIds = new[] { "20002" },
            materialItemAmounts = new[] { amount }, resultItemId = result };
        InventoryTransferRig.Set(Craft, "selectedRecipe", Recipe);
    }
    public void Material(int stack, int slot=0) => Data.Inventory.SetItemAt(slot, new NormalItemBase(Data.InventoryRig.Material, stack));
    public void Dispose()
    {
        UnityEngine.Object.DestroyImmediate(go); NpcCraftManager.instance = previous; Data.Dispose();
    }
}

public static class ReproducedFixCases
{
    public static void InvincibilityGuardsModelAndStatDamageWithoutNotifications()
    {
        using (var r = new MovementRig(false))
        {
            var previous = GameEvent.OnStatChange;
            int changes = 0; GameEvent.OnStatChange = _ => changes++;
            var buffData = ScriptableObject.CreateInstance<BuffSO>();
            try
            {
                r.Model.Stat.Stat.maxHp = new StatValue(100); r.Model.Stat.Stat.curHp = 100;
                var buff = new InvincibilityBuff(r.Model, buffData, .1f); buff.OnEnable();
                foreach (bool percent in new[] { false, true })
                {
                    r.Model.Damaged(percent ? 1f : 10000f, percent);
                    r.Model.Stat.Damaged(percent ? 1f : 10000f, percent);
                }
                Assert.AreEqual(100f, r.Model.Stat.Stat.curHp); Assert.IsFalse(r.Model.isDie);
                Assert.AreEqual(0, changes);
                buff.OnDisable(); r.Model.Buff.ImmunityEnable();
                r.Model.Damaged(10f, false); Assert.AreEqual(90f, r.Model.Stat.Stat.curHp);
                Assert.AreEqual(1, changes);
                r.Model.isDie = true; r.Model.Damaged(10f, false); r.Model.Stat.Damaged(10f, false);
                Assert.AreEqual(90f, r.Model.Stat.Stat.curHp); Assert.AreEqual(1, changes);
            }
            finally { GameEvent.OnStatChange = previous; UnityEngine.Object.DestroyImmediate(buffData); }
        }
    }

    public static void HpGearSwapsPreserveCurrentHpAndNotifyOnlyFinalState()
    {
        foreach (bool percent in new[] { false, true })
        using (var r = new InventoryTransferRig())
        {
            var old = r.Gear(0); var next = r.Gear(0);
            old.itemBaseSO.statToIncrease = next.itemBaseSO.statToIncrease = C_Enums.CharacterStat.MaxHp;
            old.itemBaseSO.isPercent = next.itemBaseSO.isPercent = percent;
            old.itemBaseSO.value = percent ? .5f : 50f;
            next.itemBaseSO.value = percent ? 1f : 100f;
            r.Inventory.SetItemAt(0, old); r.Equipment.EquipItem(old);
            r.Model.Stat.Stat.curHp = 150; r.Inventory.SetItemAt(0, next);
            int notifications = 0;
            GameEvent.OnStatChange = stat =>
            {
                notifications++;
                Assert.AreEqual(r.Equipment.equipItems[ItemEnums.EquipItemType.Weapon] == next ? 200f : 150f, stat.maxHp.FinalValue);
                Assert.AreEqual(150f, stat.curHp);
            };
            for (int i=0; i<3; i++) { r.Equipment.EquipItem(next); r.Equipment.EquipItem(old); }
            Assert.AreEqual(6, notifications); Assert.AreEqual(150f, r.Model.Stat.Stat.curHp);
            GameEvent.OnStatChange = null;
            r.Equipment.UnequipItem(ItemEnums.EquipItemType.Weapon);
            Assert.AreEqual(100f, r.Model.Stat.Stat.curHp);
        }
    }

    public static void HpSwapToDifferentStatClampsAtFinalMaximum()
    {
        using(var r=new InventoryTransferRig())
        {
            var old=r.Gear(0);old.itemBaseSO.statToIncrease=C_Enums.CharacterStat.MaxHp;old.itemBaseSO.value=100;
            r.Inventory.SetItemAt(0,old);r.Equipment.EquipItem(old);r.Model.Stat.Stat.curHp=190;
            var next=r.Gear(0);r.Inventory.SetItemAt(0,next);r.Equipment.EquipItem(next);
            Assert.AreEqual(100f,r.Model.Stat.Stat.maxHp.FinalValue);Assert.AreEqual(100f,r.Model.Stat.Stat.curHp);
        }
    }

    public static void FailedCraftPreservesCostsReferencesAndCanRetry()
    {
        using(var r=new ReproducedCraftRig(2))
        {
            r.Material(10);var other=new NormalItemBase(r.Data.Other,4);r.Data.Inventory.SetItemAt(1,other);
            var material=r.Data.Inventory.Items[0];string before=r.Data.Snapshot();int events=0;
            r.Data.Inventory.OnInventoryUpdated+=()=>events++;
            r.Craft.ExecuteCraft();
            Assert.AreEqual(before,r.Data.Snapshot());Assert.AreSame(material,r.Data.Inventory.Items[0]);
            Assert.AreSame(other,r.Data.Inventory.Items[1]);Assert.AreEqual(0,events);
            r.Data.Inventory.RemoveItemAt(1);r.Craft.ExecuteCraft();
            Assert.AreEqual(80,r.Data.Model.Stat.Stat.gold);Assert.AreEqual(9,material.currentStack);
            Assert.AreEqual(1,r.Data.Inventory.GetTotalItemCount("10008"));r.Craft.ExecuteCraft();
            Assert.AreEqual(80,r.Data.Model.Stat.Stat.gold);Assert.AreEqual(1,r.Data.Inventory.GetTotalItemCount("10008"));
        }
    }

    public static void CraftCanUseSlotFreedByConsumedMaterial()
    {
        using(var r=new ReproducedCraftRig(1))
        {
            r.Material(1);r.Craft.ExecuteCraft();
            Assert.AreEqual(80,r.Data.Model.Stat.Stat.gold);Assert.AreEqual(0,r.Data.Inventory.GetTotalItemCount("20002"));
            Assert.AreEqual(1,r.Data.Inventory.GetTotalItemCount("10008"));
        }
    }

    public static void CraftMergesOutputAndAggregatesDuplicateIngredients()
    {
        using(var r=new ReproducedCraftRig(1))
        {
            r.Material(10);var material=r.Data.Inventory.Items[0];
            r.Select(2,"20002");r.Recipe.materialItemIds=new[]{"20002","20002"};r.Recipe.materialItemAmounts=new[]{2,3};
            r.Craft.ExecuteCraft();Assert.AreEqual(6,material.currentStack);Assert.AreSame(material,r.Data.Inventory.Items[0]);
            Assert.AreEqual(80,r.Data.Model.Stat.Stat.gold);
            r.Select(1,"10008");r.Recipe.materialItemIds=new[]{"20002","20002"};r.Recipe.materialItemAmounts=new[]{4,4};
            string before=r.Data.Snapshot();r.Craft.ExecuteCraft();Assert.AreEqual(before,r.Data.Snapshot());
        }
    }

    public static void InvalidCraftDoesNotChargeOrConsume()
    {
        using(var r=new ReproducedCraftRig(2))
        {
            r.Material(10);
            foreach(int mode in new[]{0,1,2,3,4})
            {
                r.Select(1,"10008");
                if(mode==0)r.Data.Model.Stat.Stat.gold=0;
                if(mode==1)r.Recipe.requireGoldAmount=-1;
                if(mode==2)r.Recipe.materialItemAmounts=new int[0];
                if(mode==3)r.Recipe.resultItemId="99999";
                if(mode==4)r.Recipe.materialItemAmounts=new[]{-1};
                string before=r.Data.Snapshot();r.Craft.ExecuteCraft();Assert.AreEqual(before,r.Data.Snapshot());
                r.Data.Model.Stat.Stat.gold=100;
            }
        }
    }

    public static void CraftNotificationsObserveCommitAndCannotDoublePay()
    {
        using(var r=new ReproducedCraftRig(2))
        {
            r.Material(2);int observed=0;
            Action assertFinal=()=>
            {
                observed++;Assert.AreEqual(80,r.Data.Model.Stat.Stat.gold);
                Assert.AreEqual(1,r.Data.Inventory.GetTotalItemCount("20002"));
                Assert.AreEqual(1,r.Data.Inventory.GetTotalItemCount("10008"));r.Craft.ExecuteCraft();
            };
            GameEvent.OnStatChange=_=>assertFinal();
            r.Data.Inventory.OnAddItemInventory+=_=>assertFinal();
            r.Craft.ExecuteCraft();Assert.GreaterOrEqual(observed,2);
            Assert.AreEqual(80,r.Data.Model.Stat.Stat.gold);
        }
    }

    public static void OptionCyclesAndButtonCloseKeepStackConsistent()
    {
        using(var r=new ReproducedUiRig())
        {
            for(int i=0;i<4;i++)
            {
                r.Escape();Assert.IsTrue(r.Panel.activeSelf);Assert.AreEqual(1,r.Stack.Count);
                r.Escape();Assert.IsFalse(r.Panel.activeSelf);Assert.AreEqual(0,r.Stack.Count);
            }
            r.Escape();r.Manager.optionUI.Toggle(true);
            r.Escape();Assert.IsTrue(r.Panel.activeSelf);Assert.AreEqual(1,r.Stack.Count);
        }
    }

    public static void EscapeClosesVisibleTopWithoutReopeningHiddenPanels()
    {
        using(var r=new ReproducedUiRig())
        {
            r.Manager.ToggleUI(C_Enums.UIList.Inventory);
            r.Manager.ToggleUI(C_Enums.UIList.Option);
            r.Escape();Assert.IsFalse(r.Panel.activeSelf);Assert.IsTrue(r.Manager.inventoryUI.gameObject.activeSelf);
            r.Escape();Assert.IsFalse(r.Manager.inventoryUI.gameObject.activeSelf);
            r.Manager.ToggleUI(C_Enums.UIList.Inventory);r.Manager.inventoryUI.Toggle(true);
            r.Escape();Assert.IsTrue(r.Panel.activeSelf);Assert.IsFalse(r.Manager.inventoryUI.gameObject.activeSelf);
        }
    }

    public static void HiddenPanelsClearStackUsingRealUiPrefab()
    {
        var previous = UIManager.Instance;
        var root = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/03. Prefab/Manager/UIManager.prefab"));
        try
        {
            var manager=root.GetComponent<UIManager>();
            manager.ToggleUI(C_Enums.UIList.Inventory);
            typeof(UIManager).GetMethod("HideGameUIs",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(manager,null);
            var stack=(List<C_Enums.UIList>)typeof(UIManager).GetField("openedUIList",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(manager);
            Assert.AreEqual(0,stack.Count);Assert.IsFalse(manager.inventoryUI.gameObject.activeSelf);
            typeof(UIManager).GetMethod("CloseTopUI",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(manager,null);
            Assert.IsTrue(manager.optionUI.optionUI.activeSelf);Assert.IsFalse(manager.inventoryUI.gameObject.activeSelf);
        }
        finally{UnityEngine.Object.DestroyImmediate(root);typeof(UIManager).GetProperty("Instance").SetValue(null,previous);}
    }

    public static void CraftPreviewRecoversAfterSpaceAndGoldChanges()
    {
        using(var r=new ReproducedCraftRig(2))
        {
            r.Material(10);r.Data.Inventory.SetItemAt(1,new NormalItemBase(r.Data.Other,4));
            var root=new GameObject("CraftPreviewTest");root.SetActive(false);
            var ui=root.AddComponent<CraftUI>();
            Func<string,TMPro.TMP_Text> text=n=>{var go=new GameObject(n,typeof(RectTransform));go.transform.SetParent(root.transform);return go.AddComponent<TMPro.TextMeshProUGUI>();};
            Func<string,UnityEngine.UI.Image> image=n=>{var go=new GameObject(n,typeof(RectTransform));go.transform.SetParent(root.transform);return go.AddComponent<UnityEngine.UI.Image>();};
            ui.resultItemIcon=image("Icon");ui.resultItemNameText=text("Name");ui.resultItemRarityText=text("Rarity");ui.requireGoldAmountText=text("Gold");
            ui.materialImages=new[]{image("Mat")};ui.materialCountTexts=new[]{text("Count")};ui.executeButton=new GameObject("Execute");ui.executeButton.transform.SetParent(root.transform);
            try
            {
                root.SetActive(true);ui.SetCraftExecuteUI(r.Recipe,r.Data.Model);
                Assert.IsFalse(ui.executeButton.activeSelf);
                r.Data.Inventory.RemoveItemAt(1);Assert.IsTrue(ui.executeButton.activeSelf);
                r.Data.Model.UseGold(100);Assert.IsFalse(ui.executeButton.activeSelf);
                r.Data.Model.GainGold(100);Assert.IsTrue(ui.executeButton.activeSelf);
                r.Recipe.materialItemAmounts=new int[0];
                Assert.DoesNotThrow(()=>ui.SetCraftExecuteUI(r.Recipe,r.Data.Model));Assert.IsFalse(ui.executeButton.activeSelf);
            }
            finally
            {
                typeof(CraftUI).GetMethod("OnDisable",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(ui,null);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }

    public static IEnumerator InvincibilityExpiresAndRestoresRuntimeDamage()
    {
        using(var r=new MovementRig(true))
        {
            var data=ScriptableObject.CreateInstance<BuffSO>();data.isBuff=true;
            var previousDamageText=DamageTextManager.Instance;DamageTextManager.Instance=null;
            var previous=GameEvent.OnStatChange;GameEvent.OnStatChange=null;
            try
            {
                r.Model.Stat.Stat.maxHp=new StatValue(100);r.Model.Stat.Stat.curHp=100;
                var action=new InvincibilityBuff(r.Model,data,.05f);
                r.Model.Buff.AddBuff(new SBuff(r.Model.gameObject,r.Model.gameObject,action));
                r.Model.Damaged(10000,false);r.Model.Damaged(1,true);
                Assert.IsFalse(r.Model.isDie);Assert.AreEqual(100f,r.Model.Stat.Stat.curHp);
                yield return new WaitForSecondsRealtime(.1f);
                r.Model.Buff.UpdateBuff(.1f);
                Assert.IsFalse(r.Model.Buff.isInvincibility);
                r.Model.Damaged(10,false);Assert.AreEqual(90f,r.Model.Stat.Stat.curHp);
            }
            finally{GameEvent.OnStatChange=previous;DamageTextManager.Instance=previousDamageText;UnityEngine.Object.DestroyImmediate(data);}
        }
    }

    public static IEnumerator RealEscapeInputTogglesOnceAndStopsMovement()
    {
        using(var player=new MovementRig(true))
        using(var ui=new ReproducedUiRig())
        {
            var host=new GameObject("RealEscapeInput");host.SetActive(false);
            var inputs=host.AddComponent<PlayerInputs>();InventoryTransferRig.Set(inputs,"model",player.Model);
            var actions=new List<InputAction>();var refs=new List<InputActionReference>();
            var inputAsset=ScriptableObject.CreateInstance<InputActionAsset>();
            var inputMap=inputAsset.AddActionMap("Regression");
            foreach(string field in new[]{"moveAction","interactActon","attackAction","skillSlotAction","useItemSlotAction","uiAction","pointerPos"})
            {
                var action=inputMap.AddAction(field,InputActionType.Button,field=="uiAction"?"<Keyboard>/escape":null);
                var reference=InputActionReference.Create(action);actions.Add(action);refs.Add(reference);
                InventoryTransferRig.Set(inputs,field,reference);
            }
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try
            {
                host.SetActive(true);player.Controller.RequestMove(player.Model.transform.position+Vector3.forward*3);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));InputSystem.Update();yield return null;
                Assert.IsTrue(ui.Panel.activeSelf);Assert.AreEqual(1,ui.Stack.Count);
                Assert.IsTrue(player.Agent.isStopped);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Escape));InputSystem.Update();yield return null;
                Assert.IsFalse(ui.Panel.activeSelf);Assert.AreEqual(0,ui.Stack.Count);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);InputSystem.RemoveDevice(keyboard);
                inputAsset.Disable();foreach(var reference in refs)UnityEngine.Object.DestroyImmediate(reference);
                UnityEngine.Object.DestroyImmediate(inputAsset);
            }
        }
    }
}
