using System;using System.Reflection;using System.Runtime.ExceptionServices;using NUnit.Framework;
namespace BossFourFixes { public class BossFourIssuesEditModeTests
{
 static void Run(string name){try{Type.GetType("BossFourIssuesCases, Assembly-CSharp-Editor").GetMethod(name).Invoke(null,null);}catch(TargetInvocationException e){ExceptionDispatchInfo.Capture(e.InnerException).Throw();}}
 [Test] public void CameraRestoresOriginalFourShots()=>Run(nameof(CameraRestoresOriginalFourShots));
 [Test] public void EndingHasRealTimelineAndInactiveCamera()=>Run(nameof(EndingHasRealTimelineAndInactiveCamera));
}

}