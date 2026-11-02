using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public static class BuffTooltipCases
{
    private sealed class TestBuff : BuffBase
    {
        public TestBuff(BuffSO data) : base(data, 5f) { }
        public override void OnEnable() { }
        public override void OnDisable() { }
    }

    private static BuffSlot CreateSlot(string name, BuffTooltip tooltip, BuffBase buff)
    {
        var gameObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(BuffSlot));
        var slot = gameObject.GetComponent<BuffSlot>();
        typeof(BuffSlot).GetField("iconImage", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(slot, gameObject.GetComponent<Image>());
        slot.Init(new SBuff(null, null, buff), tooltip);
        return slot;
    }

    private static BuffTooltip CreateTooltip(out GameObject gameObject)
    {
        gameObject = new GameObject("BuffTooltip", typeof(RectTransform), typeof(BuffTooltip));
        var tooltip = gameObject.GetComponent<BuffTooltip>();
        typeof(BuffTooltip).GetField("rectTransform", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(tooltip, gameObject.GetComponent<RectTransform>());
        return tooltip;
    }

    private static void DisableSlot(BuffSlot slot)
    {
        slot.gameObject.SetActive(false);
        // EditMode does not dispatch OnDisable for this runtime-only component.
        typeof(BuffSlot).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(slot, null);
    }

    public static void HidesWhenHoveredBuffSlotIsDisabledOrReused()
    {
        var data = ScriptableObject.CreateInstance<BuffSO>();
        var tooltip = CreateTooltip(out var tooltipObject);
        BuffSlot slot = null;
        try
        {
            slot = CreateSlot("BuffSlot", tooltip, new TestBuff(data));
            tooltip.ToggleTooltip(true, slot.transform as RectTransform);
            slot.Init(new SBuff(null, null, new TestBuff(data)), tooltip);
            Assert.IsFalse(tooltipObject.activeSelf, "Reusing a hovered slot must dismiss its old description.");

            tooltip.ToggleTooltip(true, slot.transform as RectTransform);
            DisableSlot(slot);
            Assert.IsFalse(tooltipObject.activeSelf, "Removing a hovered buff must dismiss its description.");
        }
        finally
        {
            if (slot != null) Object.DestroyImmediate(slot.gameObject);
            Object.DestroyImmediate(tooltipObject);
            Object.DestroyImmediate(data);
        }
    }

    public static void DisablingOtherSlotKeepsCurrentDescription()
    {
        var data = ScriptableObject.CreateInstance<BuffSO>();
        var tooltip = CreateTooltip(out var tooltipObject);
        BuffSlot first = null;
        BuffSlot second = null;
        try
        {
            first = CreateSlot("First", tooltip, new TestBuff(data));
            second = CreateSlot("Second", tooltip, new TestBuff(data));
            tooltip.ToggleTooltip(true, second.transform as RectTransform);
            DisableSlot(first);
            Assert.IsTrue(tooltipObject.activeSelf, "A different slot must not close the current description.");
            DisableSlot(second);
            Assert.IsFalse(tooltipObject.activeSelf);
        }
        finally
        {
            if (first != null) Object.DestroyImmediate(first.gameObject);
            if (second != null) Object.DestroyImmediate(second.gameObject);
            Object.DestroyImmediate(tooltipObject);
            Object.DestroyImmediate(data);
        }
    }

    public static IEnumerator RuntimeDisableHidesHoveredDescription()
    {
        var data = ScriptableObject.CreateInstance<BuffSO>();
        var tooltip = CreateTooltip(out var tooltipObject);
        BuffSlot slot = null;
        try
        {
            slot = CreateSlot("BuffSlot", tooltip, new TestBuff(data));
            tooltip.ToggleTooltip(true, slot.transform as RectTransform);
            Assert.IsTrue(tooltipObject.activeSelf);

            slot.gameObject.SetActive(false);
            yield return null;
            Assert.IsFalse(tooltipObject.activeSelf);
        }
        finally
        {
            if (slot != null) Object.DestroyImmediate(slot.gameObject);
            Object.DestroyImmediate(tooltipObject);
            Object.DestroyImmediate(data);
        }
    }
}
