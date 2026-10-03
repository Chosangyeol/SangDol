using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

// Kept in the predefined Editor assembly to test Assembly-CSharp without moving production scripts.
public class PlayerMovementTests
{
    private MovementRig rig;

    [SetUp] public void SetUp() { rig = new MovementRig(false); }
    [TearDown] public void TearDown() { rig.Dispose(); }

    [Test] public void SkillWaitsForSkillPermissionEvenWhenMovementIsAllowed()
    {
        rig.Model.canSkill = false;
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        Assert.AreEqual(0, rig.Probe.Uses);
        rig.Model.canSkill = true;
        rig.Skills.UpdateSkills(0.01f);
        Assert.AreEqual(1, rig.Probe.Uses);
        rig.Skills.UpdateSkills(0.01f);
        Assert.AreEqual(1, rig.Probe.Uses);
    }

    [Test] public void BufferedSkillExpiresInsteadOfFiringLate()
    {
        rig.Model.canMove = false;
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        rig.Skills.UpdateSkills(0.2f);
        rig.Model.canMove = true;
        rig.Skills.UpdateSkills(0.01f);
        Assert.AreEqual(0, rig.Probe.Uses);
    }

    [Test] public void BufferedChargePreservesReleaseBeforeActivation()
    {
        rig.Model.canMove = false;
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        rig.Skills.ReleaseSkill(C_Enums.SkillSlot.Q, Vector3.right);
        rig.Model.canMove = true;
        rig.Skills.UpdateSkills(0.01f);
        Assert.AreEqual(1, rig.Probe.Uses);
        Assert.AreEqual(1, rig.Probe.Releases);
        Assert.AreEqual(Vector3.right, rig.Probe.LastTarget);
        Assert.IsFalse(rig.Probe.isCharging);
    }

    [TestCase(true)] [TestCase(false)]
    public void DeathOrStunDiscardsBufferedSkill(bool death)
    {
        rig.Model.canMove = false;
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        rig.Model.isDie = death;
        rig.Model.Buff.isStun = !death;
        rig.Skills.UpdateSkills(0.01f);
        rig.Model.isDie = false;
        rig.Model.Buff.isStun = false;
        rig.Model.canMove = true;
        rig.Skills.UpdateSkills(0.01f);
        Assert.AreEqual(0, rig.Probe.Uses);
    }

    [Test] public void SlotReplacementDoesNotExecuteOldBufferedSkill()
    {
        rig.Model.canMove = false;
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        rig.Skills.ClearSkillSlot(C_Enums.SkillSlot.Q);
        rig.Model.canMove = true;
        rig.Skills.UpdateSkills(0.01f);
        Assert.AreEqual(0, rig.Probe.Uses);
    }

    [Test] public void NewestBufferedInputWins()
    {
        var second = new MovementProbeSkill(rig.Model, rig.Model.skill_SpaceSO);
        rig.Skills.RegisterSkillToSlot(C_Enums.SkillSlot.W, second);
        rig.Model.canMove = false;
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        rig.Skills.UseSkill(C_Enums.SkillSlot.W, Vector3.right);
        rig.Model.canMove = true;
        rig.Skills.UpdateSkills(0.01f);
        Assert.AreEqual(0, rig.Probe.Uses);
        Assert.AreEqual(1, second.Uses);
    }

    [Test] public void InactiveAgentRejectsMovementWithoutChangingRotation()
    {
        Quaternion before = rig.Model.transform.rotation;
        Assert.DoesNotThrow(() => rig.Controller.RequestMove(Vector3.right * 5f));
        Assert.AreEqual(before, rig.Model.transform.rotation);
        Assert.DoesNotThrow(() => rig.Controller.StopMove());
    }

    [Test] public void SkillReleaseWithoutCameraStillEndsCharge()
    {
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        rig.Model.mainCam = null;
        new C_Input(rig.Model, rig.Controller).OnSkillKeyUp(C_Enums.SkillSlot.Q, Vector2.zero);
        Assert.AreEqual(1, rig.Probe.Releases);
    }

    [Test] public void DodgeRejectsInactiveAgentWithoutConsumingCooldown()
    {
        Assert.IsFalse(rig.Skills.DodgeSkill.UseSkill(Vector3.forward));
        Assert.IsTrue(rig.Skills.DodgeSkill.canUse);
        Assert.IsTrue(rig.Model.canMove);
    }

