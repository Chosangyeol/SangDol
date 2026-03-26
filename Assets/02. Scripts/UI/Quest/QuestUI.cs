using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [Header("퀘스트 UI 세팅")]
    public int maxQuestCount = 20;
    [SerializeField] QuestSlot slotPrefab;
    [SerializeField] Transform slotParent;
    [SerializeField] QuestPreview preview;

    private RectTransform rectTransform;
    private Canvas canvas;

    private List<string> currentQuestList = new List<string>();

    private List<QuestSlot> slots = new();

    public void Init()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        for (int i = 0; i < maxQuestCount; i++)
        {
            QuestSlot slot = Instantiate(slotPrefab, slotParent);
            slot.Init(preview);
            slots.Add(slot);

            slot.gameObject.SetActive(false);
        }

        preview.gameObject.SetActive(false);
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        preview.gameObject.SetActive(false);
        UpdateQuestList();
    }

    private void UpdateQuestList()
    {
        currentQuestList.Clear();   

        foreach (var quest in QuestManager.Instance.questStateDict)
        {
            if (quest.Value == QuestState.InProgress || quest.Value == QuestState.CanClear)
            {
                currentQuestList.Add(quest.Key);
            }
        }

        for (int i = 0; i < slots.Count; i++)
            slots[i].gameObject.SetActive(false);

        for (int i = 0; i < currentQuestList.Count; i++)
        {
            QuestSlot slot = slots[i];

            slot.gameObject.SetActive(true);
            slot.SetQuestSlot(currentQuestList[i]);
        }
    }


}
