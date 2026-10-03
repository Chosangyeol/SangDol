using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class GameplayAuditCases
{
    public static void ToughnessReducesDamageAndResetsWithoutDrift()
    {
        using (var r = new InventoryTransferRig())
        {
            var special = new C_SpecialStat(r.Model);
            r.Model.Stat.Stat.statPoint = 150;
            for (int cycle = 0; cycle < 3; cycle++)
            {
                special.TryInvestPont(C_Enums.SpecialStat.S4);
                for (int i = 0; i < 3; i++) special.TryInvestPont(C_Enums.SpecialStat.S5);
                Assert.That(r.Model.Stat.Stat.damageTakeMultiplier.FinalValue, Is.EqualTo(.97f).Within(.0001f));
                r.Model.Stat.Stat.curHp = 100f;
                r.Model.Stat.Stat.Damaged(100f, false);
                Assert.AreEqual(3f, r.Model.Stat.Stat.curHp);
                special.ResetSpecialStat();
                special.ResetSpecialStat();
                Assert.That(r.Model.Stat.Stat.damageTakeMultiplier.FinalValue, Is.EqualTo(1f).Within(.0001f));
                Assert.That(r.Model.Stat.Stat.maxHp.FinalValue, Is.EqualTo(100f).Within(.0001f));
            }
            for (int i = 0; i < 101; i++) special.TryInvestPont(C_Enums.SpecialStat.S5);
            r.Model.Stat.Stat.curHp = 100f;
            r.Model.Stat.Stat.Damaged(100f, false);
            Assert.AreEqual(100f, r.Model.Stat.Stat.curHp, "Damage reduction must never heal the recipient.");
            special.ResetSpecialStat();
            Assert.That(r.Model.Stat.Stat.damageTakeMultiplier.FinalValue, Is.EqualTo(1f).Within(.0001f));
        }
    }

    public static void RemovingHealthEquipmentAndBuffClampsWithoutHealing()
    {
        using (var r = new InventoryTransferRig())
        {
            var gear = r.Gear(0);
            gear.itemBaseSO.statToIncrease = C_Enums.CharacterStat.MaxHp;
            gear.itemBaseSO.value = 50f;
            r.Inventory.SetItemAt(0, gear);
            r.Equipment.EquipItem(gear);
            r.Model.Stat.Stat.curHp = 150f;
            r.Equipment.UnequipItem(ItemEnums.EquipItemType.Weapon);
            Assert.AreEqual(100f, r.Model.Stat.Stat.curHp);
            r.Model.AddStat(C_Enums.CharacterStat.MaxHp, true, .5f);
            r.Model.Stat.Stat.curHp = 80f;
            r.Model.RemoveStat(C_Enums.CharacterStat.MaxHp, true, .5f);
            Assert.AreEqual(80f, r.Model.Stat.Stat.curHp);
        }
    }

    public static void DodgeStigmaEquipReplaceAndUnequipAreReversible()
    {
        using (var r = new InventoryTransferRig())
        {
            var stigma = new C_Stigma(r.Model, null, null, null, null, null, null);
            stigma.EquipStigma(9, EStigmaType.Lv9_A);
            for (int i = 0; i < 3; i++)
            {
                stigma.EquipStigma(10, EStigmaType.Lv10_B);
                stigma.EquipStigma(10, EStigmaType.Lv10_B);
                Assert.AreEqual(3f, r.Model.Stat.Stat.dodgeCooldownReduction);
                stigma.UnEquipStigma(10);
                stigma.UnEquipStigma(10);
                Assert.AreEqual(1f, r.Model.Stat.Stat.dodgeCooldownReduction);
            }
            stigma.UnEquipStigma(9);
            Assert.AreEqual(0f, r.Model.Stat.Stat.dodgeCooldownReduction);
        }
    }

    public static void FireFistGivesTenPercentForTenSecondsEveryFifteenSeconds()
    {
        using (var r = new InventoryTransferRig())
        {
            var data = ScriptableObject.CreateInstance<BuffSO>();
            try
            {
                InventoryTransferRig.Set(r.Model, "buff", new C_Buff(r.Model));
                var stigma = new C_Stigma(r.Model, data, null, null, null, null, null);
                stigma.EquipStigma(5, EStigmaType.Lv5_A);
                var hit = typeof(C_Stigma).GetMethod("HandleOnHitTarget", BindingFlags.Instance | BindingFlags.NonPublic);
                Action trigger = () => hit.Invoke(stigma, new object[] { r.Model, 1f, true, null });
                trigger(); Assert.That(r.Attack, Is.EqualTo(110f).Within(.0001f));
                r.Model.Buff.UpdateBuff(10.01f); Assert.AreEqual(100f, r.Attack);
                stigma.UpdateStigma(14.9f); trigger(); Assert.AreEqual(100f, r.Attack);
                stigma.UpdateStigma(.11f); trigger(); Assert.That(r.Attack, Is.EqualTo(110f).Within(.0001f));
                r.Model.Buff.RemoveAllBuff(); Assert.AreEqual(100f, r.Attack);
            }
            finally { UnityEngine.Object.DestroyImmediate(data); }
        }
    }

    public static void FullInventoryDialogueBlocksSuccessAndCanRetry()
    {
        using (var r = new DialogueAuditRig(1))
        {
            r.QuestRig.Csv("Q_30001_003");
            r.QuestRig.Inventory.SetItemAt(0, new NormalItemBase(r.QuestRig.Other, 1));
            string before = r.QuestRig.Snapshot();
            r.Dialog.StartDialogue("D_30004_010");
            StringAssert.Contains("공간", r.Dialog.dialogueText.text);
            Assert.IsFalse(r.Dialog.nextButton.gameObject.activeSelf);
            Assert.IsTrue(r.Dialog.choicePanel.activeSelf);
            Assert.IsTrue(r.Dialog.choice1Button.gameObject.activeInHierarchy);
            Assert.IsTrue(r.Dialog.choice2Button.gameObject.activeInHierarchy);
            r.Dialog.nextButton.onClick.Invoke(); r.Dialog.TryClickNextButton();
            Assert.AreEqual(before, r.QuestRig.Snapshot());
            Assert.AreEqual(QuestState.CanClear, r.QuestRig.Quest.GetQuestState("Q_30001_003"));
            r.QuestRig.Inventory.RemoveItemAt(0);
            r.Dialog.choice1Button.onClick.Invoke();
            Assert.AreEqual(QuestState.Completed, r.QuestRig.Quest.GetQuestState("Q_30001_003"));
            StringAssert.Contains("보수는 여기", r.Dialog.dialogueText.text);
            string paid = r.QuestRig.Snapshot();
            r.Dialog.StartDialogue("D_30004_010"); Assert.AreEqual(paid, r.QuestRig.Snapshot());
        }
    }

    public static void FailedAppleDialogueCannotAdvanceAndExitReleasesControls()
    {
        using (var r = new DialogueAuditRig(1))
        {
            var quest = r.QuestRig.Csv("Q_30001_002");
            r.QuestRig.Inventory.AddItem(r.QuestRig.Items.GetItemBaseSO(quest.questTarget).CreateItem(1));
            r.QuestRig.Quest.questPreview.SetActive(true);
            r.QuestRig.Model.isInteracting = true; r.QuestRig.Model.ControlDisable();
            r.Dialog.StartDialogue("D_30002_008");
            StringAssert.Contains("공간", r.Dialog.dialogueText.text);
            r.Dialog.nextButton.onClick.Invoke(); r.Dialog.TryClickNextButton();
            StringAssert.DoesNotContain("체력 포션", r.Dialog.dialogueText.text);
            r.Dialog.choice2Button.onClick.Invoke();
            Assert.IsFalse(r.Dialog.IsDialogueActive());
            Assert.IsFalse(r.QuestRig.Quest.questPreview.activeSelf);
            Assert.IsFalse(r.QuestRig.Model.isInteracting);
            Assert.IsTrue(r.QuestRig.Model.canMove && r.QuestRig.Model.canAttack && r.QuestRig.Model.canSkill);
        }
    }

    public static void RefusalClosesSiblingPreviewAndDoesNotAcceptQuest()
    {
        foreach (var pair in new[] { new[] { "D_30002_004", "Q_30001_002" }, new[] { "D_30004_007", "Q_30001_003" } })
        using (var r = new DialogueAuditRig())
        {
            r.QuestRig.Csv(pair[1]); r.QuestRig.Quest.questStateDict[pair[1]] = QuestState.NotStart;
            r.Dialog.StartDialogue(pair[0]); Assert.IsTrue(r.QuestRig.Quest.questPreview.activeSelf);
            r.Dialog.choice2Button.onClick.Invoke(); Assert.IsFalse(r.QuestRig.Quest.questPreview.activeSelf);
            r.Dialog.nextButton.onClick.Invoke(); Assert.IsFalse(r.Dialog.IsDialogueActive());
            Assert.IsFalse(r.Dialog.nextButton.gameObject.activeInHierarchy);
            Assert.AreEqual(QuestState.NotStart, r.QuestRig.Quest.GetQuestState(pair[1]));
            r.QuestRig.Quest.questPreview.SetActive(true); r.Npc.CloseUI();
            Assert.IsFalse(r.QuestRig.Quest.questPreview.activeSelf);
        }
    }

    public static void DailChoicesFollowAcceptanceAndRefusalText()
    {
        using (var r = new DialogueAuditRig())
        {
            r.Dialog.StartDialogue("D_30004_003"); r.Dialog.choice1Button.onClick.Invoke();
            StringAssert.Contains("정말 감사합니다", r.Dialog.dialogueText.text);
            r.Dialog.StartDialogue("D_30004_003"); r.Dialog.choice2Button.onClick.Invoke();
            StringAssert.Contains("들어주기 싫으면", r.Dialog.dialogueText.text);
        }
    }

    public static void CompletedIntroductionAllowsAppleQuestRetry()
    {
        using (var r = new DialogueAuditRig())
        {
            r.QuestRig.Csv("Q_30001_002");
            r.QuestRig.Quest.questStateDict["Q_30001_001"] = QuestState.Completed;
            r.QuestRig.Quest.questStateDict["Q_30001_002"] = QuestState.NotStart;
            string before = r.QuestRig.Snapshot();
            r.Dialog.StartDialogue("D_30002_002");
            Assert.IsFalse(r.Dialog.choicePanel.activeSelf);
            Assert.AreEqual(before, r.QuestRig.Snapshot());
            r.Dialog.nextButton.onClick.Invoke(); r.Dialog.nextButton.onClick.Invoke();
            r.Dialog.choice1Button.onClick.Invoke();
            Assert.AreEqual(QuestState.InProgress, r.QuestRig.Quest.GetQuestState("Q_30001_002"));
        }
    }

    public static void TalkButtonUsesNpcDialogueAndFallback()
    {
        foreach (bool fallback in new[] { false, true })
        using (var r = new DialogueAuditRig())
        {
            var npc = ScriptableObject.CreateInstance<NpcSO>();
            try
            {
                npc.defaultDialogID = "D_30003_001"; npc.talkDialogID = fallback ? "" : "D_30004_001";
                r.Npc.OpenNpcUI(npc);
                foreach (var button in r.Npc.buttonGroup.GetComponentsInChildren<Button>())
                    if (button.GetComponentInChildren<TMP_Text>().text == "대화 하기") button.onClick.Invoke();
                Assert.IsFalse(r.Npc.buttonGroup.gameObject.activeSelf);
                StringAssert.Contains(fallback ? "없는거" : "화살값", r.Dialog.dialogueText.text);
            }
            finally { UnityEngine.Object.DestroyImmediate(npc); }
        }
    }

    public static void NpcDialogueAssetReferencesUseTheirOwnNames()
    {
        using (var r = new DialogueAuditRig())
        {
            var ids = new System.Collections.Generic.HashSet<string>();
            foreach (string name in new[] { "Haber", "Maria", "Shop", "Dail", "Ellen", "Yakoon" })
            {
                var npc = AssetDatabase.LoadAssetAtPath<NpcSO>("Assets/02. Scripts/Interact/Npc/SO/" + name + ".asset");
                Assert.IsNotNull(npc);
                Assert.IsTrue(ids.Add(npc.npcID), "Duplicate NPC ID: " + npc.npcID);
                Assert.AreEqual(npc.npcName, r.Dialog.dialogueDict[npc.talkDialogID].npcName, name);
                Assert.AreEqual(npc.npcID, r.Dialog.dialogueDict[npc.talkDialogID].npcID, name);
                Assert.AreEqual(npc.npcID, r.Dialog.dialogueDict[npc.defaultDialogID].npcID, name);
            }
        }
    }

    public static void TalkingToEllenDoesNotCompleteMerchantQuest()
    {
        using (var r = new QuestRewardRig())
        {
            var merchant = AssetDatabase.LoadAssetAtPath<NpcSO>("Assets/02. Scripts/Interact/Npc/SO/Shop.asset");
            var ellen = AssetDatabase.LoadAssetAtPath<NpcSO>("Assets/02. Scripts/Interact/Npc/SO/Ellen.asset");
            var quest = r.Seed(); quest.questType = "Talk"; quest.questTarget = merchant.npcID;
            r.Quest.questStateDict[quest.questID] = QuestState.InProgress;
            r.Quest.questTalkProgressDict[quest.questID] = false;
            var talk = typeof(QuestManager).GetMethod("HandleTalkNpc", BindingFlags.Instance | BindingFlags.NonPublic);
            talk.Invoke(r.Quest, new object[] { ellen.npcID });
            Assert.AreEqual(QuestState.InProgress, r.Quest.GetQuestState(quest.questID));
            Assert.IsFalse(r.Quest.questTalkProgressDict[quest.questID]);
            talk.Invoke(r.Quest, new object[] { merchant.npcID });
            Assert.AreEqual(QuestState.CanClear, r.Quest.GetQuestState(quest.questID));
            Assert.IsTrue(r.Quest.questTalkProgressDict[quest.questID]);
        }
    }

    public static void ThrowingRewardSubscriberDoesNotPermitDuplicateRewards()
    {
        using (var r = new QuestRewardRig())
        {
            var data = r.Seed(r.Reward("20002", 1));
            GameEvent.OnGetItem = _ => throw new InvalidOperationException("subscriber failure");
            Assert.Throws<InvalidOperationException>(() => r.Quest.CompleteQuest(data.questID));
            Assert.AreEqual(QuestState.Completed, r.Quest.GetQuestState(data.questID));
            string paid = r.Snapshot(); GameEvent.OnGetItem = null;
            r.Quest.CompleteQuest(data.questID); Assert.AreEqual(paid, r.Snapshot());
        }
    }
}

