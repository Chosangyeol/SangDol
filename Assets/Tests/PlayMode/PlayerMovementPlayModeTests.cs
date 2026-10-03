using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class PlayerMovementPlayModeTests
{
    [UnitySetUp]
    public IEnumerator IsolateBootstrapScene()
    {
        // SystemBootstrapper loads Main before the test scene. Keep its unrelated actors out of the test NavMesh.
        var main = SceneManager.GetSceneByName("Main");
        if (main.IsValid() && main.isLoaded)
            foreach (var root in main.GetRootGameObjects()) root.SetActive(false);
        yield return null;
    }

    private static IEnumerator Run(string name)
    {
        var bridge = Type.GetType("PlayerMovementRuntimeCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(bridge, "Editor-only integration bridge must be loaded.");
        return (IEnumerator)bridge.GetMethod(name, BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
    }

    [UnityTest] public IEnumerator MoveResumeAndTurn() => Run(nameof(MoveResumeAndTurn));
    [UnityTest] public IEnumerator Skill1StunStopsDashAndRecovers() => Run(nameof(Skill1StunStopsDashAndRecovers));
    [UnityTest] public IEnumerator Skill3StunCancelsChargeAndLateRelease() => Run(nameof(Skill3StunCancelsChargeAndLateRelease));
    [UnityTest] public IEnumerator Skill4StunStopsHoldingAndReturnsEffectOnce() => Run(nameof(Skill4StunStopsHoldingAndReturnsEffectOnce));
    [UnityTest] public IEnumerator Skill3ReleasedCastCanStillBeInterrupted() => Run(nameof(Skill3ReleasedCastCanStillBeInterrupted));
    [UnityTest] public IEnumerator SkillControlLockSurvivesStunAndLateEvents() => Run(nameof(SkillControlLockSurvivesStunAndLateEvents));
    [UnityTest] public IEnumerator SkillDeathUsesExistingCooldownResetPolicy() => Run(nameof(SkillDeathUsesExistingCooldownResetPolicy));
    [UnityTest] public IEnumerator SkillExternalLockExitsChargeAnimation() => Run(nameof(SkillExternalLockExitsChargeAnimation));
    [UnityTest] public IEnumerator Skill4ReturnedEffectDoesNotTouchNewOwner() => Run(nameof(Skill4ReturnedEffectDoesNotTouchNewOwner));
    [UnityTest] public IEnumerator Skill4DeathAfterRecoveryCancelsDelayedEffect() => Run(nameof(Skill4DeathAfterRecoveryCancelsDelayedEffect));
    [UnityTest] public IEnumerator OldSkillAnimationCannotUnlockNewCast() => Run(nameof(OldSkillAnimationCannotUnlockNewCast));
    [UnityTest] public IEnumerator Skill1NormalRecoveryStillWorks() => Run(nameof(Skill1NormalRecoveryStillWorks));
    [UnityTest] public IEnumerator Skill3NormalRecoveryStillDealsDamage() => Run(nameof(Skill3NormalRecoveryStillDealsDamage));
    [UnityTest] public IEnumerator Skill4NormalRecoveryStillWorks() => Run(nameof(Skill4NormalRecoveryStillWorks));
    [UnityTest] public IEnumerator IdenFinisherResumesHeldBasicAttack() => Run(nameof(IdenFinisherResumesHeldBasicAttack));
    [UnityTest] public IEnumerator IdenFinisherResumesReleasedBasicAttack() => Run(nameof(IdenFinisherResumesReleasedBasicAttack));
    [UnityTest] public IEnumerator IdenFinisherInterruptedByStunResumesBasicAttack() => Run(nameof(IdenFinisherInterruptedByStunResumesBasicAttack));
    [UnityTest] public IEnumerator MoveClickDuringAttackLockStartsAfterAttackRecovers() => Run(nameof(MoveClickDuringAttackLockStartsAfterAttackRecovers));
    [UnityTest] public IEnumerator JumpLandsAndResumesMovement() => Run(nameof(JumpLandsAndResumesMovement));
    [UnityTest] public IEnumerator BufferedSkillExpiresWhileModelIsNotUpdating() => Run(nameof(BufferedSkillExpiresWhileModelIsNotUpdating));
    [UnityTest] public IEnumerator DodgeDeathDoesNotRestoreControls() => Run(nameof(DodgeDeathDoesNotRestoreControls));
    [UnityTest] public IEnumerator DodgeRecoversWithoutAnimationEvent() => Run(nameof(DodgeRecoversWithoutAnimationEvent));
    [UnityTest] public IEnumerator DodgeStopsOnStun() => Run(nameof(DodgeStopsOnStun));
    [UnityTest] public IEnumerator JumpRejectsInvalidLandingAndRecoversOnInterruption() => Run(nameof(JumpRejectsInvalidLandingAndRecoversOnInterruption));
}
