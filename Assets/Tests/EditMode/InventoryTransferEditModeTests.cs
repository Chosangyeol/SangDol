using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;

public class InventoryTransferEditModeTests
{
    private static void Run(string name)
    {
        var type = Type.GetType("InventoryTransferCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(type);
        try { type.GetMethod(name).Invoke(null, null); }
        catch (TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); }
    }
    [Test] public void FullInventoryUnequipPreservesEverything() => Run(nameof(FullInventoryUnequipPreservesEverything));
    [Test] public void FullInventoryGearSwapPreservesReferencesAndStats() => Run(nameof(FullInventoryGearSwapPreservesReferencesAndStats));
    [Test] public void UnequipToOccupiedMaterialIsRejected() => Run(nameof(UnequipToOccupiedMaterialIsRejected));
    [Test] public void UnequipToDifferentGearTypeIsRejected() => Run(nameof(UnequipToDifferentGearTypeIsRejected));
    [Test] public void UnequipToSameTypeSwapsInPlace() => Run(nameof(UnequipToSameTypeSwapsInPlace));
    [Test] public void RepeatedTransfersDoNotDuplicateStatsOrItems() => Run(nameof(RepeatedTransfersDoNotDuplicateStatsOrItems));
    [Test] public void InvalidUnequipRequestsAreNoOps() => Run(nameof(InvalidUnequipRequestsAreNoOps));
    [Test] public void UnownedAndInvalidGearCannotBeEquipped() => Run(nameof(UnownedAndInvalidGearCannotBeEquipped));
    [Test] public void SetItemAtCannotOverwriteOrDuplicate() => Run(nameof(SetItemAtCannotOverwriteOrDuplicate));
    [Test] public void InventorySwapPreservesItemsAndNotifies() => Run(nameof(InventorySwapPreservesItemsAndNotifies));
    [Test] public void TransferEventsObserveCommittedStateAndDoNotAcquireAgain() => Run(nameof(TransferEventsObserveCommittedStateAndDoNotAcquireAgain));
    [Test] public void PartialAcquisitionKeepsDonorRemainder() => Run(nameof(PartialAcquisitionKeepsDonorRemainder));
    [Test] public void EquipmentCapacityIsOnePerEmptySlot() => Run(nameof(EquipmentCapacityIsOnePerEmptySlot));
}

