using NUnit.Framework;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class SkillLevelAndShortcutCases
{
    private static void RaiseToLevel(C_Stat stat, int level)
        => typeof(C_Stat).GetMethod("RaiseToLevel", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(stat, new object[] { level });

    public static void SkillRequiresConfiguredLevelAndDoesNotSpendPointsWhenLockedOrMaxed()
    {
        using (var rig = new MovementRig(false))
        {
            var data = ScriptableObject.CreateInstance<Skill_1SO>();
            try
            {
                data.requireLevel = 5;
                data.maxLevel = 2;
                var skill = data.SkillInit(rig.Model);
                var skills = rig.Skills;
                var stat = rig.Model.Stat.Stat;

                RaiseToLevel(rig.Model.Stat, 5);
                stat.currentLevel = 4;
                int points = stat.remainSkillPoint;
                skills.LevelUpSkill(skill);
                Assert.AreEqual(0, skill.SkillLevel);
                Assert.AreEqual(points, stat.remainSkillPoint);

                stat.currentLevel = 5;
                skills.LevelUpSkill(skill);
                Assert.AreEqual(1, skill.SkillLevel);
                Assert.AreEqual(points - 1, stat.remainSkillPoint);

                skills.LevelUpSkill(skill);
                Assert.AreEqual(2, skill.SkillLevel);
                Assert.AreEqual(points - 2, stat.remainSkillPoint);

                skills.LevelUpSkill(skill);
                Assert.AreEqual(2, skill.SkillLevel);
                Assert.AreEqual(points - 2, stat.remainSkillPoint);
            }
            finally { Object.DestroyImmediate(data); }
        }
    }

    public static void ShortcutLevelGrantUsesNormalRewardsAndNeverReducesLevel()
    {
        using (var rig = new InventoryTransferRig())
        {
            var stat = rig.Model.Stat.Stat;
            float attack = stat.attackDamage.FinalValue;
            float maxHealth = stat.maxHp.FinalValue;
            RaiseToLevel(rig.Model.Stat, 5);
            Assert.AreEqual(5, stat.currentLevel);
            Assert.AreEqual(4, stat.statPoint);
            Assert.AreEqual(5, stat.totalSkillPoint);
            Assert.AreEqual(5, stat.remainSkillPoint);
            Assert.That(stat.attackDamage.FinalValue, Is.EqualTo(attack + 40f).Within(.001f));
            Assert.That(stat.maxHp.FinalValue, Is.EqualTo(maxHealth + 120f).Within(.001f));

            RaiseToLevel(rig.Model.Stat, 5);
            RaiseToLevel(rig.Model.Stat, 3);
            Assert.AreEqual(5, stat.currentLevel);
            Assert.AreEqual(4, stat.statPoint);
            Assert.AreEqual(5, stat.remainSkillPoint);
        }
    }

    public static void SkillSlotTintAndButtonFollowRequiredLevel()
    {
        using (var rig = new MovementRig(false))
        {
            var data = ScriptableObject.CreateInstance<Skill_1SO>();
            var canvasObject = new GameObject("SkillTestCanvas", typeof(Canvas));
            try
            {
                data.requireLevel = 5;
                data.maxLevel = 2;
                var slotObject = new GameObject("SkillSlot", typeof(RectTransform), typeof(SkillTreeSlot));
                slotObject.transform.SetParent(canvasObject.transform);
                var slot = slotObject.GetComponent<SkillTreeSlot>();
                var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                icon.transform.SetParent(slotObject.transform);
                var name = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
                name.transform.SetParent(slotObject.transform);
                var level = new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TMP_Text>();
                level.transform.SetParent(slotObject.transform);
                var up = new GameObject("Up", typeof(RectTransform), typeof(Button)).GetComponent<Button>();
                up.transform.SetParent(slotObject.transform);
                var down = new GameObject("Down", typeof(RectTransform), typeof(Button)).GetComponent<Button>();
                down.transform.SetParent(slotObject.transform);
                InventoryTransferRig.Set(slot, "skillIcon", icon);
                InventoryTransferRig.Set(slot, "skillName", name);
                InventoryTransferRig.Set(slot, "skillLevel", level);
                InventoryTransferRig.Set(slot, "levelUpButton", up);
                InventoryTransferRig.Set(slot, "levelDownButton", down);

                rig.Model.Stat.Stat.currentLevel = 4;
                Color original = icon.color;
                slot.Init(rig.Skills, data.SkillInit(rig.Model), null);
                Assert.That(icon.color.r, Is.EqualTo(original.r * .6f).Within(.001f));
                Assert.IsFalse(up.interactable);

                rig.Model.Stat.Stat.currentLevel = 5;
                slot.Refresh();
                Assert.AreEqual(original, icon.color);
                Assert.IsTrue(up.interactable);
            }
            finally
            {
                Object.DestroyImmediate(canvasObject);
                Object.DestroyImmediate(data);
            }
        }
    }
}
