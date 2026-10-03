using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class InventoryTransferPlayModeTests
{
    [UnitySetUp] public IEnumerator IsolateBootstrapScene()
    {
        var main = SceneManager.GetSceneByName("Main");
        if (main.IsValid() && main.isLoaded)
            foreach (var root in main.GetRootGameObjects()) root.SetActive(false);
        yield return null;
    }
    private static IEnumerator Run(string name)
    {
        var type = Type.GetType("InventoryTransferRuntimeCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(type);
        return (IEnumerator)type.GetMethod(name).Invoke(null, null);
    }
    [UnityTest] public IEnumerator GearDropSwapsAndUpdatesIcons() => Run(nameof(GearDropSwapsAndUpdatesIcons));
    [UnityTest] public IEnumerator RejectedEquipmentDropRestoresDragIcon() => Run(nameof(RejectedEquipmentDropRestoresDragIcon));
    [UnityTest] public IEnumerator StaleInventoryDragCannotEquipReplacement() => Run(nameof(StaleInventoryDragCannotEquipReplacement));
    [UnityTest] public IEnumerator ForeignInventoryDragIsRejected() => Run(nameof(ForeignInventoryDragIsRejected));
    [UnityTest] public IEnumerator ClosingUIInvalidatesDrag() => Run(nameof(ClosingUIInvalidatesDrag));
    [UnityTest] public IEnumerator GroundDropKeepsRemainderAndReturnsOnlyWhenCollected() => Run(nameof(GroundDropKeepsRemainderAndReturnsOnlyWhenCollected));
}

