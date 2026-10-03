using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class PlayerMovementEditModeTests
{
    private object fixture;
    private void Run(string name, params object[] arguments)
    {
        try { fixture.GetType().GetMethod(name).Invoke(fixture, arguments); }
        catch (TargetInvocationException e)
        {
            ExceptionDispatchInfo.Capture(e.InnerException).Throw();
        }
    }
    [SetUp] public void SetUp()
    {
        var type = Type.GetType("PlayerMovementTests, Assembly-CSharp-Editor");
        Assert.IsNotNull(type);
        fixture = Activator.CreateInstance(type);
        Run("SetUp");
    }
    [TearDown] public void TearDown() { if (fixture != null) Run("TearDown"); }
    [Test] public void SkillWaitsForSkillPermissionEvenWhenMovementIsAllowed() => Run(nameof(SkillWaitsForSkillPermissionEvenWhenMovementIsAllowed));
    [Test] public void BufferedSkillExpiresInsteadOfFiringLate() => Run(nameof(BufferedSkillExpiresInsteadOfFiringLate));
    [Test] public void BufferedChargePreservesReleaseBeforeActivation() => Run(nameof(BufferedChargePreservesReleaseBeforeActivation));
    [TestCase(true)] [TestCase(false)] public void DeathOrStunDiscardsBufferedSkill(bool death) => Run(nameof(DeathOrStunDiscardsBufferedSkill), death);
    [Test] public void SlotReplacementDoesNotExecuteOldBufferedSkill() => Run(nameof(SlotReplacementDoesNotExecuteOldBufferedSkill));
    [Test] public void NewestBufferedInputWins() => Run(nameof(NewestBufferedInputWins));
    [Test] public void InactiveAgentRejectsMovementWithoutChangingRotation() => Run(nameof(InactiveAgentRejectsMovementWithoutChangingRotation));
    [Test] public void SkillReleaseWithoutCameraStillEndsCharge() => Run(nameof(SkillReleaseWithoutCameraStillEndsCharge));
    [Test] public void DodgeRejectsInactiveAgentWithoutConsumingCooldown() => Run(nameof(DodgeRejectsInactiveAgentWithoutConsumingCooldown));
    [Test] public void IdenFinisherCompletionRestoresNormalAttack() => Run(nameof(IdenFinisherCompletionRestoresNormalAttack));
    [Test] public void InterruptedChargePreservesCooldownAndPreviousPermissions() => Run(nameof(InterruptedChargePreservesCooldownAndPreviousPermissions));
    [Test] public void DeathCancelsChargeWithoutRestoringControls() => Run(nameof(DeathCancelsChargeWithoutRestoringControls));
    [Test] public void ExternalLockRejectsLateRecoveryEvents() => Run(nameof(ExternalLockRejectsLateRecoveryEvents));
    [Test] public void PreviouslyInterruptedSlotDoesNotCancelNewSkill() => Run(nameof(PreviouslyInterruptedSlotDoesNotCancelNewSkill));
    [TestCase(true)] [TestCase(false)] public void IdenFinisherDoesNotRestoreDeadOrDisabledControls(bool dead) => Run(nameof(IdenFinisherDoesNotRestoreDeadOrDisabledControls), dead);
}
