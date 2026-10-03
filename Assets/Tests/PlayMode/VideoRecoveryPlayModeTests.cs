using System; using System.Collections; using NUnit.Framework; using UnityEngine.TestTools; using UnityEngine.SceneManagement;
public class VideoRecoveryPlayModeTests {
[UnitySetUp] public IEnumerator IsolateBootstrapScene() { var main=SceneManager.GetSceneByName("Main"); if(main.IsValid()&&main.isLoaded) foreach(var root in main.GetRootGameObjects()) root.SetActive(false); yield return null; }
private static IEnumerator Run(string n) { var t=Type.GetType("VideoRecoveryRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t); return (IEnumerator)t.GetMethod(n).Invoke(null,null); }
[UnityTest] public IEnumerator WatchdogUsesRealtimeAndUnlocksFailedPlayback()=>Run(nameof(WatchdogUsesRealtimeAndUnlocksFailedPlayback));
[UnityTest] public IEnumerator SkipDisableAndStaleSessionAreSafe()=>Run(nameof(SkipDisableAndStaleSessionAreSafe));
[UnityTest] public IEnumerator ErrorAndCompletionHidePresentation()=>Run(nameof(ErrorAndCompletionHidePresentation));
[UnityTest] public IEnumerator MissingVideoWarpCompletesAndRestoresControls()=>Run(nameof(MissingVideoWarpCompletesAndRestoresControls));
[UnityTest] public IEnumerator DisabledManagerCancelsWarpWithoutMoving()=>Run(nameof(DisabledManagerCancelsWarpWithoutMoving));
}