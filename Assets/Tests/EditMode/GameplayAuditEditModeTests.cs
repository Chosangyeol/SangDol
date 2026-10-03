using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class GameplayAuditEditModeTests
{
    private static void Run(string name)
    {
        var type = Type.GetType("GameplayAuditCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(type);
        try { type.GetMethod(name).Invoke(null, null); }
        catch (TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); }
    }
    [Test] public void ToughnessReducesDamageAndResetsWithoutDrift() => Run(nameof(ToughnessReducesDamageAndResetsWithoutDrift));
    [Test] public void RemovingHealthEquipmentAndBuffClampsWithoutHealing() => Run(nameof(RemovingHealthEquipmentAndBuffClampsWithoutHealing));
    [Test] public void DodgeStigmaEquipReplaceAndUnequipAreReversible() => Run(nameof(DodgeStigmaEquipReplaceAndUnequipAreReversible));
    [Test] public void FireFistGivesTenPercentForTenSecondsEveryFifteenSeconds() => Run(nameof(FireFistGivesTenPercentForTenSecondsEveryFifteenSeconds));
    [Test] public void FullInventoryDialogueBlocksSuccessAndCanRetry() => Run(nameof(FullInventoryDialogueBlocksSuccessAndCanRetry));
    [Test] public void FailedAppleDialogueCannotAdvanceAndExitReleasesControls() => Run(nameof(FailedAppleDialogueCannotAdvanceAndExitReleasesControls));
    [Test] public void RefusalClosesSiblingPreviewAndDoesNotAcceptQuest() => Run(nameof(RefusalClosesSiblingPreviewAndDoesNotAcceptQuest));
    [Test] public void DailChoicesFollowAcceptanceAndRefusalText() => Run(nameof(DailChoicesFollowAcceptanceAndRefusalText));
    [Test] public void CompletedIntroductionAllowsAppleQuestRetry() => Run(nameof(CompletedIntroductionAllowsAppleQuestRetry));
    [Test] public void TalkButtonUsesNpcDialogueAndFallback() => Run(nameof(TalkButtonUsesNpcDialogueAndFallback));
    [Test] public void NpcDialogueAssetReferencesUseTheirOwnNames() => Run(nameof(NpcDialogueAssetReferencesUseTheirOwnNames));
    [Test] public void TalkingToEllenDoesNotCompleteMerchantQuest() => Run(nameof(TalkingToEllenDoesNotCompleteMerchantQuest));
    [Test] public void ThrowingRewardSubscriberDoesNotPermitDuplicateRewards() => Run(nameof(ThrowingRewardSubscriberDoesNotPermitDuplicateRewards));
}
