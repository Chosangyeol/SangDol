using System; using System.Reflection; using System.Runtime.ExceptionServices; using NUnit.Framework;
public class QuestRewardEditModeTests {
private static void Run(string n) { var t=Type.GetType("QuestRewardCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t); try { t.GetMethod(n).Invoke(null,null); } catch(TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); } }
[Test] public void CsvAppleAwardsAllRewards()=>Run(nameof(CsvAppleAwardsAllRewards));
[Test] public void CsvWolfAwardsGearGoldAndExp()=>Run(nameof(CsvWolfAwardsGearGoldAndExp));
[Test] public void InsufficientBatchSpaceIsAtomicAndRetryable()=>Run(nameof(InsufficientBatchSpaceIsAtomicAndRetryable));
[Test] public void DuplicateRewardsMergeAndPreserveExistingReference()=>Run(nameof(DuplicateRewardsMergeAndPreserveExistingReference));
[Test] public void MultipleEquipmentRewardsRemainDistinctUnits()=>Run(nameof(MultipleEquipmentRewardsRemainDistinctUnits));
[Test] public void InvalidSecondRewardDoesNotPartiallyPay()=>Run(nameof(InvalidSecondRewardDoesNotPartiallyPay));
[Test] public void BadInputsAndOversizedEquipmentFailWithoutMutation()=>Run(nameof(BadInputsAndOversizedEquipmentFailWithoutMutation));
[Test] public void CurrencyOnlyWorksWithoutItemManager()=>Run(nameof(CurrencyOnlyWorksWithoutItemManager));
[Test] public void GoldOverflowDoesNotPartiallyPay()=>Run(nameof(GoldOverflowDoesNotPartiallyPay));
[Test] public void RewardNotificationsObserveFullCommitAndCannotReenter()=>Run(nameof(RewardNotificationsObserveFullCommitAndCannotReenter));
[Test] public void StackMergeAndRemovalUpdateQuestState()=>Run(nameof(StackMergeAndRemovalUpdateQuestState));
[Test] public void NewSlotAcquisitionNotificationsObserveFinalQuantity()=>Run(nameof(NewSlotAcquisitionNotificationsObserveFinalQuantity));
[Test] public void StaleCollectionReadyStateIsRechecked()=>Run(nameof(StaleCollectionReadyStateIsRechecked));
}