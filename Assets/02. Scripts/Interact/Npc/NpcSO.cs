using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcSO", menuName = "ScriptableObjects/NpcSO", order = 1)]
public class NpcSO : ScriptableObject
{
    [Header("Npc 기본 정보")]
    public string npcID;
    public string npcName;
    public string defaultDialogID;

    [Header("Npc 기능 정보")]
    public bool hasShop = false;
    public string shopID;
    public bool canUpgrade = false;
    public bool canTeleport = false;

    [Header("대화하기 대사 설정")]
    public string talkDialogID;

    [Header("Npc 퀘스트 설정")]
    public List<NpcQuestData> npcQuests;
}