public sealed class DialogueAuditRig : IDisposable
{
    public readonly QuestRewardRig QuestRig;
    public readonly DialogManager Dialog;
    public readonly NpcDialogManager Npc;
    private readonly GameObject root, managers;
    private readonly DialogManager oldDialog = DialogManager.Instance;
    private readonly NpcDialogManager oldNpc = NpcDialogManager.Instance;
    private readonly Action oldHide = GameEvent.OnUIInvisable, oldShow = GameEvent.OnMainUIviable;
    public DialogueAuditRig(int capacity = 4)
    {
        QuestRig = new QuestRewardRig(capacity);
        InventoryTransferRig.Set(QuestRig.Model, "buff", new C_Buff(QuestRig.Model));
        GameEvent.OnUIInvisable = null; GameEvent.OnMainUIviable = null;
        root = new GameObject("DialogueAuditUI"); managers = new GameObject("DialogueAuditManagers"); managers.SetActive(false);
        Dialog = managers.AddComponent<DialogManager>(); Npc = managers.AddComponent<NpcDialogManager>();
        DialogManager.Instance = Dialog; NpcDialogManager.Instance = Npc;
        InventoryTransferRig.Set(Npc, "_model", QuestRig.Model);
        Dialog.dialoguePanel = Child("Dialog"); Npc.npcDialogPanel = Dialog.dialoguePanel;
        Dialog.npcNameText = Text("Name"); Dialog.dialogueText = Text("Dialogue");
        Dialog.nextButton = Button("Next"); Dialog.choicePanel = Child("Choices");
        Dialog.choice1Button = Button("Choice1"); Dialog.choice2Button = Button("Choice2");
        Dialog.choice1TextUI = Text("Choice1Text"); Dialog.choice2TextUI = Text("Choice2Text");
        Dialog.npcNameText.transform.SetParent(Dialog.dialoguePanel.transform, false);
        Dialog.dialogueText.transform.SetParent(Dialog.dialoguePanel.transform, false);
        Dialog.nextButton.transform.SetParent(Dialog.dialoguePanel.transform, false);
        Dialog.choicePanel.transform.SetParent(Dialog.dialoguePanel.transform, false);
        Dialog.choice1Button.transform.SetParent(Dialog.choicePanel.transform, false);
        Dialog.choice2Button.transform.SetParent(Dialog.choicePanel.transform, false);
        Dialog.choice1TextUI.transform.SetParent(Dialog.choice1Button.transform, false);
        Dialog.choice2TextUI.transform.SetParent(Dialog.choice2Button.transform, false);
        Npc.buttonGroup = Child("Menu").transform; Npc.buttonPrefab = Button("Template").gameObject;
        Npc.buttonGroup.SetParent(Dialog.dialoguePanel.transform, false);
        Text("ButtonLabel").transform.SetParent(Npc.buttonPrefab.transform);
        QuestRig.Quest.questPreview = Child("Preview"); QuestRig.Quest.questPreview.SetActive(false);
        QuestRig.Quest.questNameText = Text("QuestName"); QuestRig.Quest.questDialogText = Text("QuestDialog");
        QuestRig.Quest.questGoldReward = Text("Gold"); QuestRig.Quest.questExpReward = Text("Exp");
        QuestRig.Quest.questItemReward1 = Text("Reward1"); QuestRig.Quest.questItemReward2 = Text("Reward2");
        QuestRig.Quest.questItemReward3 = Text("Reward3");
        QuestRig.Quest.questRewardItemSlots = new RewardItemSlot[3];
        for (int i = 0; i < 3; i++)
        {
            var go = Child("Slot" + i); var slot = go.AddComponent<RewardItemSlot>(); slot.itemIcon = go.AddComponent<Image>();
            QuestRig.Quest.questRewardItemSlots[i] = slot;
        }
        typeof(DialogManager).GetMethod("LoadDialogueCSV", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(Dialog, new object[] { "NpcDialogDataBase" });
    }
    private GameObject Child(string name)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(root.transform); return go;
    }
    private TMP_Text Text(string name) => Child(name).AddComponent<TextMeshProUGUI>();
    private Button Button(string name) => Child(name).AddComponent<Button>();
    public void Dispose()
    {
        UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(managers);
        DialogManager.Instance = oldDialog; NpcDialogManager.Instance = oldNpc;
        GameEvent.OnUIInvisable = oldHide; GameEvent.OnMainUIviable = oldShow; QuestRig.Dispose();
    }
}

