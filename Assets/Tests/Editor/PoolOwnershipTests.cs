using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class PoolOwnershipRig : IDisposable
{
    public readonly GameObject Template, Root;
    public readonly PoolManager Manager;
    private readonly PoolManager previous = PoolManager.Instance;
    public PoolOwnershipRig()
    {
        typeof(PoolManager).GetProperty("Instance").SetValue(null, null);
        Root = new GameObject("OwnershipTestManager"); Manager = Root.AddComponent<PoolManager>();
        Template = new GameObject("OwnershipTestPrefab"); Template.SetActive(false);
        Template.AddComponent<PoolableMono>();
        Manager.CreatePool(Template.GetComponent<PoolableMono>(), 1, false);
    }
    public void Dispose()
    {
        UnityEngine.Object.DestroyImmediate(Root); UnityEngine.Object.DestroyImmediate(Template);
        typeof(PoolManager).GetProperty("Instance").SetValue(null, previous);
    }
}
public static class PoolOwnershipCases
{
    public static void DuplicateReturnCannotLeaseOneObjectTwice()
    {
        using (var r = new PoolOwnershipRig())
        {
            var a = r.Manager.Pop(r.Template.name); r.Manager.Push(a); r.Manager.Push(a);
            Assert.AreSame(a, r.Manager.Pop(r.Template.name));
            var b = r.Manager.Pop(r.Template.name);
            Assert.AreNotSame(a, b); Assert.IsTrue(b.gameObject.activeSelf);
        }
    }
    public static void ForeignAndRenamedObjectsRespectOwnership()
    {
        using (var r = new PoolOwnershipRig())
        {
            var a = r.Manager.Pop(r.Template.name); a.name = "ChangedName"; r.Manager.Push(a);
            Assert.IsFalse(a.gameObject.activeSelf); Assert.AreSame(a, r.Manager.Pop(r.Template.name));
            r.Manager.Push(r.Template.GetComponent<PoolableMono>());
            Assert.AreNotSame(r.Template, r.Manager.Pop(r.Template.name).gameObject);
        }
    }
    public static void DestroyedAvailableObjectIsReplaced()
    {
        using (var r = new PoolOwnershipRig())
        {
            var a = r.Manager.Pop(r.Template.name); r.Manager.Push(a);
            UnityEngine.Object.DestroyImmediate(a.gameObject);
            var b = r.Manager.Pop(r.Template.name); Assert.IsNotNull(b); Assert.IsTrue(b.gameObject.activeSelf);
        }
    }
    public static void DuplicateCreationAndStageClearPreserveGlobal()
    {
        using (var r = new PoolOwnershipRig())
        {
            var global = new GameObject("OwnershipGlobal"); global.SetActive(false);
            var source = global.AddComponent<PoolableMono>();
            try
            {
                r.Manager.CreatePool(source, 1, true); r.Manager.CreatePool(source, 10, false);
                var a = r.Manager.Pop(global.name);
                r.Manager.ClearStagePools(); r.Manager.ClearStagePools();
                r.Manager.Push(a); Assert.AreSame(a, r.Manager.Pop(global.name));
            }
            finally { UnityEngine.Object.DestroyImmediate(global); }
        }
    }
}
public static class PoolOwnershipRuntimeCases
{
    public static IEnumerator ReentrantDisableReturn(Type probeType)
    {
        using (var r = new PoolOwnershipRig())
        {
            var a = r.Manager.Pop(r.Template.name);
            var probe = a.gameObject.AddComponent(probeType);
            Assert.IsNotNull(probe);
            int callbacks = 0;
            probeType.GetField("Returned").SetValue(probe, (Action)(() => { callbacks++; r.Manager.Push(a); }));
            r.Manager.Push(a); Assert.AreEqual(1, callbacks);
            Assert.AreSame(a, r.Manager.Pop(r.Template.name));
            Assert.AreNotSame(a, r.Manager.Pop(r.Template.name));
            yield return null;
        }
    }

    public static IEnumerator ClearDestroysActiveAndInactiveAndRejectsLateReturn()
    {
        using (var r = new PoolOwnershipRig())
        {
            var active = r.Manager.Pop(r.Template.name);
            var inactive = r.Manager.Pop(r.Template.name); r.Manager.Push(inactive);
            var external = new GameObject("OwnershipExternalParent");
            try
            {
                active.transform.SetParent(external.transform);
                r.Manager.ClearStagePools();
                Assert.IsFalse(active.gameObject.activeSelf);
                r.Manager.CreatePool(r.Template.GetComponent<PoolableMono>(), 1, false);
                r.Manager.Push(active);
                var current = r.Manager.Pop(r.Template.name);
                Assert.AreNotSame(active, current); Assert.IsTrue(current.gameObject.activeSelf);
                yield return null;
                Assert.IsTrue(active == null); Assert.IsTrue(inactive == null); Assert.IsTrue(current != null);
            }
            finally { UnityEngine.Object.DestroyImmediate(external); }
        }
    }
    public static IEnumerator ReturnRestoresPoolParentAndSupportsOverflow()
    {
        using (var r = new PoolOwnershipRig())
        {
            var a = r.Manager.Pop(r.Template.name); var b = r.Manager.Pop(r.Template.name);
            Assert.IsTrue(a.gameObject.activeSelf && b.gameObject.activeSelf);
            var external = new GameObject("OwnershipExternalParent");
            a.transform.SetParent(external.transform); r.Manager.Push(a);
            UnityEngine.Object.DestroyImmediate(external); yield return null;
            Assert.IsTrue(a != null); Assert.AreSame(a, r.Manager.Pop(r.Template.name));
        }
    }
}