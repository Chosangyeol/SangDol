using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class SkillSlotCooldownRig : IDisposable
{
    public readonly MovementRig Movement = new MovementRig(false);
    public readonly GameObject Root;
    public readonly SkillSlot Slot;
    public readonly TMP_Text Text;
    public readonly Image Overlay, Icon;
    public SkillSlotCooldownRig(bool onlyCooldown = false)
    {
        SetLevel(Movement.Probe, 1);
        Movement.Probe.canUse = false; Movement.Probe.finalCoolTime = 10f; Movement.Probe.nowCoolTime = 4.2f;
        Root = new GameObject("SkillCooldownTestCanvas", typeof(RectTransform), typeof(Canvas));
        Slot = CreateSlot(C_Enums.SkillSlot.Q, onlyCooldown);
        Text = Slot.transform.Find("CooldownText").GetComponent<TMP_Text>();
        Overlay = Slot.transform.Find("Overlay").GetComponent<Image>();
        Icon = Slot.transform.Find("Icon").GetComponent<Image>();
    }
    public static void SetLevel(SkillBase skill, int level)
        => typeof(SkillBase).GetField("skillLevel", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(skill, level);
    public SkillSlot CreateSlot(C_Enums.SkillSlot key, bool onlyCooldown = false)
    {
        var go = new GameObject(key.ToString(), typeof(RectTransform)); go.transform.SetParent(Root.transform, false);
        var slot = go.AddComponent<SkillSlot>(); slot.skillSlot = key; slot.showOnlyOnCooldown = onlyCooldown;
        var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image)); icon.transform.SetParent(go.transform, false);
        var overlay = new GameObject("Overlay", typeof(RectTransform), typeof(Image)); overlay.transform.SetParent(go.transform, false);
        var text = new GameObject("CooldownText", typeof(RectTransform), typeof(TextMeshProUGUI)); text.transform.SetParent(go.transform, false);
        InventoryTransferRig.Set(slot, "iconImage", icon.GetComponent<Image>());
        InventoryTransferRig.Set(slot, "coolOverlay", overlay.GetComponent<Image>());
        InventoryTransferRig.Set(slot, "coolTimeText", text.GetComponent<TMP_Text>());
        slot.Init(Movement.Skills, null);
        // MainUI uses this same skill event to call Refresh on its slots.
        Movement.Skills.OnSkillDataChanged += slot.Refresh;
        return slot;
    }
    public void AssertCooling()
    {
        Assert.IsTrue(Text.gameObject.activeSelf); Assert.AreEqual("5s", Text.text);
        Assert.AreEqual(0.42f, Overlay.fillAmount, 0.001f);
    }
    public static void AssertCleared(SkillSlot slot)
    {
        var text = slot.transform.Find("CooldownText").GetComponent<TMP_Text>();
        Assert.IsFalse(text.gameObject.activeSelf); Assert.AreEqual(string.Empty, text.text);
        Assert.AreEqual(0f, slot.transform.Find("Overlay").GetComponent<Image>().fillAmount);
    }
    public void Dispose()
    {
        foreach (var slot in Root.GetComponentsInChildren<SkillSlot>(true)) Movement.Skills.OnSkillDataChanged -= slot.Refresh;
        UnityEngine.Object.DestroyImmediate(Root); Movement.Dispose();
    }
}
public static class SkillSlotCooldownCases
{
    public static void ClearingSlotRefreshRemovesCooldownImmediately()
    {
        using (var r = new SkillSlotCooldownRig())
        {
            r.AssertCooling(); r.Movement.Skills.ClearSkillSlot(C_Enums.SkillSlot.Q);
            SkillSlotCooldownRig.AssertCleared(r.Slot); Assert.IsFalse(r.Icon.enabled);
            Assert.AreEqual(4.2f, r.Movement.Probe.nowCoolTime);
        }
    }
    public static void ReadyReplacementClearsPreviousCooldown()
    {
        using (var r = new SkillSlotCooldownRig())
        {
            r.AssertCooling();
            var replacement = new MovementProbeSkill(r.Movement.Model, r.Movement.Probe.skillData);
            SkillSlotCooldownRig.SetLevel(replacement, 1); replacement.canUse = true;
            r.Movement.Skills.RegisterSkillToSlot(C_Enums.SkillSlot.Q, replacement);
            Assert.AreSame(replacement, r.Slot.CurrentSkill); Assert.IsTrue(r.Icon.enabled);
            SkillSlotCooldownRig.AssertCleared(r.Slot);
        }
    }
    public static void EmptyCooldownOnlySlotHidesAndDoesNotInterceptClicks()
    {
        using (var r = new SkillSlotCooldownRig(true))
        {
            Assert.AreEqual(1f, r.Slot.GetComponent<CanvasGroup>().alpha); Assert.IsTrue(r.Icon.raycastTarget);
            r.Slot.Init(null, null);
            SkillSlotCooldownRig.AssertCleared(r.Slot);
            Assert.AreEqual(0f, r.Slot.GetComponent<CanvasGroup>().alpha); Assert.IsFalse(r.Icon.raycastTarget);
        }
    }
    public static void LevelZeroSkillRemovalClearsCooldown()
    {
        using (var r = new SkillSlotCooldownRig())
        {
            r.AssertCooling(); SkillSlotCooldownRig.SetLevel(r.Movement.Probe, 0); r.Slot.Refresh();
            Assert.IsNull(r.Slot.CurrentSkill); Assert.IsFalse(r.Icon.enabled);
            SkillSlotCooldownRig.AssertCleared(r.Slot);
        }
    }
}
public static class SkillSlotCooldownRuntimeCases
{
    public static IEnumerator RightClickClearsAndReequipRetainsActualCooldown()
    {
        using (var r = new SkillSlotCooldownRig())
        {
            r.AssertCooling();
            r.Slot.OnPointerClick(new PointerEventData(null) { button = PointerEventData.InputButton.Right });
            SkillSlotCooldownRig.AssertCleared(r.Slot); yield return null;
            SkillSlotCooldownRig.AssertCleared(r.Slot);
            r.Movement.Skills.RegisterSkillToSlot(C_Enums.SkillSlot.Q, r.Movement.Probe);
            r.AssertCooling(); Assert.AreEqual(4.2f, r.Movement.Probe.nowCoolTime);
        }
    }
    public static IEnumerator DragOutsideClearsCooldownAndDragIcon()
    {
        using (var r = new SkillSlotCooldownRig())
        {
            r.AssertCooling();
            var pointer = new PointerEventData(null) { pointerDrag = r.Slot.gameObject };
            r.Slot.OnBeginDrag(pointer); Assert.IsNotNull(r.Root.transform.Find("DragIcon"));
            r.Slot.OnEndDrag(pointer); SkillSlotCooldownRig.AssertCleared(r.Slot);
            yield return null; Assert.IsNull(r.Root.transform.Find("DragIcon"));
        }
    }
    public static IEnumerator MovingToEmptySlotTransfersCooldownDisplay()
    {
        using (var r = new SkillSlotCooldownRig())
        {
            var destination = r.CreateSlot(C_Enums.SkillSlot.W);
            var pointer = new PointerEventData(null) { pointerDrag = r.Slot.gameObject };
            r.Slot.OnBeginDrag(pointer); destination.OnDrop(pointer); r.Slot.OnEndDrag(pointer);
            SkillSlotCooldownRig.AssertCleared(r.Slot);
            Assert.AreSame(r.Movement.Probe, destination.CurrentSkill);
            Assert.AreEqual("5s", destination.transform.Find("CooldownText").GetComponent<TMP_Text>().text);
            Assert.AreEqual(0.42f, destination.transform.Find("Overlay").GetComponent<Image>().fillAmount, 0.001f);
            yield return null;
            SkillSlotCooldownRig.AssertCleared(r.Slot); Assert.AreEqual(4.2f, r.Movement.Probe.nowCoolTime);
        }
    }
    public static IEnumerator FrameUpdateClearsEmptySlotWithoutRefresh()
    {
        using (var r = new SkillSlotCooldownRig())
        {
            r.AssertCooling();
            r.Movement.Skills.OnSkillDataChanged -= r.Slot.Refresh;
            r.Movement.Skills.ClearSkillSlot(C_Enums.SkillSlot.Q);
            yield return null;
            SkillSlotCooldownRig.AssertCleared(r.Slot);
        }
    }
}