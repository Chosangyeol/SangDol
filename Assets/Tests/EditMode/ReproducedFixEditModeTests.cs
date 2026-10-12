using System;using System.Reflection;using System.Runtime.ExceptionServices;using NUnit.Framework;
namespace ReproducedFixes {
public class ReproducedFixEditModeTests{
static void Run(string name){try{Type.GetType("ReproducedFixCases, Assembly-CSharp-Editor").GetMethod(name).Invoke(null,null);}catch(TargetInvocationException e){ExceptionDispatchInfo.Capture(e.InnerException).Throw();}}
[Test] public void HiddenPanelsClearStackUsingRealUiPrefab()=>Run(nameof(HiddenPanelsClearStackUsingRealUiPrefab));
[Test] public void CraftPreviewRecoversAfterSpaceAndGoldChanges()=>Run(nameof(CraftPreviewRecoversAfterSpaceAndGoldChanges));
[Test] public void InvincibilityGuardsModelAndStatDamageWithoutNotifications()=>Run(nameof(InvincibilityGuardsModelAndStatDamageWithoutNotifications));
[Test] public void HpGearSwapsPreserveCurrentHpAndNotifyOnlyFinalState()=>Run(nameof(HpGearSwapsPreserveCurrentHpAndNotifyOnlyFinalState));
[Test] public void HpSwapToDifferentStatClampsAtFinalMaximum()=>Run(nameof(HpSwapToDifferentStatClampsAtFinalMaximum));
[Test] public void FailedCraftPreservesCostsReferencesAndCanRetry()=>Run(nameof(FailedCraftPreservesCostsReferencesAndCanRetry));
[Test] public void CraftCanUseSlotFreedByConsumedMaterial()=>Run(nameof(CraftCanUseSlotFreedByConsumedMaterial));
[Test] public void CraftMergesOutputAndAggregatesDuplicateIngredients()=>Run(nameof(CraftMergesOutputAndAggregatesDuplicateIngredients));
[Test] public void InvalidCraftDoesNotChargeOrConsume()=>Run(nameof(InvalidCraftDoesNotChargeOrConsume));
[Test] public void CraftNotificationsObserveCommitAndCannotDoublePay()=>Run(nameof(CraftNotificationsObserveCommitAndCannotDoublePay));
[Test] public void OptionCyclesAndButtonCloseKeepStackConsistent()=>Run(nameof(OptionCyclesAndButtonCloseKeepStackConsistent));
[Test] public void EscapeClosesVisibleTopWithoutReopeningHiddenPanels()=>Run(nameof(EscapeClosesVisibleTopWithoutReopeningHiddenPanels));
}}