using System; using System.Reflection; using System.Runtime.ExceptionServices; using NUnit.Framework;
public class VideoRecoveryEditModeTests {
private static void Run(string n) { var t=Type.GetType("VideoRecoveryCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t); try { t.GetMethod(n).Invoke(null,null); } catch(TargetInvocationException e) { ExceptionDispatchInfo.Capture(e.InnerException).Throw(); } }
[Test] public void MissingPlayerAndClipAreSafe()=>Run(nameof(MissingPlayerAndClipAreSafe));
[Test] public void BusyPlaybackDoesNotLoseOwnership()=>Run(nameof(BusyPlaybackDoesNotLoseOwnership));
[Test] public void Special3GameplayConfigurationAllowsMissingVideo()=>Run(nameof(Special3GameplayConfigurationAllowsMissingVideo));
[Test] public void InvalidWarpDoesNotTakeControls()=>Run(nameof(InvalidWarpDoesNotTakeControls));
}