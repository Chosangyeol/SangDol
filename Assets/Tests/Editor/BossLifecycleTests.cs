using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BossLifecyclePattern : BossPatternBase
{
    public BossLifecyclePattern() { cooldown = 100f; }
    public void MarkUsed() => lastUsedTime = Time.time;
    public bool Ready => Time.time - lastUsedTime >= cooldown;
}
public sealed class BossLifecycleRig : IDisposable
{
    public readonly BossModel Boss;
    public readonly BossLifecyclePattern Pattern = new BossLifecyclePattern();
    public bool LateAttack;
    public bool HasCurrentPattern => Get("currentPattern") != null;
    private object Get(string name) => typeof(BossModel).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Boss);
    public void Kill() => typeof(BossModel).GetMethod("Die", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Boss, new object[] { null });
    public IEnumerator ScheduledAttack() { yield return new WaitForSeconds(0.02f); LateAttack = true; }
    public void Contaminate() { var patterns = (List<BossPatternBase>)Get("normalPatterns"); patterns.Clear(); patterns.Add(Pattern); Pattern.MarkUsed(); typeof(BossModel).GetField("currentPattern", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Boss, Pattern); Boss.isDoingSpecial = Boss.isStatic = Boss.isImmunity = Boss.isKnockDown = true; Boss.EnableCounter(); Boss.specialPatterns = new List<BossSpecialPattern> { new BossSpecialPattern { hasDone = true } }; Boss.Stat.curHp = 1; Boss.Stat.curDown = 0; }
    public readonly PoolManager Pool;
    private readonly GameObject template, poolObject;
    private readonly EnemyStatSO data;
    private readonly PoolManager previousPool = PoolManager.Instance;
    private readonly Action<string> previousKill = GameEvent.OnMonsterKill;
    private readonly Action<BossModel> previousState = GameEvent.OnBossStateChange;
    public BossLifecycleRig(bool runtime)
    {
        GameEvent.OnMonsterKill = null; GameEvent.OnBossStateChange = null;
        template = new GameObject("LifecycleTestBoss"); template.SetActive(false);
        var source = template.AddComponent<BossModel>();
        Assert.IsNotNull(source, "Unity must resolve the fixture MonoScript."); source.enabled = false;
        data = ScriptableObject.CreateInstance<EnemyStatSO>(); data.enemyID = "test-boss"; data.maxHp = 100;
        source.statSO = data;
        typeof(BossModel).GetField("deathPresentationDelay", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(source, 0.05f);
        if (runtime)
        {
            typeof(PoolManager).GetProperty("Instance").SetValue(null, null);
            poolObject = new GameObject("LifecycleTestPool"); Pool = poolObject.AddComponent<PoolManager>();
            Pool.CreatePool(source, 1, false); Boss = (BossModel)Pool.Pop(template.name);
        }
        else { Boss = source; typeof(BossModel).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Boss, null); }
    }
    public bool Finished => (bool)typeof(BossModel).GetProperty("IsDeathSequenceFinished", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Boss);
    public void Dispose()
    {
        if (poolObject != null) UnityEngine.Object.DestroyImmediate(poolObject);
        UnityEngine.Object.DestroyImmediate(template); UnityEngine.Object.DestroyImmediate(data);
        typeof(PoolManager).GetProperty("Instance").SetValue(null, previousPool);
        GameEvent.OnMonsterKill = previousKill; GameEvent.OnBossStateChange = previousState;
    }
}
public static class BossLifecycleCases
{
    public static void ResetClearsEncounterFlagsAndCooldown()
    {
        using (var r = new BossLifecycleRig(false))
        {
            r.Contaminate(); Assert.IsFalse(r.Pattern.Ready); r.Boss.Reset();
            Assert.IsFalse(r.HasCurrentPattern || r.Boss.isDoingSpecial || r.Boss.isStatic || r.Boss.isImmunity || r.Boss.isKnockDown || r.Boss.CanCounter);
            Assert.IsFalse(r.Boss.specialPatterns[0].hasDone); Assert.IsTrue(r.Pattern.Ready);
            Assert.AreEqual(100, r.Boss.Stat.curHp); Assert.AreEqual(r.Boss.Stat.maxDown, r.Boss.Stat.curDown);
            Assert.IsTrue(r.Boss.isCombatStarted); Assert.IsFalse(r.Finished);
            r.Boss.isInField = true; r.Boss.Reset(); Assert.IsFalse(r.Boss.isCombatStarted);
        }
    }
    public static void ResetAllowsMissingOptionalData()
    {
        using (var r = new BossLifecycleRig(false))
        {
            r.Boss.specialPatterns = null; Assert.DoesNotThrow(() => r.Boss.Reset());
            Assert.AreEqual(100, r.Boss.Stat.curHp);
        }
    }
}
public static class BossLifecycleRuntimeCases
{
    public static IEnumerator DeathStopsAttacksAndWaitsBeforeReturn()
    {
        using (var r = new BossLifecycleRig(true))
        {
            r.Contaminate(); var hazard = new GameObject("TestBossHazard"); r.Boss.patternObjects.Add(hazard);
            r.Boss.StartCoroutine(r.ScheduledAttack());
            int kills = 0; GameEvent.OnMonsterKill = _ => kills++;
            r.Kill(); r.Kill(); r.Boss.OnPatternEnd();
            Assert.IsTrue(r.Boss.IsDead); Assert.IsTrue(r.Boss.gameObject.activeSelf); Assert.IsFalse(r.Finished);
            Assert.IsFalse(r.HasCurrentPattern || r.Boss.CanCounter || r.Boss.isDoingSpecial);
            yield return new WaitForSecondsRealtime(0.1f);
            Assert.IsTrue(hazard == null); Assert.IsFalse(r.LateAttack); Assert.AreEqual(1, kills);
            Assert.IsTrue(r.Finished); Assert.IsFalse(r.Boss.gameObject.activeSelf);
        }
    }
    public static IEnumerator PoolReuseResetsDeathAndSpecialState()
    {
        using (var r = new BossLifecycleRig(true))
        {
            r.Contaminate(); r.Kill(); yield return new WaitForSecondsRealtime(0.1f);
            var reused = r.Pool.Pop(r.Boss.name); Assert.AreSame(r.Boss, reused);
            Assert.IsFalse(r.Boss.IsDead || r.Finished || r.HasCurrentPattern);
            Assert.IsFalse(r.Boss.specialPatterns[0].hasDone); Assert.IsTrue(r.Pattern.Ready);
            Assert.AreEqual(100, r.Boss.Stat.curHp); Assert.IsTrue(r.Boss.isCombatStarted);
        }
    }
    public static IEnumerator PlayerDeathDuringDefeatCompletesOnce()
    {
        using (var r = new BossLifecycleRig(true))
        {
            int kills = 0; GameEvent.OnMonsterKill = _ => kills++;
            r.Kill(); r.Boss.ResetBossState(); r.Boss.ResetBossState();
            Assert.IsTrue(r.Finished); Assert.IsTrue(r.Boss.IsDead); Assert.IsFalse(r.Boss.gameObject.activeSelf);
            yield return new WaitForSecondsRealtime(0.1f); Assert.AreEqual(1, kills);
            Assert.AreSame(r.Boss, r.Pool.Pop(r.Boss.name));
            Assert.AreNotSame(r.Boss, r.Pool.Pop(r.Boss.name));
        }
    }
    public static IEnumerator SectorWaitsForPresentationAndRetainsCompletionAfterReuse()
    {
        using (var r = new BossLifecycleRig(true))
        {
            var go = new GameObject("LifecycleSector"); var sector = go.AddComponent<FinalBossSector>();
            try
            {
                sector.spawnDataList = new List<SpawnData> { new SpawnData() };
                var list = (List<EnemyBase>)typeof(FinalBossSector).GetField("_spawnedEnemies", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sector);
                list.Add(r.Boss);
                typeof(FinalBossSector).GetField("_isStarted", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sector, true);
                var completed = typeof(BossModel).GetEvent("DeathPresentationCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
                var callback = Delegate.CreateDelegate(completed.EventHandlerType, sector,
                    typeof(FinalBossSector).GetMethod("OnBossDefeated", BindingFlags.Instance | BindingFlags.NonPublic));
                completed.GetAddMethod(true).Invoke(r.Boss, new object[] { callback });
                r.Kill(); Assert.IsFalse(sector.IsSatisfied); Assert.AreEqual(0, sector.DeadEnemyCount);
                yield return new WaitForSecondsRealtime(0.1f);
                r.Pool.Pop(r.Boss.name); Assert.IsTrue(sector.IsSatisfied); Assert.AreEqual(1, sector.DeadEnemyCount);
                sector.spawnDataList.Add(new SpawnData()); list.Add(r.Boss);
                Assert.IsFalse(sector.IsSatisfied);
                r.Kill(); yield return new WaitForSecondsRealtime(0.1f);
                Assert.AreEqual(2, sector.DeadEnemyCount); Assert.IsTrue(sector.IsSatisfied);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
