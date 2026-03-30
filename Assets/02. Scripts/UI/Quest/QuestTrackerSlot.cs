using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestTrackerSlot : MonoBehaviour
{
    public TMP_Text questNameText;
    public TMP_Text questProgressText;

    public void Init(string questID)
    {
        QuestData questData = QuestManager.Instance.GetQuestData(questID);
        QuestState questState = QuestManager.Instance.GetQuestState(questID);

        questNameText.text = questData.questName;

        if (questState == QuestState.CanClear)
            questProgressText.text = "<color=#00FF00> 퀘스트 완료 가능!</color>";
        else
        {
            if (questData.questType == "Kill")
            {
                int current = 0;
                if (QuestManager.Instance.questKillProgressDict.ContainsKey(questID))
                    current = QuestManager.Instance.questKillProgressDict[questID];

                questProgressText.text = $"- {questData.questTarget} 처치 ({current} / {questData.questCount})";
            }
            else if (questData.questType == "Item")
            {
                int current = 0;
                if (QuestManager.Instance.questItemProgressDict.ContainsKey(questID))
                    current = QuestManager.Instance.questItemProgressDict[questID];

                questProgressText.text = $"- {questData.questTarget} 수집 ({current} / {questData.questCount})";
            }
            else if (questData.questType == "Talk")
            {
                questProgressText.text = $"- {questData.questTarget}와(과) 대화하기";
            }
        }
    }
}
