using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuestState
{
    NotStart,
    InProgress,
    CanClear,
    Completed
}

[System.Serializable]
public class NpcQuestData
{
    // 퀘스트 ID
    public string questID;
    // 선행 퀘스트 ID
    public string requiredQuestID;
    // 퀘스트 시작 DialogID
    public string startDialogID;
    public string inProgressDialogueID;
    public string clearDialogueID;
}
