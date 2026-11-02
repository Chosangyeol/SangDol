using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class BuffTooltipPlayModeTests
{
    [UnityTest]
    public IEnumerator RuntimeDisableHidesHoveredDescription()
    {
        var cases = Type.GetType("BuffTooltipCases, Assembly-CSharp-Editor");
        Assert.IsNotNull(cases);
        return (IEnumerator)cases.GetMethod(nameof(RuntimeDisableHidesHoveredDescription)).Invoke(null, null);
    }
}
