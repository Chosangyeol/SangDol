using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvent
{
    [Header("퀘스트")]
    public static Action<string> OnMonsterKill;
    public static Action<string> OnGetItem;
    public static Action<string> OnTalkNpc;
    public static Action OnQuestStateChange;

    [Header("플레이어")]
    public static Action<CharacterStat> OnStatChange;
    public static Action OnPlayerDie;
    public static Action<bool> OnPlayerPanic;
    public static Action<bool, string, float, float, float> OnGaugeUpdate;
    public static Action OnPlayerLevelUp;

    [Header("보스")]
    public static Action<BossModel> OnBossStateChange;
    public static Action<bool,float> OnBossRoomEnterCount;

    public static Action OnUIInvisable;
    public static Action OnMainUIviable;

    

}
