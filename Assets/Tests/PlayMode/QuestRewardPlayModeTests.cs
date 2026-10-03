using System; using System.Collections; using NUnit.Framework; using UnityEngine.TestTools;
public class QuestRewardPlayModeTests {
[UnitySetUp] public IEnumerator IsolateBootstrapScene() { var main=UnityEngine.SceneManagement.SceneManager.GetSceneByName("Main"); if(main.IsValid()&&main.isLoaded) foreach(var root in main.GetRootGameObjects()) root.SetActive(false); yield return null; }
private static IEnumerator Run(string n) { var t=Type.GetType("QuestRewardRuntimeCases, Assembly-CSharp-Editor"); Assert.IsNotNull(t); return (IEnumerator)t.GetMethod(n).Invoke(null,null); }
[UnityTest] public IEnumerator RewardNotificationsObserveFullCommitAndCannotReenter()=>Run(nameof(RewardNotificationsObserveFullCommitAndCannotReenter));
[UnityTest] public IEnumerator QuestLifecycleRefreshesOnEnableAndUnsubscribesOnDestroy()=>Run(nameof(QuestLifecycleRefreshesOnEnableAndUnsubscribesOnDestroy));
[UnityTest] public IEnumerator StackMergeAndRemovalUpdateQuestState()=>Run(nameof(StackMergeAndRemovalUpdateQuestState));
}