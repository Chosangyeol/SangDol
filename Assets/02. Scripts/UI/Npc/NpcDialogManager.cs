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
    public TMP_Text nameText;
    public TMP_Text dialogText;

    [Header("버튼 생성")]
    public Transform buttonGroup;
    public GameObject buttonPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void OpenNpcUI(NpcSO npc)
    {
        npcDialogPanel.SetActive(true);
        nameText.text = npc.name;
        dialogText.text = npc.defaultDialogID;

        foreach (Transform child in buttonGroup)
        {
            Destroy(child.gameObject);
        }

        if (npc.hasShop)
        {
            CreateButton("상점 이용", () =>
            {
                Debug.Log($"상점 열기 : {npc.shopID}");
                CloseUI();
            });
        }

        CreateButton("대화하기", () =>
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

                if (state != QuestState.Completed)
                {
                    string questName = QuestManager.Instance.GetQuestName(q.questID);

                    CreateButton($"[퀘스트] {questName}", () =>
                    {
                        // 상태에 맞는 대사 ID를 찾아 대화 매니저에게 넘김
                        string dialogueToPlay = DetermineQuestDialogueID(q, state);
                        DialogManager.Instance.StartDialogue(dialogueToPlay);
                        CloseUI();
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
    }    
}
