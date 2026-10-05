using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Playables;

public static class GameFlowRuntimeCases
{
    private static float nextPotionAttempt;
    private static CharacterModel Player => UnityEngine.Object.FindAnyObjectByType<CharacterModel>();
    private static T[] All<T>() where T : UnityEngine.Object => UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
    private static void Mark(string message)
    {
        Debug.Log("[GameFlow] " + message);
        File.AppendAllText("Temp/GameFlowVerification.txt", message + Environment.NewLine);
    }
    private static IEnumerator Until(Func<bool> predicate, string stage, float seconds = 25f)
    {
        float deadline = Time.realtimeSinceStartup + seconds;
        while (!predicate() && Time.realtimeSinceStartup < deadline) { UseRewardPotion(); yield return null; }
        Assert.IsTrue(predicate(), stage);
        Mark(stage);
    }
    private static IEnumerator Move(Vector3 position)
    {
        Player.PlayerController.StopMove();
        var agent = Player.Navmesh;
        if (UnityEngine.AI.NavMesh.SamplePosition(position, out var ground, 3f, UnityEngine.AI.NavMesh.AllAreas)) position = ground.position;
        Assert.IsTrue(agent.enabled && agent.isOnNavMesh && agent.Warp(position), "Test navigation must stay on the authored NavMesh: " + position);
        Physics.SyncTransforms();
        yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
    }
    private static NpcBase Npc(string id) => All<NpcBase>().Single(n => ((NpcSO)typeof(NpcBase).GetField("npcSO", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(n)).npcID == id);
    private static IEnumerator OpenNpc(string id)
    {
        var npc = Npc(id); yield return Move(npc.transform.position + Vector3.right);
        Assert.IsFalse(npc.isLocked, "NPC locked: " + id);
        Assert.IsTrue(npc.canInteract, "NPC trigger not reached: " + id);
        Assert.IsTrue(npc.Interact(Player.transform), "NPC interaction: " + id);
        yield return null;
    }
    private static void ClickNpcButton(string text)
    {
        var button = NpcDialogManager.Instance.buttonGroup.GetComponentsInChildren<Button>()
            .SingleOrDefault(b => b.GetComponentInChildren<TMP_Text>().text.Contains(text));
        Assert.IsNotNull(button, "NPC button: " + text);
        button.onClick.Invoke();
    }
    private static IEnumerator FinishDialogue()
    {
        var dialog = DialogManager.Instance;
        for (int i = 0; i < 40 && dialog.IsDialogueActive(); i++)
        {
            if (dialog.choicePanel.activeSelf) dialog.choice1Button.onClick.Invoke();
            else dialog.TryClickNextButton();
            yield return null;
        }
        Assert.IsFalse(dialog.IsDialogueActive(), "Dialogue must finish without retry/loop.");
        Assert.IsTrue(Player.canMove, "Dialogue must release player controls.");
        Assert.IsFalse(Player.isInteracting);
    }
    private static void Hit(EnemyBase enemy, float damage)
    {
        enemy.Damaged(new SDamageInfo { damage = damage, source = Player.gameObject });
    }
    private static void UseRewardPotion()
    {
        var player = Player;
        if (player == null || player.isDie || !player.canUse || Time.time < nextPotionAttempt ||
            player.Stat.Stat.curHp >= player.Stat.Stat.maxHp.FinalValue * .8f) return;
        nextPotionAttempt = Time.time + 1f;
        int index = player.Inventory.Items.FindIndex(i => i != null && i.itemBaseSO.itemID == "20001");
        if (index < 0) return;
        player.Inventory.useSlots[C_Enums.UseSlot.Slot_1] = index;
        player.PlayerController.RequestUseItem(C_Enums.UseSlot.Slot_1);
    }
    private static IEnumerator ClearEnemies(EnemySector condition)
    {
        yield return Until(() => condition.DeadEnemyCount == condition.TotalEnemyCount || All<EnemyModel>().Any(e => !e.IsDead), "Dungeon enemies spawned");
        float deadline = Time.realtimeSinceStartup + 30f;
        while (!condition.IsSatisfied && Time.realtimeSinceStartup < deadline)
        {
            foreach (var enemy in All<EnemyModel>().Where(e => !e.IsDead)) Hit(enemy, enemy.Stat.maxHp);
            Assert.IsFalse(Player.isDie, "Player died before dungeon encounter completed.");
            yield return new WaitForSecondsRealtime(.25f);
        }
        Assert.IsTrue(condition.IsSatisfied, "Enemy sector completion: " + condition.name);
        Mark("Dungeon enemies defeated: " + condition.DeadEnemyCount + "/" + condition.TotalEnemyCount);
    }
    private static IEnumerator JumpAndVerifyLanding(JumpObject jump)
    {
        var lockField = typeof(CharacterModel).GetField("externalControlLocked", BindingFlags.Instance | BindingFlags.NonPublic);
        Player.TryInteract();
        Assert.IsTrue((bool)lockField.GetValue(Player) && !Player.Navmesh.enabled, "Actual TryInteract starts the jump: " + jump.name);
        float deadline = Time.realtimeSinceStartup + jump.jumpDuration + 3f;
        while ((bool)lockField.GetValue(Player) && Time.realtimeSinceStartup < deadline)
        {
            Assert.IsFalse(Player.canMove || Player.canAttack || Player.canSkill, "Airborne player remains locked");
            yield return null;
        }
        Assert.IsFalse((bool)lockField.GetValue(Player));
        Assert.IsTrue(Player.canMove && Player.Navmesh.enabled && Player.Navmesh.isOnNavMesh);
        Assert.IsTrue(Player.Anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"), "Authored player controller exits the jump loop on landing");
        Assert.IsFalse(Player.Anim.IsInTransition(0));
        Assert.Less(Vector3.Distance(Player.transform.position, jump.targetPos.position), .2f);
    }
    public static IEnumerator TitleForestQuestsDungeonBossAndReturn()
    {
        File.WriteAllText("Temp/GameFlowVerification.txt", "Actual scene integration; scripted UI/trigger navigation and combat damage, no forced quest/sector completion.\n");
        nextPotionAttempt = 0f;
        float previousVolume = AudioListener.volume;
        AudioListener.volume = 0f;
        try
        {
            if (PoolManager.Instance != null) UnityEngine.Object.Destroy(PoolManager.Instance.gameObject);
            yield return null;
            yield return SceneManager.LoadSceneAsync("Title", LoadSceneMode.Single);
            yield return null;
            Assert.IsNotNull(TitleManager.instance);
            TitleManager.instance.StartGame();
            yield return Until(() => SceneManager.GetSceneByName("Map1-Forest").isLoaded && !SceneManager.GetSceneByName("Title").isLoaded, "Title -> Forest loaded");
            yield return new WaitForSecondsRealtime(1f);
            Assert.IsTrue(SceneManager.GetSceneByName("Main").isLoaded);
            Assert.AreEqual(1, All<CharacterModel>().Length);
            Assert.AreEqual(1, All<UIManager>().Length);
            Assert.AreEqual(1, All<AudioListener>().Count(l => l.enabled), "One gameplay audio listener");
            Assert.AreEqual(1, All<EventSystem>().Count(e => e.enabled), "One gameplay input event system");
            Assert.IsTrue(Player.Navmesh.enabled && Player.Navmesh.isOnNavMesh, "Forest spawn on NavMesh");
            var spawn = All<SpawnPoint>().Single(s => s.pointName == "Spawn_Default");
            Assert.Less(Vector3.Distance(Player.transform.position, spawn.transform.position), 2f, "Forest default spawn");
            int startingLevel = Player.Stat.Stat.currentLevel;
            float startingAttack = Player.Stat.Stat.attackDamage.FinalValue;
            float startingHp = Player.Stat.Stat.maxHp.FinalValue;
            var quest = QuestManager.Instance;
            yield return OpenNpc("30002");
            Assert.IsFalse(NpcDialogManager.Instance.buttonGroup.GetComponentsInChildren<TMP_Text>().Any(t => t.text.Contains("제철 사과")), "Apple quest waits for the introduction");
            ClickNpcButton("대화 하기"); yield return FinishDialogue();
            yield return OpenNpc("30001"); ClickNpcButton("모험의 시작"); yield return FinishDialogue();
            Assert.AreEqual(QuestState.InProgress, quest.GetQuestState("Q_30001_001"));
            yield return OpenNpc("30002");
            Assert.AreEqual(QuestState.CanClear, quest.GetQuestState("Q_30001_001"));
            ClickNpcButton("모험의 시작"); yield return FinishDialogue();
            Assert.AreEqual(QuestState.Completed, quest.GetQuestState("Q_30001_001"));
            Assert.AreEqual(QuestState.InProgress, quest.GetQuestState("Q_30001_002"));
            yield return OpenNpc("30003"); ClickNpcButton("상점 이용"); yield return null;
            var shop = NpcShopManager.instance;
            Assert.IsTrue(shop.shopPanel.activeSelf);
            var appleSlot = shop.gridLayoutGroup.GetComponentsInChildren<ShopSlotUI>().Single(s =>
                ((ItemBaseSO)typeof(ShopSlotUI).GetField("slotItem", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(s)).itemID == "20008");
            appleSlot.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right });
            Assert.AreEqual(1, Player.Inventory.GetTotalItemCount("20008"));
            Assert.AreEqual(QuestState.CanClear, quest.GetQuestState("Q_30001_002"));
            shop.CloseShop();
            if (UIManager.Instance.inventoryUI.gameObject.activeSelf) UIManager.Instance.ToggleUI(C_Enums.UIList.Inventory);
            yield return OpenNpc("30002"); ClickNpcButton("제철 사과"); yield return FinishDialogue();
            Assert.AreEqual(QuestState.Completed, quest.GetQuestState("Q_30001_002"));
            Assert.GreaterOrEqual(Player.Inventory.GetTotalItemCount("20001"), 20);
            yield return OpenNpc("30004"); ClickNpcButton("늑대 사냥"); yield return FinishDialogue();
            Assert.AreEqual(QuestState.InProgress, quest.GetQuestState("Q_30001_003"));
            foreach (var spawner in All<EnemySpawner>())
            {
                yield return Move(spawner.transform.position);
                yield return new WaitForSecondsRealtime(.65f);
                foreach (var wolf in All<EnemyModel>().Where(e => e.statSO.enemyID == "Wolf" && !e.IsDead))
                {
                    Hit(wolf, wolf.Stat.maxHp);
                    if (quest.questKillProgressDict["Q_30001_003"] >= 5) break;
                }
                if (quest.questKillProgressDict["Q_30001_003"] >= 5) break;
            }
            Assert.AreEqual(5, quest.questKillProgressDict["Q_30001_003"]);
            yield return OpenNpc("30004"); ClickNpcButton("늑대 사냥"); yield return FinishDialogue();
            Assert.AreEqual(QuestState.Completed, quest.GetQuestState("Q_30001_003"));
            Assert.Greater(Player.Stat.Stat.currentLevel, startingLevel);
            Assert.Greater(Player.Stat.Stat.attackDamage.FinalValue, startingAttack);
            Assert.Greater(Player.Stat.Stat.maxHp.FinalValue, startingHp);
            Mark("All 3 quests completed; level " + startingLevel + " -> " + Player.Stat.Stat.currentLevel + "; gold=" + Player.Stat.Stat.gold);
            var retainedPlayer = Player;
            int forestLevel = Player.Stat.Stat.currentLevel;
            var portal = All<DungeonPortalObject>().Single();
            yield return Move(portal.transform.position + Vector3.right);
            Assert.IsTrue(portal.canInteract && !portal.isLocked);
            Assert.IsTrue(portal.Interact(Player.transform));
            Assert.IsTrue(UIManager.Instance.dungentEnterUI.gameObject.activeSelf);
            UIManager.Instance.dungentEnterUI.transform.Find("RightPart/DungeonEnter").GetComponent<Button>().onClick.Invoke();
            yield return Until(() => SceneManager.GetSceneByName("Circus-Main-Hall").isLoaded && !SceneManager.GetSceneByName("Map1-Forest").isLoaded, "Forest -> Dungeon entered");
            yield return new WaitForSecondsRealtime(2f);
            Assert.AreSame(retainedPlayer, Player);
            Assert.AreEqual(forestLevel, Player.Stat.Stat.currentLevel);
            Assert.IsTrue(Player.Navmesh.enabled && Player.Navmesh.isOnNavMesh, "Dungeon spawn on NavMesh");
            var dungeon = DungeonManager.instance; Assert.IsNotNull(dungeon);
            yield return ClearEnemies(dungeon.allSectors[0].sectorObjects.Single().GetComponent<EnemySector>());
            var jump = dungeon.allSectors[0].portalObject.GetComponent<JumpObject>();
            yield return Until(() => jump.gameObject.activeInHierarchy, "First sector jump unlocked");
            yield return Move(jump.transform.position + Vector3.right);
            Assert.IsTrue(jump.canInteract && !jump.isLocked, "Unlocked jump usable");
            yield return JumpAndVerifyLanding(jump);
            Mark("Jump state controls=" + Player.canMove + " agent=" + Player.Navmesh.enabled + " nav=" + Player.Navmesh.isOnNavMesh + " pos=" + Player.transform.position + " target=" + jump.targetPos.position);
            Assert.IsTrue(Player.canMove && Player.Navmesh.enabled && Player.Navmesh.isOnNavMesh, "Jump restores controls/NavMesh");
            var goal = dungeon.allSectors[1].sectorObjects.Single().GetComponent<MoveSector>();
            // Follow the authored chain by selecting the closest remaining departure.
            var remainingJumps = All<JumpObject>().Where(j => j != jump).ToList();
            while (remainingJumps.Count > 0 && !goal.IsSatisfied)
            {
                var nextJump = remainingJumps.OrderBy(j => Vector3.Distance(Player.transform.position, j.transform.position)).First();
                remainingJumps.Remove(nextJump);
                yield return Move(nextJump.transform.position + Vector3.right);
                Assert.IsTrue(nextJump.canInteract && !nextJump.isLocked, "Platform jump usable: " + nextJump.name);
                yield return JumpAndVerifyLanding(nextJump);
                UseRewardPotion();
                Assert.IsTrue(Player.canMove && Player.Navmesh.enabled && Player.Navmesh.isOnNavMesh, "Platform landing: " + nextJump.name);
                Mark("Platform jump completed: " + nextJump.name);
            }
            yield return Move(goal.GetComponent<Collider>().bounds.center);
            yield return Until(() => goal.IsSatisfied && dungeon.currentSector >= 2, "Movement sector -> third sector");
            yield return ClearEnemies(dungeon.allSectors[2].sectorObjects.Single().GetComponent<EnemySector>());
            var warp = dungeon.allSectors[2].portalObject;
            yield return Until(() => warp.activeInHierarchy, "Boss portal unlocked");
            yield return Move(warp.GetComponent<Collider>().bounds.center);
            var warpData = (System.Collections.Generic.List<WarpData>)typeof(DungeonManager).GetField("warpDatas", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(dungeon);
            yield return Until(() => Player.canMove && Vector3.Distance(Player.transform.position, warpData[0].targetPos.position) < 3f, "First portal -> boss waiting room");
            var bossPortal = All<WarpPortal>().Single(p => p.gameObject != warp);
            yield return Move(bossPortal.GetComponent<Collider>().bounds.center);
            float bossDeadline = Time.realtimeSinceStartup + 30f;
            while (!All<D1_FinalBoss>().Any() && Time.realtimeSinceStartup < bossDeadline)
            {
                if (VideoPlayManager.instance != null && VideoPlayManager.instance.isPlaying) VideoPlayManager.instance.SkipVideo();
                yield return null;
            }
            var boss = All<D1_FinalBoss>().SingleOrDefault(); Assert.IsNotNull(boss, "Boss warp starts final sector");
            yield return null;
            Assert.IsTrue(Player.canMove && !Player.isDie);
            Assert.IsTrue(boss.isCombatStarted);
            yield return Until(() => boss.specialPatterns[0].hasDone && !boss.isDoingSpecial, "Opening boss pattern finishes", 30f);
            Mark("Boss player HP=" + Player.Stat.Stat.curHp + "/" + Player.Stat.Stat.maxHp.FinalValue + " use=" + Player.canUse + " colliders=" + Player.GetComponentsInChildren<Collider>().Length);
            yield return Move(boss.transform.position + Vector3.back * 5f);
            var patternField = typeof(BossModel).GetField("currentPattern", BindingFlags.Instance | BindingFlags.NonPublic);
            yield return Until(() => patternField.GetValue(boss) != null, "Actual normal boss pattern starts", 20f);
            int beforeHit = boss.Stat.curHp; Hit(boss, Mathf.Max(1f, beforeHit * .05f));
            Assert.Less(boss.Stat.curHp, beforeHit, "Actual boss accepts player combat damage");
            Assert.IsNotNull(boss.Special3.cutsceneDirector, "Scene intro binds actual pooled boss on combat notification");
            Hit(boss, boss.Stat.curHp - Mathf.FloorToInt(boss.Stat.maxHp * .79f));
            float specialDeadline = Time.realtimeSinceStartup + 80f;
            bool cardObserved = false;
            while ((!boss.specialPatterns[1].hasDone || boss.isDoingSpecial) && Time.realtimeSinceStartup < specialDeadline)
            {
                UseRewardPotion();
                var survival = All<SurvivalPattern1>().FirstOrDefault();
                if (survival != null)
                {
                    if (!cardObserved) { cardObserved = true; Mark("80% card spawned at gameTime=" + Time.time); }
                    int symbol = Array.IndexOf(survival.suitSprites, survival.centerSymbol.sprite);
                    int activeSymbol = (int)typeof(SurvivalPattern1).GetField("currentPatternIndex", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(survival);
                    if (symbol >= 0 && activeSymbol >= 0)
                    {
                        var safe = survival.safeZones[symbol].transform.position;
                        if (Vector3.Distance(Player.transform.position, safe) > 1f) yield return Move(safe);
                    }
                }
                else if (Player.canMove)
                {
                    // Move across telegraphed attacks while the preceding normal pattern finishes.
                    float angle = Time.time * .4f;
                    Vector3 destination = boss.center.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 12f;
                    destination.y = Player.transform.position.y;
                    Player.PlayerController.RequestMove(destination);
                }
                Assert.IsFalse(Player.isDie, "Player can survive the 80% card mechanic; pos=" + Player.transform.position + " hp=" + Player.Stat.Stat.curHp + "/" + Player.Stat.Stat.maxHp.FinalValue + " moving=" + Player.Navmesh.velocity + " canMove=" + Player.canMove + " canUse=" + Player.canUse);
                yield return null;
            }
            if (boss.isDoingSpecial)
            {
                var card = All<SurvivalPattern1>().FirstOrDefault();
                var animator = card != null ? card.GetComponent<Animator>() : null;
                Mark("80% timeout: hp=" + boss.Stat.curHp + " special=" + boss.isDoingSpecial + " card=" + (card != null) + " state=" + (animator != null ? animator.GetCurrentAnimatorStateInfo(0).normalizedTime.ToString() : "none") + " scale=" + Time.timeScale);
            }
            Assert.IsTrue(boss.specialPatterns[1].hasDone && !boss.isDoingSpecial, "80% mechanic finishes and returns to combat");
            Mark("80% card mechanic survived");
            yield return Until(() => !boss.isImmunity, "80% return swing finishes and damage is enabled", 5f);
            Hit(boss, boss.Stat.curHp - Mathf.FloorToInt(boss.Stat.maxHp * .59f));
            var intro = boss.Special3.cutsceneDirector;
            var bgm = AudioManager.instance.transform.Find("BGM_Player").GetComponent<AudioSource>();
            bool bgmWasPlaying = bgm.isPlaying, bgmWasMuted = bgm.mute;
            bool freeSwingObserved = false;
            bool introVideoObserved = false;
            var movingField = typeof(D1_FinalBoss).GetField("special3Moving", BindingFlags.Instance | BindingFlags.NonPublic);
            var lockedField = typeof(D1_FinalBoss).GetField("special3ControlsLocked", BindingFlags.Instance | BindingFlags.NonPublic);
            float introDeadline = Time.realtimeSinceStartup + 30f;
            while (intro.state != PlayState.Playing && Time.realtimeSinceStartup < introDeadline)
            {
                UseRewardPotion();
                if ((bool)movingField.GetValue(boss) && !(bool)lockedField.GetValue(boss))
                {
                    freeSwingObserved = true;
                    Assert.IsTrue(Player.canMove && Player.canAttack && Player.canSkill && boss.isImmunity);
                }
                if (VideoPlayManager.instance != null && VideoPlayManager.instance.isPlaying)
                {
                    Assert.AreEqual("Assets/08. Sound/Jester60Intro.mp4", UnityEditor.AssetDatabase.GetAssetPath(boss.Special3.cutsceneClip));
                    introVideoObserved = true;
                    Assert.IsTrue(bgm.mute && !bgm.isPlaying, "Video starts with gameplay BGM stopped");
                    VideoPlayManager.instance.SkipVideo();
                }
                Assert.IsFalse(Player.isDie);
                yield return null;
            }
            Assert.AreEqual(PlayState.Playing, intro.state, "Actual 60% video -> Jester60 Timeline");
            Assert.IsTrue(freeSwingObserved, "Actual swing leaves the player free");
            Assert.IsTrue(introVideoObserved, "Actual 60% entry uses Jester60Intro");
            Assert.IsTrue(bgm.mute && !bgm.isPlaying);
            Assert.Less(Vector3.Distance(boss.transform.position, boss.center.position), .1f);
            Assert.IsFalse(Player.canMove);
            Assert.IsFalse(All<D1_MiddleBoss>().Any(b => b.enabled), "The cinematic actor cannot fight before the full introduction");
            Mark("Actual 60% swing/video/Timeline sequence started");
            var cinematicActor = boss.Special3.sceneMiddleBoss;
            Assert.IsNotNull(cinematicActor);
            int cinematicActorId = cinematicActor.GetInstanceID();
            yield return Until(() => All<D1_MiddleBoss>().Any(b => b.enabled), "26-second intro -> actual middle boss", 35f);
            Assert.AreNotEqual(PlayState.Playing, intro.state);
            Assert.IsTrue(intro.gameObject.activeSelf, "Cutscene map is the combat map");
            yield return Until(() => Player.canMove, "Cutscene fade completes before player control", 3f);
            Assert.IsTrue(Player.canMove);
            Assert.IsTrue(Player.Navmesh.enabled && Player.Navmesh.isOnNavMesh, "Existing middle boss room has navigation after the introduction");
            var middle = All<D1_MiddleBoss>().Single(b => b.enabled);
            Assert.Greater(Player.transform.position.y, 90f, "Middle boss starts on the high platform");
            var middleJump = intro.GetComponentInChildren<JumpObject>(true);
            yield return Move(middleJump.transform.position);
            yield return JumpAndVerifyLanding(middleJump);
            var noReturn = new UnityEngine.AI.NavMeshPath();
            UnityEngine.AI.NavMesh.CalculatePath(Player.transform.position, boss.Special3.waitingArea.position, UnityEngine.AI.NavMesh.AllAreas, noReturn);
            Assert.AreNotEqual(UnityEngine.AI.NavMeshPathStatus.PathComplete, noReturn.status);
            Assert.AreEqual(cinematicActorId, middle.GetInstanceID(), "Cutscene actor itself enters combat without a replacement");
            Assert.IsTrue(bgm.mute && !bgm.isPlaying && boss.isImmunity);
            Assert.AreEqual("Bat-Boss", middle.Anim.name);
            yield return Move(middle.transform.position + Vector3.forward);
            yield return Until(() => middle.isCombatStarted, "Middle boss combat starts on approach", 5f);
            Assert.IsTrue((LayerMask.GetMask("Enemy") & (1 << middle.gameObject.layer)) != 0, "Middle boss is reachable by player skill queries");
            var inputs = Player.GetComponent<PlayerInputs>();
            bool inputsWereEnabled = inputs != null && inputs.enabled;
            int middleHp = middle.Stat.curHp;
            try
            {
                if (inputs != null) inputs.enabled = false;
                Player.PlayerController.RequestBasicAttack(false, middle.transform.position);
                Player.PlayerController.RequestBasicAttack(true, middle.transform.position);
                yield return Until(() => middle.Stat.curHp < middleHp, "Player basic attack animation damages the middle boss", 5f);
            }
            finally
            {
                Player.PlayerController.RequestBasicAttack(false, middle.transform.position);
                if (inputs != null) inputs.enabled = inputsWereEnabled;
            }
            Hit(middle, middle.Stat.maxHp);
            Assert.IsTrue(middle.IsDead);
            Assert.IsTrue(bgm.mute && !bgm.isPlaying && boss.isImmunity, "Death presentation still holds BGM and immunity");
            float middleDeadline = Time.realtimeSinceStartup + 20f;
            while ((boss.isDoingSpecial || boss.isImmunity) && Time.realtimeSinceStartup < middleDeadline)
            {
                if (VideoPlayManager.instance != null && VideoPlayManager.instance.isPlaying) VideoPlayManager.instance.SkipVideo();
                yield return null;
            }
            Assert.IsFalse(boss.isDoingSpecial || boss.isImmunity);
            Assert.AreEqual(bgmWasMuted, bgm.mute); Assert.AreEqual(bgmWasPlaying, bgm.isPlaying);
            Assert.IsTrue(Player.canMove && !Player.isDie);
            Assert.IsNotNull(Camera.main);
            Mark("Middle boss defeated -> final boss combat restored");
            Hit(boss, boss.Stat.maxHp);
            Assert.IsTrue(boss.IsDead, "Actual boss defeat");
            Assert.IsFalse(SceneManager.GetSceneByName("Title").isLoaded, "Departure waits for death presentation");
            float endDeadline = Time.realtimeSinceStartup + 30f;
            while (!SceneManager.GetSceneByName("Title").isLoaded && Time.realtimeSinceStartup < endDeadline)
            {
                if (VideoPlayManager.instance != null && VideoPlayManager.instance.isPlaying) VideoPlayManager.instance.SkipVideo();
                yield return null;
            }
            Assert.IsTrue(SceneManager.GetSceneByName("Title").isLoaded, "Boss sector completion -> actual Title");
            Assert.AreEqual(1, SceneManager.sceneCount);
            Assert.IsTrue(PoolManager.Instance == null);
            Assert.IsTrue(retainedPlayer == null);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.AreEqual(0, All<CharacterModel>().Length);
            Mark("Boss cleared -> Title only; gameplay player/pool cleaned up");
            TitleManager.instance.StartGame();
            yield return Until(() => SceneManager.GetSceneByName("Map1-Forest").isLoaded && !SceneManager.GetSceneByName("Title").isLoaded, "Return Title -> fresh Forest restart");
            yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(1, All<CharacterModel>().Length);
            Assert.AreEqual(QuestState.NotStart, QuestManager.Instance.GetQuestState("Q_30001_001"));
            Assert.IsTrue(Player.Navmesh.enabled && Player.Navmesh.isOnNavMesh);
            Mark("Complete flow PASS");
        }
        finally { AudioListener.volume = previousVolume; }
    }
}
