using System; using System.Collections; using NUnit.Framework; using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
public class SkillSlotCooldownPlayModeTests {
[UnitySetUp] public IEnumerator IsolateBootstrapScene() { var main=SceneManager.GetSceneByName("Main"); if(main.IsValid()&&main.isLoaded) foreach(var root in main.GetRootGameObjects()) root.SetActive(false); yield return null; }
private static IEnumerator Run(string n) { var t=Type.GetType("SkillSlotCooldownRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t); return (IEnumerator)t.GetMethod(n).Invoke(null,null); }
[UnityTest] public IEnumerator RightClickClearsAndReequipRetainsActualCooldown()=>Run(nameof(RightClickClearsAndReequipRetainsActualCooldown));
[UnityTest] public IEnumerator DragOutsideClearsCooldownAndDragIcon()=>Run(nameof(DragOutsideClearsCooldownAndDragIcon));
[UnityTest] public IEnumerator MovingToEmptySlotTransfersCooldownDisplay()=>Run(nameof(MovingToEmptySlotTransfersCooldownDisplay));
[UnityTest] public IEnumerator FrameUpdateClearsEmptySlotWithoutRefresh()=>Run(nameof(FrameUpdateClearsEmptySlotWithoutRefresh));
}