using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class QuestPreview : MonoBehaviour
{
    [SerializeField] TMP_Text tmpQuestName;
    [SerializeField] TMP_Text tmpQuestDialog;
    [SerializeField] TMP_Text tmpQuestTarget;
    [SerializeField] ItemTooltip tooltip;

    [Header("퀘스트 보상")]
    [SerializeField] TMP_Text tmpQuestGoldReward;
    [SerializeField] TMP_Text tmpQuestExpReward;
    [SerializeField] RewardItemSlot[] questRewardItemSlots;
    [SerializeField] TMP_Text tmpQuestItemReward1;
    [SerializeField] TMP_Text tmpQuestItemReward2;
    [SerializeField] TMP_Text tmpQuestItemReward3;

    public void Toggle(bool onlyFalse = false)
    {
        if (onlyFalse)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
    }

    public void SetQuestPreview(string questID)
    {
        questRewardItemSlots[0].gameObject.SetActive(false);
        questRewardItemSlots[1].gameObject.SetActive(false);
        questRewardItemSlots[2].gameObject.SetActive(false);

        tmpQuestItemReward1.gameObject.SetActive(false);
        tmpQuestItemReward2.gameObject.SetActive(false);
        tmpQuestItemReward3.gameObject.SetActive(false);

        QuestData questData = QuestManager.Instance.GetQuestData(questID);
        tmpQuestName.text = questData.questName;
        tmpQuestDialog.text = questData.questDialog;

        switch (questData.questType)
        {
            case "Kill":
                tmpQuestTarget.text = $"목표를 처치하기\n" +
                    $"{questData.questTargetName} - {QuestManager.Instance.questKillProgressDict[questID]} / {questData.questCount}";
                break;
            case "Item":
                tmpQuestTarget.text = $"목표 아이템을 획득하기\n" +
                    $"{questData.questTargetName} - {QuestManager.Instance.questItemProgressDict[questID]} / {questData.questCount}";
                break;
            case "Talk":
                tmpQuestTarget.text = $"목표 NPC와 대화하기\n" +
                    $"{questData.questTargetName} - {(QuestManager.Instance.questTalkProgressDict[questID] ? "완료" : "미완료")}";
                break;
        }

        tmpQuestExpReward.text = $"{questData.rewardExp}Exp";
        tmpQuestGoldReward.text = $"{questData.rewardGold}G";

        if (questData.rewardItems.Count != 0)
        {
            for (int i = 0; i < questData.rewardItems.Count; i++)
            {                    
                ItemBaseSO rewardItem = ItemManager.Instance.GetItemBaseSO(questData.rewardItems[i].itemID);
                switch (i)
                {
                    case 0:
                        questRewardItemSlots[i].gameObject.SetActive(true);
                        questRewardItemSlots[i].InitSlot(rewardItem, tooltip);
                        tmpQuestItemReward1.gameObject.SetActive(true);
                        tmpQuestItemReward1.text = $"{questData.rewardItems[i].count}개";
                        break;
                    case 1:
                        questRewardItemSlots[i].gameObject.SetActive(true);
                        questRewardItemSlots[i].InitSlot(rewardItem, tooltip);
                        tmpQuestItemReward2.gameObject.SetActive(true);
                        tmpQuestItemReward2.text = $"{questData.rewardItems[i].count}개";
                        break;
                    case 2:
                        questRewardItemSlots[i].gameObject.SetActive(true);
                        questRewardItemSlots[i].InitSlot(rewardItem, tooltip);
                        tmpQuestItemReward3.gameObject.SetActive(true);
                        tmpQuestItemReward3.text = $"{questData.rewardItems[i].count}개";
                        break;
                }
            }
        }

    }

}
