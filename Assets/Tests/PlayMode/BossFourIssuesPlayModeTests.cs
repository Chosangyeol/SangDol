using System;using System.Collections;using System.Reflection;using System.Collections.Generic;using UnityEngine;using NUnit.Framework;using UnityEngine.TestTools;
namespace BossFourFixes { public class BossFourIssuesPlayModeTests
{
    private readonly Dictionary<GameObject, bool> roots = new Dictionary<GameObject, bool>();
    private object previousDamageText;
    private System.Reflection.FieldInfo damageTextInstance;
    [UnitySetUp] public IEnumerator IsolateMain()
    {
        damageTextInstance = Type.GetType("DamageTextManager, Assembly-CSharp").GetField("Instance");
        previousDamageText = damageTextInstance.GetValue(null); damageTextInstance.SetValue(null, null);
        roots.Clear(); var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("Main");
        if (scene.IsValid() && scene.isLoaded)
            foreach (var root in scene.GetRootGameObjects()) { roots[root] = root.activeSelf; root.SetActive(false); }
        yield return null;
    }
    [UnityTearDown] public IEnumerator RestoreMain()
    {
        foreach (var pair in roots) if (pair.Key != null) pair.Key.SetActive(pair.Value);
        roots.Clear(); yield return null;
        damageTextInstance.SetValue(null, previousDamageText);
    }

 static IEnumerator Run(string name)=>(IEnumerator)Type.GetType("BossFourIssuesCases, Assembly-CSharp-Editor").GetMethod(name).Invoke(null,null);
 [UnityTest] public IEnumerator ScaleAndCameraRecoverAcrossRetries()=>Run(nameof(ScaleAndCameraRecoverAcrossRetries));
 [UnityTest] public IEnumerator ShortAttacksHitOnceAndCancelCleanly()=>Run(nameof(ShortAttacksHitOnceAndCancelCleanly));
 [UnityTest] public IEnumerator EndingPlaysBeforeDepartureAndCancellationRestoresUI()=>Run(nameof(EndingPlaysBeforeDepartureAndCancellationRestoresUI));
}

}