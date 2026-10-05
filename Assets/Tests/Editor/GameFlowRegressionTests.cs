using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

public static class GameFlowRegressionCases
{
    public static void RuntimeMiddleBossCanBeHitBySkillQueries()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Boss60CorrectionsCases.MiddlePrefab);
        Assert.IsNotNull(root);
        foreach (var collider in root.GetComponentsInChildren<Collider>(true))
            Assert.IsTrue((LayerMask.GetMask("Enemy") & (1 << collider.gameObject.layer)) != 0, collider.name);
    }
    public static void PotionConsumesOnceSharesCooldownAndHealsExactlyItsTotal()
    {
        using (var r = new InventoryTransferRig(4))
        {
            InventoryTransferRig.Set(r.Model, "buff", new C_Buff(r.Model)); r.Model.canUse = true;
            var data = AssetDatabase.LoadAssetAtPath<UseItemSO>("Assets/Resources/ItemSOs/UseItems/20001_hpPotion_Basic.asset");
            r.Inventory.AddItem(data.CreateItem(3)); r.Model.Stat.Stat.curHp = 10f;
            r.Inventory.useSlots[C_Enums.UseSlot.Slot_1] = 0;
            r.Inventory.UseItem(C_Enums.UseSlot.Slot_1);
            Assert.AreEqual(2, r.Inventory.GetTotalItemCount("20001"));
            Assert.AreEqual(10f, r.Model.Stat.Stat.curHp, "Timed potion has no extra instant tick.");
            for (int i = 1; i <= 3; i++) { r.Model.Buff.UpdateBuff(1f); Assert.AreEqual(10f + i * 20f, r.Model.Stat.Stat.curHp, .001f); }
            Assert.AreEqual(0, r.Model.Buff.ListBuff.Count);
            r.Inventory.UseItem(C_Enums.UseSlot.Slot_1); Assert.AreEqual(2, r.Inventory.GetTotalItemCount("20001"));
            r.Inventory.SetItemAt(1, data.CreateItem(2)); r.Inventory.useSlots[C_Enums.UseSlot.Slot_2] = 1;
            r.Inventory.UseItem(C_Enums.UseSlot.Slot_2); Assert.AreEqual(2, r.Inventory.Items[1].currentStack, "Cooldown follows the item ID across stacks and slots.");
            var ready = (System.Collections.Generic.Dictionary<string, float>)typeof(C_Inventory).GetField("useItemReadyTimes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(r.Inventory);
            ready["20001"] = Time.time;
            r.Inventory.UseItem(C_Enums.UseSlot.Slot_2); Assert.AreEqual(1, r.Inventory.Items[1].currentStack, "Reuse is allowed at the cooldown boundary.");
            Assert.DoesNotThrow(() => r.Inventory.UseItem((C_Enums.UseSlot)999));
        }
    }
    public static void PotionHandlesUnevenFramesAndInfiniteHealingKeepsItsInterval()
    {
        foreach (float duration in new[] { 3f, 3.4f })
        foreach (bool largeFrame in new[] { false, true })
        using (var r = new InventoryTransferRig())
        {
            InventoryTransferRig.Set(r.Model, "buff", new C_Buff(r.Model)); r.Model.canUse = true;
            var data = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<UseItemSO>("Assets/Resources/ItemSOs/UseItems/20001_hpPotion_Basic.asset"));
            try
            {
                data.itemDuration = duration; r.Inventory.AddItem(data.CreateItem(2)); r.Inventory.useSlots[C_Enums.UseSlot.Slot_1] = 0;
                r.Model.Stat.Stat.curHp = 10f; r.Inventory.UseItem(C_Enums.UseSlot.Slot_1);
                if (largeFrame) r.Model.Buff.UpdateBuff(10f);
                else for (int i = 0; i < 40; i++) r.Model.Buff.UpdateBuff(.13f);
                Assert.AreEqual(70f, r.Model.Stat.Stat.curHp, .001f);
                Assert.AreEqual(0, r.Model.Buff.ListBuff.Count);
                Assert.AreEqual(1, r.Inventory.GetTotalItemCount("20001"));
                r.Model.Stat.Stat.curHp = 10f;
                var infinite = new HealBuff(r.Model, data.buffSO, -1f, false, 10f, 5f);
                Assert.IsFalse(infinite.OnUpdate(4f)); Assert.AreEqual(10f, r.Model.Stat.Stat.curHp);
                Assert.IsFalse(infinite.OnUpdate(1f)); Assert.AreEqual(20f, r.Model.Stat.Stat.curHp);
                Assert.IsFalse(infinite.OnUpdate(15f)); Assert.AreEqual(50f, r.Model.Stat.Stat.curHp);
            }
            finally { UnityEngine.Object.DestroyImmediate(data); }
        }
    }
    public static void MariaTurnInPrecedesAppleQuest()
    {
        var npc = AssetDatabase.LoadAssetAtPath<NpcSO>("Assets/02. Scripts/Interact/Npc/SO/Maria.asset");
        Assert.AreEqual(2, npc.npcQuests.Count);
        var intro = npc.npcQuests[0]; var apple = npc.npcQuests[1];
        Assert.AreEqual("Q_30001_001", intro.questID);
        Assert.IsTrue(string.IsNullOrEmpty(intro.startDialogID));
        Assert.AreEqual("D_30002_002", intro.clearDialogueID);
        Assert.AreEqual(intro.questID, apple.requiredQuestID);
        Assert.AreEqual("D_30002_003", apple.startDialogID);
        Assert.AreEqual("D_30002_008", apple.clearDialogueID);
    }
}