public class AuditCounterEnemy : EnemyBase, ICounterable
{
    public int Hits, Counters;
    public bool CanCounter { get; private set; } = true;
    public void EnableCounter() { CanCounter = true; }
    public void DisableCounter() { CanCounter = false; }
    public IEnumerator KnockDown(float duration, bool isReset) { yield break; }
    protected override void Awake() { }
    public override void Damaged(SDamageInfo info) { Hits++; }
    public void OnCounterSuccess(SDamageInfo info) { Counters++; }
}

public static class GameplayAuditRuntimeCases
{
    public static IEnumerator BasicAttackDamagesAndCountersEachEnemyOnce()
    {
        using (var r = new MovementRig(true))
        {
            var enemy = new GameObject("AuditEnemy");
            try
            {
                enemy.transform.position = r.Model.transform.position + Vector3.forward * 2f;
                var probe = enemy.AddComponent<AuditCounterEnemy>();
                for (int i = 0; i < 2; i++)
                {
                    var child = new GameObject("Collider" + i); child.transform.SetParent(enemy.transform, false);
                    child.AddComponent<BoxCollider>();
                }
                int notifications = 0; r.Model.OnHitTarget += (_, __, ___, ____) => notifications++;
                Physics.SyncTransforms();
                for (int combo = 0; combo < 4; combo++)
                {
                    r.Controller.currentCombo = combo; r.Model.OnAttackHit();
                    Assert.AreEqual(combo + 1, probe.Hits); Assert.AreEqual(combo + 1, probe.Counters);
                    Assert.AreEqual(combo + 1, notifications);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(enemy); }
        }
        yield return null;
    }

    public static IEnumerator PumpkinAttackHitsOnceAndRejectsInvalidTargets()
    {
        using (var r = new MovementRig(true))
        {
            r.Model.characterStatSO.maxHp = 100f;
            var controller = (UnityEditor.Animations.AnimatorController)r.Model.GetComponent<Animator>().runtimeAnimatorController;
            controller.AddParameter("AttackSpeed", AnimatorControllerParameterType.Float);
            InventoryTransferRig.Set(r.Model, "stat", new C_Stat(r.Model, r.Model.characterStatSO));
            var enemy = new GameObject("AuditPumpkin"); enemy.SetActive(false);
            var data = ScriptableObject.CreateInstance<EnemyStatSO>(); data.attackRange = 2f; data.attackDamage = 10f;
            var oldDamageText = DamageTextManager.Instance;
            DamageTextManager.Instance = null;
            try
            {
                enemy.transform.position = r.Model.transform.position - Vector3.forward;
                var pumpkin = enemy.AddComponent<PumpkinGay>(); pumpkin.enabled = false; pumpkin.statSO = data;
                typeof(EnemyBase).GetField("_target", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pumpkin, r.Model);
                for (int i = 0; i < 2; i++)
                {
                    var child = new GameObject("PlayerCollider" + i); child.transform.SetParent(r.Model.transform, false);
                    child.AddComponent<BoxCollider>();
                }
                Physics.SyncTransforms(); pumpkin.Attack(); Assert.AreEqual(90f, r.Model.Stat.Stat.curHp);
                enemy.transform.rotation = Quaternion.Euler(0, 180, 0); pumpkin.Attack(); Assert.AreEqual(90f, r.Model.Stat.Stat.curHp);
                typeof(EnemyBase).GetField("_isDead", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pumpkin, true);
                enemy.transform.rotation = Quaternion.identity; pumpkin.Attack(); Assert.AreEqual(90f, r.Model.Stat.Stat.curHp);
                typeof(EnemyBase).GetField("_target", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(pumpkin, null);
                Assert.DoesNotThrow(() => pumpkin.Attack());
            }
            finally
            {
                DamageTextManager.Instance = oldDamageText;
                UnityEngine.Object.DestroyImmediate(enemy); UnityEngine.Object.DestroyImmediate(data);
            }
        }
        yield return null;
    }

    public static IEnumerator ReopeningNpcUiDoesNotDuplicateTalkButtons()
    {
        using (var r = new DialogueAuditRig())
        {
            var npc = ScriptableObject.CreateInstance<NpcSO>();
            try
            {
                npc.defaultDialogID = "D_30003_001"; npc.talkDialogID = "D_30004_001";
                r.Npc.OpenNpcUI(npc); r.Npc.CloseUI(); r.Npc.OpenNpcUI(npc);
                yield return null;
                var buttons = r.Npc.buttonGroup.GetComponentsInChildren<Button>();
                Assert.AreEqual(1, buttons.Length);
                buttons[0].onClick.Invoke();
                StringAssert.Contains("화살값", r.Dialog.dialogueText.text);
                Assert.IsFalse(r.Npc.buttonGroup.gameObject.activeSelf);
            }
            finally { UnityEngine.Object.DestroyImmediate(npc); }
        }
    }
}
