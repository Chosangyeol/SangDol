using System;using System.Collections;using System.Collections.Generic;using NUnit.Framework;using UnityEngine;using UnityEngine.TestTools;using UnityEngine.SceneManagement;
namespace ReproducedFixes{
public class ReproducedFixPlayModeTests{
readonly Dictionary<GameObject,bool> roots=new Dictionary<GameObject,bool>();
[UnitySetUp] public IEnumerator Isolate(){foreach(string name in new[]{"Main","Title"}){var s=SceneManager.GetSceneByName(name);if(s.IsValid()&&s.isLoaded)foreach(var r in s.GetRootGameObjects()){roots[r]=r.activeSelf;r.SetActive(false);}}yield return null;}
[UnityTearDown] public IEnumerator Restore(){foreach(var p in roots)if(p.Key!=null)p.Key.SetActive(p.Value);roots.Clear();yield return null;}
[UnityTest] public IEnumerator InvincibilityExpiresAndRestoresRuntimeDamage()=>(IEnumerator)Type.GetType("ReproducedFixCases, Assembly-CSharp-Editor").GetMethod(nameof(InvincibilityExpiresAndRestoresRuntimeDamage)).Invoke(null,null);
[UnityTest] public IEnumerator RealEscapeInputTogglesOnceAndStopsMovement()=>(IEnumerator)Type.GetType("ReproducedFixCases, Assembly-CSharp-Editor").GetMethod(nameof(RealEscapeInputTogglesOnceAndStopsMovement)).Invoke(null,null);
}}