using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcDialogManager : MonoBehaviour
{
    public static NpcDialogManager Instance;

    [Header("Dialog 패널 연결")]
    public GameObject npcDialogPanel;

    [Header("버튼 생성")]
    public Transform buttonGroup;
    public GameObject buttonPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void OpenNpcUI(NpcSO npc)
    {
        GameEvent.OnUIInvisable?.Invoke();

        npcDialogPanel.SetActive(true);
        buttonGroup.gameObject.SetActive(true);
        DialogManager.Instance.StartDialogue(npc.defaultDialogID);

        foreach (Transform child in buttonGroup)
        {
            Destroy(child.gameObject);
        }

        if (npc.hasShop)
        {
            CreateButton("상점 이용", () =>
            {
                NpcShopManager.instance.OpenShop(npc.shopID);
                CloseUI();
            });
        }

        CreateButton("대화 하기", () =>
        {
            Debug.Log("대화 하기 실행 ");
        });
        
        if (npc.npcQuests != null)
        {
            foreach (var q in npc.npcQuests)
            {
                if (!string.IsNullOrEmpty(q.requiredQuestID))
                {
                    if (QuestManager.Instance.GetQuestState(q.requiredQuestID) != QuestState.Completed)
                        continue;
                }

                QuestState state = QuestManager.Instance.GetQuestState(q.questID);

                bool showButton = false;
                string prefix = "[시작 가능]";

                if (state == QuestState.NotStart)
                {
                    showButton = true;
                }
                else if (state == QuestState.CanClear)
                {
                    showButton = true;
                    prefix = "[완료 가능]";
                }
                
                if (showButton)
                {
                    string questName = QuestManager.Instance.GetQuestName(q.questID);
                    CreateButton($"{prefix} {questName}", () =>
                    {
                        string dialogToPlay = DetermineQuestDialogueID(q, state);
                        DialogManager.Instance.StartDialogue(dialogToPlay);
                        buttonGroup.gameObject.SetActive(false);
                    });    
                }
            }
        }
    }

    private string DetermineQuestDialogueID(NpcQuestData qData, QuestState state)
    {
        switch (state)
        {
            case QuestState.NotStart: return qData.startDialogID;
            case QuestState.InProgress: return qData.inProgressDialogueID;
            case QuestState.CanClear: return qData.clearDialogueID;
            default: return "";
        }
    }

    private void CreateButton(string buttonText, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = Instantiate(buttonPrefab, buttonGroup);
        btnObj.GetComponentInChildren<TMP_Text>().text = buttonText;
        btnObj.GetComponent<Button>().onClick.AddListener(action);
    }

    public void CloseUI()
    {
        npcDialogPanel.SetActive(false);
        GameEvent.OnMainUIviable?.Invoke();
    }    
}