public static class GameFlowRegressionRuntimeCases
{
    public static IEnumerator MissingGameplayCameraDoesNotBreakHover()
    {
        var go = new GameObject("HoverWithoutGameplayCamera");
        try
        {
            Assert.IsNull(Camera.main);
            var hover = go.AddComponent<CameraRay>();
            var update = typeof(CameraRay).GetMethod("HandleMouseHover", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.DoesNotThrow(() => update.Invoke(hover, null));
            yield return null;
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }
    public static IEnumerator ProjectileResolvesOnlyOnePlayerHit()
    {
        using (var r = new MovementRig(true))
        {
            var go = new GameObject("OneHitProjectile"); var bullet = go.AddComponent<D1_Bullet>();
            var collider = r.Model.gameObject.AddComponent<CapsuleCollider>(); r.Model.gameObject.tag = "Player";
            try
            {
                float hp = r.Model.Stat.Stat.curHp;
                bullet.Init(.1f, 0f, r.Model, true);
                var trigger = typeof(D1_Bullet).GetMethod("OnTriggerEnter", BindingFlags.Instance | BindingFlags.NonPublic);
                trigger.Invoke(bullet, new object[] { collider }); trigger.Invoke(bullet, new object[] { collider });
                Assert.AreEqual(hp - r.Model.Stat.Stat.maxHp.FinalValue * .1f, r.Model.Stat.Stat.curHp, .001f);
                yield return null;
                Assert.IsTrue(bullet == null);
            }
            finally { if (go != null) UnityEngine.Object.DestroyImmediate(go); }
        }
    }
    public static IEnumerator BoxPatternYieldsAndEndsWhenGroundIsMissing()
    {
        using (var r = new FinalBossTransitionRig())
        {
            var controller = (AnimatorController)r.Animator.runtimeAnimatorController;
            controller.AddParameter("Normal1Start", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Normal1End", AnimatorControllerParameterType.Trigger);
            var pattern = new D1_Final_Normal1(new D1_Final_Normal1Data { prefab = r.Boss.gameObject, boxCount = 2, groundLayer = 0 }, r.Boss.center);
            var routine = (IEnumerator)typeof(D1_Final_Normal1).GetMethod("SpawnBox", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(pattern, new object[] { r.Boss, 0f });
            int yielded = 0, failedSamples = 0;
            while (routine.MoveNext())
            {
                yielded++;
                if (routine.Current == null) failedSamples++;
                Assert.LessOrEqual(yielded, 42, "Sampling must be bounded.");
                yield return routine.Current;
            }
            Assert.AreEqual(40, failedSamples, "Each failed sample must yield to Unity.");
            Assert.AreEqual(0, r.Boss.patternObjects.Count);
        }
    }
    public static IEnumerator SwingLastFrameCannotReviveDeadOrDestroyedEnemy()
    {
        for (int scenario = 0; scenario < 2; scenario++)
        {
            var data = ScriptableObject.CreateInstance<EnemyStatSO>(); data.maxHp = 10;
            var go = new GameObject("SwingBoundaryEnemy"); go.SetActive(false);
            var agent = go.AddComponent<NavMeshAgent>(); agent.enabled = false;
            var enemy = go.AddComponent<EnemyBase>(); enemy.statSO = data;
            var sectorGo = new GameObject("SwingBoundarySector"); var sector = sectorGo.AddComponent<EnemySector>();
            var landing = new GameObject("SwingBoundaryLanding");
            try
            {
                go.SetActive(true);
                yield return null;
                sector.swingDuration = Mathf.Max(.0001f, Time.deltaTime * .5f);
                var routine = (IEnumerator)typeof(EnemySector).GetMethod("ExecuteSwingMotion", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(sector, new object[] { enemy, new SpawnData { spawnPoint = landing.transform } });
                Assert.IsTrue(routine.MoveNext());
                if (scenario == 0) typeof(EnemyBase).GetField("_isDead", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(enemy, true);
                else UnityEngine.Object.DestroyImmediate(go);
                Assert.IsFalse(routine.MoveNext(), "Landing cannot continue after death/destruction on the final swing frame.");
                if (scenario == 0) { Assert.IsTrue(enemy.IsDead); Assert.IsFalse(agent.enabled); }
            }
            finally
            {
                if (go != null) UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(sectorGo); UnityEngine.Object.DestroyImmediate(landing); UnityEngine.Object.DestroyImmediate(data);
            }
        }
    }
    public static IEnumerator JumpReleasesOnlyLivingPlayersAndHandlesCancellation()
    {
        for (int scenario = 0; scenario < 4; scenario++)
        using (var r = new MovementRig(true))
        {
            var controller = (AnimatorController)r.Model.Anim.runtimeAnimatorController;
            controller.AddParameter("JumpEnd", AnimatorControllerParameterType.Trigger);
            var go = new GameObject("JumpBoundaryObject"); go.SetActive(false);
            var jump = go.AddComponent<JumpObject>();
            var endpoint = new GameObject("JumpBoundaryEnd"); endpoint.transform.position = r.Model.transform.position + Vector3.right;
            try
            {
                jump.targetPos = jump.apexPos = endpoint.transform;
                jump.jumpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f); jump.jumpDuration = .05f;
                jump.SetLock(false); go.SetActive(true); jump.EnableInteract(r.Model.transform);
                Assert.IsTrue(jump.Interact(r.Model.transform)); Assert.IsFalse(r.Model.canMove);
                Assert.IsFalse(jump.Interact(r.Model.transform), "Repeated input cannot start overlapping jumps.");
                if (scenario == 1) r.Model.isDie = true;
                if (scenario == 2) jump.gameObject.SetActive(false);
                if (scenario == 3) r.Model.Buff.isStun = true;
                yield return new WaitForSecondsRealtime(.15f);
                if (scenario == 1) { Assert.IsFalse(r.Agent.enabled); Assert.IsFalse(r.Model.canMove); }
                else if (scenario == 3) { Assert.IsFalse(r.Model.canMove); Assert.IsFalse((bool)typeof(CharacterModel).GetField("externalControlLocked", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(r.Model)); }
                else { Assert.IsTrue(r.Model.canMove); Assert.IsTrue(r.Agent.enabled); }
            }
            finally { UnityEngine.Object.DestroyImmediate(go); UnityEngine.Object.DestroyImmediate(endpoint); }
        }
    }
}