    [Test] public void IdenFinisherCompletionRestoresNormalAttack()
    {
        rig.Model.isWaitingForRelease = true;
        rig.Model.canAttack = false;
        rig.Model.canMove = false;
        rig.Controller.isAttackHeld = true;
        rig.Controller.isAttacking = true;
        typeof(CharacterModel).GetMethod("OnIdenAttackFinished", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(rig.Model, null);
        Assert.IsFalse(rig.Model.isWaitingForRelease);
        Assert.IsTrue(rig.Model.canAttack && rig.Model.canMove);
        Assert.IsFalse(rig.Controller.isAttacking);
        rig.Controller.RequestBasicAttack(true, Vector3.forward);
        Assert.IsTrue(rig.Controller.isAttacking);
    }

    [TestCase(true)] [TestCase(false)]
    public void IdenFinisherDoesNotRestoreDeadOrDisabledControls(bool dead)
    {
        rig.Model.canAttack = false;
        rig.Model.canMove = false;
        rig.Model.isDie = dead;
        rig.Agent.enabled = dead;
        typeof(CharacterModel).GetMethod("OnIdenAttackFinished", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(rig.Model, null);
        Assert.IsFalse(rig.Model.canAttack || rig.Model.canMove);
    }

    [Test] public void InterruptedChargePreservesCooldownAndPreviousPermissions()
    {
        var skill = new InterruptibleMovementProbe(rig.Model, rig.Model.skill_SpaceSO);
        rig.Skills.RegisterSkillToSlot(C_Enums.SkillSlot.Q, skill);
        rig.Model.canAttack = false;
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        float cooldown = skill.nowCoolTime;
        rig.Model.Buff.isStun = true;
        rig.Skills.UpdateSkills(0f);
        rig.Skills.ReleaseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        Assert.IsFalse(skill.isCharging);
        Assert.AreEqual(0, skill.Releases);
        Assert.AreEqual(cooldown, skill.nowCoolTime);
        Assert.IsFalse(rig.Model.canMove || rig.Model.canAttack || rig.Model.canSkill);
        rig.Model.Buff.isStun = false;
        rig.Skills.UpdateSkills(0.1f);
        Assert.IsTrue(rig.Model.canMove && rig.Model.canSkill);
        Assert.IsFalse(rig.Model.canAttack, "Recovery must preserve an earlier attack restriction.");
        Assert.AreEqual(cooldown - 0.1f, skill.nowCoolTime, 0.001f);
    }

    [Test] public void DeathCancelsChargeWithoutRestoringControls()
    {
        var skill = new InterruptibleMovementProbe(rig.Model, rig.Model.skill_SpaceSO);
        rig.Skills.RegisterSkillToSlot(C_Enums.SkillSlot.Q, skill);
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        rig.Model.isDie = true;
        rig.Skills.UpdateSkills(0f);
        rig.Skills.ReleaseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        rig.Model.SetCanMove(); rig.Model.SetCanAttack(); rig.Model.SetCanSkill();
        Assert.IsFalse(skill.isCharging);
        Assert.AreEqual(0, skill.Releases);
        Assert.IsFalse(rig.Model.canMove || rig.Model.canAttack || rig.Model.canSkill);
    }

    [Test] public void ExternalLockRejectsLateRecoveryEvents()
    {
        var skill = new InterruptibleMovementProbe(rig.Model, rig.Model.skill_SpaceSO);
        rig.Skills.RegisterSkillToSlot(C_Enums.SkillSlot.Q, skill);
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        rig.Model.SetControlable(false);
        rig.Model.SetCanMove(); rig.Model.SetCanAttack(); rig.Model.SetCanSkill();
        Assert.IsFalse(skill.isCharging);
        Assert.IsFalse(rig.Model.canMove || rig.Model.canAttack || rig.Model.canSkill);
        Assert.IsFalse(rig.Agent.enabled);
    }

    [Test] public void PreviouslyInterruptedSlotDoesNotCancelNewSkill()
    {
        var oldSkill = new InterruptibleMovementProbe(rig.Model, rig.Model.skill_SpaceSO);
        var newSkill = new InterruptibleMovementProbe(rig.Model, rig.Model.skill_SpaceSO);
        rig.Skills.RegisterSkillToSlot(C_Enums.SkillSlot.Q, oldSkill);
        rig.Skills.RegisterSkillToSlot(C_Enums.SkillSlot.W, newSkill);
        rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
        rig.Model.Buff.isStun = true;
        rig.Skills.UpdateSkills(0f);
        rig.Model.Buff.isStun = false;
        rig.Skills.UpdateSkills(0f);
        rig.Skills.UseSkill(C_Enums.SkillSlot.W, Vector3.forward);
        rig.Skills.UpdateSkills(0.01f);
        Assert.IsTrue(newSkill.isCharging);
        Assert.IsFalse(rig.Model.canMove);
    }
}

public sealed class InterruptibleMovementProbe : SkillBase
{
    public int Releases;
    public InterruptibleMovementProbe(CharacterModel model, SkillBaseSO data) : base(model, data) { }
    public override bool UseSkill(Vector3 target)
    {
        if (!base.UseSkill(target)) return false;
        BeginExecution();
        isCharging = true;
        _model.canMove = _model.canAttack = _model.canSkill = false;
        return true;
    }
    public override void ReleaseSkill(Vector3 target)
    {
        if (!isCharging || !CanContinueExecution) return;
        Releases++;
        isCharging = false;
    }
}

public sealed class SkillInterruptionDamageProbe : EnemyBase
{
    public int Hits;
    protected override void Awake() { }
    protected override void Start() { }
    public override void Damaged(SDamageInfo info) { Hits++; }
}

public sealed class MovementProbeSkill : SkillBase
{
    public int Uses, Releases;
    public Vector3 LastTarget;
    public MovementProbeSkill(CharacterModel model, SkillBaseSO data) : base(model, data) { }
    public override bool UseSkill(Vector3 target)
    {
        if (!canUse) return false;
        Uses++;
        LastTarget = target;
        isCharging = true;
        canUse = false;
        return true;
    }
    public override void ReleaseSkill(Vector3 target)
    {
        if (!isCharging) return;
        Releases++;
        LastTarget = target;
        isCharging = false;
    }
    public override void UpdateSkill(float deltaTime) { }
}

public sealed class MovementRig : IDisposable
{
    public readonly CharacterModel Model;
    public readonly NavMeshAgent Agent;
    public readonly C_Controller Controller;
    public readonly C_SkillSystem Skills;
    public readonly MovementProbeSkill Probe;
    private readonly GameObject actor;
    private readonly List<UnityEngine.Object> assets = new List<UnityEngine.Object>();
    private NavMeshDataInstance navigation;

    public MovementRig(bool runtime)
    {
        Vector3 origin = runtime ? new Vector3(1000f, 0f, 1000f) : Vector3.zero;
        if (runtime)
        {
            var source = new NavMeshBuildSource { shape = NavMeshBuildSourceShape.Box,
                transform = Matrix4x4.TRS(origin + Vector3.down * 0.5f, Quaternion.identity, Vector3.one),
                size = new Vector3(40f, 1f, 40f), area = 0 };
            var data = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByIndex(0),
                new List<NavMeshBuildSource> { source }, new Bounds(origin, new Vector3(50f, 20f, 50f)),
                Vector3.zero, Quaternion.identity);
            Assert.IsNotNull(data);
            assets.Add(data);
            navigation = NavMesh.AddNavMeshData(data);
        }

        actor = new GameObject("MovementTestActor");
        actor.SetActive(false);
        actor.transform.position = origin;
        Agent = actor.AddComponent<NavMeshAgent>();
        Agent.speed = 7f;
        Agent.acceleration = 100f;
        Agent.stoppingDistance = 0.1f;
        var animator = actor.AddComponent<Animator>();
        var controller = new AnimatorController();
        controller.AddLayer("Base Layer");
        controller.AddParameter("Move", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Combo", AnimatorControllerParameterType.Int);
        controller.AddParameter("Skill_Space", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Skill2", AnimatorControllerParameterType.Trigger);
        controller.layers[0].stateMachine.AddState("Idle");
        assets.Add(controller);
        animator.runtimeAnimatorController = controller;
        Model = actor.AddComponent<CharacterModel>();
        actor.AddComponent<PlayerAttackContainer>();
        Model.enabled = false; // Awake initializes the real subsystems; no unrelated camera/UI Start or Update.
        Model.characterStatSO = ScriptableObject.CreateInstance<CharacterStatSO>();
        Model.skill_ZSO = ScriptableObject.CreateInstance<Skill_ZSO>();
        Model.skill_SpaceSO = ScriptableObject.CreateInstance<Skill_SpaceSO>();
        Model.skill_SpaceSO.skillCool = 3f;
        assets.Add(Model.characterStatSO);
        assets.Add(Model.skill_ZSO);
        assets.Add(Model.skill_SpaceSO);

        if (runtime)
        {
            actor.SetActive(true);
            Assert.IsTrue(Agent.isOnNavMesh);
            Controller = Model.PlayerController;
            Skills = Model.SkillSystem;
        }
        else
        {
            Set("navMesh", Agent);
            Set("anim", null); // No active Animator in the data-only EditMode fixture.
            Set("buff", new C_Buff(Model));
            Set("stat", new C_Stat(Model, Model.characterStatSO));
            Controller = new C_Controller(Model);
            Set("playerController", Controller);
            Skills = new C_SkillSystem(Model);
            Set("skillSystem", Skills);
        }
        Probe = new MovementProbeSkill(Model, Model.skill_SpaceSO);
        Skills.RegisterSkillToSlot(C_Enums.SkillSlot.Q, Probe);
    }

    private void Set(string name, object value) => typeof(CharacterModel)
        .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Model, value);

    public void Dispose()
    {
        if (Model != null) Model.StopAllCoroutines();
        UnityEngine.Object.DestroyImmediate(actor);
        if (navigation.valid) navigation.Remove();
        foreach (var asset in assets) if (asset != null) UnityEngine.Object.DestroyImmediate(asset);
    }
}

public sealed class RegularSkillTestRig : IDisposable
{
    public readonly MovementRig Movement = new MovementRig(true);
    public readonly SkillBase Skill;
    public readonly SkillBaseSO Data;
    public readonly SkillInterruptionDamageProbe Enemy;
    private readonly PoolManager previousPool;
    private readonly AudioManager previousAudio;
    private readonly GameObject poolObject, enemyObject;
    private readonly List<GameObject> prefabs = new List<GameObject>();
    private readonly List<AnimationClip> clips = new List<AnimationClip>();
    private readonly AnimatorOverrideController controller;

    public RegularSkillTestRig(int number)
    {
        previousAudio = AudioManager.instance;
        AudioManager.instance = null; // Isolate sound; exercise actual skill code, control events, physics and VFX leases.
        previousPool = PoolManager.Instance;
        typeof(PoolManager).GetProperty("Instance").SetValue(null, null);
        poolObject = new GameObject("RegularSkillTestPool");
        var pool = poolObject.AddComponent<PoolManager>();
        var effects = new PoolableMono[3];
        for (int i = 0; i < effects.Length; i++)
        {
            var prefab = new GameObject("RegularSkillTestEffect" + i);
            prefab.SetActive(false);
            prefabs.Add(prefab);
            effects[i] = prefab.AddComponent<PoolableMono>();
            pool.CreatePool(effects[i], 1, false);
        }
        Data = number == 1 ? (SkillBaseSO)ScriptableObject.CreateInstance<Skill_1SO>() :
            number == 3 ? ScriptableObject.CreateInstance<Skill_3SO>() : ScriptableObject.CreateInstance<Skill_4SO>();
        Data.skillEffects = effects;
        Data.skillCool = 5f;
        Data.maxLevel = 1;
        Data.isChargeSkill = number != 1;
        Data.damageMultipliers = new[] { 1f };
        Data.chargeStageTimes = new float[0];
        Data.chargeDamageMultipliers = new float[0];
        Data.chargeScaleMultipliers = new float[0];
        Skill = Data.SkillInit(Movement.Model);
        Movement.Skills.RegisterSkillToSlot(C_Enums.SkillSlot.Q, Skill);
        var source = AssetDatabase.LoadAssetAtPath<AnimatorController>(
            "Assets/06. Animations/Character/PlayerAnimController.controller");
        controller = new AnimatorOverrideController(source);
        var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        controller.GetOverrides(overrides);
        for (int i = 0; i < overrides.Count; i++)
        {
            var clone = UnityEngine.Object.Instantiate(overrides[i].Key);
            clips.Add(clone);
            var events = new List<AnimationEvent>();
            foreach (var e in AnimationUtility.GetAnimationEvents(clone))
                if (e.functionName.StartsWith("SetCan") || e.functionName.StartsWith("SetCant") ||
                    e.functionName.StartsWith("OnSkillCan") || e.functionName.StartsWith("OnSkillCant") ||
                    e.functionName == "AnimEvent_ExecuteManagedSkillAttack" ||
                    e.functionName == "AnimEvent_ExevuteSkillAttack") events.Add(e);
            AnimationUtility.SetAnimationEvents(clone, events.ToArray());
            overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, clone);
        }
        controller.ApplyOverrides(overrides);
        Movement.Model.Anim.runtimeAnimatorController = controller;
        Movement.Model.Anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        Movement.Model.Anim.speed = 3f;
        Movement.Model.Anim.SetFloat("AttackSpeed", 1f);
        Movement.Model.Anim.Play("Base Layer.Idle", 0, 0f);
        Movement.Model.Anim.Update(0f);
        enemyObject = new GameObject("RegularSkillDamageTarget");
        enemyObject.layer = LayerMask.NameToLayer("Enemy");
        enemyObject.transform.position = Movement.Model.transform.position + Vector3.forward * 2f;
        enemyObject.AddComponent<BoxCollider>();
        Enemy = enemyObject.AddComponent<SkillInterruptionDamageProbe>();
        Physics.SyncTransforms();
    }

    public int EffectCount(int index)
    {
        var pools = (System.Collections.IDictionary)typeof(PoolManager)
            .GetField("_pools", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(PoolManager.Instance);
        object pool = pools[prefabs[index].name];
        var stack = (System.Collections.ICollection)pool.GetType()
            .GetField("_pool", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pool);
        return stack.Count;
    }

    public void Use() => Movement.Skills.UseSkill(C_Enums.SkillSlot.Q, Enemy.transform.position);
    public void Release() => Movement.Skills.ReleaseSkill(C_Enums.SkillSlot.Q, Enemy.transform.position);
    public void Dispose()
    {
        typeof(SkillBase).GetMethod("InterruptExecution", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Skill, null);
        Movement.Model.Anim.runtimeAnimatorController = null;
        Movement.Dispose();
        UnityEngine.Object.DestroyImmediate(enemyObject);
        UnityEngine.Object.DestroyImmediate(poolObject);
        foreach (var prefab in prefabs) UnityEngine.Object.DestroyImmediate(prefab);
        UnityEngine.Object.DestroyImmediate(controller);
        foreach (var clip in clips) UnityEngine.Object.DestroyImmediate(clip);
        UnityEngine.Object.DestroyImmediate(Data);
        typeof(PoolManager).GetProperty("Instance").SetValue(null, previousPool);
        AudioManager.instance = previousAudio;
    }
}

// PlayMode tests invoke this bridge by reflection because asmdef assemblies cannot reference Assembly-CSharp.
public static class PlayerMovementRuntimeCases
{
    public static IEnumerator Skill1StunStopsDashAndRecovers() => InterruptRegularSkill(1);
    public static IEnumerator Skill3StunCancelsChargeAndLateRelease() => InterruptRegularSkill(3);
    public static IEnumerator Skill4StunStopsHoldingAndReturnsEffectOnce() => InterruptRegularSkill(4);
    private static IEnumerator InterruptRegularSkill(int number)
    {
        using (var rig = new RegularSkillTestRig(number))
        {
            var model = rig.Movement.Model;
            rig.Use();
            yield return null;
            float cooldown = rig.Skill.nowCoolTime;
            model.StunEnable();
            int hits = rig.Enemy.Hits;
            Vector3 stopped = model.transform.position;
            rig.Release(); rig.Release();
            model.SetCanMove(); model.SetCanAttack(); model.SetCanSkill();
            Assert.IsFalse(rig.Skill.isCharging);
            Assert.AreEqual(cooldown, rig.Skill.nowCoolTime);
            Assert.IsFalse(model.canMove || model.canAttack || model.canSkill);
            Assert.IsNull(model.GetComponent<PlayerAttackContainer>().currentSkill);
            yield return new WaitForSeconds(number == 4 ? 3.1f : 0.8f);
            Assert.AreEqual(hits, rig.Enemy.Hits, "Cancelled skills must not cause later damage.");
            Assert.Less(Vector3.Distance(stopped, model.transform.position), 0.05f);
            Assert.AreEqual(1, rig.EffectCount(0), "The effect must be returned exactly once.");
            Assert.AreEqual(1, rig.EffectCount(1)); Assert.AreEqual(1, rig.EffectCount(2));
            model.StunDisable();
            Assert.IsTrue(model.canMove && model.canAttack && model.canSkill);
            rig.Movement.Controller.RequestMove(stopped + Vector3.right * 5f);
            Assert.IsFalse(rig.Movement.Agent.isStopped);
            float timeout = Time.time + 1f;
            while (!model.Anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && Time.time < timeout) yield return null;
            rig.Movement.Controller.RequestBasicAttack(true, rig.Enemy.transform.position);
            timeout = Time.time + 1f;
            while (!model.Anim.GetCurrentAnimatorStateInfo(0).IsName("Atk1") && Time.time < timeout) yield return null;
            Assert.IsTrue(model.Anim.GetCurrentAnimatorStateInfo(0).IsName("Atk1"));
        }
    }

    public static IEnumerator Skill3ReleasedCastCanStillBeInterrupted()
    {
        using (var rig = new RegularSkillTestRig(3))
        {
            rig.Use(); rig.Release();
            Assert.IsFalse(rig.Skill.isCharging);
            rig.Movement.Model.StunEnable();
            yield return new WaitForSeconds(0.4f);
            Assert.AreEqual(0, rig.Enemy.Hits);
            Assert.AreEqual(1, rig.EffectCount(0));
            rig.Movement.Model.StunDisable();
            Assert.IsTrue(rig.Movement.Model.canMove && rig.Movement.Model.canSkill);
        }
    }

    public static IEnumerator SkillControlLockSurvivesStunAndLateEvents()
    {
        using (var rig = new RegularSkillTestRig(3))
        {
            rig.Use();
            var model = rig.Movement.Model;
            model.StunEnable();
            model.ControlDisable();
            model.SetCanMove(); model.SetCanAttack(); model.SetCanSkill();
            model.StunDisable();
            Assert.IsFalse(model.canMove || model.canAttack || model.canSkill || rig.Movement.Agent.enabled);
            model.StunEnable();
            model.SetControlable(true);
            Assert.IsFalse(model.canMove || model.canAttack || model.canSkill);
            model.StunDisable();
            Assert.IsTrue(model.canMove && model.canAttack && model.canSkill);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(0, rig.Enemy.Hits);
        }
    }

    public static IEnumerator SkillDeathUsesExistingCooldownResetPolicy()
    {
        using (var rig = new RegularSkillTestRig(4))
        {
            rig.Use();
            typeof(CharacterModel).GetMethod("Die", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(rig.Movement.Model, null);
            Assert.IsTrue(rig.Movement.Model.isDie);
            Assert.IsFalse(rig.Skill.isCharging);
            Assert.IsTrue(rig.Skill.canUse);
            Assert.AreEqual(0f, rig.Skill.nowCoolTime);
            Assert.IsFalse(rig.Movement.Model.canMove || rig.Movement.Model.canAttack || rig.Movement.Model.canSkill);
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual(0, rig.Enemy.Hits);
            Assert.AreEqual(1, rig.EffectCount(0));
        }
    }

    public static IEnumerator SkillExternalLockExitsChargeAnimation()
    {
        using (var rig = new RegularSkillTestRig(3))
        {
            rig.Use();
            yield return new WaitForSeconds(0.1f);
            var model = rig.Movement.Model;
            model.ControlDisable();
            rig.Release();
            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(model.canMove || model.canAttack || model.canSkill);
            Assert.IsTrue(model.Anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
            model.ControlEnable();
            Assert.IsTrue(model.canMove && model.canAttack && model.canSkill);
            rig.Movement.Controller.RequestBasicAttack(true, rig.Enemy.transform.position);
            float timeout = Time.time + 1f;
            while (!model.Anim.GetCurrentAnimatorStateInfo(0).IsName("Atk1") && Time.time < timeout) yield return null;
            Assert.IsTrue(model.Anim.GetCurrentAnimatorStateInfo(0).IsName("Atk1"));
            Assert.AreEqual(0, rig.Enemy.Hits);
        }
    }

    public static IEnumerator Skill4ReturnedEffectDoesNotTouchNewOwner()
    {
        using (var rig = new RegularSkillTestRig(4))
        {
            rig.Use(); rig.Release();
            PoolableMono reused = PoolManager.Instance.Pop(rig.Data.skillEffects[0].name);
            try
            {
                reused.transform.SetParent(rig.Enemy.transform);
                reused.transform.localScale = Vector3.one * 2f;
                yield return new WaitForSeconds(3.1f);
                Assert.AreSame(rig.Enemy.transform, reused.transform.parent);
                Assert.AreEqual(Vector3.one * 2f, reused.transform.localScale);
                Assert.IsTrue(reused.gameObject.activeSelf);
                Assert.AreEqual(0, rig.EffectCount(0), "The old coroutine must not return a new owner's lease.");
            }
            finally { PoolManager.Instance.Push(reused); }
        }
    }

    public static IEnumerator OldSkillAnimationCannotUnlockNewCast()
    {
        using (var rig = new RegularSkillTestRig(3))
        {
            var data = ScriptableObject.CreateInstance<Skill_4SO>();
            try
            {
                rig.Use();
                rig.Movement.Model.ControlDisable();
                rig.Movement.Model.ControlEnable();
                data.skillEffects = rig.Data.skillEffects;
                data.skillCool = 5f; data.maxLevel = 1;
                data.isChargeSkill = true; data.damageMultipliers = new[] { 1f };
                var next = new Skill_4(rig.Movement.Model, data);
                rig.Movement.Skills.RegisterSkillToSlot(C_Enums.SkillSlot.W, next);
                rig.Movement.Skills.UseSkill(C_Enums.SkillSlot.W, rig.Enemy.transform.position);
                var model = rig.Movement.Model;
                // Deliver real callbacks from the previous skill's clip while the new cast owns the controls.
                model.Anim.Play("Base Layer.Skill_3_Shot", 0, 0f);
                yield return new WaitForSeconds(0.3f);
                Assert.IsTrue(next.isCharging);
                Assert.IsFalse(model.canMove || model.canAttack || model.canSkill);
                Assert.AreEqual(0, rig.Enemy.Hits);
                model.ControlDisable();
            }
            finally { UnityEngine.Object.DestroyImmediate(data); }
        }
    }

    public static IEnumerator Skill4DeathAfterRecoveryCancelsDelayedEffect()
    {
        using (var rig = new RegularSkillTestRig(4))
        {
            rig.Data.hasPerfectZone = true;
            rig.Data.perfectZoneStart = 0f;
            rig.Data.perfectZoneEnd = 1f;
            rig.Use();
            yield return new WaitForSeconds(0.1f);
            rig.Release();
            Assert.IsTrue(rig.Skill.isPerfectCharge);
            var model = rig.Movement.Model;
            float timeout = Time.time + 1f;
            while (!model.canSkill && Time.time < timeout) yield return null;
            Assert.IsTrue(model.canSkill, "Exercise death after normal animation recovery.");
            Assert.AreEqual(1, rig.EffectCount(1), "The delayed effect has not spawned yet.");
            typeof(CharacterModel).GetMethod("Die", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(model, null);
            yield return new WaitForSeconds(1.4f);
            Assert.AreEqual(1, rig.EffectCount(1));
            Assert.IsFalse(model.canMove || model.canAttack || model.canSkill);
        }
    }

    public static IEnumerator Skill1NormalRecoveryStillWorks() => NormalRegularSkill(1);
    public static IEnumerator Skill3NormalRecoveryStillDealsDamage() => NormalRegularSkill(3);
    public static IEnumerator Skill4NormalRecoveryStillWorks() => NormalRegularSkill(4);
    private static IEnumerator NormalRegularSkill(int number)
    {
        using (var rig = new RegularSkillTestRig(number))
        {
            rig.Use();
            if (number != 1)
            {
                yield return new WaitForSeconds(0.1f);
                rig.Release();
            }
            float timeout = Time.time + 3f;
            var model = rig.Movement.Model;
            while ((!model.canMove || !model.canAttack || !model.canSkill) && Time.time < timeout) yield return null;
            Assert.IsTrue(model.canMove && model.canAttack && model.canSkill,
                "Normal animation recovery must still unlock controls for Skill" + number);
            if (number != 4)
            {
                timeout = Time.time + 1f;
                while (rig.Enemy.Hits == 0 && Time.time < timeout) yield return null;
                Assert.Greater(rig.Enemy.Hits, 0,
                    "Normal damage events must be retained; current clips=" +
                    string.Join(",", Array.ConvertAll(model.Anim.GetCurrentAnimatorClipInfo(0), c => c.clip.name)));
            }
        }
    }

    public static IEnumerator IdenFinisherResumesHeldBasicAttack() => FinishIdentityAttack(false);
    public static IEnumerator IdenFinisherResumesReleasedBasicAttack() => FinishIdentityAttack(true);
    public static IEnumerator IdenFinisherInterruptedByStunResumesBasicAttack() => FinishIdentityAttack(false, true);

    private static IEnumerator WaitForIdentityExpiry()
    {
        yield return new WaitForSeconds(100f);
    }

    private static IEnumerator FinishIdentityAttack(bool releaseDuringFinisher, bool stunDuringFinisher = false)
    {
        using (var rig = new MovementRig(true))
        {
            var source = AssetDatabase.LoadAssetAtPath<AnimatorController>(
                "Assets/06. Animations/Character/PlayerAnimController.controller");
            var controller = new AnimatorOverrideController(source);
            var clips = new List<AnimationClip>();
            try
            {
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                controller.GetOverrides(overrides);
                for (int i = 0; i < overrides.Count; i++)
                {
                    var clone = UnityEngine.Object.Instantiate(overrides[i].Key);
                    clips.Add(clone);
                    var events = new List<AnimationEvent>();
                    // Keep actual state/control events; isolate audio, VFX and damage dependencies.
                    foreach (var e in AnimationUtility.GetAnimationEvents(clone))
                        if (e.functionName == "SetCantMove" || e.functionName == "SetCantAttack"
                            || e.functionName == "SetCanMove" || e.functionName == "SetCanAttack"
                            || e.functionName == "RemoveIdenAura" || e.functionName == "OnIdenAttackFinished")
                            events.Add(e);
                    AnimationUtility.SetAnimationEvents(clone, events.ToArray());
                    overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, clone);
                }
                controller.ApplyOverrides(overrides);
                var animator = rig.Model.Anim;
                animator.runtimeAnimatorController = controller;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.speed = 5f;
                animator.SetFloat("AttackSpeed", 1f);
                animator.SetBool("IsIden", true);
                animator.Play("Base Layer.IdenAttacking", 0, 0f);
                animator.Update(0f);
                rig.Model.isIdenOn = true;
                rig.Model.canAttack = false;
                rig.Model.canMove = false;
                rig.Controller.isAttackHeld = true;
                rig.Controller.isAttacking = true;
                typeof(CharacterModel).GetField("attackCoroutine", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(rig.Model, rig.Model.StartCoroutine(WaitForIdentityExpiry()));
                rig.Model.IdenDisable();
                Assert.IsTrue(rig.Model.isWaitingForRelease);
                if (releaseDuringFinisher)
                {
                    rig.Controller.RequestBasicAttack(false, Vector3.forward);
                    Assert.IsTrue(rig.Model.isWaitingForRelease, "Releasing must not cancel the final strike.");
                }
                if (stunDuringFinisher)
                {
                    rig.Model.StunEnable();
                    Assert.IsFalse(rig.Model.isWaitingForRelease);
                    // A finisher event can arrive while the Animator is blending into Stun.
                    typeof(CharacterModel).GetMethod("OnIdenAttackFinished", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(rig.Model, null);
                    Assert.IsFalse(rig.Model.canAttack || rig.Model.canMove);
                    float stunTimeout = Time.time + 1f;
                    while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Stun") && Time.time < stunTimeout)
                        yield return null;
                    Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Stun"));
                    Assert.IsFalse(rig.Model.canAttack);
                    rig.Model.StunDisable();
                }
                float timeout = Time.time + 3f;
                while ((rig.Model.isWaitingForRelease || !animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    && Time.time < timeout)
                    yield return null;
                Assert.IsFalse(rig.Model.isWaitingForRelease,
                    $"state={animator.GetCurrentAnimatorStateInfo(0).shortNameHash}, normalized={animator.GetCurrentAnimatorStateInfo(0).normalizedTime}, clips={string.Join(",", Array.ConvertAll(animator.GetCurrentAnimatorClipInfo(0), c => c.clip.name))}");
                Assert.IsTrue(rig.Model.canAttack && rig.Model.canMove);
                Assert.IsFalse(rig.Controller.isAttacking);
                Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"), "Finisher must leave IdenEnd.");
                rig.Controller.RequestBasicAttack(true, rig.Model.transform.position + Vector3.forward);
                Assert.IsTrue(rig.Controller.isAttacking);
                timeout = Time.time + 1f;
                while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Atk1") && Time.time < timeout)
                    yield return null;
                Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("Atk1"),
                    "A normal attack must actually enter its Animator state after the finisher.");
            }
            finally
            {
                rig.Model.Anim.runtimeAnimatorController = null;
                UnityEngine.Object.DestroyImmediate(controller);
                foreach (var clip in clips) UnityEngine.Object.DestroyImmediate(clip);
            }
        }
    }

    public static IEnumerator MoveClickDuringAttackLockStartsAfterAttackRecovers()
    {
        using (var rig = new MovementRig(true))
        {
            Vector3 destination = rig.Model.transform.position + Vector3.right * 8f;
            rig.Model.canMove = false;
            rig.Controller.isAttacking = true;

            rig.Controller.RequestMove(destination);

            Assert.IsFalse(rig.Agent.hasPath);

            rig.Model.canMove = true;
            rig.Controller.isAttacking = false;
            rig.Controller.Tick();

            Assert.IsFalse(rig.Agent.isStopped);
            yield return null;
            Assert.IsTrue(rig.Agent.hasPath);
            Assert.Less(Vector3.Distance(rig.Agent.destination, destination), 0.2f);
        }

        yield return null;
    }

    public static IEnumerator JumpLandsAndResumesMovement()
    {
        using (var rig = new MovementRig(true))
        {
            var data = ScriptableObject.CreateInstance<Skill_2SO>();
            try
            {
                var jump = new Skill_2(rig.Model, data);
                Vector3 target = rig.Model.transform.position + Vector3.forward * 5f;
                Assert.IsTrue(jump.UseSkill(target));
                yield return new WaitForSeconds(1f);
                Assert.IsTrue(rig.Agent.isOnNavMesh);
                Assert.IsTrue(rig.Model.canMove && rig.Model.canAttack && rig.Model.canSkill);
                Assert.Less(Vector3.Distance(target, rig.Model.transform.position), 0.15f);
                rig.Controller.RequestMove(target + Vector3.right * 3f);
                Assert.IsFalse(rig.Agent.isStopped);
            }
            finally { UnityEngine.Object.DestroyImmediate(data); }
        }
    }

    public static IEnumerator BufferedSkillExpiresWhileModelIsNotUpdating()
    {
        using (var rig = new MovementRig(true))
        {
            rig.Model.canMove = false;
            rig.Skills.UseSkill(C_Enums.SkillSlot.Q, Vector3.forward);
            yield return new WaitForSeconds(0.22f);
            rig.Model.canMove = true;
            rig.Skills.UpdateSkills(0.01f);
            Assert.AreEqual(0, rig.Probe.Uses);
        }
    }

    public static IEnumerator DodgeDeathDoesNotRestoreControls()
    {
        using (var rig = new MovementRig(true))
        {
            rig.Skills.DodgeSkill.UseSkill(rig.Model.transform.position + Vector3.forward * 10f);
            yield return null;
            rig.Model.isDie = true;
            yield return new WaitForSeconds(0.4f);
            Assert.IsFalse(rig.Model.canMove);
            Assert.IsFalse(rig.Model.canAttack);
            Assert.IsFalse(rig.Model.canSkill);
        }
    }

    public static IEnumerator MoveResumeAndTurn()
    {
        using (var rig = new MovementRig(true))
        {
            Vector3 start = rig.Model.transform.position;
            rig.Controller.StopMove();
            Assert.IsTrue(rig.Agent.isStopped);
            rig.Controller.RequestMove(start + Vector3.right * 8f);
            Assert.IsFalse(rig.Agent.isStopped);
            Assert.Less(Quaternion.Angle(Quaternion.identity, rig.Model.transform.rotation), 0.01f);
            float end = Time.time + 0.35f;
            while (Time.time < end)
            {
                Quaternion before = rig.Model.transform.rotation;
                rig.Controller.Tick();
                Assert.LessOrEqual(Quaternion.Angle(before, rig.Model.transform.rotation), 360f * Time.deltaTime + 0.1f);
                rig.Controller.RequestMove(start + Vector3.right * 8f);
                yield return null;
            }
            Assert.Greater(Vector3.Distance(start, rig.Model.transform.position), 0.3f);
            Assert.Greater(rig.Model.transform.forward.x, 0.5f);
            rig.Controller.StopMove();
            rig.Controller.RequestMove(start + Vector3.forward * 5f);
            Assert.IsFalse(rig.Agent.isStopped);
            rig.Agent.enabled = false;
            Assert.DoesNotThrow(() => rig.Controller.RequestMove(start));
            rig.Controller.Tick();
        }
    }

    public static IEnumerator DodgeRecoversWithoutAnimationEvent()
    {
        using (var rig = new MovementRig(true))
        {
            Vector3 start = rig.Model.transform.position;
            Assert.IsTrue(rig.Skills.DodgeSkill.UseSkill(start + new Vector3(10f, 10f, 0f)));
            Assert.IsFalse(rig.Model.canMove);
            Assert.IsFalse(rig.Model.canSkill);
            yield return new WaitForSeconds(0.4f);
            Assert.IsTrue(rig.Model.canMove);
            Assert.IsTrue(rig.Model.canSkill);
            Assert.IsTrue(rig.Model.canAttack);
            Assert.That(rig.Model.transform.position.x - start.x, Is.InRange(3.5f, 5.5f));
            rig.Controller.RequestMove(start + Vector3.forward * 5f);
            Assert.IsFalse(rig.Agent.isStopped);
        }
    }

    public static IEnumerator DodgeStopsOnStun()
    {
        using (var rig = new MovementRig(true))
        {
            rig.Skills.DodgeSkill.UseSkill(rig.Model.transform.position + Vector3.forward * 10f);
            yield return null;
            rig.Model.Buff.isStun = true;
            yield return null;
            Vector3 stopped = rig.Model.transform.position;
            yield return new WaitForSeconds(0.1f);
            Assert.Less(Vector3.Distance(stopped, rig.Model.transform.position), 0.05f);
            rig.Controller.RequestMove(stopped + Vector3.right * 5f);
            Assert.IsTrue(rig.Agent.isStopped);
        }
    }

    public static IEnumerator JumpRejectsInvalidLandingAndRecoversOnInterruption()
    {
        using (var rig = new MovementRig(true))
        {
            var data = ScriptableObject.CreateInstance<Skill_2SO>();
            try
            {
                data.skillCool = 5f;
                var jump = new Skill_2(rig.Model, data);
                Vector3 start = rig.Model.transform.position;
                Assert.IsFalse(jump.UseSkill(start + Vector3.up * 100f));
                Assert.IsTrue(jump.canUse);
                Assert.IsTrue(rig.Agent.enabled);
                Assert.IsTrue(jump.UseSkill(start + Vector3.forward * 5f));
                Assert.IsFalse(rig.Agent.enabled);
                yield return null;
                rig.Model.Buff.isStun = true;
                yield return null;
                yield return null;
                Assert.IsTrue(rig.Agent.enabled);
                Assert.IsTrue(rig.Agent.isOnNavMesh);
                Assert.Less(Vector3.Distance(start, rig.Model.transform.position), 0.1f);
                Assert.IsTrue(rig.Model.canMove);
            }
            finally { UnityEngine.Object.DestroyImmediate(data); }
        }
    }
}
