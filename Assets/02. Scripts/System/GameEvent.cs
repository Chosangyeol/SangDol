using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvent
{
    public static Action<string> OnMonsterKill;
    public static Action<string> OnGetItem;
    public static Action<string> OnTalkNpc;

    public static Action<CharacterStat> OnStatChange;
    public static Action OnQuestStateChange;

    public static Action OnUIInvisable;
    public static Action OnMainUIviable;

    public static Action OnPlayerDie;
}
