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

    [Header("퀘스트 보상")]
    [SerializeField] TMP_Text tmpQuestGoldReward;
    [SerializeField] TMP_Text tmpQuestExpReward;
    [SerializeField] Image imgQuestItemImage1;
    [SerializeField] TMP_Text tmpQuestItemReward1;
    [SerializeField] Image imgQuestItemImage2;
    [SerializeField] TMP_Text tmpQuestItemReward2;
    [SerializeField] Image imgQuestItemImage3;
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
        imgQuestItemImage1.gameObject.SetActive(false);
        imgQuestItemImage2.gameObject.SetActive(false);
        imgQuestItemImage3.gameObject.SetActive(false);
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
                    $"{questData.questTarget} - {QuestManager.Instance.questKillProgressDict[questID]} / {questData.questCount}";
                break;
            case "Item":
                tmpQuestTarget.text = $"목표 아이템을 획득하기\n" +
                    $"{questData.questTarget} - {QuestManager.Instance.questItemProgressDict[questID]} / {questData.questCount}";
                break;
            case "Talk":
                tmpQuestTarget.text = $"목표 NPC와 대화하기\n" +
                    $"{questData.questTarget} - {(QuestManager.Instance.questTalkProgressDict[questID] ? "완료" : "미완료")}";
                break;
        }

        tmpQuestExpReward.text = $"{questData.rewardExp}Exp";
        tmpQuestGoldReward.text = $"{questData.rewardGold}G";

        if (questData.rewardItems.Count != 0)
        {
            for (int i = 0; i < questData.rewardItems.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        imgQuestItemImage1.gameObject.SetActive(true);
                        tmpQuestItemReward1.gameObject.SetActive(true);

                        imgQuestItemImage1.sprite = ItemManager.Instance.GetItemBaseSO(questData.rewardItems[i].itemID).itemIcon;
                        tmpQuestItemReward1.text = $"{questData.rewardItems[i].count}개";
                        break;
                    case 1:
                        imgQuestItemImage2.gameObject.SetActive(true);
                        tmpQuestItemReward2.gameObject.SetActive(true);

                        imgQuestItemImage2.sprite = ItemManager.Instance.GetItemBaseSO(questData.rewardItems[i].itemID).itemIcon;
                        tmpQuestItemReward2.text = $"{questData.rewardItems[i].count}개";
                        break;
                    case 2:
                        imgQuestItemImage3.gameObject.SetActive(true);
                        tmpQuestItemReward3.gameObject.SetActive(true);

                        imgQuestItemImage3.sprite = ItemManager.Instance.GetItemBaseSO(questData.rewardItems[i].itemID).itemIcon;
                        tmpQuestItemReward3.text = $"{questData.rewardItems[i].count}개";
                        break;
                }
            }
        }

    }

}
